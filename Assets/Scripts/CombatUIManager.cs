using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.Collections.Generic;

public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance;

    public GameObject combatPanel;
    public Transform leftContainer;
    public Transform rightContainer;

    public GameObject unitIconPrefab; // petit visuel pour représenter chaque unité (Image + HP bar)

    void Awake()
    {
        Instance = this;
        combatPanel.SetActive(false);
    }

    public void ShowCombat(Pion attacker, Pion defender)
    {
        combatPanel.SetActive(true);

        // Nettoie les anciens visuels
        foreach (Transform child in leftContainer) Destroy(child.gameObject);
        foreach (Transform child in rightContainer) Destroy(child.gameObject);

        // Génère les unités du camp attaquant
        foreach (Unit u in attacker.squad.GetLivingUnits())
        {
            CreateUnitIcon(u, leftContainer, false);
        }

        // Génère les unités du camp défenseur (miroir)
        foreach (Unit u in defender.squad.GetLivingUnits())
        {
            CreateUnitIcon(u, rightContainer, true);
        }

        // Lancer le combat visuel
        StartCoroutine(PlayCombat(attacker, defender));
    }

    private void CreateUnitIcon(Unit unit, Transform parent, bool flip)
    {
        GameObject icon = Instantiate(unitIconPrefab, parent);
        Image image = icon.GetComponentInChildren<Image>();
        image.sprite = unit.GetComponent<SpriteRenderer>().sprite;
        if (flip) image.rectTransform.localScale = new Vector3(-1, 1, 1); // orienter face à face
    }

    private IEnumerator PlayCombat(Pion attacker, Pion defender)
    {
        // Appel du système de combat logique
        CombatSystem.ResolveCombat(attacker, defender);
        yield return new WaitForSeconds(0.5f);

        if (defender.squad.IsAlive())
        {
            CombatSystem.ResolveCombat(defender, attacker);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);

        // Fermer la fenêtre
        combatPanel.SetActive(false);
    }
}
