using TMPro;
using UnityEngine;

public class PackEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public int PackIndex { get; private set; }

    public void Setup(int packIndex, string packName)
    {
        PackIndex = packIndex;
        if (nameText != null)
            nameText.text = packName;
    }
}