using System;
using UnityEngine;

public class PlayerLose : MonoBehaviour
{
    public event Action PlayerDied;

    [SerializeField] private Camera m_mainCamera;
    private Collider2D m_playerCollider;
    private Rigidbody2D m_playerBody;
    private Vector3 m_spawnPosition;
    private bool m_isDead;

    private void Awake()
    {
        m_playerCollider = GetComponent<Collider2D>();
        m_playerBody = GetComponent<Rigidbody2D>();
        m_spawnPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (m_isDead || Time.timeScale <= 0f || !m_playerBody.simulated)
        {
            return;
        }

        Bounds bounds = m_playerCollider.bounds;
        float cameraDepth = Vector3.Dot(transform.position - m_mainCamera.transform.position, m_mainCamera.transform.forward);
        Vector3 cameraLeft = m_mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDepth));
        Vector3 cameraRight = m_mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, cameraDepth));
        Vector3 cameraBottom = m_mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, cameraDepth));
        Vector3 cameraTop = m_mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, cameraDepth));

        // Use the camera edges so this works across different screen sizes.
        if (bounds.min.x <= cameraLeft.x || bounds.min.y <= cameraBottom.y ||
            bounds.max.x >= cameraRight.x || bounds.max.y >= cameraTop.y)
        {
            Die();
        }
    }

    public void Die()
    {
        if (m_isDead || Time.timeScale <= 0f || !m_playerBody.simulated)
        {
            return;
        }

        m_isDead = true;
        PlayerDied?.Invoke();
    }

    public void Respawn()
    {
        transform.position = m_spawnPosition;

        m_playerBody.position = new Vector2(m_spawnPosition.x, m_spawnPosition.y);
        m_playerBody.linearVelocity = Vector2.zero;
        m_playerBody.angularVelocity = 0f;

        Physics2D.SyncTransforms();
        m_isDead = false;
    }
}
