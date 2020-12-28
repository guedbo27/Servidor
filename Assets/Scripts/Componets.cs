using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Componets : MonoBehaviour
{
    public static Componets componets;
    public int isEnemy;

    private void Start()
    {
        int random = Random.Range(0, 100);
        if (random <= 70)
        {
            isEnemy = 0;
        }
    
        else
        {
            isEnemy = 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") && isEnemy == 0)
        {
            transform.gameObject.tag = "Enemy";
            ClientSend.CollisionEnemy(Client.instance.myId);
        }

        if (other.gameObject.CompareTag("Player") && isEnemy == 1)
        {
            transform.gameObject.tag = "Player";
            ClientSend.CollisionPlayer(Client.instance.myId);
        }
    }
}
