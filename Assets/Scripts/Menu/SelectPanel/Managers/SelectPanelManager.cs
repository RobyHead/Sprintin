using UnityEngine;

public class SelectPanelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SongList songList;

    public event System.Action OnRequestBack;
    public event System.Action OnRequestSettings;
    public event System.Action OnCoversReady;

    private bool _acceptInput;

    public bool IsCoverCacheReady =>
        songList != null && songList.IsCoverCacheReady;

    public void SetInteractable(bool interactable)
    {
        _acceptInput = interactable;
        if (MenuInputManager.Instance != null)
            MenuInputManager.Instance.SetInteractable(interactable);
    }

    public void RefreshPreview()
    {
        if (songList != null)
            songList.RefreshPreview();
    }

    public void StopPreview()
    {
        if (songList != null)
            songList.StopPreview();
    }

    private void OnEnable()
    {
        _acceptInput = false;
        RefreshPreview();

        if (songList != null)
            songList.OnCoverCacheReady += OnCoverCacheReady;

        if (MenuInputManager.Instance != null)
        {
            MenuInputManager.Instance.OnBack += HandleBack;
            MenuInputManager.Instance.OnSettings += HandleSettings;
        }
    }

    private void OnDisable()
    {
        if (songList != null)
            songList.OnCoverCacheReady -= OnCoverCacheReady;

        if (MenuInputManager.Instance != null)
        {
            MenuInputManager.Instance.OnBack -= HandleBack;
            MenuInputManager.Instance.OnSettings -= HandleSettings;
        }
    }

    private void OnCoverCacheReady()
    {
        OnCoversReady?.Invoke();
    }

    private void HandleBack()
    {
        if (!_acceptInput)
            return;
        StopPreview();
        OnRequestBack?.Invoke();
    }

    private void HandleSettings()
    {
        if (!_acceptInput)
            return;
        OnRequestSettings?.Invoke();
    }
}