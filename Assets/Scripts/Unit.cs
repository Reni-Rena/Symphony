using UnityEngine;
using System;

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
    Legere = 1 << 1,
    Archerie = 1 << 2,
    Magique = 1 << 3,
    Support = 1 << 4
}

public enum RaceType
{
    Humain,
    Dragonoide,
    HommeBete,
    Elf
}

public class Unit : MonoBehaviour
{
    [SerializeField] private string unitTypeName;
    public string unitName = "Mat";

    [Header("Sprites")]
    public Sprite iconeSprite;
    public Sprite iconeDeadSprite;
    public Sprite imageSprite;

    [Header("Stats d'unite")]
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

    [Header("Affectation")]
    public bool isInSquad = false;
    public Squad assignedSquad = null;

    public event Action<Unit> OnDeath;

    public bool IsDead => currentHP <= 0;

    void Awake()
    {
        maxHP = UnityEngine.Random.Range(MinHP, MaxHP + 1);
        armor = UnityEngine.Random.Range(MinArmor, MaxArmor + 1);
        strength = UnityEngine.Random.Range(MinStrength, MaxStrength + 1);
        agility = UnityEngine.Random.Range(MinAgility, MaxAgility + 1);
        magic = UnityEngine.Random.Range(MinMagic, MaxMagic + 1);
        lead = UnityEngine.Random.Range(MinLead, MaxLead + 1);
        currentHP = maxHP;
        lvl = 1;
        XP = 0;
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;

        int realDmg = Mathf.Max(0, dmg - armor);
        currentHP -= realDmg;
        Debug.Log($"{unitName} prend {realDmg} ({dmg} - {armor})");

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnDeath?.Invoke(this);
        }
    }

    public void HealDamage(int heal)
    {
        if (IsDead) return;
        currentHP = Mathf.Min(currentHP + heal, maxHP);
    }

    public int DealDamage()
    {
        float dmg = (strength * APStrength) + (agility * APAgility) + (magic * APMagic);
        return (int)dmg + 1;
    }

    public int GetAttackOrder() { return AttackOrder; }
    public MoveType GetUnitMoveType() { return moveType; }
    public UnitType GetUnitType() { return unitType; }
    public int GetTier() { return unitTier; }

    // Retourne le cout en points de commandement pour placer cette unite
    public int GetCommandCost()
    {
        switch (unitTier)
        {
            case 1: return 8;
            case 2: return 10;
            case 3: return 13;
            default: return 8;
        }
    }
}