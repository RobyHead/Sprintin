using UnityEngine;
using UnityEngine.InputSystem;

public class TitlePanelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TitlePanelAnimation titleAnimation;
    [SerializeField] private KeyHintAnimation keyHintAnimation;

    public event System.Action OnWaitingInput;
    public event System.Action OnExitStarted;
    public event System.Action OnSequenceComplete;
    public event System.Action OnQuitGame;

    private enum State { Idle, Intro, WaitingInput, Outro }
    private State _state;

    private void Awake()
    {
        if (titleAnimation != null)
        {
            titleAnimation.OnIntroComplete += HandleIntroComplete;
            titleAnimation.OnOutroComplete += HandleOutroComplete;
        }
    }

    private void OnDestroy()
    {
        if (titleAnimation != null)
        {
            titleAnimation.OnIntroComplete -= HandleIntroComplete;
            titleAnimation.OnOutroComplete -= HandleOutroComplete;
        }
    }

    public void StartSequence()
    {
        titleAnimation.ResetToOffscreen();
        titleAnimation.PlayIntro();
        _state = State.Intro;
    }

    public void Stop()
    {
        titleAnimation.ResetToOffscreen();
        _state = State.Idle;
    }

    private void HandleIntroComplete()
    {
        if (_state != State.Intro)
            return;

        _state = State.WaitingInput;
        OnWaitingInput?.Invoke();
        if (keyHintAnimation != null)
            keyHintAnimation.StartBlinking();
    }

    private void HandleOutroComplete()
    {
        if (_state != State.Outro)
            return;

        _state = State.Idle;
        OnSequenceComplete?.Invoke();
    }

    private void Update()
    {
        if (_state != State.WaitingInput)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnQuitGame?.Invoke();
        }
        else if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            OnExitStarted?.Invoke();
            _state = State.Outro;
            titleAnimation.PlayOutro();
            if (keyHintAnimation != null)
                keyHintAnimation.StartFadeOut();
        }
    }
}