using UnityEngine;
using TMPro;

public class GameTime : MonoBehaviour
{
    public static GameTime Instance { get; private set; }

    [SerializeField] private GameConfig gameConfig;

    public float ElapsedMs { get; private set; } = -3000f;
    public bool HasStarted { get; private set; }

    private float _startOffsetMs = -3000f;

    private double _startTime;

    private void Awake()
    {
        Instance = this;
        _startOffsetMs = -gameConfig.BlankMs;
        ElapsedMs = _startOffsetMs;
        HasStarted = false;
    }

    private void OnDestroy()
    {
        if (ChartManager.Instance != null)
        {
            ChartManager.Instance.OnGameEnded -= HandleGameEnded;
            ChartManager.Instance.OnReady -= HandleReady;
        }
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        ChartManager.Instance.OnGameEnded += HandleGameEnded;
        ChartManager.Instance.OnReady += HandleReady;
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

    private void Update()
    {
        if (!HasStarted)
        {
            ElapsedMs = _startOffsetMs;
            return;
        }

        ElapsedMs = (float)((AudioSettings.dspTime - _startTime) * 1000.0) + _startOffsetMs;
    }
}