using System.Collections;
using UnityEngine;

public class PawnInteractManager : MonoBehaviour
{
    public static PawnInteractManager Instance { get; private set; }

    private GameManager gameManager;
    private GridManager gridManager;
    private AudioManager audioManager;

    [SerializeField] private Transform minPoint, maxPoint;

    [SerializeField] private bool isPlayerTurn = false;
    [SerializeField] private bool isListening = false;

    [SerializeField] private float holdClickDuration;
    private Coroutine holdClickCoroutine;
    private Vector2Int clickDownPosition;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        gridManager = GridManager.Instance;
    }

    private void Update()
    {
        if (!isPlayerTurn || !isListening) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (GetGridMousePosition(out clickDownPosition))
            {
                holdClickCoroutine = StartCoroutine(HoldClick());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (holdClickCoroutine != null) StopCoroutine(holdClickCoroutine);

            //if (GetGridMousePosition(out Vector2Int position) && position == clickDownPosition)
            //{
            //    if (gameManager.SelectedPawn == null)
            //    {
            //        Pawn pawn = gridManager.GetPawn(position, Grid.Active);
            //        if (pawn &&
            //            pawn == gridManager.GetFirstPawnInCol(position.x, Grid.Active))
            //        {
            //            isListening = false;
            //            StartCoroutine(gameManager.SelectPawn(pawn, EnableInteraction));
            //        }
            //    }
            //    else
            //    {

            //    }
            //}
        }
    }

    private IEnumerator HoldClick()
    {
        yield return new WaitForSeconds(holdClickDuration);
        if (
            isPlayerTurn && isListening &&
            gameManager.SelectedPawn == null &&
            GetGridMousePosition(out Vector2Int position) && position == clickDownPosition
            )
        {
            isListening = false;
            clickDownPosition = new(-1, -1);
            if (gridManager.GetPawn(position, Grid.Active, out Pawn pawn))
            {
                yield return StartCoroutine(pawn.DestroyPawn());
                yield return StartCoroutine(gridManager.CollapsePawns(Grid.Active));
            }
            isListening = true;
        }
    }

    public void BeginPlayerTurn()
    {
        isPlayerTurn = true;
        isListening = true;
    }

    /// <summary>
    /// Get the grid box pointed to by the mouse
    /// </summary>
    /// <returns>
    /// Is the position inside the grid
    /// </returns>
    private bool GetGridMousePosition(out Vector2Int position)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position = Vector2Int.FloorToInt(MathUtils.Map(mousePosition, minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
        return 0 <= position.x && position.x < GridManager.gridSize.x && 0 <= position.y && position.y < GridManager.gridSize.y;
    }
}
