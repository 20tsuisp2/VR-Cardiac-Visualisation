using UnityEngine;

public class HandPlacedDeviceDeleteButton : MonoBehaviour
{
    [SerializeField] private GameObject targetRoot;

    public void SetTargetRoot(GameObject root)
    {
        targetRoot = root;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetRoot == null)
            return;

        if (other.GetComponent<HandGrabProxyMove>() != null)
        {
            Destroy(targetRoot);
        }
    }
}