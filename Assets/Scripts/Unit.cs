using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    private bool isSelected = false;
    private SpriteRenderer _renderer;
    public Color baseColor;
    public Color selectColor;
    public Color usedColor;

    public int moveRange = 3; // nombre de cases max par tour
    public float moveSpeed = 5f; // vitesse de déplacement

    public bool hasActed = false;
    public bool isEnemy = false; // true pour les unités ennemies

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

        // Deselect toutes les autres unités
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit u in allUnits)
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
        _renderer.color = baseColor;
    }

    public void ResetAction()
    {
        hasActed = false;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if ((isEnemy && gm.currentTurn == GameManager.Turn.Player) || (!isEnemy && gm.currentTurn == GameManager.Turn.Enemy))
            return; // bloque le mouvement si ce n'est pas le bon tour

        if (hasActed)
            return;

        StartCoroutine(MovePathCoroutine(targetPosition));
        isSelected = false;
        _renderer.color = baseColor;
        Tile.ClearHighlights();
        hasActed = true;
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
}
