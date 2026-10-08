using UnityEngine;

public class TitlePanelAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private RectTransform groundRect;

    [Header("Fade")]
    [SerializeField] private float fadeInDurationMs = 500f;
    [SerializeField] private float fadeOutDurationMs = 500f;

    [Header("Slide")]
    [SerializeField] private float slideInDurationMs = 1000f;
    [SerializeField] private float slideOutDurationMs = 500f;

    [Header("Size")]
    // [SerializeField] private float textureWidth = 1920f;
    [SerializeField] private float textureHeight = 1080f;

    public event System.Action OnIntroComplete;
    public event System.Action OnSlideOutComplete;
    public event System.Action OnOutroComplete;

    private enum Phase { Idle, FadeIn, SlideIn, SlideOut, WaitingForFadeOut, FadeOut }
    private Phase _phase;
    private float _timer;

    private Vector2 _titleTargetPos;
    private Vector2 _titleOffscreenPos;
    private Vector2 _groundTargetPos;
    private Vector2 _groundOffscreenPos;

    private void Awake()
    {
        if (titleRect != null)
        {
            _titleTargetPos = titleRect.anchoredPosition;
            _titleOffscreenPos = _titleTargetPos + new Vector2(0f, textureHeight);
            titleRect.anchoredPosition = _titleOffscreenPos;
        }

        if (groundRect != null)
        {
            _groundTargetPos = groundRect.anchoredPosition;
            _groundOffscreenPos = _groundTargetPos + new Vector2(0f, -textureHeight);
            groundRect.anchoredPosition = _groundOffscreenPos;
        }

        canvasGroup.alpha = 0f;
    }

    public void PlayIntro()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        _timer = 0f;
        _phase = Phase.FadeIn;
    }

    public void PlayOutro()
    {
        _timer = 0f;
        _phase = Phase.SlideOut;
    }

    public void StartFadeOut()
    {
        if (_phase == Phase.WaitingForFadeOut)
        {
            _timer = 0f;
            _phase = Phase.FadeOut;
        }
    }

    public void ResetToOffscreen()
    {
        _phase = Phase.Idle;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        titleRect.anchoredPosition = _titleOffscreenPos;
        if (groundRect != null)
            groundRect.anchoredPosition = _groundOffscreenPos;
    }

    private void Update()
    {
        if (_phase == Phase.Idle)
            return;

        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.FadeIn:
                {
                    float t = Mathf.Clamp01(_timer * 1000f / fadeInDurationMs);
                    canvasGroup.alpha = t;
                    if (_timer >= fadeInDurationMs / 1000f)
                    {
                        _timer = 0f;
                        _phase = Phase.SlideIn;
                    }
                    break;
                }

            case Phase.SlideIn:
                {
                    float t = Mathf.Clamp01(_timer * 1000f / slideInDurationMs);
                    t = EaseOutQuad(t);
                    titleRect.anchoredPosition = Vector2.Lerp(_titleOffscreenPos, _titleTargetPos, t);
                    if (groundRect != null)
                        groundRect.anchoredPosition = Vector2.Lerp(_groundOffscreenPos, _groundTargetPos, t);
                    if (_timer >= slideInDurationMs / 1000f)
                    {
                        titleRect.anchoredPosition = _titleTargetPos;
                        if (groundRect != null)
                            groundRect.anchoredPosition = _groundTargetPos;
                        _phase = Phase.Idle;
                        OnIntroComplete?.Invoke();
                    }
                    break;
                }

            case Phase.SlideOut:
                {
                    float t = Mathf.Clamp01(_timer * 1000f / slideOutDurationMs);
                    t = EaseInQuad(t);
                    titleRect.anchoredPosition = Vector2.Lerp(_titleTargetPos, _titleOffscreenPos, t);
                    if (groundRect != null)
                        groundRect.anchoredPosition = Vector2.Lerp(_groundTargetPos, _groundOffscreenPos, t);
                    if (_timer >= slideOutDurationMs / 1000f)
                    {
                        titleRect.anchoredPosition = _titleOffscreenPos;
                        if (groundRect != null)
                            groundRect.anchoredPosition = _groundOffscreenPos;
                        _timer = 0f;
                        _phase = Phase.WaitingForFadeOut;
                        OnSlideOutComplete?.Invoke();
                    }
                    break;
                }

            case Phase.WaitingForFadeOut:
                break;

            case Phase.FadeOut:
                {
                    float t = Mathf.Clamp01(_timer * 1000f / fadeOutDurationMs);
                    canvasGroup.alpha = 1f - t;
                    if (_timer >= fadeOutDurationMs / 1000f)
                    {
                        canvasGroup.alpha = 0f;
                        canvasGroup.blocksRaycasts = false;
                        _phase = Phase.Idle;
                        OnOutroComplete?.Invoke();
                    }
                    break;
                }
        }
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t) => t * t;
}