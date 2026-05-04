using UnityEngine;

public class PalmPanelShowHide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform headsetTransform;

    [Header("Palm Facing Settings")]
    [SerializeField] private Vector3 palmFacingDirectionLocal = Vector3.up;
    [SerializeField] private float showThreshold = 0.55f;
    [SerializeField] private float hideThreshold = 0.35f;

    private bool _isShowing = true;

    private void Start()
    {
        if (headsetTransform == null && Camera.main != null)
        {
            headsetTransform = Camera.main.transform;
        }

        SetPanelVisible(false);
    }

    private void Update()
    {
        if (panelRoot == null || headsetTransform == null)
        {
            return;
        }

        Vector3 palmWorldDirection =
            transform.TransformDirection(palmFacingDirectionLocal).normalized;
        Vector3 directionToHeadset =
            (headsetTransform.position - transform.position).normalized;

        float dot = Vector3.Dot(palmWorldDirection, directionToHeadset);

        if (!_isShowing && dot <= -showThreshold)
        {
            SetPanelVisible(true);
        }
        else if (_isShowing && dot >= -hideThreshold)
        {
            SetPanelVisible(false);
        }
    }

    private void SetPanelVisible(bool visible)
    {
        _isShowing = visible;

        Canvas[] canvases = panelRoot.GetComponentsInChildren<Canvas>(true);
        Renderer[] renderers = panelRoot.GetComponentsInChildren<Renderer>(true);
        Collider[] colliders = panelRoot.GetComponentsInChildren<Collider>(true);

        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = visible;
        }

        foreach (Renderer rend in renderers)
        {
            rend.enabled = visible;
        }

        foreach (Collider col in colliders)
        {
            col.enabled = visible;
        }
    }
}