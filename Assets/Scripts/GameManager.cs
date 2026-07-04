using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum Turn { Player, Enemy }
    public Turn currentTurn = Turn.Player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            u.ResetAction();

        HUDManager.Instance?.UpdateTopBar(currentTurn);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool actionClick = false;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("UI"));
            if (hit.collider != null) actionClick = true;

            if (!actionClick)
            {
                hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Tile"));
                if (hit.collider != null)
                {
                    Tile tile = hit.collider.GetComponent<Tile>();
                    if (tile != null) tile.OnClicked();
                }

                hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Pion"));
                if (hit.collider != null)
                {
                    Pion pion = hit.collider.GetComponent<Pion>();
                    if (pion != null) pion.OnClicked();
                }
            }
        }
    }

    public void EndPlayerTurnButton()
    {
        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            if (!u.isEnemy) u.hasActed = true;

        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        if (CheckGameOver()) return;

        currentTurn = Turn.Enemy;
        HUDManager.Instance?.UpdateTopBar(currentTurn);
        Debug.Log("Tour Ennemi");
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        foreach (Pion enemy in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (enemy == null) continue;
            if (enemy.isEnemy)
                yield return StartCoroutine(EnemyAct(enemy));
        }

        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            u.ResetAction();

        currentTurn = Turn.Player;
        HUDManager.Instance?.UpdateTopBar(currentTurn);
        Debug.Log("Tour Joueur");

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
                TriggerCombat(enemy, closest);
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
                    if (!u.isEnemy) { TriggerCombat(enemy, u); yield break; }
                    break;
                }
            }

            if (!occupied)
            {
                yield return StartCoroutine(enemy.MovePathCoroutine(nextPos));
                currentPos = nextPos;

                Pion adjacent = GetAdjacentPlayerPion(enemy);
                if (adjacent != null) { TriggerCombat(enemy, adjacent); yield break; }
            }
            else break;

            movesLeft--;
        }

        enemy.hasActed = true;
    }

    public bool CheckGameOver()
    {
        bool playerAlive = false;
        bool enemyAlive = false;

        foreach (Pion p in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (p.isEnemy) enemyAlive = true;
            else playerAlive = true;
        }

        if (!playerAlive) { GameOverUI.Instance?.ShowDefeat(); return true; }
        if (!enemyAlive) { GameOverUI.Instance?.ShowVictory(); return true; }
        return false;
    }

    private Pion GetClosestPlayerPion(Pion enemy)
    {
        Pion closest = null;
        float minDist = Mathf.Infinity;
        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
        {
            if (!u.isEnemy)
            {
                float dist = Vector2.Distance(u.GetGridPosition(), enemy.GetGridPosition());
                if (dist < minDist) { minDist = dist; closest = u; }
            }
        }
        return closest;
    }

    private Pion GetAdjacentPlayerPion(Pion enemy)
    {
        foreach (Pion u in FindObjectsByType<Pion>(FindObjectsSortMode.None))
            if (!u.isEnemy && Vector2.Distance(u.GetGridPosition(), enemy.GetGridPosition()) <= 1f)
                return u;
        return null;
    }

    private void TriggerCombat(Pion enemy, Pion player)
    {
        Debug.Log($"{enemy.name} attaque {player.name} !");
        enemy.hasActed = true;
        CombatSystem.ResolveCombat(enemy, player);
    }
}