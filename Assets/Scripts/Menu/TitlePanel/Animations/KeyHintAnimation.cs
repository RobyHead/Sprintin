using UnityEngine;

public class KeyHintAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float cyclePeriodMs = 2000f;
    [SerializeField] private float fadeOutDurationMs = 300f;

    private enum State { Idle, Blinking, FadingOut }
    private State _state = State.Idle;
    private float _timer;
    private float _fadeStartAlpha;

    private void Start()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void StartBlinking()
    {
        _state = State.Blinking;
        _timer = 0f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void StartFadeOut()
    {
        _state = State.FadingOut;
        _timer = 0f;
        _fadeStartAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
    }

    private void Update()
    {
        if (canvasGroup == null) return;

        switch (_state)
        {
            case State.Idle:
                break;

            case State.Blinking:
                _timer += Time.deltaTime;
                float timerMs = _timer * 1000f;
                float t = (timerMs % cyclePeriodMs) / cyclePeriodMs - 0.25f;
                canvasGroup.alpha = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) / 2f;
                break;

            case State.FadingOut:
                _timer += Time.deltaTime;
                float fadeT = Mathf.Clamp01(_timer * 1000f / fadeOutDurationMs);
                canvasGroup.alpha = Mathf.Lerp(_fadeStartAlpha, 0f, fadeT);
                if (_timer >= fadeOutDurationMs / 1000f)
                {
                    canvasGroup.alpha = 0f;
                    _state = State.Idle;
                }
                break;
        }
    }
}