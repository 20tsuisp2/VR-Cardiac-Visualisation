using UnityEngine;
using UnityEngine.UI;

public class HandPokeInvokeToggle : MonoBehaviour
{
    public Toggle targetToggle;
    [SerializeField] private float cooldown = 0.25f;
    [SerializeField] private bool forceOn = true;

    private float _lastPressTime = -999f;

    private void Reset()
    {
        if (targetToggle == null)
            targetToggle = GetComponent<Toggle>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastPressTime < cooldown)
            return;

        string n = other.gameObject.name.ToLower();

        if (!n.Contains("poke") &&
            !n.Contains("pinch") &&
            !n.Contains("handgrabproxy") &&
            !n.Contains("hand"))
        {
            return;
        }

        if (targetToggle != null)
        {
            _lastPressTime = Time.time;

            if (forceOn)
                targetToggle.isOn = true;
            else
                targetToggle.isOn = !targetToggle.isOn;

            Debug.Log("Invoking toggle: " + gameObject.name + " via " + other.gameObject.name);
        }
    }
}