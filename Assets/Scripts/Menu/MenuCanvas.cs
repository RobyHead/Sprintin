using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCanvas : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup blackBackground;
    [SerializeField] private CanvasGroup titlePanel;
    [SerializeField] private CanvasGroup selectPanel;
    [SerializeField] private CanvasGroup settingsPanel;

    [Header("Timing")]
    [SerializeField] private float blackScreenDuration = 0.5f;
    [SerializeField] private float titleFadeInDuration = 2f;
    [SerializeField] private float exitSlideDuration = 0.5f;
    [SerializeField] private float exitFadeDuration = 1f;

    [Header("Title Animation")]
    [SerializeField] private TitlePanelAnimation titleAnimation;

    private enum State { BlackScreen, TitleFadeIn, TitleSlideIn, WaitingInput, Exit, Done }
    private enum Panel { Title, Select, Settings }

    private State _state;
    private Panel _currentPanel;
    private float _timer;
    private bool _exitSlideStarted;

    private void Start()
    {
        if (SceneTransition.HasPendingReturn)
        {
            SkipToSelect();
            return;
        }

        blackBackground.alpha = 1f;
        blackBackground.gameObject.SetActive(true);
        titlePanel.alpha = 0f;
        titlePanel.gameObject.SetActive(true);
        selectPanel.gameObject.SetActive(false);
        selectPanel.interactable = false;
        settingsPanel.gameObject.SetActive(false);

        _state = State.BlackScreen;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        switch (_state)
        {
            case State.BlackScreen:
                if (_timer >= blackScreenDuration)
                {
                    ShowTitlePanel();
                }
                break;

            case State.TitleFadeIn:
            {
                float t = Mathf.Clamp01(_timer / titleFadeInDuration);
                titlePanel.alpha = t;
                if (_timer >= titleFadeInDuration)
                {
                    blackBackground.gameObject.SetActive(false);
                    selectPanel.gameObject.SetActive(false);
                    _timer = 0f;
                    _state = State.TitleSlideIn;
                    titleAnimation.PlayTitleSlideIn();
                }
                break;
            }

            case State.TitleSlideIn:
                if (titleAnimation.IsSlideInComplete)
                {
                    _state = State.WaitingInput;
                }
                break;

            case State.WaitingInput:
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    _timer = 0f;
                    _state = State.Exit;
                    titleAnimation.PlayExit();
                }
                break;

            case State.Exit:
                if (!_exitSlideStarted)
                {
                    selectPanel.gameObject.SetActive(true);
                }
                if (!_exitSlideStarted && _timer >= exitSlideDuration)
                {
                    _exitSlideStarted = true;
                }
                float exitFadeT = _exitSlideStarted
                    ? Mathf.Clamp01((_timer - exitSlideDuration) / exitFadeDuration)
                    : 0f;
                titlePanel.alpha = 1f - exitFadeT;
                if (_timer >= exitSlideDuration + exitFadeDuration)
                {
                    titlePanel.gameObject.SetActive(false);
                    selectPanel.interactable = true;
                    _currentPanel = Panel.Select;
                    _state = State.Done;
                }
                break;

            case State.Done:
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.escapeKey.wasPressedThisFrame)
                        HandleEscape();
                    else if (Keyboard.current.tabKey.wasPressedThisFrame && _currentPanel == Panel.Select)
                        OpenSettings();
                }
                break;
        }
    }

    private void HandleEscape()
    {
        switch (_currentPanel)
        {
            case Panel.Settings:
                CloseSettings();
                break;
            case Panel.Select:
                ReturnToTitle();
                break;
        }
    }

    private void CloseSettings()
    {
        settingsPanel.gameObject.SetActive(false);
        selectPanel.interactable = true;
        _currentPanel = Panel.Select;
    }

    private void OpenSettings()
    {
        selectPanel.interactable = false;
        settingsPanel.gameObject.SetActive(true);
        _currentPanel = Panel.Settings;
    }

    private void SkipToSelect()
    {
        blackBackground.gameObject.SetActive(false);
        titlePanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
        selectPanel.gameObject.SetActive(true);
        selectPanel.interactable = true;
        _currentPanel = Panel.Select;
        _state = State.Done;
    }

    private void ReturnToTitle()
    {
        ShowTitlePanel();
    }

    private void ShowTitlePanel()
    {
        selectPanel.interactable = false;
        titlePanel.alpha = 0f;
        titlePanel.gameObject.SetActive(true);
        titleAnimation.ResetToOffscreen();
        _timer = 0f;
        _exitSlideStarted = false;
        _currentPanel = Panel.Title;
        _state = State.TitleFadeIn;
    }
}