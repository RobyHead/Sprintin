using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text label;

    private List<TextEffectData> _effects;
    private int _currentIndex;

    private void Start()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Initialize(List<TextEffectData> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            _effects = null;
            return;
        }

        _effects = new List<TextEffectData>(effects);
        _effects.Sort((a, b) => a.ms.CompareTo(b.ms));
        _currentIndex = 0;
    }

    private void Update()
    {
        if (_effects == null || !GameTime.HasStarted)
            return;

        float elapsed = GameTime.ElapsedMs;

        while (_currentIndex < _effects.Count)
        {
            var e = _effects[_currentIndex];
            int endMs = e.ms + e.fadeInMs + e.holdMs + e.fadeOutMs;
            if (elapsed >= endMs)
                _currentIndex++;
            else
                break;
        }

        if (_currentIndex >= _effects.Count)
        {
            SetAlpha(0f);
            return;
        }

        var effect = _effects[_currentIndex];
        float t = elapsed - effect.ms;

        if (t < 0f)
        {
            SetAlpha(0f);
            return;
        }

        float alpha;

        if (t < effect.fadeInMs)
        {
            alpha = effect.fadeInMs > 0 ? t / effect.fadeInMs : 1f;
        }
        else if (t < effect.fadeInMs + effect.holdMs)
        {
            alpha = 1f;
        }
        else if (t < effect.fadeInMs + effect.holdMs + effect.fadeOutMs)
        {
            float fadeOutT = t - effect.fadeInMs - effect.holdMs;
            alpha = effect.fadeOutMs > 0 ? 1f - fadeOutT / effect.fadeOutMs : 0f;
        }
        else
        {
            alpha = 0f;
        }

        label.text = effect.content;
        SetAlpha(alpha);
    }

    private void SetAlpha(float a)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = a;
    }

    public void Clear()
    {
        _effects = null;
        SetAlpha(0f);
    }
}