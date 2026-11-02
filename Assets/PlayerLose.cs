using System;
using UnityEngine;

public class PlayerLose : MonoBehaviour
{

    public event Action PlayerDied;

    bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -12)
        {
            Debug.Log("Player has died");
            PlayerDied.Invoke();
            isDead = true;
        }
    }

    public void Respawn()
    {
        if (!isDead) return;
        // to be implemented -- reload level but continue with score?
    }
}
