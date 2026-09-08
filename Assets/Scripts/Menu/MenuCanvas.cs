using UnityEngine;

public class MenuCanvas : MonoBehaviour
{
    [Header("Panel Managers")]
    [SerializeField] private TitlePanelManager titleManager;
    [SerializeField] private SelectPanelManager selectManager;
    [SerializeField] private SettingsPanelManager settingsManager;

    private void Start()
    {
        selectManager.gameObject.SetActive(false);
        settingsManager.gameObject.SetActive(false);
        titleManager.gameObject.SetActive(false);

        titleManager.OnWaitingInput += OnTitleWaitingInput;
        titleManager.OnExitStarted += OnTitleExitStarted;
        titleManager.OnSequenceComplete += OnTitleSequenceComplete;
        titleManager.OnQuitGame += OnTitleQuitGame;

        selectManager.OnRequestBack += OnSelectRequestBack;
        selectManager.OnRequestSettings += OnSelectRequestSettings;

        settingsManager.OnRequestBack += OnSettingsRequestBack;

        if (SceneTransition.HasPendingReturn)
        {
            SkipToSelect();
            return;
        }

        ShowTitlePanel();
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
        }
    }

    private void OnTitleWaitingInput()
    {
        selectManager.gameObject.SetActive(false);
    }

    private void OnTitleExitStarted()
    {
        selectManager.gameObject.SetActive(true);
        selectManager.Show(false);
    }

    private void OnTitleSequenceComplete()
    {
        titleManager.gameObject.SetActive(false);
        selectManager.SetInteractable(true);
    }

    private void OnTitleQuitGame()
    {
        Application.Quit();
    }

    private void OnSelectRequestBack()
    {
        ShowTitlePanel();
    }

    private void OnSelectRequestSettings()
    {
        selectManager.SetInteractable(false);
        settingsManager.gameObject.SetActive(true);
        settingsManager.Show();
    }

    private void OnSettingsRequestBack()
    {
        settingsManager.Hide();
        settingsManager.gameObject.SetActive(false);
        selectManager.SetInteractable(true);
    }

    private void SkipToSelect()
    {
        titleManager.Stop();
        titleManager.gameObject.SetActive(false);
        settingsManager.Hide();
        settingsManager.gameObject.SetActive(false);
        selectManager.gameObject.SetActive(true);
        selectManager.Show(true);
    }

    private void ShowTitlePanel()
    {
        titleManager.gameObject.SetActive(true);
        titleManager.StartSequence();
    }
}