using UnityEngine;

/// <summary>
/// Classe statique centralisant toutes les regles de terrain :
/// accessibilite, cout de deplacement, et modificateur de defense.
/// A appeler depuis Tile, GameManager, ou tout systeme de deplacement/combat.
/// </summary>
public static class TerrainRules
{
    // Couts de deplacement de base
    public const float COUT_NORMAL = 1.00f;
    public const float COUT_RAPIDE = 0.66f;
    public const float COUT_LENT = 1.50f;
    public const float COUT_BLOQUE = -1f;   // case inaccessible

    /// <summary>
    /// Retourne le cout en points de deplacement pour entrer sur un terrain
    /// selon le MoveType de l'escouade.
    /// Retourne COUT_BLOQUE (-1) si la case est inaccessible.
    /// </summary>
    public static float GetMoveCost(TerrainType terrain, MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Infanterie:
                return GetCostInfanterie(terrain);

            case MoveType.Cavalerie:
                return GetCostCavalerie(terrain);

            case MoveType.Volant:
                return GetCostVolant(terrain);

            case MoveType.Maritime:
                return GetCostMaritime(terrain);

            default:
                return COUT_NORMAL;
        }
    }

    /// <summary>
    /// Retourne true si le MoveType peut entrer sur ce terrain.
    /// </summary>
    public static bool IsAccessible(TerrainType terrain, MoveType moveType)
    {
        return GetMoveCost(terrain, moveType) != COUT_BLOQUE;
    }

    /// <summary>
    /// Retourne le modificateur de degats recu par une escouade sur ce terrain.
    /// 0f    = neutre
    /// -0.10 = -10% de degats subis (defense)
    /// +0.15 = +15% de degats subis (vulnerabilite)
    /// Usage : degatsFinaux = degatsBase * (1f + GetDefenseModifier(terrain))
    /// </summary>
    public static float GetDefenseModifier(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Plaine: return 0.00f;
            case TerrainType.Foret: return -0.10f;
            case TerrainType.Colline: return -0.25f;
            case TerrainType.Montagne: return -0.75f;
            case TerrainType.Route: return 0.00f;
            case TerrainType.Riviere: return 0.00f;
            case TerrainType.Marais: return +0.15f;
            case TerrainType.Desert: return +0.10f;
            default: return 0.00f;
        }
    }

    // ----- Regles par MoveType -----

    // Infanterie :
    // Inaccessible : Montagne, Riviere
    // Lent         : Foret, Colline, Marais, Desert
    // Rapide       : Route
    // Normal       : Plaine
    private static float GetCostInfanterie(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Montagne: return COUT_BLOQUE;
            case TerrainType.Riviere: return COUT_BLOQUE;
            case TerrainType.Foret: return COUT_LENT;
            case TerrainType.Colline: return COUT_LENT;
            case TerrainType.Marais: return COUT_LENT;
            case TerrainType.Desert: return COUT_LENT;
            case TerrainType.Route: return COUT_RAPIDE;
            case TerrainType.Plaine: return COUT_NORMAL;
            default: return COUT_NORMAL;
        }
    }

    // Cavalerie :
    // Inaccessible : Colline, Montagne, Riviere
    // Lent         : Marais, Desert
    // Rapide       : Plaine, Route
    // Normal       : Foret
    private static float GetCostCavalerie(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Colline: return COUT_BLOQUE;
            case TerrainType.Montagne: return COUT_BLOQUE;
            case TerrainType.Riviere: return COUT_BLOQUE;
            case TerrainType.Marais: return COUT_LENT;
            case TerrainType.Desert: return COUT_LENT;
            case TerrainType.Plaine: return COUT_RAPIDE;
            case TerrainType.Route: return COUT_RAPIDE;
            case TerrainType.Foret: return COUT_NORMAL;
            default: return COUT_NORMAL;
        }
    }

    // Volant :
    // Tout est accessible
    // Rapide : Route
    // Normal : tout le reste
    private static float GetCostVolant(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Route: return COUT_RAPIDE;
            default: return COUT_NORMAL;
        }
    }

    // Maritime :
    // Accessible uniquement : Riviere, Marais
    // Tout le reste est bloque
    private static float GetCostMaritime(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Riviere: return COUT_NORMAL;
            case TerrainType.Marais: return COUT_NORMAL;
            default: return COUT_BLOQUE;
        }
    }
}