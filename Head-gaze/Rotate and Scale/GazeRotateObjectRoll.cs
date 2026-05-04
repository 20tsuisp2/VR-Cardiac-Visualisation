using UnityEngine;

public class GazeRotateObjectRoll : MonoBehaviour
{
    public Transform cameraTransform;
    public float rotationSensitivity = 1.0f;
    public float rotationSmoothness = 8f;

    private bool isRotateMode = false;
    private float initialHeadRoll;
    private Quaternion initialObjectRotation;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (!isRotateMode || cameraTransform == null)
            return;

        float currentHeadRoll = cameraTransform.eulerAngles.z;
        float rollDelta = Mathf.DeltaAngle(initialHeadRoll, currentHeadRoll);

        Quaternion desiredRotation =
            initialObjectRotation * Quaternion.Euler(0f, 0f, rollDelta * rotationSensitivity);

        targetRotation = desiredRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothness
        );
    }

    public void ToggleRotateMode()
    {
        isRotateMode = !isRotateMode;

        if (isRotateMode && cameraTransform != null)
        {
            initialHeadRoll = cameraTransform.eulerAngles.z;
            initialObjectRotation = transform.rotation;
            targetRotation = transform.rotation;
        }
    }

    public bool IsRotateMode()
    {
        return isRotateMode;
    }
}
