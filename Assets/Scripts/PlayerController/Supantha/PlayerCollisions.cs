using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCollisions : MonoBehaviour
{
    public static event Action StartEvent;
    public static event Action EndEvent;

    private bool firstStart = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "StartFlag")
        {
            if (firstStart)
            {
                StartEvent?.Invoke();
                firstStart = false;
            }
        }
        else if(collision.tag == "EndFlag")
        {
            EndEvent?.Invoke();
        }
    }
}
