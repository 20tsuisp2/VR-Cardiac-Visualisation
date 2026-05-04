using System.Collections.Generic;
using UnityEngine;

public class HandASDDeleteAttacher : MonoBehaviour
{
    [Header("Delete Button")]
    [SerializeField] private GameObject deleteButtonPrefab;

    [Header("Ignore Preview")]
    [SerializeField] private Transform handToolPoint;

    [Header("Optional Search Root")]
    [SerializeField] private Transform placedDeviceParent;

    [Header("Find Devices")]
    [SerializeField] private string deviceRootNameContains = "DeviceV1";

    [Header("Attach To Label")]
    [SerializeField] private string labelChildName = "Label";
    [SerializeField] private Vector3 buttonLocalPosition = new Vector3(0.05f, 0f, 0f);
    [SerializeField] private Vector3 buttonLocalEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 buttonLocalScale = Vector3.one * 0.5f;

    [Header("Scan")]
    [SerializeField] private float scanInterval = 0.5f;

    private float scanTimer = 0f;

    private void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f)
            return;

        scanTimer = scanInterval;
        AttachDeleteButtonsToPlacedDevices();
    }

    private void AttachDeleteButtonsToPlacedDevices()
    {
        List<GameObject> candidates = GetCandidateDeviceRoots();

        foreach (GameObject root in candidates)
        {
            if (root == null)
                continue;

            if (AlreadyHasHandDeleteButton(root))
                continue;

            Transform label = root.transform.Find(labelChildName);
            if (label == null)
                continue;

            SpawnDeleteButton(root, label);
        }
    }

    private List<GameObject> GetCandidateDeviceRoots()
    {
        List<GameObject> results = new List<GameObject>();

        if (placedDeviceParent != null)
        {
            for (int i = 0; i < placedDeviceParent.childCount; i++)
            {
                Transform child = placedDeviceParent.GetChild(i);
                if (IsValidPlacedDevice(child.gameObject))
                {
                    results.Add(child.gameObject);
                }
            }

            return results;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in allObjects)
        {
            if (obj == null)
                continue;

            if (!obj.name.Contains(deviceRootNameContains))
                continue;

            if (obj.transform.parent != null)
                continue;

            if (IsValidPlacedDevice(obj))
            {
                results.Add(obj);
            }
        }

        return results;
    }

    private bool IsValidPlacedDevice(GameObject root)
    {
        if (root == null)
            return false;

        if (handToolPoint != null && root.transform.IsChildOf(handToolPoint))
            return false;

        if (root.name.Contains("_HandPreview"))
            return false;

        return true;
    }

    private bool AlreadyHasHandDeleteButton(GameObject root)
    {
        return root.GetComponentInChildren<HandPlacedDeviceDeleteButton>(true) != null;
    }

    private void SpawnDeleteButton(GameObject targetRoot, Transform labelAnchor)
    {
        if (deleteButtonPrefab == null)
        {
            Debug.LogError("[HandASDDeleteAttacher] DeleteButton prefab not assigned.");
            return;
        }

        GameObject button = Instantiate(deleteButtonPrefab, labelAnchor);
        button.name = "HandDeleteButton";

        button.transform.localPosition = buttonLocalPosition;
        button.transform.localRotation = Quaternion.Euler(buttonLocalEulerAngles);
        button.transform.localScale = buttonLocalScale;

        RemoveOriginalDeleteLogic(button);

        HandPlacedDeviceDeleteButton deleteScript =
            button.GetComponent<HandPlacedDeviceDeleteButton>();

        if (deleteScript == null)
        {
            deleteScript = button.AddComponent<HandPlacedDeviceDeleteButton>();
        }

        deleteScript.SetTargetRoot(targetRoot);

        EnsureTriggerCollider(button);
    }

    private void RemoveOriginalDeleteLogic(GameObject button)
    {
        MonoBehaviour[] behaviours = button.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour is HandPlacedDeviceDeleteButton)
                continue;

            string typeName = behaviour.GetType().Name.ToLower();

            if (typeName.Contains("delete"))
            {
                Destroy(behaviour);
            }
        }
    }

    private void EnsureTriggerCollider(GameObject button)
    {
        Collider col = button.GetComponentInChildren<Collider>(true);

        if (col != null)
        {
            col.isTrigger = true;
            return;
        }

        BoxCollider newCol = button.AddComponent<BoxCollider>();
        newCol.isTrigger = true;
    }
}