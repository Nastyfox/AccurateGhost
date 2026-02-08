using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCollisions : MonoBehaviour
{
    public static event Action EndEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "EndFlag")
        {
            EndEvent?.Invoke();
        }
    }
}
