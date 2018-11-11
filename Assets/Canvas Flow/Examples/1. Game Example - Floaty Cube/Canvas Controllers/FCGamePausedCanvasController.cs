using P7.CanvasFlow;
using UnityEngine;

public class FCGamePausedCanvasController : CanvasController
{
    public RectTransform mainContent;

    #region Canvas Controller Appearance

	protected override void CanvasWillAppear()
	{
        // If our pause canvas is about to appear and it is being presented, pause the game.
        if (IsBeingPresented)
        {
            Time.timeScale = 0f;
        }
	}

	protected override void CanvasWillDisappear()
    {
        // If our pause canvas is about to disappear and it is being dismissed, un-pause the game.
        if (IsBeingDismissed)
        {
            Time.timeScale = 1f;
        }
    }

    #endregion

	#region Storyboard Transition

	public override void PrepareForStoryboardTransition(StoryboardTransition transition)
    {
        var destination = transition.DestinationCanvasController();
        if (destination is FCLoadingCanvasController &&
            transition.direction == StoryboardTransitionDirection.Downstream)
        {
            // We are presenting the loading screen. Configure it to present the Menu scene.
            var loadingCanvasController = (FCLoadingCanvasController)destination;
            loadingCanvasController.SceneToLoad = "FCMenuScene";

            // Reset time scale.
            Time.timeScale = 1f;
        }
    }

    #endregion
}