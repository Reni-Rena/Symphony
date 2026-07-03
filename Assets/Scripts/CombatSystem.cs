using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class CombatSystem
{
    public async static void ResolveCombat(Pion attacker, Pion defender)
    {
        Pion left = attacker.isEnemy ? defender : attacker;
        Pion right = attacker.isEnemy ? attacker : defender;

        CombatUI.Instance.ShowCombat(left, right);
        await Task.Delay(5000);

        bool attackerTurn = true;
        for (int i = 0; i < 6; i++)
        {
            if (!attacker.squad.IsAlive() || !defender.squad.IsAlive())
                break;

            if (attackerTurn)
                ResolveAttack(attacker.squad, defender.squad);
            else
                ResolveAttack(defender.squad, attacker.squad);

            attackerTurn = !attackerTurn;
            await Task.Delay(3000);
        }

        CombatUI.Instance.HideCombat();

        // Retire les pions morts de la carte
        if (!attacker.squad.IsAlive()) attacker.Die();
        if (!defender.squad.IsAlive()) defender.Die();

        // Vérifie la fin de partie immédiatement après le combat
        GameManager.Instance?.CheckGameOver();
    }

    public static void ResolveAttack(Squad attacker, Squad defender)
    {
        if (attacker == null || defender == null) return;

        Debug.Log("Combat entre " + attacker.name + " et " + defender.name);

        List<Unit> round2 = new List<Unit>();
        List<Unit> round3 = new List<Unit>();
        List<Unit> round4 = new List<Unit>();
        List<Unit> round5 = new List<Unit>();
        List<Unit> round6 = new List<Unit>();

        foreach (Unit u in attacker.GetLivingUnits())
        {
            switch (u.GetAttackOrder())
            {
                case 2: round2.Add(u); break;
                case 3: round3.Add(u); break;
                case 4: round4.Add(u); break;
                case 5: round5.Add(u); break;
                case 6: round6.Add(u); break;
                default: Debug.Log(u.unitName + " : Order inconnu."); break;
            }
        }

        if (round2.Count > 0)
        {
            foreach (Unit u in round2)
                u.target = u.GetComponent<UnitMagic>().CanAttack() ? FindUnprotectedUnits(defender) : null;
            foreach (Unit u in round2)
            {
                if (u.target == null) continue;
                u.target.TakeDamage(u.DealDamage());
                foreach (Unit t in u.GetComponent<UnitMagic>().GetSecondaryTarget(defender, u.target))
                    t.TakeDamage(u.DealDamage() / 2);
            }
            defender.UpdatedAttackInfo();
        }

        if (round3.Count > 0)
        {
            foreach (Unit u in round3) u.target = FindUnprotectedUnits(defender);
            foreach (Unit u in round3) if (u.target != null) u.target.TakeDamage(u.DealDamage());
            defender.UpdatedAttackInfo();
        }

        if (round4.Count > 0)
        {
            foreach (Unit u in round4) u.target = FindTargetFirstLine(defender);
            foreach (Unit u in round4) if (u.target != null) u.target.TakeDamage(u.DealDamage());
            defender.UpdatedAttackInfo();
        }

        if (round5.Count > 0)
        {
            foreach (Unit u in round5) u.target = FindTargetFirstLine(defender);
            foreach (Unit u in round5) if (u.target != null) u.target.TakeDamage(u.DealDamage());
            defender.UpdatedAttackInfo();
        }

        if (round6.Count > 0)
        {
            foreach (Unit u in round6) u.target = FindHurtUnit(attacker);
            foreach (Unit u in round6) if (u.target != null) u.target.HealDamage(u.DealDamage());
            attacker.UpdatedAttackInfo();
        }
    }

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