using P7.CanvasFlow;
using UnityEngine;

public class JoinGameCanvasController : CanvasController
{
    #region Storyboard Transition

    public override void PrepareForStoryboardTransition(StoryboardTransition transition)
    {
        var destination = transition.DestinationCanvasController();
        if (destination is LoadingCanvasController &&
            transition.direction == StoryboardTransitionDirection.Downstream)
        {
            // We are presenting the loading screen. Configure it to present the offline game scene.
            var loadingCanvasController = (LoadingCanvasController)destination;
            loadingCanvasController.SceneToLoad = "Offline";
        }
    }

    #endregion
}