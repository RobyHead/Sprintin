using UnityEngine;

public class TitlePanelAnimation : MonoBehaviour
{
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Ground")]
    [SerializeField] private RectTransform groundRect;

    [Header("Title")]
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private float slideInDuration = 2f;

    [Header("Exit")]
    [SerializeField] private float exitDuration = 0.5f;

    public bool IsSlideInComplete { get; private set; }

    private Vector2 _titleTargetPos;
    private Vector2 _titleOffscreenPos;
    private Vector2 _groundTargetPos;
    private Vector2 _groundOffscreenPos;

    private bool _isSlidingIn;
    private bool _isExiting;
    private float _slideInTimer;
    private float _exitTimer;

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

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        IsSlideInComplete = false;
    }

    public void PlayTitleSlideIn()
    {
        _isSlidingIn = true;
        _slideInTimer = 0f;
    }

    public void PlayExit()
    {
        _isExiting = true;
        _exitTimer = 0f;
    }

    public void ResetToOffscreen()
    {
        _isSlidingIn = false;
        _isExiting = false;
        IsSlideInComplete = false;
        titleRect.anchoredPosition = _titleOffscreenPos;
        if (groundRect != null)
            groundRect.anchoredPosition = _groundOffscreenPos;
    }

    private void Update()
    {
        if (_isSlidingIn)
        {
            _slideInTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_slideInTimer / slideInDuration);
            t = EaseOutQuad(t);
            titleRect.anchoredPosition = Vector2.Lerp(_titleOffscreenPos, _titleTargetPos, t);
            if (groundRect != null)
                groundRect.anchoredPosition = Vector2.Lerp(_groundOffscreenPos, _groundTargetPos, t);

            if (_slideInTimer >= slideInDuration)
            {
                _isSlidingIn = false;
                titleRect.anchoredPosition = _titleTargetPos;
                if (groundRect != null)
                    groundRect.anchoredPosition = _groundTargetPos;
                IsSlideInComplete = true;
            }
        }

        if (_isExiting)
        {
            _exitTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_exitTimer / exitDuration);
            t = EaseInQuad(t);
            titleRect.anchoredPosition = Vector2.Lerp(_titleTargetPos, _titleOffscreenPos, t);

            if (groundRect != null)
                groundRect.anchoredPosition = Vector2.Lerp(_groundTargetPos, _groundOffscreenPos, t);
        }
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t) => t * t;
}