using System.Collections.Generic;
using UnityEngine;

public class Judge : MonoBehaviour
{
    public static Judge Instance { get; private set; }

    public int PerfectCount { get; private set; }
    public int GreatCount { get; private set; }
    public int BadCount { get; private set; }
    public int MissCount { get; private set; }

    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }
    public int FinalScore { get; private set; }

    [Header("Tap / Hold Head")]
    [SerializeField] private float perfectWindowMs = 50f;
    [SerializeField] private float greatWindowMs = 100f;
    [SerializeField] private float badWindowMs = 150f;

    [Header("Hold Tail")]
    [SerializeField] private float tailEarlyWindowMs = 100f;

    [Header("Ground")]
    [SerializeField] private float groundWindowMs = 50f;

    public event System.Action<Judgement> OnJudged;

    private int _currentScore;
    private int _maxScore;

    private readonly List<Tap>[] _taps = new List<Tap>[4];
    private readonly List<Hold>[] _holds = new List<Hold>[4];
    private readonly List<Ground> _grounds = new List<Ground>();

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < 4; i++)
        {
            _taps[i] = new List<Tap>();
            _holds[i] = new List<Hold>();
        }
    }

    private void Start()
    {
        ChartManager.Instance.OnGameEnded += HandleGameEnded;
    }

    private void OnDestroy()
    {
        if (ChartManager.Instance != null)
            ChartManager.Instance.OnGameEnded -= HandleGameEnded;
    }

    private void HandleGameEnded()
    {
        enabled = false;
    }

    public void InitializeScore()
    {
        PerfectCount = 0;
        GreatCount = 0;
        BadCount = 0;
        MissCount = 0;
        Combo = 0;
        MaxCombo = 0;
        FinalScore = 0;

        int tapCount = 0;
        int holdCount = 0;
        for (int i = 0; i < 4; i++)
        {
            tapCount += _taps[i].Count;
            holdCount += _holds[i].Count;
        }
        int n = tapCount + _grounds.Count + holdCount * 2;
        _maxScore = n * 3;
        _currentScore = 0;
    }

    private void Update()
    {
        if (!GameTime.Instance.HasStarted)
            return;

        CheckGround();
        CheckAutoMiss();
    }

    public void RegisterTap(Tap tap)
    {
        _taps[tap.Key - 1].Add(tap);
    }

    public void UnregisterTap(Tap tap)
    {
        _taps[tap.Key - 1].Remove(tap);
    }

    public void RegisterHold(Hold hold)
    {
        _holds[hold.Key - 1].Add(hold);
    }

    public void UnregisterHold(Hold hold)
    {
        _holds[hold.Key - 1].Remove(hold);
    }

    public void RegisterGround(Ground ground)
    {
        _grounds.Add(ground);
    }

    public void UnregisterGround(Ground ground)
    {
        _grounds.Remove(ground);
    }

    public void JudgePress(int trackIndex)
    {
        Tap earliestTap = null;
        float earliestTapMs = float.MaxValue;

        foreach (var tap in _taps[trackIndex])
        {
            float diff = GameTime.Instance.ElapsedMs - tap.Ms;
            if (Mathf.Abs(diff) <= badWindowMs && tap.Ms < earliestTapMs)
            {
                earliestTapMs = tap.Ms;
                earliestTap = tap;
            }
        }

        Hold earliestHold = null;
        float earliestHoldMs = float.MaxValue;

        foreach (var hold in _holds[trackIndex])
        {
            if (!hold.HeadJudged)
            {
                float diff = GameTime.Instance.ElapsedMs - hold.Ms;
                if (Mathf.Abs(diff) <= badWindowMs && hold.Ms < earliestHoldMs)
                {
                    earliestHoldMs = hold.Ms;
                    earliestHold = hold;
                }
            }
        }

        if (earliestTap != null && earliestHold != null)
        {
            if (earliestTapMs <= earliestHoldMs)
                JudgeTap(earliestTap);
            else
                JudgeHoldHead(earliestHold);
        }
        else if (earliestTap != null)
        {
            JudgeTap(earliestTap);
        }
        else if (earliestHold != null)
        {
            JudgeHoldHead(earliestHold);
        }
    }

    public void JudgeRelease(int trackIndex)
    {
        foreach (var hold in _holds[trackIndex])
        {
            if (hold.HeadJudged && !hold.TailJudged && !hold.HeadWasMiss)
            {
                float diff = GameTime.Instance.ElapsedMs - hold.EndMs;
                var judgement = diff >= -tailEarlyWindowMs ? Judgement.Perfect : Judgement.Bad;
                HandleJudgement(judgement);
                hold.JudgeTail(judgement, diff);
                return;
            }
        }
    }

    private void JudgeTap(Tap tap)
    {
        float diff = GameTime.Instance.ElapsedMs - tap.Ms;
        var judgement = GetJudgement(Mathf.Abs(diff));
        HandleJudgement(judgement);
        tap.OnJudged(judgement, diff);
    }

    private void JudgeHoldHead(Hold hold)
    {
        float diff = GameTime.Instance.ElapsedMs - hold.Ms;
        var judgement = GetJudgement(Mathf.Abs(diff));
        HandleJudgement(judgement);
        hold.JudgeHead(judgement, diff);

        if (judgement == Judgement.Miss)
            HandleJudgement(Judgement.Miss);
    }

    private void HandleJudgement(Judgement judgement)
    {
        switch (judgement)
        {
            case Judgement.Perfect: PerfectCount++; break;
            case Judgement.Great:   GreatCount++;   break;
            case Judgement.Bad:     BadCount++;     break;
            case Judgement.Miss:    MissCount++;    break;
        }

        if (judgement == Judgement.Bad || judgement == Judgement.Miss)
            Combo = 0;
        else
            Combo++;
        if (Combo > MaxCombo)
            MaxCombo = Combo;

        _currentScore += judgement switch
        {
            Judgement.Perfect => 3,
            Judgement.Great => 2,
            Judgement.Bad => 1,
            _ => 0,
        };
        FinalScore = _maxScore == 0 ? 0
            : Mathf.CeilToInt((float)_currentScore / _maxScore * 1000000f);

        OnJudged?.Invoke(judgement);
    }

    private Judgement GetJudgement(float absDiff)
    {
        if (absDiff <= perfectWindowMs) return Judgement.Perfect;
        if (absDiff <= greatWindowMs) return Judgement.Great;
        if (absDiff <= badWindowMs) return Judgement.Bad;
        return Judgement.Miss;
    }

    private void CheckGround()
    {
        for (int j = _grounds.Count - 1; j >= 0; j--)
        {
            var ground = _grounds[j];
            if (ground.IsJudged)
                continue;

            if (GameTime.Instance.ElapsedMs >= ground.Ms + groundWindowMs)
            {
                float judgeTime = ground.Ms + groundWindowMs;
                var judgement = Judgement.Miss;
                if (Player.Instance.WasJumpingAt(judgeTime)) judgement = Judgement.Perfect;
                HandleJudgement(judgement);
                ground.OnJudged(judgement, GameTime.Instance.ElapsedMs - ground.Ms);
            }
        }
    }

    private void CheckAutoMiss()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = _taps[i].Count - 1; j >= 0; j--)
            {
                if (GameTime.Instance.ElapsedMs > _taps[i][j].Ms + badWindowMs)
                {
                    HandleJudgement(Judgement.Miss);
                    _taps[i][j].OnJudged(Judgement.Miss, GameTime.Instance.ElapsedMs - _taps[i][j].Ms);
                }
            }

            for (int j = _holds[i].Count - 1; j >= 0; j--)
            {
                if (!_holds[i][j].HeadJudged && GameTime.Instance.ElapsedMs > _holds[i][j].Ms + badWindowMs)
                {
                    HandleJudgement(Judgement.Miss);
                    _holds[i][j].JudgeHead(Judgement.Miss, GameTime.Instance.ElapsedMs - _holds[i][j].Ms);
                    HandleJudgement(Judgement.Miss);
                    continue;
                }

                if (_holds[i][j].HeadJudged && !_holds[i][j].TailJudged && !_holds[i][j].HeadWasMiss
                    && GameTime.Instance.ElapsedMs > _holds[i][j].EndMs)
                {
                    HandleJudgement(Judgement.Perfect);
                    _holds[i][j].JudgeTail(Judgement.Perfect, GameTime.Instance.ElapsedMs - _holds[i][j].EndMs);
                }
            }
        }
    }
}