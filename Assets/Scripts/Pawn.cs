using UnityEngine;

public class Pawn : MonoBehaviour
{
    private int grid;
    private Vector2Int position;
    [SerializeField] private Vector2Int size = Vector2Int.one;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new(1f, 0.6f, 0.1f);
        Gizmos.DrawWireCube(transform.position + new Vector3(0.5f * size.x - 0.5f, -0.5f * size.y + 0.5f), (Vector2)size);
    }

    public bool Match(Vector2Int position)
    {
        return (this.position.x <= position.x && position.x < this.position.x + size.x &&
                this.position.y <= position.y && position.y < this.position.y + size.y);
    }

    private Vector3 GetTransformPos()
    {
        if (grid == 0)
        {
            return new(position.x, -position.y);
        }
        else
        {
            return new(-position.x - size.x + 1, position.y + size.y - 1);
        }
    }

    public void GoTo(Vector2Int position)
    {
        
        this.position = position;
        transform.localPosition = GetTransformPos();
    }

    public void Init(int grid, Vector2Int position)
    {
        this.grid = grid;
        GoTo(position);
    }
}
