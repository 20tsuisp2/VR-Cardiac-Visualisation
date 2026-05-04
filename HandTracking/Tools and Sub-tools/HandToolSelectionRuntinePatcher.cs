using UnityEngine;
using UnityEngine.UI;

public class HandToolSelectionRuntimePatcher : MonoBehaviour
{
    [SerializeField] private Transform buttonParent;
    [SerializeField] private float scanInterval = 0.5f;

    private float _nextScanTime = 0f;

    private void Reset()
    {
        if (buttonParent == null)
        {
            Transform spacer = transform.Find("Spacer");
            if (spacer != null)
                buttonParent = spacer;
        }
    }

    private void Update()
    {
        if (Time.time < _nextScanTime)
            return;

        _nextScanTime = Time.time + scanInterval;
        PatchRuntimeButtons();
    }

    private void PatchRuntimeButtons()
    {
        if (buttonParent == null)
            return;

        Toggle[] toggles = buttonParent.GetComponentsInChildren<Toggle>(true);

        foreach (Toggle toggle in toggles)
        {
            HandPokeInvokeToggle pokeToggle =
                toggle.GetComponent<HandPokeInvokeToggle>();

            if (pokeToggle == null)
            {
                pokeToggle = toggle.gameObject.AddComponent<HandPokeInvokeToggle>();
                pokeToggle.targetToggle = toggle;
                Debug.Log("Added HandPokeInvokeToggle to " + toggle.gameObject.name);
            }
        }
    }
}