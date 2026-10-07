using System.Collections;
using UnityEngine;

public class MenuCanvasManager : MonoBehaviour
{
    [Header("Panel Managers")]
    [SerializeField] private SelectPanelManager selectManager;
    [SerializeField] private SettingsPanelManager settingsManager;
    [SerializeField] private CalibratePanelManager calibrateManager;
    [SerializeField] private TitlePanelManager titleManager;

    private bool _pendingTitle;
    private bool _pendingSelect;
    private bool _firstTimeSetup;

    private void Start()
    {
        selectManager.gameObject.SetActive(false);
        settingsManager.gameObject.SetActive(false);
        calibrateManager.gameObject.SetActive(false);
        titleManager.gameObject.SetActive(false);

        titleManager.OnWaitingInput += OnTitleWaitingInput;
        titleManager.OnExitStarted += OnTitleExitStarted;
        titleManager.OnSequenceComplete += OnTitleSequenceComplete;
        titleManager.OnQuitGame += OnTitleQuitGame;

        selectManager.OnRequestBack += OnSelectRequestBack;
        selectManager.OnRequestSettings += OnSelectRequestSettings;

        settingsManager.OnRequestBack += OnSettingsRequestBack;
        settingsManager.OnRequestCalibrate += OnSettingsRequestCalibrate;

        calibrateManager.OnRequestBack += OnCalibrateRequestBack;

        if (SceneTransitionManager.Instance.ConsumeColdStart())
        {
            _pendingTitle = true;
        }
        else
        {
            _pendingSelect = true;
        }

        ExecutePending();

        if (SceneTransitionManager.Instance.IsTransitioning)
        {
            SceneTransitionManager.Instance.OnIntroComplete += OnTransitionIntroComplete;
            SceneTransitionManager.Instance.RequestIntro();
        }
    }

    private void ExecutePending()
    {
        if (_pendingSelect)
        {
            _pendingSelect = false;
            SkipToSelect();
        }
        else if (_pendingTitle)
        {
            _pendingTitle = false;
            ShowTitlePanel();
        }
    }

    private void OnDestroy()
    {
        if (titleManager != null)
        {
            titleManager.OnWaitingInput -= OnTitleWaitingInput;
            titleManager.OnExitStarted -= OnTitleExitStarted;
            titleManager.OnSequenceComplete -= OnTitleSequenceComplete;
            titleManager.OnQuitGame -= OnTitleQuitGame;
        }

        if (selectManager != null)
        {
            selectManager.OnRequestBack -= OnSelectRequestBack;
            selectManager.OnRequestSettings -= OnSelectRequestSettings;
        }

        if (settingsManager != null)
        {
            settingsManager.OnRequestBack -= OnSettingsRequestBack;
            settingsManager.OnRequestCalibrate -= OnSettingsRequestCalibrate;
        }

        if (calibrateManager != null)
        {
            calibrateManager.OnRequestBack -= OnCalibrateRequestBack;
        }
    }

    private void OnTitleWaitingInput()
    {
        selectManager.gameObject.SetActive(false);
    }

    private void OnTitleExitStarted()
    {
        if (!PlayerPrefs.HasKey("offset"))
        {
            _firstTimeSetup = true;
            calibrateManager.gameObject.SetActive(true);
        }
        else
        {
            selectManager.gameObject.SetActive(true);
        }
    }

    private void OnTitleSequenceComplete()
    {
        titleManager.gameObject.SetActive(false);

        if (_firstTimeSetup)
        {
            calibrateManager.SetInteractable(true);
        }
        else
        {
            selectManager.SetInteractable(true);
        }
    }

    private void OnTitleQuitGame()
    {
        Application.Quit();
    }

    private void OnTransitionIntroComplete()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.OnIntroComplete -= OnTransitionIntroComplete;
        selectManager.SetInteractable(true);
    }

    private void OnSelectRequestBack()
    {
        selectManager.SetInteractable(false);
        ShowTitlePanel();
    }

    private void OnSelectRequestSettings()
    {
        selectManager.SetInteractable(false);
        selectManager.StopPreview();
        StartCoroutine(TransitionSelectToSettings());
    }

    private void OnSettingsRequestBack()
    {
        settingsManager.SetInteractable(false);
        StartCoroutine(TransitionSettingsToSelect());
    }

    private void OnSettingsRequestCalibrate()
    {
        settingsManager.SetInteractable(false);
        StartCoroutine(TransitionSettingsToCalibrate());
    }

    private void OnCalibrateRequestBack()
    {
        calibrateManager.SetInteractable(false);

        if (_firstTimeSetup)
        {
            SaveDefaultSettings();
            SceneTransitionManager.Instance.TransitionToGame("default", "tutorial", 0);
        }
        else
        {
            StartCoroutine(TransitionCalibrateToSettings());
        }
    }

    private IEnumerator TransitionSelectToSettings()
    {
        yield return SceneTransitionManager.Instance.PlayOutroAndWait();

        selectManager.gameObject.SetActive(false);
        settingsManager.gameObject.SetActive(true);

        yield return SceneTransitionManager.Instance.PlayIntroAndWait();

        settingsManager.SetInteractable(true);
    }

    private IEnumerator TransitionSettingsToSelect()
    {
        yield return SceneTransitionManager.Instance.PlayOutroAndWait();

        settingsManager.gameObject.SetActive(false);
        selectManager.gameObject.SetActive(true);

        yield return SceneTransitionManager.Instance.PlayIntroAndWait();

        selectManager.SetInteractable(true);
    }

    private IEnumerator TransitionSettingsToCalibrate()
    {
        yield return SceneTransitionManager.Instance.PlayOutroAndWait();

        settingsManager.gameObject.SetActive(false);
        calibrateManager.gameObject.SetActive(true);

        yield return SceneTransitionManager.Instance.PlayIntroAndWait();

        calibrateManager.SetInteractable(true);
    }

    private IEnumerator TransitionCalibrateToSettings()
    {
        yield return SceneTransitionManager.Instance.PlayOutroAndWait();

        calibrateManager.gameObject.SetActive(false);
        settingsManager.gameObject.SetActive(true);

        yield return SceneTransitionManager.Instance.PlayIntroAndWait();

        settingsManager.SetInteractable(true);
    }

    private void SkipToSelect()
    {
        titleManager.Stop();
        titleManager.gameObject.SetActive(false);
        settingsManager.gameObject.SetActive(false);
        calibrateManager.gameObject.SetActive(false);
        selectManager.gameObject.SetActive(true);
        selectManager.RefreshPreview();
    }

    private void ShowTitlePanel()
    {
        titleManager.gameObject.SetActive(true);
        titleManager.StartSequence();
    }

    private void SaveDefaultSettings()
    {
        if (!PlayerPrefs.HasKey("speed"))
            PlayerPrefs.SetString("speed", "2");
        if (!PlayerPrefs.HasKey("music_volume"))
            PlayerPrefs.SetString("music_volume", "100");
        if (!PlayerPrefs.HasKey("sfx_volume"))
            PlayerPrefs.SetString("sfx_volume", "100");
        PlayerPrefs.Save();
        _firstTimeSetup = false;
    }
}