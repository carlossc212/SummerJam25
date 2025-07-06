// UnityMainThreadDispatcher.cs
using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnityMainThreadDispatcher>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("UnityMainThreadDispatcher");
                    _instance = go.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(go); // Keep it alive across scenes
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Ensure only one instance
        }
    }

    public void Enqueue(Action action)
    {
        if (action == null) return;
        _queue.Enqueue(action);
    }

    private void Update()
    {
        while (_queue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }
}