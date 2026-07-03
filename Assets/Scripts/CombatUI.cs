using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// À attacher sur le GameObject "CombatScreen" dans la scène.
// Assigne combatScreen et squadScreen dans l'inspecteur.
public class CombatUI : MonoBehaviour
{
    public static CombatUI Instance;

    [Header("Références scène")]
    public GameObject combatScreen;
    public GameObject squadScreen;

    private const int GRID_SIZE = 6;
    private static readonly string[] INDEX = { "/1", "/2", "/3", "/4", "/5", "/6" };

    // Garde en mémoire les slots d'affichage pour chaque unité vivante
    // afin de pouvoir les cacher rapidement à la mort
    private Dictionary<Unit, Image> unitToImage = new Dictionary<Unit, Image>();
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
        unitToImage.Clear();
        unitToIcImages.Clear();

        combatScreen.SetActive(true);
        squadScreen.SetActive(true);
        RefreshSquadDisplay(left, "PlayerSquad", mirrorSprite: false);
        RefreshSquadDisplay(right, "EnemySquad", mirrorSprite: true);
    }

    public void HideCombat()
    {
        // Désabonne tous les events avant de fermer (évite les fuites)
        foreach (Unit u in unitToImage.Keys)
            u.OnDeath -= OnUnitDied;

        unitToImage.Clear();
        unitToIcImages.Clear();

        combatScreen.SetActive(false);
        squadScreen.SetActive(false);
    }

    // Appelé automatiquement quand une unité meurt (via l'event OnDeath)
    private void OnUnitDied(Unit unit)
    {
        if (unitToImage.TryGetValue(unit, out Image img))
            img.enabled = false;

        if (unitToIcImages.TryGetValue(unit, out Image[] icImages))
            foreach (var ic in icImages) ic.enabled = false;
    }

    private void RefreshSquadDisplay(Pion pion, string squadName, bool mirrorSprite)
    {
        for (int lin = 0; lin < GRID_SIZE; lin++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                string slot = INDEX[lin] + INDEX[col];

                Transform imageT = combatScreen.transform.Find($"{squadName}{slot}/Image");
                Image image = imageT != null ? imageT.GetComponent<Image>() : null;

                Transform iconeT = squadScreen.transform.Find($"{squadName}{slot}");
                Image[] iconeImages = iconeT != null ? iconeT.GetComponentsInChildren<Image>() : null;

                // Désactive tout par défaut
                if (image != null) image.enabled = false;
                if (iconeImages != null) foreach (var img in iconeImages) img.enabled = false;

                Unit unit = pion.squad.formation[col, lin];
                if (unit == null || unit.IsDead) continue;

                // Grand sprite
                if (image != null)
                {
                    image.enabled = true;
                    image.sprite = unit.imageSprite;
                    image.rectTransform.localScale = mirrorSprite ? new Vector3(-1, 1, 1) : Vector3.one;

                    // Enregistre le slot pour réagir à la mort
                    unitToImage[unit] = image;
                }

                // Icône + HealthBar
                if (iconeT != null && iconeImages != null)
                {
                    foreach (var img in iconeImages) img.enabled = true;
                    iconeT.GetComponentInChildren<Image>().sprite = unit.iconeSprite;

                    HealthBar hb = iconeT.GetComponentInChildren<HealthBar>();
                    if (hb != null) hb.unit = unit;

                    unitToIcImages[unit] = iconeImages;
                }

                // Abonne l'event de mort
                unit.OnDeath += OnUnitDied;
            }
        }
    }
}