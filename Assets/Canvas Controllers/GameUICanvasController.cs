using P7.CanvasFlow;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUICanvasController : CanvasController
{
    private RaceManager m_RaceManager;
    [SerializeField] private Text m_CountdownText;

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

    #region Player UI

    private IEnumerator UpdateCountdown()
    {
        // Update our race timer.
        while (m_RaceManager.TimeBeforeStart > 0)
        {
            // Get ceil time before start.
            int roundedTime = Mathf.CeilToInt(m_RaceManager.TimeBeforeStart);

            // Update countdown time.
            m_CountdownText.text = roundedTime.ToString();

            yield return new WaitUntil(() => Mathf.CeilToInt(m_RaceManager.TimeBeforeStart) < roundedTime || m_RaceManager.TimeBeforeStart <= 0);
        }

        // Update countdown text.
        m_CountdownText.text = "GO!";

        // Wait before dismissing countdown text.
        yield return new WaitForSeconds(1f);

        // Hide countdown text.
        m_CountdownText.enabled = false;

        yield break;
    }

    public void Configure(RaceManager raceManager)
    {
        // Assign reference to race manager.
        m_RaceManager = raceManager;

        // Add race state listener.
        raceManager.OnRaceStateChanged.AddListener(OnRaceStateChanged);

        // Start countdown coroutine.
        StartCoroutine(UpdateCountdown());
    }

    public void OnRaceStateChanged(RaceManager.RaceState raceState)
    {
        
    }

    #endregion
}