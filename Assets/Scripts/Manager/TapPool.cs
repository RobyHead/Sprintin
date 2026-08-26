using UnityEngine;

public class TapPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tap tapPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    public void Spawn(int trackKey, int ms)
    {
        var tap = Instantiate(tapPrefab, spawnParent);
        tap.Initialize(trackKey, ms);
    }
}