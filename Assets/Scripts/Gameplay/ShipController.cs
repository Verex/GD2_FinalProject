using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipController : NetworkBehaviour
{
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private float m_BaseForwardSpeed = 1.0f;
    [SerializeField] private float m_BaseForwardDistance = 1.0f;
    [SerializeField] private float m_BaseHorizontalSpeed = 1.0f;
    [SerializeField] private float m_BaseHorizontalDistance = 1.0f;

    [SerializeField] private float m_BaseAcceleration = 5.0f;
    [SerializeField] private float m_MaxHorizontalVelocity = 3.0f;
    [SerializeField] private float m_FrictionCoefficient = 0.3f;
    [SerializeField] private float m_HorizontalDecceleration = 0.1f;
    [SerializeField] private LayerMask m_ShipLayer;
    private Vector3 m_CurrentAcceleration = Vector3.zero;
    private float m_NewSpeed = 0f;
    private float m_SpeedDrop = 0f;
    private float m_CurrentSpeed = 0f;
    private Vector3 m_CurrentVelocity;
    

    public int HorizontalMoveDirection = 0;
    public Player Owner;

    private BoxCollider2D m_BoxCollider;

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

    private Vector3 ApplyFriction(float dt, Vector3 velocity)
    {
        float currentSpeed = velocity.magnitude;
        m_CurrentSpeed = currentSpeed;
        float speedDrop = m_HorizontalDecceleration * m_FrictionCoefficient * dt;
        m_SpeedDrop = speedDrop;
        float newSpeed = currentSpeed - speedDrop;
        m_NewSpeed = newSpeed;
        if(newSpeed < 0.0f)
            newSpeed = 0.0f;
        if(currentSpeed > 0f)
            newSpeed /= currentSpeed;

        return velocity * newSpeed;
    }

    void Awake()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
    }

    public PlayerState StateUpdate(UserCmd cmd, PlayerState predictedState, float dt)
    {

        PlayerState newState = new PlayerState(Vector3.zero, Vector3.zero);
        Vector3 startingOrigin = predictedState.Origin;
        Vector3 startingVelocity = predictedState.Velocity;

        bool moveLeft = cmd.ActionIsPressed(PlayerInputSynchronization.IN_LEFT),
            moveRight = cmd.ActionIsPressed(PlayerInputSynchronization.IN_RIGHT);

        Vector3 startingPosition = predictedState.Origin;
        if (moveLeft ^ moveRight)
        {
            m_CurrentAcceleration.x = ((moveLeft) ? -1 : 1) * m_BaseAcceleration;
        }
        else
        {
            m_CurrentAcceleration.x = 0;
        }
        if(m_CurrentAcceleration.x == 0)
        {
            startingVelocity = ApplyFriction(dt, startingVelocity);
        }
        newState.Velocity = startingVelocity + m_CurrentAcceleration * dt;
        m_CurrentVelocity = newState.Velocity;
        newState.Velocity.x = Mathf.Clamp(newState.Velocity.x, -m_MaxHorizontalVelocity, m_MaxHorizontalVelocity);
        newState.Origin = startingOrigin + newState.Velocity * dt;

        if(isServer)
        {
            //Is there a collider where we're trying to move
            var overlappingColliders = Physics2D.OverlapBoxAll(
                new Vector2(newState.Origin.x, newState.Origin.y),
                m_BoxCollider.size, m_ShipLayer
            );
            foreach(var collider in overlappingColliders)
            {
                if(collider.gameObject != this.gameObject)
                {
                    var colliderOrigin = new Vector2(startingOrigin.x, startingOrigin.y);
                    var colliderDisplacement = (new Vector2(newState.Origin.x, newState.Origin.y) - colliderOrigin);
                    var colliderHit = Physics2D.Raycast(
                        colliderOrigin,
                        colliderDisplacement.normalized,
                        colliderDisplacement.magnitude,
                        m_ShipLayer 
                    );
                    newState = predictedState;
                    newState.Velocity = Vector3.zero;
                    m_CurrentAcceleration = Vector3.zero;
                }
            }
        }
        return newState;
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
