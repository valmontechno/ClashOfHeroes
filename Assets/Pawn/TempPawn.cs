using System;
using UnityEngine;

public class TempPawn
{
    public Pawn pawn;
    public Type type;

    public GridIndex grid;
    public Vector2Int position;

    public UnitRank rank;
    public UnitColor color;
    public GameObject formation;
    public GameObject wall;

    public bool formed = false;
    public bool walled = false;

    public TempPawn(Pawn pawn)
    {
        this.pawn = pawn;
        type = pawn.GetType();

        grid = pawn.Grid;
        position = pawn.Position;

        if (pawn is Unit unit)
        {
            rank = unit.Rank;
            color = unit.Color;
            formation = unit.Formation;
            wall = unit.Wall;
        }
    }
}
