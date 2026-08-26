using UnityEngine;
using UnityEngine.InputSystem;

public class TapPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tap tapPrefab;

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

        for (int i = 0; i < 4; i++)
        {
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
            {
                Spawn(i + 1, Mathf.RoundToInt(GameTime.ElapsedMs + spawnLookaheadMs));
            }
        }
    }

    public void Spawn(int trackKey, int ms)
    {
        var tap = Instantiate(tapPrefab, spawnParent);
        tap.Initialize(trackKey, ms);
    }
}