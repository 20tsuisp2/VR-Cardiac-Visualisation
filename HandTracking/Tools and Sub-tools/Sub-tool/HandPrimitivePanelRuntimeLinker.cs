using UnityEngine;
using UnityEngine.UI;

public class HandPrimitivePanelRuntimeLinker : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private HandPrimitiveOccluderToolController handPrimitiveToolController;
    [SerializeField] private Transform secondaryUiPanelsRoot;

    [Header("Search")]
    [SerializeField] private string panelRootNameContains = "XrToolPanelPrimitiveTwinRing";

    private bool hasLinked = false;

    private void Update()
    {
        if (hasLinked)
            return;

        if (handPrimitiveToolController == null || secondaryUiPanelsRoot == null)
            return;

        Transform panelRoot = FindPanelRoot();
        if (panelRoot == null)
            return;

        bool linked = LinkPanel(panelRoot);
        if (linked)
        {
            hasLinked = true;
        }
    }

    private Transform FindPanelRoot()
    {
        Transform[] allChildren = secondaryUiPanelsRoot.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child == null)
                continue;

            if (child.name.Contains(panelRootNameContains))
            {
                return child;
            }
        }

        return null;
    }

    private bool LinkPanel(Transform panelRoot)
    {
        Button blueMinusButton = FindButton(panelRoot, "BlueMinusButton");
        Button bluePlusButton = FindButton(panelRoot, "BluePlusButton");
        Button greenMinusButton = FindButton(panelRoot, "GreenMinusButton");
        Button greenPlusButton = FindButton(panelRoot, "GreenPlusButton");

        Slider blueSizeSlider = FindSlider(panelRoot, "BlueSizeSlider");
        Slider greenSizeSlider = FindSlider(panelRoot, "GreenSizeSlider");

        if (blueMinusButton == null ||
            bluePlusButton == null ||
            greenMinusButton == null ||
            greenPlusButton == null)
        {
            return false;
        }

        blueMinusButton.onClick.RemoveAllListeners();
        bluePlusButton.onClick.RemoveAllListeners();
        greenMinusButton.onClick.RemoveAllListeners();
        greenPlusButton.onClick.RemoveAllListeners();

        blueMinusButton.onClick.AddListener(handPrimitiveToolController.DecreaseBlue);
        bluePlusButton.onClick.AddListener(handPrimitiveToolController.IncreaseBlue);
        greenMinusButton.onClick.AddListener(handPrimitiveToolController.DecreaseGreen);
        greenPlusButton.onClick.AddListener(handPrimitiveToolController.IncreaseGreen);

        if (blueSizeSlider != null)
        {
            blueSizeSlider.onValueChanged.RemoveAllListeners();
            blueSizeSlider.onValueChanged.AddListener(handPrimitiveToolController.OnBlueSliderChanged);
        }

        if (greenSizeSlider != null)
        {
            greenSizeSlider.onValueChanged.RemoveAllListeners();
            greenSizeSlider.onValueChanged.AddListener(handPrimitiveToolController.OnGreenSliderChanged);
        }

        handPrimitiveToolController.SetRuntimeSliders(blueSizeSlider, greenSizeSlider);

        return true;
    }

    private Button FindButton(Transform root, string name)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                return child.GetComponent<Button>();
            }
        }

        return null;
    }

    private Slider FindSlider(Transform root, string name)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                return child.GetComponent<Slider>();
            }
        }

        return null;
    }
}