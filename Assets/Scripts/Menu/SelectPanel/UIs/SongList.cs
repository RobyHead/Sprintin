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
    [SerializeField] private float stableTime = 0.5f;

    [Header("Preview")]
    [SerializeField] private SongPreviewManager songPreview;

    private readonly List<SongListEntry> _entries = new();
    private int _snappedIndex = -1;
    private float _snapTargetY;
    private bool _isSnapping;
    private string _songsPath;
    private float _stableTimer;
    private bool _previewStarted;

    private enum NavKey { None, W, S, A, D }
    private NavKey _heldKey = NavKey.None;
    private float _holdStartTime;
    private float _lastRepeatTime;

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
        if (_interactable)
        {
            HandleSongInput();
            HandleDifficultyInput();
        }
        UpdateScales();
        UpdateSnapping();
        UpdateStableSelection();
    }

    private void UpdateStableSelection()
    {
        if (_snappedIndex < 0)
            return;

        var entry = _entries[_snappedIndex];
        if (entry.Data.Type != SongListItem.ItemType.Song)
            return;

        _stableTimer += Time.deltaTime;
        if (_stableTimer < stableTime)
            return;

        if (_previewStarted)
            return;

        _previewStarted = true;
        if (songPreview != null)
            songPreview.OnSongSelected(entry.Data.Song, entry.Data.Pack.id, _songsPath);
    }

    private void HandleSongInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (_heldKey != NavKey.None)
        {
            if (!IsNavKeyPressed(kb, _heldKey))
            {
                _heldKey = NavKey.None;
                return;
            }

            float holdDuration = Time.time - _holdStartTime;
            float interval = holdDuration < 0.5f ? 0.3f : 0.1f;
            if (Time.time - _lastRepeatTime >= interval)
            {
                _lastRepeatTime = Time.time;
                DoNavAction(_heldKey);
            }
            return;
        }

        NavKey pressed = NavKey.None;
        if (kb.wKey.wasPressedThisFrame) pressed = NavKey.W;
        else if (kb.sKey.wasPressedThisFrame) pressed = NavKey.S;
        else if (kb.aKey.wasPressedThisFrame) pressed = NavKey.A;
        else if (kb.dKey.wasPressedThisFrame) pressed = NavKey.D;

        if (pressed == NavKey.None) return;

        _heldKey = pressed;
        _holdStartTime = Time.time;
        _lastRepeatTime = Time.time;
        DoNavAction(pressed);
    }

    private bool IsNavKeyPressed(Keyboard kb, NavKey key)
    {
        return key switch
        {
            NavKey.W => kb.wKey.isPressed,
            NavKey.S => kb.sKey.isPressed,
            NavKey.A => kb.aKey.isPressed,
            NavKey.D => kb.dKey.isPressed,
            _ => false
        };
    }

    private void DoNavAction(NavKey key)
    {
        switch (key)
        {
            case NavKey.W: SelectPreviousSong(); break;
            case NavKey.S: SelectNextSong(); break;
            case NavKey.A: SelectPreviousPack(); break;
            case NavKey.D: SelectNextPack(); break;
        }
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
        _previewStarted = false;

        var entry = _entries[index];
        if (entry.Data.Type == SongListItem.ItemType.Song && songInfo != null)
        {
            songInfo.DisplayMeta(entry.Data.Song, entry.Data.Pack.id, _songsPath);
            songInfo.DisplayCover();
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