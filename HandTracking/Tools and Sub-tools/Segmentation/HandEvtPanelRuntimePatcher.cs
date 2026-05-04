using UnityEngine;

public class HandEvtPanelRuntimePatcher : MonoBehaviour
{
    [SerializeField] private Transform evtPanelRoot;
    [SerializeField] private bool debugLogs = true;

    private bool _patchedSubtract = false;

    private void Update()
    {
        if (_patchedSubtract)
            return;

        if (evtPanelRoot == null)
            return;

        TryPatchSubtractControl();
    }

    private void TryPatchSubtractControl()
    {
        Transform subtractTarget = FindSubtractTarget(evtPanelRoot);

        if (subtractTarget == null)
            return;

        if (subtractTarget.GetComponent<HandPokeInvokeEvtSubtract>() == null)
        {
            subtractTarget.gameObject.AddComponent<HandPokeInvokeEvtSubtract>();

            if (debugLogs)
                Debug.Log("Patched EVT subtract control: " + subtractTarget.name);
        }

        _patchedSubtract = true;
    }

    private Transform FindSubtractTarget(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            string n = child.name.ToLower();

            if (n.Contains("subtract"))
                return child;

            if (n.Contains("sub") && n.Contains("evt"))
                return child;

            if (n.Contains("mode") && n.Contains("toggle"))
                return child;
        }

        return null;
    }
}