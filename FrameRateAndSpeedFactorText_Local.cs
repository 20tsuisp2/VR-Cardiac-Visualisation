using System;
using UnityEngine;
using TMPro;

public class FrameRateAndSpeedFactorText_Local : MonoBehaviour
{
    private AnimationControl _animationControl;
    private TextMeshPro _textUi;

    private void OnEnable()
    {
        EventManager.StartListening("AnimationControlInitialised", UpdateText);
        EventManager.StartListening("SpeedFactorChanged", UpdateText);
    }

    private void OnDisable()
    {
        EventManager.StopListening("AnimationControlInitialised", UpdateText);
        EventManager.StopListening("SpeedFactorChanged", UpdateText);
    }

    void Start()
    {
        _animationControl = FindObjectOfType<AnimationControl>();
        _textUi = GetComponent<TextMeshPro>();

        if (_animationControl == null)
        {
            Debug.LogError("FrameRateAndSpeedFactorText_Local cannot find AnimationControl");
            return;
        }

        if (_textUi == null)
        {
            Debug.LogError("FrameRateAndSpeedFactorText_Local cannot find TextMeshPro");
            return;
        }

        UpdateText();
    }

    public static string GetFrameRateSpeedText(float frameIntervalS, float speedFactor)
    {
        if (frameIntervalS <= 0.0f || speedFactor <= 0.0f || speedFactor > 1.0f)
        {
            return "Invalid Data";
        }

        float frameRateFPS = 1.0f / frameIntervalS;

        return frameRateFPS.ToString("0.0") + " fps    " +
               "Speed: x" + speedFactor.ToString("0.00");
    }

    private void UpdateText()
    {
        if (_animationControl == null || _textUi == null)
            return;

        _textUi.text = GetFrameRateSpeedText(
            _animationControl.FactoredFrameIntervalS,
            _animationControl.SpeedFactor);
    }
}