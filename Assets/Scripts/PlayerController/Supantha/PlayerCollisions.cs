using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCollisions : MonoBehaviour
{
    public static event Action StartEvent;
    public static event Action EndEvent;

    private bool firstStart = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
