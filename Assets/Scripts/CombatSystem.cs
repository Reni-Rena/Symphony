using UnityEngine;
using UnityEngine.UI;
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

        Image image;
        GameObject icone;
        Unit unit;

        for (int lin = 0; lin < 3; lin++)
        {
            if (lin == 0) LIN = "/FrontRow";
            if (lin == 1) LIN = "/MidleRow";
            if (lin == 2) LIN = "/BackRow";

            for (int col = 0; col < 3; col++)
            {
                if (col == 0) COL = "/Left";
                if (col == 1) COL = "/Center";
                if (col == 2) COL = "/Right";

                imageL = GameObject.Find($"CombatScreen/PlayerSquad{LIN}{COL}/Image").GetComponent<Image>();
                imageL.enabled = false;
                iconeL = GameObject.Find($"SquadScreen/PlayerSquad{LIN}{COL}");
                iconeLImages = iconeL.GetComponentsInChildren<Image>();
                foreach (var iconeLImage in iconeLImages) iconeLImage.enabled = false;

                unitL = Left.squad.formation[col, lin];
                if (unitL != null)
                {
                    imageL.enabled = true;
                    imageL.sprite = unitL.sprite.sprite;

                    foreach (var iconeLImage in iconeLImages) iconeLImage.enabled = true;
                    iconeL.GetComponentInChildren<Image>().sprite = unitL.sprite.sprite;
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
                    imageR.sprite = unitR.sprite.sprite;
                    imageR.rectTransform.localScale = new Vector3(-1, 1, 1);

                    foreach (var iconeRImage in iconeRImages) iconeRImage.enabled = true;
                    iconeR.GetComponentInChildren<Image>().sprite = unitR.sprite.sprite;
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

    // Trouver une cible en fonction du type de l�attaquant
    private static Unit FindTarget(Unit attacker, Squad defender)
    {
        for (int y = 0; y < 3; y++) // front  arri�re
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
