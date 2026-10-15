using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Ground groundPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    private List<Ground> _activeGrounds = new List<Ground>();

    public List<Ground> ActiveGrounds => _activeGrounds;

    private void Update()
    {
        if (!GameTime.Instance.HasStarted)
            return;

        for (int i = _activeGrounds.Count - 1; i >= 0; i--)
        {
            if (!_activeGrounds[i].UpdateState())
                _activeGrounds.RemoveAt(i);
        }
    }

    public void Spawn(int ms)
    {
        var ground = Instantiate(groundPrefab, spawnParent);
        ground.Initialize(ms);
        _activeGrounds.Add(ground);
    }

    public void Clear()
    {
        _activeGrounds.Clear();
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}