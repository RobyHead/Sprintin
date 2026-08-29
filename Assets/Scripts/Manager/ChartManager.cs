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

    [Header("References")]
    [SerializeField] private TapManager tapManager;
    [SerializeField] private HoldManager holdManager;
    [SerializeField] private GroundManager groundManager;
    [SerializeField] private BarManager barManager;

    [Header("Song")]
    [SerializeField] private string songFolder = "Dev/Sound Chimera";
    [SerializeField] private Difficulty difficulty = Difficulty.Easy;
    [SerializeField] private float delay = 0f;

    private List<TapData> _taps = new List<TapData>();
    private List<HoldData> _holds = new List<HoldData>();
    private List<GroundData> _grounds = new List<GroundData>();

    private AudioSource _audioSource;
    private AudioClip _songClip;
    private bool _playbackScheduled;

    private List<BpmData> _jumpBpms;
    private List<BpmData> _barBpms;
    private int _nextJumpBpmIndex;

    private void Awake()
    {
        IsReady = false;
        SongFolder = songFolder;

        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        LoadChart();
        StartCoroutine(LoadAudio());
    }

    private void Start()
    {
        PreSpawnAllNotes();
        ScoreInfo.CalculateMaxScore(_taps.Count, _grounds.Count, _holds.Count);
        IsReady = true;
    }

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        if (!_playbackScheduled)
            SchedulePlayback();

        UpdateJumpBpm();
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

        double timeToZero = -GameTime.ElapsedMs / 1000.0;
        double scheduledTime = AudioSettings.dspTime + timeToZero - delay / 1000.0;
        if (scheduledTime < 0)
            scheduledTime = 0;

        _audioSource.PlayScheduled(scheduledTime);
        _playbackScheduled = true;
    }

    private IEnumerator LoadAudio()
    {
        var srcPath = Path.Combine(Application.streamingAssetsPath, "Songs", songFolder, "track.mp3");
        var uri = new System.Uri(srcPath).AbsoluteUri;

        using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load audio: {request.error}");
            yield break;
        }

        _songClip = DownloadHandlerAudioClip.GetContent(request);
        if (_songClip == null)
        {
            Debug.LogError("DownloadHandlerAudioClip.GetContent returned null");
            yield break;
        }
    }

    private void LoadChart()
    {
        var chartFileName = $"{(int)difficulty}.spr";
        var path = Path.Combine(Application.streamingAssetsPath, "Songs", songFolder, chartFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"Chart not found: {path}");
            return;
        }

        var text = File.ReadAllText(path);
        var chart = ChartParser.Parse(text);

        SongName = chart.name;
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
}