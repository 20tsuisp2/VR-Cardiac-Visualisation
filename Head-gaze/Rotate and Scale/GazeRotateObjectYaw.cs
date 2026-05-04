using UnityEngine;

public class GazeRotateObjectYaw : MonoBehaviour
{
    public Transform cameraTransform;
    public float rotationSensitivity = 1.0f;
    public float rotationSmoothness = 8f;

    private bool isRotateMode = false;
    private float initialHeadTilt;
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

        float currentHeadTilt = cameraTransform.eulerAngles.z;
        float tiltDelta = Mathf.DeltaAngle(initialHeadTilt, currentHeadTilt);

        Quaternion desiredRotation =
            initialObjectRotation * Quaternion.Euler(0f, tiltDelta * rotationSensitivity, 0f);

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
            initialHeadTilt = cameraTransform.eulerAngles.z;
            initialObjectRotation = transform.rotation;
            targetRotation = transform.rotation;
        }
    }

    public bool IsRotateMode()
    {
        return isRotateMode;
    }
}
