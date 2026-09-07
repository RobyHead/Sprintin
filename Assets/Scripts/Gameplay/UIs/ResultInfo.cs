using TMPro;
using UnityEngine;

public class ResultInfo : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highestScoreText;

    [Header("Combo")]
    [SerializeField] private TMP_Text comboText;

    [Header("Judgements")]
    [SerializeField] private TMP_Text perfectText;
    [SerializeField] private TMP_Text greatText;
    [SerializeField] private TMP_Text badText;
    [SerializeField] private TMP_Text missText;

    [Header("Record")]
    [SerializeField] private int defaultRecord = 1000000;

    public void Populate()
    {
        var judge = Judge.Instance;
        if (judge == null)
        {
            Debug.LogError("ResultInfo: Judge.Instance is null");
            return;
        }

        int score = judge.FinalScore;
        int maxCombo = judge.MaxCombo;

        if (scoreText != null)
            scoreText.text = score == 1000000 ? score.ToString() : score.ToString("D6");

        if (highestScoreText != null)
            highestScoreText.text = $"Best:\n{defaultRecord}";

        if (comboText != null)
            comboText.text = $"Combo:\n{maxCombo}";

        if (perfectText != null)
            perfectText.text = Judge.PerfectCount.ToString();

        if (greatText != null)
            greatText.text = Judge.GreatCount.ToString();

        if (badText != null)
            badText.text = Judge.BadCount.ToString();

        if (missText != null)
            missText.text = Judge.MissCount.ToString();
    }
}