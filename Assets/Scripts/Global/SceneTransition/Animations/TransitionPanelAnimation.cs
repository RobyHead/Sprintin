using UnityEngine;

public class TransitionPanelAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("Timing")]
    [SerializeField] private float outroDuration = 0.4f;
    [SerializeField] private float introDuration = 0.4f;

    [Header("Layout")]
    [SerializeField] private float slideDistance = 1080f;

    public event System.Action OnOutroComplete;
    public event System.Action OnIntroComplete;

    private enum Phase { Idle, Outro, Intro }
    private Phase _phase;
    private float _timer;

    private Vector2 _centerPos;
    private Vector2 _topOffscreenPos;
    private Vector2 _bottomOffscreenPos;

    private void Awake()
    {
        _centerPos = panelRect.anchoredPosition;
        _topOffscreenPos = _centerPos + new Vector2(0f, slideDistance);
        _bottomOffscreenPos = _centerPos + new Vector2(0f, -slideDistance);

        panelRect.anchoredPosition = _topOffscreenPos;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
    }

    public void PlayOutro()
    {
        _timer = 0f;
        _phase = Phase.Outro;
        panelRect.anchoredPosition = _topOffscreenPos;
        canvasGroup.blocksRaycasts = true;
    }

    public void PlayIntro()
    {
        _timer = 0f;
        _phase = Phase.Intro;
        panelRect.anchoredPosition = _centerPos;
    }

    public void ResetToIdle()
    {
        _phase = Phase.Idle;
        panelRect.anchoredPosition = _topOffscreenPos;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (_phase == Phase.Idle)
            return;

        _timer += Time.unscaledDeltaTime;

        switch (_phase)
        {
            case Phase.Outro:
            {
                float t = Mathf.Clamp01(_timer / outroDuration);
                t = EaseOutQuad(t);
                panelRect.anchoredPosition = Vector2.Lerp(_topOffscreenPos, _centerPos, t);
                if (_timer >= outroDuration)
                {
                    panelRect.anchoredPosition = _centerPos;
                    _phase = Phase.Idle;
                    OnOutroComplete?.Invoke();
                }
                break;
            }

            case Phase.Intro:
            {
                float t = Mathf.Clamp01(_timer / introDuration);
                t = EaseInQuad(t);
                panelRect.anchoredPosition = Vector2.Lerp(_centerPos, _bottomOffscreenPos, t);
                if (_timer >= introDuration)
                {
                    panelRect.anchoredPosition = _bottomOffscreenPos;
                    canvasGroup.blocksRaycasts = false;
                    _phase = Phase.Idle;
                    OnIntroComplete?.Invoke();
                }
                break;
            }
        }
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t) => t * t;
}