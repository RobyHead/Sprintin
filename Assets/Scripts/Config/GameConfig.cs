using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    [Header("Notes Movement")]
    [SerializeField] private float speed = 10f;

    [Header("Notes Visibility")]
    [SerializeField] private float visibleRangeMax = 30f;
    [SerializeField] private float visibleRangeMin = -5f;

    public float Speed => speed;
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

    public void SetSpeed(float value)
    {
        speed = value;
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("GameConfig_Speed", speed);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        speed = PlayerPrefs.GetFloat("GameConfig_Speed", speed);
    }
}