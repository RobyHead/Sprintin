using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameTime : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;

    private static float s_startOffsetMs = -3000f;

    public static float ElapsedMs { get; private set; } = -3000f;
    public static bool HasStarted { get; private set; }

    private void Awake()
    {
        s_startOffsetMs = -gameConfig.BlankMs;
        ElapsedMs = s_startOffsetMs;
    }

    public static void Reset()
    {
        ElapsedMs = s_startOffsetMs;
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
                ElapsedMs = s_startOffsetMs;
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
            ElapsedMs = (float)((AudioSettings.dspTime - _startTime) * 1000.0) + s_startOffsetMs;
        }

        if (timeText != null)
        {
            timeText.text = $"{(int)ElapsedMs}";
        }
    }
}