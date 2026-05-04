using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandASDOccluderToolController : MonoBehaviour
{
    [Header("Tool Selection")]
    [SerializeField] private Transform toolSelectionParent;

    [Header("Device")]
    [SerializeField] private GameObject devicePrefab;
    [SerializeField] private Transform placedDeviceParent;

    [Header("Preview")]
    [SerializeField] private Vector3 previewLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 previewLocalEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 previewLocalScale = Vector3.one;
    [SerializeField] private bool hidePreviewWhenNotSelected = true;

    [Header("UI")]
    [SerializeField] private TMP_Text currentSizeLabel;

    [Header("Pinch")]
    [SerializeField] private bool useLeftHand = false;
    [SerializeField] private float pinchDistanceThreshold = 0.025f;

    private XRHandSubsystem handSubsystem;
    private bool wasPinchingLastFrame = false;

    private GameObject previewInstance;

    private readonly string[] deviceSizeNames = new string[]
    {
        "ASD Occluder 12.0mm",
        "ASD Occluder 13.5mm",
        "ASD Occluder 16.5mm",
        "ASD Occluder 18.0mm",
        "ASD Occluder 21.0mm",
        "ASD Occluder 24.0mm",
        "ASD Occluder 27.0mm",
        "ASD Occluder 30.0mm",
        "ASD Occluder 33.0mm",
        "ASD Occluder 36.0mm"
    };

    private int currentSizeIndex = 0;
    private int currentComponentIndex = 1;
    // 0 = first component only
    // 1 = both components visible
    // 2 = second component only

    private void OnEnable()
    {
        handSubsystem = XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();
    }

    private void Start()
    {
        UpdateSizeLabel();
    }

    private void Update()
    {
        bool toolSelected = IsAsdToolSelected();
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
            PlaceDevice();
        }

        wasPinchingLastFrame = pinching;
    }

    private bool IsAsdToolSelected()
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

                if (childName.Contains("animatedasddevicetoolicon") ||
                    childName.Contains("asd"))
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

        if (devicePrefab == null)
        {
            Debug.LogError("[HandASDOccluderToolController] No devicePrefab assigned.");
            return;
        }

        previewInstance = Instantiate(devicePrefab, transform);
        previewInstance.name = devicePrefab.name + "_HandPreview";

        previewInstance.transform.localPosition = previewLocalPosition;
        previewInstance.transform.localRotation = Quaternion.Euler(previewLocalEulerAngles);
        previewInstance.transform.localScale = previewLocalScale;

        MakePreviewSafe(previewInstance);
        ApplyStateToDevice(previewInstance);
    }

    private void PlaceDevice()
    {
        if (devicePrefab == null || previewInstance == null)
            return;

        GameObject placedInstance = Instantiate(devicePrefab);
        placedInstance.name = devicePrefab.name + "_Placed";

        placedInstance.transform.position = previewInstance.transform.position;
        placedInstance.transform.rotation = previewInstance.transform.rotation;
        placedInstance.transform.localScale = previewInstance.transform.lossyScale;

        if (placedDeviceParent != null)
        {
            placedInstance.transform.SetParent(placedDeviceParent, true);
        }

        ApplyStateToDevice(placedInstance);
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

    public void IncreaseSize()
    {
        if (currentSizeIndex < deviceSizeNames.Length - 1)
        {
            currentSizeIndex++;
            ApplyCurrentState();
        }
    }

    public void DecreaseSize()
    {
        if (currentSizeIndex > 0)
        {
            currentSizeIndex--;
            ApplyCurrentState();
        }
    }

    public void NextComponent()
    {
        currentComponentIndex++;
        if (currentComponentIndex > 2)
        {
            currentComponentIndex = 0;
        }

        ApplyCurrentState();
    }

    public void PreviousComponent()
    {
        currentComponentIndex--;
        if (currentComponentIndex < 0)
        {
            currentComponentIndex = 2;
        }

        ApplyCurrentState();
    }

    private void ApplyCurrentState()
    {
        if (previewInstance != null)
        {
            ApplyStateToDevice(previewInstance);
        }

        UpdateSizeLabel();
    }

    private void ApplyStateToDevice(GameObject deviceRoot)
    {
        if (deviceRoot == null)
            return;

        MonoBehaviour control = FindDeviceModelControl(deviceRoot);
        if (control == null)
        {
            Debug.LogWarning("[HandASDOccluderToolController] No DeviceModelControl found on device.");
            return;
        }

        SetDeviceSize(control, currentSizeIndex);
        SetDeviceComponent(control, currentComponentIndex);
    }

    private MonoBehaviour FindDeviceModelControl(GameObject root)
    {
        MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour.GetType().Name == "DeviceModelControl")
                return behaviour;
        }

        return null;
    }

    private void SetDeviceSize(MonoBehaviour control, int targetIndex)
    {
        while (GetSize(control) < targetIndex)
        {
            InvokeNoArg(control, "IncreaseSize");
        }

        while (GetSize(control) > targetIndex)
        {
            InvokeNoArg(control, "DecreaseSize");
        }
    }

    private void SetDeviceComponent(MonoBehaviour control, int targetMode)
    {
        while (GetComponentIndex(control) != -1)
        {
            InvokeNoArg(control, "PreviousComponent");
        }

        if (targetMode == 0)
        {
            InvokeNoArg(control, "PreviousComponent");
        }
        else if (targetMode == 1)
        {
            // all visible
        }
        else if (targetMode == 2)
        {
            InvokeNoArg(control, "NextComponent");
        }
    }

    private int GetSize(MonoBehaviour control)
    {
        object result = InvokeReturn(control, "Size");
        return result is int intResult ? intResult : 0;
    }

    private int GetComponentIndex(MonoBehaviour control)
    {
        object result = InvokeReturn(control, "Component");
        return result is int intResult ? intResult : -1;
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

    private void UpdateSizeLabel()
    {
        if (currentSizeLabel != null &&
            currentSizeIndex >= 0 &&
            currentSizeIndex < deviceSizeNames.Length)
        {
            currentSizeLabel.text = deviceSizeNames[currentSizeIndex];
        }
    }

    public void SetCurrentSizeLabel(TMP_Text label)
    {
        currentSizeLabel = label;
        UpdateSizeLabel();
    }
}