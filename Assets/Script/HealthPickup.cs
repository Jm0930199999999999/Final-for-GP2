using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
   public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(collision.gameObject.GetComponent<PlayerHealth>().playerHealth < 3)
            {
                collision.gameObject.GetComponent<PlayerHealth>().AddHealth();
                Destroy(gameObject);
            }

        }
    }
}
