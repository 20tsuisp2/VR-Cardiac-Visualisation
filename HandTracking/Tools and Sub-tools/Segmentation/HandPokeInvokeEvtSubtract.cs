using System.Reflection;
using UnityEngine;

public class HandPokeInvokeEvtSubtract : MonoBehaviour
{
    private Component _toolPanelEvtComponent;
    private bool _handInside = false;

    private void Awake()
    {
        FindToolPanelEvt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_handInside)
            return;

        if (!other.name.ToLower().Contains("handgrabproxy"))
            return;

        _handInside = true;

        if (_toolPanelEvtComponent == null)
            FindToolPanelEvt();

        if (_toolPanelEvtComponent != null)
        {
            MethodInfo method = _toolPanelEvtComponent.GetType().GetMethod(
                "ToggleEvtModeAddSubtract",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method != null)
            {
                method.Invoke(_toolPanelEvtComponent, null);
                Debug.Log("Hand invoked EVT add/subtract toggle");
            }
            else
            {
                Debug.LogWarning("ToggleEvtModeAddSubtract method not found on ToolPanelEvt");
            }
        }
        else
        {
            Debug.LogWarning("ToolPanelEvt component not found");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.ToLower().Contains("handgrabproxy"))
            return;

        _handInside = false;
    }

    private void FindToolPanelEvt()
    {
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        foreach (MonoBehaviour mb in allBehaviours)
        {
            if (mb == null)
                continue;

            if (mb.GetType().Name == "ToolPanelEvt")
            {
                _toolPanelEvtComponent = mb;
                Debug.Log("Found ToolPanelEvt on object: " + mb.gameObject.name);
                return;
            }
        }
    }
}