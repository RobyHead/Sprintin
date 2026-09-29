using UnityEngine;
using UnityEngine.InputSystem;

public class ResultPanelManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private ResultInfo resultInfo;

    [Header("Input")]
    [SerializeField] private Key restartKey = Key.R;
    [SerializeField] private Key exitKey = Key.Escape;

    private bool _shown;
    private bool _inputEnabled;

    public void Show()
    {
        if (_shown)
            return;
        _shown = true;

        if (Judge.Instance != null && RecordManager.Instance != null)
        {
            bool fullCombo = Judge.BadCount == 0 && Judge.MissCount == 0;
            RecordManager.Instance.UpdateRecord(
                SceneTransitionManager.Instance.PackId,
                SceneTransitionManager.Instance.SongId,
                SceneTransitionManager.Instance.DifficultyId,
                Judge.Instance.FinalScore,
                Judge.Instance.MaxCombo,
                fullCombo);
        }

        if (resultInfo != null)
            resultInfo.Populate();
    }

    public void EnableInput()
    {
        _inputEnabled = true;
    }

    private void Update()
    {
        if (!_inputEnabled)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard[restartKey].wasPressedThisFrame)
            SceneTransitionManager.Instance.TransitionToGame();

        if (keyboard[exitKey].wasPressedThisFrame)
            SceneTransitionManager.Instance.TransitionToMenu();
    }
}