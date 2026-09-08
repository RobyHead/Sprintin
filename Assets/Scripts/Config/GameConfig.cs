using System.Globalization;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    [Header("Notes Movement")]
    [SerializeField] private string speedKey = "speed";
    [SerializeField] private float defaultSpeed = 1f;
    private float speed;

    [Header("Offset")]
    [SerializeField] private string offsetKey = "offset";
    [SerializeField] private float defaultOffset = 0f;
    private float offset;

    [Header("Notes Visibility")]
    [SerializeField] private float visibleRangeMax = 30f;
    [SerializeField] private float visibleRangeMin = -5f;

    [Header("Timing")]
    [SerializeField] private float blankMs = 3000f;
    [SerializeField] private float maxSkipMs = 2000f;
    [SerializeField] private float fadeEndMs = 1000f;

    public float Speed => speed;
    public float Offset => offset;
    public float VisibleRangeMax => visibleRangeMax;
    public float VisibleRangeMin => visibleRangeMin;
    public float BlankMs => blankMs;
    public float MaxSkipMs => maxSkipMs;
    public float FadeEndMs => fadeEndMs;

    private void Awake()
    {
        Load();
    }

    private void Load()
    {
        speed = SafeParseFloat(PlayerPrefs.GetString(speedKey), defaultSpeed) * 10;
        offset = SafeParseFloat(PlayerPrefs.GetString(offsetKey), defaultOffset);
    }

    private static float SafeParseFloat(string s, float fallback)
    {
        if (string.IsNullOrEmpty(s))
            return fallback;
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            return val;
        return fallback;
    }
}