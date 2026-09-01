using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text artistText;
    [SerializeField] private TMP_Text bpmText;
    [SerializeField] private RawImage coverImage;

    public int PackIndex { get; private set; }
    public int SongIndex { get; private set; }
    public string SongId { get; private set; }

    public void Setup(int packIndex, int songIndex, SongData song, string packId,
        string songsPath)
    {
        PackIndex = packIndex;
        SongIndex = songIndex;
        SongId = song.id;

        if (nameText != null) nameText.text = song.name;
        if (artistText != null) artistText.text = song.artist;
        if (bpmText != null) bpmText.text = $"BPM {song.bpm:F0}";

        StartCoroutine(LoadCover(packId, song.id, songsPath));
    }

    private IEnumerator LoadCover(string packId, string songId, string songsPath)
    {
        if (coverImage == null)
            yield break;

        var jpgPath = Path.Combine(songsPath, packId, songId, "cover.jpg");
        var pngPath = Path.Combine(songsPath, packId, songId, "cover.png");
        var path = File.Exists(jpgPath) ? jpgPath : pngPath;

        if (!File.Exists(path))
            yield break;

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