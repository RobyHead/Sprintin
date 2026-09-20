using UnityEngine;
using UnityEngine.InputSystem;

public class SelectPanelManager : MonoBehaviour
{
    [SerializeField] private SongList songList;

    public event System.Action OnRequestBack;
    public event System.Action OnRequestSettings;

    private bool _acceptInput;

    public void SetInteractable(bool interactable)
    {
        _acceptInput = interactable;
        if (songList != null)
            songList.SetInteractable(interactable);
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
            songList.SetInteractable(false);
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || !_acceptInput)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopPreview();
            OnRequestBack?.Invoke();
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
            OnRequestSettings?.Invoke();
    }
}