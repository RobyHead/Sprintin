using UnityEngine;
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
        _waitElapsedMs = 0f;
    }

    private void OnEnable()
    {
        ChartManager.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        ChartManager.OnGameEnded -= HandleGameEnded;
    }

    private void HandleGameEnded()
    {
        enabled = false;
    }

    public static void Reset()
    {
        ElapsedMs = s_startOffsetMs;
        HasStarted = false;
    }

    [Header("Display")]
    [SerializeField] private TMP_Text timeText;

    private double _startTime;
    private float _waitElapsedMs;

    private void Update()
    {
        if (!HasStarted)
        {
            if (!ChartManager.IsReady)
            {
                ElapsedMs = s_startOffsetMs;
                _waitElapsedMs = 0f;
                return;
            }

            _waitElapsedMs += Time.deltaTime * 1000f;
            if (_waitElapsedMs >= gameConfig.AutoStartDelayMs)
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