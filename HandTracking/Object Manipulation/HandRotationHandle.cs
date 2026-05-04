using UnityEngine;

public class HandRotationHandle : MonoBehaviour
{
    public enum RotationAxis
    {
        Yaw,
        Pitch
    }

    [Header("Rotation Handle")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Yaw;
    [SerializeField] private Transform rotationTarget;

    public RotationAxis Axis => rotationAxis;
    public Transform RotationTarget => rotationTarget;

    private void Reset()
    {
        if (rotationTarget == null && transform.parent != null)
        {
            rotationTarget = transform.parent;
        }
    }
}