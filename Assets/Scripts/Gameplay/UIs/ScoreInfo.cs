using TMPro;
using UnityEngine;

public class ScoreInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private int _currentScore;
    private int _maxScore;

    public int FinalScore { get; private set; }

    private void Awake()
    {
        text.text = "";
    }

    public void CalculateMaxScore(int tapCount, int groundCount, int holdCount)
    {
        int n = tapCount + groundCount + holdCount * 2;
        _maxScore = n * 3;
        _currentScore = 0;
        FinalScore = 0;
        UpdateDisplay();
    }

    public void AddScore(Judgement judgement)
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
        int display = _maxScore == 0 ? 0 : Mathf.CeilToInt((float)_currentScore / _maxScore * 1000000f);
        text.text = $"{display:D6}";
        FinalScore = display;
    }
}