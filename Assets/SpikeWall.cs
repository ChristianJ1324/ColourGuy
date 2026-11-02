using UnityEngine;

// End the run when the player hits the spikes.
public class SpikeWall : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.collider.GetComponent<PlayerLose>()?.Die();
    }
}
