using P7.CanvasFlow;
using UnityEngine;

public class FCPassDataFromGameToUI : MonoBehaviour
{
    public FCPlayer player;

	public void OnGameStoryboardWillPresentInitialCanvasController(StoryboardTransition transition)
    {
        var gameOverlayCanvasController =
            transition.DestinationCanvasController<FCGameOverlayCanvasController>();
        gameOverlayCanvasController.ConfigureWithPlayer(player);
    }
}
