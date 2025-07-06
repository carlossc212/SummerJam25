using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocoController : MonoBehaviour
{
    private Music music;
    private Color[] colors;
    private int actualColor = 0;
    private Light light;
    [SerializeField] private int offSet;

    private void Start()
    {
        actualColor = offSet;
        light = GetComponentInChildren<Light>();
        music = FindObjectOfType<Music>();
        colors = new Color[] // Initialize the array
        {
            Color.blue,
            Color.red,
            Color.green,
            Color.cyan,
            Color.magenta,
            Color.yellow
        };
        music.OnBeat += changeColor;
        light.color = colors[actualColor];
    }

    private void changeColor(int beat) {
        if (actualColor == colors.Length - 1)
        {
            actualColor = 0;
        }
        else {
            actualColor++;
        }
        light.color = colors[actualColor];
    }
}
