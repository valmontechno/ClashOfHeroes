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
        yield return null;

        yield return new WaitForSeconds(1);

        gridManager.CollapsePawns(GridTarget.Active);
        gridManager.CollapsePawns(GridTarget.Opponent);
    }
}
