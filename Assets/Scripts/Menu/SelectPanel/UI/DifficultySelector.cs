using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private GameObject[] diffOptions;
    [SerializeField] private float selectedWidth = 100f;
    [SerializeField] private float normalWidth = 90f;
    [SerializeField] private DifficultyBackground difficultyBackground;

    private List<DifficultyData> _difficulties;
    private int _selectedId = -1;

    public int SelectedId => _selectedId;

    public void Setup(List<DifficultyData> difficulties, int preferredId = -1)
    {
        _difficulties = difficulties;

        for (int i = 0; i < diffOptions.Length; i++)
        {
            if (diffOptions[i] == null) continue;

            var data = difficulties.Find(d => d.id == i);
            if (data != null)
            {
                diffOptions[i].SetActive(true);
                var text = diffOptions[i].GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = data.value.ToString("F1");
            }
            else
            {
                diffOptions[i].SetActive(false);
            }
        }

        _selectedId = preferredId;
        if (_difficulties.Exists(d => d.id == _selectedId))
            SelectDifficulty(_selectedId);
        else {
            _selectedId = -1;
            SelectNext();
        }
    }

    public void SelectDifficulty(int id)
    {
        _selectedId = id;
        difficultyBackground.SetDifficulty(id);

        for (int i = 0; i < diffOptions.Length; i++)
        {
            if (diffOptions[i] == null) continue;

            var rect = diffOptions[i].GetComponent<RectTransform>();
            if (rect == null) continue;

            float targetWidth = (i == id) ? selectedWidth : normalWidth;
            var size = rect.sizeDelta;
            size.x = targetWidth;
            rect.sizeDelta = size;
        }
    }

    public void SelectNext()
    {
        if (_difficulties == null || _difficulties.Count == 0) return;

        for (int i = 0; i < 4; i++)
        {
            int next = (_selectedId + 1) % 4;
            _selectedId = next;
            if (_difficulties.Exists(d => d.id == next))
            {
                SelectDifficulty(next);
                return;
            }
        }
    }

    public void SelectPrevious()
    {
        if (_difficulties == null || _difficulties.Count == 0) return;

        for (int i = 0; i < 4; i++)
        {
            int next = (_selectedId - 1 + 4) % 4;
            _selectedId = next;
            if (_difficulties.Exists(d => d.id == next))
            {
                SelectDifficulty(next);
                return;
            }
        }
    }
}