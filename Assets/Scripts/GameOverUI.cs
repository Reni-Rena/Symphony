using UnityEngine;
using UnityEngine.UI;

// À attacher sur un GameObject "GameOverScreen" dans la scène.
// Assigne les références dans l'inspecteur.
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("Références scène")]
    public GameObject victoryScreen;  // écran affiché si le joueur gagne
    public GameObject defeatScreen;   // écran affiché si le joueur perd

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Les deux écrans sont cachés au départ
        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (defeatScreen != null) defeatScreen.SetActive(false);
    }

    public void ShowVictory()
    {
        Debug.Log("Victoire !");
        if (victoryScreen != null) victoryScreen.SetActive(true);
    }

    public void ShowDefeat()
    {
        Debug.Log("Défaite !");
        if (defeatScreen != null) defeatScreen.SetActive(true);
    }
}