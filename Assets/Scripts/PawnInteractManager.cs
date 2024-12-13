using System.Collections;
using UnityEngine;

public class PawnInteractManager : MonoBehaviour
{
    public static PawnInteractManager Instance { get; private set; }

    GameManager gameManager;
    GridManager gridManager;

    [SerializeField] private Transform minPoint, maxPoint;

    public bool interactEnabled;

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
        if (!interactEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            if(GetGridMousePosition(out clickDownPosition))
            {
                holdClickCoroutine = StartCoroutine(HoldClick());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (holdClickCoroutine != null) StopCoroutine(holdClickCoroutine);

            if (GetGridMousePosition(out Vector2Int position) && position == clickDownPosition)
            {
                if (gameManager.SelectedPawn == null)
                {
                    Pawn pawn = gridManager.GetPawn(position, GridTarget.Active);
                    if (pawn &&
                        pawn == gridManager.GetFirstPawnInCol(position.x, GridTarget.Active))
                    {
                        interactEnabled = false;
                        StartCoroutine(gameManager.SelectPawn(pawn, EnableInteraction));
                    }
                }
                else
                {

                }
            }
        }
    }

    private IEnumerator HoldClick()
    {
        yield return new WaitForSeconds(holdClickDuration);
        if (interactEnabled && gameManager.SelectedPawn == null && GetGridMousePosition(out Vector2Int position) && position == clickDownPosition)
        {
            interactEnabled = false;
            clickDownPosition = new(-1, -1);
            Pawn pawn = gridManager.GetPawn(position, GridTarget.Active);
            if (pawn)
            {
                StartCoroutine(gameManager.DestroyPawn(pawn, EnableInteraction));
            }
        }
    }

    private void EnableInteraction()
    {
        interactEnabled = true;
    }

    /// <summary>
    /// Get the grid box pointed to by the mouse
    /// </summary>
    /// <returns>
    /// Is the position in the grid
    /// </returns>
    private bool GetGridMousePosition(out Vector2Int position)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position = Vector2Int.FloorToInt(MathUtils.Map(mousePosition, minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
        return 0 <= position.x && position.x < GridManager.gridSize.x && 0 <= position.y && position.y < GridManager.gridSize.y;
    }
}
