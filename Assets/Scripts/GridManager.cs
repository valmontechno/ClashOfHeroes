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


    public Pawn GetPawn(Vector2Int position, GridTarget target)
    {
        Grid grid = GetGrid(target);
        foreach (Pawn pawn in grid)
        {
            if (pawn.Match(position))
            {
                return pawn;
            }
        }
        return null;
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
