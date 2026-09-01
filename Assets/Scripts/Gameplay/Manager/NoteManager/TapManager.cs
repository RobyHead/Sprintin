using UnityEngine;

public class TapManager : MonoBehaviour
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

    public void Clear()
    {
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}