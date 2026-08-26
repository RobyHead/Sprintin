using UnityEngine;

public class GroundPool : MonoBehaviour
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
}