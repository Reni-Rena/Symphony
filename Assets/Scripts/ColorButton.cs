using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class ColorButton : MonoBehaviour
{
    [Header("Couleurs")]
    public GamePalette palette;
    void Start()
    {
        InitializedColor();
    }
    public void InitializedColor()
    {
        if (palette == null) return;
        GetComponent<Image>().color = palette.encreNoire02;
        GetComponentInChildren<TextMeshProUGUI>().color = palette.parchemin;
    }
}