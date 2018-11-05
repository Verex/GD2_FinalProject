using P7.CanvasFlow;
using UnityEngine;

public class MainMenuCanvasController : CanvasController
{
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
}