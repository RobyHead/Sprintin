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

    [Header("Volume")]
    [SerializeField] private string musicVolumeKey = "music_volume";
    [SerializeField] private string sfxVolumeKey = "sfx_volume";
    [SerializeField] private float defaultMusicVolume = 100f;
    [SerializeField] private float defaultSfxVolume = 100f;
    private float musicVolume = 100f;
    private float sfxVolume = 100f;

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
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        Load();
    }

    private void Load()
    {
        speed = SafeParseFloat(PlayerPrefs.GetString(speedKey), defaultSpeed) * 10;
        offset = SafeParseFloat(PlayerPrefs.GetString(offsetKey), defaultOffset);
        musicVolume = SafeParseFloat(PlayerPrefs.GetString(musicVolumeKey), defaultMusicVolume);
        sfxVolume = SafeParseFloat(PlayerPrefs.GetString(sfxVolumeKey), defaultSfxVolume);
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