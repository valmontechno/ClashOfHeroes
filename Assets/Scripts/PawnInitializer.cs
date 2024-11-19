using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class PawnInitialState
{
    public GameObject gameObject;
    public Vector2Int position;
    [Range(0, 1)] public int grid;
}

public class PawnInitializer : MonoBehaviour
{
    [SerializeField] PawnInitialState[] pawns;

    private void Start()
    {
        foreach (PawnInitialState pawn in pawns)
        {
            GridManager.Instance.InstantiatePawn(pawn.gameObject, pawn.position, pawn.grid);
        }
    }
}
