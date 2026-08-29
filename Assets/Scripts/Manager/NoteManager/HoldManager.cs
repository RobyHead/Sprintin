using UnityEngine;

public class HoldManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Hold holdPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    public void Spawn(int trackKey, int ms, int endMs)
    {
        var hold = Instantiate(holdPrefab, spawnParent);
        hold.Initialize(trackKey, ms, endMs);
    }

    public void Clear()
    {
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}