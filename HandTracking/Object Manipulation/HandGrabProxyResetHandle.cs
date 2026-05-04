using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandGrabProxyResetHandle : MonoBehaviour
{
    [SerializeField] private bool useLeftHand = false;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    private XRHandSubsystem _handSubsystem;

    private readonly List<HandRotationResetHandle> _resetStack = new List<HandRotationResetHandle>();
    private HandRotationResetHandle _hoveredResetHandle;

    private bool _wasPinchingLastFrame = false;

    private readonly Dictionary<Transform, Quaternion> _defaultRotations = new Dictionary<Transform, Quaternion>();

    private void OnEnable()
    {
        _handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        if (_handSubsystem == null)
        {
            Debug.LogError("HandGrabProxyResetHandle: No XRHandSubsystem found.");
        }

        CacheDefaultRotations();
    }

    private void CacheDefaultRotations()
    {
        HandRotationResetHandle[] handles = FindObjectsOfType<HandRotationResetHandle>(true);

        foreach (HandRotationResetHandle handle in handles)
        {
            if (handle != null && handle.RotationTarget != null && !_defaultRotations.ContainsKey(handle.RotationTarget))
            {
                _defaultRotations.Add(handle.RotationTarget, handle.RotationTarget.rotation);
            }
        }
    }

    private void Update()
    {
        bool isPinching = ReadPinchState();

        if (isPinching && !_wasPinchingLastFrame)
        {
            TryReset();
        }

        _wasPinchingLastFrame = isPinching;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandRotationResetHandle handle = other.GetComponent<HandRotationResetHandle>();
        if (handle == null)
        {
            return;
        }

        if (!_resetStack.Contains(handle))
        {
            _resetStack.Add(handle);
        }

        _hoveredResetHandle = handle;
    }

    private void OnTriggerExit(Collider other)
    {
        HandRotationResetHandle handle = other.GetComponent<HandRotationResetHandle>();
        if (handle == null)
        {
            return;
        }

        _resetStack.Remove(handle);
        _hoveredResetHandle = _resetStack.Count > 0 ? _resetStack[_resetStack.Count - 1] : null;
    }

    private void TryReset()
    {
        if (_hoveredResetHandle == null || _hoveredResetHandle.RotationTarget == null)
        {
            return;
        }

        Transform target = _hoveredResetHandle.RotationTarget;

        if (_defaultRotations.TryGetValue(target, out Quaternion defaultRotation))
        {
            target.rotation = defaultRotation;
            EventManager.TriggerEvent("ObjectMoved");
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