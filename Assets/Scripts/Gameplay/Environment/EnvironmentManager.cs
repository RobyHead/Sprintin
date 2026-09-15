using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> piecePrefabs;

    [Header("Spawning")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float visibleBuffer = 20f;

    [Header("Pool")]
    [SerializeField] private int prewarmCount = 10;

    private readonly Queue<EnvironmentPiece> _pool = new();
    private readonly List<EnvironmentPiece> _activePieces = new();
    private float _furthestSpawnZ;
    private bool _initialized;

    private void Start()
    {
        if (piecePrefabs == null || piecePrefabs.Count == 0)
        {
            Debug.LogError("EnvironmentManager: No piece prefabs assigned");
            return;
        }

        for (int i = 0; i < prewarmCount; i++)
        {
            var piece = InstantiateRandom();
            piece.ReturnToPool();
            _pool.Enqueue(piece);
        }

        PrefillVisibleArea();
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized || SpeedTimeline.Instance == null)
            return;

        float distanceTraveled = SpeedTimeline.Instance.GetDistance(0, GameTime.ElapsedMs);
        float visibleFront = SpeedTimeline.Instance.VisibleRangeMax + distanceTraveled + visibleBuffer;

        while (_furthestSpawnZ < visibleFront)
            SpawnAt(_furthestSpawnZ);

        for (int i = _activePieces.Count - 1; i >= 0; i--)
        {
            if (_activePieces[i].FarEdgeZ < SpeedTimeline.Instance.VisibleRangeMin)
            {
                _activePieces[i].ReturnToPool();
                _pool.Enqueue(_activePieces[i]);
                _activePieces.RemoveAt(i);
            }
        }
    }

    private void PrefillVisibleArea()
    {
        float distanceTraveled = 0f;
        float rangeMin = 0f;
        float rangeMax = 30f;

        if (SpeedTimeline.Instance != null)
        {
            distanceTraveled = SpeedTimeline.Instance.GetDistance(0, GameTime.ElapsedMs);
            rangeMin = SpeedTimeline.Instance.VisibleRangeMin;
            rangeMax = SpeedTimeline.Instance.VisibleRangeMax;
        }

        _furthestSpawnZ = rangeMin + distanceTraveled;
        float visibleFront = rangeMax + distanceTraveled + visibleBuffer;

        while (_furthestSpawnZ < visibleFront)
            SpawnAt(_furthestSpawnZ);
    }

    private void SpawnAt(float spawnZ)
    {
        var piece = GetFromPool();
        piece.PlaceAt(spawnZ);
        _activePieces.Add(piece);
        _furthestSpawnZ = spawnZ + piece.Length;
    }

    private EnvironmentPiece GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        return InstantiateRandom();
    }

    private EnvironmentPiece InstantiateRandom()
    {
        var prefab = piecePrefabs[Random.Range(0, piecePrefabs.Count)];
        var go = Instantiate(prefab, spawnParent);
        go.name = prefab.name;
        var piece = go.GetComponent<EnvironmentPiece>();
        if (piece == null)
            piece = go.AddComponent<EnvironmentPiece>();
        return piece;
    }

    public void Clear()
    {
        _activePieces.Clear();
        _pool.Clear();

        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);

        _initialized = false;
    }
}