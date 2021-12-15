using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Reset()
    {
        active = status;
    }
}
