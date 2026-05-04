using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DisableParentInteractableOnHover : MonoBehaviour
{
    public XRBaseInteractable parentInteractable;

    private XRBaseInteractable thisInteractable;

    void Start()
    {
        thisInteractable = GetComponent<XRBaseInteractable>();

        if (thisInteractable != null)
        {
            thisInteractable.hoverEntered.AddListener(OnHoverEntered);
            thisInteractable.hoverExited.AddListener(OnHoverExited);
        }
    }

    void OnDestroy()
    {
        if (thisInteractable != null)
        {
            thisInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            thisInteractable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (parentInteractable != null)
            parentInteractable.enabled = false;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (parentInteractable != null)
            parentInteractable.enabled = true;
    }
}