using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WaitForAction : CustomYieldInstruction
{
    public override bool keepWaiting => GameManager.Instance.WaitingCount > 0;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private GridManager gridManager;
    private PawnInteractManager pawnInteractManager;
    private AudioManager audioManager;

    public GridIndex ActivePlayer { get; private set; } = GridIndex.Player0;

    public Action waitingCallback = null;

    [SerializeField] private int waitingCount = 0;
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
        audioManager = AudioManager.Instance;

        StartCoroutine(GameFlow());
    }

    private IEnumerator GameFlow()
    {
        yield return null;

        yield return StartCoroutine(gridManager.CollapsePawns(Grid.Active));

        print("Enable Interaction");

        SelectedPawn = null;
        pawnInteractManager.BeginPlayerTurn();
    }

    public IEnumerator SelectPawn(Pawn pawn)
    {
        audioManager.Play(audioManager.selectPawnSound);
        if (SelectedPawn)
        {
            SelectedPawn.Deselect();
        }
        SelectedPawn = pawn;
        pawn.Select();
        yield return null;
    }

    public IEnumerator DeselectPawn()
    {
        audioManager.Play(audioManager.deselectPawnSound);
        SelectedPawn.Deselect();
        SelectedPawn = null;
        yield return null;
    }

    public IEnumerator DestroyPawn(Pawn pawn)
    {
        audioManager.Play(audioManager.destroyPawnSound);
        yield return StartCoroutine(pawn.DestroyPawn());
        yield return StartCoroutine(gridManager.CollapsePawns(Grid.Active));
    }
}
