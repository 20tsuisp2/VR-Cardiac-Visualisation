using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using UnityEngine.UI;

public class HandEvtToolController : MonoBehaviour
{
    [Header("Tool Selection")]
    [SerializeField] private Transform toolIconParent;

    [Header("EVT")]
    [SerializeField] private float pinchDistanceThreshold = 0.025f;
    [SerializeField] private GameObject evtVisualRoot;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private XRHandSubsystem _handSubsystem;
    private Component _evtShaderControl;

    private Transform _shaderSpaceTransform;
    private Transform _volumeSizeTransform;

    private bool _wasPinchingLastFrame = false;

    private void Reset()
    {
        if (evtVisualRoot == null)
            evtVisualRoot = gameObject;
    }

    private void OnEnable()
    {
        _handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        FindEvtShaderControl();
    }

    private void Update()
    {
        if (_evtShaderControl == null)
            FindEvtShaderControl();

        bool evtSelected = IsEvtSelected();

        Log("EVT selected = " + evtSelected);
        Log("EVT shader found = " + (_evtShaderControl != null));
        Log("Shader space transform = " + (_shaderSpaceTransform != null ? _shaderSpaceTransform.name : "NULL"));

        if (evtVisualRoot != null)
            evtVisualRoot.SetActive(evtSelected);

        if (!evtSelected || _evtShaderControl == null)
        {
            if (_evtShaderControl != null)
                SetEvtBoolProperty(_evtShaderControl, "PermanentMode", false);

            _wasPinchingLastFrame = false;
            return;
        }

        Vector3 localPoint = GetVolumeLocalPoint(transform.position);

        Log("ET point world pos = " + transform.position);
        LogEvtCoordinateSpaces(transform.position);
        Log("ET point local pos USED FOR SHADER = " + localPoint);

        List<Vector3> positionLocal = new List<Vector3>
        {
            localPoint
        };

        float radiusValue = GetEvtFloatProperty(_evtShaderControl, "RadiusSomeUnits", 0.2f);

        Log("ET radius = " + radiusValue);

        List<float> radius = new List<float>
        {
            radiusValue
        };

        bool isPinching = ReadRightHandPinchState();

        SetEvtBoolProperty(_evtShaderControl, "PermanentMode", isPinching);
        SetEvtBoolProperty(_evtShaderControl, "ShowEvt", true);

        Log("Forcing EVT ShowEvt = true");
        Log("Sending EVT position to shader");
        InvokeSetEvtPositionsLocal(_evtShaderControl, positionLocal, radius);

        Log("ET isPinching = " + isPinching);

        if (isPinching && !_wasPinchingLastFrame)
        {
            EventManager.TriggerEvent("EvtEditStarted");
            Log("EVT pinch started");
        }
        else if (!isPinching && _wasPinchingLastFrame)
        {
            EventManager.TriggerEvent("EvtEditEnded");
            Log("EVT pinch ended");
        }

        _wasPinchingLastFrame = isPinching;
    }

    private void FindEvtShaderControl()
    {
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        foreach (MonoBehaviour mb in allBehaviours)
        {
            if (mb == null)
                continue;

            if (mb.GetType().Name == "EvtShaderControl")
            {
                _evtShaderControl = mb;
                _shaderSpaceTransform = mb.transform;

                if (mb.transform.parent != null)
                    _volumeSizeTransform = mb.transform.parent;

                Log("Found EvtShaderControl on object: " + mb.gameObject.name);

                if (_shaderSpaceTransform != null)
                    Log("Shader space transform set to: " + _shaderSpaceTransform.name);

                if (_volumeSizeTransform != null)
                    Log("Volume size transform set to: " + _volumeSizeTransform.name);

                return;
            }
        }
    }

    private bool IsEvtSelected()
    {
        if (toolIconParent == null)
            return false;

        Toggle[] toggles = toolIconParent.GetComponentsInChildren<Toggle>(true);

        foreach (Toggle toggle in toggles)
        {
            if (!toggle.isOn)
                continue;

            for (int i = 0; i < toggle.transform.childCount; i++)
            {
                string childName = toggle.transform.GetChild(i).name.ToLower();
                if (childName.Contains("evt"))
                    return true;
            }
        }

        return false;
    }

    private Vector3 GetVolumeLocalPoint(Vector3 worldPoint)
    {
        if (_shaderSpaceTransform == null)
            return Vector3.zero;

        return _shaderSpaceTransform.InverseTransformPoint(worldPoint);
    }

    private void LogEvtCoordinateSpaces(Vector3 worldPoint)
    {
        if (!debugLogs)
            return;

        Debug.Log("========== EVT SPACE DEBUG ==========");
        Debug.Log("World point = " + worldPoint);

        if (_shaderSpaceTransform != null)
        {
            Vector3 shaderLocal = _shaderSpaceTransform.InverseTransformPoint(worldPoint);
            Debug.Log("Shader object local (VolumeObject expected) = " + shaderLocal);
        }
        else
        {
            Debug.Log("Shader object local = NULL");
        }

        if (_volumeSizeTransform != null)
        {
            Vector3 sizeLocal = _volumeSizeTransform.InverseTransformPoint(worldPoint);
            Debug.Log("VolumeSizeObject local = " + sizeLocal);
        }
        else
        {
            Debug.Log("VolumeSizeObject local = NULL");
        }

        Debug.Log("====================================");
    }

    private bool ReadRightHandPinchState()
    {
        if (_handSubsystem == null)
            return false;

        XRHand hand = _handSubsystem.rightHand;

        if (!hand.isTracked)
            return false;

        XRHandJoint thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        XRHandJoint indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (!thumbTip.TryGetPose(out Pose thumbPose))
            return false;

        if (!indexTip.TryGetPose(out Pose indexPose))
            return false;

        float distance = Vector3.Distance(thumbPose.position, indexPose.position);
        return distance <= pinchDistanceThreshold;
    }

    private System.Type GetEvtInterfaceType(Component target)
    {
        if (target == null)
            return null;

        return target.GetType().GetInterface("IEvtShaderControl");
    }

    private MethodInfo GetInterfaceMethod(Component target, string methodName)
    {
        if (target == null)
            return null;

        System.Type interfaceType = GetEvtInterfaceType(target);
        if (interfaceType == null)
        {
            LogWarning("IEvtShaderControl interface not found on " + target.GetType().Name);
            return null;
        }

        MethodInfo interfaceMethod = interfaceType.GetMethod(methodName);
        if (interfaceMethod == null)
        {
            LogWarning("Interface method not found: " + methodName);
            return null;
        }

        InterfaceMapping map = target.GetType().GetInterfaceMap(interfaceType);

        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.InterfaceMethods[i].Name == interfaceMethod.Name)
                return map.TargetMethods[i];
        }

        LogWarning("Could not map interface method: " + methodName);
        return null;
    }

    private PropertyInfo GetInterfaceProperty(Component target, string propertyName)
    {
        if (target == null)
            return null;

        System.Type interfaceType = GetEvtInterfaceType(target);
        if (interfaceType == null)
        {
            LogWarning("IEvtShaderControl interface not found on " + target.GetType().Name);
            return null;
        }

        PropertyInfo interfaceProperty = interfaceType.GetProperty(propertyName);
        if (interfaceProperty == null)
        {
            LogWarning("Interface property not found: " + propertyName);
            return null;
        }

        return interfaceProperty;
    }

    private void SetEvtBoolProperty(Component target, string propertyName, bool value)
    {
        if (target == null)
            return;

        PropertyInfo interfaceProperty = GetInterfaceProperty(target, propertyName);
        if (interfaceProperty == null)
            return;

        MethodInfo setter = interfaceProperty.GetSetMethod();
        if (setter == null)
        {
            LogWarning("No setter for EVT property: " + propertyName);
            return;
        }

        MethodInfo targetSetter = GetInterfaceMethod(target, setter.Name);
        if (targetSetter != null)
        {
            targetSetter.Invoke(target, new object[] { value });
            Log("Set EVT bool property " + propertyName + " = " + value);
        }
    }

    private float GetEvtFloatProperty(Component target, string propertyName, float fallback)
    {
        if (target == null)
            return fallback;

        PropertyInfo interfaceProperty = GetInterfaceProperty(target, propertyName);
        if (interfaceProperty == null)
            return fallback;

        MethodInfo getter = interfaceProperty.GetGetMethod();
        if (getter == null)
        {
            LogWarning("No getter for EVT property: " + propertyName);
            return fallback;
        }

        MethodInfo targetGetter = GetInterfaceMethod(target, getter.Name);
        if (targetGetter == null)
            return fallback;

        object value = targetGetter.Invoke(target, null);
        if (value is float f)
            return f;

        return fallback;
    }

    private void InvokeSetEvtPositionsLocal(Component target, List<Vector3> positionsLocal, List<float> radii)
    {
        if (target == null)
            return;

        MethodInfo targetMethod = GetInterfaceMethod(target, "SetEvtPositonsLocal");
        if (targetMethod != null)
        {
            targetMethod.Invoke(target, new object[] { positionsLocal, radii });
            Log("Successfully invoked SetEvtPositonsLocal");
        }
        else
        {
            LogWarning("Could not invoke SetEvtPositonsLocal on " + target.GetType().Name);
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log(message);
    }

    private void LogWarning(string message)
    {
        if (debugLogs)
            Debug.LogWarning(message);
    }
}