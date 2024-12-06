using Unity.VisualScripting;
using UnityEngine;

using Grid = System.Collections.Generic.List<Pawn>;

public enum GridTarget
{
    Active, Opponent
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance {  get; private set; }

    private readonly Grid[] grids = {new(), new()};

    [SerializeField] Transform grid0Origin, grid1Origin;

    public static readonly Vector2Int gridSize = new(8, 6);

    [Space(10)]
    public float collapseSpeed;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Get the index of the <c>ActivePlayer</c> grid
    /// </summary>
    public int GetGridIndex(GridTarget target)
    
    {
       return target == GridTarget.Active ? GameManager.Instance.ActivePlayer : 1 - GameManager.Instance.ActivePlayer;
    }

    /// <summary>
    /// Get the <c>ActivePlayer</c> grid
    /// </summary>
    public ref Grid GetGrid(GridTarget target)
    {
        return ref grids[GetGridIndex(target)];
    }

    /// <summary>
    /// Sort the grid by collapse priority
    /// </summary>
    public void SortGrid(Grid grid)
    {
        grid.Sort((a, b) => Pawn.Compare(a, b));
    }

    /// <summary>
    /// Get the pawn with this exact position
    /// </summary>
    public Pawn GetPawnExactly(Vector2Int position, GridTarget target)
    {
        Grid grid = GetGrid(target);
        foreach (Pawn pawn in grid)
        {
            if (pawn.Position == position)
            {
                return pawn;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the pawn at this position
    /// </summary>
    public Pawn GetPawn(Vector2Int position, GridTarget target)
    {
        Grid grid = GetGrid(target);
        foreach (Pawn pawn in grid)
        {
            if (pawn.IsMatch(position))
            {
                return pawn;
            }
        }
        return null;
    }

    /// <summary>
    /// Check if space is free
    /// </summary>
    /// <param name="ignored">
    /// Ignore this pawn (usually the one you are trying to find a place for)
    /// </param>
    /// <param name="edge">
    /// Tke into account the edges
    /// </param>
    /// <returns></returns>
    public bool IsFree(Vector2Int position, Vector2Int size, Grid grid, Pawn ignored = null, bool edge = true)
    {
        if (edge && (position.x < 0 || position.y < 0 || position.x + size.x > gridSize.x ||  position.y + size.y > gridSize.y))
            return false;
        foreach (Pawn pawn in grid)
        {
            if (pawn == ignored) continue;
            if (pawn.IsOverlapping(position, size)) return false;
        }
        return true;
    }

    /// <summary>
    /// Try to find the first free slot in a column
    /// </summary>
    public bool TryFindFirstFreeInCol(int col, Vector2Int size, Grid grid, out Vector2Int position)
    {
        position = new(col, gridSize.y - size.y);

        if (IsFree(position, size, grid))
        {
            do
            {
                position.y--;
            } while (IsFree(position, size, grid));
            position.y++;
            return true;
        }
        else
        {
            position.y = gridSize.y;
            return false;
        }
    }

    /// <summary>
    /// Find the first free slot in a column (possibly outside the edges)
    /// </summary>
    public Vector2Int FindFirstFreeInCol(int col, Vector2Int size, Grid grid)
    {
        Vector2Int position = new(col, gridSize.y - size.y);

        if (IsFree(position, size, grid, null, false))
        {
            do
            {
                position.y--;
            } while (IsFree(position, size, grid));
            position.y++;
            return position;
        }
        else
        {
            do
            {
                position.y++;
            } while (!IsFree(position, size, grid, null, false));
            return position;
        }
    }

    /// <summary>
    /// Move forward all pawns respecting their collapse priorities</c>
    /// </summary>
    /// <param name="target"></param>
    public void CollapsePawns(GridTarget target)
    {
        Grid grid = GetGrid(target);
        SortGrid(grid);

        Grid newGrid = new();

        foreach (Pawn pawn in grid)
        {
            if (pawn.CollapsePriority != PawnCollapsePriority.Static)
            {
                pawn.CollapseTo(FindFirstFreeInCol(pawn.Position.x, pawn.Size, newGrid));
            }
            pawn.CalculateSortingOrder();
            newGrid.Add(pawn);
        }
    }


    /// <summary>
    /// Instantiate a new pawn in the game and add it to the grid
    /// </summary>
    public Pawn InstantiatePawn(GameObject gameObject, Vector2Int position, int grid)
    {
        gameObject = Instantiate(gameObject, grid == 0 ? grid0Origin : grid1Origin);
        if (gameObject.TryGetComponent<Pawn>(out var pawn))
        {
            grids[grid].Add(pawn);
            pawn.Init(grid, position);
        }
        else
        {
            Debug.LogError("GameObject is not a Pawn");
        }
        return pawn;
    }
}
