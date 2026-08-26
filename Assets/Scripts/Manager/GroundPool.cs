using UnityEngine;
using UnityEngine.InputSystem;

public class GroundPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Ground groundPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float spawnLookaheadMs = 2000f;

    private bool _debugMode = true;

    private void Update()
    {
        if (!_debugMode)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit5Key.wasPressedThisFrame)
        {
            Spawn(Mathf.RoundToInt(GameTime.ElapsedMs + spawnLookaheadMs));
        }
    }

    public void Spawn(int ms)
    {
        var ground = Instantiate(groundPrefab, spawnParent);
        ground.Initialize(ms);
    }
}