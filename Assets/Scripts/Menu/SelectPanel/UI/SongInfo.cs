using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongInfo : MonoBehaviour
{
    [Header("Meta")]
    [SerializeField] private TMP_Text songNameText;
    [SerializeField] private TMP_Text artistText;
    [SerializeField] private TMP_Text bpmText;

    [Header("Cover")]
    [SerializeField] private RawImage coverImage;

    [Header("Difficulties")]
    [SerializeField] private DifficultySelector difficultySelector;

    [Header("Record")]
    [SerializeField] private TMP_Text recordScoreText;
    [SerializeField] private TMP_Text recordComboText;

    private int _selectedDifficultyId = -1;
    public int SelectedDifficultyId => _selectedDifficultyId;

    public void SetPendingDifficulty(int id)
    {
        _selectedDifficultyId = id;
        UpdateRecordDisplay();
    }
    private SongData _pendingSong;
    private string _pendingPackId;
    private string _pendingSongsPath;

    public void DisplayMeta(SongData song, string packId, string songsPath)
    {
        if (songNameText != null) songNameText.text = song.name;
        if (artistText != null) artistText.text = song.artist;
        if (bpmText != null) bpmText.text = $"BPM: {song.bpm}";

        if (difficultySelector != null)
        {
            difficultySelector.Setup(song.difficulties, _selectedDifficultyId);
            _selectedDifficultyId = difficultySelector.SelectedId;
        }

        _pendingSong = song;
        _pendingPackId = packId;
        _pendingSongsPath = songsPath;

        UpdateRecordDisplay();
    }

    public void DisplayCover()
    {
        if (_pendingSong == null) return;
        StartCoroutine(LoadCover(_pendingPackId, _pendingSong.id, _pendingSongsPath));
    }

    public void SelectNextDifficulty()
    {
        difficultySelector?.SelectNext();
        _selectedDifficultyId = difficultySelector?.SelectedId ?? -1;
        UpdateRecordDisplay();
    }

    public void SelectPreviousDifficulty()
    {
        difficultySelector?.SelectPrevious();
        _selectedDifficultyId = difficultySelector?.SelectedId ?? -1;
        UpdateRecordDisplay();
    }

    private IEnumerator LoadCover(string packId, string songId, string songsPath)
    {
        if (coverImage == null) yield break;

        var jpgPath = Path.Combine(songsPath, packId, songId, "cover.jpg");
        var pngPath = Path.Combine(songsPath, packId, songId, "cover.png");
        var path = File.Exists(jpgPath) ? jpgPath : pngPath;

        if (!File.Exists(path)) yield break;

        var uri = new Uri(path).AbsoluteUri;
        using var request = UnityWebRequestTexture.GetTexture(uri);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var tex = DownloadHandlerTexture.GetContent(request);
            coverImage.texture = tex;
        }
    }

    private void UpdateRecordDisplay()
    {
        if (_pendingSong == null || _pendingPackId == null)
            return;

        if (_selectedDifficultyId < 0)
            return;

        if (RecordManager.Instance == null)
            return;

        var diff = RecordManager.Instance.GetDiffRecord(
            _pendingPackId, _pendingSong.id, _selectedDifficultyId);

        if (recordScoreText != null)
            recordScoreText.text = diff.highestScore.ToString("D6");

        if (recordComboText != null)
            recordComboText.text = diff.maxCombo.ToString();
    }
}