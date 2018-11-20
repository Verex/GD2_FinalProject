using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipController : NetworkBehaviour
{
    [SerializeField] private float m_BaseForwardSpeed = 1.0f;
    [SerializeField] private float m_BaseForwardDistance = 1.0f;
    [SerializeField] private float m_BaseHorizontalSpeed = 1.0f;
    [SerializeField] private float m_BaseHorizontalDistance = 1.0f;

    public int HorizontalMoveDirection = 0;

    [TargetRpc]
    public void TargetSetupShip(NetworkConnection target)
    {
        // Get player camera component.
        PlayerCamera camera = Camera.main.GetComponent<PlayerCamera>();

        // Assign camera target to ship.
        if (camera != null)
        {
            camera.Target = transform;
        }

        // Possess ship if not server.
        if (!isServer)
        {
            // MIGHT BE HACK-HACK: Get player component reference....
            Player playerComponent = NetworkHandler.Instance.client.connection.playerControllers[0].gameObject.GetComponent<Player>();

            // Possess the ship.
            playerComponent.Possess(this);
        }
    }

    private IEnumerator MoveForward()
    {
        while (true)
        {

            float time = m_BaseForwardDistance / m_BaseForwardSpeed,
                elapsedTime = 0f,
                startingY = transform.position.y,
                targetY = startingY + m_BaseForwardDistance;

            while (elapsedTime < time)
            {
                float yComponent = Mathf.Lerp(startingY, targetY, (elapsedTime / time));

                transform.position = new Vector3(transform.position.x, yComponent, transform.position.z);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            yield return null;
        }
    }

    private IEnumerator MoveHorizontal()
    {
        while (true)
        {
            if (HorizontalMoveDirection != 0)
            {
                float time = m_BaseHorizontalDistance / m_BaseHorizontalSpeed,
                    elapsedTime = 0f,
                    startingX = transform.position.x,
                    targetX = startingX + (m_BaseHorizontalDistance * HorizontalMoveDirection);

                int currentMoveDirection = HorizontalMoveDirection;

                while (elapsedTime < time && currentMoveDirection == HorizontalMoveDirection)
                {
                    float xComponent = Mathf.Lerp(startingX, targetX, (elapsedTime / time));

                    transform.position = new Vector3(xComponent, transform.position.y, transform.position.z);

                    elapsedTime += Time.deltaTime;

                    yield return null;
                }
            }

            yield return null;
        }
    }

    void Start()
    {
        StartCoroutine(MoveForward());
        StartCoroutine(MoveHorizontal());
    }
}
