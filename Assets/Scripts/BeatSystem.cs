using System;
using System.Runtime.InteropServices;
using UnityEngine;

class BeatSystem : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    class TimelineInfo
    {
        public int currentMusicBeat = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
        public BeatSystem beatSystemInstance; // Reference to the BeatSystem instance
    }

    TimelineInfo timelineInfo;
    GCHandle timelineHandle;

    FMOD.Studio.EVENT_CALLBACK beatCallback;

    public static int beat;
    public static string marker;

    public event Action<int> OnBeat;
    public event Action<string> OnMarker;
    public event Action OnSongFinished;

    public void AssignBeatEvent(FMOD.Studio.EventInstance instance)
    {
        // Ensure that the instance is valid before proceeding
        if (!instance.isValid())
        {
            Debug.LogError("BeatSystem: Attempted to assign event to an invalid FMOD EventInstance.");
            return;
        }

        timelineInfo = new TimelineInfo();
        timelineInfo.beatSystemInstance = this; // Store reference to this script instance
        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);

        instance.setUserData(GCHandle.ToIntPtr(timelineHandle));
        instance.setCallback(beatCallback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT |
            FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER |
            FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED);

        Debug.Log("BeatSystem: FMOD Event callbacks assigned.");
    }

    public void StopAndClear(FMOD.Studio.EventInstance instance)
    {
        if (instance.isValid()) // Only act if the instance is still valid
        {
            instance.setUserData(IntPtr.Zero);
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            Debug.Log("BeatSystem: FMOD EventInstance stopped and released.");
        }
        else
        {
            Debug.LogWarning("BeatSystem: Attempted to stop and clear an invalid FMOD EventInstance.");
        }

        if (timelineHandle.IsAllocated)
        {
            timelineHandle.Free();
            Debug.Log("BeatSystem: timelineHandle freed.");
        }
    }

    private void TriggerOnBeat(int currentBeat)
    {
        OnBeat?.Invoke(currentBeat);
    }

    private void TriggerOnMarker(string markerName)
    {
        OnMarker?.Invoke(markerName);
    }

    private void TriggerOnSongFinished()
    {
        Debug.Log("BeatSystem: TriggerOnSongFinished() called. Invoking OnSongFinished event.");
        OnSongFinished?.Invoke();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);
        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);

        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("BeatSystem FMOD Callback Error: Failed to get user data - " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero)
        {
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            // CRITICAL: Ensure beatSystem is not null before accessing its methods
            BeatSystem beatSystem = timelineInfo.beatSystemInstance;
            if (beatSystem == null)
            {
                Debug.LogError("BeatSystem FMOD Callback Error: beatSystemInstance is null in TimelineInfo.");
                return FMOD.RESULT.ERR_INTERNAL;
            }

            switch (type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        timelineInfo.currentMusicBeat = parameter.beat;
                        beat = timelineInfo.currentMusicBeat;
                        beatSystem.TriggerOnBeat(timelineInfo.currentMusicBeat);
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                        marker = timelineInfo.lastMarker;
                        beatSystem.TriggerOnMarker(timelineInfo.lastMarker);
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED:
                    {
                        // THIS IS THE KEY DEBUG.LOG: It MUST appear if FMOD event stops.
                        Debug.Log("BeatSystem FMOD Callback: Received STOPPED type. Song has finished or was stopped.");
                        beatSystem.TriggerOnSongFinished(); // Call the non-static method on the instance
                    }
                    break;
            }
        }
        else
        {
            Debug.LogWarning("BeatSystem FMOD Callback Warning: timelineInfoPtr is IntPtr.Zero. User data might not be set.");
        }
        return FMOD.RESULT.OK;
    }
}