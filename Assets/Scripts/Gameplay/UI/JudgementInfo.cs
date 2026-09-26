using System.Collections;
using TMPro;
using UnityEngine;

public class JudgementInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Animation")]
    [SerializeField] private float scaleInDurationMs = 100f;
    [SerializeField] private float scaleOvershoot = 1f;
    [SerializeField] private float scaleSettleDurationMs = 50f;
    [SerializeField] private float fadeDelayMs = 500f;
    [SerializeField] private float fadeDurationMs = 500f;

    private Coroutine _animation;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
        text.text = "";
        SetAlpha(0f);
    }

    public void Show(Judgement judgement)
    {
        if (_animation != null)
            StopCoroutine(_animation);

        text.text = judgement.ToString();
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

        yield return new WaitForSeconds(fadeDelayMs / 1000f);

        elapsed = 0f;
        durationS = fadeDurationMs / 1000f;
        while (elapsed < durationS)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / durationS));
            yield return null;
        }
        SetAlpha(0f);
        text.text = "";
    }

    private void SetAlpha(float alpha)
    {
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
}