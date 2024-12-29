using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PawnInteractManager : MonoBehaviour
{
    public static PawnInteractManager Instance { get; private set; }

    private GameManager gameManager;
    private GridManager gridManager;

    [SerializeField] private Transform minPoint, maxPoint;

    [SerializeField] private bool isPlayerTurn = false;
    [SerializeField] private bool isListening = false;

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
        /*
        if (!isPlayerTurn || !isListening) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (GetMouseGridPosition(GetMouseWorldPosition(), out clickDownPosition))
            {
                holdClickCoroutine = StartCoroutine(HoldClick());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (holdClickCoroutine != null) StopCoroutine(holdClickCoroutine);

            Vector2 mousePosition = GetMouseWorldPosition();

            if (GetMouseGridPosition(mousePosition, out Vector2Int position) && position == clickDownPosition)
            {
                Pawn SelectedPawn = gameManager.SelectedPawn;
                if (SelectedPawn == null)
                {
                    if (gridManager.TryGetSelectedPawn(position, Grid.Active, out Pawn pawn) && pawn is Unit)
                    {
                        isListening = false;
                        StartCoroutine(gameManager.SelectPawn(pawn,
                            () => isListening = true
                        ));
                    }
                }
                else
                {
                    position = GetGridMousePositionWithOffset(mousePosition, SelectedPawn.Size.x);
                    if (position.x == SelectedPawn.Position.x)
                    {
                        isListening = false;
                        StartCoroutine(gameManager.DeselectPawn(
                            () => isListening = true
                        ));
                    }
                    else if (gridManager.TryFindFirstFreeInCol(position.x, SelectedPawn.Size, Grid.Active, out position, SelectedPawn))
                    {
                        isListening = false;
                        StartCoroutine(gameManager.DropPawn(position,
                            () => isListening = true
                        ));
                    }
                }
            }
        }
        */

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

            Pawn pawn;

            if (SelectedPawn == null)
            {
                // Select
                if (
                    (
                    // by drag
                    dragXOk &&
                    clickDownWorldPosition.y - worldPosition.y >= mouseDragYMin &&
                    gridManager.CheckInGrid(clickDownPosition) &&
                    gridManager.TrySelectFirstPawn(clickDownPosition.x, Grid.Active, out pawn)
                    ) || (
                    // by click
                    isInGrid &&
                    position == clickDownPosition &&
                    gridManager.TryGetSelectedPawn(position, Grid.Active, out pawn) && pawn is Unit
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
                    worldPosition.y - clickDownWorldPosition.y>= mouseDragYMin &&
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
                        )); // !!! large units !!!
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
            GetMouseGridPosition(GetMouseWorldPosition(), out Vector2Int position) && position == clickDownPosition
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

    public void BeginPlayerTurn()
    {
        isPlayerTurn = true;
        isListening = true;
    }

    private Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

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
    private bool GetMouseGridPosition(Vector2 mousePosition, out Vector2Int position)
    {
        position = Vector2Int.FloorToInt(MathUtils.Map(mousePosition, minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
        return 0 <= position.x && position.x < GridManager.gridSize.x && 0 <= position.y && position.y < GridManager.gridSize.y;
    }


    /// <summary>
    /// Get the grid square pointed to by the mouse taking into account the offset due to the width of the selected unit
    /// </summary>
    private Vector2Int GetGridMousePositionWithOffset(Vector2 mousePosition, int width)
    {
        float offset = -(width - 1) / 2f;
        mousePosition.x += offset;
        Vector2Int position = Vector2Int.FloorToInt(MathUtils.Map(mousePosition, minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
        position.x = Mathf.Clamp(position.x, 0, GridManager.gridSize.x - width);
        return position;
    }
}
