using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryTitle : MonoBehaviour
{
    [Tooltip("Referencia al texto del titulo")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Tooltip("Referencia al sprite colorido de la categoria")]
    [SerializeField] private Image titleSprite;

    [Tooltip("Referencia al RectTransform del título")]
    [SerializeField] private RectTransform titleRect;

    public void SetText(string newText)
    {
        titleText.text = newText;
    }

    public void SetCategoryColor(Color newColor) 
    {
        titleSprite.color = newColor;
    }

    public void SetSizeText(float tam)
    {
        titleText.fontSizeMax = tam;
    }

    public Vector2 GetSizeDelta() 
    {
        return titleRect.sizeDelta;
    }

    public void SetSizeDelta(Vector2 newDelta) 
    {
        titleRect.sizeDelta = newDelta;
    }

    public void SetPos(Vector2 newPos) 
    {
        titleRect.localPosition = newPos;
    }
}
