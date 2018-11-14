using UnityEngine;

public class FCPlayerBooster : MonoBehaviour
{
    public float boostStrength;
    public Rigidbody targetRigidbody;
    public ParticleSystem boostParticles;

    private bool isBoosting;
    public bool Boosting
    {
        get
        {
            return isBoosting;
        }

        set
        {
            isBoosting = value;

            var particleEmmision = boostParticles.emission;
            particleEmmision.enabled = isBoosting;
        }
    }

    #region Mono Behaviour Lifecycle

    private void FixedUpdate()
    {
        ApplyBoostIfNecessary();
    }

    #endregion

    #region Boost

	private void ApplyBoostIfNecessary()
    {
        if (isBoosting)
        {
            Vector3 direction = transform.forward * -1f;
            Vector3 position = transform.position;
            targetRigidbody.AddForceAtPosition(direction * boostStrength * Time.fixedDeltaTime,
                                               position,
                                               ForceMode.Acceleration);
        }
    }

    #endregion
}
