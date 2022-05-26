using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitSceneManager : MonoBehaviour
{
    public void LoadScene(int scene)
    {
        GameManager.instance.LoadScene(scene);
    }
}
