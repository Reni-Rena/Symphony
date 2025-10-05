using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{

    public string unitName = "Soldat";
    public SpriteRenderer sprite;
    [Header("Stats de base")]
    public int maxHP = 10;
    public int currentHP;
    public int attack = 5;
    public int defense = 2;
    public bool isRanged;

    [Header("Health Bar UI")]
    public Image healthBarFill; // Assigne dans l’inspecteur

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
        UpdateHealthBar();
    }

    // Inflige des dégâts à cette unité
    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0)
        {
            Debug.Log(name + " est mort !");
            Destroy(gameObject);
        }
        else
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHP / maxHP;
        }
    }


}
