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
    public bool isEnemy = false;

    public Squad squad;
    private Vector3 caseDepart;
    public int moveRange = 3;
    public float moveSpeed = 5f;

    void Start()
    {
        squad = GetComponent<Squad>();
        _renderer = GetComponent<SpriteRenderer>();
        baseColor = _renderer.color;
    }

    void Update()
    {
        if (hasActed)
            _renderer.color = usedColor;
        else
            _renderer.color = isSelected ? selectColor : baseColor;
    }

    public void OnClicked()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm.currentTurn != GameManager.Turn.Player || isEnemy) return;

        foreach (Pion u in FindObjectsOfType<Pion>())
            if (u != this) u.Deselect();

        Tile.ClearHighlights();

        isSelected = !isSelected;
        _renderer.color = isSelected ? Color.yellow : baseColor;

        if (isSelected)
        {
            Tile.HighlightTiles(this);
            HUDManager.Instance?.ShowPionInfo(this); // affiche les infos
        }
        else
        {
            HUDManager.Instance?.HidePanel(); // cache le panneau si on déselectionne
        }
    }

    public void Deselect()
    {
        isSelected = false;
        HUDManager.Instance?.HidePanel();
    }

    public void ResetAction() { hasActed = false; }

    public void MoveTo(Vector2 targetPosition)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if ((isEnemy && gm.currentTurn == GameManager.Turn.Player) ||
            (!isEnemy && gm.currentTurn == GameManager.Turn.Enemy)) return;
        if (hasActed) return;

        StartDeplacement();
        StartCoroutine(MovePathCoroutine(targetPosition));
        Tile.ClearHighlights();
        EndDeplacement();
    }

    public IEnumerator MovePathCoroutine(Vector2 targetPos)
    {
        Vector2 currentPos = new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y));

        List<Vector2> path = new List<Vector2>();

        float dx = targetPos.x - currentPos.x;
        float dy = targetPos.y - currentPos.y;

        int stepX = dx > 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(dx); i++)
        { currentPos.x += stepX; path.Add(currentPos); }

        int stepY = dy > 0 ? 1 : -1;
        for (int i = 0; i < Mathf.Abs(dy); i++)
        { currentPos.y += stepY; path.Add(currentPos); }

        foreach (Vector2 pos in path)
        {
            Vector3 target = new Vector3(pos.x, pos.y, transform.position.z);
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }
    }

    void StartDeplacement() { caseDepart = transform.position; }

    void EndDeplacement()
    {
        ActionMenuUI menu = FindObjectOfType<ActionMenuUI>();
        menu.Show(
            () => AnnulerDeplacement(),
            () => ValiderDeplacement(),
            () => Attaquer()
        );
    }

    void AnnulerDeplacement() { Deselect(); transform.position = caseDepart; }
    void ValiderDeplacement() { Deselect(); hasActed = true; }
    void Attaquer() { Tile.HighlightAttackableTiles(this); }

    public void Combat(Pion squadB)
    {
        CombatSystem.ResolveCombat(this, squadB);
        Tile.ClearHighlights();
        Deselect();
        hasActed = true;
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " est éliminé !");
        Destroy(gameObject);
    }

    public bool IsSelected() { return isSelected; }
    public int GetMoveRange() { return moveRange; }
    public Vector2 GetGridPosition()
    {
        return new Vector2(
        Mathf.Round(transform.position.x),
        Mathf.Round(transform.position.y));
    }
}