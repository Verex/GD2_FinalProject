using P7.CanvasFlow;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
public class GameEndCanvasController : CanvasController
{
    [SerializeField] private Button m_QuitButton;

    #region Mono Behaviour Lifecycle

    protected override void Start()
    {
        base.Start();

        // Add listener to button.
        m_QuitButton.onClick.AddListener(OnQuitClicked);
    }

    #endregion

    #region Storyboard Transition

    public override void PrepareForStoryboardTransition(StoryboardTransition transition)
    {
        CanvasController destination = transition.DestinationCanvasController();
        if (destination is LoadingCanvasController && transition.direction == StoryboardTransitionDirection.Downstream)
        {
            // We are presenting the loading screen. Configure it to present the offline game scene.
            LoadingCanvasController loadingCanvasController = (LoadingCanvasController)destination;
            loadingCanvasController.SceneToLoad = "MainMenu";
        }
    }

    #endregion

    #region PlayerUI

    public void OnQuitClicked()
    {
        // Destroy network handler.
        Destroy(NetworkHandler.Instance.gameObject);
    }

    public void Configure(RaceManager raceManager)
    {

    }

    #endregion
}