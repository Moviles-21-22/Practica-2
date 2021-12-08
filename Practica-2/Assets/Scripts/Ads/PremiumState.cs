using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PremiumState : MonoBehaviour
{
    void Start()
    {
        if (GameManager.instance.IsPremium())
        {
            this.gameObject.SetActive(false);
        }
    }

    public void UnlockPremium()
    {
        GameManager.instance.UnLockPremium();
        this.gameObject.SetActive(false);
    }
}
