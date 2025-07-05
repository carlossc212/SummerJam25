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
        // AÑADIDO: Referencia a la instancia de BeatSystem
        public BeatSystem beatSystemInstance;
    }

    TimelineInfo timelineInfo;
    GCHandle timelineHandle;

    FMOD.Studio.EVENT_CALLBACK beatCallback;

    public static int beat;
    public static string marker;

    // TUS EVENTOS: Ya estaban bien declarados
    public event Action<int> OnBeat;
    public event Action<string> OnMarker;

    public void AssignBeatEvent(FMOD.Studio.EventInstance instance)
    {
        timelineInfo = new TimelineInfo();
        // AÑADIDO: Guardar una referencia a esta instancia del script
        timelineInfo.beatSystemInstance = this;
        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        instance.setUserData(GCHandle.ToIntPtr(timelineHandle));
        instance.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
    }

    public void StopAndClear(FMOD.Studio.EventInstance instance)
    {
        instance.setUserData(IntPtr.Zero);
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        if (timelineHandle.IsAllocated)
        {
            timelineHandle.Free();
        }
    }

    // AÑADIDO: Métodos no estáticos para invocar los eventos
    // El callback estático los llamará a través de la instancia.
    private void TriggerOnBeat(int currentBeat)
    {
        OnBeat?.Invoke(currentBeat); // El '?' asegura que solo se invoque si hay suscriptores.
    }

    private void TriggerOnMarker(string markerName)
    {
        OnMarker?.Invoke(markerName);
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);
        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero)
        {
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            // AÑADIDO: Obtener la instancia del BeatSystem desde TimelineInfo
            BeatSystem beatSystem = timelineInfo.beatSystemInstance;

            switch (type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        timelineInfo.currentMusicBeat = parameter.beat;
                        beat = timelineInfo.currentMusicBeat;
                        // AÑADIDO: Invocar el evento a través de la instancia
                        beatSystem?.TriggerOnBeat(timelineInfo.currentMusicBeat);
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                        marker = timelineInfo.lastMarker;
                        // AÑADIDO: Invocar el evento a través de la instancia
                        beatSystem?.TriggerOnMarker(timelineInfo.lastMarker);
                    }
                    break;
            }
        }
        return FMOD.RESULT.OK;
    }
}