using UnityEngine;

public enum TerrainType { Plaine, Foret, Eau, Montagne }

/// <summary>
/// La Tile ne prend plus de décisions : elle expose son état (deplacement/attaque)
/// via des flags booléens, et ne fait plus de FindObjectsByType dans OnClicked.
/// Toute la logique d'interaction est dans GameManager.
/// </summary>
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

    //  État interne (plus fiable que tester la couleur) 
    private bool _estCaseDeplacement = false;
    private bool _estCaseAttaque = false;

    public bool EstCaseDeplacement() => _estCaseDeplacement;
    public bool EstCaseAttaque() => _estCaseAttaque;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        ApplyTerrainColor();
        baseColor = _renderer.color;
    }

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

    //  Survol 

    void OnMouseEnter()
    {
        Vector2 pos = new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y));
        HUDManager.Instance?.UpdateBottomBar(GetTerrainName(), GetTerrainDefense(), pos);
    }

    //  Highlights 

    public static void HighlightTiles(Pion pion)
    {
        Vector2 pionPos = pion.GetGridPosition();
        int range = pion.GetMoveRange();

        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Vector2 tilePos = new Vector2(
                Mathf.Round(tile.transform.position.x),
                Mathf.Round(tile.transform.position.y));

            // Ne pas surligner la case du pion lui-même
            if (tilePos == pionPos) continue;

            float dist = Mathf.Abs(tilePos.x - pionPos.x) + Mathf.Abs(tilePos.y - pionPos.y);
            if (dist <= range)
            {
                tile._renderer.color = tile.moveColor;
                tile._estCaseDeplacement = true;
            }
        }
    }

    public static void HighlightAttackableTiles(Pion pion)
    {
        Vector2 pionPos = pion.GetGridPosition();
        int range = 1; // portée d'attaque — ajustez selon vos règles

        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Vector2 tilePos = new Vector2(
                Mathf.Round(tile.transform.position.x),
                Mathf.Round(tile.transform.position.y));

            if (tilePos == pionPos) continue;

            float dist = Mathf.Abs(tilePos.x - pionPos.x) + Mathf.Abs(tilePos.y - pionPos.y);
            if (dist <= range)
            {
                tile._renderer.color = tile.attackColor;
                tile._estCaseAttaque = true;
            }
        }
    }

    public static void ClearHighlights()
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            tile._renderer.color = tile.baseColor;
            tile._estCaseDeplacement = false;
            tile._estCaseAttaque = false;
        }
    }
}