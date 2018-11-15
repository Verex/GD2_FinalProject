using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RaceManager : NetworkBehaviour
{
	public enum RaceState
	{
		WAITING,
		STARTING,
		IN_PROGRESS,
		FINISHED
	}

	[SyncVar] public RaceState CurrentState = RaceState.WAITING;

	// Starting time of the race (before countdown).
	[SyncVar] public float StartingTime;

	[SerializeField] private float m_RaceStartDelay = 3.0f;

    private static NetworkHandler s_Instance;

    public static NetworkHandler Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = GameObject.FindObjectOfType(
                    typeof(NetworkHandler)
                ) as NetworkHandler; //Initialize our network handler
            }

            return s_Instance;
        }
    }

	[Server]
	public void StartRace()
	{
		// Set our starting race state.
		CurrentState = RaceState.STARTING;

		// Assign the starting time.
		StartingTime = Time.time;
	}
}
