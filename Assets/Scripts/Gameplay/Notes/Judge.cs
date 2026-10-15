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

    [Header("Managers")]
    [SerializeField] private TapManager tapManager;
    [SerializeField] private HoldManager holdManager;
    [SerializeField] private GroundManager groundManager;

    public event System.Action<Judgement> OnJudged;

    private int _currentScore;
    private int _maxScore;

    private void Awake()
    {
        Instance = this;
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
            tapCount += tapManager.ActiveTaps[i].Count;
            holdCount += holdManager.ActiveHolds[i].Count;
        }
        int n = tapCount + groundManager.ActiveGrounds.Count + holdCount * 2;
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

    public void JudgePress(int trackIndex)
    {
        Tap earliestTap = null;
        float earliestTapMs = float.MaxValue;

        foreach (var tap in tapManager.ActiveTaps[trackIndex])
        {
            if (tap.IsJudged)
                continue;
            float diff = GameTime.Instance.ElapsedMs - tap.Ms;
            if (Mathf.Abs(diff) <= badWindowMs && tap.Ms < earliestTapMs)
            {
                earliestTapMs = tap.Ms;
                earliestTap = tap;
            }
        }

        Hold earliestHold = null;
        float earliestHoldMs = float.MaxValue;

        foreach (var hold in holdManager.ActiveHolds[trackIndex])
        {
            if (hold.HeadJudged)
                continue;
            float diff = GameTime.Instance.ElapsedMs - hold.Ms;
            if (Mathf.Abs(diff) <= badWindowMs && hold.Ms < earliestHoldMs)
            {
                earliestHoldMs = hold.Ms;
                earliestHold = hold;
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
        foreach (var hold in holdManager.ActiveHolds[trackIndex])
        {
            if (hold.HeadJudged && !hold.TailJudged && !hold.HeadWasMiss)
            {
                float diff = GameTime.Instance.ElapsedMs - hold.EndMs;
                var judgement = diff >= -tailEarlyWindowMs ? Judgement.Perfect : Judgement.Bad;
                HandleJudgement(judgement);
                hold.MarkTailJudged();
                return;
            }
        }
    }

    private void JudgeTap(Tap tap)
    {
        float diff = GameTime.Instance.ElapsedMs - tap.Ms;
        var judgement = GetJudgement(Mathf.Abs(diff));
        HandleJudgement(judgement);
        tap.MarkJudged();
    }

    private void JudgeHoldHead(Hold hold)
    {
        float diff = GameTime.Instance.ElapsedMs - hold.Ms;
        var judgement = GetJudgement(Mathf.Abs(diff));
        HandleJudgement(judgement);
        hold.MarkHeadJudged(judgement == Judgement.Miss);

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
        var grounds = groundManager.ActiveGrounds;
        for (int j = grounds.Count - 1; j >= 0; j--)
        {
            var ground = grounds[j];
            if (ground.IsJudged)
                continue;

            if (GameTime.Instance.ElapsedMs >= ground.Ms + groundWindowMs)
            {
                float judgeTime = ground.Ms + groundWindowMs;
                var judgement = Judgement.Miss;
                if (Player.Instance.WasJumpingAt(judgeTime)) judgement = Judgement.Perfect;
                HandleJudgement(judgement);
                ground.MarkJudged();
            }
        }
    }

    private void CheckAutoMiss()
    {
        for (int i = 0; i < 4; i++)
        {
            var taps = tapManager.ActiveTaps[i];
            for (int j = taps.Count - 1; j >= 0; j--)
            {
                if (taps[j].IsJudged)
                    continue;
                if (GameTime.Instance.ElapsedMs > taps[j].Ms + badWindowMs)
                {
                    HandleJudgement(Judgement.Miss);
                    taps[j].MarkJudged();
                }
            }

            var holds = holdManager.ActiveHolds[i];
            for (int j = holds.Count - 1; j >= 0; j--)
            {
                var hold = holds[j];
                if (!hold.HeadJudged && GameTime.Instance.ElapsedMs > hold.Ms + badWindowMs)
                {
                    HandleJudgement(Judgement.Miss);
                    hold.MarkHeadJudged(true);
                    HandleJudgement(Judgement.Miss);
                    continue;
                }

                if (hold.HeadJudged && !hold.TailJudged && !hold.HeadWasMiss
                    && GameTime.Instance.ElapsedMs > hold.EndMs)
                {
                    HandleJudgement(Judgement.Perfect);
                    hold.MarkTailJudged();
                }
            }
        }
    }
}