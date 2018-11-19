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
    public float StartingTime;

    [SerializeField] public float RaceStartDelay = 3.0f;

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

    private void HandleRaceState(RaceState newState)
    {
        switch (newState)
        {
            case RaceState.STARTING:
                // Assign the starting time.
                StartingTime = Time.time;

                break;
            case RaceState.IN_PROGRESS:

                break;

        }
    }

    public override void OnStartClient()
    {
        HandleRaceState(CurrentState);
    }

    private void OnRaceStateReceived(RaceState raceState)
    {
        HandleRaceState(raceState);

        OnRaceStateChanged.Invoke(raceState);
    }
}
