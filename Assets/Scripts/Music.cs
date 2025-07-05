using UnityEngine;
using FMODUnity;
using System;

public class Music : MonoBehaviour
{
    public event Action<int> OnBeat;

    [SerializeField] private EventReference m_Reference;
    private FMOD.Studio.EventInstance instance;
    private BeatSystem bS;

    void Start()
    {
        bS = GetComponent<BeatSystem>();
        bS.OnBeat += HandleBeatEvent;

        instance = FMODUnity.RuntimeManager.CreateInstance(m_Reference.Path);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        instance.start();
        bS.AssignBeatEvent(instance);
    }

    public void stopAndClear() {
        bS.StopAndClear(instance);
    }

    // Métodos que serán llamados cuando los eventos se disparen
    private void HandleBeatEvent(int currentBeat)
    {
        OnBeat?.Invoke(currentBeat);
    }
}