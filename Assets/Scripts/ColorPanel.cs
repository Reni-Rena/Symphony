using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class ColorPanel : MonoBehaviour
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
    }
}