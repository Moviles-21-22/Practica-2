using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private TILE_COLOR tileColor;

    private Color color;

    private int x;
    private int y;

    //Variable que define si el tile es hueco o no
    private bool empty = false;

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
    private SpriteRenderer wallUp;
    [SerializeField]
    private SpriteRenderer wallRight;
    [SerializeField]
    private SpriteRenderer wallDown;
    [SerializeField]
    private SpriteRenderer wallLeft;

    [SerializeField]
    private SpriteRenderer bgColor;

    [SerializeField]
    private SpriteRenderer lines;

    private Rect tileRect;

    //Tienen que tener los mismos colores y en el mismo orden que colors de BoardMngr
    /*
     Color.red, Color.blue, Color.green,
                               Color.magenta, Color.cyan, Color.yellow,
                               Color.grey, Color.white,
                               new Color(251.0f, 112.0f, 0.0f),
                               new Color(115.0f, 7.0f, 155.0f),
                               new Color(171.0f, 40.0f, 40.0f),
                               new Color(147.0f, 120.0f, 55.0f)
     */
    public enum TILE_COLOR : int
    {
        RED, BLUE, GREEN, MAGENTA, CYAN, YELLOW, GREY, WHITE, ORANGE, PURPLE, BROWN, NONE   //Este enum no es necesario, se puede cambiar tileColor por un int
    };

    public void InitTile(int c, Color _color, int _x = 0, int _y = 0)
    {
        x = _x;
        y = _y;
        if (c == -1)
        {
            InitEmptyTile();
        }
        else
        {
            tileColor = (TILE_COLOR)c;
            circle.enabled = true;
            circle.color = _color;
            color = _color;
        }
    }

    public void InitEmptyTile()
    {
        tileColor = TILE_COLOR.NONE;
        color = Color.white;
        empty = true;
        lines.enabled = false;
    }

    public bool CircleActive()
    {
        return circle.enabled;
    }
    
    //  Activa el color de fondo del tile
    public void ActiveBgColor(bool status,Color _color)
    {
        color = _color;
        bgColor.color = color;
        bgColor.enabled = status;
    }

    //  Activa la estrella de un tile
    public void ActiveStar(bool status)
    {
        star.enabled = status;
    }

    //dir == 0 → muro encima
    //dir == 1 → muro a la derecha
    //dir == 2 → muro debajo
    //dir == 3 → muro a la izquierda
    public void ActiveWall(int dir)
    {
        if (dir == 0)
            wallUp.enabled = true;
        else if (dir == 1)
            wallRight.enabled = true;
        else if (dir == 2)
            wallDown.enabled = true;
        else if (dir == 3)
            wallLeft.enabled = true;
    }

    public void ActiveTail(Vector2 _dir, Color _color)
    {
        color = _color;
        bridgeTail.enabled = true;
        bridgeTail.color = color;
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
        else if (_dir.y == -1.0f)
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

    public void SetRect(float x, float y)
    {
        Vector2 worldPos = Camera.main.WorldToScreenPoint(new Vector2(x, y));
        tileRect = new Rect(worldPos.x,worldPos.y, bgColor.sprite.rect.width, bgColor.sprite.rect.height);
    }

    public void SetX(int _x)
    {
        x = _x;
    }

    public void SetY(int _y)
    {
        y = _y;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public Rect GetRect()
    {
        return tileRect;
    }

    public float GetWidth()
    {
        return bgColor.size.x;
    }

    public float GetHeight()
    {
        return bgColor.size.y;
    }

    public bool GetEmpty()
    {
        return empty;
    }

    public Color GetColor()
    {
        return color;
    }

    public int GetTileColor()
    {
        return (int)tileColor;
    }

    public SpriteRenderer GetCircleRender()
    {
        return circle;
    }

    public void Touched()
    {
        bgColor.enabled = true;
        bgColor.color = color;
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
}