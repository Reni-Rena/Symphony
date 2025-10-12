using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SquadSlot
{
    public Unit unitPrefab;
    [Range(0, 5)] public int x;
    [Range(0, 5)] public int y;
}

public class Squad : MonoBehaviour
{
    [Header("Taille de la grille")]
    public int gridWidth = 6;
    public int gridHeight = 6;
    public float cellSize = 0.5f;

    [Header("Configuration dans l’inspecteur")]
    public List<SquadSlot> slots = new List<SquadSlot>();

    [HideInInspector] public Unit[,] formation;
    public Dictionary<Unit, List<Vector2Int>> occupiedCells = new Dictionary<Unit, List<Vector2Int>>();
    public List<Unit> attackTargetFirstLine = new List<Unit>();



    void Awake()
    {
        BuildFormation();
        UpdatedAttackInfo();
    }

    // Construction de la formation
    public void BuildFormation()
    {
        formation = new Unit[gridWidth, gridHeight];
        occupiedCells.Clear();

        foreach (var slot in slots)
        {
            if (slot.unitPrefab == null)
                continue;

            Unit u = Instantiate(slot.unitPrefab, transform);
            Vector2Int size = u.size;

            if (!CanPlaceUnit(slot.x, slot.y, size))
            {
                Debug.LogWarning($"Impossible de placer {u.unitName} en {slot.x},{slot.y} : place occupée !");
                Destroy(u.gameObject);
                continue;
            }

            List<Vector2Int> cells = new List<Vector2Int>();

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    formation[slot.x + i, slot.y + j] = u;
                    cells.Add(new Vector2Int(slot.x + i, slot.y + j));
                }
            }

            occupiedCells[u] = cells;
        }
    }

    // Vérifie si une unité peut être placée à une position donnée
    private bool CanPlaceUnit(int x, int y, Vector2Int size)
    {
        if (x + size.x > gridWidth || y + size.y > gridHeight)
            return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (formation[x + i, y + j] != null)
                    return false;
            }
        }
        return true;
    }

    public void UpdatedAttackInfo()
    {
        attackTargetFirstLine.Clear();

        foreach (Unit u in GetLivingUnits())
        {
            attackTargetFirstLine = GetFrontlineUnits();
        }
    }

    public List<Unit> GetFrontlineUnits()
    {
        List<Unit> frontline = new List<Unit>();

        foreach (var kvp in occupiedCells)
        {
            Unit unit = kvp.Key;
            if (unit == null || unit.currentHP <= 0)
                continue;

            bool hasSomeoneInFront = false;

            foreach (Vector2Int cell in kvp.Value)
            {
                // Parcourt toutes les cases devant cette cellule jusqu'au bord avant
                for (int y = cell.y - 1; y >= 0; y--)
                {
                    Unit frontUnit = formation[cell.x, y];
                    if (frontUnit != null && frontUnit != unit && frontUnit.currentHP > 0)
                    {
                        hasSomeoneInFront = true;
                        break; // pas besoin de continuer
                    }
                }

                if (hasSomeoneInFront)
                    break; // on arrête si une seule case est bloquée
            }

            if (!hasSomeoneInFront)
                frontline.Add(unit);
        }

        return frontline;
    }


    // Retourner toutes les unités vivantes
    public List<Unit> GetLivingUnits()
    {
        List<Unit> alive = new List<Unit>();
        foreach (Unit u in formation)
        {
            if (u != null && u.currentHP > 0)
                alive.Add(u);
        }
        return alive;
    }

    // Vérifier si l’escouade est encore en vie
    public bool IsAlive()
    {
        return GetLivingUnits().Count > 0;
    }
}
