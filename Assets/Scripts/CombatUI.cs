using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    public static CombatUI Instance;

    [Header("Références scène")]
    public GameObject combatScreen;
    public GameObject squadScreen;

    private const int GRID_SIZE = 6;
    private static readonly string[] INDEX = { "/1", "/2", "/3", "/4", "/5", "/6" };

    // Pour chaque unité : toutes les Images du combatScreen qui lui appartiennent
    private Dictionary<Unit, List<Image>> unitToImages = new Dictionary<Unit, List<Image>>();
    // Pour chaque unité : les Images de l'icône (squadScreen) - une seule entrée par unité
    private Dictionary<Unit, Image[]> unitToIcImages = new Dictionary<Unit, Image[]>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        combatScreen.SetActive(false);
        squadScreen.SetActive(false);
    }

    public void ShowCombat(Pion left, Pion right)
    {
        unitToImages.Clear();
        unitToIcImages.Clear();

        combatScreen.SetActive(true);
        squadScreen.SetActive(true);
        RefreshSquadDisplay(left, "PlayerSquad", mirrorSprite: false);
        RefreshSquadDisplay(right, "EnemySquad", mirrorSprite: true);
    }

    public void HideCombat()
    {
        foreach (Unit u in unitToImages.Keys)
            u.OnDeath -= OnUnitDied;

        unitToImages.Clear();
        unitToIcImages.Clear();

        combatScreen.SetActive(false);
        squadScreen.SetActive(false);
    }

    private void OnUnitDied(Unit unit)
    {
        // Cache tous les sprites de cette unité (les 4 cases du 2×2)
        if (unitToImages.TryGetValue(unit, out List<Image> imgs))
            foreach (var img in imgs) img.enabled = false;

        // Cache l'icône et la barre de vie
        if (unitToIcImages.TryGetValue(unit, out Image[] icImages))
            foreach (var ic in icImages) ic.enabled = false;
    }

    private void RefreshSquadDisplay(Pion pion, string squadName, bool mirrorSprite)
    {
        // Unités déjà traitées (pour dédupliquer les unités 2×2+)
        HashSet<Unit> processedUnits = new HashSet<Unit>();

        // D'abord, désactive tout
        for (int lin = 0; lin < GRID_SIZE; lin++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                string slot = INDEX[lin] + INDEX[col];

                Transform imageT = combatScreen.transform.Find($"{squadName}{slot}/Image");
                if (imageT != null) imageT.GetComponent<Image>().enabled = false;

                Transform iconeT = squadScreen.transform.Find($"{squadName}{slot}");
                if (iconeT != null)
                    foreach (var img in iconeT.GetComponentsInChildren<Image>())
                        img.enabled = false;
            }
        }

        // Ensuite, affiche chaque unité unique
        for (int lin = 0; lin < GRID_SIZE; lin++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                Unit unit = pion.squad.formation[col, lin];
                if (unit == null || unit.IsDead) continue;

                string slot = INDEX[lin] + INDEX[col];

                //  Grand sprite (une image par case occupée) 
                Transform imageT = combatScreen.transform.Find($"{squadName}{slot}/Image");
                if (imageT != null)
                {
                    Image img = imageT.GetComponent<Image>();
                    img.enabled = true;
                    img.sprite = unit.imageSprite;
                    img.rectTransform.localScale = mirrorSprite ? new Vector3(-1, 1, 1) : Vector3.one;

                    // Enregistre toutes les images de cette unité (les 4 cases du 2×2)
                    if (!unitToImages.ContainsKey(unit))
                        unitToImages[unit] = new List<Image>();
                    unitToImages[unit].Add(img);
                }

                //  Icône + barre de vie : une seule fois par unité 
                if (!processedUnits.Contains(unit))
                {
                    processedUnits.Add(unit);

                    Transform iconeT = squadScreen.transform.Find($"{squadName}{slot}");
                    if (iconeT != null)
                    {
                        Image[] iconeImages = iconeT.GetComponentsInChildren<Image>();
                        foreach (var ic in iconeImages) ic.enabled = true;

                        Image iconeImg = iconeT.GetComponentInChildren<Image>();
                        if (iconeImg != null) iconeImg.sprite = unit.iconeSprite;

                        HealthBar hb = iconeT.GetComponentInChildren<HealthBar>();
                        if (hb != null) hb.unit = unit;

                        unitToIcImages[unit] = iconeImages;
                    }

                    // Abonne l'event de mort (une seule fois)
                    unit.OnDeath += OnUnitDied;
                }
            }
        }
    }
}