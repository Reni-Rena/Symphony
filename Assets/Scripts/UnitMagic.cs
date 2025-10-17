using System.Collections.Generic;
using UnityEngine;

public enum AttackTypeMagic
{
    Basic,
    Fire,
    Ice,
    Tunder
}

public class UnitMagic : MonoBehaviour
{
    public AttackTypeMagic attackType = AttackTypeMagic.Basic;
    public int spellIncantation = 1;
    public int incantation = 0;

    public bool CanAttack()
    {
        incantation += 1;
        if (incantation >= spellIncantation)
        {
            incantation -= spellIncantation;
            return true;
        }
        return false;
    }

    public List<Unit> GetSecondaryTarget(Squad defender, Unit target)
    {
        List<Unit> secondaryTarget = new List<Unit>();
        if (attackType == AttackTypeMagic.Basic)
        {
            if (!defender.occupiedCells.ContainsKey(target))
                return secondaryTarget;

            // Ensemble pour éviter les doublons
            HashSet<Unit> uniqueUnits = new HashSet<Unit>();

            // Pour chaque cellule que le target occupe
            foreach (Vector2Int cell in defender.occupiedCells[target])
            {
                // On parcourt les cases voisines (8 directions)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        // On saute la cellule centrale (celle du target)
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = cell.x + dx;
                        int ny = cell.y + dy;

                        // Vérifie que la case est dans la grille
                        if (nx < 0 || nx >= defender.gridWidth || ny < 0 || ny >= defender.gridHeight)
                            continue;

                        Unit neighbor = defender.formation[nx, ny];

                        // Vérifie que c'est une unité différente et vivante
                        if (neighbor != null && neighbor != target && neighbor.currentHP > 0)
                        {
                            uniqueUnits.Add(neighbor);
                        }
                    }
                }
            }

            secondaryTarget.AddRange(uniqueUnits);
        }
        return secondaryTarget;
    }
}
