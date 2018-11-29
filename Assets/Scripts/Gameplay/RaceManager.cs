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
    public float StartingTime;

    public bool IsRacing
    {
        get 
        {
            return CurrentState == RaceState.IN_PROGRESS;
        }
    }


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

    public float TimeBeforeStart
    {
        get
        {
            return (StartingTime + m_RaceStartDelay) - Time.time;
        }
    }

    [Server]
    public void StartRace()
    {
        // Set our starting race state.
        CurrentState = RaceState.STARTING;

        // Assign the starting time.
        StartingTime = Time.time;

        // Start race waiting coroutine.
        StartCoroutine(WaitForStart());
    }

    private IEnumerator WaitForStart()
    {
        // Wait until the race can start.
        yield return new WaitUntil(() => TimeBeforeStart <= 0f);

        // Assign new race state.
        CurrentState = RaceState.IN_PROGRESS;

        CurrentState = RaceState.FINISHED;

        yield break;
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
