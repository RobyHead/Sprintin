using UnityEngine;

public class TitlePanelAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private RectTransform groundRect;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Slide")]
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideOutDuration = 0.5f;

    public bool IsIntroComplete { get; private set; }
    public bool IsOutroComplete { get; private set; }

    private enum Phase { Idle, FadeIn, SlideIn, SlideOut, FadeOut }
    private Phase _phase;
    private float _timer;

    private Vector2 _titleTargetPos;
    private Vector2 _titleOffscreenPos;
    private Vector2 _groundTargetPos;
    private Vector2 _groundOffscreenPos;

    private void Awake()
    {
        _titleTargetPos = titleRect.anchoredPosition;
        _titleOffscreenPos = _titleTargetPos + new Vector2(0f, Screen.height);
        titleRect.anchoredPosition = _titleOffscreenPos;

        if (groundRect != null)
        {
            _groundTargetPos = groundRect.anchoredPosition;
            _groundOffscreenPos = _groundTargetPos + new Vector2(0f, -Screen.height);
            groundRect.anchoredPosition = _groundOffscreenPos;
        }

        canvasGroup.alpha = 0f;
    }

    public void PlayIntro()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        IsIntroComplete = false;
        _timer = 0f;
        _phase = Phase.FadeIn;
    }

    public void PlayOutro()
    {
        IsOutroComplete = false;
        _timer = 0f;
        _phase = Phase.SlideOut;
    }

    public void ResetToOffscreen()
    {
        _phase = Phase.Idle;
        IsIntroComplete = false;
        IsOutroComplete = false;
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
                float t = Mathf.Clamp01(_timer / fadeInDuration);
                canvasGroup.alpha = t;
                if (_timer >= fadeInDuration)
                {
                    _timer = 0f;
                    _phase = Phase.SlideIn;
                }
                break;
            }

            case Phase.SlideIn:
            {
                float t = Mathf.Clamp01(_timer / slideInDuration);
                t = EaseOutQuad(t);
                titleRect.anchoredPosition = Vector2.Lerp(_titleOffscreenPos, _titleTargetPos, t);
                if (groundRect != null)
                    groundRect.anchoredPosition = Vector2.Lerp(_groundOffscreenPos, _groundTargetPos, t);
                if (_timer >= slideInDuration)
                {
                    titleRect.anchoredPosition = _titleTargetPos;
                    if (groundRect != null)
                        groundRect.anchoredPosition = _groundTargetPos;
                    _phase = Phase.Idle;
                    IsIntroComplete = true;
                }
                break;
            }

            case Phase.SlideOut:
            {
                float t = Mathf.Clamp01(_timer / slideOutDuration);
                t = EaseInQuad(t);
                titleRect.anchoredPosition = Vector2.Lerp(_titleTargetPos, _titleOffscreenPos, t);
                if (groundRect != null)
                    groundRect.anchoredPosition = Vector2.Lerp(_groundTargetPos, _groundOffscreenPos, t);
                if (_timer >= slideOutDuration)
                {
                    _timer = 0f;
                    _phase = Phase.FadeOut;
                }
                break;
            }

            case Phase.FadeOut:
            {
                float t = Mathf.Clamp01(_timer / fadeOutDuration);
                canvasGroup.alpha = 1f - t;
                if (_timer >= fadeOutDuration)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.blocksRaycasts = false;
                    _phase = Phase.Idle;
                    IsOutroComplete = true;
                }
                break;
            }
        }
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t) => t * t;
}