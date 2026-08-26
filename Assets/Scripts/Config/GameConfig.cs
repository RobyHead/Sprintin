using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    [Header("Notes Movement")]
    [SerializeField] private float speed = 10f;

    public float Speed => speed;

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