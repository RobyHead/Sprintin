using System.Collections.Generic;
using UnityEngine;

public class Judge : MonoBehaviour
{
    public static Judge Instance { get; private set; }

    [Header("Tap / Hold Head")]
    [SerializeField] private float perfectWindow = 50f;
    [SerializeField] private float greatWindow = 100f;
    [SerializeField] private float badWindow = 150f;

    [Header("Hold Tail")]
    [SerializeField] private float tailEarlyWindow = 100f;

    [Header("Ground")]
    [SerializeField] private float groundWindow = 50f;

    [Header("Info")]
    [SerializeField] private ScoreInfo scoreInfo;
    [SerializeField] private JudgementInfo judgementInfo;
    [SerializeField] private ComboInfo comboInfo;

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

    public void InitializeScore()
    {
        if (scoreInfo == null)
            return;

        int tapCount = 0;
        int holdCount = 0;
        for (int i = 0; i < 4; i++)
        {
            tapCount += _taps[i].Count;
            holdCount += _holds[i].Count;
        }
        int groundCount = _grounds.Count;

        scoreInfo.CalculateMaxScore(tapCount, groundCount, holdCount);
    }

    private void Update()
    {
        if (!GameTime.HasStarted)
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
            float diff = GameTime.ElapsedMs - tap.Ms;
            if (Mathf.Abs(diff) <= badWindow && tap.Ms < earliestTapMs)
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
                float diff = GameTime.ElapsedMs - hold.Ms;
                if (Mathf.Abs(diff) <= badWindow && hold.Ms < earliestHoldMs)
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
                float diff = GameTime.ElapsedMs - hold.EndMs;
                var judgement = diff >= -tailEarlyWindow ? Judgement.Perfect : Judgement.Bad;
                HandleJudgement(judgement);
                hold.JudgeTail(judgement, diff);
                return;
            }
        }
    }

    private void JudgeTap(Tap tap)
    {
        float diff = GameTime.ElapsedMs - tap.Ms;
        var judgement = GetJudgement(Mathf.Abs(diff));
        HandleJudgement(judgement);
        tap.OnJudged(judgement, diff);
    }

    private void JudgeHoldHead(Hold hold)
    {
        float diff = GameTime.ElapsedMs - hold.Ms;
        var judgement = GetJudgement(Mathf.Abs(diff));
        HandleJudgement(judgement);
        hold.JudgeHead(judgement, diff);

        if (judgement == Judgement.Miss)
            HandleJudgement(Judgement.Miss);
    }

    private void HandleJudgement(Judgement judgement)
    {
        if (judgementInfo != null)
            judgementInfo.Show(judgement);
        if (comboInfo != null)
            comboInfo.UpdateCombo(judgement);
        if (scoreInfo != null)
            scoreInfo.AddScore(judgement);
    }

    private Judgement GetJudgement(float absDiff)
    {
        if (absDiff <= perfectWindow) return Judgement.Perfect;
        if (absDiff <= greatWindow) return Judgement.Great;
        if (absDiff <= badWindow) return Judgement.Bad;
        return Judgement.Miss;
    }

    private void CheckGround()
    {
        for (int j = _grounds.Count - 1; j >= 0; j--)
        {
            var ground = _grounds[j];
            if (ground.IsJudged)
                continue;

            if (GameTime.ElapsedMs >= ground.Ms + groundWindow)
            {
                float judgeTime = ground.Ms + groundWindow;
                var judgement = Judgement.Miss;
                if (Player.WasJumpingAt(judgeTime)) judgement = Judgement.Perfect;
                HandleJudgement(judgement);
                ground.OnJudged(judgement, GameTime.ElapsedMs - ground.Ms);
            }
        }
    }

    private void CheckAutoMiss()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = _taps[i].Count - 1; j >= 0; j--)
            {
                if (GameTime.ElapsedMs > _taps[i][j].Ms + badWindow)
                {
                    HandleJudgement(Judgement.Miss);
                    _taps[i][j].OnJudged(Judgement.Miss, GameTime.ElapsedMs - _taps[i][j].Ms);
                }
            }

            for (int j = _holds[i].Count - 1; j >= 0; j--)
            {
                if (!_holds[i][j].HeadJudged && GameTime.ElapsedMs > _holds[i][j].Ms + badWindow)
                {
                    HandleJudgement(Judgement.Miss);
                    _holds[i][j].JudgeHead(Judgement.Miss, GameTime.ElapsedMs - _holds[i][j].Ms);
                    HandleJudgement(Judgement.Miss);
                    continue;
                }

                if (_holds[i][j].HeadJudged && !_holds[i][j].TailJudged && !_holds[i][j].HeadWasMiss
                    && GameTime.ElapsedMs > _holds[i][j].EndMs)
                {
                    HandleJudgement(Judgement.Perfect);
                    _holds[i][j].JudgeTail(Judgement.Perfect, GameTime.ElapsedMs - _holds[i][j].EndMs);
                }
            }
        }
    }
}