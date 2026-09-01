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
    private Material _currentMaterial;
    private bool _wasPressed;

    private void Start()
    {
        _trackIndex = transform.GetSiblingIndex();
        ApplyMaterial(normalMaterial);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool isPressed = keyboard[key].isPressed;

        if (isPressed && !_wasPressed)
        {
            LaneGroup.Instance.OnLanePressed(_trackIndex);
        }
        else if (!isPressed && _wasPressed)
        {
            LaneGroup.Instance.OnLaneReleased(_trackIndex);
        }

        _wasPressed = isPressed;

        var targetMaterial = isPressed ? pressedMaterial : normalMaterial;
        if (targetMaterial != _currentMaterial)
        {
            ApplyMaterial(targetMaterial);
        }
    }

    private void ApplyMaterial(Material mat)
    {
        _currentMaterial = mat;
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
                renderer.material = mat;
        }
    }
}