using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Pion : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color baseColor;
    public Color selectColor;
    public Color usedColor;

    private bool isSelected = false;
    public bool hasActed = false;
    public bool isEnemy = false; // true pour les unités ennemies

    public int moveRange = 3; // nombre de cases max par tour
    public float moveSpeed = 5f; // vitesse du déplacement

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        baseColor = _renderer.color;
    }

    void Update()
    {
        // Feedback visuel : couleur selon action
        if (hasActed)
        {
            _renderer.color = usedColor;
        }
        else
        {
            _renderer.color = isSelected ? selectColor : baseColor;
        }
    }

    void OnMouseDown()
    {
        // Vérifie si c'est le tour joueur et si l'unité est jouable
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm.currentTurn != GameManager.Turn.Player || isEnemy)
            return;

        // Deselect toutes les autres pions
        Pion[] allPions = FindObjectsOfType<Pion>();
        foreach (Pion u in allPions)
        {
            if (u != this)
                u.Deselect(); // désélectionne
        }

        // Annule les highlights des cases avant de sélectionner la nouvelle unité
        Tile.ClearHighlights();

        // Sélectionne cette unité
        isSelected = !isSelected;
        _renderer.color = isSelected ? Color.yellow : baseColor;

        // Met à jour les cases accessibles pour cette unité
        if (isSelected)
            Tile.HighlightTiles(this);
    }

    public void Deselect()
    {
        isSelected = false;
    }

    public void ResetAction()
    {
        hasActed = false;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if ((isEnemy && gm.currentTurn == GameManager.Turn.Player) || (!isEnemy && gm.currentTurn == GameManager.Turn.Enemy))
            return;

        if (hasActed)
            return;

        StartCoroutine(MoveAndCheckCombat(targetPosition));
        isSelected = false;
        Tile.ClearHighlights();
        hasActed = true;
    }

    private IEnumerator MoveAndCheckCombat(Vector2 targetPosition)
    {
        yield return StartCoroutine(MovePathCoroutine(targetPosition));
    }


    public IEnumerator MovePathCoroutine(Vector2 targetPos)
    {
        Vector2 currentPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        List<Vector2> path = new List<Vector2>();

        // Calculer le chemin simple : d'abord X, ensuite Y
        float dx = targetPos.x - currentPos.x;
        float dy = targetPos.y - currentPos.y;

        int stepX = dx > 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(dx); i++)
        {
            currentPos.x += stepX;
            path.Add(new Vector2(currentPos.x, currentPos.y));
        }

        int stepY = dy > 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(dy); i++)
        {
            currentPos.y += stepY;
            path.Add(new Vector2(currentPos.x, currentPos.y));
        }

        // Déplacer l'unité sur chaque case
        foreach (Vector2 pos in path)
        {
            Vector3 target = new Vector3(pos.x, pos.y, -0.5f);
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public int GetMoveRange()
    {
        return moveRange;
    }

    public Vector2 GetGridPosition()
    {
        return new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
    }


    /*public void CheckCombat()
    {
        // Cherche toutes les unités autour (haut, bas, gauche, droite)
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir;
            foreach (Pion other in FindObjectsOfType<Pion>())
            {
                if ((Vector2)other.transform.position == checkPos && other.isEnemy != this.isEnemy)
                {
                    // Combat trouvé
                    int damage = Mathf.Max(1, this.attack - other.defense);
                    other.TakeDamage(damage);

                    // Riposte si vivant
                    if (other.currentHP > 0)
                    {
                        int counterDamage = Mathf.Max(1, other.attack - this.defense);
                        this.TakeDamage(counterDamage);
                    }
                    return; // un seul combat
                }
            }
        }
    }*/
}
