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

    private int _selectedDifficultyId = -1;
    public int SelectedDifficultyId => _selectedDifficultyId;

    public void SetPendingDifficulty(int id)
    {
        _selectedDifficultyId = id;
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
    }

    public void SelectPreviousDifficulty()
    {
        difficultySelector?.SelectPrevious();
        _selectedDifficultyId = difficultySelector?.SelectedId ?? -1;
    }

    private IEnumerator LoadCover(string packId, string songId, string songsPath)
    {
        if (coverImage == null) yield break;

        var jpgPath = Path.Combine(songsPath, packId, songId, "cover.jpg");
        var pngPath = Path.Combine(songsPath, packId, songId, "cover.png");
        var path = File.Exists(jpgPath) ? jpgPath : pngPath;

        if (!File.Exists(path)) yield break;

        var uri = new System.Uri(path).AbsoluteUri;
        using var request = UnityWebRequestTexture.GetTexture(uri);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var tex = DownloadHandlerTexture.GetContent(request);
            coverImage.texture = tex;
        }
    }
}