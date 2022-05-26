using TMPro;
using UnityEngine;

public class GridPack : MonoBehaviour
{
    [SerializeField]
    public CellLevel[] boxs;

    [SerializeField]
    private TextMeshProUGUI title;

    /// <summary>
    /// Cambia el nombre del pack
    /// </summary>
    /// <param name="text">Nombre del pack</param>
    public void SetText(string text)
    {
        title.color = Color.white;
        title.text = text;
    }

    /// <summary>
    /// Devuelve una caja en concreto del grid
    /// </summary>
    /// <param name="index">Index de la caja del grid</param>
    /// <returns></returns>
    public CellLevel GetBox(int index)
    {
        return boxs[index];
    }

    /// <summary>
    /// Devuelve todas las cajas del grid
    /// </summary>
    /// <returns></returns>
    public CellLevel[] GetAllBoxes()
    {
        return boxs;
    }
}
