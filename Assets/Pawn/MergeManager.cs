using System.Collections;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }

    GridManager gridManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
    }

    /// <summary>
    /// MergeIEnumerator units together to form formations and walls
    /// </summary>
    public IEnumerator MergeUnits(Grid grid)
    {
        TempPawn[,] table = gridManager.CreateGridTable(grid);

        for (int x = 0; x < GridManager.gridSize.x; x++)
        {
            for (int y = 0; y < GridManager.gridSize.y; y++)
            {
                TempPawn pawn = table[x, y];

                if (pawn == null) continue;
                if (pawn.position != new Vector2Int(x, y)) continue;

                if (typeof(Unit).IsAssignableFrom(pawn.type))
                {
                    switch (pawn.rank)
                    {
                        case UnitRank.Core:
                            CoreMerge(table, pawn, x, y);
                            break;
                        case UnitRank.Elite:
                            EliteMerge(table, pawn, x, y);
                            break;
                        case UnitRank.Champion:
                            ChampionMerge(table, pawn, x, y);
                            break;
                    }
                }
            }
        }

        yield return new WaitForAction();
    }

    /// <summary>
    /// Merge core units
    /// </summary>
    private void CoreMerge(TempPawn[,] table, TempPawn unit0, int x, int y)
    {
        // Formation
        if (
            CoreCanMerge(table, x, y + 1, unit0.color, true, out TempPawn unit1) &&
            CoreCanMerge(table, x, y + 2, unit0.color, true, out TempPawn unit2)
            )
        {
            unit0.formed = true;
            unit1.formed = true;
            unit2.formed = true;

            unit0.pawn.Merge(0);
            unit1.pawn.Merge(0);
            unit2.pawn.Merge(0);
            gridManager.InstantiatePawn(unit0.formation, unit0.position, unit0.grid);
        }

        // Wall
        if (
            CoreCanMerge(table, x + 1, y, unit0.color, false, out unit1) &&
            CoreCanMerge(table, x + 2, y, unit0.color, false, out unit2)
            )
        {
            if (!unit0.walled)
            {
                unit0.walled = true;
                unit0.pawn.Merge(0);
                if (table[unit0.position.x, GridManager.gridSize.y - 1] == null)
                {
                    gridManager.InstantiatePawn(unit0.wall, unit0.position, unit0.grid);
                }
            }
            if (!unit1.walled)
            {
                unit1.walled = true;
                unit1.pawn.Merge(0);
                if (table[unit1.position.x, GridManager.gridSize.y - 1] == null)
                {
                    gridManager.InstantiatePawn(unit1.wall, unit1.position, unit1.grid);
                }
            }
            if (!unit2.walled)
            {
                unit2.walled = true;
                unit2.pawn.Merge(0);
                if (table[unit2.position.x, GridManager.gridSize.y - 1] == null)
                {
                    gridManager.InstantiatePawn(unit2.wall, unit2.position, unit2.grid);
                }
            }
        }
    }

    /// <summary>
    /// Merge elite units
    /// </summary>
    private void EliteMerge(TempPawn[,] table, TempPawn unit0, int x, int y)
    {
        if (
            CoreCanMerge(table, x, y + 2, unit0.color, true, out TempPawn unit1) &&
            CoreCanMerge(table, x, y + 3, unit0.color, true, out TempPawn unit2)
            )
        {
            unit0.formed = true;

            unit1.pawn.Merge(2);
            unit2.pawn.Merge(2, () =>
            {
                unit0.pawn.Merge(0);
                gridManager.InstantiatePawn(unit0.formation, unit0.position, unit0.grid);
            });
        }
    }

    /// <summary>
    /// Merge champion units
    /// </summary>
    private void ChampionMerge(TempPawn[,] table, TempPawn unit0, int x, int y)
    {
        if (
            CoreCanMerge(table, x    , y + 2, unit0.color, true, out TempPawn unit1) &&
            CoreCanMerge(table, x + 1, y + 2, unit0.color, true, out TempPawn unit2) &&
            CoreCanMerge(table, x    , y + 3, unit0.color, true, out TempPawn unit3) &&
            CoreCanMerge(table, x + 1, y + 3, unit0.color, true, out TempPawn unit4)
            )
        {
            unit0.formed = true;

            unit1.pawn.Merge(2);
            unit2.pawn.Merge(2);
            unit3.pawn.Merge(2);
            unit4.pawn.Merge(2, () =>
            {
                unit0.pawn.Merge(0);
                gridManager.InstantiatePawn(unit0.formation, unit0.position, unit0.grid);
            });
        }
    }

    // <summary>
    /// Check if the core unit at the position can merge with the color
    /// </summary>
    public bool CoreCanMerge(TempPawn[,] table, int x, int y, UnitColor color, bool notFormed, out TempPawn unit)
    {
        unit = gridManager.GetTempPawn(table, x, y);
        return
            unit != null &&
            typeof(Unit).IsAssignableFrom(unit.type) &&
            unit.rank == UnitRank.Core && unit.color == color &&
            (!notFormed || !unit.formed)
        ;
    }
}
