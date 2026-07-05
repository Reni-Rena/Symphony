using UnityEngine;

public enum TerrainType
{
    Plaine,
    Foret,
    Eau,
    Montagne
}

public class Tile : MonoBehaviour
{
    [Header("Terrain")]
    public TerrainType terrainType = TerrainType.Plaine;

    [Header("Palette")]
    public GamePalette palette;

    private SpriteRenderer _renderer;
    private Color baseColor;
    public Color moveColor;
    public Color attackColor;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        ApplyTerrainColor();
        baseColor = _renderer.color;
    }

    // Applique la couleur de base selon le type de terrain
    public void ApplyTerrainColor()
    {
        if (palette == null) return;
        _renderer.color = GetTerrainColor();
        baseColor = _renderer.color;
    }

    private Color GetTerrainColor()
    {
        switch (terrainType)
        {
            case TerrainType.Plaine: return palette.terreCuite01;
            case TerrainType.Foret: return palette.vertForet01;
            case TerrainType.Eau: return palette.bleuRoyal01;
            case TerrainType.Montagne: return palette.encreNoire03;
            default: return palette.terreCuite01;
        }
    }

    public string GetTerrainName()
    {
        switch (terrainType)
        {
            case TerrainType.Plaine: return "Plaine";
            case TerrainType.Foret: return "Forêt";
            case TerrainType.Eau: return "Eau";
            case TerrainType.Montagne: return "Montagne";
            default: return "Plaine";
        }
    }

    public int GetTerrainDefense()
    {
        switch (terrainType)
        {
            case TerrainType.Plaine: return 0;
            case TerrainType.Foret: return 2;
            case TerrainType.Eau: return -1;
            case TerrainType.Montagne: return 3;
            default: return 0;
        }
    }

    // Survol souris : met à jour la bottom bar 

    void OnMouseEnter()
    {
        Vector2 pos = new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y));

        HUDManager.Instance?.UpdateBottomBar(GetTerrainName(), GetTerrainDefense(), pos);
    }

    // Clic 

    public void OnClicked()
    {
        Pion selectedPion = null;
        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (u.IsSelected()) { selectedPion = u; break; }
        }

        if (selectedPion != null && _renderer.color == moveColor)
        {
            bool occupied = false;
            foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            {
                if ((Vector2)u.transform.position == (Vector2)transform.position)
                { occupied = true; break; }
            }
            if (!occupied) selectedPion.MoveTo(transform.position);
        }

        if (selectedPion != null && _renderer.color == attackColor)
        {
            foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            {
                if ((Vector2)u.transform.position == (Vector2)transform.position && u.isEnemy)
                { selectedPion.Combat(u); break; }
            }
        }
    }

    // Highlights

    public static void HighlightTiles(Pion pion)
    {
        Vector2 pionPos = pion.GetGridPosition();
        int range = pion.GetMoveRange();

        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Vector2 tilePos = new Vector2(
                Mathf.Round(tile.transform.position.x),
                Mathf.Round(tile.transform.position.y));
            float dist = Mathf.Abs(tilePos.x - pionPos.x) + Mathf.Abs(tilePos.y - pionPos.y);
            if (dist <= range) tile._renderer.color = tile.moveColor;
        }
    }

    public static void HighlightAttackableTiles(Pion pion)
    {
        Vector2 pionPos = pion.GetGridPosition();

        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Vector2 tilePos = new Vector2(
                Mathf.Round(tile.transform.position.x),
                Mathf.Round(tile.transform.position.y));
            float dist = Mathf.Abs(tilePos.x - pionPos.x) + Mathf.Abs(tilePos.y - pionPos.y);
            if (dist <= 1) tile._renderer.color = tile.attackColor;
        }
    }

    public static void ClearHighlights()
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
            tile._renderer.color = tile.baseColor;
    }
}