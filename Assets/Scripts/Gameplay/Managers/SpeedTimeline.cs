using System.Collections.Generic;
using UnityEngine;

public class SpeedTimeline : MonoBehaviour
{
    public static SpeedTimeline Instance { get; private set; }

    [SerializeField] private GameConfig gameConfig;

    private List<SpeedData> _speeds = new List<SpeedData>();

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

    public void SetSpeeds(List<SpeedData> speeds)
    {
        _speeds = speeds ?? new List<SpeedData>();
        _speeds.Sort((a, b) => a.ms.CompareTo(b.ms));
    }

    public float GetDistance(float fromMs, float toMs)
    {
        if (fromMs > toMs)
            return -GetDistance(toMs, fromMs);

        float totalDistance = 0f;
        float currentMs = fromMs;
        int speedIndex = FindStartIndex(fromMs);
        float currentMultiplier = speedIndex > 0 ? _speeds[speedIndex - 1].multiplier : 1f;

        while (currentMs < toMs)
        {
            float nextMs = (speedIndex < _speeds.Count) ? _speeds[speedIndex].ms : toMs;
            nextMs = Mathf.Min(nextMs, toMs);

            totalDistance += (nextMs - currentMs) / 1000f * Speed * currentMultiplier;

            if (speedIndex < _speeds.Count && nextMs == _speeds[speedIndex].ms)
            {
                currentMultiplier = _speeds[speedIndex].multiplier;
                speedIndex++;
            }

            currentMs = nextMs;
        }

        return totalDistance;
    }

    private int FindStartIndex(float ms)
    {
        int lo = 0;
        int hi = _speeds.Count - 1;
        int result = 0;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_speeds[mid].ms <= ms)
            {
                result = mid + 1;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result;
    }
}