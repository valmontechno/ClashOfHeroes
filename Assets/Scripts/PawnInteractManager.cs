using UnityEngine;

public class PawnInteractManager : MonoBehaviour
{
    GameManager gameManager;
    GridManager gridManager;

    [SerializeField] private Transform minPoint, maxPoint;

    private void Start()
    {
        gameManager = GameManager.Instance;
        gridManager = GridManager.Instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int position = Vector2Int.FloorToInt(MathUtils.Map(mousePosition, minPoint.position, maxPoint.position, Vector2.zero, GridManager.gridSize));
            
            if (0 <= position.x && position.x < GridManager.gridSize.x && 0 <= position.y && position.y < GridManager.gridSize.y)
            {
                print(position);
            }
        }
    }
}
