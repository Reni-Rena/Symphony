using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour
{
    // Grille 3x3 d’Unit
    public Unit[,] formation = new Unit[3, 3];

    // Ajouter une unité à une case (x = colonne, y = ligne)
    public void AddUnit(Unit unit, int x, int y)
    {
        if (x >= 0 && x < 3 && y >= 0 && y < 3)
        {
            formation[x, y] = unit;
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
