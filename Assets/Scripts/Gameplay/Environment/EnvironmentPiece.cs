using UnityEngine;

public class EnvironmentPiece : MonoBehaviour
{
    public float Length { get; private set; }
    public float FarEdgeZ => transform.position.z + Length;

    private float _spawnZ;
    private MeshRenderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void PlaceAt(float spawnZ)
    {
        _spawnZ = spawnZ;
        Length = CalculateLength();

        if (SpeedTimeline.Instance != null)
            transform.position = PositionAt(spawnZ);

        gameObject.SetActive(true);
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (SpeedTimeline.Instance == null)
            return;

        transform.position = PositionAt(_spawnZ);

        float z = transform.position.z;
        bool visible = z + Length >= SpeedTimeline.Instance.VisibleRangeMin
                       && z <= SpeedTimeline.Instance.VisibleRangeMax;
        foreach (var r in _renderers)
            r.enabled = visible;
    }

    private Vector3 PositionAt(float spawnZ)
    {
        float distanceTraveled = SpeedTimeline.Instance.GetDistance(0, GameTime.ElapsedMs);
        var pos = transform.position;
        pos.z = spawnZ - distanceTraveled;
        return pos;
    }

    private float CalculateLength()
    {
        if (_renderers.Length == 0)
            return 10f;

        return _renderers[0].bounds.size.z;
    }
}