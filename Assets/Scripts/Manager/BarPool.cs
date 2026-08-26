using UnityEngine;

public class BarPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Bar barPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    public void Spawn(int ms)
    {
        var bar = Instantiate(barPrefab, spawnParent);
        bar.Initialize(ms);
    }
}