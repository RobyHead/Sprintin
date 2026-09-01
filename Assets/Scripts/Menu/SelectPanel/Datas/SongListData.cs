using System;
using System.Collections.Generic;

[Serializable]
public class SongListRoot
{
    public List<PackData> packs;
}

[Serializable]
public class PackData
{
    public string id;
    public string name;
}

[Serializable]
public class SongListPack
{
    public List<SongData> songs;
}

[Serializable]
public class SongData
{
    public string id;
    public string name;
    public string artist;
    public List<DifficultyData> difficulties;
    public float bpm;
    public int viewbegin;
    public int viewend;
}

[Serializable]
public class DifficultyData
{
    public int id;
    public float value;
    public string charter;
}

public class SongListItem
{
    public enum ItemType { Pack, Song }

    public ItemType Type;
    public int PackIndex;
    public int SongIndex;
    public PackData Pack;
    public SongData Song;
}