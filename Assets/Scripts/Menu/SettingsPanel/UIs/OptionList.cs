using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionList : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    [Header("Layout")]
    [SerializeField] private float optionHeight = 80f;
    [SerializeField] private float spacing = 10f;

    [Header("Snapping")]
    [SerializeField] private float snapSpeed = 10f;

    private readonly List<OptionEntry> _entries = new();
    private int _selectedIndex;
    private float _snapTargetY;
    private bool _isSnapping;

    private enum NavKey { None, W, S, A, D }
    private NavKey _heldKey = NavKey.None;
    private float _holdStartTime;
    private float _lastRepeatTime;

    private class OptionEntry
    {
        public Option Option;
        public RectTransform Rect;
        public float BaseY;
    }

    public void Open()
    {
        if (_entries.Count == 0)
        {
            var options = content.GetComponentsInChildren<Option>(true);
            if (options.Length == 0) return;

            float y = -optionHeight / 2f;

            foreach (var option in options)
            {
                var rect = option.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0f, y);

                var entry = new OptionEntry
                {
                    Option = option,
                    Rect = rect,
                    BaseY = y
                };
                _entries.Add(entry);

                y -= optionHeight + spacing;
            }

            content.sizeDelta = new Vector2(content.sizeDelta.x, -y);
        }

        foreach (var entry in _entries)
            entry.Option.SetSelected(false);

        _selectedIndex = 0;
        _heldKey = NavKey.None;
        UpdateSelection();
        JumpToSelected();
    }

    public void Close()
    {
    }

    private void Update()
    {
        HandleInput();
        UpdateSnapping();
    }

    private void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (_heldKey != NavKey.None)
        {
            if (!IsNavKeyPressed(kb, _heldKey))
            {
                _heldKey = NavKey.None;
                return;
            }

            float holdDuration = Time.time - _holdStartTime;
            float interval = holdDuration < 0.5f ? 0.2f : 0.05f;
            if (Time.time - _lastRepeatTime >= interval)
            {
                _lastRepeatTime = Time.time;
                DoNavAction(_heldKey);
            }
            return;
        }

        NavKey pressed = NavKey.None;
        if (kb.wKey.wasPressedThisFrame) pressed = NavKey.W;
        else if (kb.sKey.wasPressedThisFrame) pressed = NavKey.S;
        else if (kb.aKey.wasPressedThisFrame) pressed = NavKey.A;
        else if (kb.dKey.wasPressedThisFrame) pressed = NavKey.D;

        if (pressed == NavKey.None) return;

        _heldKey = pressed;
        _holdStartTime = Time.time;
        _lastRepeatTime = Time.time;
        DoNavAction(pressed);
    }

    private bool IsNavKeyPressed(Keyboard kb, NavKey key)
    {
        return key switch
        {
            NavKey.W => kb.wKey.isPressed,
            NavKey.S => kb.sKey.isPressed,
            NavKey.A => kb.aKey.isPressed,
            NavKey.D => kb.dKey.isPressed,
            _ => false
        };
    }

    private void DoNavAction(NavKey key)
    {
        switch (key)
        {
            case NavKey.W: SelectPrevious(); break;
            case NavKey.S: SelectNext(); break;
            case NavKey.A: _entries[_selectedIndex].Option.Decrease(); break;
            case NavKey.D: _entries[_selectedIndex].Option.Increase(); break;
        }
    }

    private void SelectPrevious()
    {
        if (_entries.Count == 0) return;
        _selectedIndex = (_selectedIndex - 1 + _entries.Count) % _entries.Count;
        UpdateSelection();
        SnapToSelected();
    }

    private void SelectNext()
    {
        if (_entries.Count == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _entries.Count;
        UpdateSelection();
        SnapToSelected();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < _entries.Count; i++)
            _entries[i].Option.SetSelected(i == _selectedIndex);
    }

    private void SnapToSelected()
    {
        if (_entries.Count == 0) return;
        _snapTargetY = -_entries[_selectedIndex].BaseY;
        _isSnapping = true;
    }

    private void JumpToSelected()
    {
        if (_entries.Count == 0) return;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, -_entries[_selectedIndex].BaseY);
        _isSnapping = false;
    }

    private void UpdateSnapping()
    {
        if (!_isSnapping) return;

        var target = new Vector2(content.anchoredPosition.x, _snapTargetY);
        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, target,
            snapSpeed * Time.deltaTime);

        if (Mathf.Abs(content.anchoredPosition.y - _snapTargetY) < 0.5f)
        {
            content.anchoredPosition = target;
            _isSnapping = false;
        }
    }
}