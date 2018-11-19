using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using P7.CanvasFlow;

public class UIDataPass : MonoBehaviour
{
    public void OnGameStoryboardWillPresentInitialCanvasController(StoryboardTransition transition)
    {
		// Get our canvas controller.
        GameWaitingCanvasController gameOverlayCanvasController =
            transition.DestinationCanvasController<GameWaitingCanvasController>();
    }

	public void OnGameStoryboardWillPerformTransition(StoryboardTransition transition)
	{
		CanvasController destination = transition.DestinationCanvasController();
		
		if (destination is GameUICanvasController && transition.direction == StoryboardTransitionDirection.Downstream)
		{
			Debug.Log("Transitioning.");
		}
	}
}
