using System;
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

    private void OnValidate()
    {
        foreach (PawnInitialState pawn in pawns)
        {
            pawn.position = new(
                Mathf.Clamp(pawn.position.x, 0, GridManager.gridSize.x - 1),
                Mathf.Clamp(pawn.position.y, 0, GridManager.gridSize.x - 1)
                );
        }
    }
}
