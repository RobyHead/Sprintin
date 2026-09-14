using System.Collections;
using UnityEngine;

public class TransitionPanelManager : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private TransitionPanelAnimation panelAnimation;

    public event System.Action OnOutroComplete;
    public event System.Action OnIntroComplete;

    private enum State { Idle, Outro, SceneLoading, Intro }
    private State _state;

    private void Awake()
    {
        panelAnimation.OnOutroComplete += HandleAnimationOutroComplete;
        panelAnimation.OnIntroComplete += HandleAnimationIntroComplete;
    }

    private void OnDestroy()
    {
        panelAnimation.OnOutroComplete -= HandleAnimationOutroComplete;
        panelAnimation.OnIntroComplete -= HandleAnimationIntroComplete;
    }

    public void PlayOutro()
    {
        _state = State.Outro;
        panelAnimation.PlayOutro();
    }

    public void RequestIntro()
    {
        if (_state != State.SceneLoading)
            return;

        _state = State.Intro;
        StartCoroutine(PlayIntroNextFrame());
    }

    private IEnumerator PlayIntroNextFrame()
    {
        yield return null;
        panelAnimation.PlayIntro();
    }

    private void HandleAnimationOutroComplete()
    {
        _state = State.SceneLoading;
        OnOutroComplete?.Invoke();
    }

    private void HandleAnimationIntroComplete()
    {
        _state = State.Idle;
        OnIntroComplete?.Invoke();
    }
}