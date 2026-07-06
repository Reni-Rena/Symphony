using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[System.Serializable]
public class SquadSlot
{
    public Unit unitPrefab;
    [Range(0, 5)] public int x;
    [Range(0, 5)] public int y;
}

public class Squad : MonoBehaviour
{
    public string squadName = "Test";

    [Header("Taille de la grille")]
    public int gridWidth = 6;
    public int gridHeight = 6;
    public float cellSize = 0.5f;

    [Header("Configuration dans l'inspecteur")]
    public List<SquadSlot> slots = new List<SquadSlot>();

    [HideInInspector] public Unit[,] formation;
    public Dictionary<Unit, List<Vector2Int>> occupiedCells = new Dictionary<Unit, List<Vector2Int>>();
    public List<Unit> attackTargetFirstLine = new List<Unit>();
    public List<Unit> attackUnprotectedUnits = new List<Unit>();
    public List<Unit> attackHurtUnit = new List<Unit>();

    // Chef de l'escouade : la premiere unite placee avec succes
    public Unit squadLeader { get; private set; }
    public int Lead { get { return squadLeader != null ? squadLeader.lead : 0; } }
    public int leadPointsUsed { get; private set; }
    public int leadPointsMax { get { return Lead; } }
    public int leadPointsRemaining { get { return leadPointsMax - leadPointsUsed; } }
    public MoveType squadMoveType { get; private set; }
    public List<UnitType> squadTypes { get; private set; } = new List<UnitType>();


    void Awake()
    {
        BuildFormation();
        UpdatedAttackInfo();
    }

    // Construction de la formation.
    // La premiere unite placee avec succes devient chef (gratuite).
    // Chaque unite suivante doit avoir la place disponible ET
    // les points de commandement suffisants (selon son tier).
    public void BuildFormation()
    {
        formation = new Unit[gridWidth, gridHeight];
        occupiedCells.Clear();
        squadLeader = null;
        leadPointsUsed = 0;

        foreach (var slot in slots)
        {
            if (slot.unitPrefab == null) continue;

            Unit u = Instantiate(slot.unitPrefab, transform);
            Vector2Int size = u.size;

            // Verifier la place disponible dans la grille
            if (!CanPlaceUnit(slot.x, slot.y, size))
            {
                Debug.LogWarning($"Impossible de placer {u.unitName} en ({slot.x},{slot.y}) : place occupee.");
                Destroy(u.gameObject);
                continue;
            }

            // La premiere unite reussie devient chef : elle est gratuite
            bool isLeader = (squadLeader == null);

            if (!isLeader)
            {
                // Verifier les points de commandement disponibles
                // Le chef doit deja etre place pour que Lead soit connu
                int cost = u.GetCommandCost();
                if (leadPointsUsed + cost > leadPointsMax)
                {
                    Debug.LogWarning(
                        $"Impossible de placer {u.unitName} (Tier {u.GetTier()}) en ({slot.x},{slot.y}) : " +
                        $"cout {cost} pts, disponibles {leadPointsMax - leadPointsUsed}/{leadPointsMax}.");
                    Destroy(u.gameObject);
                    continue;
                }

                leadPointsUsed += cost;
            }

            // Placer l'unite dans la formation
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    formation[slot.x + i, slot.y + j] = u;
                    cells.Add(new Vector2Int(slot.x + i, slot.y + j));
                }
            }
            occupiedCells[u] = cells;

            if (isLeader)
            {
                squadLeader = u;
                Debug.Log($"{u.unitName} devient chef d'escouade avec {u.lead} pts de commandement.");
            }
            else
            {
                Debug.Log(
                    $"{u.unitName} (Tier {u.GetTier()}) place : cout {u.GetCommandCost()} pts, " +
                    $"restants {leadPointsRemaining}/{leadPointsMax}.");
            }
        }

        ComputeSquadMoveType();
        ComputeSquadTypes();
    }

    // Calcule le moveType de l'escouade selon les regles definies
    private void ComputeSquadMoveType()
    {
        List<Unit> units = GetLivingUnits();

        if (units.Count == 0)
        {
            squadMoveType = MoveType.Infanterie;
            return;
        }

        // Maritime si le chef a moveType Maritime
        if (squadLeader != null && GetMoveType(squadLeader) == MoveType.Maritime)
        {
            squadMoveType = MoveType.Maritime;
            return;
        }

        // Volant si toutes les unites sont Volant
        bool allFlying = true;
        foreach (Unit u in units)
        {
            if (GetMoveType(u) != MoveType.Volant)
            {
                allFlying = false;
                break;
            }
        }

        if (allFlying)
        {
            squadMoveType = MoveType.Volant;
            return;
        }

        // Cavalerie si toutes les unites sont Volant ou Cavalerie
        bool allCavalryOrFlying = true;
        foreach (Unit u in units)
        {
            MoveType mt = GetMoveType(u);
            if (mt != MoveType.Volant && mt != MoveType.Cavalerie)
            {
                allCavalryOrFlying = false;
                break;
            }
        }

        if (allCavalryOrFlying)
        {
            squadMoveType = MoveType.Cavalerie;
            return;
        }

        // Infanterie dans tous les autres cas
        squadMoveType = MoveType.Infanterie;
    }

    private MoveType GetMoveType(Unit u)
    {
        return u.GetUnitMoveType();
    }

    // Calcule la liste des squadTypes en comptant les unites de chaque UnitType
    // et en comparant aux seuils definis pour la taille de l'escouade.
    private void ComputeSquadTypes()
    {
        squadTypes.Clear();

        List<Unit> units = GetLivingUnits();
        int count = units.Count;

        if (count == 0) return;

        // Compte les unites portant un UnitType donne (flags)
        int CountOfType(UnitType type)
        {
            int n = 0;
            foreach (Unit u in units)
                if (u.GetUnitType().HasFlag(type)) n++;
            return n;
        }

        // Lourd : seuil 1 pour 1-3 unites, 2 pour 4-7, 3 pour 8+
        int lourdRequired = count <= 3 ? 1 : count <= 7 ? 2 : 3;
        if (CountOfType(UnitType.Lourd) >= lourdRequired)
            squadTypes.Add(UnitType.Lourd);

        // Legere : meme logique que Lourd
        int legereRequired = count <= 3 ? 1 : count <= 7 ? 2 : 3;
        if (CountOfType(UnitType.Legere) >= legereRequired)
            squadTypes.Add(UnitType.Legere);

        // Magique : seuil 1 pour 1-4, 2 pour 5-6, 3 pour 7-8, 4 pour 9+
        int magiqueRequired = count <= 4 ? 1 : count <= 6 ? 2 : count <= 8 ? 3 : 4;
        if (CountOfType(UnitType.Magique) >= magiqueRequired)
            squadTypes.Add(UnitType.Magique);

        // Support : seuil 1 pour 1, 2 pour 2-4, 3 pour 5-7, 4 pour 8+
        int supportRequired = count == 1 ? 1 : count <= 4 ? 2 : count <= 7 ? 3 : 4;
        if (CountOfType(UnitType.Support) >= supportRequired)
            squadTypes.Add(UnitType.Support);
    }

    // Verifie si une unite peut etre placee a une position donnee
    private bool CanPlaceUnit(int x, int y, Vector2Int size)
    {
        if (x + size.x > gridWidth || y + size.y > gridHeight) return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (formation[x + i, y + j] != null) return false;
            }
        }
        return true;
    }

    public void UpdatedAttackInfo()
    {
        attackTargetFirstLine.Clear();
        attackUnprotectedUnits.Clear();
        attackHurtUnit.Clear();

        attackTargetFirstLine = GetFrontlineUnits();
        attackUnprotectedUnits = GetUnprotectedUnits();
        attackHurtUnit = GetHurtUnits();
    }

    public List<Unit> GetFrontlineUnits()
    {
        List<Unit> frontline = new List<Unit>();

        foreach (var kvp in occupiedCells)
        {
            Unit unit = kvp.Key;
            if (unit == null || unit.currentHP <= 0) continue;

            bool hasSomeoneInFront = false;

            foreach (Vector2Int cell in kvp.Value)
            {
                for (int y = cell.y - 1; y >= 0; y--)
                {
                    Unit frontUnit = formation[cell.x, y];
                    if (frontUnit != null &&
                        frontUnit != unit &&
                        frontUnit.currentHP > 0)
                    {
                        hasSomeoneInFront = true;
                        break;
                    }
                }
                if (hasSomeoneInFront) break;
            }
            if (!hasSomeoneInFront) frontline.Add(unit);
        }
        return frontline;
    }

    public List<Unit> GetUnprotectedUnits()
    {
        List<Unit> Unprotected = new List<Unit>();

        foreach (var kvp in occupiedCells)
        {
            Unit unit = kvp.Key;
            if (unit == null || unit.currentHP <= 0) continue;

            bool isProtected = false;

            foreach (Vector2Int cell in kvp.Value)
            {
                // On regarde toutes les lignes devant la cellule
                for (int y = cell.y - 1; y >= 0; y--)
                {
                    Unit frontUnit = formation[cell.x, y];
                    if (frontUnit != null &&
                        frontUnit != unit &&
                        frontUnit.currentHP > 0 &&
                        frontUnit.GetUnitType().HasFlag(UnitType.Lourd))
                    {
                        if (frontUnit.GetComponent<UnitHeavy>().protectedCase + y >= cell.y)
                        {
                            isProtected = true;
                            break;
                        }
                    }
                }

                if (isProtected) break;
            }
            if (!isProtected) Unprotected.Add(unit);
        }

        return Unprotected;
    }

    public List<Unit> GetHurtUnits()
    {
        List<Unit> hurtunit = new List<Unit>();
        foreach (Unit u in GetLivingUnits())
        {
            if (u.currentHP < u.maxHP) hurtunit.Add(u);
        }
        return hurtunit;
    }

    // Retourner toutes les unites vivantes
    public List<Unit> GetLivingUnits()
    {
        HashSet<Unit> alive = new HashSet<Unit>();
        foreach (Unit u in formation)
        {
            if (u != null && u.currentHP > 0) alive.Add(u);
        }
        return new List<Unit>(alive);
    }

    // Verifier si l'escouade est encore en vie
    public bool IsAlive()
    {
        return GetLivingUnits().Count > 0;
    }
}