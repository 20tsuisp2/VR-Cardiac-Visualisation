using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GazeDwellRotateToggle : MonoBehaviour
{
    public float dwellTime = 1.2f;
    public GazeInteractionModeManager modeManager;

    private float timer = 0f;
    private bool gazing = false;
    private XRBaseInteractable interactable;

    void Start()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    void Update()
    {
        if (!gazing)
            return;

        timer += Time.deltaTime;

        float progress = timer / dwellTime;
        if (DwellIndicatorController.Instance != null)
            DwellIndicatorController.Instance.SetProgress(progress);

        if (timer >= dwellTime)
        {
            if (modeManager != null)
                modeManager.ToggleYawModeExclusive();

            gazing = false;
            timer = 0f;

            if (DwellIndicatorController.Instance != null)
                DwellIndicatorController.Instance.Hide();
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        gazing = true;
        timer = 0f;

        if (DwellIndicatorController.Instance != null)
            DwellIndicatorController.Instance.SetProgress(0f);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        gazing = false;
        timer = 0f;

        if (DwellIndicatorController.Instance != null)
            DwellIndicatorController.Instance.Hide();
    }
}