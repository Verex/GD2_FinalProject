using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private LayerMask m_InitialRayLayerMask;
    [SerializeField] private ContactFilter2D m_ContactFilter;
    [SerializeField] private Collider2D[] m_ContactColliders;
    [SerializeField] private float m_MaxDistance = 1.0f;
    [SerializeField] private float m_Speed = 1.0f;

    private BoxCollider2D m_BoxCollider;

    public BoxCollider2D BoxCollider
    {
        get
        {
            if (m_BoxCollider == null)
            {
                m_BoxCollider = GetComponent<BoxCollider2D>();
            }

            return m_BoxCollider;
        }
    }

    void Start()
    {
        if (isServer)
        {
            Vector2 targetPosition = (transform.position + (transform.right * m_MaxDistance));
            float distance = m_MaxDistance;

            // Cast ray.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, m_MaxDistance, m_InitialRayLayerMask);

            // Check for collision.
            if (hit.collider != null)
            {
                // Update destination to hit.
                targetPosition = hit.point;
                distance = hit.distance;
            }

            // Start movement coroutine.
            StartCoroutine(Move(distance, targetPosition));
        }
    }

    private IEnumerator Move(float distance, Vector2 targetPosition)
    {
        float time = distance / m_Speed,
            elapsedTime = 0f;

        Vector2 startingPosition = transform.position;

        while (elapsedTime < time)
        {
            // Update position with interpolated position.
            transform.position = Vector2.Lerp(startingPosition, targetPosition, (elapsedTime / time));

            // Increment time.
            elapsedTime += Time.deltaTime;

            // Update collisions.
            UpdateCollisions();

            yield return null;
        }

        Debug.Log("Finished movement!");

        // Destroy projectile.
        NetworkServer.Destroy(gameObject);

        yield break;
    }

    private void UpdateCollisions()
    {
        if(!isServer)
        {
            return;
        }
        if (BoxCollider.OverlapCollider(m_ContactFilter, m_ContactColliders) != 0)
        {
            var collidedObject = m_ContactColliders[0].gameObject;
            if(collidedObject.tag == "Enemy")
            {
                collidedObject.GetComponent<Enemy>().TakeDamage(1);
            }
            NetworkServer.Destroy(gameObject);
        }
    }
}
