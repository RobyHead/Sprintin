using System.IO;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }

    private RootRecord _root;
    private string _filePath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _filePath = Path.Combine(Application.persistentDataPath, "records.json");
        Load();
    }

    private void Load()
    {
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _root = JsonUtility.FromJson<RootRecord>(json);
        }
        else
        {
            _root = new RootRecord();
        }
    }

    private void Save()
    {
        var json = JsonUtility.ToJson(_root, true);
        File.WriteAllText(_filePath, json);
    }

    public void UpdateRecord(string packId, string songId, int diffId, int score, int maxCombo, bool fullCombo)
    {
        var diff = FindOrCreateDiff(packId, songId, diffId);

        if (score > diff.highestScore)
            diff.highestScore = score;
        if (maxCombo > diff.maxCombo)
            diff.maxCombo = maxCombo;
        if (fullCombo)
            diff.fullCombo = true;

        Save();
    }

    public DiffRecord GetDiffRecord(string packId, string songId, int diffId)
    {
        return FindOrCreateDiff(packId, songId, diffId);
    }

    public bool GetAllPerfect(string packId, string songId, int diffId)
    {
        var diff = FindOrCreateDiff(packId, songId, diffId);
        return diff.highestScore == 1000000;
    }

    private DiffRecord FindOrCreateDiff(string packId, string songId, int diffId)
    {
        var pack = FindOrCreatePack(packId);
        var song = FindOrCreateSong(pack, songId);

        foreach (var diff in song.diffs)
        {
            if (diff.id == diffId)
                return diff;
        }

        var newDiff = new DiffRecord { id = diffId };
        song.diffs.Add(newDiff);
        return newDiff;
    }

    private PackRecord FindOrCreatePack(string packId)
    {
        foreach (var pack in _root.packs)
        {
            if (pack.id == packId)
                return pack;
        }

        var newPack = new PackRecord { id = packId };
        _root.packs.Add(newPack);
        return newPack;
    }

    private SongRecord FindOrCreateSong(PackRecord pack, string songId)
    {
        foreach (var song in pack.songs)
        {
            if (song.id == songId)
                return song;
        }

        var newSong = new SongRecord { id = songId };
        pack.songs.Add(newSong);
        return newSong;
    }
}