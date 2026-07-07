using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contient toutes les unites du joueur pour toute la partie.
/// Les unites sont instanciees une seule fois ici et ne sont jamais detruites
/// (sauf mort permanente, a implementer plus tard si souhaite).
///
/// Les escouades referent ces unites sans les posseder : elles lisent
/// unit.isInSquad pour savoir si une unite est disponible ou deja affectee.
///
/// Setup dans Unity :
///   Creer un GameObject "PlayerRoster" dans la scene, lui attacher ce script.
///   Dans l'inspecteur, remplir la liste unitPrefabs avec les prefabs de toutes
///   les unites que le joueur possede au debut de la partie.
///   Les unites sont instanciees comme enfants de ce GameObject au demarrage.
/// </summary>
public class PlayerRoster : MonoBehaviour
{
    public static PlayerRoster Instance { get; private set; }

    [Header("Prefabs des unites du joueur au debut de la partie")]
    [Tooltip("Remplir avec les prefabs. Chaque prefab est instancie une seule fois ici.")]
    public List<Unit> unitPrefabs = new List<Unit>();

    // Toutes les unites du joueur (instances vivantes).
    private List<Unit> toutesLesUnites = new List<Unit>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InstancierUnites();
    }

    // Instancie chaque prefab une seule fois comme enfant de ce GameObject.
    void InstancierUnites()
    {
        foreach (Unit prefab in unitPrefabs)
        {
            if (prefab == null) continue;

            Unit u = Instantiate(prefab, transform);
            u.isInSquad = false;
            u.assignedSquad = null;

            // Desactiver le GameObject : l'unite n'est pas sur la carte tant
            // qu'elle n'est pas assignee a une escouade deployee.
            u.gameObject.SetActive(false);

            toutesLesUnites.Add(u);
        }
    }

    // Toutes les unites du joueur, disponibles ou non.
    public List<Unit> GetAllUnits()
    {
        return new List<Unit>(toutesLesUnites);
    }

    // Unites non encore assignees a une escouade.
    public List<Unit> GetAvailableUnits()
    {
        List<Unit> disponibles = new List<Unit>();
        foreach (Unit u in toutesLesUnites)
            if (!u.isInSquad) disponibles.Add(u);
        return disponibles;
    }

    // Unites assignees a une escouade specifique.
    public List<Unit> GetUnitsInSquad(Squad squad)
    {
        List<Unit> result = new List<Unit>();
        foreach (Unit u in toutesLesUnites)
            if (u.isInSquad && u.assignedSquad == squad) result.Add(u);
        return result;
    }
}