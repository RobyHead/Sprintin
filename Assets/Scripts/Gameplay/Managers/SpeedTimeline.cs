using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTimeline : MonoBehaviour
{
    public static SpeedTimeline Instance { get; private set; }

    [SerializeField] private GameConfig gameConfig;

    private List<SpeedData> _speeds = new List<SpeedData>();
    private List<StretchData> _stretchs = new List<StretchData>();
    private float _currentStretch = 1f;
    private int _activeStretchIndex = -1;
    private float _stretchFromValue = 1f;
    private float _environmentDistance;
    private float _lastFrameMs;

    public float Speed => gameConfig.Speed;
    public float VisibleRangeMin => gameConfig.VisibleRangeMin;
    public float VisibleRangeMax => gameConfig.VisibleRangeMax;

    private float CurrentMs => GameTime.Instance != null ? GameTime.Instance.ElapsedMs : 0f;

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

        if (_speeds.Count == 0 || _speeds[0].ms != 0)
        {
            _speeds.Insert(0, new SpeedData(0, 1f));
        }
    }

    public void SetStretchs(List<StretchData> stretchs)
    {
        _stretchs = stretchs ?? new List<StretchData>();
        _stretchs.Sort((a, b) =>
        {
            int cmp = a.ms.CompareTo(b.ms);
            if (cmp != 0) return cmp;
            return a.endms.CompareTo(b.endms);
        });

        if (_stretchs.Count == 0 || _stretchs[0].endms > 0)
        {
            _stretchs.Insert(0, new StretchData(0, 0, 1f, "s"));
        }

        _currentStretch = _stretchs[0].multiplier;
        _activeStretchIndex = 0;
        _stretchFromValue = _currentStretch;
        _environmentDistance = 0f;
        _lastFrameMs = 0f;
    }

    private void Update()
    {
        if (_stretchs.Count == 0)
            return;

        float now = CurrentMs;
        float ms = Mathf.Max(0f, now);

        while (_activeStretchIndex + 1 < _stretchs.Count && _stretchs[_activeStretchIndex + 1].ms <= ms)
        {
            _stretchFromValue = _stretchs[_activeStretchIndex].multiplier;
            _activeStretchIndex++;
        }

        var s = _stretchs[_activeStretchIndex];

        if (ms < s.endms)
        {
            float duration = s.endms - s.ms;
            float t = duration > 0f ? Mathf.Clamp01((ms - s.ms) / duration) : 1f;
            float progress = ApplyEasing(t, s.easing);
            _currentStretch = Mathf.Lerp(_stretchFromValue, s.multiplier, progress);
        }
        else
        {
            _currentStretch = s.multiplier;
        }

        float dt = now - _lastFrameMs;
        _lastFrameMs = now;
        dt = Mathf.Max(0f, dt);
        if (dt > 0f && _speeds.Count > 0)
        {
            float speedMul, stretch;
            if (now <= 0f)
            {
                speedMul = _speeds[0].multiplier;
                stretch = _stretchs[0].multiplier;
            }
            else
            {
                int idx = FindStartIndex(now);
                speedMul = idx > 0 ? _speeds[idx - 1].multiplier : _speeds[0].multiplier;
                stretch = _currentStretch;
            }
            _environmentDistance += dt / 1000f * Speed * speedMul * stretch;
        }
    }

    private static float ApplyEasing(float t, string easing)
    {
        switch (easing)
        {
            case "si":
                return Mathf.Sin(t * Mathf.PI * 0.5f);
            case "so":
                return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            default:
                return t;
        }
    }

    public float GetDistance(float toMs)
    {
        float fromMs = CurrentMs;
        if (Mathf.Approximately(fromMs, toMs))
            return 0f;

        float speedDist;
        if (fromMs > toMs)
            speedDist = -SpeedDistance(toMs, fromMs);
        else
            speedDist = SpeedDistance(fromMs, toMs);

        return speedDist * _currentStretch;
    }

    public float GetEnvironmentDistance()
    {
        return _environmentDistance;
    }

    private float SpeedDistance(float fromMs, float toMs)
    {
        float totalDistance = 0f;
        float currentMs = fromMs;
        int speedIndex = FindStartIndex(fromMs);
        float currentMultiplier = speedIndex > 0 ? _speeds[speedIndex - 1].multiplier : _speeds[0].multiplier;

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