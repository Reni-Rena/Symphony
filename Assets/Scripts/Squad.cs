using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SquadSlot
{
    public Unit unitPrefab;
    [Range(0, 5)] public int x;
    [Range(0, 5)] public int y;
}

/// <summary>
/// Une escouade reference des unites existantes du PlayerRoster.
///
/// PlaceUnit  : reference une unite existante, marque isInSquad = true.
/// RemoveUnit : retire l'unite de la formation, marque isInSquad = false.
///              Ne detruit jamais le GameObject.
/// MoveUnit   : deplace une unite deja referencee dans la grille.
/// </summary>
public class Squad : MonoBehaviour
{
    public string squadName = "Test";

    [Header("Taille de la grille")]
    public int gridWidth = 6;
    public int gridHeight = 6;
    public float cellSize = 0.5f;

    // Slots utilises uniquement pour les escouades ennemies (instanciation propre).
    // Pour les escouades joueur, utiliser PlaceUnit directement.
    [Header("Slots (escouades ennemies uniquement)")]
    public List<SquadSlot> slots = new List<SquadSlot>();

    [HideInInspector] public Unit[,] formation;
    public Dictionary<Unit, List<Vector2Int>> occupiedCells = new Dictionary<Unit, List<Vector2Int>>();

    public List<Unit> attackTargetFirstLine = new List<Unit>();
    public List<Unit> attackUnprotectedUnits = new List<Unit>();
    public List<Unit> attackHurtUnit = new List<Unit>();

    public Unit squadLeader { get; private set; }

    public int Lead { get { return squadLeader != null ? squadLeader.lead : 0; } }
    public int leadPointsUsed { get; private set; }
    public int leadPointsMax { get { return Lead; } }
    public int leadPointsRemaining { get { return leadPointsMax - leadPointsUsed; } }

    public MoveType squadMoveType { get; private set; }
    public List<UnitType> squadTypes { get; private set; } = new List<UnitType>();

    // True si c'est une escouade ennemie (gere ses propres instances d'unites).
    [Header("Escouade ennemie")]
    public bool isEnemySquad = false;

    // -----------------------------------------------------------------------
    // Cycle de vie
    // -----------------------------------------------------------------------

    void Awake()
    {
        formation = new Unit[gridWidth, gridHeight];

        // Les escouades ennemies construisent leur formation depuis les slots
        // en instanciant leurs propres unites (comportement original).
        if (isEnemySquad)
            BuildFormationEnnemie();
    }

    // -----------------------------------------------------------------------
    // Construction pour escouades ennemies (instanciation propre)
    // -----------------------------------------------------------------------

    void BuildFormationEnnemie()
    {
        formation = new Unit[gridWidth, gridHeight];
        occupiedCells.Clear();
        squadLeader = null;
        leadPointsUsed = 0;

        foreach (SquadSlot slot in slots)
        {
            if (slot.unitPrefab == null) continue;

            Unit u = Instantiate(slot.unitPrefab, transform);

            if (!CanPlaceUnit(slot.x, slot.y, u.size))
            {
                Debug.LogWarning("Ennemi : impossible de placer " + u.unitName
                                 + " en (" + slot.x + "," + slot.y + ").");
                Destroy(u.gameObject);
                continue;
            }

            bool isLeader = (squadLeader == null);

            if (!isLeader)
            {
                int cost = u.GetCommandCost();
                if (leadPointsUsed + cost > leadPointsMax)
                {
                    Debug.LogWarning("Ennemi : cout insuffisant pour " + u.unitName + ".");
                    Destroy(u.gameObject);
                    continue;
                }
                leadPointsUsed += cost;
            }

            InscrireUnite(u, slot.x, slot.y);

            if (isLeader) squadLeader = u;
        }

        ComputeSquadMoveType();
        ComputeSquadTypes();
        UpdatedAttackInfo();
    }

    // -----------------------------------------------------------------------
    // API editeur : placement, deplacement, retrait (escouades joueur)
    // Ne cree ni ne detruit jamais de GameObject.
    // -----------------------------------------------------------------------

    // Assigne une unite existante a cette escouade et l'inscrit dans la grille.
    // L'unite doit provenir du PlayerRoster et avoir isInSquad == false.
    // isLeader = true : cette unite devient le chef (ignore le cout de commandement).
    // Retourne false si la place est insuffisante ou les points manquent.
    public bool PlaceUnit(Unit unit, int col, int row, bool isLeader = false)
    {
        if (unit == null) return false;
        if (unit.isInSquad) return false;
        if (!CanPlaceUnit(col, row, unit.size)) return false;

        if (!isLeader)
        {
            int cost = unit.GetCommandCost();
            if (leadPointsUsed + cost > leadPointsMax) return false;
            leadPointsUsed += cost;
        }

        InscrireUnite(unit, col, row);

        unit.isInSquad = true;
        unit.assignedSquad = this;

        if (isLeader) squadLeader = unit;

        ComputeSquadMoveType();
        ComputeSquadTypes();
        return true;
    }

    // Retire une unite de la formation et la rend disponible dans le PlayerRoster.
    // Ne detruit jamais le GameObject.
    // Refuse si l'unite est le chef et qu'il reste d'autres membres.
    // Retourne false si l'operation est interdite.
    public bool RemoveUnit(Unit unit)
    {
        if (unit == null) return false;
        if (!occupiedCells.ContainsKey(unit)) return false;

        bool isLeader = (unit == squadLeader);
        if (isLeader && GetLivingUnits().Count > 1) return false;

        // Liberer les cellules
        foreach (Vector2Int c in occupiedCells[unit])
            formation[c.x, c.y] = null;
        occupiedCells.Remove(unit);

        // Rembourser les points de commandement
        if (!isLeader)
            leadPointsUsed -= unit.GetCommandCost();

        if (isLeader) squadLeader = null;

        // Rendre l'unite disponible dans le roster
        unit.isInSquad = false;
        unit.assignedSquad = null;

        ComputeSquadMoveType();
        ComputeSquadTypes();
        return true;
    }

    // Deplace une unite deja dans la formation vers une nouvelle position.
    // Retourne false si la destination est invalide ou occupee.
    public bool MoveUnit(Unit unit, int newCol, int newRow)
    {
        if (!occupiedCells.ContainsKey(unit)) return false;

        // Liberer les cellules actuelles temporairement pour valider la destination
        List<Vector2Int> oldCells = occupiedCells[unit];
        foreach (Vector2Int c in oldCells)
            formation[c.x, c.y] = null;

        if (!CanPlaceUnit(newCol, newRow, unit.size))
        {
            // Remettre l'unite en place et echouer
            foreach (Vector2Int c in oldCells)
                formation[c.x, c.y] = unit;
            return false;
        }

        occupiedCells.Remove(unit);
        InscrireUnite(unit, newCol, newRow);

        ComputeSquadMoveType();
        ComputeSquadTypes();
        return true;
    }

    // Definit manuellement le chef (appele par SquadEditorUI apres un PlaceUnit en tant que chef).
    // Recalcule leadPointsUsed depuis zero.
    public void SetLeaderFromEditor(Unit unit)
    {
        squadLeader = unit;
        leadPointsUsed = 0;
        foreach (Unit u in GetLivingUnits())
            if (u != squadLeader) leadPointsUsed += u.GetCommandCost();

        ComputeSquadMoveType();
        ComputeSquadTypes();
    }

    // -----------------------------------------------------------------------
    // Requetes utilitaires
    // -----------------------------------------------------------------------

    public bool CanPlaceUnit(int x, int y, Vector2Int size)
    {
        if (x < 0 || y < 0) return false;
        if (x + size.x > gridWidth) return false;
        if (y + size.y > gridHeight) return false;

        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                if (formation[x + i, y + j] != null) return false;

        return true;
    }

    // -----------------------------------------------------------------------
    // Infos de combat
    // -----------------------------------------------------------------------

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

        foreach (KeyValuePair<Unit, List<Vector2Int>> kvp in occupiedCells)
        {
            Unit unit = kvp.Key;
            if (unit == null || unit.currentHP <= 0) continue;

            bool hasSomeoneInFront = false;

            foreach (Vector2Int cell in kvp.Value)
            {
                for (int y = cell.y - 1; y >= 0; y--)
                {
                    Unit frontUnit = formation[cell.x, y];
                    if (frontUnit != null && frontUnit != unit && frontUnit.currentHP > 0)
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
        List<Unit> unprotected = new List<Unit>();

        foreach (KeyValuePair<Unit, List<Vector2Int>> kvp in occupiedCells)
        {
            Unit unit = kvp.Key;
            if (unit == null || unit.currentHP <= 0) continue;

            bool isProtected = false;

            foreach (Vector2Int cell in kvp.Value)
            {
                for (int y = cell.y - 1; y >= 0; y--)
                {
                    Unit frontUnit = formation[cell.x, y];
                    if (frontUnit != null && frontUnit != unit && frontUnit.currentHP > 0
                        && frontUnit.GetUnitType().HasFlag(UnitType.Lourd))
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

            if (!isProtected) unprotected.Add(unit);
        }
        return unprotected;
    }

    public List<Unit> GetHurtUnits()
    {
        List<Unit> hurt = new List<Unit>();
        foreach (Unit u in GetLivingUnits())
            if (u.currentHP < u.maxHP) hurt.Add(u);
        return hurt;
    }

    public List<Unit> GetLivingUnits()
    {
        HashSet<Unit> alive = new HashSet<Unit>();
        foreach (Unit u in formation)
            if (u != null && u.currentHP > 0) alive.Add(u);
        return new List<Unit>(alive);
    }

    public bool IsAlive()
    {
        return GetLivingUnits().Count > 0;
    }

    // -----------------------------------------------------------------------
    // Interne
    // -----------------------------------------------------------------------

    // Ecrit l'unite dans formation[] et occupiedCells sans aucune verification.
    void InscrireUnite(Unit unit, int col, int row)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int i = 0; i < unit.size.x; i++)
        {
            for (int j = 0; j < unit.size.y; j++)
            {
                formation[col + i, row + j] = unit;
                cells.Add(new Vector2Int(col + i, row + j));
            }
        }
        occupiedCells[unit] = cells;
    }

    void ComputeSquadMoveType()
    {
        List<Unit> units = GetLivingUnits();
        if (units.Count == 0) { squadMoveType = MoveType.Infanterie; return; }

        if (squadLeader != null && squadLeader.GetUnitMoveType() == MoveType.Maritime)
        { squadMoveType = MoveType.Maritime; return; }

        bool allFlying = true;
        foreach (Unit u in units)
            if (u.GetUnitMoveType() != MoveType.Volant) { allFlying = false; break; }
        if (allFlying) { squadMoveType = MoveType.Volant; return; }

        bool allCavOrFly = true;
        foreach (Unit u in units)
        {
            MoveType mt = u.GetUnitMoveType();
            if (mt != MoveType.Volant && mt != MoveType.Cavalerie) { allCavOrFly = false; break; }
        }

        squadMoveType = allCavOrFly ? MoveType.Cavalerie : MoveType.Infanterie;
    }

    void ComputeSquadTypes()
    {
        squadTypes.Clear();
        List<Unit> units = GetLivingUnits();
        int count = units.Count;
        if (count == 0) return;

        int CountOfType(UnitType type)
        {
            int n = 0;
            foreach (Unit u in units)
                if (u.GetUnitType().HasFlag(type)) n++;
            return n;
        }

        int lourdReq = count <= 3 ? 1 : count <= 7 ? 2 : 3;
        int legereReq = count <= 3 ? 1 : count <= 7 ? 2 : 3;
        int magiqueReq = count <= 4 ? 1 : count <= 6 ? 2 : count <= 8 ? 3 : 4;
        int supportReq = count == 1 ? 1 : count <= 4 ? 2 : count <= 7 ? 3 : 4;

        if (CountOfType(UnitType.Lourd) >= lourdReq) squadTypes.Add(UnitType.Lourd);
        if (CountOfType(UnitType.Legere) >= legereReq) squadTypes.Add(UnitType.Legere);
        if (CountOfType(UnitType.Magique) >= magiqueReq) squadTypes.Add(UnitType.Magique);
        if (CountOfType(UnitType.Support) >= supportReq) squadTypes.Add(UnitType.Support);
    }
}