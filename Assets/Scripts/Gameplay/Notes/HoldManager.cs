using UnityEngine;

public class HoldManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Hold holdPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    public void Spawn(int trackKey, int ms, int endMs, bool isDual = false)
    {
        var hold = Instantiate(holdPrefab, spawnParent);
        hold.Initialize(trackKey, ms, endMs, isDual);
    }

    public void Clear()
    {
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}