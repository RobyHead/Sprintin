using System.Collections;
using UnityEngine;

public class TransitionPanelManager : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private TransitionPanelAnimation panelAnimation;

    public event System.Action OnOutroComplete;
    public event System.Action OnIntroComplete;

    private void Awake()
    {
        panelAnimation.OnOutroComplete += HandleOutroComplete;
        panelAnimation.OnIntroComplete += HandleIntroComplete;
    }

    private void OnDestroy()
    {
        panelAnimation.OnOutroComplete -= HandleOutroComplete;
        panelAnimation.OnIntroComplete -= HandleIntroComplete;
    }

    public void PlayOutro()
    {
        panelAnimation.PlayOutro();
    }

    public void PlayIntro()
    {
        panelAnimation.PlayIntro();
    }

    public IEnumerator PlayOutroAndWait()
    {
        bool done = false;
        System.Action handler = null;
        handler = () =>
        {
            done = true;
            OnOutroComplete -= handler;
        };
        OnOutroComplete += handler;
        PlayOutro();
        yield return new WaitUntil(() => done);
    }

    public IEnumerator PlayIntroAndWait()
    {
        bool done = false;
        System.Action handler = null;
        handler = () =>
        {
            done = true;
            OnIntroComplete -= handler;
        };
        OnIntroComplete += handler;
        PlayIntro();
        yield return new WaitUntil(() => done);
    }

    private void HandleOutroComplete()
    {
        OnOutroComplete?.Invoke();
    }

    private void HandleIntroComplete()
    {
        OnIntroComplete?.Invoke();
    }
}