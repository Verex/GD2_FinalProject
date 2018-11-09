using P7.CanvasFlow;
using UnityEngine;

public class HostGameCanvasController : CanvasController
{
    #region Storyboard Transition

    public override void PrepareForStoryboardTransition(StoryboardTransition transition)
    {
        CanvasController destination = transition.DestinationCanvasController();
        if (destination is LoadingCanvasController && transition.direction == StoryboardTransitionDirection.Downstream)
        {
            // Get host button hook that set up network handler.
            HostGameButtonHook hostHook = (HostGameButtonHook) transition.invokedHook;

            // We are presenting the loading screen. Configure it to present the offline game scene.
            LoadingCanvasController loadingCanvasController = (LoadingCanvasController)destination;
            loadingCanvasController.SceneToLoad = "Offline";
            
            loadingCanvasController.NetworkHandler = hostHook.NetworkHandler;
            loadingCanvasController.handlerMode = LoadingCanvasController.NetworkMode.SERVER;
        }
    }

    #endregion
}