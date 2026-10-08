using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalibratePanelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CalibrateNote calibrateNote;
    [SerializeField] private TMP_Text offsetText;

    [Header("Offset")]
    [SerializeField] private string offsetKey = "offset";
    [SerializeField] private float offsetStep = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cueClip;
    [SerializeField] private float fadeOutMs = 300f;

    [Header("Timing")]
    [SerializeField] private float startDelayMs = 1500f;
    [SerializeField] private float playToHitMs = 2000f;
    [SerializeField] private float cooldownMs = 2000f;
    [SerializeField] private float scheduleAheadMs = 100f;

    public event System.Action OnRequestBack;

    private enum State { Idle, WaitingToStart, Running }
    private State _state;
    private float _stateTimer;

    private float _cachedOffsetMs;
    private readonly Queue<float> _offsetSamples = new Queue<float>();

    private double _nextSoundDspTime;
    private double _targetDspTime;
    private bool _hasHitThisRound;
    private bool _scheduled;
    private Coroutine _fadeRoutine;
    private bool _acceptInput;
    private bool _calibrateHitRequested;

    private void OnEnable()
    {
        _acceptInput = false;
        _calibrateHitRequested = false;
        LoadOffsetFromPrefs();
        UpdateOffsetDisplay();
        calibrateNote.Stop();
        calibrateNote.gameObject.SetActive(false);

        _offsetSamples.Clear();
        _offsetSamples.Enqueue(_cachedOffsetMs);

        _state = State.WaitingToStart;
        _stateTimer = 0f;

        if (MenuInputManager.Instance != null)
        {
            MenuInputManager.Instance.OnBack += HandleBack;
            MenuInputManager.Instance.OnLeft += HandleDecreaseOffset;
            MenuInputManager.Instance.OnRight += HandleIncreaseOffset;
            MenuInputManager.Instance.OnCalibrateHit += HandleCalibrateHit;
        }
    }

    private void OnDisable()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
        StopAudioImmediate();
        calibrateNote.Stop();
        SaveOffsetToPrefs();

        if (MenuInputManager.Instance != null)
        {
            MenuInputManager.Instance.OnBack -= HandleBack;
            MenuInputManager.Instance.OnLeft -= HandleDecreaseOffset;
            MenuInputManager.Instance.OnRight -= HandleIncreaseOffset;
            MenuInputManager.Instance.OnCalibrateHit -= HandleCalibrateHit;
        }
    }

    public void SetInteractable(bool interactable)
    {
        _acceptInput = interactable;
        if (MenuInputManager.Instance != null)
            MenuInputManager.Instance.SetInteractable(interactable);
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || !_acceptInput)
            return;

        HandleState();
    }

    private void HandleBack()
    {
        if (!_acceptInput)
            return;

        SaveOffsetToPrefs();
        calibrateNote.Stop();
        _state = State.Idle;
        FadeOutAndExit();
    }

    private void HandleDecreaseOffset()
    {
        if (!_acceptInput)
            return;
        AdjustOffset(-offsetStep);
    }

    private void HandleIncreaseOffset()
    {
        if (!_acceptInput)
            return;
        AdjustOffset(offsetStep);
    }

    private void HandleCalibrateHit()
    {
        if (!_acceptInput)
            return;
        _calibrateHitRequested = true;
    }

    private void AdjustOffset(float delta)
    {
        _cachedOffsetMs += delta;
        _cachedOffsetMs = Mathf.Round(_cachedOffsetMs);
        _offsetSamples.Clear();
        _offsetSamples.Enqueue(_cachedOffsetMs);
        UpdateOffsetDisplay();
        UpdateNoteTarget();
    }

    private void HandleState()
    {
        switch (_state)
        {
            case State.Idle:
                break;

            case State.WaitingToStart:
                _stateTimer += Time.deltaTime * 1000f;
                if (_stateTimer >= startDelayMs)
                    StartMasterTimeline();
                break;

            case State.Running:
                HandleRunning();
                break;
        }
    }

    private void StartMasterTimeline()
    {
        calibrateNote.gameObject.SetActive(true);
        _nextSoundDspTime = AudioSettings.dspTime + scheduleAheadMs / 1000.0;
        _scheduled = false;
        _state = State.Running;
    }

    private void HandleRunning()
    {
        if (!_scheduled && AudioSettings.dspTime >= _nextSoundDspTime - scheduleAheadMs / 1000.0)
        {
            ScheduleSound();
            _scheduled = true;
        }

        if (AudioSettings.dspTime >= _nextSoundDspTime)
        {
            _targetDspTime = _nextSoundDspTime + playToHitMs / 1000.0;
            _hasHitThisRound = false;

            UpdateNoteTarget();
            _nextSoundDspTime += cooldownMs / 1000.0;
            _scheduled = false;
        }

        if (_hasHitThisRound || !_calibrateHitRequested)
            return;

        _calibrateHitRequested = false;
        double pressDspTime = AudioSettings.dspTime;
        float attemptOffset = (float)((pressDspTime - _targetDspTime) * 1000.0);

        AddOffsetSample(attemptOffset);
        UpdateOffsetDisplay();
        UpdateNoteTarget();
        _hasHitThisRound = true;
    }

    private void ScheduleSound()
    {
        if (audioSource != null && cueClip != null)
        {
            audioSource.Stop();
            audioSource.clip = cueClip;
            audioSource.PlayScheduled(_nextSoundDspTime);
        }
    }

    private void UpdateNoteTarget()
    {
        double displayHitTime = _targetDspTime + _cachedOffsetMs / 1000.0;
        calibrateNote.StartMoving(displayHitTime);
    }

    private void AddOffsetSample(float sample)
    {
        _offsetSamples.Enqueue(sample);
        while (_offsetSamples.Count > 5)
            _offsetSamples.Dequeue();

        _cachedOffsetMs = Mathf.Round(CalculateTrimmedMean());
    }

    private float CalculateTrimmedMean()
    {
        int count = _offsetSamples.Count;
        if (count <= 1)
        {
            float sum = 0f;
            foreach (var s in _offsetSamples)
                sum += s;
            return sum / count;
        }

        var values = _offsetSamples.ToArray();
        float sumAll = 0f;
        for (int i = 0; i < values.Length; i++)
            sumAll += values[i];
        float mean = sumAll / values.Length;

        int outlierIndex = 0;
        float maxDeviation = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            float dev = Mathf.Abs(values[i] - mean);
            if (dev > maxDeviation)
            {
                maxDeviation = dev;
                outlierIndex = i;
            }
        }

        float sumTrim = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            if (i == outlierIndex) continue;
            sumTrim += values[i];
        }

        return sumTrim / (values.Length - 1);
    }

    private void LoadOffsetFromPrefs()
    {
        string saved = PlayerPrefs.GetString(offsetKey, null);
        if (!string.IsNullOrEmpty(saved) && float.TryParse(saved, out float val))
        {
            _cachedOffsetMs = Mathf.Round(val);
        }
        else
        {
            _cachedOffsetMs = 0f;
        }
    }

    private void SaveOffsetToPrefs()
    {
        PlayerPrefs.SetString(offsetKey, _cachedOffsetMs.ToString("F0"));
        PlayerPrefs.Save();
    }

    private void StopAudioImmediate()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    private void FadeOutAndExit()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            _fadeRoutine = StartCoroutine(FadeOutRoutine());
        }
        else
        {
            StopAudioImmediate();
            OnRequestBack?.Invoke();
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        float duration = fadeOutMs / 1000f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        audioSource.clip = null;

        _fadeRoutine = null;
        OnRequestBack?.Invoke();
    }

    private void UpdateOffsetDisplay()
    {
        if (offsetText != null)
            offsetText.text = _cachedOffsetMs.ToString("F0");
    }
}