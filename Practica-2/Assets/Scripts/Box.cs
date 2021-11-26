using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Box : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Text numText;

    private Color color;

    [SerializeField]
    private RawImage star;

    [SerializeField]
    private RawImage background;

    [SerializeField]
    private RawImage frame;

    public void SetLevelNum(int num)
    {
        numText.text = num.ToString();
    }

    public void ActiveStar()
    {
        star.enabled = true;
        frame.enabled = false;
    }

    public void SelectBox()
    {
        color = Color.white;
        background.enabled = true;
        frame.enabled = true;
        background.color = color;
        frame.color = color;
    }

    public void initBox(Color _color)
    {
        color = _color;
        background.color = color;
        frame.color = color;
    }

    public void SetCallBack(int level)
    {
        button.onClick.AddListener(() => GameManager.instance.LoadPackLevel(level));
    }
}
