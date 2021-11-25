using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private TILE_COLOR tileColor;

    private Color color;

    public bool empty = true;

    [SerializeField]
    private SpriteRenderer bridgeTail;

    [SerializeField]
    private SpriteRenderer bridge;

    [SerializeField]
    private SpriteRenderer elbow;

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

    public void InitTile(int c, Color _color)
    {
        if (c == -1)
        {
            tileColor = TILE_COLOR.NONE;
            color = Color.white;
            empty = true;
        }
        else
        {
            tileColor = (TILE_COLOR)c;
            circle.enabled = true;
            circle.color = _color;
            color = _color;
        }
    }

    public void SetRect(float x, float y)
    {
        Vector2 worldPos = Camera.main.WorldToScreenPoint(new Vector2(x, y));
        tileRect = new Rect(worldPos.x,worldPos.y, bgColor.sprite.rect.width, bgColor.sprite.rect.height);
    }

    public Rect GetRect()
    {
        return tileRect;
    }

    public bool CircleActive()
    {
        return circle.enabled;
    }
    
    public void ActiveBgColor(bool status)
    {
        bgColor.enabled = status;
    }

    public void ActiveStar(bool status)
    {
        star.enabled = status;
    }

    public void ActiveTail(Vector2 _dir, Color _color)
    {
        color = _color;
        bridgeTail.enabled = true;
        bridgeTail.color = color;
        bgColor.enabled = true;
        bgColor.color = color;

        bridgeTail.flipY = false;
        float factor = circle.enabled ? -1.0f : 1.0f;
        bridgeTail.transform.rotation = Quaternion.identity;

        if (_dir.x == 1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, -90 * factor);
        }
        else if (_dir.x == -1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, 90 * factor);
        }
        else if (_dir.y == 1.0f)
        {

            bridgeTail.flipY = true;
        }
    }

    public void ActiveBridge(Vector2 _dir, Color _color)
    {
        bridgeTail.enabled = false;
        color = _color;
        bridge.enabled = true;
        bridge.color = color;

        //bridge.transform.rotation = Quaternion.identity;
        bridge.transform.rotation = bridgeTail.transform.rotation;
        //if (_dir.x == 1.0f || _dir.x == 0.0f)
        //{
        //    bridge.transform.Rotate(Vector3.forward, 90);
        //}
    }

    public float GetWidth()
    {
        return bgColor.size.x;
    }

    public float GetHeight()
    {
        return bgColor.size.y;
    }

    public void Touched()
    {
        bgColor.enabled = true;
        bgColor.color = color;
    }

    public void ActiveElbow(Color _color ,Vector2 dir, Vector2 previous)
    {
        bridgeTail.enabled = false;
        elbow.enabled = true;
        elbow.color = _color;
        color = _color;
        elbow.transform.rotation = Quaternion.identity;
        elbow.flipX = false;
        elbow.flipY = false;

        if (dir.y < 0.0f) // Va para abajo 
        {
            if (previous.x > 1.0f) // Viene de un movimiento a la derecha
            {
                print("Para abajo desde la derecha");
            }
            else if (previous.x < 0.0f)// Viene de un movimiento de la izquierda
            {
                print("Para abajo desde la izquierda");
                elbow.flipX = true;
            }
        }
        else if (dir.y > 0.0f) // Va para arriba
        {
            if (previous.x == -1.0f) // Viene de un movimiento a la izquierda
            {
                print("Para arriba desde la izquierda");
                elbow.flipX = true;
                elbow.flipY = true;
            }
            else if (previous.x > 0.0f)// Viene de un movimiento a la derecha
            {
                elbow.flipY = true;
                print("Para arriba desde la derecha");
            }
        }
        else if (dir.x > 0.0f) //Va a la derecha
        {
            if (previous.y < 0.0f)// Viene de abajo
            {
                print("Para derecha desde arriba");
                elbow.flipX = true;
                elbow.flipY = true;
            }
            else if (previous.y > 0.0f)// Viene de arriba
            {
                print("Para derecha desde abajo");
                elbow.flipX = true;
            }
        }
        else if (dir.x < 0.0f) //Va a la izquierda
        {
            if (previous.y > 0.0f)// Viene de abajo
            {
                print("Para izquierda desde abajo");
            }
            else if (previous.y < 0.0f)// Viene de arriba
            {
                print("Para izquierda desde arriba");
                elbow.flipY = true;
            }
        }
    }

    public Color GetColor()
    {
        return color;
    }



    public void RemoveTail()
    {
        if (bridgeTail.enabled && !circle.enabled)
        {
            bridgeTail.enabled = false;
            bridge.enabled = true;
            bridge.color = bridgeTail.color;
            bridge.transform.rotation = bridgeTail.transform.rotation;
        }
    }

    public SpriteRenderer GetCircleRender()
    {
        return circle;
    }
}
