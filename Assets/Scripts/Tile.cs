using UnityEngine;

// 8 types de terrain jouables
public enum TerrainType
{
    Plaine,
    Foret,
    Colline,
    Montagne,
    Route,
    Riviere,
    Marais,
    Desert
}

/// <summary>
/// La Tile ne prend pas de decisions : elle expose son etat (deplacement/attaque)
/// via des flags booleens, et ne fait pas de FindObjectsByType dans OnClicked.
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

    // Etat interne (plus fiable que tester la couleur)
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
            case TerrainType.Colline: return palette.vertForet01;   // a ajuster dans la palette
            case TerrainType.Montagne: return palette.encreNoire03;
            case TerrainType.Route: return palette.terreCuite01;  // a ajuster dans la palette
            case TerrainType.Riviere: return palette.bleuRoyal01;
            case TerrainType.Marais: return palette.bleuRoyal01;   // a ajuster dans la palette
            case TerrainType.Desert: return palette.terreCuite01;  // a ajuster dans la palette
            default: return palette.terreCuite01;
        }
    }

    public string GetTerrainName()
    {
        switch (terrainType)
        {
            case TerrainType.Plaine: return "Plaine";
            case TerrainType.Foret: return "Foret";
            case TerrainType.Colline: return "Colline";
            case TerrainType.Montagne: return "Montagne";
            case TerrainType.Route: return "Route";
            case TerrainType.Riviere: return "Riviere";
            case TerrainType.Marais: return "Marais";
            case TerrainType.Desert: return "Desert";
            default: return "Plaine";
        }
    }

    /// <summary>
    /// Retourne le modificateur de degats recu par l'escouade sur cette case.
    /// 0f = neutre, -0.10f = -10% de degats, +0.15f = +15% de degats.
    /// Usage : degatsFinaux = degatsBase * (1f + tile.GetDefenseModifier())
    /// </summary>
    public float GetDefenseModifier()
    {
        return TerrainRules.GetDefenseModifier(terrainType);
    }

    /// <summary>
    /// Retourne le cout en points de deplacement pour entrer sur cette case
    /// selon le MoveType de l'escouade. -1 signifie case inaccessible.
    /// </summary>
    public float GetMoveCost(MoveType moveType)
    {
        return TerrainRules.GetMoveCost(terrainType, moveType);
    }

    /// <summary>
    /// Retourne true si l'escouade de ce MoveType peut entrer sur cette case.
    /// </summary>
    public bool IsAccessible(MoveType moveType)
    {
        return TerrainRules.IsAccessible(terrainType, moveType);
    }

    // Survol

    void OnMouseEnter()
    {
        Vector2 pos = new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y));

        int defensePercent = Mathf.RoundToInt(GetDefenseModifier() * 100f);
        HUDManager.Instance?.UpdateBottomBar(GetTerrainName(), defensePercent, pos);
    }

    // Highlights

    /// <summary>
    /// Surligne les cases accessibles au pion en tenant compte du MoveType et du cout reel.
    /// Utilise un Dijkstra simple pour propager les points de deplacement restants.
    /// </summary>
    public static void HighlightTiles(Pion pion)
    {
        Vector2 pionPos = pion.GetGridPosition();
        float movePoints = pion.GetMoveRange();
        MoveType moveType = pion.squad != null ? pion.squad.squadMoveType : MoveType.Infanterie;

        // Dictionnaire : position -> points de deplacement restants apres avoir atteint cette case
        System.Collections.Generic.Dictionary<Vector2, float> reachable =
            new System.Collections.Generic.Dictionary<Vector2, float>();

        System.Collections.Generic.List<(float remaining, Vector2 pos)> open =
            new System.Collections.Generic.List<(float, Vector2)>();

        reachable[pionPos] = movePoints;
        open.Add((movePoints, pionPos));

        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        while (open.Count > 0)
        {
            // On prend le noeud avec le plus de points restants
            open.Sort((a, b) => b.remaining.CompareTo(a.remaining));
            var (remaining, current) = open[0];
            open.RemoveAt(0);

            if (reachable.TryGetValue(current, out float best) && remaining < best) continue;

            foreach (Vector2 dir in dirs)
            {
                Vector2 neighbor = current + dir;
                Tile neighborTile = GetTileAt(neighbor);
                if (neighborTile == null) continue;
                if (!neighborTile.IsAccessible(moveType)) continue;

                float cost = neighborTile.GetMoveCost(moveType);
                float newRemaining = remaining - cost;
                if (newRemaining < 0f) continue;

                if (!reachable.TryGetValue(neighbor, out float prevBest) || newRemaining > prevBest)
                {
                    reachable[neighbor] = newRemaining;
                    open.Add((newRemaining, neighbor));
                }
            }
        }

        // Surlignage des cases atteignables
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Vector2 tilePos = new Vector2(
                Mathf.Round(tile.transform.position.x),
                Mathf.Round(tile.transform.position.y));

            if (tilePos == pionPos) continue;

            if (reachable.ContainsKey(tilePos))
            {
                tile._renderer.color = tile.moveColor;
                tile._estCaseDeplacement = true;
            }
        }
    }

    public static void HighlightAttackableTiles(Pion pion)
    {
        Vector2 pionPos = pion.GetGridPosition();
        int range = 1;

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

    // Utilitaire interne : retrouver une Tile a une position monde arrondie
    private static Tile GetTileAt(Vector2 worldPos)
    {
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Vector2 tilePos = new Vector2(
                Mathf.Round(tile.transform.position.x),
                Mathf.Round(tile.transform.position.y));
            if (tilePos == worldPos) return tile;
        }
        return null;
    }
}