using System;
using UnityEngine;
using UnityEngine.Rendering;

public enum PawnCollapsePriority
{
    Wall = 1, Formation = 2, Default = 3, Static = 0
}

public class Pawn : MonoBehaviour
{
    private int grid;
    private Vector2Int position;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private PawnCollapsePriority collapsePriority = PawnCollapsePriority.Default;

    public Vector2Int Position { get => position; }
    public Vector2Int Size { get => size; }
    public PawnCollapsePriority CollapsePriority { get => collapsePriority; }

    private Vector2? movementTarget;
    private float movementSpeed;
    private Action movementCallback;

    private GameManager gameManager;
    private GridManager gridManager;

    private SpriteRenderer sprite;

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

    private void Update()
    {
        Move();
    }

    /// <summary>
    /// Initialize the pawn after its instantiation
    /// </summary>
    public void Init(int grid, Vector2Int position)
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
        if (grid == 0)
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
    /// Set the pawn's target position
    /// </summary>
    public void MoveTo(Vector2Int position, float speed, Action callback = null)
    {
        this.position = position;
        movementTarget = GetTransformPos();
        movementSpeed = speed;
        movementCallback = callback;
        gameManager.WaitingCount++;
    }


    /// <summary>
    /// On Update, move the pawn to its target position
    /// </summary>
    private void Move()
    {
        if (movementTarget.HasValue)
        {
            Vector2 pos = Vector2.MoveTowards(transform.localPosition, movementTarget.Value, movementSpeed * Time.deltaTime);
            if (Vector2.Distance(pos, movementTarget.Value) < 0.01f) {
                transform.localPosition = movementTarget.Value;
                movementTarget = null;
                movementCallback?.Invoke();
                gameManager.WaitingCount--;
            }
            else
            {
                transform.localPosition = pos;
            }

        }
    }

    /// <summary>
    /// Set the pawn's position following a collapse
    /// </summary>
    public void CollapseTo(Vector2Int position)
    {
        MoveTo(position, gridManager.collapseSpeed, CollapseCallback);
    }

    /// <summary>
    /// Pawn destruction animation after collapse
    /// </summary>
    public void CollapseCallback()
    {
        if (position.y + size.y > GridManager.gridSize.y)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Calculate <c>sprite.sortingOrder</c> based on position
    /// </summary>
    public void CalculateSortingOrder()
    {
        sprite.sortingOrder = -(position.x + position.y * GridManager.gridSize.x);
    }
}
