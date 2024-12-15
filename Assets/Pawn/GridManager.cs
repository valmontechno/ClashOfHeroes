using System;
using System.Collections;
using UnityEngine;

public enum GridIndex : int
{
    Player0 = 0, Player1 = 1
}

public class Grid : System.Collections.Generic.List<Pawn>
{
    public static Grid Active { get => GridManager.Instance.grids[(int)GameManager.Instance.ActivePlayer]; }
    public static Grid Opponent { get => GridManager.Instance.grids[1 - (int)GameManager.Instance.ActivePlayer]; }
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance {  get; private set; }

    public readonly Grid[] grids = { new(), new() };

    [Space(10)]
    [SerializeField] Transform grid0Origin;
    [SerializeField] Transform grid1Origin;

    public static readonly Vector2Int gridSize = new(8, 6);

    [Space(10)]
    public float collapseSpeed;
    public float dropSpeed;

    [Space(10)]
    public Material defaultMaterial;
    public Material selectedMaterial;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Sort the grid by collapse priority
    /// </summary>
    public void SortGrid(Grid grid)
    {
        grid.Sort((a, b) => Pawn.Compare(a, b));
    }

    /// <summary>
    /// Get the Pawn with this exact position
    /// </summary>
    public Pawn GetPawnExactly(Vector2Int position, Grid grid)
    {
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
    /// Get the Pawn at this position
    /// </summary>
    public Pawn GetPawn(Vector2Int position, Grid grid)
    {
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
    /// Get the Pawn at this position
    /// </summary>
    /// <returns>
    /// Is the Pawn found
    /// </returns>
    public bool GetPawn(Vector2Int position, Grid grid, out Pawn pawn)
    {
        foreach (Pawn p in grid)
        {
            if (p.IsMatch(position))
            {
                pawn = p;
                return true;
            }
        }
        pawn = null;
        return false;
    }

    /// <summary>
    /// Check if space is free
    /// </summary>
    /// <param name="ignored">
    /// Ignore this firstPawn (usually the one you are trying to find a place for)
    /// </param>
    /// <param name="edge">
    /// Tke into account the edges
    /// </param>
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
    /// Get the selectablePawn with the highest y position in the column
    /// </summary>
    public Pawn GetFirstPawnInCol(int col, Grid grid)
    {
        Pawn firstPawn = null;
        foreach (Pawn pawn in grid)
        {
            if (pawn.Position.x <= col && col < pawn.Position.x + pawn.Size.x)
            {
                if (firstPawn == null || pawn.Position.y > firstPawn.Position.y)
                {
                    firstPawn = pawn;
                }
            }
        }
        return firstPawn;
    }


    /// <summary>
    /// Check if the pawn at the position can be selected
    /// </summary>
    /// <param name="pawn">
    /// If so, the selected pawn
    /// </param>
    public bool TryGetSelectedPawn(Vector2Int position, Grid grid, out Pawn pawn)
    {
        pawn = GetFirstPawnInCol(position.x, grid);

        if (pawn == null || !pawn.IsMatch(position) || !IsFree(pawn.Position, new(pawn.Size.x, gridSize.y), grid, pawn, false))
        {
            pawn = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to find the first free slot in a column
    /// </summary>
    public bool TryFindFirstFreeInCol(int col, Vector2Int size, Grid grid, out Vector2Int position, Pawn ignored = null)
    {
        position = new(col, gridSize.y - size.y);

        if (IsFree(position, size, grid, ignored))
        {
            do
            {
                position.y--;
            } while (IsFree(position, size, grid, ignored));
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
    public IEnumerator CollapsePawns(Grid grid)
    {
        SortGrid(grid);

        Grid newGrid = new();

        foreach (Pawn pawn in grid)
        {
            StartCoroutine(pawn.CollapseTo(FindFirstFreeInCol(pawn.Position.x, pawn.Size, newGrid)));
            pawn.CalculateSortingOrder();
            newGrid.Add(pawn);
        }

        yield return new WaitForAction();
    }


    /// <summary>
    /// Instantiate a new firstPawn in the game and add it to the grid
    /// </summary>
    public Pawn InstantiatePawn(GameObject gameObject, Vector2Int position, GridIndex grid)
    {
        gameObject = Instantiate(gameObject, grid == 0 ? grid0Origin : grid1Origin);
        if (gameObject.TryGetComponent<Pawn>(out var pawn))
        {
            grids[(int)grid].Add(pawn);
            pawn.Init(grid, position);
        }
        else
        {
            Debug.LogError("GameObject is not a Pawn");
        }
        return pawn;
    }

    /// <summary>
    /// Remove a firstPawn from the grid list
    /// </summary>
    public void RemovePawnFromGrid(Pawn pawn, GridIndex grid)
    {
        grids[(int)grid].Remove(pawn);
    }
}
