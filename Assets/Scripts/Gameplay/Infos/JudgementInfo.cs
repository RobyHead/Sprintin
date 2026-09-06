using System.Collections;
using TMPro;
using UnityEngine;

public class JudgementInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Animation")]
    [SerializeField] private float scaleInDuration = 0.1f;
    [SerializeField] private float scaleOvershoot = 1f;
    [SerializeField] private float scaleSettleDuration = 0.05f;
    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;

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
        while (elapsed < scaleInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleInDuration);
            float scale = Mathf.Lerp(0.5f, scaleOvershoot, t);
            transform.localScale = _baseScale * scale;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < scaleSettleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleSettleDuration);
            float scale = Mathf.Lerp(scaleOvershoot, 1f, t);
            transform.localScale = _baseScale * scale;
            yield return null;
        }
        transform.localScale = _baseScale;

        yield return new WaitForSeconds(fadeDelay);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
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