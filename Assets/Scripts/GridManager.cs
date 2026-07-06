using UnityEngine;
using System.Collections.Generic;

// Zone rectangulaire d'un type de terrain donne.
// Les zones sont appliquees dans l'ordre -- la derniere ecrase les precedentes.
[System.Serializable]
public class TerrainZone
{
    public TerrainType type = TerrainType.Plaine;
    [Tooltip("Coin bas-gauche de la zone (coordonnees grille)")]
    public Vector2Int from;
    [Tooltip("Coin haut-droit de la zone (coordonnees grille)")]
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

    [Header("Prefab et Palette")]
    public GameObject tilePrefab;
    public GamePalette palette;

    [Header("Zones de terrain")]
    [Tooltip("Par defaut tout est Plaine. Ajoute des zones pour placer Foret, Colline, Montagne, Route, Riviere, Marais ou Desert.")]
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

                // Terrain par defaut : Plaine
                tile.terrainType = TerrainType.Plaine;

                // Applique les zones dans l'ordre (la derniere gagne)
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

    // Retrouver une tile par coordonnee
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