using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;


public static class CombatSystem
{
    // Combat entre deux escouades
    public async static void ResolveCombat(Pion attacker, Pion defender)
    {

        if (attacker.isEnemy == false)
            AfficheCombat(attacker, defender);
        else
            AfficheCombat(defender, attacker);

        await Task.Delay(1000);
        bool Att = true;
        for (int i = 0; i < 6; i++)
        {
            if (Att)
                CombatSystem.ResolveAttack(attacker.squad, defender.squad);
            else
                CombatSystem.ResolveAttack(defender.squad, attacker.squad);
            Att = !Att;
            await Task.Delay(500);
        }
        HideCombat();
    }
        

    public static void AfficheCombat(Pion Left, Pion Right)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CombatScreen.SetActive(true);
        string LIN = "";
        string COL = "";
        Image imageL;
        Image imageR;
        GameObject iconeL;
        GameObject iconeR;
        Image[] iconeLImages;
        Image[] iconeRImages;
        Unit unitL;
        Unit unitR;

        for (int lin = 0; lin < 6; lin++)
        {
            if (lin == 0) LIN = "/1";
            if (lin == 1) LIN = "/2";
            if (lin == 2) LIN = "/3";
            if (lin == 3) LIN = "/4";
            if (lin == 4) LIN = "/5";
            if (lin == 5) LIN = "/6";

            for (int col = 0; col < 6; col++)
            {
                if (col == 0) COL = "/1";
                if (col == 1) COL = "/2";
                if (col == 2) COL = "/3"; 
                if (col == 3) COL = "/4";
                if (col == 4) COL = "/5";
                if (col == 5) COL = "/6";

                imageL = GameObject.Find($"CombatScreen/PlayerSquad{LIN}{COL}/Image").GetComponent<Image>();
                imageL.enabled = false;
                iconeL = GameObject.Find($"SquadScreen/PlayerSquad{LIN}{COL}");
                iconeLImages = iconeL.GetComponentsInChildren<Image>();
                foreach (var iconeLImage in iconeLImages) iconeLImage.enabled = false;

                unitL = Left.squad.formation[col, lin];
                if (unitL != null)
                {
                    imageL.enabled = true;
                    imageL.sprite = unitL.imageSprite;

                    foreach (var iconeLImage in iconeLImages) iconeLImage.enabled = true;
                    iconeL.GetComponentInChildren<Image>().sprite = unitL.iconeSprite;
                    iconeL.GetComponentInChildren<HealthBar>().unit = unitL;
                }

                imageR = GameObject.Find($"CombatScreen/EnemySquad{LIN}{COL}/Image").GetComponent<Image>();
                imageR.enabled = false;
                iconeR = GameObject.Find($"SquadScreen/EnemySquad{LIN}{COL}");
                iconeRImages = iconeR.GetComponentsInChildren<Image>();
                foreach (var iconeRImage in iconeRImages) iconeRImage.enabled = false;

                unitR = Right.squad.formation[col, lin];
                if (unitR != null)
                {
                    imageR.enabled = true;
                    imageR.sprite = unitR.imageSprite;
                    imageR.rectTransform.localScale = new Vector3(-1, 1, 1);

                    foreach (var iconeRImage in iconeRImages) iconeRImage.enabled = true;
                    iconeR.GetComponentInChildren<Image>().sprite = unitR.iconeSprite;
                    iconeR.GetComponentInChildren<HealthBar>().unit = unitR;
                }
            }
        }
    }

    public static void HideCombat()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CombatScreen.SetActive(false);
    }


    // Attack entre deux escouades
    public static void ResolveAttack(Squad attacker, Squad defender)
    {   
        List<Unit> attackRound1 = new List<Unit>();
        List<Unit> attackRound2 = new List<Unit>();
        List<Unit> attackRound3 = new List<Unit>();
        List<Unit> attackRound4 = new List<Unit>();
        List<Unit> attackRound5 = new List<Unit>();
        List<Unit> attackRound6 = new List<Unit>();


        if (attacker == null || defender == null) return;

        Debug.Log(" Combat entre " + attacker.name + " et " + defender.name);


        // Update Attack Round
        foreach (Unit u in attacker.GetLivingUnits())
        {
            switch (u.GetAttackOrder())
            {
                case 1:
                    attackRound1.Add(u);
                    break;
                case 2:
                    attackRound2.Add(u);
                    break;
                case 3:
                    attackRound3.Add(u);
                    break;
                case 4:
                    attackRound4.Add(u);
                    break;
                case 5:
                    attackRound5.Add(u);
                    break;
                case 6:
                    attackRound6.Add(u);
                    break;
                default:
                    Debug.Log(u.unitName + " : Order inconue.");
                    break;
            }
        }
        
        // 2 : Unité magic
        if (!(attackRound2.Count == 0))
        {
            foreach (Unit u in attackRound2)
            {
                if (u.GetComponent<UnitMagic>().CanAttack()) u.target = FindUnprotectedUnits(defender);
                else u.target = null;
            }
            foreach (Unit u in attackRound2)
            {
                if (u.target != null)
                {
                    u.target.TakeDamage(u.DealDamage());
                    List<Unit> secondaryTarget = u.GetComponent<UnitMagic>().GetSecondaryTarget(defender, u.target);
                    foreach (Unit t in secondaryTarget)
                    {
                        t.TakeDamage(u.DealDamage()/2);
                    }
                }
            }
            defender.UpdatedAttackInfo();
        }

        // 3 : Unité archerie
        if (!(attackRound3.Count == 0))
        {
            foreach (Unit u in attackRound3)
            {
                u.target = FindUnprotectedUnits(defender);
            }
            foreach (Unit u in attackRound3)
            {
                if (u.target != null) u.target.TakeDamage(u.DealDamage());
            }
            defender.UpdatedAttackInfo();
        }

        // 4 : Unité légère
        if (!(attackRound4.Count == 0))
        {
            foreach (Unit u in attackRound4)
            {
                u.target = FindTargetFirstLine(defender);
            }
            foreach (Unit u in attackRound4)
            {
                if (u.target != null) u.target.TakeDamage(u.DealDamage());
            }
            defender.UpdatedAttackInfo();
        }

        // 5 : Unité lourd
        if (!(attackRound5.Count == 0))
        {
            foreach (Unit u in attackRound5)
            {
                u.target = FindTargetFirstLine(defender);
            }
            foreach (Unit u in attackRound5)
            {
                if (u.target != null) u.target.TakeDamage(u.DealDamage());
            }
            defender.UpdatedAttackInfo();
        }

        // 6 : Unité support (Défensif)
        if (!(attackRound6.Count == 0))
        {
            foreach (Unit u in attackRound6)
            {
                u.target = FindHurtUnit(attacker);
            }
            foreach (Unit u in attackRound6)
            {
                if (u.target != null) u.target.HealDamage(u.DealDamage());
            }
            attacker.UpdatedAttackInfo();
        }

    }

    // Trouver une cible (Première ligne)
    private static Unit FindTargetFirstLine(Squad defender)
    {
        if (defender.attackTargetFirstLine.Count == 0) return null;

        int randomIndex = Random.Range(0, defender.attackTargetFirstLine.Count);
        return defender.attackTargetFirstLine[randomIndex];
    }

    // Trouver une cible (Unité non protéger)
    private static Unit FindUnprotectedUnits(Squad defender)
    {
        if (defender.attackUnprotectedUnits.Count == 0) return null;

        int randomIndex = Random.Range(0, defender.attackUnprotectedUnits.Count);
        return defender.attackUnprotectedUnits[randomIndex];
        
        
    }

    // Trouver une cible (Blèsser)
    private static Unit FindHurtUnit(Squad attacker)
    {
        if (attacker.attackHurtUnit.Count == 0) return null;

        int randomIndex = Random.Range(0, attacker.attackHurtUnit.Count);
        return attacker.attackHurtUnit[randomIndex];
        
    }
}
