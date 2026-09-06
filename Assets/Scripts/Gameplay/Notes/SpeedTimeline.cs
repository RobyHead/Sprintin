using UnityEngine;

public class SpeedTimeline : MonoBehaviour
{
    public static SpeedTimeline Instance { get; private set; }

    [SerializeField] private GameConfig gameConfig;

    public float Speed => gameConfig.Speed;
    public float VisibleRangeMin => gameConfig.VisibleRangeMin;
    public float VisibleRangeMax => gameConfig.VisibleRangeMax;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public float GetDistance(float fromMs, float toMs)
    {
        return (toMs - fromMs) / 1000f * Speed;
    }
}