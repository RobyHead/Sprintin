using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongList : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    [Header("Prefabs")]
    [SerializeField] private PackEntry packPrefab;
    [SerializeField] private SongEntry songPrefab;

    [Header("Layout")]
    [SerializeField] private float packHeight = 80f;
    [SerializeField] private float songHeight = 120f;
    [SerializeField] private float spacing = 10f;

    [Header("Snapping")]
    [SerializeField] private float snapSpeed = 10f;
    [SerializeField] private float scaleMax = 1f;
    [SerializeField] private float scaleMin = 0.7f;

    [Header("Info")]
    [SerializeField] private SongInfo songInfo;
    [SerializeField] private float stableTimeMs = 800f;

    [Header("Preview")]
    [SerializeField] private SongPreviewManager songPreview;

    private readonly List<SongListEntry> _entries = new();
    private int _snappedIndex = -1;
    private float _snapTargetY;
    private bool _isSnapping;
    private string _songsPath;
    private float _stableTimer;
    private bool _previewStarted;

    private readonly Dictionary<string, Texture2D> _coverCache = new();
    private Coroutine _preloadRoutine;
    private bool _coverCacheReady;
    public bool IsCoverCacheReady => _coverCacheReady;
    public event System.Action OnCoverCacheReady;

    private string _restorePackId;
    private string _restoreSongId;
    private int _restoreDifficultyId;
    private bool _finishedInit;

    public void StopPreview()
    {
        _previewStarted = true;
        if (songPreview != null)
            songPreview.FadeOutAndStop();
    }

    public void RefreshPreview()
    {
        _previewStarted = false;
        _stableTimer = 0f;
    }

    public event System.Action<string, string, int> OnRequestStartGame;

    private class SongListEntry
    {
        public SongListItem Data;
        public RectTransform Rect;
        public PackEntry PackUI;
        public SongEntry SongUI;
        public float BaseY;
    }

    public void Initialize(List<SongListItem> items, string songsPath,
        string restorePackId = null, string restoreSongId = null, int restoreDifficultyId = -1)
    {
        _songsPath = songsPath;
        _restorePackId = restorePackId;
        _restoreSongId = restoreSongId;
        _restoreDifficultyId = restoreDifficultyId;
        _finishedInit = false;
        _coverCacheReady = false;

        scrollRect.enabled = false;
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        BuildList(items);

        if (_preloadRoutine != null)
            StopCoroutine(_preloadRoutine);
        _preloadRoutine = StartCoroutine(PreloadAllCovers(items));
    }

    private IEnumerator PreloadAllCovers(List<SongListItem> items)
    {
        _coverCacheReady = false;
        _coverCache.Clear();

        foreach (var item in items)
        {
            if (item.Type != SongListItem.ItemType.Song) continue;
            string key = CoverCacheKey(item.Pack.id, item.Song.id);

            var jpgPath = Path.Combine(_songsPath, item.Pack.id, item.Song.id, "cover.jpg");
            var pngPath = Path.Combine(_songsPath, item.Pack.id, item.Song.id, "cover.png");
            var path = File.Exists(jpgPath) ? jpgPath : pngPath;

            if (!File.Exists(path))
                continue;

            var uri = new System.Uri(path).AbsoluteUri;
            using var request = UnityWebRequestTexture.GetTexture(uri);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                _coverCache[key] = DownloadHandlerTexture.GetContent(request);
        }

        _coverCacheReady = true;
        OnCoverCacheReady?.Invoke();
        DoFinishInit();
    }

    private static string CoverCacheKey(string packId, string songId)
    {
        return $"{packId}/{songId}";
    }

    private void DoFinishInit()
    {
        if (_finishedInit) return;
        if (_entries.Count == 0) return;
        _finishedInit = true;

        if (!string.IsNullOrEmpty(_restorePackId))
            RestoreSelection(_restorePackId, _restoreSongId, _restoreDifficultyId);
        else
            SnapToFirstSong();
    }

    public void SnapToFirstSong()
    {
        var songs = GetSongIndices();
        if (songs.Count > 0)
            SnapToEntry(songs[0]);
    }

    private void BuildList(List<SongListItem> items)
    {
        float y = 0;

        foreach (var item in items)
        {
            SongListEntry entry = new() { Data = item };

            if (item.Type == SongListItem.ItemType.Pack)
            {
                var packUI = Instantiate(packPrefab, content);
                packUI.Setup(item.Pack.name);
                entry.PackUI = packUI;
                entry.Rect = packUI.GetComponent<RectTransform>();
            }
            else
            {
                var songUI = Instantiate(songPrefab, content);
                songUI.Setup(item.Pack.id, item.Song.id, item.Song);
                entry.SongUI = songUI;
                entry.Rect = songUI.GetComponent<RectTransform>();
            }

            entry.Rect.pivot = new Vector2(1f, 0.5f);
            entry.Rect.anchorMin = new Vector2(0f, 1f);
            entry.Rect.anchorMax = new Vector2(1f, 1f);

            float h = item.Type == SongListItem.ItemType.Pack ? packHeight : songHeight;
            y -= h / 2f;
            entry.Rect.anchoredPosition = new Vector2(0f, y);
            y -= h / 2f + spacing;

            entry.BaseY = entry.Rect.anchoredPosition.y;
            _entries.Add(entry);
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, -y);
    }

    private void OnEnable()
    {
        if (MenuInputManager.Instance != null)
        {
            var input = MenuInputManager.Instance;
            input.OnUp += SelectPreviousSong;
            input.OnDown += SelectNextSong;
            input.OnLeft += SelectPreviousPack;
            input.OnRight += SelectNextPack;
            input.OnConfirm += StartGame;
            input.OnDifficultyPrev += HandleDifficultyPrev;
            input.OnDifficultyNext += HandleDifficultyNext;
        }
    }

    private void OnDisable()
    {
        if (MenuInputManager.Instance != null)
        {
            var input = MenuInputManager.Instance;
            input.OnUp -= SelectPreviousSong;
            input.OnDown -= SelectNextSong;
            input.OnLeft -= SelectPreviousPack;
            input.OnRight -= SelectNextPack;
            input.OnConfirm -= StartGame;
            input.OnDifficultyPrev -= HandleDifficultyPrev;
            input.OnDifficultyNext -= HandleDifficultyNext;
        }
    }

    private void Update()
    {
        UpdateScales();
        UpdateSnapping();
        UpdateStableSelection();
    }

    private void HandleDifficultyPrev()
    {
        if (songInfo != null)
            songInfo.SelectPreviousDifficulty();
    }

    private void HandleDifficultyNext()
    {
        if (songInfo != null)
            songInfo.SelectNextDifficulty();
    }

    private void UpdateStableSelection()
    {
        if (_previewStarted)
            return;

        if (_snappedIndex < 0)
            return;

        var entry = _entries[_snappedIndex];
        if (entry.Data.Type != SongListItem.ItemType.Song)
            return;

        _stableTimer += Time.deltaTime;
        if (_stableTimer < stableTimeMs / 1000f)
            return;

        _previewStarted = true;
        if (songPreview != null)
            songPreview.OnSongSelected(entry.Data.Song, entry.Data.Pack.id, _songsPath);
    }

    private void StartGame()
    {
        var entry = _entries[_snappedIndex];
        if (entry.Data.Type != SongListItem.ItemType.Song) return;

        StopPreview();
        int diffId = songInfo != null ? songInfo.SelectedDifficultyId : 0;
        OnRequestStartGame?.Invoke(entry.Data.Pack.id, entry.Data.Song.id, diffId);
    }

    public void RestoreSelection(string packId, string songId, int difficultyId)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            if (e.Data.Type == SongListItem.ItemType.Song &&
                e.Data.Pack.id == packId &&
                e.Data.Song.id == songId)
            {
                if (songInfo != null)
                    songInfo.SetPendingDifficulty(difficultyId);
                SnapToEntry(i);
                break;
            }
        }
    }

    private void SelectPreviousSong()
    {
        var songs = GetSongIndices();
        if (songs.Count == 0) return;

        int cur = songs.IndexOf(_snappedIndex);
        if (cur < 0) cur = 0;
        int next = (cur - 1 + songs.Count) % songs.Count;
        SnapToEntry(songs[next]);
    }

    private void SelectNextSong()
    {
        var songs = GetSongIndices();
        if (songs.Count == 0) return;

        int cur = songs.IndexOf(_snappedIndex);
        if (cur < 0) cur = 0;
        int next = (cur + 1) % songs.Count;
        SnapToEntry(songs[next]);
    }

    private void SelectPreviousPack()
    {
        var packs = GetPackIndices();
        if (packs.Count == 0) return;

        int cur = 0;
        for (int i = 0; i < packs.Count; i++)
            if (packs[i] <= _snappedIndex) cur = i;

        int next = (cur - 1 + packs.Count) % packs.Count;
        int songIdx = GetFirstSongIndexOfPack(packs[next]);
        if (songIdx >= 0) SnapToEntry(songIdx);
    }

    private void SelectNextPack()
    {
        var packs = GetPackIndices();
        if (packs.Count == 0) return;

        int cur = 0;
        for (int i = 0; i < packs.Count; i++)
            if (packs[i] <= _snappedIndex) cur = i;

        int next = (cur + 1) % packs.Count;
        int songIdx = GetFirstSongIndexOfPack(packs[next]);
        if (songIdx >= 0) SnapToEntry(songIdx);
    }

    private int GetFirstSongIndexOfPack(int packEntryIndex)
    {
        for (int i = packEntryIndex + 1; i < _entries.Count; i++)
        {
            if (_entries[i].Data.Type == SongListItem.ItemType.Pack)
                return -1;
            if (_entries[i].Data.Type == SongListItem.ItemType.Song)
                return i;
        }
        return -1;
    }

    private void SnapToEntry(int index)
    {
        _snappedIndex = index;
        RefreshPreview();

        var entry = _entries[index];
        if (entry.Data.Type == SongListItem.ItemType.Song && songInfo != null)
        {
            songInfo.DisplayMeta(entry.Data.Song, entry.Data.Pack.id);
            string key = CoverCacheKey(entry.Data.Pack.id, entry.Data.Song.id);
            if (_coverCache.TryGetValue(key, out var tex))
                songInfo.SetCoverTexture(tex);
            else
                songInfo.SetCoverTexture(null);
        }

        if (songPreview != null)
            songPreview.FadeOutAndStop();

        float viewportHalf = scrollRect.viewport != null
            ? scrollRect.viewport.rect.height / 2f : 0f;
        _snapTargetY = -viewportHalf - _entries[index].BaseY;
        _isSnapping = true;
    }

    private List<int> GetSongIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < _entries.Count; i++)
            if (_entries[i].Data.Type == SongListItem.ItemType.Song)
                list.Add(i);
        return list;
    }

    private List<int> GetPackIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].Data.Type == SongListItem.ItemType.Pack)
                list.Add(i);
        }
        return list;
    }

    private void UpdateSnapping()
    {
        if (!_isSnapping)
            return;

        var target = new Vector2(content.anchoredPosition.x, _snapTargetY);
        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, target,
            snapSpeed * Time.deltaTime);

        if (Mathf.Abs(content.anchoredPosition.y - _snapTargetY) < 0.5f)
        {
            content.anchoredPosition = target;
            _isSnapping = false;
        }
    }

    private void UpdateScales()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            float scale = (i == _snappedIndex) ? scaleMax : scaleMin;
            _entries[i].Rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}