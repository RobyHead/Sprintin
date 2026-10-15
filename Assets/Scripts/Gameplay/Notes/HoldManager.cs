using System.Collections.Generic;
using UnityEngine;

public class HoldManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Hold holdPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    private List<Hold>[] _activeHolds = new List<Hold>[4];

    public List<Hold>[] ActiveHolds => _activeHolds;

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
            _activeHolds[i] = new List<Hold>();
    }

    private void Update()
    {
        if (!GameTime.Instance.HasStarted)
            return;

        for (int track = 0; track < 4; track++)
        {
            for (int i = _activeHolds[track].Count - 1; i >= 0; i--)
            {
                if (!_activeHolds[track][i].UpdateState())
                    _activeHolds[track].RemoveAt(i);
            }
        }
    }

    public void Spawn(int trackKey, int ms, int endMs, bool isDual = false)
    {
        var hold = Instantiate(holdPrefab, spawnParent);
        hold.Initialize(trackKey, ms, endMs, isDual);
        _activeHolds[trackKey - 1].Add(hold);
    }

    public void Clear()
    {
        for (int i = 0; i < 4; i++)
            _activeHolds[i].Clear();
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}