using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TankNetworkHelper : MonoBehaviour
{
    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            this.enabled = false;
            return;
        }
        // game Object -> for the rest of the setup features
        GameObject.FindObjectOfType<GameManager>().AddTank(gameObject);
    }
}
