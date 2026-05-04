using UnityEngine;

public class AnimationPanelController : MonoBehaviour
{
    private AnimationControl animationControl;

    void Start()
    {
        animationControl = FindObjectOfType<AnimationControl>();

        if (animationControl == null)
        {
            Debug.LogWarning("AnimationControl not found in scene.");
        }
    }

    public void TogglePlayPause()
    {
        if (animationControl == null)
            return;

        animationControl.On = !animationControl.On;
    }

    public void StepForward()
    {
        if (animationControl == null)
            return;

        animationControl.StepForward();
    }

    public void StepBackward()
    {
        if (animationControl == null)
            return;

        animationControl.StepBackward();
    }

    public void SpeedUp()
    {
        if (animationControl == null)
            return;

        animationControl.SpeedUp();
    }

    public void SpeedDown()
    {
        if (animationControl == null)
            return;

        animationControl.SpeedDown();
    }
}