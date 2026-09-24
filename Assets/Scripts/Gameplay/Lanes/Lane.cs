using UnityEngine;
using UnityEngine.InputSystem;

public class Lane : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material pressedMaterial;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;

    [Header("Input")]
    [SerializeField] private Key key = Key.D;

    public int TrackIndex => _trackIndex;
    private int _trackIndex;

    private void Start()
    {
        _trackIndex = transform.GetSiblingIndex();
        ApplyMaterial(normalMaterial);
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
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard[key].wasPressedThisFrame)
        {
            LaneGroup.Instance.OnLanePressed(_trackIndex);
            ApplyMaterial(pressedMaterial);
        }

        if (keyboard[key].wasReleasedThisFrame)
        {
            LaneGroup.Instance.OnLaneReleased(_trackIndex);
            ApplyMaterial(normalMaterial);
        }
    }

    private void ApplyMaterial(Material mat)
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.material = mat;
        }
    }
}