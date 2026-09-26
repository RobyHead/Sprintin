using TMPro;
using UnityEngine;

public class PackEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public void Setup(string packName)
    {
        if (nameText != null)
            nameText.text = packName;
    }
}