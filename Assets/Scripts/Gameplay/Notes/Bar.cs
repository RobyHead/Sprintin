using UnityEngine;

public class Bar : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material barMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;

    private int _ms;

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        float z = SpeedTimeline.Instance.GetDistance(GameTime.ElapsedMs, _ms);
        var pos = transform.position;
        pos.z = z;
        transform.position = pos;

        bool visible = z >= SpeedTimeline.Instance.VisibleRangeMin && z <= SpeedTimeline.Instance.VisibleRangeMax;
        SetRenderersVisible(visible);
    }

    public void Initialize(int ms)
    {
        _ms = ms;
        ApplyMaterial();
        SetRenderersVisible(false);
        transform.SetPositionAndRotation(
            new Vector3(0f, 0.0001f, 50f),
            Quaternion.identity
        );
    }

    private void ApplyMaterial()
    {
        if (barMaterial == null)
            return;

        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.material = barMaterial;
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null) renderer.enabled = visible;
        }
    }
}