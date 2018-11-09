using UnityEngine;

public class FCPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float maximumSpeed;

    [Header("Boost")]
    public FCPlayerBooster boosterLeft;
    public FCPlayerBooster boosterRight;

    private new Rigidbody rigidbody;

    [Header("Height")]
    public float targetHeight = 10f;
    private bool hasReachedTargetHeight = false;
    private float currentHeight;
    public float CurrentHeight
    {
        get
        {
            return currentHeight;
        }

        set
        {
            currentHeight = value;

            // If our current height reaches the target height, fire the event.
            if (currentHeight >= targetHeight &&
                (hasReachedTargetHeight == false))
            {
                hasReachedTargetHeight = true;

                if (OnPlayerReachedHeight != null)
                {
                    OnPlayerReachedHeight(this);
                }
            }
        }
    }

    public event System.Action<FCPlayer> OnPlayerReachedHeight;

	#region Mono Behaviour Lifecycle

	private void Awake()
	{
        rigidbody = GetComponent<Rigidbody>();
	}

	private void Update()
    {
        CurrentHeight = transform.position.y;
    }

	private void FixedUpdate()
    {
        LimitVelocity();
	}

	#endregion

    #region Maximum Velocity

    private void LimitVelocity()
    {
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maximumSpeed);
    }

    #endregion

    #region Boost Control

    public enum BoostId
    {
        Left,
        Right
    }

    public void SetBoosting(BoostId boosterId, bool active)
    {
        FCPlayerBooster booster = (boosterId == BoostId.Left) ? boosterLeft : boosterRight;
        booster.Boosting = active;
    }

    #endregion

    #region Reset Position

    public void ResetPosition()
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    #endregion
}
