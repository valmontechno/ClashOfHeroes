using System.Collections.Generic;
using UnityEngine;

using Grid = System.Collections.Generic.List<Pawn>;

public enum GridTarget
{
    active, opponent
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance {  get; private set; }

    private readonly Grid[] grids = {new(), new()};

    [SerializeField] Transform grid0Origin, grid1Origin;

    private void Awake()
    {
        Instance = this;
    }

    private int GetGridIndex(GridTarget target)
    {
       return target == GridTarget.active ? GameManager.Instance.ActivePlayer : 1 - GameManager.Instance.ActivePlayer;
    }

    private Grid GetGrid(GridTarget target)
    {
        return grids[GetGridIndex(target)];
    }

    public void SortGrid(GridTarget target)
    {
        GetGrid(target).Sort((a, b) => Pawn.ComparePosition(a, b));
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

    public bool IsFree(Vector2Int position, Vector2Int size, GridTarget target)
    {
        foreach (Pawn pawn in GetGrid(target))
        {
            if (pawn.IsOverlapping(position, size)) return false;
        }
        return true;
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
