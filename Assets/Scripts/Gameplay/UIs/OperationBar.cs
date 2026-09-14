using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public enum OperationAction
{
    Restart,
    ExitToMenu,
    Custom
}

public class OperationBar : MonoBehaviour
{
    [System.Serializable]
    public class OperationEntry
    {
        public Key key;
        public string displayName;
        public OperationAction action;
        public UnityEvent onCustom;
    }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform bar;
    [SerializeField] private TMP_Text operationText;

    [Header("Settings")]
    [SerializeField] private float barMaxWidth = 200f;
    [SerializeField] private float holdDuration = 1.5f;

    [Header("Operations")]
    [SerializeField] private OperationEntry[] operations;

    private int _activeIndex = -1;
    private float _holdTimer;
    private bool _isHolding;

    private void Start()
    {
        Hide();

        if (background != null)
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barMaxWidth);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (_isHolding)
        {
            var activeOp = operations[_activeIndex];
            if (keyboard[activeOp.key].isPressed)
            {
                _holdTimer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(_holdTimer / holdDuration);
                UpdateBar(progress);

                if (_holdTimer >= holdDuration)
                {
                    ExecuteOperation(activeOp);
                    ResetOperation();
                }
            }
            else
            {
                ResetOperation();
            }
        }
        else
        {
            for (int i = 0; i < operations.Length; i++)
            {
                if (keyboard[operations[i].key].wasPressedThisFrame)
                {
                    _activeIndex = i;
                    _isHolding = true;
                    _holdTimer = 0f;

                    Show();

                    if (operationText != null)
                        operationText.text = operations[i].displayName;

                    UpdateBar(0f);
                    break;
                }
            }
        }
    }

    private void ExecuteOperation(OperationEntry op)
    {
        switch (op.action)
        {
            case OperationAction.Restart:
                SceneTransitionManager.Instance.TransitionToGame();
                break;

            case OperationAction.ExitToMenu:
                SceneTransitionManager.Instance.TransitionToMenu();
                break;

            case OperationAction.Custom:
                op.onCustom.Invoke();
                break;
        }
    }

    private void UpdateBar(float progress)
    {
        if (bar != null)
        {
            var anchorMax = bar.anchorMax;
            anchorMax.x = progress;
            bar.anchorMax = anchorMax;
        }
    }

    private void ResetOperation()
    {
        _isHolding = false;
        _activeIndex = -1;
        _holdTimer = 0f;

        Hide();
    }

    private void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}