using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color baseColor;
    public Color moveColor;
    public Color attakColor;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        baseColor = _renderer.color;
    }

    public void OnClicked()
    {
        Pion selectedPion = null;
        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
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
            foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
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

        if (selectedPion != null && _renderer.color == attakColor)
        {
            foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            {
                if ((Vector2)u.transform.position == new Vector2(transform.position.x, transform.position.y) && u.isEnemy == true)
                {
                    selectedPion.Combat(u);
                    break;
                }
            }
        }
    }

    public static void HighlightTiles(Pion pion)
    {
        Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
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

    public static void HighlightAttackableTiles(Pion pion)
    {
        Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        Vector2 pionPos = pion.GetGridPosition();

        foreach (Tile tile in tiles)
        {
            Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
            float distance = Mathf.Abs(tilePos.x - pionPos.x) + Mathf.Abs(tilePos.y - pionPos.y); // distance Manhattan
            if (distance <= 1)
            {
                tile._renderer.color = tile.attakColor;
            }
        }
    }

    public static void ClearHighlights()
    {
        Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in tiles)
        {
            tile._renderer.color = tile.baseColor;
        }
    }
}
