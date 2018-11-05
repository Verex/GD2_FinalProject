using P7.CanvasFlow;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FCGameOverlayCanvasController : CanvasController
{
    // Data references.
    private FCPlayer player;

    // UI references.
    [Header("FCGameOverlayCanvasController")]
    public Text playerHeightLabel;
    public Text playerTargetHeightLabel;
    public Button playerBoostLeftButton;
    public Button playerBoostRightButton;

    // Internal state.
    private float playerHeightPrevious;

	#region Mono Lifecycle

    private void Update()
    {
        // Allow the player to use A & D to control the boost as well.
        ListenForKeyPresses();
    }

    private void LateUpdate()
    {
        // Each frame, update the player height label if it has changed.
        UpdatePlayerHeightLabelIfNecessary();
    }

	private void OnDestroy()
	{
        if (player != null)
        {
            // Remove ourself from the player event callback.
            player.OnPlayerReachedHeight -= OnPlayerReachedHeight;
        }
	}

	#endregion

	#region Player UI

	public void ConfigureWithPlayer(FCPlayer _player)
    {
        // Store a reference to the player.
        player = _player;

        // Set the 'player target height' label's text.
        playerTargetHeightLabel.text = string.Format("TARGET : {0}m", player.targetHeight);

        // Subscribe to the 'player reached height' event.
        player.OnPlayerReachedHeight += OnPlayerReachedHeight;

        UpdatePlayerHeightLabelIfNecessary(true);
    }

    private void UpdatePlayerHeightLabelIfNecessary(bool force = false)
    {
        if (player == null)
        {
            return;
        }

        // Update the player height label if the player's height has changed.
        float height = player.CurrentHeight;
        if (force ||
            (Mathf.Approximately(height, playerHeightPrevious) == false))
        {
            playerHeightLabel.text = string.Format("•{0:0.0}m•", height);
            playerHeightPrevious = height;
        }
    }

    #endregion

    #region Player Controls

    public void OnBoostPressed(int boostId)
    {
        if (player == null)
        {
            return;
        }

        // Turn the player's boost on.
        player.SetBoosting((FCPlayer.BoostId)boostId, true);
    }

    public void OnBoostReleased(int boostId)
    {
        if (player == null)
        {
            return;
        }

        // Turn the player's boost off.
        player.SetBoosting((FCPlayer.BoostId)boostId, false);
    }

    private void ListenForKeyPresses()
    {
        // Trigger our buttons from key presses.
        ProcessKeyCodeForTarget(KeyCode.A, playerBoostLeftButton.gameObject);
        ProcessKeyCodeForTarget(KeyCode.D, playerBoostRightButton.gameObject);
    }

    private void ProcessKeyCodeForTarget(KeyCode keyCode, GameObject target)
    {
        if (Input.GetKeyDown(keyCode))
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
        }
        else if (Input.GetKeyUp(keyCode))
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerUpHandler);
        }
    }

    #endregion

    #region Player Events

    private void OnPlayerReachedHeight(FCPlayer _player)
    {
        PerformTransitionWithIdentifier("PresentGameCompleteScreen");
    }

    #endregion

    #region Reset Button Callback

    public void OnResetButtonPressed()
    {
        if (player == null)
        {
            return;
        }

        // Reset the player's position.
        player.ResetPosition();
    }

    #endregion
}