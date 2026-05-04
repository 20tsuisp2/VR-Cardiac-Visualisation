using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandPrimitiveOccluderToolController : MonoBehaviour
{
    [Header("Tool Selection")]
    [SerializeField] private Transform toolSelectionParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject previewPrefab;   // PrimitiveV3
    [SerializeField] private GameObject placedPrefab;    // PrimitiveV3_delete
    [SerializeField] private Transform placedDeviceParent;

    [Header("Preview")]
    [SerializeField] private Vector3 previewLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 previewLocalEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 previewLocalScale = Vector3.one;
    [SerializeField] private bool hidePreviewWhenNotSelected = true;

    [Header("Pinch")]
    [SerializeField] private bool useLeftHand = false;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    [Header("Sliders")]
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private int maxSteps = 20;

    private XRHandSubsystem handSubsystem;
    private bool wasPinchingLastFrame = false;

    private GameObject previewInstance;

    private void OnEnable()
    {
        handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();
    }

    private void Update()
    {
        bool toolSelected = IsPrimitiveToolSelected();
        bool pinching = IsPinching();

        if (!toolSelected)
        {
            if (hidePreviewWhenNotSelected && previewInstance != null)
            {
                previewInstance.SetActive(false);
            }

            wasPinchingLastFrame = pinching;
            return;
        }

        EnsurePreviewExists();

        if (previewInstance != null && !previewInstance.activeSelf)
        {
            previewInstance.SetActive(true);
        }

        bool pinchStartedThisFrame = pinching && !wasPinchingLastFrame;

        if (pinchStartedThisFrame)
        {
            PlacePrimitive();
        }

        wasPinchingLastFrame = pinching;
    }

    private bool IsPrimitiveToolSelected()
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

                if (childName.Contains("animatedprimitivetwinringtoolicon") ||
                    childName.Contains("primitive") ||
                    childName.Contains("twinring"))
                {
                    return true;
                }
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

        XRHandJoint thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        XRHandJoint indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (!thumbTip.TryGetPose(out Pose thumbPose))
            return false;

        if (!indexTip.TryGetPose(out Pose indexPose))
            return false;

        float distance = Vector3.Distance(thumbPose.position, indexPose.position);
        return distance <= pinchDistanceThreshold;
    }

    private void EnsurePreviewExists()
    {
        if (previewInstance != null)
            return;

        if (previewPrefab == null)
        {
            Debug.LogError("[HandPrimitiveOccluderToolController] No previewPrefab assigned.");
            return;
        }

        previewInstance = Instantiate(previewPrefab, transform);
        previewInstance.name = previewPrefab.name + "_HandPreview";

        previewInstance.transform.localPosition = previewLocalPosition;
        previewInstance.transform.localRotation = Quaternion.Euler(previewLocalEulerAngles);
        previewInstance.transform.localScale = previewLocalScale;

        MakePreviewSafe(previewInstance);
        SyncSlidersToPreview();
    }

    private void PlacePrimitive()
    {
        if (placedPrefab == null || previewInstance == null)
            return;

        GameObject placedInstance = Instantiate(placedPrefab);
        placedInstance.name = placedPrefab.name + "_Placed";

        placedInstance.transform.position = previewInstance.transform.position;
        placedInstance.transform.rotation = previewInstance.transform.rotation;
        placedInstance.transform.localScale = previewInstance.transform.lossyScale;

        if (placedDeviceParent != null)
        {
            placedInstance.transform.SetParent(placedDeviceParent, true);
        }

        CopyPrimitiveState(previewInstance, placedInstance);
    }

    private void MakePreviewSafe(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void CopyPrimitiveState(GameObject sourceRoot, GameObject targetRoot)
    {
        MonoBehaviour sourceControl = FindPrimitiveModelControl(sourceRoot);
        MonoBehaviour targetControl = FindPrimitiveModelControl(targetRoot);

        if (sourceControl == null || targetControl == null)
        {
            Debug.LogWarning("[HandPrimitiveOccluderToolController] Could not find primitive control on preview or placed object.");
            return;
        }

        InvokeWithArgument(targetControl, "CopySizeAndComponent", sourceControl);
    }

    private MonoBehaviour FindPrimitiveModelControl(GameObject root)
    {
        if (root == null)
            return null;

        MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour.GetType().Name == "PrimitiveModelControl")
                return behaviour;
        }

        return null;
    }

    public void IncreaseBlue()
    {
        InvokeNoArgOnPreview("IncreaseSize");
        SyncSlidersToPreview();
    }

    public void DecreaseBlue()
    {
        InvokeNoArgOnPreview("DecreaseSize");
        SyncSlidersToPreview();
    }

    public void IncreaseGreen()
    {
        InvokeNoArgOnPreview("NextComponent");
        SyncSlidersToPreview();
    }

    public void DecreaseGreen()
    {
        InvokeNoArgOnPreview("PreviousComponent");
        SyncSlidersToPreview();
    }

    public void OnBlueSliderChanged(float value)
    {
        if (previewInstance == null)
            return;

        MonoBehaviour control = FindPrimitiveModelControl(previewInstance);
        if (control == null)
            return;

        int targetSteps = Mathf.RoundToInt(value * maxSteps);

        ResetSize(control);

        for (int i = 0; i < targetSteps; i++)
        {
            InvokeNoArg(control, "IncreaseSize");
        }
    }

    public void OnGreenSliderChanged(float value)
    {
        if (previewInstance == null)
            return;

        MonoBehaviour control = FindPrimitiveModelControl(previewInstance);
        if (control == null)
            return;

        int targetSteps = Mathf.RoundToInt(value * maxSteps);

        ResetComponent(control);

        for (int i = 0; i < targetSteps; i++)
        {
            InvokeNoArg(control, "NextComponent");
        }
    }

    private void ResetSize(MonoBehaviour control)
    {
        for (int i = 0; i < maxSteps; i++)
        {
            InvokeNoArg(control, "DecreaseSize");
        }
    }

    public void SetRuntimeSliders(Slider blue, Slider green)
    {
        blueSlider = blue;
        greenSlider = green;
        SyncSlidersToPreview();
    }

    private void ResetComponent(MonoBehaviour control)
    {
        for (int i = 0; i < maxSteps; i++)
        {
            InvokeNoArg(control, "PreviousComponent");
        }
    }

    private void SyncSlidersToPreview()
    {
        if (previewInstance == null)
            return;

        MonoBehaviour control = FindPrimitiveModelControl(previewInstance);
        if (control == null)
            return;

        if (blueSlider != null)
        {
            float blueValue = Mathf.Clamp01(GetBlueStepCount(control) / (float)maxSteps);
            blueSlider.SetValueWithoutNotify(blueValue);
        }

        if (greenSlider != null)
        {
            float greenValue = Mathf.Clamp01(GetGreenStepCount(control) / (float)maxSteps);
            greenSlider.SetValueWithoutNotify(greenValue);
        }
    }

    private int GetBlueStepCount(MonoBehaviour control)
    {
        object sizeResult = InvokeReturn(control, "Size");
        if (sizeResult is int sizeInt)
        {
            float scaleX = sizeInt / 1000f;
            int stepsFromMin = Mathf.RoundToInt((scaleX - 0.2f) / 0.025f);
            return Mathf.Clamp(stepsFromMin, 0, maxSteps);
        }

        return 0;
    }

    private int GetGreenStepCount(MonoBehaviour control)
    {
        object componentResult = InvokeReturn(control, "Component");
        if (componentResult is int componentInt)
        {
            float scaleX = componentInt / 1000f;
            int stepsFromMin = Mathf.RoundToInt((scaleX - 0.2f) / 0.025f);
            return Mathf.Clamp(stepsFromMin, 0, maxSteps);
        }

        return 0;
    }

    private void InvokeNoArgOnPreview(string methodName)
    {
        if (previewInstance == null)
            return;

        MonoBehaviour control = FindPrimitiveModelControl(previewInstance);
        if (control == null)
        {
            Debug.LogWarning("[HandPrimitiveOccluderToolController] No PrimitiveModelControl found on preview.");
            return;
        }

        InvokeNoArg(control, methodName);
    }

    private void InvokeNoArg(MonoBehaviour target, string methodName)
    {
        if (target == null)
            return;

        var method = target.GetType().GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(target, null);
        }
    }

    private object InvokeReturn(MonoBehaviour target, string methodName)
    {
        if (target == null)
            return null;

        var method = target.GetType().GetMethod(methodName);
        if (method != null)
        {
            return method.Invoke(target, null);
        }

        return null;
    }

    private void InvokeWithArgument(MonoBehaviour target, string methodName, object argument)
    {
        if (target == null)
            return;

        var method = target.GetType().GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(target, new object[] { argument });
        }
    }
}