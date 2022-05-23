using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    //  Enum que representa el color del tile
    private TILE_COLOR tileColor = TILE_COLOR.NONE;
    //  Color del tile
    private Color color = Color.clear;
    //  index x del tile
    private int x;
    //  Index y del tile
    private int y;

    [Tooltip("Valor del alpha del color del fondo del tile")]
    [SerializeField]
    //  Alpha del backgroud
    private float backgroundAlpha = 0.5f;
    //Variable que define si el tile es hueco o no
    private bool empty = false;
    [Tooltip("Sprite tail del tile")]
    [SerializeField]
    private SpriteRenderer bridgeTail;

    [Tooltip("Sprite del puente del tile")]
    [SerializeField]
    private SpriteRenderer bridge;

    [Tooltip("Sprite del codo del tile")]
    [SerializeField]
    private SpriteRenderer elbow;

    [Tooltip("Sprite del circulo del tile")]
    [SerializeField]
    private SpriteRenderer circle;

    [Tooltip("Sprite de estrella del tile")]
    [SerializeField]
    private SpriteRenderer star;

    [Tooltip("Sprite del muro superior del tile")]
    [SerializeField]
    private SpriteRenderer wallUp;

    [Tooltip("Sprite del muro derecho del tile")]
    [SerializeField]
    private SpriteRenderer wallRight;

    [Tooltip("Sprite del muro inferior del tile")]
    [SerializeField]
    private SpriteRenderer wallDown;

    [Tooltip("Sprite del muro izquierdo del tile")]
    [SerializeField]
    private SpriteRenderer wallLeft;

    [Tooltip("Sprite del background del tile")]
    [SerializeField]
    private SpriteRenderer bgColor;

    [Tooltip("Sprite del marco del tile")]
    [SerializeField]
    private SpriteRenderer lines;

    [Tooltip("sistema de particulas del tile")]
    [SerializeField]
    private ParticleSystem particleSystem;

    //  Rect en coordenadas de cámara del tile
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

    /// <summary>
    /// Inicializa el tile
    /// </summary>
    /// <param name="c">enum del tile</param>
    /// <param name="_color">color del tile</param>
    /// <param name="_x">index x</param>
    /// <param name="_y">index y</param>
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

    /// <summary>
    /// Inicializa un tile vacío
    /// </summary>
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

    /// <summary>
    /// Devuelve true si el tile tiene el circulo activo
    /// </summary>
    /// <returns></returns>
    public bool CircleActive()
    {
        return circle.enabled;
    }

    /// <summary>
    /// Activa el color de fondo del tile
    /// </summary>
    /// <param name="status">status del backgroud</param>
    /// <param name="_color">color del tile</param>
    public void ActiveBgColor(bool status, Color _color)
    {
        color = _color;
        bgColor.color = new Color(color.r, color.g, color.b, backgroundAlpha);
        bgColor.enabled = status;
    }

    /// <summary>
    /// Determina si la estrella está activa en el flow
    /// </summary>
    /// <returns></returns>
    public bool IsStarActive()
    {
        return star.enabled;
    }

    /// <summary>
    /// Activa la estrella de un tile
    /// </summary>
    /// <param name="status">status de la estrella</param>
    public void ActiveStar(bool status)
    {
        //if (!status)
        //    return;

        //star.color = Color.white;
        //star.enabled = status;

        if (status)
        {
            star.enabled = true;
            star.color = Color.white;
        }
        else
            star.enabled = false;
    }


    //dir == 0 → muro encima
    //dir == 1 → muro a la derecha
    //dir == 2 → muro debajo
    //dir == 3 → muro a la izquierda
    /// <summary>
    /// Activa un muro en función de la dirección
    /// </summary>
    /// <param name="dir">Dirección del muro</param>
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

    /// <summary>
    /// Activa el tail del tile para que se dibuje en función
    /// de la dirección con el siguiente tille. 
    /// </summary>
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
            bridgeTail.transform.Rotate(Vector3.forward, -90/* * factor*/);
        }
        // der->izq
        else if (_dir.x == -1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, 90/* * factor*/);
        }
        // bot->top
        else if (_dir.y == -1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, 0.0f/*factor == 1.0f ? 0.0f : 180.0f*/);
        }
        // top->bot
        else if (_dir.y == 1.0f)
        {
            bridgeTail.transform.Rotate(Vector3.forward, 180.0f/*factor == 1.0f ? 180.0f : 0.0f*/);
        }
    }

    /// <summary>
    /// Activa un puente
    /// </summary>
    /// <param name="_color">Color del puente</param>
    public void ActiveBridge(Color _color)
    {
        bridgeTail.enabled = false;
        color = _color;
        bridge.enabled = true;
        bridge.color = color;
    }

    /// <summary>
    /// Activa un codo
    /// </summary>
    /// <param name="_color">Color tile</param>
    /// <param name="dir">Dirección del tile</param>
    /// <param name="next">Dirección siguiente al codo</param>
    public void ActiveElbow(Color _color, Vector2 dir, Vector2 next)
    {
        bridgeTail.enabled = false;
        elbow.enabled = true;
        elbow.color = _color;
        color = _color;
        elbow.transform.rotation = Quaternion.identity;

        if (dir.y == 1.0f) // Va para abajo 
        {
            if (next.x == 1.0f) // abajo->derecha
            {
                elbow.transform.Rotate(Vector3.forward, 180.0f);
            }
            else if (next.x == -1.0f) // abajo->izquierda
            {
                elbow.transform.Rotate(Vector3.forward, -90.0f);
            }
        }
        else if (dir.y == -1.0f) // Va para arriba
        {
            if (next.x == 1.0f) // arriba->derecha
            {
                elbow.transform.Rotate(Vector3.forward, 90.0f);
            }
            else if (next.x == -1.0f)// arriba->izquierda
            {
                elbow.transform.Rotate(Vector3.forward, 0.0f);
            }
        }
        else if (dir.x == 1.0f) //Va para derecha
        {
            if (next.y == 1.0f)// derecha->abajo
            {
                elbow.transform.Rotate(Vector3.forward, 0.0f);
            }
            else if (next.y == -1.0f)// derecha->arriba
            {
                elbow.transform.Rotate(Vector3.forward, -90);
            }
        }
        else if (dir.x == -1.0f) //Va para izquierda
        {
            if (next.y == 1.0f)// izquierda->abajo
            {
                elbow.transform.Rotate(Vector3.forward, 90);
            }
            else if (next.y == -1.0f)// izquierda->arriba
            {
                elbow.transform.Rotate(Vector3.forward, 180.0f);
            }
        }
    }

    /// <summary>
    /// Desactiva las lineas del tile
    /// </summary>
    public void DesactiveLines()
    {
        lines.enabled = false;
    }

    /// <summary>
    /// Desactiva los elementos del tile no incluido el circulo
    /// </summary>
    public void DesactiveAll()
    {
        bgColor.enabled = false;
        bridgeTail.enabled = false;
        bridge.enabled = false;
        elbow.enabled = false;
    }

    /// <summary>
    /// Limpia colores y desactiva los sprites del tile
    /// </summary>
    public void ClearTile()
    {
        if (!CircleActive())
        {
            tileColor = TILE_COLOR.NONE;
            color = Color.clear;
        }
        DesactiveAll();
    }

    /// <summary>
    /// Desactiva la cola del tile
    /// </summary>
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

    /// <summary>
    /// Cambio del index
    /// </summary>
    /// <param name="_x">Index x</param>
    public void SetX(int _x)
    {
        x = _x;
    }

    /// <summary>
    /// Cambio del index
    /// </summary>
    /// <param name="_x">Index y</param>
    public void SetY(int _y)
    {
        y = _y;
    }

    /// <summary>
    /// Cambia el color de un tile
    /// </summary>
    /// <param name="c">Color del tile</param>
    public void SetTileColor(int c)
    {
        tileColor = (TILE_COLOR)c;
    }

    /// <summary>
    /// Devuelve el index x del tile
    /// </summary>
    /// <returns>index x</returns>
    public int GetX()
    {
        return x;
    }

    /// <summary>
    /// Devuelve el index y del tile
    /// </summary>
    /// <returns>index y</returns>
    public int GetY()
    {
        return y;
    }

    /// <summary>
    /// Devuelve el rect del tile
    /// </summary>
    /// <returns></returns>
    public Rect GetLogicRect()
    {
        return logicRect;
    }

    /// <summary>
    /// Determina si el tile está vacío
    /// </summary>
    /// <returns></returns>
    public bool GetEmpty()
    {
        return empty;
    }

    /// <summary>
    /// Devuelve el color del tile
    /// </summary>
    /// <returns></returns>
    public Color GetColor()
    {
        return color;
    }

    /// <summary>
    /// Devuelve el color del tile
    /// </summary>
    /// <returns></returns>
    public int GetTileColor()
    {
        return (int)tileColor;
    }

    /// <summary>
    /// Devuelve el sprite de circulo del tile
    /// </summary>
    /// <returns></returns>
    public SpriteRenderer GetCircleRender()
    {
        return circle;
    }

    /// <summary>
    /// Primer contacto con un tile
    /// </summary>
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

    /// <summary>
    /// Reproduce las particulas de un tile
    /// </summary>
    public void PlayParticle()
    {
        particleSystem.Play();
    }
}