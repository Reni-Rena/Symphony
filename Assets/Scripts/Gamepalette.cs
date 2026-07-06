using UnityEngine;

[CreateAssetMenu(fileName = "GamePalette", menuName = "Symphony/Palette")]
public class GamePalette : ScriptableObject
{
    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    [Header("Encre noire")]
    public Color encreNoire01 = Hex("#1A0F06");
    public Color encreNoire02 = Hex("#2C1A0E");
    public Color encreNoire03 = Hex("#533A1E");

    [Header("Terre cuite")]
    public Color terreCuite01 = Hex("#7A4A1E");
    public Color terreCuite02 = Hex("#65330E");

    [Header("Parchemin")]
    public Color parchemin = Hex("#F0E6C8");

    [Header("Or")]
    public Color orBrule = Hex("#C8953A");
    public Color orClaire = Hex("#F5C97A");

    [Header("Bleu royal")]
    public Color bleuRoyal01 = Hex("#14283D");
    public Color bleuRoyal02 = Hex("#1E3A5F");
    public Color bleuRoyal03 = Hex("#5683B8");

    [Header("Bordeaux")]
    public Color bordeaux01 = Hex("#2E1516");
    public Color bordeaux02 = Hex("#4A1E2E");
    public Color bordeaux03 = Hex("#933d61");

    [Header("Vert forêt")]
    public Color vertForet01 = Hex("#1E3318");
    public Color vertForet02 = Hex("#35612D");
}