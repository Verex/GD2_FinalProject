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
            // Get host button hook that set up network handler.
            JoinGameButtonHook joinHook = (JoinGameButtonHook) transition.invokedHook;

            // We are presenting the loading screen. Configure it to present the offline game scene.
            LoadingCanvasController loadingCanvasController = (LoadingCanvasController)destination;
            loadingCanvasController.SceneToLoad = "Offline";
            
            // Assign network handler properties and network mode.
            loadingCanvasController.NetworkHandler = joinHook.NetworkHandler;
            loadingCanvasController.handlerMode = LoadingCanvasController.NetworkMode.CLIENT;
        }
    }

    #endregion
}