using System.Collections.Generic;
using UnityEngine;

// End the run when the player hits the spikes.
public class SpikeWall : MonoBehaviour
{
    [SerializeField] private Camera m_mainCamera;
    private SpriteRenderer m_spriteRenderer;
    private PolygonCollider2D m_spikeCollider;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spikeCollider = GetComponent<PolygonCollider2D>();

        Sprite spikeSprite = m_spriteRenderer.sprite;
        m_spikeCollider.pathCount = spikeSprite.GetPhysicsShapeCount();
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < m_spikeCollider.pathCount; i++)
        {
            points.Clear();
            spikeSprite.GetPhysicsShape(i, points);
            m_spikeCollider.SetPath(i, points);
        }
    }

    private void LateUpdate()
    {
        float cameraDepth = Vector3.Dot(transform.position - m_mainCamera.transform.position, m_mainCamera.transform.forward);
        float screenLeft = m_mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDepth)).x;
        float wallLeft = m_spriteRenderer.bounds.min.x;
        float offset = screenLeft - wallLeft;
        if (Mathf.Abs(offset) > 0.001f)
        {
            transform.position += Vector3.right * offset;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.collider.GetComponent<PlayerLose>()?.Die();
    }
}
