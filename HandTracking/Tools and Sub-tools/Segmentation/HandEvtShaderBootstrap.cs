using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class HandEvtShaderBootstrap : MonoBehaviour
{
    [SerializeField] private string volumeObjectName = "VolumeObject";
    [SerializeField] private float retryInterval = 0.5f;

    private float _nextRetryTime = 0f;
    private bool _done = false;

    private void OnEnable()
    {
        EventManager.StartListening("VolumeRenderCreated", TryEnsureEvtShaderControlExists);
    }

    private void OnDisable()
    {
        EventManager.StopListening("VolumeRenderCreated", TryEnsureEvtShaderControlExists);
    }

    private void Start()
    {
        TryEnsureEvtShaderControlExists();
    }

    private void Update()
    {
        if (_done)
            return;

        if (Time.time >= _nextRetryTime)
        {
            _nextRetryTime = Time.time + retryInterval;
            TryEnsureEvtShaderControlExists();
        }
    }

    public void TryEnsureEvtShaderControlExists()
    {
        GameObject volumeObject = GameObject.Find(volumeObjectName);
        if (volumeObject == null)
        {
            Debug.Log("HandEvtShaderBootstrap: VolumeObject not found yet.");
            return;
        }

        Component existing = volumeObject.GetComponents<MonoBehaviour>()
            .FirstOrDefault(c => c != null && c.GetType().Name == "EvtShaderControl");

        if (existing != null)
        {
            Debug.Log("HandEvtShaderBootstrap: EvtShaderControl already present.");
            _done = true;
            return;
        }

        Type evtType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
            })
            .FirstOrDefault(t => t != null && t.Name == "EvtShaderControl");

        if (evtType == null)
        {
            Debug.LogError("HandEvtShaderBootstrap: Could not find EvtShaderControl type.");
            return;
        }

        volumeObject.AddComponent(evtType);
        Debug.Log("HandEvtShaderBootstrap: Added EvtShaderControl to VolumeObject.");
        _done = true;
    }
}