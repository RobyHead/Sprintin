using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [System.Serializable]
    public struct WeightedPrefab
    {
        public GameObject prefab;
        [Min(0)] public int pieceCount;
    }

    [Header("Prefabs")]
    [SerializeField] private List<WeightedPrefab> piecePrefabs;

    [Header("Spawning")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float visibleBuffer = 20f;

    [Header("Pool")]

    private readonly List<EnvironmentPiece> _pool = new();
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

        foreach (var wp in piecePrefabs)
        {
            int count = wp.pieceCount;
            for (int i = 0; i < count; i++)
            {
                var piece = InstantiatePiece(wp.prefab);
                piece.ReturnToPool();
                _pool.Add(piece);
            }
        }

        PrefillVisibleArea();
        _initialized = true;
    }

    private void OnEnable()
    {
        ChartManager.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        ChartManager.OnGameEnded -= HandleGameEnded;
    }

    private void HandleGameEnded()
    {
        enabled = false;
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
                _pool.Add(_activePieces[i]);
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
        {
            int idx = Random.Range(0, _pool.Count);
            var piece = _pool[idx];
            _pool.RemoveAt(idx);
            return piece;
        }

        var fallbackPrefab = piecePrefabs[Random.Range(0, piecePrefabs.Count)].prefab;
        return InstantiatePiece(fallbackPrefab);
    }

    private EnvironmentPiece InstantiatePiece(GameObject prefab)
    {
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