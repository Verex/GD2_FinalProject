using P7.CanvasFlow;
using UnityEngine;
using System.Collections;

public class GameWaitingCanvasController : CanvasController
{
    #region Mono Behaviour Lifecycle

    protected override void Start()
    {
        base.Start();
    }

    #endregion

    #region Player waiting UI

    public void Configure(RaceManager raceManager)
    {
        // Add race state listener.
        raceManager.OnRaceStateChanged.AddListener(OnRaceStateChanged);

        CheckRaceState(raceManager.CurrentState);
    }

    private void CheckRaceState(RaceManager.RaceState state)
    {
        // Transition to main UI overlay if starting.
        if (state == RaceManager.RaceState.STARTING)
        {
            PerformTransitionWithIdentifier("GameStarting");
        }
    }

    public void OnRaceStateChanged(RaceManager.RaceState raceState)
    {
        CheckRaceState(raceState);
    }

    #endregion
}