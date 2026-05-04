using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandAreaCircumferenceToolController : MonoBehaviour
{
    [Header("Tool Selection")]
    [SerializeField] private Transform toolSelectionParent;

    [Header("Measurement Spawn")]
    [SerializeField] private Transform measurementIconParent;
    [SerializeField] private GameObject defaultIcon;

    [Header("Area/Circ Prefabs")]
    [SerializeField] private GameObject startEndPrefab;
    [SerializeField] private GameObject endPrefab;
    [SerializeField] private GameObject closingPrefab;

    [Header("Pinch")]
    [SerializeField] private bool useLeftHand = false;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    private XRHandSubsystem handSubsystem;

    private GameObject objectInHand;
    private Transform objectInHandOldParent;
    private Transform measurementRoot;

    // store as object so we do not directly reference prototype-only types
    private object currentMeasurement;

    private bool wasPinchingLastFrame = false;

    private Type controllerToolAreaCircType;

    private void OnEnable()
    {
        handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        controllerToolAreaCircType = FindTypeByName("ControllerToolAreaCircumference");
    }

    private void Update()
    {
        bool toolSelected = IsAreaCircToolSelected();
        bool pinching = IsPinching();

        if (defaultIcon != null)
        {
            defaultIcon.SetActive(toolSelected && objectInHand == null);
        }

        if (!toolSelected)
        {
            wasPinchingLastFrame = pinching;
            return;
        }

        bool pinchStartedThisFrame = pinching && !wasPinchingLastFrame;

        if (pinchStartedThisFrame)
        {
            try
            {
                if (controllerToolAreaCircType == null)
                {
                    Debug.LogError("[HandAreaCircumferenceToolController] Could not find ControllerToolAreaCircumference type.");
                    wasPinchingLastFrame = pinching;
                    return;
                }

                if (objectInHand == null)
                {
                    InvokeStartMeasurement();
                    Debug.Log("[HandAreaCircumferenceToolController] Started area/circ measurement");
                }
                else if (!GetCurrentMeasurementCanClose())
                {
                    InvokeContinueMeasurement();
                    Debug.Log("[HandAreaCircumferenceToolController] Added point");
                }
                else
                {
                    InvokeCloseMeasurement();
                    Debug.Log("[HandAreaCircumferenceToolController] Closed area/circ measurement");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[HandAreaCircumferenceToolController] " + e);
            }
        }

        wasPinchingLastFrame = pinching;
    }

    private void InvokeStartMeasurement()
    {
        MethodInfo method = controllerToolAreaCircType.GetMethod(
            "StartMeasurement",
            BindingFlags.Public | BindingFlags.Static);

        if (method == null)
            throw new MissingMethodException("StartMeasurement not found on ControllerToolAreaCircumference");

        object[] args = new object[]
        {
            defaultIcon,
            measurementIconParent != null ? measurementIconParent.gameObject : null,
            startEndPrefab,
            transform,
            objectInHand,
            objectInHandOldParent,
            measurementRoot,
            currentMeasurement
        };

        method.Invoke(null, args);

        objectInHand = args[4] as GameObject;
        objectInHandOldParent = args[5] as Transform;
        measurementRoot = args[6] as Transform;
        currentMeasurement = args[7];
    }

    private void InvokeContinueMeasurement()
    {
        // This is the point that was moving just before we add a new segment.
        // The new segment should start from here.
        GameObject previousMovingPoint = objectInHand;

        MethodInfo method = controllerToolAreaCircType.GetMethod(
            "ContinueMeasurement",
            BindingFlags.Public | BindingFlags.Static);

        if (method == null)
            throw new MissingMethodException("ContinueMeasurement not found on ControllerToolAreaCircumference");

        object[] args = new object[]
        {
        defaultIcon,
        measurementIconParent != null ? measurementIconParent.gameObject : null,
        endPrefab,
        currentMeasurement,
        objectInHand,
        objectInHandOldParent,
        measurementRoot
        };

        method.Invoke(null, args);

        objectInHand = args[4] as GameObject;
        objectInHandOldParent = args[5] as Transform;
        measurementRoot = args[6] as Transform;

        // Patch the newly spawned endpoint prefab so its dotted lines know
        // what point they should start from in hand mode.
        if (objectInHandOldParent != null && previousMovingPoint != null)
        {
            DottedLine[] dottedLines = objectInHandOldParent.GetComponentsInChildren<DottedLine>(true);

            foreach (DottedLine line in dottedLines)
            {
                line.StartObject = previousMovingPoint;
            }

            Debug.Log("[HandAreaCircumferenceToolController] Patched StartObject on new area/circ segment");
        }
    }

    private void InvokeCloseMeasurement()
    {
        MethodInfo method = controllerToolAreaCircType.GetMethod(
            "CloseMeasurement",
            BindingFlags.Public | BindingFlags.Static);

        if (method == null)
            throw new MissingMethodException("CloseMeasurement not found on ControllerToolAreaCircumference");

        object[] args = new object[]
        {
            defaultIcon,
            closingPrefab,
            objectInHand,
            objectInHandOldParent,
            measurementRoot,
            currentMeasurement
        };

        method.Invoke(null, args);

        objectInHand = args[2] as GameObject;
        objectInHandOldParent = args[3] as Transform;
        measurementRoot = args[4] as Transform;
        currentMeasurement = args[5];
    }

    private bool GetCurrentMeasurementCanClose()
    {
        if (currentMeasurement == null)
            return false;

        Type t = currentMeasurement.GetType();

        FieldInfo canCloseField = t.GetField("CanClose", BindingFlags.Public | BindingFlags.Instance);
        if (canCloseField != null)
        {
            object value = canCloseField.GetValue(currentMeasurement);
            if (value is bool b)
                return b;
        }

        PropertyInfo canCloseProp = t.GetProperty("CanClose", BindingFlags.Public | BindingFlags.Instance);
        if (canCloseProp != null)
        {
            object value = canCloseProp.GetValue(currentMeasurement);
            if (value is bool b)
                return b;
        }

        Debug.LogWarning("[HandAreaCircumferenceToolController] Could not read CanClose from currentMeasurement.");
        return false;
    }

    private bool IsAreaCircToolSelected()
    {
        if (toolSelectionParent == null)
            return false;

        Toggle[] toggles = toolSelectionParent.GetComponentsInChildren<Toggle>(true);

        foreach (Toggle toggle in toggles)
        {
            if (!toggle.isOn)
                continue;

            for (int i = 0; i < toggle.transform.childCount; i++)
            {
                string childName = toggle.transform.GetChild(i).name.ToLower();

                if (childName.Contains("animatedareacirctoolicon"))
                    return true;
            }
        }

        return false;
    }

    private bool IsPinching()
    {
        if (handSubsystem == null)
            return false;

        XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked)
            return false;

        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (!thumbTip.TryGetPose(out Pose thumbPose) || !indexTip.TryGetPose(out Pose indexPose))
            return false;

        float distance = Vector3.Distance(thumbPose.position, indexPose.position);
        return distance <= pinchDistanceThreshold;
    }

    private static Type FindTypeByName(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            if (types == null)
                continue;

            foreach (Type type in types)
            {
                if (type == null)
                    continue;

                if (type.Name == typeName)
                    return type;
            }
        }

        return null;
    }
}
