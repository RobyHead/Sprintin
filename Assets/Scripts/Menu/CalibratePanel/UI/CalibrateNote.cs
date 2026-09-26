using UnityEngine;

public class CalibrateNote : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 300f;

    [Header("Visibility")]
    [SerializeField] private float visibleRangeMin = -50f;
    [SerializeField] private float visibleRangeMax = 800f;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private double _targetDspTime;
    private bool _isMoving;

    public float CurrentX => _rectTransform != null ? _rectTransform.anchoredPosition.x : 0f;
    public bool IsVisible { get; private set; }
    public bool IsMoving => _isMoving;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        _canvasGroup.alpha = 0f;
    }

    public void StartMoving(double targetDspTime)
    {
        _targetDspTime = targetDspTime;
        _isMoving = true;
    }

    public void Stop()
    {
        _isMoving = false;
        SetVisible(false);
    }

    private void Update()
    {
        if (!_isMoving)
            return;

        double remaining = _targetDspTime - AudioSettings.dspTime;
        float x = (float)(remaining * speed);

        _rectTransform.anchoredPosition = new Vector2(x, _rectTransform.anchoredPosition.y);

        bool inRange = x >= visibleRangeMin && x <= visibleRangeMax;
        SetVisible(inRange);
    }

    private void SetVisible(bool visible)
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = visible ? 1f : 0f;
        IsVisible = visible;
    }
}