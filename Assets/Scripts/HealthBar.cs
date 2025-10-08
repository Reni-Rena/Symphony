using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image healthBarFill;
    public Unit unit;

    void Awake()
    {
        healthBarFill = GetComponent<Image>();
    }

    void Update()
    {
        if (unit != null)
        {
            healthBarFill.fillAmount = (float)unit.currentHP / unit.maxHP;
        }
    }
}
