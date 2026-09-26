using TMPro;
using UnityEngine;

public class SongEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public string PackId { get; private set; }
    public string SongId { get; private set; }

    public void Setup(string packId, string songId, SongData song)
    {
        PackId = packId;
        SongId = songId;

        if (nameText != null) nameText.text = song.name;
    }
}