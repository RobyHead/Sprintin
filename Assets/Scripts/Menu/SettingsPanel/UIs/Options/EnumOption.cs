using System;
using TMPro;
using UnityEngine;

public class EnumOption : Option
{
    [Serializable]
    public struct Entry
    {
        public string label;
        public string displayName;
    }

    [Header("Entries")]
    [SerializeField] private Entry[] entries;

    [Header("Persistence")]
    [SerializeField] private string persistKey;
    [SerializeField] private string defaultLabel;

    [Header("Display")]
    [SerializeField] private TMP_Text valueText;

    private int _currentIndex;

    public override string PersistKey => persistKey;
    public string CurrentLabel => entries.Length > 0 ? entries[_currentIndex].label : string.Empty;
    public int CurrentIndex => _currentIndex;

    public override void LoadDefault()
    {
        _currentIndex = FindLabelIndex(defaultLabel);
        UpdateDisplay();
    }

    public override void LoadFrom(string data)
    {
        _currentIndex = FindLabelIndex(data);
        UpdateDisplay();
    }

    public override string SaveData()
    {
        return CurrentLabel;
    }

    private int FindLabelIndex(string label)
    {
        for (int i = 0; i < entries.Length; i++)
            if (entries[i].label == label)
                return i;
        return 0;
    }

    public override void Increase()
    {
        if (entries.Length == 0) return;
        _currentIndex = (_currentIndex + 1) % entries.Length;
        UpdateDisplay();
        NotifyValueChanged();
    }

    public override void Decrease()
    {
        if (entries.Length == 0) return;
        _currentIndex = (_currentIndex - 1 + entries.Length) % entries.Length;
        UpdateDisplay();
        NotifyValueChanged();
    }

    private void UpdateDisplay()
    {
        if (valueText != null && entries.Length > 0)
            valueText.text = entries[_currentIndex].displayName;
    }
}