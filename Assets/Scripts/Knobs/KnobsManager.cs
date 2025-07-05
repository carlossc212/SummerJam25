using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobsManager : MonoBehaviour
{
    int lastRandomKnob = 0;
    private Music music;
    private KnobController[] knobs;

    // Start is called before the first frame update
    void Start()
    {
        music = FindObjectOfType<Music>();
        knobs = GetComponentsInChildren<KnobController>();

        music.OnBeat += OnBeat;
    }

    private void OnBeat(int actualBeat) {
        if (2 % actualBeat == 0)
        {
            changeKnob();
        }
    }

    private void changeKnob() {
        int randomInt = Random.Range(0, knobs.Length);
        if (randomInt == lastRandomKnob)
        {
            changeKnob();
            return;
        }

        knobs[randomInt].updateAskedColor();
        lastRandomKnob = randomInt;
    }

    public float GetCorrectKnobsNormalized()
    {
        if (knobs == null || knobs.Length == 0)
        {
            Debug.LogWarning("KnobsManager no tiene perillas asignadas. Retornando 0.");
            return 0f;
        }

        int correctKnobsCount = 0;
        foreach (KnobController knob in knobs)
        {
            if (knob.IsCorrectlyColored()) // Usamos la nueva función del KnobController
            {
                correctKnobsCount++;
            }
        }

        // Normaliza el contador de perillas correctas
        return (float)correctKnobsCount / knobs.Length;
    }
}
