using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotsManager : MonoBehaviour
{
    private List<int> activePositions;
    private SlotController[] slotControllers;
    private Music music;
    [SerializeField] private KeyCode keyCode;
    private SlotController lastSlot;
    private KnobsManager knobsManager;

    // Start is called before the first frame update
    void Start()
    {
        music = FindObjectOfType<Music>();
        activePositions = new List<int>();
        slotControllers = GetComponentsInChildren<SlotController>();
        music.OnBeat += onBeat;
        knobsManager = FindObjectOfType<KnobsManager>();

        lastSlot = slotControllers[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyCode)) {
            guessSlot();
        }
    }

    private void guessSlot() {
        lastSlot.guess(activePositions.Contains(0));
    }

    private void onBeat(int beat) {
        increatePositions();
    }

    private void increatePositions()
    {
        List<int> newPositions = new List<int>();
        foreach (int position in activePositions)
        {
            int nextPosition = position - 1; // Decrementa siempre la posición
            if (nextPosition >= 0)
            { // Solo añade la posición si sigue siendo válida (mayor o igual a 0)
                newPositions.Add(nextPosition);
            }
        }
        activePositions = newPositions;
        updateSlotsVisuals();
    }

    private void updateSlotsVisuals() {
        float normalizedCorrectKnobsCount = knobsManager.GetCorrectKnobsNormalized();
        int correctSlotsCount = (int)(normalizedCorrectKnobsCount * slotControllers.Length);

        for (int i = 0; i < slotControllers.Length; i++)
        {
            SlotController slotController = slotControllers[i];
            
            if (activePositions.Contains(i))
            {
                slotController.activate();
            }
            else
            {
                slotController.deactivate();
            }

            if (i > correctSlotsCount) {
                slotController.block();
            }

        }
    }

    public void addActiveSlot() {
        activePositions.Add(slotControllers.Length - 1);
    }
}
