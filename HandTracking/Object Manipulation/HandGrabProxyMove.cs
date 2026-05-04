using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandGrabProxyMove : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField] private bool useLeftHand = true;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    private XRHandSubsystem _handSubsystem;

    private List<Transform> _colliderStack = new List<Transform>();

    private GameObject _collidingObject;
    private GameObject _objectInHand;
    private Transform _objectInHandOldParent;

    private bool _wasPinchingLastFrame = false;

    private void OnEnable()
    {
        _handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        if (_handSubsystem == null)
        {
            Debug.LogError("HandGrabProxyMove: No XRHandSubsystem found.");
        }
    }

    private void Update()
    {
        bool isPinching = ReadPinchState();

        if (isPinching && !_wasPinchingLastFrame)
        {
            if (_collidingObject != null && _objectInHand == null)
            {
                GameObject targetToGrab = GetValidGrabTarget(_collidingObject);
                if (targetToGrab != null)
                {
                    _collidingObject = targetToGrab;

                    ControllerToolMove.GrabObject(
                        transform,
                        ref _collidingObject,
                        ref _objectInHand,
                        ref _objectInHandOldParent);

                    if (_objectInHand != null)
                    {
                        ControllerHelpers.SetGameObjectIndicationState(
                            _objectInHand,
                            IndicatorBase.IndicateState.Active);
                    }
                }
            }
        }

        if (!isPinching && _wasPinchingLastFrame)
        {
            if (_objectInHand != null)
            {
                BoxCollider thisCollider = GetComponent<BoxCollider>();
                if (thisCollider == null)
                {
                    Debug.LogError("HandGrabProxyMove requires a BoxCollider.");
                    return;
                }

                ControllerToolMove.ReleaseObject(
                    thisCollider,
                    transform,
                    ref _objectInHand,
                    ref _objectInHandOldParent,
                    ref _colliderStack,
                    ref _collidingObject);

                EventManager.TriggerEvent("ObjectMoved");
            }
        }

        _wasPinchingLastFrame = isPinching;
    }

    private void OnTriggerEnter(Collider other)
    {
        ControllerToolMove.UpdateStackOnEnter(other.transform, ref _colliderStack);

        if (_colliderStack.Count > 0)
        {
            SetCollidingObject(
                GetValidGrabTarget(_colliderStack[_colliderStack.Count - 1].gameObject));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool shouldUpdate = ControllerToolMove.UpdateStackOnExit(
            other.transform,
            _collidingObject,
            ref _colliderStack);

        if (!shouldUpdate)
        {
            return;
        }

        if (_collidingObject != null && _objectInHand == null)
        {
            ControllerHelpers.SetGameObjectIndicationState(
                _collidingObject,
                IndicatorBase.IndicateState.Off);
        }

        if (_colliderStack.Count > 0)
        {
            SetCollidingObject(
                GetValidGrabTarget(_colliderStack[_colliderStack.Count - 1].gameObject));
        }
        else
        {
            _collidingObject = null;
        }
    }

    private void SetCollidingObject(GameObject colGameObject)
    {
        if (colGameObject == null)
        {
            _collidingObject = null;
            return;
        }

        if (_collidingObject == colGameObject)
        {
            return;
        }

        if (_collidingObject != null && _objectInHand == null)
        {
            ControllerHelpers.SetGameObjectIndicationState(
                _collidingObject,
                IndicatorBase.IndicateState.Off);
        }

        _collidingObject = colGameObject;

        if (_objectInHand == null)
        {
            ControllerHelpers.SetGameObjectIndicationState(
                _collidingObject,
                IndicatorBase.IndicateState.Highlight);
        }
    }

    private GameObject GetValidGrabTarget(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        if (obj.GetComponent<GrabActionsChildMoveParent>() != null)
        {
            return obj;
        }

        Transform t = obj.transform;

        while (t != null)
        {
            if (t.name.Contains("MovableVolumeRender"))
            {
                return t.gameObject;
            }

            if (t.name.Contains("MprDisplayPlane"))
            {
                return t.gameObject;
            }

            if (t.GetComponent<ParentMoveBase>() != null)
            {
                return t.gameObject;
            }

            t = t.parent;
        }

        return obj;
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