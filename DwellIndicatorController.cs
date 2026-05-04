using UnityEngine;

public class DwellIndicatorController : MonoBehaviour
{
    public static DwellIndicatorController Instance;

    public Transform progressVisual;
    public Renderer progressRenderer;

    public Vector3 startScale = new Vector3(0.007f, 0.007f, 0.007f);
    public Vector3 fullScale = new Vector3(0.014f, 0.014f, 0.014f);

    public Color startColor = Color.white;
    public Color fullColor = Color.green;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void SetProgress(float progress01)
    {
        progress01 = Mathf.Clamp01(progress01);

        if (progressVisual != null)
        {
            progressVisual.gameObject.SetActive(true);
            progressVisual.localScale = Vector3.Lerp(startScale, fullScale, progress01);
        }

        if (progressRenderer != null)
        {
            progressRenderer.material.color = Color.Lerp(startColor, fullColor, progress01);
        }
    }

    public void Hide()
    {
        if (progressVisual != null)
        {
            progressVisual.gameObject.SetActive(true);
            progressVisual.localScale = startScale;
        }

        if (progressRenderer != null)
        {
            progressRenderer.material.color = startColor;
        }
    }
}