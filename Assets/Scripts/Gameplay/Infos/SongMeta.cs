using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongMeta : MonoBehaviour
{
    [SerializeField] private Image coverImage;
    [SerializeField] private TMP_Text titleText;

    private void Start()
    {
        titleText.text = ChartManager.SongName;
        StartCoroutine(LoadCover());
    }

    private IEnumerator LoadCover()
    {
        var folder = Path.Combine(Application.streamingAssetsPath, "Songs", ChartManager.SongFolder);

        var jpgPath = Path.Combine(folder, "cover.jpg");
        var pngPath = Path.Combine(folder, "cover.png");

        var coverPath = File.Exists(jpgPath) ? jpgPath
            : File.Exists(pngPath) ? pngPath
            : null;

        if (coverPath == null)
        {
            Debug.LogWarning($"SongMeta: cover not found in {folder}");
            yield break;
        }

        var uri = new System.Uri(coverPath).AbsoluteUri;
        using var request = UnityWebRequestTexture.GetTexture(uri);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"SongMeta: failed to load cover: {request.error}");
            yield break;
        }

        var texture = DownloadHandlerTexture.GetContent(request);
        coverImage.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}