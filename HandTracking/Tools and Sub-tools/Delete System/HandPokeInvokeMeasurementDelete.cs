using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HandPokeInvokeMeasurementDelete : MonoBehaviour
{
    [SerializeField] private string validHandColliderName = "HandGrabProxy";

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered)
            return;

        if (!other.name.Contains(validHandColliderName))
            return;

        alreadyTriggered = true;

        Debug.Log("[HandPokeInvokeMeasurementDelete] Hand entered DeleteButton, invoking delete");

        // 1. Try Unity UI Button onClick first
        Button uiButton = GetComponent<Button>();
        if (uiButton != null)
        {
            Debug.Log("[HandPokeInvokeMeasurementDelete] Invoking Button.onClick");
            uiButton.onClick.Invoke();
            return;
        }

        // 2. Try ClickButtonAnimation.OnClick()
        MonoBehaviour[] allMonos = GetComponents<MonoBehaviour>();
        foreach (var mono in allMonos)
        {
            if (mono == null)
                continue;

            var type = mono.GetType();

            if (type.Name == "ClickButtonAnimation")
            {
                MethodInfo onClickMethod = type.GetMethod(
                    "OnClick",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (onClickMethod != null)
                {
                    Debug.Log("[HandPokeInvokeMeasurementDelete] Invoking ClickButtonAnimation.OnClick()");
                    onClickMethod.Invoke(mono, null);
                }
            }
        }

        // 3. Then try any UnityEvent fields on any attached scripts
        foreach (var mono in allMonos)
        {
            if (mono == null)
                continue;

            var type = mono.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (typeof(UnityEvent).IsAssignableFrom(field.FieldType))
                {
                    var evt = field.GetValue(mono) as UnityEvent;
                    if (evt != null)
                    {
                        Debug.Log("[HandPokeInvokeMeasurementDelete] Invoking UnityEvent field: " + field.Name);
                        evt.Invoke();
                        return;
                    }
                }
            }
        }

        Debug.LogWarning("[HandPokeInvokeMeasurementDelete] No actual delete action found on " + gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains(validHandColliderName))
            return;

        alreadyTriggered = false;
    }
}