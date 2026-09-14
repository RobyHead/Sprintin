using System.Globalization;
using TMPro;
using UnityEngine;

public class NumericOption : Option
{
    [Header("Value Range")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float step = 1f;
    [SerializeField] private string format = "F0";

    [Header("Persistence")]
    [SerializeField] private string persistKey;
    [SerializeField] private float defaultValue;

    [Header("Display")]
    [SerializeField] private TMP_Text valueText;

    private float _currentValue;

    public override string PersistKey => persistKey;
    public float Value => _currentValue;

    public override void LoadDefault()
    {
        _currentValue = defaultValue;
        UpdateDisplay();
    }

    public override void LoadFrom(string data)
    {
        if (float.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            _currentValue = Mathf.Clamp(val, minValue, maxValue);
        else
            _currentValue = defaultValue;
        UpdateDisplay();
    }

    public override string SaveData()
    {
        return _currentValue.ToString(CultureInfo.InvariantCulture);
    }

    public override void Increase()
    {
        _currentValue = Mathf.Min(_currentValue + step, maxValue);
        UpdateDisplay();
        NotifyValueChanged();
    }

    public override void Decrease()
    {
        _currentValue = Mathf.Max(_currentValue - step, minValue);
        UpdateDisplay();
        NotifyValueChanged();
    }

    private void UpdateDisplay()
    {
        if (valueText != null)
            valueText.text = _currentValue.ToString(format);
    }
}