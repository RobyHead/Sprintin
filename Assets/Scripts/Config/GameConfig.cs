using UnityEngine;

public class GameConfig : MonoBehaviour
{
    [Header("Notes Movement")]
    [SerializeField] private string speedKey = "GameConfig_Speed";
    private float speed;

    [Header("Offset")]
    [SerializeField] private string offsetKey = "GameConfig_Offset";
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
        speed = float.Parse(PlayerPrefs.GetString(speedKey)) * 10;
        offset = float.Parse(PlayerPrefs.GetString(offsetKey));
    }
}