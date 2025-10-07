using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int widthMax = 10;   // largeur de la grille
    public int widthMin = -10;
    public int heightMax = 10;
    public int heightMin = -10;
    public float cellSize = 1f; // taille d'une case

    public GameObject tilePrefab; // prefab de case (un simple carré SpriteRenderer)

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
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.name = $"Tile {x} {y}";
                tile.transform.parent = transform; // pour ranger les cases sous GridManager
            }
        }
    }
}
