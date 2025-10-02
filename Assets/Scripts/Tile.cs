using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    public Color baseColor;
    public Color moveColor;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        Pion selectedPion = null;
        foreach (Pion u in FindObjectsOfType<Pion>())
        {
            if (u.IsSelected())
            {
                selectedPion = u;
                break;
            }
        }

        if (selectedPion != null && _renderer.color == moveColor)
        {
            bool occupied = false;
            foreach (Pion u in FindObjectsOfType<Pion>())
            {
                if ((Vector2)u.transform.position == new Vector2(transform.position.x, transform.position.y))
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                selectedPion.MoveTo(transform.position);
            }
        }
    }

    public static void HighlightTiles(Pion pion)
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        Vector2 pionPos = pion.GetGridPosition();
        int range = pion.GetMoveRange();

        foreach (Tile tile in tiles)
        {
            Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
            float distance = Mathf.Abs(tilePos.x - pionPos.x) + Mathf.Abs(tilePos.y - pionPos.y); // distance Manhattan
            if (distance <= range)
            {
                tile._renderer.color = tile.moveColor;
            }
        }
    }

    public static void ClearHighlights()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile._renderer.color = tile.baseColor;
        }
    }
}
