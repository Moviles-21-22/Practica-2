using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="levelpack",menuName ="Flow/level pack",order = 1)]
public class LevelPack : ScriptableObject
{
    [Tooltip("fichero del lote")]
    public TextAsset txt;
}


