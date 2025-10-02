using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SquadSlot
{
    public Unit unitPrefab; // Drag & drop d’un prefab d’unité
    [Range(0, 2)] public int Lin;
    [Range(0, 2)] public int Col;
}

public class Squad : MonoBehaviour
{

    public float cellSize = 0.5f;
    public List<SquadSlot> slots = new List<SquadSlot>();

    public Unit[,] formation = new Unit[3, 3];


    void Awake()
    {
        BuildFormation();
    }

    // Construit la formation à partir des slots de l’inspecteur
    public void BuildFormation()
    {
        formation = new Unit[3, 3];

        foreach (var slot in slots)
        {
            if (slot.unitPrefab != null)
            {
                Unit u = Instantiate(slot.unitPrefab, transform);
                formation[slot.Lin, slot.Col] = u;

                Vector3 localPos = new Vector3(
                    (slot.Lin - 1) * cellSize,
                    -(slot.Col - 1) * cellSize,
                    0
                );
                u.transform.localPosition = localPos;
            }
        }
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
