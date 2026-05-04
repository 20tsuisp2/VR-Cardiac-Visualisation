using UnityEngine;

public class RotationPanelController : MonoBehaviour
{
    public Transform rotationTarget;

    private ParentMoveScaleCentre scaleController;
    private Transform scaleTarget;

    [Header("History")]
    public RotationHistory rotationHistory;
    public RotationHistory scaleHistory;

    void Start()
    {
        FindScaleTarget();
    }

    void FindScaleTarget()
    {
        scaleController = FindObjectOfType<ParentMoveScaleCentre>();

        if (scaleController != null)
        {
            scaleTarget = scaleController.transform;

            if (scaleHistory != null)
                scaleHistory.SetTarget(scaleTarget);
        }
        else
        {
            Debug.LogWarning("ParentMoveScaleCentre not found in scene.");
        }
    }

    public void RotateYaw(float degrees)
    {
        if (rotationTarget == null)
            return;

        SaveRotationState();
        rotationTarget.Rotate(0f, degrees, 0f, Space.World);
    }

    public void RotateRoll(float degrees)
    {
        if (rotationTarget == null)
            return;

        SaveRotationState();
        rotationTarget.Rotate(0f, 0f, degrees, Space.Self);
    }

    public void ScaleUniform(float factor)
    {
        if (scaleController == null || scaleTarget == null)
            FindScaleTarget();

        if (scaleController == null || scaleTarget == null)
            return;

        SaveScaleState();

        Vector3 newScale = scaleTarget.localScale * factor;
        newScale = Vector3.Min(Vector3.one * scaleController.MaximumScale, newScale);
        newScale = Vector3.Max(Vector3.one * scaleController.MinimumScale, newScale);

        scaleTarget.localScale = newScale;
    }

    public void UndoLastRotation()
    {
        if (rotationHistory != null)
            rotationHistory.Undo();
    }

    public void ResetRotation()
    {
        if (rotationHistory != null)
            rotationHistory.ResetToInitial();
    }

    public void UndoLastScale()
    {
        if (scaleHistory != null)
            scaleHistory.Undo();
    }

    public void ResetScaleOnly()
    {
        if (scaleHistory != null)
            scaleHistory.ResetScaleOnly();
    }

    private void SaveRotationState()
    {
        if (rotationHistory != null)
            rotationHistory.SaveCurrentState();
    }

    private void SaveScaleState()
    {
        if (scaleHistory != null)
            scaleHistory.SaveCurrentState();
    }
}
