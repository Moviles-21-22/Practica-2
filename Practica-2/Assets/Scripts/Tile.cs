using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    private TILE_COLOR tileColor = TILE_COLOR.NONE;

    private Color color = Color.clear;

    private int x;
    private int y;

    [Tooltip("Valor del alpha del color del fondo del tile")]
    [SerializeField]
    private float backgroundAlpha = 0.5f;

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

    private Rect logicRect;

    private void OnEnable()
    {
        //var a = transform;
        //var b = transform.position;
        //var c = transform.localPosition;
        //var d = transform.rect.position;
        //worldPos = transform.TransformPoint(transform.rect.position);
        //logicRect = new Rect(worldPos.x, worldPos.y, transform.rect.width, transform.rect.height);
    }

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
        color = Color.clear;
        empty = true;
        lines.enabled = false;
    }

    /// <summary>
    /// Inicializa el rectangulo logico del tile, con las dimensiones 
    /// y coordenadas en pixeles
    /// </summary>
    /// <param name="x_">Posicion x del tile dentro del array de tiles. Para debug</param>
    /// <param name="y_">Posicion y del tile dentro del array de tiles. Para debug</param>
    public void InitLogicalRect(int x_, int y_)
    {
        // Tamaño del tile en pixeles
        Vector2 origin = Camera.main.WorldToScreenPoint(new Vector2(lines.bounds.min.x, lines.bounds.min.y));
        Vector2 extent = Camera.main.WorldToScreenPoint(new Vector2(lines.bounds.max.x, lines.bounds.max.y));
        Vector2 size = (extent - origin);

        logicRect = new Rect(origin.x, origin.y, size.x, size.y);
    }

    public bool CircleActive()
    {
        return circle.enabled;
    }

    //  Activa el color de fondo del tile
    public void ActiveBgColor(bool status, Color _color)
    {
        color = _color;
        bgColor.color = new Color(color.r, color.g, color.b, backgroundAlpha);
        bgColor.enabled = status;
    }

    //  Activa la estrella de un tile
    public void ActiveStar(bool status)
    {
        star.color = Color.white;
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
        float factor = circle.enabled ? -1.0f : 1.0f;
        bridgeTail.transform.rotation = Quaternion.identity;

        //izq->der
        if (_dir.x == 1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, -90 * factor);
        }
        // der->izq
        else if (_dir.x == -1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, 90 * factor);
        }
        // bot->top
        else if (_dir.y == -1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, factor == -1.0f ? 0.0f : 180.0f);
        }
        // top->bot
        else if (_dir.y == 1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, factor == -1.0f ? 180.0f : 0.0f);
        }
    }

    public void ActiveBridge(Vector2 _dir, Color _color)
    {
        bridgeTail.enabled = false;
        color = _color;
        bridge.enabled = true;
        bridge.color = color;
    }

    public void ActiveElbow(Color _color, Vector2 dir, Vector2 previous)
    {
        bridgeTail.enabled = false;
        elbow.enabled = true;
        elbow.color = _color;
        color = _color;
        elbow.transform.rotation = Quaternion.identity;

        if (dir.y < 0.0f) // Va para abajo 
        {
            if (previous.x > 1.0f) // izq->abajo
            {
                //print("Para abajo desde la izquierda");
            }
            else if (previous.x < 0.0f) // der->abajo
            {
                elbow.transform.Rotate(Vector3.forward, 90);
            }
        }
        else if (dir.y > 0.0f) // Va para arriba
        {
            if (previous.x == -1.0f) // der->arriba
            {
                elbow.transform.Rotate(Vector3.forward, 180);
            }
            else if (previous.x > 0.0f)// izq->arriba
            {
                elbow.transform.Rotate(Vector3.forward, -90);
            }
        }
        else if (dir.x > 0.0f) //Va a la derecha
        {
            if (previous.y < 0.0f)// arriba->derecha
            {
                elbow.transform.Rotate(Vector3.forward, 180);
            }
            else if (previous.y > 0.0f)// abajo->derecha
            {
                elbow.transform.Rotate(Vector3.forward, 90);
            }
        }
        else if (dir.x < 0.0f) //Va a la izquierda
        {
            if (previous.y > 0.0f)// abajo->izquierda
            {
                //print("Para izquierda desde abajo");
            }
            else if (previous.y < 0.0f)// arriba->izquierda
            {
                elbow.transform.Rotate(Vector3.forward, -90);
            }
        }
    }

    public void DesactiveLines()
    {
        lines.enabled = false;
    }
    public void DesactiveAll()
    {
        bgColor.enabled = false;
        bridgeTail.enabled = false;
        bridge.enabled = false;
        elbow.enabled = false;
    }

    public void ClearTile()
    {
        if (!CircleActive())
        {
            tileColor = TILE_COLOR.NONE;
            color = Color.clear;
        }
        DesactiveAll();
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

    public void SetLocalGraphicPos(float x, float y)
    {
        //transform.anchoredPosition = new Vector2(x, y);    
    }

    public void SetSize(float width, float height)
    {
        //transform.sizeDelta = new Vector2(width, height);
    }

    public void SetX(int _x)
    {
        x = _x;
    }

    public void SetY(int _y)
    {
        y = _y;
    }

    public void SetTileColor(int c)
    {
        tileColor = (TILE_COLOR)c;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public Rect GetLogicRect()
    {
        return logicRect;
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
        bgColor.color = new Color(color.r, color.g, color.b, backgroundAlpha);
    }

    /// <summary>
    /// Comprueba si hay un muro en la dirección dir
    /// </summary>
    public bool WallCollision(Vector2 _dir)
    {
        //izq->der
        if (_dir.x == 1.0f)
            if (wallRight.enabled)
                return true;
            else
                return false;
        // der->izq
        else if (_dir.x == -1.0f)
            if (wallLeft.enabled)
                return true;
            else
                return false;
        // bot->top
        else if (_dir.y == -1.0f)
            if (wallDown.enabled)
                return true;
            else
                return false;
        // top->bot
        else if (_dir.y == 1.0f)
            if (wallUp.enabled)
                return true;
            else
                return false;
        else
            return false;
    }
}