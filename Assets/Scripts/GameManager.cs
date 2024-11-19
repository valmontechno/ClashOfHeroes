using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    GridManager gridManager;

    public int ActivePlayer { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gridManager = GridManager.Instance;

        StartCoroutine(MainTest());
    }

    private IEnumerator MainTest()
    {
        //yield return new WaitForSeconds(1);
        //gridManager.GetPawn(new(0, 0), GridTarget.active).GoTo(new(7, 5));
        //yield return new WaitForSeconds(1);
        //ActivePlayer = 1;
        //gridManager.GetPawn(new(3, 1), GridTarget.opponent).GoTo(new(5, 2));
        //yield return new WaitForSeconds(1);
        //gridManager.GetPawn(new(1, 5), GridTarget.active).GoTo(new(6, 4));

        yield return null;

        gridManager.SortGrid(GridTarget.active);

        print(gridManager.IsFree(new(1, 2), new(2, 2), GridTarget.active));
    }
}
