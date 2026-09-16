using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class ChartManager : MonoBehaviour
{
    public static bool IsReady { get; private set; }
    public static string SongFolder { get; private set; }
    public static string SongName { get; private set; }
    public static float CurrentJumpBpm { get; private set; } = 120f;
    public static int LastNoteMs { get; private set; }
    public static bool AudioLoaded { get; private set; }
    public static bool CoverLoaded { get; set; } = true;

    [Header("References")]
    [SerializeField] private TapManager tapManager;
    [SerializeField] private HoldManager holdManager;
    [SerializeField] private GroundManager groundManager;
    [SerializeField] private BarManager barManager;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private ResultPanelManager resultPanel;
    [SerializeField] private TextEffect textEffect;

    public static event System.Action OnGameEnded;

    [Header("Fade")]
    [SerializeField] private float quickFadeOutMs = 300f;

    private List<TapData> _taps = new List<TapData>();
    private List<HoldData> _holds = new List<HoldData>();
    private List<GroundData> _grounds = new List<GroundData>();

    private AudioSource _audioSource;
    private AudioClip _songClip;
    private bool _playbackScheduled;
    private bool _resultShown;
    private int _chartOffset;

    private List<BpmData> _jumpBpms;
    private List<BpmData> _barBpms;
    private int _nextJumpBpmIndex;

    private string _packId;
    private string _songId;
    private int _difficultyId;

    private void Awake()
    {
        ResetStatics();

        _packId = SceneTransitionManager.Instance.PackId;
        _songId = SceneTransitionManager.Instance.SongId;
        _difficultyId = SceneTransitionManager.Instance.DifficultyId;
        SongFolder = SceneTransitionManager.Instance.SongFolder;

        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        SceneTransitionManager.Instance.OnOutroStarted += HandleOutroStarted;
    }

    private void OnDestroy()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.OnOutroStarted -= HandleOutroStarted;
        OnGameEnded = null;
    }

    private static void ResetStatics()
    {
        IsReady = false;
        CurrentJumpBpm = 120f;
        LastNoteMs = 0;
        AudioLoaded = false;
        CoverLoaded = false;
        GameTime.Reset();
        Player.ResetStatics();
    }

    private void Start()
    {
        LoadChart();
        PreSpawnAllNotes();
        StartCoroutine(LoadAudio());
        Judge.Instance.InitializeScore();
        IsReady = true;
    }

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        if (!_playbackScheduled)
            SchedulePlayback();

        UpdateVolumeFade();
        UpdateJumpBpm();

        if (!_resultShown && GameTime.ElapsedMs >= LastNoteMs)
        {
            _resultShown = true;
            StartCoroutine(ResultSequence());
        }
    }

    private void UpdateJumpBpm()
    {
        while (_nextJumpBpmIndex < _jumpBpms.Count
               && GameTime.ElapsedMs >= _jumpBpms[_nextJumpBpmIndex].ms)
        {
            CurrentJumpBpm = _jumpBpms[_nextJumpBpmIndex].bpm;
            _nextJumpBpmIndex++;
        }
    }

    private void SchedulePlayback()
    {
        if (_songClip == null)
            return;

        _audioSource.clip = _songClip;

        float totalOffset = _chartOffset + gameConfig.Offset;
        float musicStartOffset = Mathf.Min(totalOffset, gameConfig.MaxSkipMs);
        float skipMs = Mathf.Max(0f, totalOffset - gameConfig.MaxSkipMs);

        double timeToMusicStart = (-musicStartOffset - GameTime.ElapsedMs) / 1000.0;
        double scheduledTime = AudioSettings.dspTime + timeToMusicStart;
        if (scheduledTime < 0)
            scheduledTime = 0;

        _audioSource.time = skipMs / 1000f;
        _audioSource.PlayScheduled(scheduledTime);
        _playbackScheduled = true;
    }

    private void UpdateVolumeFade()
    {
        if (!_playbackScheduled)
            return;

        float master = gameConfig != null ? gameConfig.MusicVolume / 100f : 1f;
        float elapsedMs = GameTime.ElapsedMs;
        if (elapsedMs >= -gameConfig.MaxSkipMs && elapsedMs <= -gameConfig.FadeEndMs)
            _audioSource.volume = master * (elapsedMs + gameConfig.MaxSkipMs) / (gameConfig.MaxSkipMs - gameConfig.FadeEndMs);
        else if (elapsedMs > -gameConfig.FadeEndMs)
            _audioSource.volume = master;
        else
            _audioSource.volume = 0f;
    }

    private IEnumerator LoadAudio()
    {
        var srcPath = Path.Combine(Application.streamingAssetsPath, "Songs", SongFolder, "track.mp3");
        var uri = new System.Uri(srcPath).AbsoluteUri;

        using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load audio: {request.error}");
        }
        else
        {
            _songClip = DownloadHandlerAudioClip.GetContent(request);
            if (_songClip == null)
                Debug.LogError("DownloadHandlerAudioClip.GetContent returned null");
        }

        AudioLoaded = true;
        TryRequestIntro();
    }

    public static void TryRequestIntro()
    {
        if (AudioLoaded && CoverLoaded && SceneTransitionManager.Instance.IsTransitioning)
            SceneTransitionManager.Instance.RequestIntro();
    }

    private void LoadChart()
    {
        var chartFileName = $"{_difficultyId}.spr";
        var path = Path.Combine(Application.streamingAssetsPath, "Songs", SongFolder, chartFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"Chart not found: {path}");
            return;
        }

        var text = File.ReadAllText(path);
        var chart = ChartParser.Parse(text);

        SongName = chart.name;
        _chartOffset = chart.offset;
        SpeedTimeline.Instance.SetSpeeds(chart.speeds);
        CurrentJumpBpm = chart.jumpBpm;
        _jumpBpms = new List<BpmData>(chart.jumpBpms);
        _jumpBpms.Sort((a, b) => a.ms.CompareTo(b.ms));
        _barBpms = new List<BpmData>(chart.barBpms);
        _barBpms.Sort((a, b) => a.ms.CompareTo(b.ms));
        _nextJumpBpmIndex = 0;

        _taps = new List<TapData>(chart.taps);
        _holds = new List<HoldData>(chart.holds);
        _grounds = new List<GroundData>(chart.grounds);

        _taps.Sort((a, b) => a.ms.CompareTo(b.ms));
        _holds.Sort((a, b) => a.ms.CompareTo(b.ms));
        _grounds.Sort((a, b) => a.ms.CompareTo(b.ms));

        LastNoteMs = 0;
        foreach (var t in _taps)
            if (t.ms > LastNoteMs) LastNoteMs = t.ms;
        foreach (var h in _holds)
            if (h.endMs > LastNoteMs) LastNoteMs = h.endMs;
        foreach (var g in _grounds)
            if (g.ms > LastNoteMs) LastNoteMs = g.ms;

        if (textEffect != null)
            textEffect.Initialize(chart.textEffects);
    }

    private void PreSpawnAllNotes()
    {
        foreach (var t in _taps)
            tapManager.Spawn(t.key, t.ms);

        foreach (var h in _holds)
            holdManager.Spawn(h.key, h.ms, h.endMs);

        foreach (var g in _grounds)
            groundManager.Spawn(g.ms);

        SpawnBars();
    }

    private void SpawnBars()
    {
        if (barManager == null || _barBpms == null || _barBpms.Count == 0)
            return;

        int lastNoteMs = 0;
        foreach (var t in _taps)
            if (t.ms > lastNoteMs) lastNoteMs = t.ms;
        foreach (var h in _holds)
            if (h.endMs > lastNoteMs) lastNoteMs = h.endMs;
        foreach (var g in _grounds)
            if (g.ms > lastNoteMs) lastNoteMs = g.ms;

        var barTimes = new List<int>();

        for (int i = 0; i < _barBpms.Count; i++)
        {
            var current = _barBpms[i];
            int nextMs = (i + 1 < _barBpms.Count) ? _barBpms[i + 1].ms : lastNoteMs + 5000;
            float interval = 60000f / current.bpm * current.beatsPerBar;

            if (i == 0)
            {
                float t = current.ms - interval;
                while (t >= -3000f)
                {
                    barTimes.Add(Mathf.RoundToInt(t));
                    t -= interval;
                }
            }

            float barT = current.ms;
            while (barT < nextMs)
            {
                barTimes.Add(Mathf.RoundToInt(barT));
                barT += interval;
            }
        }

        barTimes.Sort();

        int lastSpawned = int.MinValue;
        foreach (var t in barTimes)
        {
            if (t != lastSpawned)
                barManager.Spawn(t);
            lastSpawned = t;
        }
    }

    public void UnloadChart()
    {
        tapManager.Clear();
        holdManager.Clear();
        groundManager.Clear();
        barManager.Clear();
        IsReady = false;
    }

    private void HandleOutroStarted()
    {
        if (!_resultShown)
            StartCoroutine(FadeOutAudioRoutine(quickFadeOutMs / 1000f));
    }

    private IEnumerator ResultSequence()
    {
        var config = gameConfig;
        float startDelay = config.FadeOutStartDelayMs / 1000f;
        float fadeDur = config.FadeOutDurationMs / 1000f;
        float settleDelay = config.SettlementDelayMs / 1000f;

        yield return new WaitForSeconds(startDelay);

        yield return StartCoroutine(FadeOutAudioRoutine(fadeDur));

        yield return new WaitForSeconds(settleDelay);

        OnGameEnded?.Invoke();

        yield return SceneTransitionManager.Instance.PlayOutroAndWait();

        if (resultPanel != null)
            resultPanel.Show();

        yield return SceneTransitionManager.Instance.PlayIntroAndWait();

        if (resultPanel != null)
            resultPanel.EnableInput();
    }

    private IEnumerator FadeOutAudioRoutine(float durationSeconds)
    {
        if (_audioSource == null || !_audioSource.isPlaying)
            yield break;

        float startVolume = _audioSource.volume;
        float elapsed = 0f;
        while (elapsed < durationSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / durationSeconds);
            yield return null;
        }
        _audioSource.Stop();
        _audioSource.volume = 0f;
    }
}