using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PremiumState : MonoBehaviour
{
    /// <summary>
    /// Activa o desactiva el marco
    /// </summary>
    void Start()
    {
        if (GameManager.instance.IsPremium())
        {
            this.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Desbloquea el premiun y desactiva el marco
    /// </summary>
    public void UnlockPremium()
    {
        GameManager.instance.UnLockPremium();
        this.gameObject.SetActive(false);
    }
}
