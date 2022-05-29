using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryTitle : MonoBehaviour
{
    [Tooltip("Referencia al texto del titulo")]
    public TextMeshProUGUI titleText;

    [Tooltip("Referencia al sprite colorido de la categoria")]
    public Image titleSprite;

    [Tooltip("Referencia al RectTransform del título")]
    public RectTransform titleRect;
}
