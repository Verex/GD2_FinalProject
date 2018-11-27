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
    public Player Owner;

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

    public PlayerState StateUpdate(UserCmd cmd, PlayerState predictedState)
    {

        PlayerState newState = new PlayerState();
        Vector3 startingOrigin = predictedState.Origin;

        bool moveLeft = cmd.ActionIsPressed(PlayerInputSynchronization.IN_LEFT),
            moveRight = cmd.ActionIsPressed(PlayerInputSynchronization.IN_RIGHT);

        Vector3 startingPosition = predictedState.Origin;
        if (moveLeft ^ moveRight)
        {
            HorizontalMoveDirection = (moveLeft) ? -1 : 1;
        }
        else
        {
            HorizontalMoveDirection = 0;
        }

        newState.Origin = new Vector3(
            startingPosition.x + (m_BaseHorizontalDistance * HorizontalMoveDirection),
            startingPosition.y,
            startingPosition.z
        );

        return newState;
    }

    void Awake()
    {

    }

    void Update()
    {

    }

    private IEnumerator MoveForward()
    {
        // Wait for race to start.
        yield return new WaitUntil(() => RaceManager.Instance.CurrentState == RaceManager.RaceState.IN_PROGRESS);

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
        // Wait for race to start.
        yield return new WaitUntil(() => RaceManager.Instance.CurrentState == RaceManager.RaceState.IN_PROGRESS);

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
        //StartCoroutine(MoveForward());
        //StartCoroutine(MoveHorizontal());
    }
}
