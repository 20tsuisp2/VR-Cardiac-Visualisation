using UnityEngine;

public class HandRotationResetHandle : MonoBehaviour
{
    [SerializeField] private Transform rotationTarget;

    public Transform RotationTarget => rotationTarget;

    private void Reset()
    {
        if (rotationTarget == null && transform.parent != null)
        {
            rotationTarget = transform.parent;
        }
    }
}