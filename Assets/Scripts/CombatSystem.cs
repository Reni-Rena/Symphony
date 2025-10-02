using UnityEngine;

public static class CombatSystem
{
    // Combat entre deux escouades
    public static void ResolveCombat(Squad attacker, Squad defender)
    {
        if (attacker == null || defender == null) return;

        Debug.Log(" Combat entre " + attacker.name + " et " + defender.name);

        foreach (Unit u in attacker.GetLivingUnits())
        {
            if (u == null || u.currentHP <= 0) continue;

            Unit target = FindTarget(u, defender);

            if (target != null)
            {
                int damage = Mathf.Max(1, u.attack - target.defense);
                target.TakeDamage(damage);
                Debug.Log(u.name + " attaque " + target.name + " pour " + damage + " dégâts !");
            }
        }
    }

    // Trouver une cible en fonction du type de l’attaquant
    private static Unit FindTarget(Unit attacker, Squad defender)
    {
        for (int y = 0; y < 3; y++) // front  arrière
        {
            for (int x = 0; x < 3; x++)
            {
                Unit candidate = defender.formation[x, y];
                if (candidate != null && candidate.currentHP > 0)
                {
                    // Si distance  peut tirer partout
                    if (attacker.isRanged)
                        return candidate;

                    // Si melee  tape seulement la frontline (y == 0)
                    if (!attacker.isRanged && y == 0)
                        return candidate;
                }
            }
        }
        return null;
    }
}
