using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Composant d'une cellule de la grille de formation dans l'editeur d'escouade.
///
/// Setup dans l'inspecteur :
///   GameObject cellule
///     Image (fond de la cellule)               -> referencee dans bgImage
///     Image "UnitIcon" (icone de l'unite)      -> referencee dans unitIcon
///     TMP_Text "UnitName"                      -> referencee dans txtUnitName
///     Image "LeaderCrown" (couronne chef)      -> referencee dans leaderCrown
///     Button (sur le GameObject racine)        -> referencee dans cellButton
/// </summary>
public class SquadEditorSlotUI : MonoBehaviour
{
    // Etat visuel de la cellule
    public enum EtatCellule
    {
        Neutre,      // vide, rien de selectionne
        Libre,       // vide, peut accueillir l'unite selectionnee
        Bloque,      // vide, ne peut pas accueillir (hors grille ou trop petit)
        Occupe,      // occupe par une unite non selectionnee
        Selectionne  // occupe par l'unite en cours de deplacement
    }

    [Header("References UI")]
    public Image bgImage;
    public Image unitIcon;
    public TMP_Text txtUnitName;
    public Image leaderCrown;
    public Button cellButton;

    [Header("Couleurs d'etat")]
    public Color couleurNeutre = new Color(0.15f, 0.12f, 0.08f, 1f);
    public Color couleurLibre = new Color(0.20f, 0.45f, 0.20f, 1f);
    public Color couleurBloque = new Color(0.40f, 0.15f, 0.10f, 1f);
    public Color couleurOccupe = new Color(0.18f, 0.22f, 0.38f, 1f);
    public Color couleurSelectionne = new Color(0.70f, 0.55f, 0.10f, 1f);

    // Position dans la grille logique
    public int Col { get; private set; }
    public int Row { get; private set; }

    // Unite occupant cette cellule (null si vide)
    public Unit UniteSurCellule { get; private set; }

    private Action<int, int> onClique;

    // -----------------------------------------------------------------------
    // Initialisation
    // -----------------------------------------------------------------------

    public void Init(int col, int row, Action<int, int> callbackClic)
    {
        Col = col;
        Row = row;
        onClique = callbackClic;

        if (cellButton != null)
            cellButton.onClick.AddListener(OnClic);
    }

    // -----------------------------------------------------------------------
    // Affichage de l'unite
    // -----------------------------------------------------------------------

    // Affiche (ou efface) l'unite sur cette cellule.
    // estChef = true : affiche la couronne.
    // estCellulePrincipale = true : affiche le nom (la cellule "top-left" de l'empreinte).
    public void AfficherUnite(Unit unite, bool estChef, bool estCellulePrincipale)
    {
        UniteSurCellule = unite;

        bool aUnite = unite != null;

        if (unitIcon != null) unitIcon.gameObject.SetActive(aUnite && estCellulePrincipale);
        if (txtUnitName != null)
        {
            txtUnitName.gameObject.SetActive(aUnite && estCellulePrincipale);
            if (aUnite && estCellulePrincipale)
                txtUnitName.text = unite.unitName;
        }
        if (leaderCrown != null)
            leaderCrown.gameObject.SetActive(aUnite && estChef && estCellulePrincipale);

        // Couleur de fond selon presence d'une unite
        SetEtat(aUnite ? EtatCellule.Occupe : EtatCellule.Neutre);
    }

    // -----------------------------------------------------------------------
    // Etat visuel
    // -----------------------------------------------------------------------

    public void SetEtat(EtatCellule etat)
    {
        if (bgImage == null) return;

        switch (etat)
        {
            case EtatCellule.Neutre: bgImage.color = couleurNeutre; break;
            case EtatCellule.Libre: bgImage.color = couleurLibre; break;
            case EtatCellule.Bloque: bgImage.color = couleurBloque; break;
            case EtatCellule.Occupe: bgImage.color = couleurOccupe; break;
            case EtatCellule.Selectionne: bgImage.color = couleurSelectionne; break;
        }
    }

    // -----------------------------------------------------------------------
    // Callback
    // -----------------------------------------------------------------------

    void OnClic()
    {
        onClique?.Invoke(Col, Row);
    }
}