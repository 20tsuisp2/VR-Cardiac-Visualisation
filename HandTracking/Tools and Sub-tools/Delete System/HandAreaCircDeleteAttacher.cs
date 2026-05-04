using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class HandAreaCircDeleteAttacher : MonoBehaviour
{
    [SerializeField] private GameObject deleteButtonPrefab;
    [SerializeField] private Vector3 localOffset = new Vector3(0.03f, 0.0f, 0.0f);
    [SerializeField] private Vector3 localScale = Vector3.one;

    private Type areaCircMeasurementType;

    private void OnEnable()
    {
        areaCircMeasurementType = FindTypeByName("AreaCircumferenceMeasurement");
    }

    private void Start()
    {
        InvokeRepeating(nameof(AttachDeleteButtons), 1f, 1f);
    }

    private void AttachDeleteButtons()
    {
        if (deleteButtonPrefab == null)
            return;

        if (areaCircMeasurementType == null)
        {
            areaCircMeasurementType = FindTypeByName("AreaCircumferenceMeasurement");
            if (areaCircMeasurementType == null)
                return;
        }

        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        foreach (MonoBehaviour mb in allBehaviours)
        {
            if (mb == null)
                continue;

            if (mb.GetType() != areaCircMeasurementType)
                continue;

            GameObject measurementRoot = mb.gameObject;

            bool closed = GetBoolMember(mb, "Closed");
            if (!closed)
                continue;

            Transform label = measurementRoot.transform.Find("Label");
            if (label == null)
                continue;

            if (label.Find("DeleteButtonHand") != null)
                continue;

            CreateDeleteButton(label, measurementRoot);
        }
    }

    private void CreateDeleteButton(Transform label, GameObject measurementRoot)
    {
        // Try to anchor to the same UI row as the ExportButton
        Transform spacer = label.Find("LabelPhysics/ExportUI/Spacer");
        Transform exportButton = label.Find("LabelPhysics/ExportUI/Spacer/ExportButton");

        Transform parentForButton = spacer != null ? spacer : label;

        GameObject button = Instantiate(deleteButtonPrefab, parentForButton);
        button.name = "DeleteButtonHand";

        // Remove original behaviour that assumes controller scene logic
        Component deleteByUi = button.GetComponent("DeleteObjectByUi");
        if (deleteByUi != null)
            Destroy(deleteByUi);

        Button uiButton = button.GetComponent<Button>();
        if (uiButton != null)
            uiButton.onClick.RemoveAllListeners();

        HandAreaCircDeleteButton handDelete = button.GetComponent<HandAreaCircDeleteButton>();
        if (handDelete == null)
            handDelete = button.AddComponent<HandAreaCircDeleteButton>();

        handDelete.SetMeasurementRoot(measurementRoot);

        // If this is a UI-style prefab, position it with RectTransform, not localPosition
        RectTransform rt = button.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            if (exportButton != null)
            {
                RectTransform exportRt = exportButton.GetComponent<RectTransform>();
                if (exportRt != null)
                {
                    // Put it just to the right of the ExportButton
                    float x = exportRt.anchoredPosition.x + exportRt.rect.width + 12f;
                    float y = exportRt.anchoredPosition.y;
                    rt.anchoredPosition = new Vector2(x, y);
                }
                else
                {
                    rt.anchoredPosition = new Vector2(60f, 0f);
                }
            }
            else
            {
                rt.anchoredPosition = new Vector2(60f, 0f);
            }
        }
        else
        {
            // Fallback for non-UI prefabs
            button.transform.localPosition = new Vector3(0.06f, 0f, 0f);
            button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;
        }

        Debug.Log("[HandAreaCircDeleteAttacher] Added anchored delete button to " + measurementRoot.name);
    }

    private bool GetBoolMember(object obj, string name)
    {
        if (obj == null)
            return false;

        Type t = obj.GetType();

        FieldInfo field = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(obj);

        PropertyInfo prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(bool))
            return (bool)prop.GetValue(obj);

        return false;
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