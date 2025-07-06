using UnityEngine;
using FMODUnity;
using System;

// Ya no necesitamos System.Collections para las coroutines aqu�,
// ya que el dispatcher se encarga de eso.
// Si tienes otras coroutines en este script, mant�n 'using System.Collections;'.

public class Music : MonoBehaviour
{
    public event Action<int> OnBeat;
    public event Action<bool> OnWin;

    [SerializeField] private string m_Reference;
    private FMOD.Studio.EventInstance instance;
    private BeatSystem bS;

    public void StartMusic()
    {
        bS = GetComponent<BeatSystem>();
        if (bS == null)
        {
            Debug.LogError("Music: BeatSystem component not found on this GameObject! Cannot start music callbacks. Make sure BeatSystem is on the same GameObject as Music, or configured to be found.");
            return;
        }

        bS.OnBeat += HandleBeatEvent;
        // Suscribimos FinishSongSafe al evento OnSongFinished del BeatSystem
        bS.OnSongFinished += FinishSongSafe;
        Debug.Log("Music: Subscribed to BeatSystem events (OnBeat, OnSongFinished).");

        // Creamos la instancia de FMOD
        instance = FMODUnity.RuntimeManager.CreateInstance(m_Reference);

        // Verificamos que la instancia de FMOD sea v�lida antes de usarla
        if (!instance.isValid())
        {
            Debug.LogError("Music: Failed to create FMOD EventInstance. It might be invalid or the path is incorrect. Check FMOD EventReference.");
            return;
        }

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        instance.start();
        bS.AssignBeatEvent(instance); // Asignamos los callbacks de FMOD al BeatSystem
        Debug.Log("Music: FMOD music started and BeatSystem callbacks assigned.");
    }

    public void stopAndClear()
    {
        // Asegurarse de que BeatSystem existe antes de intentar usarlo
        if (bS != null)
        {
            bS.StopAndClear(instance);
        }
        else
        {
            Debug.LogWarning("Music: BeatSystem reference is null. Attempting to release FMOD instance directly.");
            // Si BeatSystem es nulo, liberamos la instancia directamente como respaldo
            if (instance.isValid())
            {
                instance.release();
            }
        }
        Debug.Log("Music: FMOD instance stopped and cleared.");
    }

    // M�todo que ser� llamado cuando los eventos de beat se disparen
    private void HandleBeatEvent(int currentBeat)
    {
        if (!Application.isPlaying) return; // Evitar llamadas en el editor cuando no se est� ejecutando
        OnBeat?.Invoke(currentBeat);
        // Debug.Log($"Music: Beat received: {currentBeat}"); // Descomentar para depurar beats, puede ser ruidoso
    }

    // Este m�todo es llamado por BeatSystem cuando la canci�n termina
    private void FinishSongSafe()
    {
        Debug.Log("Music: FinishSongSafe() called. This is triggered by BeatSystem.OnSongFinished.");

        // Primero, detiene y limpia la instancia de FMOD inmediatamente.
        // Esto libera los recursos de audio.
        stopAndClear();

        // Encolamos la invocaci�n del evento OnWin en el dispatcher para que se ejecute en el hilo principal
        if (UnityMainThreadDispatcher.Instance != null)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                Debug.Log("Music: Invoking OnWin event on Main Thread via Dispatcher.");
                OnWin?.Invoke(true); // Ahora es seguro invocar el evento OnWin
            });
        }
        else
        {
            Debug.LogError("Music: UnityMainThreadDispatcher instance is null! Cannot invoke OnWin safely. Make sure it's in your scene.");
            // Fallback: Invocar directamente (RIESGOSO, podr�a causar el congelamiento si no estamos en el hilo principal)
            OnWin?.Invoke(true);
        }
    }

    // Importante: Desuscribirse de los eventos cuando el objeto se destruye
    private void OnDestroy()
    {
        // Desuscribirse de los eventos de BeatSystem para evitar referencias nulas y fugas de memoria
        if (bS != null)
        {
            bS.OnBeat -= HandleBeatEvent;
            bS.OnSongFinished -= FinishSongSafe;
            Debug.Log("Music: Unsubscribed from BeatSystem events in OnDestroy.");
        }

        // Asegurarse de que la instancia de FMOD se libere si a�n es v�lida
        if (instance.isValid())
        {
            instance.release();
            Debug.Log("Music: FMOD instance released in OnDestroy.");
        }
    }
}