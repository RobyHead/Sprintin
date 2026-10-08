using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private RawImage coverImage;

    [Header("Difficulty Indicators")]
    [SerializeField] private RawImage[] diffIndicators;

    [Header("Indicator Colors")]
    [SerializeField] private Color noneColor = Color.clear;
    [SerializeField] private Color notPlayedColor = Color.gray;
    [SerializeField] private Color playedColor = Color.white;
    [SerializeField] private Color fullComboColor = Color.yellow;
    [SerializeField] private Color allPerfectColor = Color.cyan;

    public string PackId { get; private set; }
    public string SongId { get; private set; }

    public void Setup(string packId, string songId, string songName)
    {
        PackId = packId;
        SongId = songId;

        if (nameText != null) nameText.text = songName;
    }

    public void SetCoverTexture(Texture2D texture)
    {
        if (coverImage != null)
            coverImage.texture = texture;
    }

    public void SetDifficultyIndicators(List<DifficultyData> difficulties)
    {
        if (diffIndicators == null || diffIndicators.Length == 0)
            return;

        for (int i = 0; i < diffIndicators.Length; i++)
        {
            if (diffIndicators[i] == null)
                continue;

            DifficultyData difficulty = null;
            foreach (var d in difficulties)
            {
                if (d.id == i)
                {
                    difficulty = d;
                    break;
                }
            }

            if (difficulty == null)
            {
                diffIndicators[i].color = noneColor;
                continue;
            }

            DiffRecord diffRecord = null;
            if (RecordManager.Instance != null)
                diffRecord = RecordManager.Instance.GetDiffRecord(PackId, SongId, difficulty.id);

            if (diffRecord == null || diffRecord.highestScore == 0)
                diffIndicators[i].color = notPlayedColor;
            else if (diffRecord.highestScore >= 1000000)
                diffIndicators[i].color = allPerfectColor;
            else if (diffRecord.fullCombo)
                diffIndicators[i].color = fullComboColor;
            else
                diffIndicators[i].color = playedColor;
        }
    }
}