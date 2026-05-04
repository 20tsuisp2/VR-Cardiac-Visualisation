using UnityEngine;

public class GazeMoveObject : MonoBehaviour
{
    public Transform cameraTransform;
    public float followDistance = 0.6f;
    public float followSmoothness = 6f;

    private bool isMoveMode = false;

    void Update()
    {
        if (!isMoveMode || cameraTransform == null)
            return;

        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * followDistance;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * followSmoothness
        );
    }

    public void ToggleMoveMode()
    {
        isMoveMode = !isMoveMode;
    }

    public bool IsMoveMode()
    {
        return isMoveMode;
    }
}