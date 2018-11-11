using UnityEngine;

public class FCFollowCamera : MonoBehaviour
{
    public Transform target;

	#region Mono Lifecycle

	private void LateUpdate()
	{
        // Look at the target.
        transform.LookAt(target);
	}

	#endregion
}