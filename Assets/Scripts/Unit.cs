using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum MoveType
{
    Infanterie,
    Cavalerie,
    Volant,
    Maritime
}
[System.Flags]
public enum UnitType
{
    Aucun = 0,
    Lourd = 1 << 0,
    Légère = 1 << 1,
    Archerie = 1 << 2,
    Magique = 1 << 3,
    Support = 1 << 4
}
public enum RaceType
{
    Humain,
    Dragonoide,
    HommeBête,
    Elf
}

public class Unit : MonoBehaviour
{

    [SerializeField] private string unitTypeName;
    public string unitName = "Mat";

    [Header("Sprits")]
    public Sprite iconeSprite;
    public Sprite iconeDeadSprite;
    public Sprite imageSprite;

    [Header("Stats d'unité")]
    [SerializeField] private int unitTier = 1;
    [SerializeField] private MoveType moveType;
    [SerializeField] private UnitType unitType;
    [SerializeField] private int range = 1;
    public Vector2Int size = new Vector2Int(2, 2);

    [Header("Stats de LVL")]
    [SerializeField] private int lvlHP;
    [SerializeField] private float lvlArmor;
    [SerializeField] private float APStrength;
    [SerializeField] private float APAgility;
    [SerializeField] private float APMagic;

    [Header("Stats de spawn")]
    [SerializeField] private int MinHP;
    [SerializeField] private int MaxHP;
    [SerializeField] private int MinArmor;
    [SerializeField] private int MaxArmor;
    [SerializeField] private int MinStrength;
    [SerializeField] private int MaxStrength;
    [SerializeField] private int MinAgility;
    [SerializeField] private int MaxAgility;
    [SerializeField] private int MinMagic;
    [SerializeField] private int MaxMagic;
    [SerializeField] private int MinLead;
    [SerializeField] private int MaxLead;

    [Header("Stats de Combat")]
    [SerializeField] private int AttackOrder;
    public Unit target;

    [Header("Stats de base")]
    public RaceType raceType = RaceType.Humain;
    public int lvl;
    public int XP;
    public int maxHP;
    public int currentHP;
    public int armor;
    public int strength;
    public int agility;
    public int magic;
    public int lead;


    void Awake()
    {
        maxHP = Random.Range(MinHP, MaxHP+1);
        armor = Random.Range(MinArmor, MaxArmor+1);
        strength = Random.Range(MinStrength, MaxStrength+1);
        agility = Random.Range(MinAgility, MaxAgility+1);
        magic = Random.Range(MinMagic, MaxMagic+1);
        lead = Random.Range(MinLead, MaxLead+1);
        currentHP = maxHP;
        lvl = 1;
        XP = 0;
    }

    // Inflige des dégâts à cette unité
    public void TakeDamage(int dmg)
    {
        int realDmg = dmg - armor;
        if (realDmg < 0) realDmg = 0;
        currentHP -= realDmg;
        Debug.Log(unitName + " prend " + realDmg + " (" + dmg + " - " + armor + ")");


        if (currentHP <= 0)
        {
            // l'unité est dead
            currentHP = 0;
        }
    }
    public void HealDamage(int heal)
    {
        currentHP += heal;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public int DealDamage()
    {
        float dmg = (strength * APStrength) + (agility * APAgility) + (magic * APMagic);
        return (int)dmg + 1;
    }

    public int GetAttackOrder() { return AttackOrder; }
    public UnitType GetUnitType() { return unitType; }

}
