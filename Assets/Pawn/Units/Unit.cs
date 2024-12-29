using UnityEngine;

public enum UnitRank
{
    Core, Elite, Champion
}

public enum UnitColor
{
    Alpha, Beta, Gamma
}

public class Unit : Pawn
{
    [Header("Unit")]
    [SerializeField] private UnitRank rank;
    [SerializeField] private UnitColor color;

    [SerializeField] private GameObject formation;
    [SerializeField] private GameObject wall;

    public UnitRank Rank { get => rank; }
    public UnitColor Color { get => color; }
    public GameObject Formation { get => formation; }
    public GameObject Wall { get => wall; }
}
