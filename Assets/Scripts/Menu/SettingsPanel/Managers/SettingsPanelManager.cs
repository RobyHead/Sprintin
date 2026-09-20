using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsPanelManager : MonoBehaviour
{
    public event System.Action OnRequestBack;
    public event System.Action OnRequestCalibrate;

    private bool _acceptInput;

    public void SetInteractable(bool interactable)
    {
        _acceptInput = interactable;
    }

    private void OnEnable()
    {
        _acceptInput = false;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || !_acceptInput)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            OnRequestBack?.Invoke();
        else if (Keyboard.current.cKey.wasPressedThisFrame)
            OnRequestCalibrate?.Invoke();
    }
}