using P7.CanvasFlow;
using UnityEngine;
using UnityEngine.UI;

public class GameUICanvasController : CanvasController
{
    [SerializeField] private Text m_CountdownText;

    #region Mono Behaviour Lifecycle

    protected override void Start()
    {
        base.Start();

        // Configure your canvas controller.
    }

    #endregion

    /*
    #region Storyboard Transition

    // An opportunity to pass data between canvas controllers when using Storyboards.
    public override void PrepareForStoryboardTransition(StoryboardTransition transition)
    {
        //var source = transition.SourceCanvasController<YourSourceCanvasControllerType>();
        //var destination = transition.DestinationCanvasController<YourDestinationCanvasControllerType>();
    }

    #endregion
    */

    #region Player UI

    public void Configure(RaceManager raceManager)
    {
        // Add race state listener.
        raceManager.OnRaceStateChanged.AddListener(OnRaceStateChanged);

        m_CountdownText.text = ((raceManager.StartingTime + raceManager.RaceStartDelay) - Time.time).ToString();
    }

    public void OnRaceStateChanged(RaceManager.RaceState raceState)
    {
        
    }

    #endregion
}