using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class RaceManager : NetworkBehaviour
{
    [System.Serializable] public class RaceStateChanged : UnityEvent<RaceState> { };

    public enum RaceState
    {
        WAITING,
        STARTING,
        IN_PROGRESS,
        FINISHED
    }

    [SerializeField] public RaceStateChanged OnRaceStateChanged;

    [SyncVar(hook = "OnRaceStateReceived")] public RaceState CurrentState;

    // Starting time of the race (before countdown).
    [SyncVar] public float StartingTime;

    [SerializeField] private float m_RaceStartDelay = 3.0f;

    private static RaceManager s_Instance;

    public static RaceManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = GameObject.FindObjectOfType(
                    typeof(RaceManager)
                ) as RaceManager; //Initialize our network handler
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

    public override void OnStartClient()
    {
        if (CurrentState == RaceState.STARTING)
        {
			// Invoke race state changed for transition.
			OnRaceStateChanged.Invoke(CurrentState);
        }
    }

    private void OnRaceStateReceived(RaceState raceState)
	{
		OnRaceStateChanged.Invoke(raceState);
	}
}
