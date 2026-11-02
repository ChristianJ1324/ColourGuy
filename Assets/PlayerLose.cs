using System;
using UnityEngine;

public class PlayerLose : MonoBehaviour
{
    private const float c_deathBoundary = -12f;

    public event Action PlayerDied;

    private Rigidbody2D m_playerBody;
    private Vector3 m_spawnPosition;
    private bool m_isDead;

    private void Awake()
    {
        m_playerBody = GetComponent<Rigidbody2D>();
        m_spawnPosition = transform.position;
    }

    private void Update()
    {
        if (!m_isDead && transform.position.x < c_deathBoundary)
        {
            m_isDead = true;
            PlayerDied?.Invoke();
        }
    }

    public void Respawn()
    {
        transform.position = m_spawnPosition;

        if (m_playerBody != null)
        {
            m_playerBody.linearVelocity = Vector2.zero;
            m_playerBody.angularVelocity = 0f;
        }

        m_isDead = false;
    }
}
