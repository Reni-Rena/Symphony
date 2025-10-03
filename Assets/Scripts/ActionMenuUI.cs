using UnityEngine;
using UnityEngine.UI;
using System;

public class ActionMenuUI : MonoBehaviour
{
    public GameObject menuPanel;
    public Button cancelButton;
    public Button validateButton;
    public Button attackButton;

    private Action onCancel;
    private Action onValidate;
    private Action onAttack;

    public void Show(Action cancel, Action validate, Action attack)
    {
        onCancel = cancel;
        onValidate = validate;
        onAttack = attack;
        menuPanel.SetActive(true);
    }

    public void Hide()
    {
        menuPanel.SetActive(false);
    }

    void Awake()
    {
        cancelButton.onClick.AddListener(() => { onCancel?.Invoke(); Hide(); });
        validateButton.onClick.AddListener(() => { onValidate?.Invoke(); Hide(); });
        attackButton.onClick.AddListener(() => { onAttack?.Invoke(); Hide(); });
        Hide();
    }
}