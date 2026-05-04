using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GazeDwellToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float dwellTime = 1.5f;

    private float timer;
    private bool gazing;
    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    void Update()
    {
        if (!gazing) return;

        timer += Time.deltaTime;

        float progress = timer / dwellTime;
        if (DwellIndicatorController.Instance != null)
            DwellIndicatorController.Instance.SetProgress(progress);

        if (timer >= dwellTime)
        {
            toggle.isOn = true;
            gazing = false;
            timer = 0f;

            if (DwellIndicatorController.Instance != null)
                DwellIndicatorController.Instance.Hide();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gazing = true;
        timer = 0f;

        if (DwellIndicatorController.Instance != null)
            DwellIndicatorController.Instance.SetProgress(0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gazing = false;
        timer = 0f;

        if (DwellIndicatorController.Instance != null)
            DwellIndicatorController.Instance.Hide();
    }
}
