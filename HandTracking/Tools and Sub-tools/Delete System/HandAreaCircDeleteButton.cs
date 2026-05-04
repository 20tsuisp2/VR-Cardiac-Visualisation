using UnityEngine;

public class HandAreaCircDeleteButton : MonoBehaviour
{
    [SerializeField] private string validHandColliderName = "HandGrabProxy";

    private GameObject measurementRoot;
    private bool alreadyTriggered = false;

    public void SetMeasurementRoot(GameObject root)
    {
        measurementRoot = root;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered)
            return;

        if (!other.name.Contains(validHandColliderName))
            return;

        alreadyTriggered = true;

        if (measurementRoot != null)
        {
            Debug.Log("[HandAreaCircDeleteButton] Deleting " + measurementRoot.name);
            Destroy(measurementRoot);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains(validHandColliderName))
            return;

        alreadyTriggered = false;
    }
}