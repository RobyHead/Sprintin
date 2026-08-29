using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Ground groundPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    public void Spawn(int ms)
    {
        var ground = Instantiate(groundPrefab, spawnParent);
        ground.Initialize(ms);
    }

    public void Clear()
    {
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}