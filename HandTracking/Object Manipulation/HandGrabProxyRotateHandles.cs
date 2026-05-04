using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandGrabProxyRotateHandles : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField] private bool useLeftHand = false;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothness = 12f;

    [Header("References")]
    [SerializeField] private HandGrabProxyMove handGrabProxyMove;

    [Header("Hover Highlight")]
    [SerializeField] private float normalHandleScale = 1f;
    [SerializeField] private float hoveredHandleScale = 1.35f;

    private XRHandSubsystem _handSubsystem;

    private readonly List<HandRotationHandle> _handleStack = new List<HandRotationHandle>();
    private HandRotationHandle _hoveredHandle;
    private HandRotationHandle _activeHandle;

    private bool _wasPinchingLastFrame = false;
    private bool _isRotating = false;

    private Quaternion _initialObjectRotation;
    private Quaternion _targetRotation;

    private Vector3 _initialPitchAxis;
    private Vector3 _initialHandVectorFromTarget;

    private Vector3 _initialYawAxis;
    private Vector3 _initialYawVectorFromTarget;

    private void OnEnable()
    {
        _handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        if (_handSubsystem == null)
        {
            Debug.LogError("HandGrabProxyRotateHandles: No XRHandSubsystem found.");
        }

        if (handGrabProxyMove == null)
        {
            handGrabProxyMove = GetComponent<HandGrabProxyMove>();
        }
    }

    private void Update()
    {
        bool isPinching = ReadPinchState();

        if (isPinching && !_wasPinchingLastFrame)
        {
            TryBeginRotation();
        }

        if (_isRotating && isPinching)
        {
            UpdateRotation();
        }

        if (!isPinching && _wasPinchingLastFrame)
        {
            EndRotation();
        }

        UpdateHandleHighlight();

        _wasPinchingLastFrame = isPinching;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandRotationHandle handle = other.GetComponent<HandRotationHandle>();
        if (handle == null)
        {
            return;
        }

        if (!_handleStack.Contains(handle))
        {
            _handleStack.Add(handle);
        }

        if (!_isRotating)
        {
            _hoveredHandle = handle;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HandRotationHandle handle = other.GetComponent<HandRotationHandle>();
        if (handle == null)
        {
            return;
        }

        _handleStack.Remove(handle);

        if (_isRotating)
        {
            return;
        }

        if (_hoveredHandle == handle)
        {
            _hoveredHandle = _handleStack.Count > 0 ? _handleStack[_handleStack.Count - 1] : null;
        }
    }

    private void TryBeginRotation()
    {
        if (_hoveredHandle == null)
        {
            return;
        }

        if (_hoveredHandle.RotationTarget == null)
        {
            Debug.LogWarning("HandGrabProxyRotateHandles: Rotation handle has no target.");
            return;
        }

        _isRotating = true;
        _activeHandle = _hoveredHandle;

        _initialObjectRotation = _activeHandle.RotationTarget.rotation;
        _targetRotation = _initialObjectRotation;

        if (_activeHandle.Axis == HandRotationHandle.RotationAxis.Pitch)
        {
            _initialPitchAxis = _activeHandle.RotationTarget.forward;
            _initialHandVectorFromTarget =
                transform.position - _activeHandle.RotationTarget.position;
        }

        if (_activeHandle.Axis == HandRotationHandle.RotationAxis.Yaw)
        {
            _initialYawAxis = Vector3.up;
            _initialYawVectorFromTarget =
                transform.position - _activeHandle.RotationTarget.position;
        }

        if (handGrabProxyMove != null)
        {
            handGrabProxyMove.enabled = false;
        }
    }

    private void UpdateRotation()
    {
        if (_activeHandle == null || _activeHandle.RotationTarget == null)
        {
            return;
        }

        Quaternion desiredRotation = _initialObjectRotation;

        if (_activeHandle.Axis == HandRotationHandle.RotationAxis.Yaw)
        {
            Vector3 currentHandVectorFromTarget =
                transform.position - _activeHandle.RotationTarget.position;

            Vector3 initialProjected =
                Vector3.ProjectOnPlane(_initialYawVectorFromTarget, _initialYawAxis).normalized;

            Vector3 currentProjected =
                Vector3.ProjectOnPlane(currentHandVectorFromTarget, _initialYawAxis).normalized;

            if (initialProjected.sqrMagnitude > 0.0001f &&
                currentProjected.sqrMagnitude > 0.0001f)
            {
                float yawDelta = Vector3.SignedAngle(
                    initialProjected,
                    currentProjected,
                    _initialYawAxis
                );

                desiredRotation =
                    Quaternion.AngleAxis(yawDelta, _initialYawAxis) *
                    _initialObjectRotation;
            }
        }
        else if (_activeHandle.Axis == HandRotationHandle.RotationAxis.Pitch)
        {
            Vector3 currentHandVectorFromTarget =
                transform.position - _activeHandle.RotationTarget.position;

            Vector3 initialProjected =
                Vector3.ProjectOnPlane(_initialHandVectorFromTarget, _initialPitchAxis).normalized;

            Vector3 currentProjected =
                Vector3.ProjectOnPlane(currentHandVectorFromTarget, _initialPitchAxis).normalized;

            if (initialProjected.sqrMagnitude > 0.0001f &&
                currentProjected.sqrMagnitude > 0.0001f)
            {
                float pitchDelta = Vector3.SignedAngle(
                    initialProjected,
                    currentProjected,
                    _initialPitchAxis
                );

                desiredRotation =
                    Quaternion.AngleAxis(pitchDelta, _initialPitchAxis) *
                    _initialObjectRotation;
            }
        }

        _targetRotation = desiredRotation;

        _activeHandle.RotationTarget.rotation = Quaternion.Slerp(
            _activeHandle.RotationTarget.rotation,
            _targetRotation,
            Time.deltaTime * rotationSmoothness
        );
    }

    private void EndRotation()
    {
        if (!_isRotating)
        {
            return;
        }

        _isRotating = false;
        _activeHandle = null;
        _hoveredHandle = _handleStack.Count > 0 ? _handleStack[_handleStack.Count - 1] : null;

        if (handGrabProxyMove != null)
        {
            handGrabProxyMove.enabled = true;
        }

        EventManager.TriggerEvent("ObjectMoved");
    }

    private void UpdateHandleHighlight()
    {
        HandRotationHandle[] allHandles = FindObjectsOfType<HandRotationHandle>(true);

        foreach (HandRotationHandle handle in allHandles)
        {
            if (handle == null)
            {
                continue;
            }

            float targetScale = normalHandleScale;

            if (!_isRotating && handle == _hoveredHandle)
            {
                targetScale = hoveredHandleScale;
            }

            handle.transform.localScale = Vector3.one * targetScale;
        }
    }

    private bool ReadPinchState()
    {
        if (_handSubsystem == null)
        {
            return false;
        }

        XRHand hand = useLeftHand ? _handSubsystem.leftHand : _handSubsystem.rightHand;

        if (!hand.isTracked)
        {
            return false;
        }

        XRHandJoint thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        XRHandJoint indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (!thumbTip.TryGetPose(out Pose thumbPose))
        {
            return false;
        }

        if (!indexTip.TryGetPose(out Pose indexPose))
        {
            return false;
        }

        float distance = Vector3.Distance(thumbPose.position, indexPose.position);
        return distance <= pinchDistanceThreshold;
    }
}