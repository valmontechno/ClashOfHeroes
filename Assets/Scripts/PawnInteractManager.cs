using System.Collections;
using UnityEngine;

public class PawnInteractManager : MonoBehaviour
{
    public static PawnInteractManager Instance { get; private set; }

    private GameManager gameManager;
    private GridManager gridManager;

    [Space(10)]
    [SerializeField] private Transform minPoint;
    [SerializeField] private Transform maxPoint;

    [Space(10)]
    [ReadOnly] [SerializeField] private bool isPlayerTurn = false;
    [ReadOnly] [SerializeField] private bool isListening = false;

    [Space(10)]
    [SerializeField] private float holdClickDuration;
    private Coroutine holdClickCoroutine;

    private Vector2Int clickDownPosition;
    private Vector2 clickDownWorldPosition;

    [SerializeField] float mouseDragXMax;
    [SerializeField] float mouseDragYMin;

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
            Pawn SelectedPawn = gameManager.SelectedPawn;
            if (GetMousePosition(SelectedPawn == null ? 1 : SelectedPawn.Size.x, out clickDownPosition, out clickDownWorldPosition))
            {
                holdClickCoroutine = StartCoroutine(HoldClick());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (holdClickCoroutine != null) StopCoroutine(holdClickCoroutine);

            Pawn SelectedPawn = gameManager.SelectedPawn;

            bool isInGrid = GetMousePosition(SelectedPawn == null ? 1 : SelectedPawn.Size.x, out Vector2Int position, out Vector2 worldPosition);

            bool dragXOk = Mathf.Abs(clickDownWorldPosition.x - worldPosition.x) <= mouseDragXMax;

            if (SelectedPawn == null)
            {
                // Select
                if (
                    gridManager.CanSelectFirstPawn(clickDownPosition.x, Grid.Active, out Pawn pawn) &&
                    pawn is Unit &&
                    (
                    // by drag
                    dragXOk &&
                    clickDownWorldPosition.y - worldPosition.y >= mouseDragYMin &&
                    gridManager.CheckInGrid(clickDownPosition)
                    ) || (
                    // by click
                    isInGrid &&
                    position == clickDownPosition &&
                    pawn.IsMatch(position)
                    )
                )
                {
                    isListening = false;
                    StartCoroutine(gameManager.SelectPawn(pawn,
                        () => isListening = true
                    ));
                }
            }
            else
            {
                // Drop or deselect
                if (
                    (
                    // by drag
                    dragXOk &&
                    worldPosition.y - clickDownWorldPosition.y >= mouseDragYMin &&
                    gridManager.CheckInGrid(clickDownPosition)
                    ) || (
                    // by click
                    isInGrid &&
                    position == clickDownPosition
                    )
                )
                {
                    if (position.x == SelectedPawn.Position.x)
                    {
                        // Deselect
                        isListening = false;
                        StartCoroutine(gameManager.DeselectPawn(
                            () => isListening = true
                        ));
                    }
                    else if (gridManager.TryFindFirstFreeInCol(position.x, SelectedPawn.Size, Grid.Active, out position, SelectedPawn))
                    {
                        // Drop
                        isListening = false;
                        StartCoroutine(gameManager.DropPawn(position,
                            () => isListening = true
                        ));
                    }
                }
            }
        }
    }

    private IEnumerator HoldClick()
    {
        yield return new WaitForSeconds(holdClickDuration);
        if (
            isPlayerTurn && isListening &&
            gameManager.SelectedPawn == null &&
            GetMousePosition(out Vector2Int position) && position == clickDownPosition
            )
        {
            isListening = false;
            clickDownPosition = new(-1, -1);
            if (gridManager.GetPawn(position, Grid.Active, out Pawn pawn) && (pawn is Unit || pawn is Wall))
            {
                yield return StartCoroutine(gameManager.DestroyPawn(pawn));
            }
            isListening = true;
        }
    }


    /// <summary>
    /// Enable interactions with the player at the beginning of his turn
    /// </summary>
    public void BeginPlayerTurn()
    {
        isPlayerTurn = true;
        isListening = true;
    }

    /// <summary>
    /// Get the mouse world position
    /// </summary>
    private Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Get the grid square pointed to by the mouse taking into account the offset due to the width of the selected unit
    /// </summary>
    /// <returns>
    /// Is the position inside the grid
    /// </returns>
    private bool GetMousePosition(int width, out Vector2Int position, out Vector2 worldPosition)
    {
        worldPosition = GetMouseWorldPosition();
        float offset = -(width - 1) / 2f;

        position = Vector2Int.FloorToInt(MathUtils.Map(worldPosition + Vector2.right * offset, minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
        position.x = Mathf.Clamp(position.x, 0, GridManager.gridSize.x - width);

        return gridManager.CheckInGrid(position);
    }

    /// <summary>
    /// Get the grid square pointed to by the mouse
    /// </summary>
    /// <returns>
    /// Is the position inside the grid
    /// </returns>
    private bool GetMousePosition(out Vector2Int position)
    {
        position = Vector2Int.FloorToInt(MathUtils.Map(GetMouseWorldPosition(), minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
        return gridManager.CheckInGrid(position);
    }
}
