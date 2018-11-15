using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	public Transform Target;
	
	[SerializeField] private Vector3 m_TargetOffset = Vector3.zero;
	[SerializeField] private float m_SmoothTime = 0.3f;
	private Vector3 m_DampVelocity = Vector3.zero;

	public void LateUpdate()
	{
		if (Target != null)
		{
			// Calculate offset position from target.
			Vector3 targetPosition = Target.TransformPoint(m_TargetOffset);

			// Smooth damp to target.
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_DampVelocity, m_SmoothTime);
		}
	}
}
