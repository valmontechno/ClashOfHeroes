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

    public readonly Vector2Int gridSize = new(8, 6);

    private void Awake()
    {
        Instance = this;
    }

    private int GetGridIndex(GridTarget target)
    {
       return target == GridTarget.Active ? GameManager.Instance.ActivePlayer : 1 - GameManager.Instance.ActivePlayer;
    }

    private Grid GetGrid(GridTarget target)
    {
        return grids[GetGridIndex(target)];
    }

    public void SortGrid(Grid grid)
    {
        grid.Sort((a, b) => Pawn.ComparePosition(a, b));
    }

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

    public bool IsFree(Vector2Int position, Vector2Int size, GridTarget target, Pawn ignored = null)
    {
        if (position.x < 0 || position.y < 0 || position.x + size.x >= gridSize.x || position.y + size.y >= gridSize.y)
            return false;
        foreach (Pawn pawn in GetGrid(target))
        {
            if (pawn == ignored) continue;
            if (pawn.IsOverlapping(position, size)) return false;
        }
        return true;
    }


    public void CollapsePawns(GridTarget target)
    {
        Grid grid = GetGrid(target);
        SortGrid(grid);

        foreach (Pawn pawn in grid)
        {
            if (pawn.CollapsePriority == PawnCollapsePriority.Static)
                continue;

            Vector2Int pos = pawn.Position;
            do
            {
                pos.y--;
            } while (IsFree(pos, pawn.Size, target, pawn));

            pos.y++;
            pawn.GoTo(pos);
        }
    }



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
