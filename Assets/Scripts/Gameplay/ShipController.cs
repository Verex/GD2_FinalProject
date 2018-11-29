using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipController : NetworkBehaviour
{
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private float m_BaseAcceleration = 5.0f;
    [SerializeField] private float m_VerticalAcceleration = 1.5f;
    [SerializeField] private float m_MaxHorizontalVelocity = 8.0f;
    [SerializeField] private float m_MinimumVerticalVelocity = 3f;
    [SerializeField] private float m_MaxVerticalVelocity = 8f;
    [SerializeField] private float m_MaxVerticalBoostVelocity = 11f;
    [SerializeField] private float m_FrictionCoefficient = 0.3f;
    [SerializeField] private float m_HorizontalDecceleration = 0.1f;
    [SerializeField] private float m_BrakingDecceleration = 1.5f;
    [SerializeField] private float m_AcceleratorMaxAcceleration = 5f;
    private float m_AcceleratorAcceleration = 0f;
    [SyncVar] private bool m_RaceBegun = false;
    [SerializeField] private LayerMask m_CollisionLayer;
    private Vector3 m_CurrentAcceleration = Vector3.zero;
    private float m_NewSpeed = 0f;
    private float m_SpeedDrop = 0f;
    private float m_CurrentSpeed = 0f;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_CurrentPosition;
    private bool m_OverrideVelocity = false;
    private bool m_ControlThresholdHit = false;
    [SerializeField] private float maxHP = 3;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audiosource;
    [SerializeField] private AudioClip sound_explode;
    [SerializeField] private AudioClip sound_boost;
    [SerializeField] private AudioClip sound_hit;
    [SyncVar] private float currentHP = 1;

    public Vector3 Velocity
    {
        get
        {
            return m_CurrentVelocity;
        }
    }

    public Vector3 Position
    {
        get
        {
            return m_CurrentPosition;
        }
    }


    public int HorizontalMoveDirection = 0;
    public Player Owner;

    private BoxCollider2D m_BoxCollider;
    public Vector3 TargetPosition;
    private float m_MovementSharpness = 15f;

    private float m_CollisionInvincibility = 1f;
    private bool m_CollisionImmunity = false;
    private float m_LastMoveDirection = 0f;

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
        float savedY = velocity.y;
        m_CurrentSpeed = currentSpeed;
        float speedDrop = m_HorizontalDecceleration * m_FrictionCoefficient * dt;
        m_SpeedDrop = speedDrop;
        float newSpeed = currentSpeed - speedDrop;
        m_NewSpeed = newSpeed;
        if (newSpeed < 0.0f)
            newSpeed = 0.0f;
        if (currentSpeed > 0f)
            newSpeed /= currentSpeed;

        var newVelocity = velocity * newSpeed;
        newVelocity.y = savedY;

        return newVelocity;
    }

    void Start()
    {
        currentHP = maxHP;
    }

    void Awake()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
        RaceManager.Instance.OnRaceStateChanged.AddListener(
            newState =>
            {
                if (newState == RaceManager.RaceState.IN_PROGRESS)
                {
                    m_CurrentAcceleration.y = 5f;
                    m_RaceBegun = true;
                }
            }
        );
    }

    void Update()
    {
        if (isServer)
        {
            transform.position = TargetPosition;
        }
        if (isClient && !isServer)
        {
            Vector3 newPos = Vector3.Lerp(transform.position, TargetPosition, 1 - Mathf.Exp(-m_MovementSharpness * Time.deltaTime));
            if (!float.IsNaN(newPos.x) && !float.IsNaN(newPos.y) && !float.IsNaN(newPos.z))
            {
                transform.position = newPos;
            }
            else
            {
                Debug.Log("We almost assigned position to NaN.");
            }
        }
    }

    public PlayerState StateUpdate(UserCmd cmd, PlayerState predictedState, float dt)
    {

        PlayerState newState = new PlayerState(Vector3.zero, Vector3.zero);
        Vector3 startingOrigin = predictedState.Origin;
        Vector3 startingVelocity = predictedState.Velocity;
        if (m_OverrideVelocity == true)
        {
            m_OverrideVelocity = false;
            startingVelocity = m_CurrentVelocity;
        }

        bool moveLeft = cmd.ActionIsPressed(PlayerInputSynchronization.IN_LEFT),
            moveRight = cmd.ActionIsPressed(PlayerInputSynchronization.IN_RIGHT);

        bool accel = cmd.ActionIsPressed(PlayerInputSynchronization.IN_ACCELERATE),
            deccel = cmd.ActionIsPressed(PlayerInputSynchronization.IN_DECCELERATE);

        Vector3 startingPosition = predictedState.Origin;
        if (moveLeft ^ moveRight)
        {
            m_CurrentAcceleration.x = ((moveLeft) ? -1 : 1) * m_BaseAcceleration;
            var moveDirection = Mathf.Sign(m_CurrentAcceleration.x);
            if(moveDirection != m_LastMoveDirection && (m_LastMoveDirection < 0f || m_LastMoveDirection > 0f))
            {
                m_CurrentAcceleration.x *= 1.5f;
            }
            m_LastMoveDirection = Mathf.Sign(m_CurrentAcceleration.x);
        }
        else
        {
            m_CurrentAcceleration.x = 0;
        }
        if (m_CurrentAcceleration.x == 0)
        {
            startingVelocity = ApplyFriction(dt, startingVelocity);
        }

        if (m_ControlThresholdHit)
        {
            if (accel ^ deccel)
            {
                if (accel)
                {
                    m_AcceleratorAcceleration = Mathf.Clamp(m_AcceleratorAcceleration + (3 * dt), 0f, m_AcceleratorMaxAcceleration);
                    m_CurrentAcceleration.y = m_AcceleratorAcceleration;
                }
                else
                {
                    m_AcceleratorAcceleration = Mathf.Clamp(m_AcceleratorAcceleration - (3 * dt), 0f, m_AcceleratorMaxAcceleration);
                }
                if (deccel)
                {
                    m_AcceleratorAcceleration = 0f;
                    if (m_CurrentVelocity.y > m_MinimumVerticalVelocity)
                    {
                        m_CurrentAcceleration.y = -m_BrakingDecceleration;
                    }
                }
            }
        }
        else
        {
            if (startingVelocity.y > m_MinimumVerticalVelocity)
            {
                m_ControlThresholdHit = true;
            }
        }
        if (!m_RaceBegun)
        {
            m_CurrentAcceleration = Vector3.zero;
        }

        newState.Velocity = startingVelocity + m_CurrentAcceleration * dt;
        newState.Velocity.x = Mathf.Clamp(newState.Velocity.x, -m_MaxHorizontalVelocity, m_MaxHorizontalVelocity);
        newState.Velocity.y = Mathf.Clamp(newState.Velocity.y, (m_ControlThresholdHit) ? m_MinimumVerticalVelocity : 0f, m_MaxVerticalVelocity);
        newState.Origin = startingOrigin + newState.Velocity * dt;

        if (isServer)
        {
            //Is there a collider where we're trying to move
            var overlappingColliders = Physics2D.OverlapBoxAll(
                new Vector2(newState.Origin.x, newState.Origin.y),
                m_BoxCollider.size, m_CollisionLayer
            );
            foreach (var collider in overlappingColliders)
            {
                if (collider.gameObject != this.gameObject)
                {
                    var colliderOrigin = new Vector2(startingOrigin.x, startingOrigin.y);
                    var colliderDisplacement = (new Vector2(newState.Origin.x, newState.Origin.y) - colliderOrigin);
                    var colliderHit = Physics2D.Raycast(
                        colliderOrigin,
                        colliderDisplacement.normalized,
                        colliderDisplacement.magnitude,
                        m_CollisionLayer
                    );
                    if (collider.tag == "Ship")
                    {
                        //Do our bounce
                        var otherShip = collider.GetComponent<ShipController>();
                        Vector3 p1 = startingPosition;
                        Vector3 p2 = otherShip.Position;
                        Vector3 v1 = startingVelocity;
                        Vector3 v2 = otherShip.Velocity;

                        Vector3 collisionVelocity1 = v1 - (Vector3.Dot(v1 - v2, p1 - p2) / ((p1 - p2).magnitude * (p1 - p2).magnitude)) * (p1 - p2);
                        Vector3 collisionVelocity2 = v2 - (Vector3.Dot(v2 - v1, p2 - p1) / ((p2 - p1).magnitude * (p2 - p1).magnitude)) * (p2 - p1);

                        collisionVelocity1.y = v1.y;
                        collisionVelocity2.y = v2.y;
                        otherShip.OverrideNextVelocity(collisionVelocity2);

                        newState = predictedState;
                        newState.Velocity = collisionVelocity1;
                    }
                    if(!m_CollisionImmunity && (collider.tag == "Obstacle") || (collider.tag == "Enemy"))
                    {
                        audiosource.PlayOneShot(sound_hit, 0.7f);
                        TakeDamage(1);
                        newState = predictedState;
                        newState.Velocity.y *= 0.3f; //Slow down
                        StartCoroutine(CollisionImmunity());

                    }
                    if (!m_CollisionImmunity && collider.tag == "Speed")
                    {
                        audiosource.PlayOneShot(sound_boost, 0.7f);
                        newState = predictedState;
                        newState.Velocity.y *= 2f; //Speed up
                        StartCoroutine(DecaySpeedBoost(m_MaxVerticalVelocity, 0.5f));
                        m_MaxVerticalVelocity = m_MaxVerticalBoostVelocity;
                        StartCoroutine(CollisionImmunity());
                    }
                    if (collider.tag == "Border")
                    {
                        newState = predictedState;
                        newState.Velocity.x = 0f;
                        m_CurrentAcceleration.x = 0;
                    }
                    if (collider.tag == "FinishLine")
                    {
                        RaceManager.Instance.FinishRace();
                    }
                }
            }
        }

        newState.Velocity.x = Mathf.Clamp(newState.Velocity.x, -m_MaxHorizontalVelocity, m_MaxHorizontalVelocity);
        newState.Velocity.y = Mathf.Clamp(newState.Velocity.y, (m_ControlThresholdHit) ? m_MinimumVerticalVelocity : 0f, m_MaxVerticalVelocity);
        m_CurrentVelocity = newState.Velocity;
        m_CurrentPosition = newState.Origin;
        return newState;
    }

    private IEnumerator CollisionImmunity(float collisionTime = 0f)
    {
        if (!isServer)
        {
            yield return null;
        }
        m_CollisionImmunity = true;
        yield return new WaitForSeconds((collisionTime < 0f || collisionTime > 0f) ? collisionTime : m_CollisionInvincibility);
        m_CollisionImmunity = false;
        yield return null;
    }

    private IEnumerator DecaySpeedBoost(float previousMax, float timePeriod = 1f)
    {
        float elapsedTime = 0f;
        var deltaSpeed = (m_MaxVerticalBoostVelocity - previousMax) / timePeriod;
        while(elapsedTime < timePeriod)
        {
            m_MaxVerticalVelocity = Mathf.Clamp(m_MaxHorizontalVelocity - (deltaSpeed * Time.deltaTime), previousMax, m_MaxVerticalBoostVelocity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        m_MaxVerticalVelocity = previousMax;
        yield return null;
    }

    public void OverrideNextVelocity(Vector3 velocity)
    {
        if (!isServer)
        {
            return;
        }
        m_CurrentVelocity = velocity;
        m_OverrideVelocity = true;
    }

    public void TakeDamage(int amount)
    {
        //Add a check for “isServer” to the TakeDamage function, so that damage will only be applied on the Server.
        if (!isServer)
        {
            return;
        }

        currentHP -= amount;
        if (currentHP <= 0)
        {
            StartCoroutine(Death());
        }

    }

    private IEnumerator Death()
    {
        animator.Play("explosion");
        audiosource.PlayOneShot(sound_explode, 0.7f);
        OverrideNextVelocity(Vector3.zero);
        m_CurrentAcceleration = Vector3.zero;
        m_ControlThresholdHit = false;
        m_RaceBegun = false;
        m_CollisionImmunity = true;
        yield return new WaitForSeconds(0.7f);
        animator.Play("ship2", -1, 0f);
        m_RaceBegun = true;
        m_CurrentAcceleration.y = 5f;
        StartCoroutine(CollisionImmunity(2f));
        currentHP = maxHP;
        yield return null;
    }
}
