using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MenuInputManager : MonoBehaviour
{
    public static MenuInputManager Instance { get; private set; }

    [Header("Hold Repeat")]
    [SerializeField] private float _holdRepeatInitialDelay = 0.5f;
    [SerializeField] private float _holdRepeatInitialRate = 0.25f;
    [SerializeField] private float _holdRepeatFastRate = 0.05f;

    public event Action OnUp;
    public event Action OnDown;
    public event Action OnLeft;
    public event Action OnRight;
    public event Action OnConfirm;
    public event Action OnBack;
    public event Action OnSettings;
    public event Action OnCalibrate;
    public event Action OnCalibrateHit;
    public event Action OnDifficultyPrev;
    public event Action OnDifficultyNext;

    private bool _interactable;

    private enum Dir { W, S, A, D }

    private readonly bool[] _held = new bool[4];
    private readonly float[] _holdStart = new float[4];
    private readonly float[] _lastRepeat = new float[4];
    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;
        if (!interactable)
        {
            for (int i = 0; i < 4; i++)
                _held[i] = false;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!_interactable)
            return;

        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.escapeKey.wasPressedThisFrame) OnBack?.Invoke();
        if (kb.tabKey.wasPressedThisFrame) OnSettings?.Invoke();
        if (kb.cKey.wasPressedThisFrame) OnCalibrate?.Invoke();
        if (kb.enterKey.wasPressedThisFrame) OnConfirm?.Invoke();
        if (kb.spaceKey.wasPressedThisFrame) OnCalibrateHit?.Invoke();
        if (kb.qKey.wasPressedThisFrame) OnDifficultyPrev?.Invoke();
        if (kb.eKey.wasPressedThisFrame) OnDifficultyNext?.Invoke();

        HandleHoldRepeat(kb.wKey, OnUp, (int)Dir.W);
        HandleHoldRepeat(kb.sKey, OnDown, (int)Dir.S);
        HandleHoldRepeat(kb.aKey, OnLeft, (int)Dir.A);
        HandleHoldRepeat(kb.dKey, OnRight, (int)Dir.D);
    }

    private void HandleHoldRepeat(KeyControl key, Action onTrigger, int dirIndex)
    {
        if (_held[dirIndex])
        {
            if (!key.isPressed)
            {
                _held[dirIndex] = false;
                return;
            }

            float duration = Time.time - _holdStart[dirIndex];
            float interval = duration < _holdRepeatInitialDelay ? _holdRepeatInitialRate : _holdRepeatFastRate;
            if (Time.time - _lastRepeat[dirIndex] >= interval)
            {
                _lastRepeat[dirIndex] = Time.time;
                onTrigger?.Invoke();
            }
            return;
        }

        if (key.wasPressedThisFrame)
        {
            _held[dirIndex] = true;
            _holdStart[dirIndex] = Time.time;
            _lastRepeat[dirIndex] = Time.time;
            onTrigger?.Invoke();
        }
    }
}