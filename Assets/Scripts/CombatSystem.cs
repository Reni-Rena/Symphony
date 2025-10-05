using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


public static class CombatSystem
{
    // Combat entre deux escouades
    public async static void ResolveCombat(Pion attacker, Pion defender)
    {

        if (attacker.isEnemy == false)
            AfficheCombat(attacker, defender);
        else
            AfficheCombat(defender, attacker);

        bool Att = true;
        for (int i = 0; i < 6; i++)
        {
            if (Att)
                CombatSystem.ResolveAttack(attacker.squad, defender.squad);
            else
                CombatSystem.ResolveAttack(defender.squad, attacker.squad);
            Att = !Att;
            await Task.Delay(2000);
        }
    }
        

    public static void AfficheCombat(Pion Left, Pion Right)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().CombatScreen.SetActive(true);
        Image image;
        Unit unit;

        /*
         * 01: Unit FrontRow Left
         * 02: Unit FrontRow Center
         * 03: Unit FrontRow Right
         * 04: Unit MidleRow Left
         * 05: Unit MidleRow Center
         * 06: Unit MidleRow Right
         * 07: Unit BackRow Left
         * 08: Unit BackRow Center
         * 09: Unit BackRow Right
         */

        // Player Unit 

        // 01
        image = GameObject.Find("CombatScreen/PlayerSquad/FrontRow/Left/Image").GetComponent<Image>();
        unit = Left.squad.formation[0, 0];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 02
        image = GameObject.Find("CombatScreen/PlayerSquad/FrontRow/Center/Image").GetComponent<Image>();
        unit = Left.squad.formation[1, 0];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 03
        image = GameObject.Find("CombatScreen/PlayerSquad/FrontRow/Right/Image").GetComponent<Image>();
        unit = Left.squad.formation[2, 0];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 04
        image = GameObject.Find("CombatScreen/PlayerSquad/MidleRow/Left/Image").GetComponent<Image>();
        unit = Left.squad.formation[0, 1];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 05
        image = GameObject.Find("CombatScreen/PlayerSquad/MidleRow/Center/Image").GetComponent<Image>();
        unit = Left.squad.formation[1, 1];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 06
        image = GameObject.Find("CombatScreen/PlayerSquad/MidleRow/Right/Image").GetComponent<Image>();
        unit = Left.squad.formation[2, 1];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 07
        image = GameObject.Find("CombatScreen/PlayerSquad/BackRow/Left/Image").GetComponent<Image>();
        unit = Left.squad.formation[0, 2];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 08
        image = GameObject.Find("CombatScreen/PlayerSquad/BackRow/Center/Image").GetComponent<Image>();
        unit = Left.squad.formation[1, 2];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }

        // 09
        image = GameObject.Find("CombatScreen/PlayerSquad/BackRow/Right/Image").GetComponent<Image>();
        unit = Left.squad.formation[2, 2];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
        }


        // Enemy Unit 

        // 01
        image = GameObject.Find("CombatScreen/EnemySquad/FrontRow/Left/Image").GetComponent<Image>();
        unit = Right.squad.formation[0, 0];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 02
        image = GameObject.Find("CombatScreen/EnemySquad/FrontRow/Center/Image").GetComponent<Image>();
        unit = Right.squad.formation[1, 0];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 03
        image = GameObject.Find("CombatScreen/EnemySquad/FrontRow/Right/Image").GetComponent<Image>();
        unit = Right.squad.formation[2, 0];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 04
        image = GameObject.Find("CombatScreen/EnemySquad/MidleRow/Left/Image").GetComponent<Image>();
        unit = Right.squad.formation[0, 1];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 05
        image = GameObject.Find("CombatScreen/EnemySquad/MidleRow/Center/Image").GetComponent<Image>();
        unit = Right.squad.formation[1, 1];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 06
        image = GameObject.Find("CombatScreen/EnemySquad/MidleRow/Right/Image").GetComponent<Image>();
        unit = Right.squad.formation[2, 1];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 07
        image = GameObject.Find("CombatScreen/EnemySquad/BackRow/Left/Image").GetComponent<Image>();
        unit = Right.squad.formation[0, 2];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 08
        image = GameObject.Find("CombatScreen/EnemySquad/BackRow/Center/Image").GetComponent<Image>();
        unit = Right.squad.formation[1, 2];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        // 09
        image = GameObject.Find("CombatScreen/EnemySquad/BackRow/Right/Image").GetComponent<Image>();
        unit = Right.squad.formation[2, 2];
        if (unit != null)
        {
            image.enabled = true;
            image.sprite = unit.sprite.sprite;
            image.rectTransform.localScale = new Vector3(-1, 1, 1);
        }
        
    }


    // Attack entre deux escouades
    public static void ResolveAttack(Squad attacker, Squad defender)
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
