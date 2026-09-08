using UnityEngine;
using UnityEngine.InputSystem;

public class SelectPanelManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private SongList songList;

    public event System.Action OnRequestBack;
    public event System.Action OnRequestSettings;

    public void Show(bool interactable)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        SetInteractable(interactable);
        if (songList != null && !interactable)
            songList.RefreshPreview();
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        SetInteractable(false);
    }

    public void SetInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
        if (songList != null)
            songList.SetInteractable(interactable);
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || !canvasGroup.interactable)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (songList != null)
                songList.StopPreview();
            OnRequestBack?.Invoke();
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
            OnRequestSettings?.Invoke();
    }
}