using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// Wait until all actions are completed [WaitingCount]
/// </summary>
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

    [ReadOnly] [SerializeField] private int waitingCount = 0;
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

        SelectedPawn = null;
        pawnInteractManager.BeginPlayerTurn();
    }

    public IEnumerator SelectPawn(Pawn pawn, Action callback = null)
    {
        audioManager.Play(audioManager.selectPawnSound);
        if (SelectedPawn)
        {
            SelectedPawn.Deselect();
        }
        SelectedPawn = pawn;
        pawn.Select();
        yield return null;
        callback?.Invoke();
    }

    public IEnumerator DeselectPawn(Action callback = null)
    {
        audioManager.Play(audioManager.deselectPawnSound);
        SelectedPawn.Deselect();
        SelectedPawn = null;
        yield return null;
        callback?.Invoke();
    }

    public IEnumerator DestroyPawn(Pawn pawn)
    {
        audioManager.Play(audioManager.destroyPawnSound);
        yield return StartCoroutine(pawn.DestroyPawn());
        yield return StartCoroutine(gridManager.ProcessPawns(Grid.Active));
    }

    public IEnumerator DropPawn(Vector2Int position, Action callback = null)
    {
        audioManager.Play(audioManager.dropPawnSound);
        SelectedPawn.Deselect();

        SelectedPawn.GoTo(new(position.x, GridManager.gridSize.y));
        yield return StartCoroutine(SelectedPawn.MoveTo(position, gridManager.dropSpeed));
        SelectedPawn = null;

        yield return StartCoroutine(gridManager.ProcessPawns(Grid.Active));

        callback?.Invoke();
    }
}
