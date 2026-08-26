using TMPro;
using UnityEngine;

public class ScoreInfo : MonoBehaviour
{
    public static ScoreInfo Instance { get; private set; }

    [SerializeField] private TMP_Text text;

    private int _currentScore;
    private int _maxScore;

    private void Awake()
    {
        Instance = this;
        text.text = "";
    }

    public static void CalculateMaxScore(int tapCount, int groundCount, int holdCount)
    {
        if (Instance == null)
            return;

        Instance.CalculateMaxScoreInternal(tapCount, groundCount, holdCount);
    }

    private void CalculateMaxScoreInternal(int tapCount, int groundCount, int holdCount)
    {
        int n = tapCount + groundCount + holdCount * 2;
        _maxScore = n * 3;
        _currentScore = 0;
        UpdateDisplay();
    }

    public static void AddScore(Judgement judgement)
    {
        if (Instance == null)
            return;

        Instance.AddScoreInternal(judgement);
    }

    private void AddScoreInternal(Judgement judgement)
    {
        _currentScore += judgement switch
        {
            Judgement.Perfect => 3,
            Judgement.Great => 2,
            Judgement.Bad => 1,
            _ => 0,
        };

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_maxScore == 0)
        {
            text.text = "000000";
            return;
        }
        if (_currentScore >= _maxScore)
        {
            text.text = "1000000";
            return;
        }
        int display = Mathf.CeilToInt((float)_currentScore / _maxScore * 1000000f);
        text.text = display.ToString("D6");
    }
}