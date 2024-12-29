using UnityEngine;

public class Formation : Pawn
{
    [Header("Formation")]
    [SerializeField] private UnitColor color;

    public UnitColor Color { get => color; }
}
