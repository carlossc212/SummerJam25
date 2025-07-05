using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraSlotsManager : MonoBehaviour
{
    private SlotsManager[] slotsManagers;
    private Music music;
    private int lastInt;

    // Start is called before the first frame update
    void Start()
    {
        music = FindObjectOfType<Music>();
        slotsManagers = GetComponentsInChildren<SlotsManager>();
        music.OnBeat += onBeat;
    }

    private void onBeat(int currentBeat) {
        addActiveSlot();

    }

    private void addActiveSlot() {
        if (slotsManagers.Length == 0) {
            return;
        }

        int randInt = Random.Range(0, slotsManagers.Length);
        if (randInt == lastInt) {
            addActiveSlot();
            return;
        }

        slotsManagers[randInt].addActiveSlot();

        lastInt = randInt;
    }
}
