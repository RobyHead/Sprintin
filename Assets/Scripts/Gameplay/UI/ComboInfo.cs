using System.Collections;
using TMPro;
using UnityEngine;

public class ComboInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Animation")]
    [SerializeField] private float scaleInDurationMs = 100f;
    [SerializeField] private float scaleOvershoot = 1f;
    [SerializeField] private float scaleSettleDurationMs = 50f;

    private Coroutine _animation;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
        text.text = "";
        SetAlpha(0f);
    }

    private void Start()
    {
        Judge.Instance.OnJudged += OnJudged;
    }

    private void OnDestroy()
    {
        if (Judge.Instance != null)
            Judge.Instance.OnJudged -= OnJudged;
    }

    private void OnJudged(Judgement judgement)
    {
        var judge = Judge.Instance;
        if (judge == null)
            return;

        int combo = judge.Combo;

        if (combo < 10)
        {
            SetAlpha(0f);
            text.text = "";
            return;
        }

        text.text = combo.ToString();

        if (_animation != null)
            StopCoroutine(_animation);

        SetAlpha(1f);
        transform.localScale = _baseScale * 0.5f;
        _animation = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        float durationS = scaleInDurationMs / 1000f;
        while (elapsed < durationS)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / durationS);
            float scale = Mathf.Lerp(0.5f, scaleOvershoot, t);
            transform.localScale = _baseScale * scale;
            yield return null;
        }

        elapsed = 0f;
        durationS = scaleSettleDurationMs / 1000f;
        while (elapsed < durationS)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / durationS);
            float scale = Mathf.Lerp(scaleOvershoot, 1f, t);
            transform.localScale = _baseScale * scale;
            yield return null;
        }
        transform.localScale = _baseScale;
    }

    private void SetAlpha(float alpha)
    {
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
}