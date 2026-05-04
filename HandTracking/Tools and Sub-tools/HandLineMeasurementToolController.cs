using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandLineMeasurementToolController : MonoBehaviour
{
    [Header("Tool Selection")]
    [SerializeField] private Transform toolSelectionParent;

    [Header("Measurement Spawn")]
    [SerializeField] private Transform measurementIconParent;
    [SerializeField] private GameObject measurementPrefab;
    [SerializeField] private GameObject defaultIcon;

    [Header("Pinch")]
    [SerializeField] private bool useLeftHand = false;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    private XRHandSubsystem handSubsystem;

    private GameObject objectInHand;
    private Transform objectInHandOldParent;
    private bool wasPinchingLastFrame = false;

    private void OnEnable()
    {
        handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();
    }

    private void Update()
    {
        bool measurementSelected = IsLineMeasurementToolSelected();
        bool pinching = IsPinching();

        if (defaultIcon != null)
        {
            defaultIcon.SetActive(measurementSelected && objectInHand == null);
        }

        if (!measurementSelected)
        {
            wasPinchingLastFrame = pinching;
            return;
        }

        bool pinchStartedThisFrame = pinching && !wasPinchingLastFrame;

        if (pinchStartedThisFrame)
        {
            try
            {
                if (objectInHand == null)
                {
                    ControllerToolLineMeasurement.StartMeasurement(
                        defaultIcon,
                        measurementIconParent != null ? measurementIconParent.gameObject : null,
                        measurementPrefab,
                        transform,
                        ref objectInHand,
                        ref objectInHandOldParent);

                    Debug.Log("[HandLineMeasurementToolController] Started measurement");
                }
                else
                {
                    ControllerToolLineMeasurement.EndMeasurement(
                        defaultIcon,
                        ref objectInHand,
                        ref objectInHandOldParent);

                    Debug.Log("[HandLineMeasurementToolController] Ended measurement");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[HandLineMeasurementToolController] " + e);
            }
        }

        wasPinchingLastFrame = pinching;
    }

    private bool IsLineMeasurementToolSelected()
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

                if (childName.Contains("animatedmeasurementtoolicon"))
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
}