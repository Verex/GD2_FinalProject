using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using P7.CanvasFlow;

public class UIDataPass : MonoBehaviour
{
    [SerializeField] private RaceManager m_RaceManager;

    public void OnGameStoryboardWillPresentInitialCanvasController(StoryboardTransition transition)
    {
        // Get our canvas controller.
        GameWaitingCanvasController gameOverlayCanvasController =
            transition.DestinationCanvasController<GameWaitingCanvasController>();

        gameOverlayCanvasController.Configure(m_RaceManager);
    }

    public void OnGameStoryboardWillPerformTransition(StoryboardTransition transition)
    {
        CanvasController destination = transition.DestinationCanvasController();

        if (transition.direction == StoryboardTransitionDirection.Downstream)
        {
            if (destination is GameUICanvasController)
            {
                GameUICanvasController gameUICanvasController = (GameUICanvasController)destination;

                // Configure UI with race manager. 
                gameUICanvasController.Configure(m_RaceManager);
            }

            if (destination is GameEndCanvasController)
            {
                GameEndCanvasController gameEndCanvasController = (GameEndCanvasController)destination;

                // Configure UI with race manager. 
                gameEndCanvasController.Configure(m_RaceManager);
            }
        }
    }
}
