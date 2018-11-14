using UnityEngine;

public class FCShadow : MonoBehaviour
{
    public Transform target;
    public float visibleDistanceSquared;
	
	private void Update()
    {
        // Scale our transform based upon the distance from the target.
        Vector3 targetPosition = target.transform.position;
        Vector3 distanceToTarget = targetPosition - transform.position;

        float squaredMagnitude = Vector3.SqrMagnitude(distanceToTarget);
        squaredMagnitude = Mathf.Clamp(squaredMagnitude, 0f, visibleDistanceSquared);

        float scalar = 1 - (squaredMagnitude / visibleDistanceSquared);
        transform.localScale = Vector3.one * scalar;

        // Update our position to be underneath the target.
        Vector3 position = transform.position;
        position.x = targetPosition.x;
        position.z = targetPosition.z;
        transform.position = position;
	}
}
