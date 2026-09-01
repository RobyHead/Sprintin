using UnityEngine;

public class LaneGroup : MonoBehaviour
{
    public static LaneGroup Instance { get; private set; }

    [Header("Track Layout")]
    [SerializeField] private Vector2[] keyPositions = new Vector2[]
    {
        new Vector2(-4f, 3f),
        new Vector2(-3f, 1f),
        new Vector2( 3f, 1f),
        new Vector2( 4f, 3f),
    };

    [SerializeField] private float[] keyRotationsX = new float[]
    {
         60f,
         30f,
        -30f,
        -60f,
    };

    public Vector2[] KeyPositions => keyPositions;
    public float[] KeyRotationsX => keyRotationsX;

    private void Awake()
    {
        Instance = this;
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        for (int i = 0; i < transform.childCount && i < keyPositions.Length; i++)
        {
            var child = transform.GetChild(i);
            var pos = keyPositions[i];
            var rotX = keyRotationsX[i];
            child.SetPositionAndRotation(
                new Vector3(pos.x, pos.y, 0f),
                Quaternion.Euler(rotX, 90f, 0f)
            );
        }
    }

    public void OnLanePressed(int trackIndex)
    {
        if (Judge.Instance == null || !GameTime.HasStarted)
            return;

        Judge.Instance.JudgePress(trackIndex);
    }

    public void OnLaneReleased(int trackIndex)
    {
        if (Judge.Instance == null || !GameTime.HasStarted)
            return;

        Judge.Instance.JudgeRelease(trackIndex);
    }
}