using UnityEngine;

public class Ground : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material groundMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;

    [Header("Delete")]
    [SerializeField] private float deleteZ = -10f;

    public int Ms => _ms;
    public bool IsJudged { get; private set; }

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
        {
            Judge.Instance.UnregisterGround(this);
            Destroy(gameObject);
        }
    }

    public void Initialize(int ms)
    {
        _ms = ms;
        ApplyMaterial();
        transform.SetPositionAndRotation(
            new Vector3(0f, 0f, 50f),
            Quaternion.identity
        );
        Judge.Instance.RegisterGround(this);
    }

    public void OnJudged(Judgement judgement, float diff)
    {
        IsJudged = true;
    }

    private void ApplyMaterial()
    {
        if (groundMaterial == null)
            return;

        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.material = groundMaterial;
        }
    }
}