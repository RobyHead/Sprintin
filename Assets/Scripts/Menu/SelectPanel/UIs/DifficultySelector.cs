using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private GameObject[] diffOptions;
    [SerializeField] private DiffBar diffBar;

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
        else
            SelectPrevious();
    }

    public void SelectDifficulty(int id)
    {
        _selectedId = id;
        diffBar.SetDifficulty(id);
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