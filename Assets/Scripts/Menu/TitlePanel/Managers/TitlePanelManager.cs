using UnityEngine;
using UnityEngine.InputSystem;

public class TitlePanelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TitlePanelAnimation titleAnimation;

    public event System.Action OnWaitingInput;
    public event System.Action OnExitStarted;
    public event System.Action OnSequenceComplete;
    public event System.Action OnQuitGame;

    private enum State { Idle, Intro, WaitingInput, Outro }
    private State _state;

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

    private void Update()
    {
        switch (_state)
        {
            case State.Intro:
                if (titleAnimation.IsIntroComplete)
                {
                    _state = State.WaitingInput;
                    OnWaitingInput?.Invoke();
                }
                break;

            case State.WaitingInput:
                if (Keyboard.current == null)
                    break;

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    OnQuitGame?.Invoke();
                }
                else if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    OnExitStarted?.Invoke();
                    _state = State.Outro;
                    titleAnimation.PlayOutro();
                }
                break;

            case State.Outro:
                if (titleAnimation.IsOutroComplete)
                {
                    _state = State.Idle;
                    OnSequenceComplete?.Invoke();
                }
                break;
        }
    }
}