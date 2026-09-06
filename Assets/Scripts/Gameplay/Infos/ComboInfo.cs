using System.Collections;
using TMPro;
using UnityEngine;

public class ComboInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Animation")]
    [SerializeField] private float scaleInDuration = 0.1f;
    [SerializeField] private float scaleOvershoot = 1f;
    [SerializeField] private float scaleSettleDuration = 0.05f;

    private int _combo;
    private Coroutine _animation;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
        text.text = "";
        SetAlpha(0f);
    }

    public void UpdateCombo(Judgement judgement)
    {
        if (judgement == Judgement.Bad || judgement == Judgement.Miss)
        {
            _combo = 0;
            SetAlpha(0f);
            text.text = "";
            return;
        }

        _combo++;

        if (_combo < 10)
        {
            SetAlpha(0f);
            text.text = "";
            return;
        }

        text.text = _combo.ToString();

        if (_animation != null)
            StopCoroutine(_animation);

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
    }

    private void SetAlpha(float alpha)
    {
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
}