using UnityEngine;

public class Ground : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material groundMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;

    [Header("Landing")]
    [SerializeField] private float landingStartMs = 100f;
    [SerializeField] private float landingDurationMs = 80f;
    [SerializeField] private float landingY = -0.18f;

    public int Ms => _ms;
    public bool IsJudged { get; private set; }

    private int _ms;
    private bool _hidden;

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        if (_hidden)
        {
            SetRenderersVisible(false);
            return;
        }

        float z = (_ms - GameTime.ElapsedMs) / 1000f * GameConfig.Instance.Speed;
        var pos = transform.position;
        pos.z = z;

        float elapsedSinceMs = GameTime.ElapsedMs - _ms;
        if (IsJudged && elapsedSinceMs >= landingStartMs)
        {
            float landingT = Mathf.Clamp01((elapsedSinceMs - landingStartMs) / landingDurationMs);
            pos.y = Mathf.Lerp(0f, landingY, landingT);

            if (landingT >= 1f)
            {
                _hidden = true;
                SetRenderersVisible(false);
                Judge.Instance.UnregisterGround(this);
                return;
            }
        }
        else
        {
            pos.y = 0f;
        }

        transform.position = pos;

        bool visible = z >= GameConfig.Instance.VisibleRangeMin && z <= GameConfig.Instance.VisibleRangeMax;
        SetRenderersVisible(visible);
    }

    public void Initialize(int ms)
    {
        _ms = ms;
        ApplyMaterial();
        SetRenderersVisible(false);
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

    private void SetRenderersVisible(bool visible)
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null) renderer.enabled = visible;
        }
    }
}