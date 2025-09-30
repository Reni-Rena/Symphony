using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color baseColor;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        baseColor = _renderer.color;
    }

    void OnMouseDown()
    {
        Unit selectedUnit = null;
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u.IsSelected())
            {
                selectedUnit = u;
                break;
            }
        }

        if (selectedUnit != null && _renderer.color == Color.cyan)
        {
            // Vérifie qu'aucune unité n'est déjà sur cette case
            bool occupied = false;
            foreach (Unit u in FindObjectsOfType<Unit>())
            {
                if ((Vector2)u.transform.position == new Vector2(transform.position.x, transform.position.y))
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                selectedUnit.MoveTo(transform.position);
            }
        }
    }


    public static void HighlightTiles(Unit unit)
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        Vector2 unitPos = unit.GetGridPosition();
        int range = unit.GetMoveRange();

        foreach (Tile tile in tiles)
        {
            Vector2 tilePos = new Vector2(Mathf.Round(tile.transform.position.x), Mathf.Round(tile.transform.position.y));
            float distance = Mathf.Abs(tilePos.x - unitPos.x) + Mathf.Abs(tilePos.y - unitPos.y); // distance Manhattan
            if (distance <= range)
            {
                tile._renderer.color = Color.cyan;
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
