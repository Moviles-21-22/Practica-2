using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridPack : MonoBehaviour
{
    [SerializeField]
    public Box[] boxs;

    [SerializeField]
    private Text title;

    public void SetText(string text)
    {
        title.color = Color.white;
        title.text = text;
    }

    public Box GetBox(int index)
    {
        return boxs[index];
    }

    public Box[] GetAllBoxes()
    {
        return boxs;
    }
}
