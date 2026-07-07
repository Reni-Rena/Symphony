using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Panneau d'edition d'escouade affiche par-dessus la carte.
/// Le jeu est en pause (Time.timeScale = 0) pendant toute la session.
///
/// Source des unites disponibles : PlayerRoster.GetAvailableUnits()
/// Les unites ne sont jamais instanciees ou detruites ici.
/// PlaceUnit/RemoveUnit dans Squad mettent a jour unit.isInSquad.
///
/// TROIS MODES INTERNES :
///
///   Neutre        : rien de selectionne.
///                   Clic cellule occupee  -> EnDeplacement.
///                   Clic carte roster     -> EnPlacement.
///
///   EnDeplacement : une unite deja dans la formation est selectionnee pour bouger.
///                   Cellules vertes = destinations valides.
///                   Re-clic sur l'unite   -> retour Neutre.
///                   Clic cellule verte    -> MoveUnit, retour Neutre.
///                   Clic autre unite      -> change de selection.
///                   Bouton Supprimer      -> RemoveUnit (interdit pour le chef
///                                           si d'autres membres sont presents).
///
///   EnPlacement   : une unite du roster est selectionnee pour etre placee.
///                   Cellules vertes = emplacements valides.
///                   Re-clic sur la carte  -> retour Neutre.
///                   Clic cellule verte    -> PlaceUnit, retour Neutre.
///
/// HIERARCHIE UI ATTENDUE :
///
///   SquadEditorPanel   (GameObject racine, couvre tout l'ecran)
///     Overlay          (Image semi-transparente, bloque les clics derriere)
///     Window
///       Header
///         TxtTitre     (TMP_Text : nom du chef)
///         BtnFermer    (Button : icone X, appelle Annuler)
///       Body
///         PanelGauche
///           LeadRow
///             TxtLeadValeurs  (TMP_Text : "18 / 22 pts")
///             SliderLead      (Slider, interactable = false)
///           TxtFrontLabel     (TMP_Text : "Front" ou fleche)
///           GridFormation     (GridLayoutGroup 6 colonnes)
///         PanelDroit
///           TxtRosterLabel    (TMP_Text)
///           RosterScroll      (ScrollRect)
///             RosterContent   (VerticalLayoutGroup)
///           DetailCard
///             TxtDetailNom    (TMP_Text)
///             TxtDetailTier   (TMP_Text)
///             TxtDetailStats  (TMP_Text)
///             TxtDetailCout   (TMP_Text)
///             TxtHint         (TMP_Text : instructions contextuelles)
///             BtnAction       (Button : "Annuler selection")
///             TxtBtnAction    (TMP_Text : label du bouton)
///             BtnSupprimer    (Button : visible seulement en EnDeplacement non-chef)
///       Footer
///         BtnAnnuler   (Button : restaure la formation originale)
///         BtnConfirmer (Button : applique et ferme)
/// </summary>
public class SquadEditorUI : MonoBehaviour
{
    public static SquadEditorUI Instance { get; private set; }

    // -----------------------------------------------------------------------
    // References Inspector
    // -----------------------------------------------------------------------

    [Header("Racine")]
    public GameObject panneauRacine;

    [Header("Header")]
    public TMP_Text txtTitre;
    public Button btnFermer;

    [Header("Lead")]
    public Slider sliderLead;
    public TMP_Text txtLeadValeurs;

    [Header("Grille de formation")]
    public Transform gridFormation;
    public GameObject prefabCellule;

    [Header("Roster")]
    public Transform rosterContent;
    public GameObject prefabCarteUnite;

    [Header("Detail")]
    public TMP_Text txtDetailNom;
    public TMP_Text txtDetailTier;
    public TMP_Text txtDetailStats;
    public TMP_Text txtDetailCout;
    public TMP_Text txtHint;
    public Button btnAction;
    public TMP_Text txtBtnAction;
    public Button btnSupprimer;

    [Header("Footer")]
    public Button btnAnnuler;
    public Button btnConfirmer;

    // -----------------------------------------------------------------------
    // Mode interne
    // -----------------------------------------------------------------------

    private enum Mode { Neutre, EnDeplacement, EnPlacement }
    private Mode modeActuel = Mode.Neutre;

    // -----------------------------------------------------------------------
    // Etat de la session
    // -----------------------------------------------------------------------

    private Squad squadEnEdition;

    // Sauvegarde pour le bouton Annuler.
    // On memorise quelles unites etaient dans l'escouade et a quelle position,
    // sans jamais instancier ni detruire quoi que ce soit.
    private struct SlotSave
    {
        public Unit unit;
        public int col, row;
        public bool estChef;
    }
    private List<SlotSave> sauvegardeSlots = new List<SlotSave>();

    // Cellules UI de la grille
    private List<SquadEditorSlotUI> cellules = new List<SquadEditorSlotUI>();

    // Cartes UI du roster
    private List<SquadEditorUnitCardUI> cartesRoster = new List<SquadEditorUnitCardUI>();

    // Unite selectionnee en mode EnDeplacement (instance dans la formation)
    private Unit uniteEnDeplacement = null;

    // Unite selectionnee en mode EnPlacement (depuis le roster, pas encore placee)
    private Unit uniteEnPlacement = null;

    // -----------------------------------------------------------------------
    // Cycle de vie
    // -----------------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (panneauRacine != null) panneauRacine.SetActive(false);

        btnFermer?.onClick.AddListener(Annuler);
        btnAnnuler?.onClick.AddListener(Annuler);
        btnConfirmer?.onClick.AddListener(Confirmer);
        btnAction?.onClick.AddListener(OnBtnActionClic);
        btnSupprimer?.onClick.AddListener(OnBtnSupprimerClic);
    }

    // -----------------------------------------------------------------------
    // API publique
    // -----------------------------------------------------------------------

    public void Open(Squad squad)
    {
        if (squad == null) return;

        squadEnEdition = squad;
        SauvegarderConfiguration();

        panneauRacine.SetActive(true);

        if (txtTitre != null)
            txtTitre.text = squad.squadLeader != null ? squad.squadLeader.unitName : "Escouade";

        ConstruireGrille();
        ConstruireRoster();
        MettreAJourLeadBar();
        PasserEnModeNeutre();
    }

    // -----------------------------------------------------------------------
    // Construction de la grille
    // -----------------------------------------------------------------------

    void ConstruireGrille()
    {
        foreach (Transform child in gridFormation) Destroy(child.gameObject);
        cellules.Clear();

        int cols = squadEnEdition.gridWidth;
        int rows = squadEnEdition.gridHeight;

        // Parcours de haut (y max) vers bas (y 0) : le front apparait en bas de l'UI.
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < cols; col++)
            {
                GameObject go = Instantiate(prefabCellule, gridFormation);
                SquadEditorSlotUI slot = go.GetComponent<SquadEditorSlotUI>();

                int c = col, r = row;
                slot.Init(col, row, (cc, rr) => OnCelluleCliquee(cc, rr));
                cellules.Add(slot);
            }
        }

        RafraichirAffichageGrille();
    }

    // Met a jour les visuels de toutes les cellules d'apres formation[].
    void RafraichirAffichageGrille()
    {
        foreach (SquadEditorSlotUI slot in cellules)
        {
            Unit u = UniteSurCellule(slot.Col, slot.Row);
            bool estPrincipale = EstCellulePrincipale(u, slot.Col, slot.Row);
            slot.AfficherUnite(u, EstChef(u), estPrincipale);
        }
    }

    // La cellule "principale" d'une unite est la plus en haut a gauche de son empreinte.
    // C'est la seule qui affiche le nom et l'icone.
    bool EstCellulePrincipale(Unit u, int col, int row)
    {
        if (u == null) return false;
        if (!squadEnEdition.occupiedCells.TryGetValue(u, out List<Vector2Int> cells)) return false;

        int minCol = int.MaxValue, maxRow = int.MinValue;
        foreach (Vector2Int c in cells)
        {
            if (c.x < minCol) minCol = c.x;
            if (c.y > maxRow) maxRow = c.y;
        }
        return col == minCol && row == maxRow;
    }

    // -----------------------------------------------------------------------
    // Construction du roster
    // -----------------------------------------------------------------------

    void ConstruireRoster()
    {
        foreach (Transform child in rosterContent) Destroy(child.gameObject);
        cartesRoster.Clear();

        if (PlayerRoster.Instance == null) return;

        // Unites disponibles = non assignees a une escouade
        List<Unit> disponibles = PlayerRoster.Instance.GetAvailableUnits();

        foreach (Unit unite in disponibles)
        {
            bool abordable = EstAbordable(unite);

            GameObject go = Instantiate(prefabCarteUnite, rosterContent);
            SquadEditorUnitCardUI carte = go.GetComponent<SquadEditorUnitCardUI>();

            Unit captured = unite;
            carte.Init(unite, abordable, (u) => OnCarteRosterCliquee(u));
            cartesRoster.Add(carte);
        }
    }

    // -----------------------------------------------------------------------
    // Lead bar
    // -----------------------------------------------------------------------

    void MettreAJourLeadBar()
    {
        int utilises = squadEnEdition.leadPointsUsed;
        int max = squadEnEdition.leadPointsMax;

        if (txtLeadValeurs != null)
            txtLeadValeurs.text = utilises + " / " + max + " pts";

        if (sliderLead != null)
        {
            sliderLead.minValue = 0;
            sliderLead.maxValue = Mathf.Max(1, max);
            sliderLead.value = utilises;
        }
    }

    // -----------------------------------------------------------------------
    // Gestion des modes
    // -----------------------------------------------------------------------

    void PasserEnModeNeutre()
    {
        modeActuel = Mode.Neutre;
        uniteEnDeplacement = null;
        uniteEnPlacement = null;

        DeselectionnnerCartesRoster();
        ResetCouleursGrille();
        MasquerDetail();

        if (txtHint != null) txtHint.text = "";
    }

    void PasserEnModeEnDeplacement(Unit unite)
    {
        modeActuel = Mode.EnDeplacement;
        uniteEnDeplacement = unite;
        uniteEnPlacement = null;

        DeselectionnnerCartesRoster();
        ResetCouleursGrille();
        SurlignCellulesLibresPour(unite.size, excluireUnite: unite);

        // La cellule de l'unite passe en "Selectionne"
        foreach (SquadEditorSlotUI slot in cellules)
            if (slot.UniteSurCellule == unite)
                slot.SetEtat(SquadEditorSlotUI.EtatCellule.Selectionne);

        AfficherDetailUniteEnFormation(unite);

        if (txtHint != null)
            txtHint.text = "Cliquez une cellule verte pour deplacer, "
                         + "ou re-cliquez l'unite pour annuler.";
    }

    void PasserEnModeEnPlacement(Unit unite)
    {
        modeActuel = Mode.EnPlacement;
        uniteEnPlacement = unite;
        uniteEnDeplacement = null;

        ResetCouleursGrille();
        SurlignCellulesLibresPour(unite.size, excluireUnite: null);

        SquadEditorUnitCardUI carte = TrouverCarte(unite);
        DeselectionnnerCartesRoster();
        if (carte != null) carte.SetSelectionne(true);

        AfficherDetailPrefab(unite);

        if (txtHint != null)
            txtHint.text = "Cliquez une cellule verte pour placer l'unite.";
    }

    // -----------------------------------------------------------------------
    // Callbacks cellules
    // -----------------------------------------------------------------------

    void OnCelluleCliquee(int col, int row)
    {
        switch (modeActuel)
        {
            case Mode.Neutre:
                TraiterClicNeutre(col, row);
                break;
            case Mode.EnDeplacement:
                TraiterClicEnDeplacement(col, row);
                break;
            case Mode.EnPlacement:
                TraiterClicEnPlacement(col, row);
                break;
        }
    }

    void TraiterClicNeutre(int col, int row)
    {
        Unit u = UniteSurCellule(col, row);
        if (u != null) PasserEnModeEnDeplacement(u);
    }

    void TraiterClicEnDeplacement(int col, int row)
    {
        Unit u = UniteSurCellule(col, row);

        // Re-clic sur l'unite selectionnee : annuler
        if (u == uniteEnDeplacement)
        {
            PasserEnModeNeutre();
            return;
        }

        // Clic sur une autre unite : changer de selection
        if (u != null)
        {
            PasserEnModeEnDeplacement(u);
            return;
        }

        // Clic sur une cellule vide : tenter le deplacement
        bool ok = squadEnEdition.MoveUnit(uniteEnDeplacement, col, row);
        if (ok)
        {
            RafraichirAffichageGrille();
            MettreAJourLeadBar();
            PasserEnModeNeutre();
        }
        else
        {
            if (txtHint != null)
                txtHint.text = "Impossible de placer ici (trop petit ou deja occupe).";
        }
    }

    void TraiterClicEnPlacement(int col, int row)
    {
        // On ne remplace jamais une unite existante
        if (UniteSurCellule(col, row) != null) return;

        bool estPremiere = (squadEnEdition.squadLeader == null);
        bool ok = squadEnEdition.PlaceUnit(uniteEnPlacement, col, row, isLeader: estPremiere);

        if (!ok)
        {
            if (txtHint != null)
                txtHint.text = "Impossible de placer ici.";
            return;
        }

        if (estPremiere && txtTitre != null)
            txtTitre.text = uniteEnPlacement.unitName;

        RafraichirAffichageGrille();
        ConstruireRoster();        // la liste des disponibles a change
        MettreAJourLeadBar();
        PasserEnModeNeutre();
    }

    // -----------------------------------------------------------------------
    // Callback roster
    // -----------------------------------------------------------------------

    void OnCarteRosterCliquee(Unit unite)
    {
        // Re-clic sur l'unite deja en cours de placement : annuler
        if (modeActuel == Mode.EnPlacement && uniteEnPlacement == unite)
        {
            PasserEnModeNeutre();
            return;
        }

        // Si une unite est en cours de deplacement, annuler ce mode d'abord
        if (modeActuel == Mode.EnDeplacement)
            PasserEnModeNeutre();

        PasserEnModeEnPlacement(unite);
    }

    // -----------------------------------------------------------------------
    // Boutons du detail
    // -----------------------------------------------------------------------

    // Bouton "Annuler selection" : valable dans EnDeplacement et EnPlacement
    void OnBtnActionClic()
    {
        PasserEnModeNeutre();
    }

    // Bouton "Supprimer" : retire l'unite de la formation, la remet dans le roster
    void OnBtnSupprimerClic()
    {
        if (modeActuel != Mode.EnDeplacement || uniteEnDeplacement == null) return;

        bool ok = squadEnEdition.RemoveUnit(uniteEnDeplacement);
        if (!ok)
        {
            if (txtHint != null)
                txtHint.text = "Retirez d'abord tous les autres membres avant de retirer le chef.";
            return;
        }

        RafraichirAffichageGrille();
        ConstruireRoster();
        MettreAJourLeadBar();
        PasserEnModeNeutre();
    }

    // -----------------------------------------------------------------------
    // Detail
    // -----------------------------------------------------------------------

    void MasquerDetail()
    {
        if (txtDetailNom != null) txtDetailNom.text = "Selectionnez une unite";
        if (txtDetailTier != null) txtDetailTier.text = "";
        if (txtDetailStats != null) txtDetailStats.text = "";
        if (txtDetailCout != null) txtDetailCout.text = "";

        if (btnAction != null) btnAction.gameObject.SetActive(false);
        if (btnSupprimer != null) btnSupprimer.gameObject.SetActive(false);
    }

    // Detail pour une unite deja dans la formation.
    void AfficherDetailUniteEnFormation(Unit unite)
    {
        bool estChef = EstChef(unite);

        if (txtDetailNom != null) txtDetailNom.text = unite.unitName;
        if (txtDetailTier != null) txtDetailTier.text = "Tier " + unite.GetTier();
        if (txtDetailCout != null)
            txtDetailCout.text = estChef
                ? "Chef d'escouade (gratuit)"
                : unite.GetCommandCost() + " pts de commandement";
        if (txtDetailStats != null) txtDetailStats.text = BuildStatsText(unite);

        if (btnAction != null)
        {
            btnAction.gameObject.SetActive(true);
            if (txtBtnAction != null) txtBtnAction.text = "Annuler selection";
        }

        // Le bouton Supprimer est cache pour le chef
        if (btnSupprimer != null)
            btnSupprimer.gameObject.SetActive(!estChef);
    }

    // Detail pour une unite du roster (pas encore placee).
    void AfficherDetailPrefab(Unit unite)
    {
        if (txtDetailNom != null) txtDetailNom.text = unite.unitName;
        if (txtDetailTier != null) txtDetailTier.text = "Tier " + unite.GetTier();
        if (txtDetailCout != null) txtDetailCout.text = unite.GetCommandCost() + " pts de commandement";
        if (txtDetailStats != null) txtDetailStats.text = BuildStatsText(unite);

        if (btnAction != null)
        {
            btnAction.gameObject.SetActive(true);
            if (txtBtnAction != null) txtBtnAction.text = "Annuler selection";
        }

        if (btnSupprimer != null) btnSupprimer.gameObject.SetActive(false);
    }

    string BuildStatsText(Unit u)
    {
        return "Taille      : " + u.size.x + " x " + u.size.y + "\n"
             + "Force       : " + u.strength + "\n"
             + "Armure      : " + u.armor + "\n"
             + "Magie       : " + u.magic + "\n"
             + "Deplacement : " + u.GetUnitMoveType().ToString();
    }

    // -----------------------------------------------------------------------
    // Surlignes de grille
    // -----------------------------------------------------------------------

    void ResetCouleursGrille()
    {
        foreach (SquadEditorSlotUI slot in cellules)
        {
            slot.SetEtat(slot.UniteSurCellule != null
                ? SquadEditorSlotUI.EtatCellule.Occupe
                : SquadEditorSlotUI.EtatCellule.Neutre);
        }
    }

    // Surligne en vert les cellules ou "size" peut etre posee.
    // excluireUnite : si non null, ses cellules sont liberees temporairement
    // pour que la verification soit correcte lors d'un deplacement.
    void SurlignCellulesLibresPour(Vector2Int size, Unit excluireUnite)
    {
        // Liberer temporairement les cellules de l'unite a deplacer
        List<Vector2Int> cellulesExclues = null;
        if (excluireUnite != null
            && squadEnEdition.occupiedCells.TryGetValue(excluireUnite, out cellulesExclues))
        {
            foreach (Vector2Int c in cellulesExclues)
                squadEnEdition.formation[c.x, c.y] = null;
        }

        foreach (SquadEditorSlotUI slot in cellules)
        {
            // Cellules de l'unite en deplacement : deja colorees Selectionne, on les ignore
            if (excluireUnite != null && slot.UniteSurCellule == excluireUnite) continue;

            if (slot.UniteSurCellule != null)
            {
                slot.SetEtat(SquadEditorSlotUI.EtatCellule.Occupe);
                continue;
            }

            slot.SetEtat(squadEnEdition.CanPlaceUnit(slot.Col, slot.Row, size)
                ? SquadEditorSlotUI.EtatCellule.Libre
                : SquadEditorSlotUI.EtatCellule.Bloque);
        }

        // Remettre les cellules exclues
        if (excluireUnite != null && cellulesExclues != null)
            foreach (Vector2Int c in cellulesExclues)
                squadEnEdition.formation[c.x, c.y] = excluireUnite;
    }

    // -----------------------------------------------------------------------
    // Utilitaires
    // -----------------------------------------------------------------------

    Unit UniteSurCellule(int col, int row)
    {
        if (col < 0 || row < 0
            || col >= squadEnEdition.gridWidth
            || row >= squadEnEdition.gridHeight) return null;
        return squadEnEdition.formation[col, row];
    }

    bool EstChef(Unit u)
    {
        return u != null && u == squadEnEdition.squadLeader;
    }

    bool EstAbordable(Unit unite)
    {
        if (squadEnEdition.squadLeader == null) return true;
        return squadEnEdition.leadPointsUsed + unite.GetCommandCost()
               <= squadEnEdition.leadPointsMax;
    }

    SquadEditorUnitCardUI TrouverCarte(Unit unite)
    {
        foreach (SquadEditorUnitCardUI c in cartesRoster)
            if (c.Unite == unite) return c;
        return null;
    }

    void DeselectionnnerCartesRoster()
    {
        foreach (SquadEditorUnitCardUI c in cartesRoster)
            c.SetSelectionne(false);
    }

    // -----------------------------------------------------------------------
    // Sauvegarde et restauration (bouton Annuler)
    // -----------------------------------------------------------------------

    void SauvegarderConfiguration()
    {
        sauvegardeSlots.Clear();

        // On sauvegarde les positions depuis occupiedCells en ne retenant
        // que la cellule principale (haut-gauche de l'empreinte).
        foreach (KeyValuePair<Unit, List<Vector2Int>> kvp in squadEnEdition.occupiedCells)
        {
            Unit u = kvp.Key;
            int minCol = int.MaxValue, maxRow = int.MinValue;
            foreach (Vector2Int c in kvp.Value)
            {
                if (c.x < minCol) minCol = c.x;
                if (c.y > maxRow) maxRow = c.y;
            }
            sauvegardeSlots.Add(new SlotSave
            {
                unit = u,
                col = minCol,
                row = maxRow,
                estChef = (u == squadEnEdition.squadLeader)
            });
        }
    }

    void RestaurerConfiguration()
    {
        // Retirer toutes les unites actuelles sans les detruire
        List<Unit> vivantes = new List<Unit>(squadEnEdition.GetLivingUnits());
        foreach (Unit u in vivantes)
        {
            // Liberer les cellules manuellement (RemoveUnit refuserait le chef s'il reste des membres)
            if (squadEnEdition.occupiedCells.TryGetValue(u, out List<Vector2Int> cells))
            {
                foreach (Vector2Int c in cells)
                    squadEnEdition.formation[c.x, c.y] = null;
                squadEnEdition.occupiedCells.Remove(u);
            }
            u.isInSquad = false;
            u.assignedSquad = null;
        }

        // Remettre leadPointsUsed a zero avant de replacer
        // (SetLeaderFromEditor recalcule tout apres)

        // Replacer les unites sauvegardees
        // Le chef en premier pour que leadPointsMax soit correct
        List<SlotSave> chefs = new List<SlotSave>();
        List<SlotSave> membres = new List<SlotSave>();
        foreach (SlotSave s in sauvegardeSlots)
            if (s.estChef) chefs.Add(s); else membres.Add(s);

        foreach (SlotSave s in chefs)
            squadEnEdition.PlaceUnit(s.unit, s.col, s.row, isLeader: true);

        foreach (SlotSave s in membres)
            squadEnEdition.PlaceUnit(s.unit, s.col, s.row, isLeader: false);

        squadEnEdition.UpdatedAttackInfo();
    }

    // -----------------------------------------------------------------------
    // Fermeture
    // -----------------------------------------------------------------------

    void Annuler()
    {
        RestaurerConfiguration();
        Fermer();
    }

    void Confirmer()
    {
        squadEnEdition.UpdatedAttackInfo();
        Fermer();
    }

    void Fermer()
    {
        PasserEnModeNeutre();
        panneauRacine.SetActive(false);
        squadEnEdition = null;
        GameManager.Instance?.FermerEditeurEscouade();
    }
}