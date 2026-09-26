using UnityEngine;
using TMPro;

public class GameTime : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;

    private static float _startOffsetMs = -3000f;

    public static float ElapsedMs { get; private set; } = -3000f;
    public static bool HasStarted { get; private set; }

    private void Awake()
    {
        _startOffsetMs = -gameConfig.BlankMs;
        ElapsedMs = _startOffsetMs;
    }

    private void OnEnable()
    {
        ChartManager.OnGameEnded += HandleGameEnded;
        ChartManager.OnReady += HandleReady;
    }

    private void OnDisable()
    {
        ChartManager.OnGameEnded -= HandleGameEnded;
        ChartManager.OnReady -= HandleReady;
    }

    private void HandleGameEnded()
    {
        enabled = false;
    }

    private void HandleReady()
    {
        HasStarted = true;
        _startTime = AudioSettings.dspTime;
    }

    public static void Reset()
    {
        ElapsedMs = _startOffsetMs;
        HasStarted = false;
    }

    [Header("Display")]
    [SerializeField] private TMP_Text timeText;

    private double _startTime;

    private void Update()
    {
        if (!HasStarted)
        {
            ElapsedMs = _startOffsetMs;
            return;
        }

        ElapsedMs = (float)((AudioSettings.dspTime - _startTime) * 1000.0) + _startOffsetMs;

        if (timeText != null)
        {
            timeText.text = $"{(int)ElapsedMs}";
        }
    }
}