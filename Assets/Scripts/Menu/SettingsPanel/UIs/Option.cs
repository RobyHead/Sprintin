using System;
using UnityEngine;

public abstract class Option : MonoBehaviour
{
    [SerializeField] protected GameObject buttonsContainer;

    public abstract string PersistKey { get; }
    public abstract void Increase();
    public abstract void Decrease();
    public abstract void LoadDefault();
    public abstract void LoadFrom(string data);
    public abstract string SaveData();

    public event Action OnValueChanged;

    public virtual void SetSelected(bool selected)
    {
        if (buttonsContainer != null)
            buttonsContainer.SetActive(selected);
    }

    protected void NotifyValueChanged()
    {
        OnValueChanged?.Invoke();
    }
}