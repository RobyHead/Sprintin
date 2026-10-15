using System.Collections.Generic;
using UnityEngine;

public class TapManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tap tapPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;

    private List<Tap>[] _activeTaps = new List<Tap>[4];

    public List<Tap>[] ActiveTaps => _activeTaps;

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
            _activeTaps[i] = new List<Tap>();
    }

    private void Update()
    {
        if (!GameTime.Instance.HasStarted)
            return;

        for (int track = 0; track < 4; track++)
        {
            for (int i = _activeTaps[track].Count - 1; i >= 0; i--)
            {
                _activeTaps[track][i].UpdateState();
                if (_activeTaps[track][i].IsJudged)
                    _activeTaps[track].RemoveAt(i);
            }
        }
    }

    public void Spawn(int trackKey, int ms, bool isDual = false)
    {
        var tap = Instantiate(tapPrefab, spawnParent);
        tap.Initialize(trackKey, ms, isDual);
        _activeTaps[trackKey - 1].Add(tap);
    }

    public void Clear()
    {
        for (int i = 0; i < 4; i++)
            _activeTaps[i].Clear();
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);
    }
}