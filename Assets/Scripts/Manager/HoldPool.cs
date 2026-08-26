using UnityEngine;
using UnityEngine.InputSystem;

public class HoldPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Hold holdPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float spawnLookaheadMs = 2000f;
    [SerializeField] private float holdDurationMs = 1000f;

    private bool _debugMode = true;

    private void Update()
    {
        if (!_debugMode)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            if (keyboard[Key.Digit6 + i].wasPressedThisFrame)
            {
                int ms = Mathf.RoundToInt(GameTime.ElapsedMs + spawnLookaheadMs);
                Spawn(i + 1, ms, ms + Mathf.RoundToInt(holdDurationMs));
            }
        }
    }

    public void Spawn(int trackKey, int ms, int endMs)
    {
        var hold = Instantiate(holdPrefab, spawnParent);
        hold.Initialize(trackKey, ms, endMs);
    }
}