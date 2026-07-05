using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CombatSystem refactorisé en Coroutine (plus de async/await).
/// GameManager attend la fin du combat avant de changer de phase.
/// </summary>
public static class CombatSystem
{
    /// <summary>
    /// Coroutine principale appelée par GameManager via StartCoroutine.
    /// </summary>
    public static IEnumerator ResolveCombatCoroutine(Pion attacker, Pion defender)
    {
        if (attacker == null || defender == null) yield break;

        // Afficher l'UI de combat (pion gauche = joueur, droit = ennemi)
        Pion left = attacker.isEnemy ? defender : attacker;
        Pion right = attacker.isEnemy ? attacker : defender;
        CombatUI.Instance?.ShowCombat(left, right);

        yield return new WaitForSeconds(2f); // pause avant les échanges

        // Jusqu'à 6 échanges alternés
        bool attackerTurn = true;
        for (int i = 0; i < 6; i++)
        {
            // Vérification de nullité (pion peut être détruit pendant les tours)
            if (attacker == null || !attacker.squad.IsAlive()) break;
            if (defender == null || !defender.squad.IsAlive()) break;

            if (attackerTurn)
                ResolveAttack(attacker.squad, defender.squad);
            else
                ResolveAttack(defender.squad, attacker.squad);

            attackerTurn = !attackerTurn;
            yield return new WaitForSeconds(1.5f);
        }

        CombatUI.Instance?.HideCombat();

        // Éliminer les pions morts
        if (attacker != null && !attacker.squad.IsAlive()) attacker.Die();
        if (defender != null && !defender.squad.IsAlive()) defender.Die();

        // Petite pause après la fin du combat avant de rendre la main
        yield return new WaitForSeconds(0.5f);
    }

    //  Résolution d'un échange 

    public static void ResolveAttack(Squad attacker, Squad defender)
    {
        if (attacker == null || defender == null) return;

        Debug.Log($"Combat : {attacker.name} → {defender.name}");

        // Trier les unités par ordre d'attaque
        var round2 = new List<Unit>();
        var round3 = new List<Unit>();
        var round4 = new List<Unit>();
        var round5 = new List<Unit>();
        var round6 = new List<Unit>();

        foreach (Unit u in attacker.GetLivingUnits())
        {
            switch (u.GetAttackOrder())
            {
                case 2: round2.Add(u); break;
                case 3: round3.Add(u); break;
                case 4: round4.Add(u); break;
                case 5: round5.Add(u); break;
                case 6: round6.Add(u); break;
                default: Debug.LogWarning($"{u.unitName} : ordre d'attaque inconnu."); break;
            }
        }

        // Ordre 2 – Magie (splash)
        if (round2.Count > 0)
        {
            foreach (Unit u in round2)
                u.target = u.GetComponent<UnitMagic>()?.CanAttack() == true
                    ? FindUnprotectedUnits(defender) : null;

            foreach (Unit u in round2)
            {
                if (u.target == null) continue;
                u.target.TakeDamage(u.DealDamage());
                foreach (Unit t in u.GetComponent<UnitMagic>().GetSecondaryTarget(defender, u.target))
                    t.TakeDamage(u.DealDamage() / 2);
            }
            defender.UpdatedAttackInfo();
        }

        // Ordre 3 – Attaque sur unités non protégées
        if (round3.Count > 0)
        {
            foreach (Unit u in round3) u.target = FindUnprotectedUnits(defender);
            foreach (Unit u in round3) if (u.target != null) u.target.TakeDamage(u.DealDamage());
            defender.UpdatedAttackInfo();
        }

        // Ordre 4 – Première ligne
        if (round4.Count > 0)
        {
            foreach (Unit u in round4) u.target = FindTargetFirstLine(defender);
            foreach (Unit u in round4) if (u.target != null) u.target.TakeDamage(u.DealDamage());
            defender.UpdatedAttackInfo();
        }

        // Ordre 5 – Première ligne
        if (round5.Count > 0)
        {
            foreach (Unit u in round5) u.target = FindTargetFirstLine(defender);
            foreach (Unit u in round5) if (u.target != null) u.target.TakeDamage(u.DealDamage());
            defender.UpdatedAttackInfo();
        }

        // Ordre 6 – Soin sur alliés blessés
        if (round6.Count > 0)
        {
            foreach (Unit u in round6) u.target = FindHurtUnit(attacker);
            foreach (Unit u in round6) if (u.target != null) u.target.HealDamage(u.DealDamage());
            attacker.UpdatedAttackInfo();
        }
    }

    //  Sélection de cibles 

    private static Unit FindTargetFirstLine(Squad defender)
    {
        if (defender.attackTargetFirstLine.Count == 0) return null;
        return defender.attackTargetFirstLine[Random.Range(0, defender.attackTargetFirstLine.Count)];
    }

    private static Unit FindUnprotectedUnits(Squad defender)
    {
        if (defender.attackUnprotectedUnits.Count == 0) return null;
        return defender.attackUnprotectedUnits[Random.Range(0, defender.attackUnprotectedUnits.Count)];
    }

    private static Unit FindHurtUnit(Squad attacker)
    {
        if (attacker.attackHurtUnit.Count == 0) return null;
        return attacker.attackHurtUnit[Random.Range(0, attacker.attackHurtUnit.Count)];
    }
}