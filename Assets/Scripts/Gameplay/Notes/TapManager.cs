using UnityEngine;

public class TapManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tap tapPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    public void Spawn(int trackKey, int ms, bool isDual = false)
    {
        var tap = Instantiate(tapPrefab, spawnParent);
        tap.Initialize(trackKey, ms, isDual);
    }

    public void Clear()
    {
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}