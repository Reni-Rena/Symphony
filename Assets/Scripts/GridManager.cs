using UnityEngine;
using System.Collections.Generic;

// Zone rectangulaire d'un type de terrain donné.
// Les zones sont appliquées dans l'ordre — la dernière écrase les précédentes.
[System.Serializable]
public class TerrainZone
{
    public TerrainType type = TerrainType.Plaine;
    [Tooltip("Coin bas-gauche de la zone (coordonnées grille)")]
    public Vector2Int from;
    [Tooltip("Coin haut-droit de la zone (coordonnées grille)")]
    public Vector2Int to;
}

public class GridManager : MonoBehaviour
{
    [Header("Taille de la grille")]
    public int widthMin = -10;
    public int widthMax = 10;
    public int heightMin = -10;
    public int heightMax = 10;
    public float cellSize = 1f;

    [Header("Prefab & Palette")]
    public GameObject tilePrefab;
    public GamePalette palette;

    [Header("Zones de terrain")]
    [Tooltip("Par défaut tout est Plaine. Ajoute des zones pour placer Forêt, Eau, Montagne...")]
    public List<TerrainZone> terrainZones = new List<TerrainZone>();

    // Dictionnaire pour retrouver une tile par position
    private Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = widthMin; x < widthMax; x++)
        {
            for (int y = heightMin; y < heightMax; y++)
            {
                Vector2 position = new Vector2(x * cellSize, y * cellSize);
                GameObject go = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                go.name = $"Tile {x} {y}";

                Tile tile = go.GetComponent<Tile>();
                tile.palette = palette;

                // Terrain par défaut : Plaine
                tile.terrainType = TerrainType.Plaine;

                // Applique les zones dans l'ordre (la dernière gagne)
                Vector2Int coord = new Vector2Int(x, y);
                foreach (TerrainZone zone in terrainZones)
                {
                    if (coord.x >= zone.from.x && coord.x <= zone.to.x &&
                        coord.y >= zone.from.y && coord.y <= zone.to.y)
                    {
                        tile.terrainType = zone.type;
                    }
                }

                tile.ApplyTerrainColor();
                tileMap[coord] = tile;
            }
        }
    }

    // Utilitaire : retrouver une tile par coordonnée
    public Tile GetTile(int x, int y)
    {
        tileMap.TryGetValue(new Vector2Int(x, y), out Tile tile);
        return tile;
    }

    public Tile GetTile(Vector2Int coord)
    {
        tileMap.TryGetValue(coord, out Tile tile);
        return tile;
    }
}