using UnityEngine;
using UnityEngine.UI;

public class HandToolIconPanelLinker : MonoBehaviour
{
    [SerializeField] private Transform toolIconParent;
    [SerializeField] private Transform subToolUiParent;
    [SerializeField] private float scanInterval = 0.1f;

    private float _nextScanTime;

    private void Reset()
    {
        if (toolIconParent == null)
        {
            Transform spacer = transform.Find("Spacer");
            if (spacer != null)
                toolIconParent = spacer;
        }
    }

    private void Update()
    {
        if (Time.time < _nextScanTime)
            return;

        _nextScanTime = Time.time + scanInterval;
        RefreshPanelsFromSelectedToggle();
    }

    private void RefreshPanelsFromSelectedToggle()
    {
        if (toolIconParent == null || subToolUiParent == null)
            return;

        Toggle[] toggles = toolIconParent.GetComponentsInChildren<Toggle>(true);

        string selectedIconType = "none";

        foreach (Toggle toggle in toggles)
        {
            if (!toggle.isOn)
                continue;

            selectedIconType = GetIconType(toggle.transform);
            break;
        }

        for (int i = 0; i < subToolUiParent.childCount; i++)
        {
            GameObject panel = subToolUiParent.GetChild(i).gameObject;
            string panelName = panel.name.ToLower();

            bool active = false;

            if (selectedIconType == "move" && panelName.Contains("scalemove"))
                active = true;
            else if (selectedIconType == "evt" && panelName.Contains("evt"))
                active = true;
            else if (selectedIconType == "lighting" && panelName.Contains("lighting"))
                active = true;
            else if (selectedIconType == "asd" && panelName.Contains("asddevice"))
                active = true;
            else if (selectedIconType == "primitive" && panelName.Contains("primitivetwinring"))
                active = true;

            panel.SetActive(active);
        }
    }

    private string GetIconType(Transform toggleTransform)
    {
        for (int i = 0; i < toggleTransform.childCount; i++)
        {
            string childName = toggleTransform.GetChild(i).name.ToLower();

            if (childName.Contains("animatedprimitivetwinringtoolicon") ||
                childName.Contains("primitivetwinring") ||
                childName.Contains("primitive"))
                return "primitive";

            if (childName.Contains("animatedasddevicetoolicon") ||
                childName.Contains("asd"))
                return "asd";

            if (childName.Contains("move"))
                return "move";

            if (childName.Contains("evt"))
                return "evt";

            if (childName.Contains("lighting"))
                return "lighting";

            if (childName.Contains("measurement"))
                return "measurement";
        }

        return "unknown";
    }
}