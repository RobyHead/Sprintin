using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongMeta : MonoBehaviour
{
    [SerializeField] private Image coverImage;
    [SerializeField] private TMP_Text songNameText;
    [SerializeField] private TMP_Text artistText;
    [SerializeField] private TMP_Text difficultyText;

    [SerializeField] private Color[] difficultyColors =
    {
        new(0.4f, 0.9f, 0.4f),
        new(0.9f, 0.9f, 0.2f),
        new(0.9f, 0.3f, 0.3f),
        new(0.7f, 0.4f, 0.9f),
    };

    [SerializeField] private string[] difficultyAbbr = { "EZ", "NM", "HD", "RS" };

    private void Start()
    {
        LoadSongData();
        StartCoroutine(LoadCover());
    }

    private void LoadSongData()
    {
        var folder = Path.Combine(Application.streamingAssetsPath, "Songs", ChartManager.SongFolder);
        var songPath = Path.Combine(folder, "song.json");
        if (!File.Exists(songPath))
        {
            Debug.LogWarning($"SongMeta: song.json not found at {songPath}");
            return;
        }

        var json = File.ReadAllText(songPath);
        var song = JsonUtility.FromJson<SongData>(json);
        if (song == null) return;

        if (songNameText != null) songNameText.text = song.name;
        if (artistText != null) artistText.text = song.artist;

        int diffId = SceneTransitionManager.Instance.DifficultyId;
        if (difficultyText != null && song.difficulties != null)
        {
            foreach (var diff in song.difficulties)
            {
                if (diff.id == diffId)
                {
                    difficultyText.text = $"{difficultyAbbr[diffId]} {diff.value.ToString("F1")}";
                    difficultyText.color = diffId < difficultyColors.Length
                        ? difficultyColors[diffId]
                        : Color.white;
                    break;
                }
            }
        }
    }

    private IEnumerator LoadCover()
    {
        var folder = Path.Combine(Application.streamingAssetsPath, "Songs", ChartManager.SongFolder);

        var jpgPath = Path.Combine(folder, "cover.jpg");
        var pngPath = Path.Combine(folder, "cover.png");

        var coverPath = File.Exists(jpgPath) ? jpgPath
            : File.Exists(pngPath) ? pngPath
            : null;

        if (coverPath != null)
        {
            var uri = new System.Uri(coverPath).AbsoluteUri;
            using var request = UnityWebRequestTexture.GetTexture(uri);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                coverImage.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            else
            {
                Debug.LogWarning($"SongMeta: failed to load cover: {request.error}");
            }
        }
        else
        {
            Debug.LogWarning($"SongMeta: cover not found in {folder}");
        }

        ChartManager.CoverLoaded = true;
        ChartManager.TryRequestIntro();
    }
}