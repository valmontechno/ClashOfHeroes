using System;
using System.Collections;
using UnityEngine;

public class WaitForAction : CustomYieldInstruction
{
    public override bool keepWaiting => GameManager.Instance.WaitingCount > 0;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    GridManager gridManager;
    PawnInteractManager pawnInteractManager;

    public GridIndex ActivePlayer { get; private set; } = GridIndex.Player0;

    public Action waitingCallback = null;

    private int waitingCount = 0;
    public int WaitingCount {
        get => waitingCount;
        set {
            waitingCount = value;
            if (waitingCount < 0) throw new System.Exception("Waiting Count < 0");
        }
    }

    public Pawn SelectedPawn { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (waitingCallback != null && WaitingCount == 0)
        {
            Action callback = waitingCallback;
            waitingCallback = null;
            callback.Invoke();
        }
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
        pawnInteractManager = PawnInteractManager.Instance;

        StartCoroutine(GameFlow());
    }

    private IEnumerator GameFlow()
    {
        yield return null;

        yield return StartCoroutine(gridManager.CollapsePawns(Grid.Active));

        print("Enable Interaction");

        SelectedPawn = null;
        pawnInteractManager.interactEnabled = true;
    }

    //public IEnumerator SelectPawn(Pawn pawn, Action callback = null)
    //{
    //    SelectedPawn = pawn;
    //    //pawn.MoveTo(new(pawn.Position.x, GridManager.gridSize.y), gridManager.dropSpeed);
    //    pawn.Select();
    //    yield return new WaitForAction();
    //    callback?.Invoke();
    //}

    //public IEnumerator DestroyPawn(Pawn pawn, Action callback = null)
    //{
    //    pawn.DestroyPawn();
    //    yield return new WaitForAction();
    //    gridManager.CollapsePawns(Grid.Active);
    //    yield return new WaitForAction();
    //    callback?.Invoke();
    //}
}
