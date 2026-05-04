using UnityEngine;

public class HandMeasurementDeleteRuntimePatcher : MonoBehaviour
{
    private void Update()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            if (go == null)
                continue;

            if (!go.name.Contains("Measurement_MkXII_delete(Clone)"))
                continue;

            Transform deleteButton = go.transform.Find("Label/DeleteButton");
            if (deleteButton == null)
                continue;

            if (deleteButton.GetComponent<HandPokeInvokeMeasurementDelete>() == null)
            {
                deleteButton.gameObject.AddComponent<HandPokeInvokeMeasurementDelete>();
                Debug.Log("[HandMeasurementDeleteRuntimePatcher] Patched DeleteButton on " + go.name);
            }
        }
    }
}