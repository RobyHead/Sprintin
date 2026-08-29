using UnityEngine;

public class Tap : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material tapOutMaterial;
    [SerializeField] private Material tapInMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;

    public int Key => _key;
    public int Ms => _ms;
    public bool IsJudged { get; private set; }

    private int _key;
    private int _ms;

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        if (IsJudged)
        {
            SetRenderersVisible(false);
            return;
        }

        float z = (_ms - GameTime.ElapsedMs) / 1000f * GameConfig.Instance.Speed;
        var pos = transform.position;
        pos.z = z;
        transform.position = pos;

        bool visible = z >= GameConfig.Instance.VisibleRangeMin && z <= GameConfig.Instance.VisibleRangeMax;
        SetRenderersVisible(visible);
    }

    public void Initialize(int trackKey, int ms)
    {
        _key = trackKey;
        _ms = ms;
        ApplyMaterial();
        ApplyStaticTransform();
        SetRenderersVisible(false);
        Judge.Instance.RegisterTap(this);
    }

    public void OnJudged(Judgement judgement, float diff)
    {
        IsJudged = true;
        SetRenderersVisible(false);
        Judge.Instance.UnregisterTap(this);
    }

    private void ApplyStaticTransform()
    {
        var pos = Lanes.Instance.KeyPositions[_key - 1];
        var rotX = Lanes.Instance.KeyRotationsX[_key - 1];
        transform.SetPositionAndRotation(
            new Vector3(pos.x, pos.y, 50f),
            Quaternion.Euler(rotX, 90f, 0f)
        );
    }

    private void ApplyMaterial()
    {
        var mat = (_key == 1 || _key == 4) ? tapOutMaterial : tapInMaterial;
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null) renderer.material = mat;
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