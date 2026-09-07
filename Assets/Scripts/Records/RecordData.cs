using System;
using System.Collections.Generic;

[Serializable]
public class DiffRecord
{
    public int id;
    public int highestScore;
    public int maxCombo;
    public bool fullCombo;
}

[Serializable]
public class SongRecord
{
    public string id;
    public List<DiffRecord> diffs = new List<DiffRecord>();
}

[Serializable]
public class PackRecord
{
    public string id;
    public List<SongRecord> songs = new List<SongRecord>();
}

[Serializable]
public class RootRecord
{
    public string id = "root";
    public List<PackRecord> packs = new List<PackRecord>();
}