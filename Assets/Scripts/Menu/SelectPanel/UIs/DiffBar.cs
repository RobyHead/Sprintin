using UnityEngine;
using UnityEngine.UI;

public class DiffBar : MonoBehaviour
{
    [SerializeField] private RawImage barImage;
    [SerializeField] private Color[] colors;

    public void SetDifficulty(int id)
    {
        if (barImage == null) return;
        if (colors == null || id < 0 || id >= colors.Length) return;
        barImage.color = colors[id];
    }
}