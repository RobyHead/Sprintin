using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private TransitionPanelManager panelManager;

    public string PackId { get; private set; }
    public string SongId { get; private set; }
    public int DifficultyId { get; private set; }
    public string SongFolder => $"{PackId}/{SongId}";
    public bool HasPendingReturn { get; private set; }
    public bool IsTransitioning => _phase != Phase.Idle;

    public event System.Action OnOutroStarted;
    public event System.Action OnIntroComplete;

    private enum Phase { Idle, Outro, Loading, Intro }
    private Phase _phase;

    private string _pendingScene;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        panelManager.gameObject.SetActive(false);

        panelManager.OnOutroComplete += HandleOutroComplete;
        panelManager.OnIntroComplete += HandleIntroComplete;
    }

    private void OnDestroy()
    {
        if (panelManager != null)
        {
            panelManager.OnOutroComplete -= HandleOutroComplete;
            panelManager.OnIntroComplete -= HandleIntroComplete;
        }
    }

    public void TransitionToGame(string packId, string songId, int difficultyId)
    {
        PackId = packId;
        SongId = songId;
        DifficultyId = difficultyId;
        HasPendingReturn = false;
        BeginTransition("GameScene");
    }

    public void TransitionToGame()
    {
        HasPendingReturn = false;
        BeginTransition("GameScene");
    }

    public void TransitionToMenu()
    {
        HasPendingReturn = true;
        BeginTransition("MenuScene");
    }

    public bool ConsumePendingReturn()
    {
        bool had = HasPendingReturn;
        HasPendingReturn = false;
        return had;
    }

    private void BeginTransition(string targetScene)
    {
        if (_phase != Phase.Idle)
            return;

        _phase = Phase.Outro;
        _pendingScene = targetScene;

        panelManager.gameObject.SetActive(true);
        panelManager.PlayOutro();
        OnOutroStarted?.Invoke();
    }

    public void RequestIntro()
    {
        if (_phase != Phase.Loading)
            return;

        _phase = Phase.Intro;
        StartCoroutine(PlayIntroNextFrame());
    }

    public IEnumerator PlayOutroAndWait()
    {
        panelManager.gameObject.SetActive(true);
        yield return panelManager.PlayOutroAndWait();
    }

    public IEnumerator PlayIntroAndWait()
    {
        yield return panelManager.PlayIntroAndWait();
        panelManager.gameObject.SetActive(false);
    }

    private IEnumerator PlayIntroNextFrame()
    {
        yield return null;
        panelManager.PlayIntro();
    }

    private void HandleOutroComplete()
    {
        if (_phase != Phase.Outro)
            return;

        _phase = Phase.Loading;
        StartCoroutine(LoadSceneRoutine());
    }

    private void HandleIntroComplete()
    {
        if (_phase != Phase.Intro)
            return;

        _phase = Phase.Idle;
        panelManager.gameObject.SetActive(false);
        OnIntroComplete?.Invoke();
    }

    private IEnumerator LoadSceneRoutine()
    {
        var operation = SceneManager.LoadSceneAsync(_pendingScene);
        _pendingScene = null;
        while (!operation.isDone)
            yield return null;
    }
}