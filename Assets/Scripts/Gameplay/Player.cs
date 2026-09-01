using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static bool IsJumping { get; private set; }
    public static bool CanOperate { get; private set; }

    private static float _jumpStartTime = float.MinValue;
    private static float _jumpEndTime = float.MinValue;

    public static void ResetStatics()
    {
        IsJumping = false;
        CanOperate = false;
        _jumpStartTime = float.MinValue;
        _jumpEndTime = float.MinValue;
    }

    public static bool WasJumpingAt(float elapsedMs)
    {
        if (_jumpStartTime == float.MinValue)
            return false;

        if (_jumpEndTime == float.MinValue)
            return elapsedMs >= _jumpStartTime;

        return elapsedMs >= _jumpStartTime && elapsedMs <= _jumpEndTime;
    }

    [Header("Jump")]
    [SerializeField] private float maxHeight = 0.8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float preLandWindow = 0.05f;

    private enum State { Idle, Rising, Holding, Falling }
    private State _state = State.Idle;
    private float _stateTimer;
    private float _groundY;
    private float _riseDuration;
    private float _holdDuration;
    private float _fallDuration;
    private float _peakHeight;

    private void Start()
    {
        _groundY = transform.position.y;
        CanOperate = true;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        switch (_state)
        {
            case State.Idle:
                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    StartJump();
                }
                break;

            case State.Rising:
                _stateTimer += Time.deltaTime;
                float riseT = Mathf.Clamp01(_stateTimer / _riseDuration);
                SetY(ParabolaRise(riseT));

                if (_stateTimer >= _riseDuration)
                {
                    _state = State.Holding;
                    _stateTimer = 0f;
                }
                break;

            case State.Holding:
                _stateTimer += Time.deltaTime;
                SetY(_groundY + _peakHeight);

                if (_stateTimer >= _holdDuration)
                {
                    _state = State.Falling;
                    _stateTimer = 0f;
                }
                break;

            case State.Falling:
                _stateTimer += Time.deltaTime;
                float fallT = Mathf.Clamp01(_stateTimer / _fallDuration);
                SetY(ParabolaFall(fallT));

                float remaining = _fallDuration - _stateTimer;
                if (remaining <= preLandWindow && !CanOperate)
                {
                    CanOperate = true;
                }

                if (keyboard.spaceKey.wasPressedThisFrame && CanOperate)
                {
                    StartJump();
                }

                if (_stateTimer >= _fallDuration)
                {
                    _state = State.Idle;
                    _stateTimer = 0f;
                    IsJumping = false;
                    CanOperate = true;
                    _jumpEndTime = GameTime.ElapsedMs;
                }
                break;
        }
    }

    private void StartJump()
    {
        float airTime = 60000f / ChartManager.CurrentJumpBpm / 1000f;
        float v0Max = Mathf.Sqrt(2f * acceleration * maxHeight);
        float fullRiseFall = 2f * v0Max / acceleration;

        if (airTime >= fullRiseFall)
        {
            _riseDuration = v0Max / acceleration;
            _fallDuration = _riseDuration;
            _holdDuration = airTime - _riseDuration - _fallDuration;
            _peakHeight = maxHeight;
        }
        else
        {
            _riseDuration = airTime / 2f;
            _fallDuration = airTime / 2f;
            _holdDuration = 0f;
            float v0 = acceleration * _riseDuration;
            _peakHeight = v0 * v0 / (2f * acceleration);
        }

        IsJumping = true;
        CanOperate = false;
        _state = State.Rising;
        _stateTimer = 0f;
        _jumpStartTime = GameTime.ElapsedMs;
        _jumpEndTime = float.MinValue;
    }

    private float ParabolaRise(float t)
    {
        return _groundY + _peakHeight * (2f * t - t * t);
    }

    private float ParabolaFall(float t)
    {
        return _groundY + _peakHeight * (1f - t * t);
    }

    private void SetY(float y)
    {
        var pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}