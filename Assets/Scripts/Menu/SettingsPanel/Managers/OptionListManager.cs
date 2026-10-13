using UnityEngine;

public class OptionListManager : MonoBehaviour
{
    [SerializeField] private OptionList optionList;

    private Option[] _options;

    private void Awake()
    {
        _options = GetComponentsInChildren<Option>(true);
        InitializeDefaults();
    }

    private void OnEnable()
    {
        LoadAll();
        optionList.Open(_options);
    }

    private void OnDisable()
    {
        optionList.Close();
        SaveAll();
    }

    private void LoadAll()
    {
        foreach (var opt in _options)
        {
            string saved = string.IsNullOrEmpty(opt.PersistKey) ? null : PlayerPrefs.GetString(opt.PersistKey, null);
            opt.Load(saved);
        }
    }

    private void SaveAll()
    {
        foreach (var opt in _options)
        {
            if (!string.IsNullOrEmpty(opt.PersistKey))
                PlayerPrefs.SetString(opt.PersistKey, opt.Save());
        }
        PlayerPrefs.Save();
    }

    private void InitializeDefaults()
    {
        bool anyMissing = false;
        foreach (var opt in _options)
        {
            if (string.IsNullOrEmpty(opt.PersistKey)) continue;
            if (PlayerPrefs.HasKey(opt.PersistKey)) continue;

            opt.Load();
            PlayerPrefs.SetString(opt.PersistKey, opt.Save());
            anyMissing = true;
        }
        if (anyMissing)
            PlayerPrefs.Save();
    }
}