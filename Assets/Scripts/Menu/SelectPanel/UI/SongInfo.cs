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

    private SongData _currentSong;
    private string _currentPackId;

    public void SetPendingDifficulty(int id)
    {
        _selectedDifficultyId = id;
        UpdateRecordDisplay();
    }

    public void DisplayMeta(SongData song, string packId)
    {
        _currentSong = song;
        _currentPackId = packId;

        if (songNameText != null) songNameText.text = song.name;
        if (artistText != null) artistText.text = song.artist;
        if (bpmText != null) bpmText.text = $"BPM: {song.bpm}";

        if (difficultySelector != null)
        {
            difficultySelector.Setup(song.difficulties, _selectedDifficultyId);
            _selectedDifficultyId = difficultySelector.SelectedId;
        }

        UpdateRecordDisplay();
    }

    public void SetCoverTexture(Texture2D texture)
    {
        if (coverImage != null)
            coverImage.texture = texture;
    }

    public void SelectNextDifficulty()
    {
        if (difficultySelector != null)
        {
            difficultySelector.SelectNext();
            _selectedDifficultyId = difficultySelector.SelectedId;
        }
        UpdateRecordDisplay();
    }

    public void SelectPreviousDifficulty()
    {
        if (difficultySelector != null)
        {
            difficultySelector.SelectPrevious();
            _selectedDifficultyId = difficultySelector.SelectedId;
        }
        UpdateRecordDisplay();
    }

    private void UpdateRecordDisplay()
    {
        if (_currentSong == null || _currentPackId == null)
            return;

        if (_selectedDifficultyId < 0)
            return;

        if (RecordManager.Instance == null)
            return;

        var diff = RecordManager.Instance.GetDiffRecord(
            _currentPackId, _currentSong.id, _selectedDifficultyId);

        if (recordScoreText != null)
            recordScoreText.text = diff.highestScore.ToString("D6");

        if (recordComboText != null)
            recordComboText.text = diff.maxCombo.ToString();
    }
}