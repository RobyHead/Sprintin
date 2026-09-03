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
        optionList.Open();
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
            if (string.IsNullOrEmpty(opt.PersistKey))
            {
                opt.LoadDefault();
                continue;
            }

            string saved = PlayerPrefs.GetString(opt.PersistKey, null);
            if (saved != null)
                opt.LoadFrom(saved);
            else
                opt.LoadDefault();
        }
    }

    private void SaveAll()
    {
        foreach (var opt in _options)
        {
            if (!string.IsNullOrEmpty(opt.PersistKey))
                PlayerPrefs.SetString(opt.PersistKey, opt.SaveData());
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

            opt.LoadDefault();
            PlayerPrefs.SetString(opt.PersistKey, opt.SaveData());
            anyMissing = true;
        }
        if (anyMissing)
            PlayerPrefs.Save();
    }
}