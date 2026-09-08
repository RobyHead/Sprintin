using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsPanelManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public event System.Action OnRequestBack;

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            OnRequestBack?.Invoke();
    }
}