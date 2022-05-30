using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Check Amaro

public class HudElement : MonoBehaviour
{
    [Tooltip("Diferentes sprites del elemento de HUD")]
    public Sprite[] elementSprites;

    [Tooltip("Referencia al componente Image del elemento del HUD")]
    public Image elementImage;

    [Tooltip("Referencia al componente Text del elemento del HUD")]
    public TextMeshProUGUI elementText;

    [Tooltip("Referencia al componente Button del elemento del HUD")]
    public Button elementButton;
}