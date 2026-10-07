using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public bool IsJumping { get; private set; }
    public bool CanOperate { get; private set; }

    private float _jumpStartTime = float.MinValue;
    private float _jumpEndTime = float.MinValue;

    public bool WasJumpingAt(float elapsedMs)
    {
        if (_jumpStartTime == float.MinValue)
            return false;

        if (_jumpEndTime == float.MinValue)
            return elapsedMs >= _jumpStartTime;

        return elapsedMs >= _jumpStartTime && elapsedMs <= _jumpEndTime;
    }

    [Header("Jump")]
    [SerializeField] private float maxJumpHeight = 0.8f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float preLandWindowMs = 100f;

    private enum State { Idle, Rising, Holding, Conflicting, Falling }
    private State _state = State.Idle;

    private float _jumpElapsed;
    private float _fallStartTimeS;

    private float _groundY;
    private float _riseDuration;
    private float _fallDuration;

    private float _airTime;
    private float _cachedAirTime;
    private float _jumpStartTimeS;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (ChartManager.Instance != null)
        {
            ChartManager.Instance.OnGameEnded -= HandleGameEnded;
            ChartManager.Instance.OnJumpBpmChanged -= HandleJumpBpmChanged;
        }
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        _groundY = transform.position.y;
        CanOperate = true;
        ChartManager.Instance.OnGameEnded += HandleGameEnded;
        ChartManager.Instance.OnJumpBpmChanged += HandleJumpBpmChanged;
    }

    private void HandleGameEnded()
    {
        enabled = false;
    }

    private void HandleJumpBpmChanged(float newBpm)
    {
        float newAirTime = 60000f / newBpm / 1000f;

        if (_state == State.Falling)
        {
            _cachedAirTime = newAirTime;
            return;
        }

        _airTime = Mathf.Max(_fallDuration, newAirTime);

        if (_state == State.Conflicting)
        {
            float jumpElapsed = GameTime.Instance.ElapsedMs / 1000f - _jumpStartTimeS;
            if (jumpElapsed < _airTime - _fallDuration)
            {
                _state = State.Rising;
            }
            else
            {
                _cachedAirTime = newAirTime;
            }
            return;
        }

        _cachedAirTime = 0f;
        if (_state == State.Holding)
        {
            float jumpElapsed = GameTime.Instance.ElapsedMs / 1000f - _jumpStartTimeS;
            if (jumpElapsed >= _airTime - _fallDuration)
            {
                _state = State.Falling;
                _fallStartTimeS = GameTime.Instance.ElapsedMs / 1000f;
                _airTime = jumpElapsed + _fallDuration;
            }
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (!GameTime.Instance.HasStarted)
            return;

        if (_state != State.Idle)
        {
            _jumpElapsed = GameTime.Instance.ElapsedMs / 1000f - _jumpStartTimeS;
        }

        if (_jumpElapsed >= _airTime - _fallDuration)
        {
            if (_state == State.Rising)
            {
                _state = State.Conflicting;
                _fallStartTimeS = GameTime.Instance.ElapsedMs / 1000f;
                _airTime = _jumpElapsed + _fallDuration;
            }
            else if (_state == State.Holding)
            {
                _state = State.Falling;
                _fallStartTimeS = GameTime.Instance.ElapsedMs / 1000f;
                _airTime = _jumpElapsed + _fallDuration;
            }
        }

        switch (_state)
        {
            case State.Idle:
                SetY(_groundY);
                if (keyboard.spaceKey.wasPressedThisFrame)
                    StartJump();
                break;

            case State.Rising:
                SetY(ParabolaRise(Mathf.Clamp01(_jumpElapsed / _riseDuration)));
                if (_jumpElapsed >= _riseDuration)
                    _state = State.Holding;
                break;

            case State.Holding:
                SetY(_groundY + maxJumpHeight);
                break;

            case State.Conflicting:
                float riseY = ParabolaRise(Mathf.Clamp01(_jumpElapsed / _riseDuration));
                float fallY = ParabolaFall(Mathf.Clamp01(_jumpElapsed / _fallDuration));
                SetY(Mathf.Min(riseY, fallY));
                if (fallY <= riseY)
                    _state = State.Falling;
                break;

            case State.Falling:
                float fallTimer = GameTime.Instance.ElapsedMs / 1000f - _fallStartTimeS;
                SetY(ParabolaFall(Mathf.Clamp01(fallTimer / _fallDuration)));
                float remaining = _airTime - _jumpElapsed;
                if (remaining <= preLandWindowMs / 1000f && !CanOperate)
                    CanOperate = true;
                if (keyboard.spaceKey.wasPressedThisFrame && CanOperate)
                {
                    StartJump();
                    break;
                }
                if (remaining <= 0f)
                {
                    _state = State.Idle;
                    IsJumping = false;
                    CanOperate = true;
                    _jumpEndTime = GameTime.Instance.ElapsedMs;
                }
                break;
        }
    }

    private void StartJump()
    {
        float v0Max = Mathf.Sqrt(2f * acceleration * maxJumpHeight);
        _riseDuration = v0Max / acceleration;
        _fallDuration = _riseDuration;

        if (_cachedAirTime > 0)
        {
            _airTime = Mathf.Max(_fallDuration, _cachedAirTime);
            _cachedAirTime = 0f;
        }
        else
        {
            float bpmAir = 60000f / ChartManager.Instance.CurrentJumpBpm / 1000f;
            _airTime = Mathf.Max(_fallDuration, bpmAir);
        }

        _jumpElapsed = 0f;

        IsJumping = true;
        CanOperate = false;
        _state = State.Rising;
        _jumpStartTimeS = GameTime.Instance.ElapsedMs / 1000f;
        _jumpStartTime = GameTime.Instance.ElapsedMs;
        _jumpEndTime = float.MinValue;
    }

    private float ParabolaRise(float t)
    {
        return _groundY + maxJumpHeight * (2f * t - t * t);
    }

    private float ParabolaFall(float t)
    {
        return _groundY + maxJumpHeight * (1f - t * t);
    }

    private void SetY(float y)
    {
        var pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}