using System.Collections;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ResultInfo resultInfo;

    [Header("Timing")]
    [SerializeField] private float delayMs = 3000f;
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

    private void Update()
    {
        if (_shown)
            return;

        if (!GameTime.HasStarted)
            return;

        if (GameTime.ElapsedMs >= ChartManager.LastNoteMs + delayMs)
        {
            _shown = true;
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        if (Judge.Instance != null && RecordManager.Instance != null)
        {
            bool fullCombo = Judge.BadCount == 0 && Judge.MissCount == 0;
            RecordManager.Instance.UpdateRecord(
                SceneTransition.PackId,
                SceneTransition.SongId,
                SceneTransition.DifficultyId,
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