using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase scripteable usada para representar los diferentes packs/skins
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "skinpack", menuName = "Flow/color", order = 1)]
public class ColorPack : ScriptableObject
{
    [Tooltip("Nombre del skin")]
    [SerializeField] public string colorPackName;
    [Tooltip("Lista con los colores de esta skin")]
    [SerializeField] public List<Color> colors;
    [Tooltip("Está desbloqueado")]
    [SerializeField] public bool active;
    [Tooltip("Estado inicial del theme")]
    [SerializeField] public bool status;

    /// <summary>
    /// Resetea la skin a un estado inicial
    /// </summary>
    public void Reset()
    {
        active = status;
    }
}
