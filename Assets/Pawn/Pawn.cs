using System;
using System.Collections;
using UnityEngine;

public enum PawnCollapsePriority
{
    Wall = 1, Formation = 2, Default = 3, Static = 0
}

public class Pawn : MonoBehaviour
{
    private GridIndex grid;
    private Vector2Int position;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private PawnCollapsePriority collapsePriority = PawnCollapsePriority.Default;

    public Vector2Int Position { get => position; }
    public Vector2Int Size { get => size; }
    public PawnCollapsePriority CollapsePriority { get => collapsePriority; }

    private GameManager gameManager;
    private GridManager gridManager;

    private SpriteRenderer sprite;
    [SerializeField] private Material selectedMaterial;

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
    }

    /// <summary>
    /// Initialize the pawn after its instantiation
    /// </summary>
    public void Init(GridIndex grid, Vector2Int position)
    {
        this.grid = grid;
        GoTo(position);
    }

    /// <summary>
    /// Check if the pawn overlaps this position
    /// </summary>
    public bool IsMatch(Vector2Int position)
    {
        return (this.position.x <= position.x && position.x < this.position.x + size.x &&
                this.position.y <= position.y && position.y < this.position.y + size.y);
    }

    /// <summary>
    /// Check if the pawn overlaps this rectangle
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
    /// Progressively move the pawn to this position
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
    /// Move the pawn following a collapse and remove it if it leaves the grid [WaitingCount]
    /// </summary>
    public IEnumerator CollapseTo(Vector2Int position)
    {
        gameManager.WaitingCount++;
        yield return StartCoroutine(MoveTo(position, gridManager.collapseSpeed));

        if (position.y + size.y > GridManager.gridSize.y)
        {
            StartCoroutine(DestroyPawn(false));
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
    /// Destroy the pawn and remove this from grid list
    /// </summary>
    public IEnumerator DestroyPawn(bool playSound = true)
    {
        if (playSound)
        {
            AudioManager.Instance.PlayDestroyPawnSound();
        }
        gridManager.RemovePawnFromGrid(this, grid);
        Destroy(gameObject);
        yield return null;
    }

    //public void Select()
    //{
    //    gameManager.WaitingCount++;
    //    //Color color = sprite.color;
    //    //color.a = 0.5f;
    //    //sprite.color = color;
    //    sprite.material = selectedMaterial;
    //    gameManager.WaitingCount--;
    //}
}
