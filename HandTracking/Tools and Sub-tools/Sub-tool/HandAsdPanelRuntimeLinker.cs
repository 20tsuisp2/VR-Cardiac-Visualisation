using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandAsdPanelRuntimeLinker : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private HandASDOccluderToolController handAsdToolController;
    [SerializeField] private Transform secondaryUiPanelsRoot;

    [Header("Search")]
    [SerializeField] private string panelRootNameContains = "XrToolPanelAsdDevice";

    private bool hasLinked = false;

    private void Update()
    {
        if (hasLinked)
            return;

        if (handAsdToolController == null || secondaryUiPanelsRoot == null)
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
        Button sizeMinusButton = FindButton(panelRoot, "SizeMinusButton");
        Button sizePlusButton = FindButton(panelRoot, "SizePlusButton");
        Button componentPrevButton = FindButton(panelRoot, "ComponentPrevButton");
        Button componentNextButton = FindButton(panelRoot, "ComponentNextButton");

        TMP_Text currentSizeLabel = FindText(panelRoot, "CurrentSizeLabel");

        if (sizeMinusButton == null ||
            sizePlusButton == null ||
            componentPrevButton == null ||
            componentNextButton == null)
        {
            return false;
        }

        sizeMinusButton.onClick.RemoveAllListeners();
        sizePlusButton.onClick.RemoveAllListeners();
        componentPrevButton.onClick.RemoveAllListeners();
        componentNextButton.onClick.RemoveAllListeners();

        sizeMinusButton.onClick.AddListener(handAsdToolController.DecreaseSize);
        sizePlusButton.onClick.AddListener(handAsdToolController.IncreaseSize);
        componentPrevButton.onClick.AddListener(handAsdToolController.PreviousComponent);
        componentNextButton.onClick.AddListener(handAsdToolController.NextComponent);

        if (currentSizeLabel != null)
        {
            handAsdToolController.SetCurrentSizeLabel(currentSizeLabel);
        }

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

    private TMP_Text FindText(Transform root, string name)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                return child.GetComponent<TMP_Text>();
            }
        }

        return null;
    }
}