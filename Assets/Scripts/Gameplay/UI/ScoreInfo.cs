using TMPro;
using UnityEngine;

public class ScoreInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        text.text = "";
    }

    private void OnEnable()
    {
        Judge.Instance.OnJudged += OnJudged;
    }

    private void OnDisable()
    {
        Judge.Instance.OnJudged -= OnJudged;
    }

    private void OnJudged(Judgement judgement)
    {
        if (Judge.Instance != null)
            text.text = $"{Judge.Instance.FinalScore:D6}";
    }
}