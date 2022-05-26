using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase scripteable usada para representar los diferentes packs/skins
/// </summary>
[System.Serializable]
public class ColorPack : ScriptableObject
{
    [Tooltip("Nombre del skin")]
    [SerializeField] public string colorPackName;
    [Tooltip("Lista con los colores de esta skin")]
    [SerializeField] public List<Color> colors;
    [Tooltip("Está desbloqueado")]
    [SerializeField] public bool unlocked;
    [Tooltip("Estado inicial del theme")]
    [SerializeField] public bool initState;

    /// <summary>
    /// Resetea la skin a un estado inicial
    /// </summary>
    public void Reset()
    {
        unlocked = initState;
    }
}
