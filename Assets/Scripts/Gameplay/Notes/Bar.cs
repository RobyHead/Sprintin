using UnityEngine;

public class Bar : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material barMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;

    [Header("Delete")]
    [SerializeField] private float deleteZ = -10f;

    private int _ms;

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        float z = (_ms - GameTime.ElapsedMs) / 1000f * GameConfig.Instance.Speed;
        var pos = transform.position;
        pos.z = z;
        transform.position = pos;

        if (z < deleteZ)
            Destroy(gameObject);
    }

    public void Initialize(int ms)
    {
        _ms = ms;
        ApplyMaterial();
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
}