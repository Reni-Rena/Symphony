using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Menu contextuel (Attaquer / Valider / Annuler).
/// - N'appelle plus Hide() lui-même depuis les boutons : c'est GameManager qui gère l'état.
/// - Expose showValidate pour n'afficher "Valider" que si le pion a bougé.
/// - Le clic droit est géré dans GameManager (Update), pas ici.
/// </summary>
public class ActionMenuUI : MonoBehaviour
{
    public static ActionMenuUI Instance;

    public GameObject menuPanel;
    public Button cancelButton;
    public Button validateButton;
    public Button attackButton;

    private Canvas canvas;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    public bool IsOpen() => menuPanel.activeSelf;

    /// <summary>
    /// Affiche le menu à côté du pion.
    /// showValidate : vrai si le pion vient de bouger (on affiche "Valider").
    /// </summary>
    public void ShowAt(Vector3 worldPos,
                       Action onCancel,
                       Action onValidate,
                       Action onAttack,
                       bool showValidate)
    {
        // Positionner le panel
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out localPos);

        localPos += new Vector2(60f, 0f);
        panelRect.localPosition = localPos;

        // Activer/désactiver le bouton Valider selon le contexte
        validateButton.gameObject.SetActive(showValidate);

        // Abonner les boutons (on retire les anciens listeners d'abord)
        cancelButton.onClick.RemoveAllListeners();
        validateButton.onClick.RemoveAllListeners();
        attackButton.onClick.RemoveAllListeners();

        cancelButton.onClick.AddListener(() =>
        {
            Hide();
            onCancel?.Invoke();
        });

        validateButton.onClick.AddListener(() =>
        {
            Hide();
            onValidate?.Invoke();
        });

        attackButton.onClick.AddListener(() =>
        {
            Hide();
            onAttack?.Invoke();
        });

        menuPanel.SetActive(true);
    }

    public void Hide() => menuPanel.SetActive(false);
}