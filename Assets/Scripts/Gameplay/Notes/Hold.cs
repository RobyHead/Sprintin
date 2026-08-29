using UnityEngine;

public class Hold : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform tail;

    [Header("Materials")]
    [SerializeField] private Material tapOutMaterial;
    [SerializeField] private Material tapInMaterial;
    [SerializeField] private Material bodyMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] headRenderers;
    [SerializeField] private MeshRenderer[] bodyRenderers;
    [SerializeField] private MeshRenderer[] tailRenderers;

    public int Key => _key;
    public int Ms => _ms;
    public int EndMs => _endMs;
    public bool HeadJudged { get; private set; }
    public bool TailJudged { get; private set; }
    public bool HeadWasMiss { get; private set; }

    private int _key;
    private int _ms;
    private int _endMs;
    private bool _fullyJudged;

    private void Update()
    {
        if (!GameTime.HasStarted)
            return;

        if (_fullyJudged)
        {
            SetRenderersVisible(false);
            return;
        }

        float z;

        if (!HeadJudged)
        {
            z = (_ms - GameTime.ElapsedMs) / 1000f * GameConfig.Instance.Speed;
            var pos = transform.position;
            pos.z = z;
            transform.position = pos;
        }
        else if (!TailJudged)
        {
            float remaining = (_endMs - GameTime.ElapsedMs) / 1000f * GameConfig.Instance.Speed;
            if (remaining < 0f) remaining = 0f;

            head.localPosition = Vector3.zero;
            tail.localPosition = new Vector3(-remaining, 0f, 0f);
            body.localPosition = new Vector3(-remaining / 2f, 0f, 0f);

            var bodyScale = body.localScale;
            bodyScale.x = remaining / 2f;
            body.localScale = bodyScale;

            z = 0f;
            var pos = transform.position;
            pos.z = 0f;
            transform.position = pos;
        }
        else
        {
            z = 0f;
        }

        bool visible = z >= GameConfig.Instance.VisibleRangeMin && z <= GameConfig.Instance.VisibleRangeMax;
        SetRenderersVisible(visible);
    }

    public void Initialize(int trackKey, int ms, int endMs)
    {
        _key = trackKey;
        _ms = ms;
        _endMs = endMs;

        ApplyStaticTransform();
        ApplyMaterial();
        SetRenderersVisible(false);
        Judge.Instance.RegisterHold(this);
    }

    public void JudgeHead(Judgement judgement, float diff)
    {
        HeadJudged = true;

        if (judgement == Judgement.Miss)
        {
            HeadWasMiss = true;
            TailJudged = true;
            _fullyJudged = true;
            SetRenderersVisible(false);
            Judge.Instance.UnregisterHold(this);
        }
    }

    public void JudgeTail(Judgement judgement, float diff)
    {
        TailJudged = true;
        _fullyJudged = true;
        SetRenderersVisible(false);
        Judge.Instance.UnregisterHold(this);
    }

    private void ApplyStaticTransform()
    {
        var pos = Lanes.Instance.KeyPositions[_key - 1];
        var rotX = Lanes.Instance.KeyRotationsX[_key - 1];
        float tailOffset = (_endMs - _ms) / 1000f * GameConfig.Instance.Speed;

        var bodyScale = body.localScale;
        bodyScale.x = tailOffset / 2f;
        body.localScale = bodyScale;

        head.localPosition = Vector3.zero;
        tail.localPosition = new Vector3(-tailOffset, 0f, 0f);
        body.localPosition = new Vector3(-tailOffset / 2f, 0f, 0f);

        transform.SetPositionAndRotation(
            new Vector3(pos.x, pos.y, 50f),
            Quaternion.Euler(rotX, 90f, 0f)
        );
    }

    private void ApplyMaterial()
    {
        var mat = (_key == 1 || _key == 4) ? tapOutMaterial : tapInMaterial;
        foreach (var renderer in headRenderers)
        {
            if (renderer != null) renderer.material = mat;
        }
        foreach (var renderer in tailRenderers)
        {
            if (renderer != null) renderer.material = mat;
        }
        foreach (var renderer in bodyRenderers)
        {
            if (renderer != null)
            {
                if (bodyMaterial != null) renderer.material = bodyMaterial;
                else renderer.material = mat;
            }
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var renderer in headRenderers)
            if (renderer != null) renderer.enabled = visible;
        foreach (var renderer in bodyRenderers)
            if (renderer != null) renderer.enabled = visible;
        foreach (var renderer in tailRenderers)
            if (renderer != null) renderer.enabled = visible;
    }
}