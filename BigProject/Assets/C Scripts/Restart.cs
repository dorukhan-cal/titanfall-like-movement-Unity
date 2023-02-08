using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restart : MonoBehaviour
{
    public Transform respawnTarget;
    public GameObject thePlayer;
    
    void OnTriggerEnter(Collider other)
    {
        thePlayer.transform.position = respawnTarget.transform.position;
    }
}
