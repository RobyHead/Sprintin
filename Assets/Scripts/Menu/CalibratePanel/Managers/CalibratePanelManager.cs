using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private enum OffsetKey { None, Decrease, Increase }
    private OffsetKey _heldKey = OffsetKey.None;
    private float _holdStartTime;
    private float _lastRepeatTime;

    private void OnEnable()
    {
        _acceptInput = false;
        LoadOffsetFromPrefs();
        UpdateOffsetDisplay();
        calibrateNote.Stop();
        calibrateNote.gameObject.SetActive(false);

        _offsetSamples.Clear();
        _offsetSamples.Enqueue(_cachedOffsetMs);

        _state = State.WaitingToStart;
        _stateTimer = 0f;
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
    }

    public void SetInteractable(bool interactable)
    {
        _acceptInput = interactable;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || !_acceptInput)
            return;

        var kb = Keyboard.current;
        if (kb == null)
            return;

        HandleADInput(kb);
        HandleEscape(kb);
        HandleState(kb);
    }

    private void HandleEscape(Keyboard kb)
    {
        if (kb.escapeKey.wasPressedThisFrame)
        {
            SaveOffsetToPrefs();
            calibrateNote.Stop();
            _state = State.Idle;
            FadeOutAndExit();
        }
    }

    private void HandleADInput(Keyboard kb)
    {
        if (_heldKey != OffsetKey.None)
        {
            if (!IsOffsetKeyPressed(kb, _heldKey))
            {
                _heldKey = OffsetKey.None;
                return;
            }

            float holdDuration = Time.time - _holdStartTime;
            float interval = holdDuration < 0.5f ? 0.25f : 0.05f;
            if (Time.time - _lastRepeatTime >= interval)
            {
                _lastRepeatTime = Time.time;
                DoOffsetAction(_heldKey);
            }
            return;
        }

        OffsetKey pressed = OffsetKey.None;
        if (kb.aKey.wasPressedThisFrame) pressed = OffsetKey.Decrease;
        else if (kb.dKey.wasPressedThisFrame) pressed = OffsetKey.Increase;

        if (pressed == OffsetKey.None)
            return;

        _heldKey = pressed;
        _holdStartTime = Time.time;
        _lastRepeatTime = Time.time;
        DoOffsetAction(pressed);
    }

    private bool IsOffsetKeyPressed(Keyboard kb, OffsetKey key)
    {
        return key switch
        {
            OffsetKey.Decrease => kb.aKey.isPressed,
            OffsetKey.Increase => kb.dKey.isPressed,
            _ => false
        };
    }

    private void DoOffsetAction(OffsetKey key)
    {
        float delta = key == OffsetKey.Decrease ? -offsetStep : offsetStep;
        AdjustOffset(delta);
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

    private void HandleState(Keyboard kb)
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
                HandleRunning(kb);
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

    private void HandleRunning(Keyboard kb)
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

        if (_hasHitThisRound)
            return;

        if (!kb.spaceKey.wasPressedThisFrame)
            return;

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

        float sum = 0f;
        foreach (var s in _offsetSamples)
            sum += s;
        _cachedOffsetMs = Mathf.Round(sum / _offsetSamples.Count);
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