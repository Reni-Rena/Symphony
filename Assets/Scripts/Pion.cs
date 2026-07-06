using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Le Pion ne contient plus de logique FSM.
/// Il expose des méthodes appelées par GameManager.
/// </summary>
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

    //  Sélection 

    /// <summary>Appelé par GameManager pour marquer ce pion comme sélectionné/désélectionné.</summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool IsSelected() => isSelected;
    public int GetMoveRange() => moveRange;

    public Vector2 GetGridPosition()
    {
        return new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y));
    }

    //  Déplacement 

    /// <summary>
    /// Déplace le pion case par case en suivant un chemin Manhattan.
    /// À appeler via StartCoroutine depuis GameManager.
    /// Le menu n'est PAS ouvert ici — c'est GameManager qui le fait après le yield.
    /// </summary>
    public IEnumerator MovePathCoroutine(Vector2 targetPos)
    {
        Vector2 currentPos = GetGridPosition();

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

    //  Combat 

    public void Die()
    {
        Debug.Log(gameObject.name + " est éliminé !");
        Destroy(gameObject);
    }

    //  Tour 

    public void ResetAction() { hasActed = false; }
}