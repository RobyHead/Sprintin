using System.Collections;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ResultInfo resultInfo;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.5f;

    private bool _shown;

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show()
    {
        if (_shown)
            return;
        _shown = true;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (Judge.Instance != null && RecordManager.Instance != null)
        {
            bool fullCombo = Judge.BadCount == 0 && Judge.MissCount == 0;
            RecordManager.Instance.UpdateRecord(
                SceneTransitionManager.Instance.PackId,
                SceneTransitionManager.Instance.SongId,
                SceneTransitionManager.Instance.DifficultyId,
                Judge.Instance.FinalScore,
                Judge.Instance.MaxCombo,
                fullCombo);
        }

        if (resultInfo != null)
            resultInfo.Populate();

        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}