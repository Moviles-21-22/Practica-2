using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private TILE_COLOR tileColor;

    private bool empty = false;

    [SerializeField]
    private SpriteRenderer bridge;

    [SerializeField]
    private SpriteRenderer circle;

    [SerializeField]
    private SpriteRenderer star;

    [SerializeField]
    private SpriteRenderer bgColor;

    private Rect tileRect;

    //TODO : FALTAN MÁS COLORES
    public enum TILE_COLOR : int
    {
        RED, BLUE, ORANGE, YELLOW, GREEN, NONE
    };

    public void SetColor(int c, Color color)
    {
        if (c == -1)
        {
            tileColor = TILE_COLOR.NONE; 
            empty = true;
        }
        else
        {
            tileColor = (TILE_COLOR)c;
            circle.enabled = true;
            circle.color = color;
        }
        tileRect = new Rect(bgColor.sprite.rect);
    }

    public Rect GetRect()
    {
        return tileRect;
    }

    public bool EmptyTile()
    {
        return empty;
    }
    
    public void ActiveBgColor(bool status)
    {
        bgColor.enabled = status;
    }

    public void ActiveStar(bool status)
    {
        star.enabled = status;
    }

    public void SetActiveBridge(Sprite render,Color color)
    {
        bridge.enabled = true;
        bridge.sprite = render;
        bridge.color = color;
    }

    public void SetDeactiveBridge()
    {
        bridge.enabled = false;
    }

    public float GetWidth()
    {
        return bgColor.size.x;
    }

    public float GetHeight()
    {
        return bgColor.size.y;
    }

    public void touched()
    {
        if (empty)
        {
            bgColor.enabled = true;
        }
        else
        {
            //  Activar bridge
        }
    }
}
