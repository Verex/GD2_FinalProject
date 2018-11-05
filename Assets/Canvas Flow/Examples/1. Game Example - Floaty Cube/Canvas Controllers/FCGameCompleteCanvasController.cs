using P7.CanvasFlow;
using UnityEngine;

public class FCGameCompleteCanvasController : CanvasController
{
    public RectTransform mainContent;

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
        }
    }

    #endregion
}