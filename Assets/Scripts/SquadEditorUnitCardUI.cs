using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Composant d'une carte d'unite dans le roster de l'editeur d'escouade.
///
/// Setup dans l'inspecteur :
///   GameObject carte
///     Image (fond de la carte)           -> referencee dans bgImage
///     TMP_Text "NomUnite"                -> referencee dans txtNom
///     TMP_Text "TierType"                -> referencee dans txtTierType
///     TMP_Text "Cout"                    -> referencee dans txtCout
///     Image "IndispoOverlay" (rouge semi-transparent, masque si abordable)
///                                        -> referencee dans overlayIndispo
///     Button (sur le GameObject racine)  -> referencee dans cardButton
/// </summary>
public class SquadEditorUnitCardUI : MonoBehaviour
{
    [Header("References UI")]
    public Image bgImage;
    public TMP_Text txtNom;
    public TMP_Text txtTierType;
    public TMP_Text txtCout;
    public Image overlayIndispo;
    public Button cardButton;

    [Header("Couleurs")]
    public Color couleurNormale = new Color(0.18f, 0.22f, 0.38f, 1f);
    public Color couleurSelectionne = new Color(0.70f, 0.55f, 0.10f, 1f);
    public Color couleurIndispo = new Color(0.25f, 0.10f, 0.10f, 1f);

    // Prefab ou ScriptableObject representant cette unite
    public Unit Unite { get; private set; }

    private bool estAbordable;
    private Action<Unit> onClique;

    // -----------------------------------------------------------------------
    // Initialisation
    // -----------------------------------------------------------------------

    // prefabUnite   : le prefab de l'unite representee par cette carte
    // abordable     : l'unite peut etre ajoutee (points de commandement suffisants)
    // callbackClic  : appele avec le prefab quand la carte est cliquee
    public void Init(Unit prefabUnite, bool abordable, Action<Unit> callbackClic)
    {
        Unite = prefabUnite;
        estAbordable = abordable;
        onClique = callbackClic;

        if (txtNom != null)
            txtNom.text = prefabUnite.unitName;

        if (txtTierType != null)
            txtTierType.text = "Tier " + prefabUnite.GetTier()
                             + " - " + prefabUnite.GetUnitType().ToString();

        if (txtCout != null)
            txtCout.text = prefabUnite.GetCommandCost() + " pts";

        // Overlay d'indisponibilite
        if (overlayIndispo != null)
            overlayIndispo.gameObject.SetActive(!abordable);

        // La carte est cliquable meme si indisponible (pour voir les stats),
        // mais le bouton "Placer" dans le detail sera desactive.
        if (cardButton != null)
            cardButton.onClick.AddListener(OnClic);

        // Couleur de fond initiale
        if (bgImage != null)
            bgImage.color = abordable ? couleurNormale : couleurIndispo;
    }

    // -----------------------------------------------------------------------
    // Selection visuelle
    // -----------------------------------------------------------------------

    public void SetSelectionne(bool selectionne)
    {
        if (bgImage == null) return;

        if (selectionne)
            bgImage.color = couleurSelectionne;
        else
            bgImage.color = estAbordable ? couleurNormale : couleurIndispo;
    }

    // -----------------------------------------------------------------------
    // Callback
    // -----------------------------------------------------------------------

    void OnClic()
    {
        onClique?.Invoke(Unite);
    }
}