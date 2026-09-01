using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SongListManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SongList songList;

    public List<SongListItem> Items { get; private set; } = new();
    public string SongsPath { get; private set; }

    private void Awake()
    {
        SongsPath = Path.Combine(Application.streamingAssetsPath, "Songs");
        LoadPacks();
    }

    private void Start()
    {
        if (songList != null)
            songList.Initialize(Items, SongsPath);

        if (SceneTransition.HasPendingReturn)
        {
            songList.RestoreSelection(
                SceneTransition.PackId,
                SceneTransition.SongId,
                SceneTransition.DifficultyId);
            SceneTransition.HasPendingReturn = false;
        }
    }

    private void LoadPacks()
    {
        var packsPath = Path.Combine(SongsPath, "packs.json");
        if (!File.Exists(packsPath))
        {
            Debug.LogError($"packs.json not found at: {packsPath}");
            return;
        }

        var json = File.ReadAllText(packsPath);
        var root = JsonUtility.FromJson<SongListRoot>(json);

        if (root == null || root.packs == null)
        {
            Debug.LogError("Failed to parse packs.json");
            return;
        }

        for (int p = 0; p < root.packs.Count; p++)
        {
            var pack = root.packs[p];

            Items.Add(new SongListItem
            {
                Type = SongListItem.ItemType.Pack,
                PackIndex = p,
                Pack = pack
            });

            LoadSongs(pack, p);
        }
    }

    private void LoadSongs(PackData pack, int packIndex)
    {
        var songsPath = Path.Combine(SongsPath, pack.id, "songs.json");
        if (!File.Exists(songsPath))
        {
            Debug.LogWarning($"songs.json not found for pack: {pack.id}");
            return;
        }

        var json = File.ReadAllText(songsPath);
        var packData = JsonUtility.FromJson<SongListPack>(json);

        if (packData == null || packData.songs == null)
        {
            Debug.LogWarning($"Failed to parse songs.json for pack: {pack.id}");
            return;
        }

        for (int s = 0; s < packData.songs.Count; s++)
        {
            Items.Add(new SongListItem
            {
                Type = SongListItem.ItemType.Song,
                PackIndex = packIndex,
                SongIndex = s,
                Pack = pack,
                Song = packData.songs[s]
            });
        }
    }
}