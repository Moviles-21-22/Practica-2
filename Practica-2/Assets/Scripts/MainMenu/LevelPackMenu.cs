using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPackMenu : MonoBehaviour
{
    [Tooltip("Referencia al boton para elegir un nivel")]
    public Button button;

    [Tooltip("Referencia al texto del nombre del nivel")]
    public TextMeshProUGUI packName;

    [Tooltip("Referencia al texto que muestra los niveles completados")]
    public TextMeshProUGUI levels;

    [Tooltip("Referencia al RectTransform del objeto correspondiente")]
    public RectTransform levelRect;

    public void AddCallBack(int indexPack, int indexCat, AudioSource audioSource, AudioClip forward, MainMenuManager menu)
    {
        // El botón le comunica al mainMenu que se quiere cargar un nuevo paquete de niveles
        // de manera que sea el mainMenu quien se lo diga al GameManager
        button.onClick.AddListener(() => menu.LoadPackage(indexPack, indexCat));
        audioSource.PlayOneShot(forward);
    }
}
