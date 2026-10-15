using System.Collections.Generic;
using UnityEngine;

public class BarManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Bar barPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    private List<Bar> _activeBars = new List<Bar>();

    private void Update()
    {
        if (!GameTime.Instance.HasStarted)
            return;

        for (int i = 0; i < _activeBars.Count; i++)
            _activeBars[i].UpdateState();
    }

    public void Spawn(int ms)
    {
        var bar = Instantiate(barPrefab, spawnParent);
        bar.Initialize(ms);
        _activeBars.Add(bar);
    }

    public void Clear()
    {
        _activeBars.Clear();
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}