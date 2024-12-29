using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PawnCollapsePriority
{
    Wall = 0, Formation = 1, Default = 2
}

public class Pawn : MonoBehaviour
{
    private GridIndex grid;
    private Vector2Int position;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private PawnCollapsePriority collapsePriority = PawnCollapsePriority.Default;

    public GridIndex Grid { get => grid; }
    public Vector2Int Position { get => position; }
    public Vector2Int Size { get => size; }
    public PawnCollapsePriority CollapsePriority { get => collapsePriority; }

    protected GameManager gameManager;
    protected GridManager gridManager;

    private SpriteRenderer sprite;

    private int mergeTo = GridManager.gridSize.y;
    private readonly List<Action> mergeCallbacks = new();

    private void OnDrawGizmos()
    {
        Gizmos.color = new(1f, 0.6f, 0.1f);
        Gizmos.DrawWireCube(transform.position + new Vector3(0.5f * size.x - 0.5f, -0.5f * size.y + 0.5f), (Vector2)size);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        gridManager = GridManager.Instance;

        sprite = GetComponentInChildren<SpriteRenderer>();

        gameManager.WaitingCount--;
    }

    /// <summary>
    /// Initialize the other after its instantiation [WaitingCount]
    /// </summary>
    public void Init(GridIndex grid, Vector2Int position)
    {
        GameManager.Instance.WaitingCount++;
        this.grid = grid;
        GoTo(position);
    }

    private void Update()
    {
        MergeUpdate();
    }

    /// <summary>
    /// Check if the other overlaps this position
    /// </summary>
    public bool IsMatch(Vector2Int position)
    {
        return (this.position.x <= position.x && position.x < this.position.x + size.x &&
                this.position.y <= position.y && position.y < this.position.y + size.y);
    }

    /// <summary>
    /// Check if the other overlaps this rectangle
    /// </summary>
    public bool IsOverlapping(Vector2Int position, Vector2Int size)
    {
        if (this.position.x + this.size.x <= position.x || position.x + size.x <= this.position.x) return false;
        if (this.position.y + this.size.y <= position.y || position.y + size.y <= this.position.y) return false;
        return true;
    }

    /// <summary>
    /// Compare two pawns based on their collapse priorities
    /// </summary>
    public static int Compare(Pawn a, Pawn b)
    {
        int result = a.collapsePriority.CompareTo(b.collapsePriority);
        if (result != 0) return result;

        result = a.position.y.CompareTo(b.position.y);
        if (result != 0) return result;

        return a.position.x.CompareTo(b.position.x);
    }

    /// <summary>
    /// Get the pawn's local position in the world
    /// </summary>
    private Vector3 GetTransformPos()
    {
        if (grid == GridIndex.Player0)
        {
            return new(position.x, -position.y);
        }
        else
        {
            return new(-position.x - size.x + 1, position.y + size.y - 1);
        }
    }

    /// <summary>
    /// Teleport the pawn to a position
    /// </summary>
    public void GoTo(Vector2Int position)
    {
        this.position = position;
        transform.localPosition = GetTransformPos();
    }

    /// <summary>
    /// Progressively move the other to this position
    /// </summary>
    public IEnumerator MoveTo(Vector2Int position, float speed)
    {
        this.position = position;
        Vector2 targetPosition = GetTransformPos();

        while (Vector2.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            Vector2 pos = Vector2.MoveTowards(transform.localPosition, targetPosition, speed * Time.deltaTime);
            transform.localPosition = pos;
            yield return null;
        }
        transform.localPosition = targetPosition;
    }

    /// <summary>
    /// Move the other following a collapse and remove it if it leaves the grid [WaitingCount]
    /// </summary>
    public IEnumerator CollapseTo(Vector2Int position)
    {
        gameManager.WaitingCount++;

        if (!gridManager.hasCollapseEffect && this.position != position)
        {
            gridManager.hasCollapseEffect = true;
        }

        yield return StartCoroutine(MoveTo(position, gridManager.collapseSpeed));

        if (position.y + size.y > GridManager.gridSize.y)
        {
            print("Collapsed and destroyed out of the grid");
            StartCoroutine(DestroyPawn());
        }
        gameManager.WaitingCount--;
    }

    /// <summary>
    /// Calculate <c>sprite.sortingOrder</c> based on position
    /// </summary>
    public void CalculateSortingOrder()
    {
        sprite.sortingOrder = -(position.x + position.y * GridManager.gridSize.x);
    }

    /// <summary>
    /// Destroy the other and remove this from grid list
    /// </summary>
    public IEnumerator DestroyPawn()
    {
        gridManager.RemovePawnFromGrid(this, grid);
        Destroy(gameObject);
        yield return null;
    }

    /// <summary>
    /// Apply select effect
    /// </summary>
    public void Select()
    {
        sprite.material = gridManager.selectedMaterial;
    }

    /// <summary>
    /// Apply deselect effect
    /// </summary>
    public void Deselect()
    {
        sprite.material = gridManager.defaultMaterial;
    }

    /// <summary>
    /// Move the pawn up to merge it [WaitingCount]
    /// </summary>
    public void Merge(int moveUp, Action callback = null)
    {
        int posY = position.y - moveUp;
        if (posY < mergeTo)
        {
            if (mergeTo >= GridManager.gridSize.y)
            {
                gameManager.WaitingCount++;
            }
            mergeTo = posY;
            position.y = posY;
            if (callback != null)
            {
                mergeCallbacks.Add(callback);
            }
        }
    }

    private void MergeUpdate()
    {
        if (mergeTo < GridManager.gridSize.y)
        {
            Vector2 targetPosition = GetTransformPos();

            if (Vector2.Distance(transform.localPosition, targetPosition) > 0.01f)
            {
                Vector2 pos = Vector2.MoveTowards(transform.localPosition, targetPosition, gridManager.mergeSpeed * Time.deltaTime);
                transform.localPosition = pos;
            }
            else
            {
                gridManager.RemovePawnFromGrid(this, grid);
                Destroy(gameObject);

                foreach (Action callback in mergeCallbacks)
                {
                    callback.Invoke();
                }
                mergeCallbacks.Clear();

                mergeTo = GridManager.gridSize.y;
                gameManager.WaitingCount--;
            }
        }
    }
}
