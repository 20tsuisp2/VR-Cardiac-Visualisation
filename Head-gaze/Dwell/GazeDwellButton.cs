using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GazeDwellButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float dwellTime = 1.5f;

    private float timer;
    private bool gazing;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
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
            button.onClick.Invoke();
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
