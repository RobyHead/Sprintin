using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    [Header("Notes Movement")]
    [SerializeField] private string speedKey = "GameConfig_Speed";
    private float speed;

    [Header("Offset")]
    [SerializeField] private string offsetKey = "GameConfig_Offset";
    private float offset;

    [Header("Notes Visibility")]
    [SerializeField] private float visibleRangeMax = 30f;
    [SerializeField] private float visibleRangeMin = -5f;

    public float Speed => speed;
    public float Offset => offset;
    public float VisibleRangeMax => visibleRangeMax;
    public float VisibleRangeMin => visibleRangeMin;

    private void Awake()
    {
        Instance = this;
        Load();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Load()
    {
        speed = float.Parse(PlayerPrefs.GetString(speedKey)) * 10;
        offset = float.Parse(PlayerPrefs.GetString(offsetKey));
    }
}