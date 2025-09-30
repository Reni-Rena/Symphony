using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{    
    private List<Unit> playerUnits = new List<Unit>();

    public enum Turn { Player, Enemy }
    public Turn currentTurn = Turn.Player;

    void Start()
    {
        // Récupère toutes les unités joueur au début
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit u in allUnits)
        {
            if (!u.isEnemy)
                playerUnits.Add(u);
            u.ResetAction();
        }
    }

    public void EndPlayerTurnButton()
    {
        // Marque toutes les unités joueur comme ayant agi
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (!u.isEnemy)
                u.hasActed = true;
        }

        EndPlayerTurn();
    }


    void EndPlayerTurn()
    {
        currentTurn = Turn.Enemy;
        Debug.Log("Tour Ennemi");

        // Lancer les actions ennemies
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        Unit[] enemyUnits = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy.isEnemy)
            {
                yield return StartCoroutine(EnemyAct(enemy));
            }
        }

        // Réinitialise toutes les unités (joueur + ennemis) pour le prochain tour
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            u.ResetAction();
        }

        currentTurn = Turn.Player;
        Debug.Log("Tour Joueur");
    }


    private IEnumerator EnemyAct(Unit enemy)
    {
        Unit[] playerUnitsArray = FindObjectsOfType<Unit>();
        Unit closest = null;
        float minDist = Mathf.Infinity;

        foreach (Unit u in playerUnitsArray)
        {
            if (!u.isEnemy)
            {
                float dist = Vector2.Distance(u.GetGridPosition(), enemy.GetGridPosition());
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = u;
                }
            }
        }

        if (closest != null)
        {
            Vector2 currentPos = enemy.GetGridPosition();
            Vector2 targetPos = closest.GetGridPosition();

            int movesLeft = enemy.moveRange;

            while (movesLeft > 0 && currentPos != targetPos)
            {
                Vector2 nextPos = currentPos;

                // Déplacement simple : priorise X, puis Y
                Vector2 direction = targetPos - currentPos;
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    nextPos.x += Mathf.Sign(direction.x);
                else
                    nextPos.y += Mathf.Sign(direction.y);

                // Vérifie que la case n'est pas occupée
                bool occupied = false;
                foreach (Unit u in FindObjectsOfType<Unit>())
                {
                    if ((Vector2)u.transform.position == nextPos)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                {
                    yield return StartCoroutine(enemy.MovePathCoroutine(nextPos));
                    currentPos = nextPos;
                }
                else
                {
                    break; // bloqué, stop
                }

                movesLeft--;
            }
        }

        enemy.hasActed = true;
    }

}
