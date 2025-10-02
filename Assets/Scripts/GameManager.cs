using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{    
    private List<Pion> playerPions = new List<Pion>();

    public enum Turn { Player, Enemy }
    public Turn currentTurn = Turn.Player;
    public GameObject canvas;

    void Start()
    {
        canvas.SetActive(true);
        // Récupère toutes les pions joueur au début
        Pion[] allPions = FindObjectsOfType<Pion>();
        foreach (Pion u in allPions)
        {
            if (!u.isEnemy)
                playerPions.Add(u);
            u.ResetAction();
        }
    }

    public void EndPlayerTurnButton()
    {
        // Marque toutes les pions joueur comme ayant agi
        foreach (Pion u in FindObjectsOfType<Pion>())
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
        Pion[] enemyPions = FindObjectsOfType<Pion>();
        foreach (Pion enemy in enemyPions)
        {
            if (enemy.isEnemy)
            {
                yield return StartCoroutine(EnemyAct(enemy));
            }
        }

        // Réinitialise toutes les pions (joueur + ennemis) pour le prochain tour
        foreach (Pion u in FindObjectsOfType<Pion>())
        {
            u.ResetAction();
        }

        currentTurn = Turn.Player;
        Debug.Log("Tour Joueur");
    }


    private IEnumerator EnemyAct(Pion enemy)
    {
        Pion[] playerPionsArray = FindObjectsOfType<Pion>();
        Pion closest = null;
        float minDist = Mathf.Infinity;

        foreach (Pion u in playerPionsArray)
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
                foreach (Pion u in FindObjectsOfType<Pion>())
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

                    // Vérifie s'il y a un combat après déplacement
                    //enemy.CheckCombat();
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
