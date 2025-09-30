using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;   // largeur de la grille
    public int height = 10;  // hauteur de la grille
    public float cellSize = 1f; // taille d'une case

    public GameObject tilePrefab; // prefab de case (un simple carré SpriteRenderer)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * cellSize, y * cellSize);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.name = $"Tile {x} {y}";
                tile.transform.parent = transform; // pour ranger les cases sous GridManager
            }
        }
    }
}
