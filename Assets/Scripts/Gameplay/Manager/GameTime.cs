using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameTime : MonoBehaviour
{
    private const float StartOffsetMs = -3000f;

    public static float ElapsedMs { get; private set; } = StartOffsetMs;
    public static bool HasStarted { get; private set; }

    public static void Reset()
    {
        ElapsedMs = StartOffsetMs;
        HasStarted = false;
    }

    [Header("Display")]
    [SerializeField] private TMP_Text timeText;

    private double _startTime;

    private void Update()
    {
        if (!HasStarted)
        {
            if (!ChartManager.IsReady)
            {
                ElapsedMs = StartOffsetMs;
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            {
                HasStarted = true;
                _startTime = AudioSettings.dspTime;
            }
        }

        if (HasStarted)
        {
            ElapsedMs = (float)((AudioSettings.dspTime - _startTime) * 1000.0) + StartOffsetMs;
        }

        if (timeText != null)
        {
            timeText.text = $"{(int)ElapsedMs}";
        }
    }
}