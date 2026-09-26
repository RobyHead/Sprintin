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

        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.ConsumePendingReturn())
        {
            var manager = SceneTransitionManager.Instance;
            songList.RestoreSelection(
                manager.PackId, manager.SongId, manager.DifficultyId);
        }
    }

    private void LoadPacks()
    {
        var rootPath = Path.Combine(SongsPath, "root.json");
        if (!File.Exists(rootPath))
        {
            Debug.LogError($"root.json not found at: {rootPath}");
            return;
        }

        var json = File.ReadAllText(rootPath);
        var root = JsonUtility.FromJson<SongListRoot>(json);

        if (root == null || root.packs == null)
        {
            Debug.LogError("Failed to parse root.json");
            return;
        }

        for (int p = 0; p < root.packs.Count; p++)
        {
            var packId = root.packs[p];
            var pack = LoadPackData(packId);
            if (pack == null) continue;

            Items.Add(new SongListItem
            {
                Type = SongListItem.ItemType.Pack,
                Pack = pack
            });

            LoadSongs(pack);
        }
    }

    private PackData LoadPackData(string packId)
    {
        var packPath = Path.Combine(SongsPath, packId, "pack.json");
        if (!File.Exists(packPath))
        {
            Debug.LogWarning($"pack.json not found for pack: {packId}");
            return null;
        }

        var json = File.ReadAllText(packPath);
        var pack = JsonUtility.FromJson<PackData>(json);
        pack.id = packId;
        return pack;
    }

    private void LoadSongs(PackData pack)
    {
        if (pack.songs == null) return;

        foreach (var songId in pack.songs)
        {
            var song = LoadSongData(pack.id, songId);
            if (song == null) continue;

            Items.Add(new SongListItem
            {
                Type = SongListItem.ItemType.Song,
                Pack = pack,
                Song = song
            });
        }
    }

    private SongData LoadSongData(string packId, string songId)
    {
        var songPath = Path.Combine(SongsPath, packId, songId, "song.json");
        if (!File.Exists(songPath))
        {
            Debug.LogWarning($"song.json not found for: {packId}/{songId}");
            return null;
        }

        var json = File.ReadAllText(songPath);
        var song = JsonUtility.FromJson<SongData>(json);
        song.id = songId;
        return song;
    }
}