using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Machine d'états centralisée pour le déplacement et le combat.
///
/// États :
///   IDLE        : aucun pion sélectionné
///   SELECTED    : pion sélectionné, cases de déplacement affichées
///   MENU        : menu ouvert (après déplacement ou clic sur soi-même)
///   ATTACKING   : cases d'attaque affichées
///   BUSY        : combat/animation en cours, aucune entrée acceptée
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //  Tour 
    public enum Turn { Player, Enemy }
    public Turn currentTurn = Turn.Player;

    //  FSM 
    public enum Phase { Idle, Selected, Menu, Attacking, Busy }
    public Phase currentPhase = Phase.Idle;

    // Pion actuellement sélectionné
    public Pion selectedPion = null;

    // Position du pion avant son déplacement (pour annulation)
    private Vector3 positionAvantDeplacement;

    // A-t-il été déplacé depuis la sélection ?
    private bool aEteDeplace = false;

    //  Singleton 
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            p.ResetAction();
        HUDManager.Instance?.UpdateTopBar(currentTurn);
    }

    //  Input principal 
    void Update()
    {
        // Aucune entrée pendant une animation / combat
        if (currentPhase == Phase.Busy) return;

        // Clic droit : annulation dans tous les états
        if (Input.GetMouseButtonDown(1))
        {
            AnnulerEtDeselectionner();
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;

        // Ignorer si clic sur un élément UI (boutons du menu)
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D uiHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity,
                                                 LayerMask.GetMask("UI"));
        if (uiHit.collider != null) return;

        // Cherche d'abord un pion, puis une tile
        RaycastHit2D pionHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity,
                                                  LayerMask.GetMask("Pion"));
        RaycastHit2D tileHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity,
                                                  LayerMask.GetMask("Tile"));

        Pion pionClique = pionHit.collider != null ? pionHit.collider.GetComponent<Pion>() : null;
        Tile tileCliquee = tileHit.collider != null ? tileHit.collider.GetComponent<Tile>() : null;

        TraiterClic(pionClique, tileCliquee);
    }

    //  Dispatch selon la phase 
    void TraiterClic(Pion pionClique, Tile tileCliquee)
    {
        switch (currentPhase)
        {
            case Phase.Idle:
                TraiterClic_Idle(pionClique);
                break;

            case Phase.Selected:
                TraiterClic_Selected(pionClique, tileCliquee);
                break;

            case Phase.Menu:
                // Le menu est géré par ses propres boutons ; clic sur plateau ignoré
                break;

            case Phase.Attacking:
                TraiterClic_Attacking(pionClique, tileCliquee);
                break;
        }
    }

    //  IDLE 
    void TraiterClic_Idle(Pion pionClique)
    {
        if (pionClique == null) return;

        // Seulement les pions alliés non utilisés
        if (currentTurn == Turn.Player && !pionClique.isEnemy && !pionClique.hasActed)
            SelectionnerPion(pionClique);
    }

    //  SELECTED 
    void TraiterClic_Selected(Pion pionClique, Tile tileCliquee)
    {
        // Clic sur le pion sélectionné lui-même : ouvrir le menu
        if (pionClique != null && pionClique == selectedPion)
        {
            OuvrirMenu();
            return;
        }

        // Clic sur un autre pion (allié ou ennemi) : rien
        if (pionClique != null) return;

        // Clic sur une case de déplacement
        if (tileCliquee != null && tileCliquee.EstCaseDeplacement())
        {
            // Vérifier que la case n'est pas occupée
            if (!CaseOccupee(tileCliquee.transform.position))
                DeplacerPion(tileCliquee.transform.position);
        }
    }

    //  ATTACKING 
    void TraiterClic_Attacking(Pion pionClique, Tile tileCliquee)
    {
        // Clic sur une case d'attaque sans ennemi : rien
        // Clic sur un ennemi dans une case d'attaque : combat
        if (pionClique != null && pionClique.isEnemy && tileCliquee != null && tileCliquee.EstCaseAttaque())
        {
            LancerCombat(selectedPion, pionClique);
            return;
        }

        // Clic sur case d'attaque vide : rien
        if (tileCliquee != null && tileCliquee.EstCaseAttaque() && pionClique == null) return;

        // Clic ailleurs : rien (le clic droit annule)
    }

    //  Actions 

    void SelectionnerPion(Pion pion)
    {
        // Nettoyer l'ancienne sélection
        if (selectedPion != null) selectedPion.SetSelected(false);

        selectedPion = pion;
        positionAvantDeplacement = pion.transform.position;
        aEteDeplace = false;

        pion.SetSelected(true);
        Tile.HighlightTiles(pion);

        currentPhase = Phase.Selected;
        HUDManager.Instance?.ShowPionInfo(pion);
    }

    void OuvrirMenu()
    {
        currentPhase = Phase.Menu;
        Tile.ClearHighlights();

        ActionMenuUI.Instance?.ShowAt(
            selectedPion.transform.position,
            onCancel: () => AnnulerEtDeselectionner(),
            onValidate: () => ValiderAction(),
            onAttack: () => AfficherCasesAttaque(),
            showValidate: aEteDeplace   // "Valider" seulement si le pion a bougé
        );
    }

    void DeplacerPion(Vector3 destination)
    {
        currentPhase = Phase.Busy;
        Tile.ClearHighlights();

        StartCoroutine(DeplacerEtOuvrirMenu(destination));
    }

    IEnumerator DeplacerEtOuvrirMenu(Vector3 destination)
    {
        yield return StartCoroutine(selectedPion.MovePathCoroutine(destination));
        aEteDeplace = true;
        currentPhase = Phase.Menu;

        ActionMenuUI.Instance?.ShowAt(
            selectedPion.transform.position,
            onCancel: () => AnnulerEtDeselectionner(),
            onValidate: () => ValiderAction(),
            onAttack: () => AfficherCasesAttaque(),
            showValidate: true
        );
    }

    void AfficherCasesAttaque()
    {
        currentPhase = Phase.Attacking;
        Tile.ClearHighlights();
        Tile.HighlightAttackableTiles(selectedPion);
    }

    void ValiderAction()
    {
        selectedPion.hasActed = true;
        Deselectionner();
        VerifierFinDeTour();
    }

    public void AnnulerEtDeselectionner()
    {
        if (selectedPion == null) { Deselectionner(); return; }

        // Remettre à la position d'origine si le pion avait bougé
        if (aEteDeplace)
            selectedPion.transform.position = positionAvantDeplacement;

        Deselectionner();
    }

    void Deselectionner()
    {
        if (selectedPion != null)
        {
            selectedPion.SetSelected(false);
            selectedPion = null;
        }

        aEteDeplace = false;
        currentPhase = Phase.Idle;
        Tile.ClearHighlights();
        ActionMenuUI.Instance?.Hide();
        HUDManager.Instance?.HidePanel();
    }

    void LancerCombat(Pion attaquant, Pion defenseur)
    {
        currentPhase = Phase.Busy;
        Tile.ClearHighlights();
        ActionMenuUI.Instance?.Hide();

        StartCoroutine(CombatEtSuite(attaquant, defenseur));
    }

    IEnumerator CombatEtSuite(Pion attaquant, Pion defenseur)
    {
        yield return StartCoroutine(CombatSystem.ResolveCombatCoroutine(attaquant, defenseur));

        if (attaquant != null) attaquant.hasActed = true;
        Deselectionner();
        CheckGameOver();
        VerifierFinDeTour();
    }

    //  Fin de tour 

    void VerifierFinDeTour()
    {
        bool tousOntAgi = true;
        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (!p.isEnemy && !p.hasActed) { tousOntAgi = false; break; }
        }
        if (tousOntAgi) EndPlayerTurn();
    }

    public void EndPlayerTurnButton()
    {
        if (currentPhase != Phase.Idle && currentPhase != Phase.Selected)
            AnnulerEtDeselectionner();

        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            if (!p.isEnemy) p.hasActed = true;

        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        if (CheckGameOver()) return;

        currentTurn = Turn.Enemy;
        currentPhase = Phase.Busy;
        HUDManager.Instance?.UpdateTopBar(currentTurn);
        StartCoroutine(EnemyTurn());
    }

    //  Tour ennemi (inchangé dans la logique, corrigé pour Busy) 
    private IEnumerator EnemyTurn()
    {
        foreach (Pion enemy in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (enemy == null || !enemy.isEnemy) continue;
            yield return StartCoroutine(EnemyAct(enemy));
        }

        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            p.ResetAction();

        currentTurn = Turn.Player;
        currentPhase = Phase.Idle;
        HUDManager.Instance?.UpdateTopBar(currentTurn);
        CheckGameOver();
    }

    private IEnumerator EnemyAct(Pion enemy)
    {
        Pion closest = GetClosestPlayerPion(enemy);
        if (closest == null) yield break;

        Vector2 currentPos = enemy.GetGridPosition();
        Vector2 targetPos = closest.GetGridPosition();
        int movesLeft = enemy.moveRange;

        while (movesLeft > 0 && currentPos != targetPos)
        {
            if (Vector2.Distance(currentPos, targetPos) <= 1f)
            {
                yield return StartCoroutine(CombatSystem.ResolveCombatCoroutine(enemy, closest));
                enemy.hasActed = true;
                yield break;
            }

            Vector2 nextPos = currentPos;
            Vector2 direction = targetPos - currentPos;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
                nextPos.x += Mathf.Sign(direction.x);
            else
                nextPos.y += Mathf.Sign(direction.y);

            bool occupied = false;
            foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            {
                if (u != enemy && (Vector2)u.transform.position == nextPos)
                {
                    occupied = true;
                    if (!u.isEnemy)
                    {
                        yield return StartCoroutine(CombatSystem.ResolveCombatCoroutine(enemy, u));
                        enemy.hasActed = true;
                        yield break;
                    }
                    break;
                }
            }

            if (!occupied)
            {
                yield return StartCoroutine(enemy.MovePathCoroutine(nextPos));
                currentPos = nextPos;

                Pion adjacent = GetAdjacentPlayerPion(enemy);
                if (adjacent != null)
                {
                    yield return StartCoroutine(CombatSystem.ResolveCombatCoroutine(enemy, adjacent));
                    enemy.hasActed = true;
                    yield break;
                }
            }
            else break;

            movesLeft--;
        }

        enemy.hasActed = true;
    }

    //  Utilitaires 

    public bool CheckGameOver()
    {
        bool playerAlive = false, enemyAlive = false;
        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (p.isEnemy) enemyAlive = true;
            else playerAlive = true;
        }
        if (!playerAlive) { GameOverUI.Instance?.ShowDefeat(); return true; }
        if (!enemyAlive) { GameOverUI.Instance?.ShowVictory(); return true; }
        return false;
    }

    bool CaseOccupee(Vector3 pos)
    {
        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            if ((Vector2)p.transform.position == (Vector2)pos) return true;
        return false;
    }

    Pion GetClosestPlayerPion(Pion enemy)
    {
        Pion closest = null; float minDist = Mathf.Infinity;
        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (p.isEnemy) continue;
            float d = Vector2.Distance(p.GetGridPosition(), enemy.GetGridPosition());
            if (d < minDist) { minDist = d; closest = p; }
        }
        return closest;
    }

    Pion GetAdjacentPlayerPion(Pion enemy)
    {
        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            if (!p.isEnemy && Vector2.Distance(p.GetGridPosition(), enemy.GetGridPosition()) <= 1f)
                return p;
        return null;
    }
}