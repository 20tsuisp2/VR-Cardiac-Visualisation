using System.Collections.Generic;
using UnityEngine;

public class RotationHistory : MonoBehaviour
{
    public Transform targetObject;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private bool hasInitialState = false;

    private Stack<TransformState> history = new Stack<TransformState>();

    private struct TransformState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformState(Vector3 p, Quaternion r, Vector3 s)
        {
            position = p;
            rotation = r;
            scale = s;
        }
    }

    void Start()
    {
        TryCaptureInitialState();
    }

    public void SetTarget(Transform newTarget)
    {
        targetObject = newTarget;
        history.Clear();
        TryCaptureInitialState();
    }

    private void TryCaptureInitialState()
    {
        if (targetObject == null)
            return;

        initialPosition = targetObject.position;
        initialRotation = targetObject.rotation;
        initialScale = targetObject.localScale;
        hasInitialState = true;
    }

    public void SaveCurrentState()
    {
        if (targetObject == null)
            return;

        history.Push(new TransformState(
            targetObject.position,
            targetObject.rotation,
            targetObject.localScale
        ));
    }

    public void Undo()
    {
        if (targetObject == null || history.Count == 0)
            return;

        TransformState previous = history.Pop();
        targetObject.position = previous.position;
        targetObject.rotation = previous.rotation;
        targetObject.localScale = previous.scale;
    }

    public void ResetToInitial()
    {
        if (targetObject == null || !hasInitialState)
            return;

        SaveCurrentState();

        targetObject.position = initialPosition;
        targetObject.rotation = initialRotation;
        targetObject.localScale = initialScale;
    }

    public void ResetScaleOnly()
    {
        if (targetObject == null || !hasInitialState)
            return;

        SaveCurrentState();
        targetObject.localScale = initialScale;
    }
}
