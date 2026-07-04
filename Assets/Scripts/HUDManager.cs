using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Couleurs tour")]
    public GamePalette palette;

    [Header("Top Bar")]
    public Image topBar;
    public Image turnDot;
    public TextMeshProUGUI turnLabel;
    public TextMeshProUGUI turnNumber;
    public GameObject endTurnButton;

    [Header("Side Panel")]
    public GameObject sidePanel;
    public Image portraitIcon;
    public TextMeshProUGUI squadName;
    public TextMeshProUGUI squadType;
    public TextMeshProUGUI statPortee;
    public TextMeshProUGUI statDeplacement;
    public Transform movePipsContainer;

    [Header("Pip prefabs")]
    public GameObject unitPipPrefab;   // petit carré bleu (unité vivante)
    public GameObject unitPipDeadPrefab; // petit carré grisé (unité morte)
    public GameObject movePipPrefab;   // petit carré or (déplacement dispo)
    public GameObject movePipUsedPrefab; // petit carré grisé (déplacement utilisé)

    [Header("Bottom Bar")]
    public Image bottomBar;
    public TextMeshProUGUI terrainLabelTitle;
    public TextMeshProUGUI terrainLabel;
    public TextMeshProUGUI terrainDefTitle;
    public TextMeshProUGUI terrainDef;
    public TextMeshProUGUI positionLabelTitle;
    public TextMeshProUGUI positionLabel;

    private int turnCount = 1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        endTurnButton.GetComponent<Button>().onClick.AddListener(() => FindAnyObjectByType<GameManager>().EndPlayerTurnButton());
        HidePanel();
        InitializedColor();
        UpdateTopBar(GameManager.Turn.Player);
        UpdateBottomBar("plaine", 6, new Vector2(3, 3));
    }

    public void InitializedColor()
    {
        topBar.color = palette.encreNoire02; 
        turnNumber.color = palette.encreNoire03;
        endTurnButton.GetComponent<Image>().color = palette.encreNoire02;
        endTurnButton.GetComponentInChildren<TextMeshProUGUI>().color = palette.parchemin;

        bottomBar.color = palette.encreNoire02;
        terrainLabelTitle.color = palette.encreNoire03;
        terrainLabel.color = palette.parchemin;
        terrainDefTitle.color = palette.encreNoire03;
        terrainDef.color = palette.parchemin;
        positionLabelTitle.color = palette.encreNoire03;
        positionLabel.color = palette.parchemin;
    }

    // TOP BAR

    public void UpdateTopBar(GameManager.Turn turn)
    {
        bool isPlayer = turn == GameManager.Turn.Player;

        turnDot.color = isPlayer ? palette.orBrule : palette.bordeaux03;
        turnLabel.text = isPlayer ? "TOUR JOUEUR" : "TOUR ENNEMI";
        turnLabel.color = isPlayer ? palette.orBrule : palette.bordeaux03;
        turnNumber.text = $"- Tour {turnCount}";

        // Bouton fin de tour uniquement actif pendant le tour joueur
        endTurnButton.GetComponent<Button>().interactable = isPlayer;

        if (!isPlayer) turnCount++;
    }

    // PANNEAU LATÉRAL

    public void ShowPionInfo(Pion pion)
    {
        sidePanel.SetActive(true);

        Squad squad = pion.squad;
        if (squad == null) return;

        // Portrait
        SpriteRenderer sr = pion.GetComponent<SpriteRenderer>();
        if (sr != null && portraitIcon != null)
            portraitIcon.sprite = sr.sprite;

        // Nom & type
        squadName.text = pion.gameObject.name;
        squadType.text = squad.name;

        // Stats (moyennes de l'escouade)
        statPortee.text = "1"; // à brancher si tu ajoutes une portée sur Squad
        statDeplacement.text = pion.moveRange.ToString();

        // Pips déplacement
        int movesLeft = pion.hasActed ? 0 : pion.moveRange;
        RefreshPips(movePipsContainer, movesLeft, pion.moveRange, movePipPrefab, movePipUsedPrefab);
    }

    public void HidePanel()
    {
        if (sidePanel != null) sidePanel.SetActive(false);
    }

    private void RefreshPips(Transform container, int active, int total,
                              GameObject activePrefab, GameObject inactivePrefab)
    {
        if (container == null) return;

        // Vide les pips existants
        foreach (Transform child in container)
            Destroy(child.gameObject);

        for (int i = 0; i < total; i++)
        {
            GameObject prefab = i < active ? activePrefab : inactivePrefab;
            if (prefab != null) Instantiate(prefab, container);
        }
    }

    // BOTTOM BAR

    public void UpdateBottomBar(string terrain, int defense, Vector2 position)
    {
        if (terrainLabel != null) terrainLabel.text = terrain;
        if (terrainDef != null) terrainDef.text = defense >= 0 ? $"+{defense}" : $"{defense}";
        if (positionLabel != null) positionLabel.text = $"{(int)position.x}, {(int)position.y}";
    }
}