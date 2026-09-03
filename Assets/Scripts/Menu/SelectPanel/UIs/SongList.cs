using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private readonly List<SongListEntry> _entries = new();
    private int _snappedIndex = -1;
    private float _snapTargetY;
    private bool _isSnapping;
    private float _holdTimer;
    private float _nextActionTime;
    private string _songsPath;
    private float _stableTimer;
    private int _lastNotifiedIndex = -1;

    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;
    }

    private bool _interactable = true;

    private class SongListEntry
    {
        public SongListItem Data;
        public RectTransform Rect;
        public PackEntry PackUI;
        public SongEntry SongUI;
        public float BaseY;
    }

    public void Initialize(List<SongListItem> items, string songsPath)
    {
        _songsPath = songsPath;
        scrollRect.enabled = false;
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        BuildList(items, songsPath);

        var songs = GetSongIndices();
        if (songs.Count > 0)
            SnapToEntry(songs[0]);
    }

    private void BuildList(List<SongListItem> items, string songsPath)
    {
        float y = 0;

        foreach (var item in items)
        {
            SongListEntry entry = new() { Data = item };

            if (item.Type == SongListItem.ItemType.Pack)
            {
                var packUI = Instantiate(packPrefab, content);
                packUI.Setup(item.PackIndex, item.Pack.name);
                entry.PackUI = packUI;
                entry.Rect = packUI.GetComponent<RectTransform>();
            }
            else
            {
                var songUI = Instantiate(songPrefab, content);
                songUI.Setup(item.PackIndex, item.SongIndex, item.Song,
                    item.Pack.id, songsPath);
                entry.SongUI = songUI;
                entry.Rect = songUI.GetComponent<RectTransform>();
            }

            entry.Rect.pivot = new Vector2(0.5f, 0.5f);
            entry.Rect.anchorMin = new Vector2(0.5f, 1f);
            entry.Rect.anchorMax = new Vector2(0.5f, 1f);

            float h = item.Type == SongListItem.ItemType.Pack ? packHeight : songHeight;
            y -= h / 2f;
            entry.Rect.anchoredPosition = new Vector2(0f, y);
            y -= h / 2f + spacing;

            entry.BaseY = entry.Rect.anchoredPosition.y;
            _entries.Add(entry);
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, -y);
    }

    private void Update()
    {
        if (!_interactable) return;

        _holdTimer += Time.deltaTime;

        if (Time.time >= _nextActionTime)
            HandleSongInput();

        HandleDifficultyInput();
        UpdateStableSelection();
        UpdateScales();
        UpdateSnapping();
    }

    private void UpdateStableSelection()
    {
        if (_snappedIndex < 0 || _snappedIndex == _lastNotifiedIndex)
            return;

        _stableTimer += Time.deltaTime;
        if (_stableTimer < 0.5f)
            return;

        var entry = _entries[_snappedIndex];
        if (entry.Data.Type == SongListItem.ItemType.Song && songInfo != null)
        {
            songInfo.DisplayCover();
            _lastNotifiedIndex = _snappedIndex;
        }
    }

    private void HandleSongInput()
    {
        var kb = Keyboard.current;
        if (kb == null) { _holdTimer = 0f; return; }

        bool any = false;

        if (kb.wKey.isPressed) { SelectPreviousSong(); any = true; }
        else if (kb.sKey.isPressed) { SelectNextSong(); any = true; }
        else if (kb.aKey.isPressed) { SelectPreviousPack(); any = true; }
        else if (kb.dKey.isPressed) { SelectNextPack(); any = true; }

        if (!any)
        {
            _holdTimer = 0f;
            return;
        }

        _nextActionTime = Time.time + (_holdTimer >= 0.5f ? 0.1f : 0.3f);
    }

    private void HandleDifficultyInput()
    {
        var kb = Keyboard.current;
        if (kb == null || songInfo == null) return;

        if (kb.qKey.wasPressedThisFrame)
            songInfo.SelectPreviousDifficulty();
        else if (kb.eKey.wasPressedThisFrame)
            songInfo.SelectNextDifficulty();
        else if (kb.enterKey.wasPressedThisFrame)
            StartGame();
    }

    private void StartGame()
    {
        var entry = _entries[_snappedIndex];
        if (entry.Data.Type != SongListItem.ItemType.Song) return;

        int diffId = songInfo != null ? songInfo.SelectedDifficultyId : 0;
        SceneTransition.GoToGame(entry.Data.Pack.id, entry.Data.Song.id, diffId);
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
        int packIdx = _entries[packEntryIndex].Data.PackIndex;
        for (int i = packEntryIndex + 1; i < _entries.Count; i++)
        {
            if (_entries[i].Data.Type == SongListItem.ItemType.Song &&
                _entries[i].Data.PackIndex == packIdx)
                return i;
        }
        return -1;
    }

    private void SnapToEntry(int index)
    {
        _snappedIndex = index;
        _stableTimer = 0f;
        _lastNotifiedIndex = -1;

        var entry = _entries[index];
        if (entry.Data.Type == SongListItem.ItemType.Song && songInfo != null)
            songInfo.DisplayMeta(entry.Data.Song, entry.Data.Pack.id, _songsPath);

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