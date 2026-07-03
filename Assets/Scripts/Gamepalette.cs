using UnityEngine;

[CreateAssetMenu(fileName = "GamePalette", menuName = "Symphony/Palette")]
public class GamePalette : ScriptableObject
{
    [Header("Encre noire")]
    public Color encreNoire01 = new Color(0.173f, 0.102f, 0.055f); // #2C1A0E
    public Color encreNoire02 = new Color(0.161f, 0.078f, 0.031f); // #291408

    [Header("Terre cuite")]
    public Color terreCuite01 = new Color(0.478f, 0.290f, 0.118f); // #7A4A1E
    public Color terreCuite02 = new Color(0.396f, 0.200f, 0.055f); // #65330E

    [Header("Or")]
    public Color orBrule = new Color(0.784f, 0.584f, 0.227f); // #C8953A
    public Color orClaire = new Color(0.961f, 0.788f, 0.478f); // #F5C97A

    [Header("Parchemin")]
    public Color parchemin = new Color(0.941f, 0.902f, 0.784f); // #F0E6C8

    [Header("Bleu royal")]
    public Color bleuRoyal01 = new Color(0.118f, 0.227f, 0.373f); // #1E3A5F
    public Color bleuRoyal02 = new Color(0.035f, 0.184f, 0.369f); // #092F5E

    [Header("Bordeaux")]
    public Color bordeaux01 = new Color(0.290f, 0.118f, 0.180f); // #4A1E2E
    public Color bordeaux02 = new Color(0.369f, 0.004f, 0.227f); // #5E013A
    public Color bordeaux03 = new Color(0.573f, 0.125f, 0.306f); // #921F4E

    [Header("Vert forêt")]
    public Color vertForet = new Color(0.118f, 0.290f, 0.180f); // #1E4A2E
}