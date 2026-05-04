using UnityEngine;
using UnityEngine.UI;

public class HandPrimitiveDeleteBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject handGrabProxy;

    [Header("Hover Settings")]
    [SerializeField] private float requiredHoverTime = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private float heartbeatInterval = 1.0f;

    private float hoverTimer = 0f;
    private float nextHeartbeatTime = 0f;

    private Transform currentDeleteRoot;
    private Button currentButton;

    private void Update()
    {
        if (debugLogs && Time.time >= nextHeartbeatTime)
        {
            nextHeartbeatTime = Time.time + heartbeatInterval;
            Debug.Log("[DeleteBridge] Update running on " + gameObject.name);
        }

        DetectDeleteButtonFromProxyBounds();

        if (currentDeleteRoot == null || currentButton == null)
        {
            hoverTimer = 0f;
            return;
        }

        hoverTimer += Time.deltaTime;

        if (debugLogs)
        {
            Debug.Log("[DeleteBridge] Hovering " + currentDeleteRoot.name +
                      " : " + hoverTimer.ToString("F2") + " / " + requiredHoverTime.ToString("F2"));
        }

        if (hoverTimer >= requiredHoverTime)
        {
            if (currentButton.interactable)
            {
                Debug.Log("[DeleteBridge] DELETE TRIGGERED on " + currentButton.name);
                currentButton.onClick.Invoke();
            }

            hoverTimer = 0f;
            currentDeleteRoot = null;
            currentButton = null;
        }
    }

    private void DetectDeleteButtonFromProxyBounds()
    {
        if (handGrabProxy == null)
        {
            if (debugLogs)
                Debug.LogWarning("[DeleteBridge] No handGrabProxy assigned.");
            return;
        }

        Collider[] proxyColliders = handGrabProxy.GetComponentsInChildren<Collider>(true);
        if (proxyColliders == null || proxyColliders.Length == 0)
        {
            if (debugLogs)
                Debug.LogWarning("[DeleteBridge] No colliders found on handGrabProxy.");
            return;
        }

        Transform foundRoot = null;
        Button foundButton = null;

        foreach (Collider proxyCol in proxyColliders)
        {
            if (proxyCol == null || !proxyCol.enabled)
                continue;

            Bounds b = proxyCol.bounds;

            Collider[] hits = Physics.OverlapBox(
                b.center,
                b.extents,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide);

            if (debugLogs && hits.Length > 0)
            {
                string hitNames = "";
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i] != null)
                        hitNames += hits[i].name + " | ";
                }

                Debug.Log("[DeleteBridge] Proxy overlap hits: " + hitNames);
            }

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                Button button = FindButtonInParents(hit.transform);
                if (button == null)
                    continue;

                Transform deleteRoot = FindDeleteButtonRoot(button.transform);
                if (deleteRoot == null)
                    continue;

                foundRoot = deleteRoot;
                foundButton = button;
                break;
            }

            if (foundRoot != null)
                break;
        }

        if (foundRoot != currentDeleteRoot)
        {
            hoverTimer = 0f;

            if (debugLogs && foundRoot != null)
            {
                Debug.Log("[DeleteBridge] New delete target detected: " + foundRoot.name);
            }

            currentDeleteRoot = foundRoot;
            currentButton = foundButton;
        }
    }

    private Button FindButtonInParents(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            Button button = current.GetComponent<Button>();
            if (button != null)
                return button;

            current = current.parent;
        }

        return null;
    }

    private Transform FindDeleteButtonRoot(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            if (current.name.ToLower().Contains("deletebutton"))
                return current;

            current = current.parent;
        }

        return null;
    }
}