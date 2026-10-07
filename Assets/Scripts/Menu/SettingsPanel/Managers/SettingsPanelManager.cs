using UnityEngine;

public class SettingsPanelManager : MonoBehaviour
{
    public event System.Action OnRequestBack;
    public event System.Action OnRequestCalibrate;

    private bool _acceptInput;

    public void SetInteractable(bool interactable)
    {
        _acceptInput = interactable;
        if (MenuInputManager.Instance != null)
            MenuInputManager.Instance.SetInteractable(interactable);
    }

    private void OnEnable()
    {
        _acceptInput = false;
        if (MenuInputManager.Instance != null)
        {
            MenuInputManager.Instance.OnBack += HandleBack;
            MenuInputManager.Instance.OnCalibrate += HandleCalibrate;
        }
    }

    private void OnDisable()
    {
        if (MenuInputManager.Instance != null)
        {
            MenuInputManager.Instance.OnBack -= HandleBack;
            MenuInputManager.Instance.OnCalibrate -= HandleCalibrate;
        }
    }

    private void HandleBack()
    {
        if (!_acceptInput)
            return;
        OnRequestBack?.Invoke();
    }

    private void HandleCalibrate()
    {
        if (!_acceptInput)
            return;
        OnRequestCalibrate?.Invoke();
    }
}