using TMPro;
using UnityEngine;

// Check Amaro

/// <summary>
/// Clase para representar una "caja" con todos los niveles de un pack
/// </summary>
public class GridPack : MonoBehaviour
{
    /// <summary>
    /// Array con las celdas
    /// </summary>
    [SerializeField]
    public CellLevel[] boxs;

    /// <summary>
    /// Texto del titulo
    /// </summary>
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
