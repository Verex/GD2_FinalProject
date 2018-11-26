using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;


[RequireComponent(typeof(PlayerInputSynchronization))]
[RequireComponent(typeof(PlayerNetworkTransform))]
public class Player : NetworkBehaviour
{

    //This is the target ship controller, it can be set in the editor or overrided at runtime. 
    [SerializeField] private ShipController m_TargetController;

    private PlayerInputSynchronization m_Input;

    public PlayerInputSynchronization Input
    {
        get 
        {
            if (m_Input == null)
            {
                m_Input = GetComponent<PlayerInputSynchronization>();
            }

            return m_Input;
        }
    }

    public readonly float CONVERGENCE_RATE = 0.05f;

    public void Possess(ShipController sc)
    {
        Assert.IsNotNull(sc, "Cannot possess a null ship controller, stupid");
        m_TargetController = sc; //Possess target ship controller

        //TODO(Any): Maybe create an OnShipPossessed event or something
    }

    /*
        ProcessUserCmd
        Parameters: 
        1) UserCmd cmd
            This is the usercmd that we're going to process, this code should only be called on the server instance of the object
        TODO(Jake): Allow the local player to process their own usercmd upon creation AKA Client-Sided Prediction 
        https://en.wikipedia.org/wiki/Client-side_prediction
     */
    public PlayerState ProcessUserCmd(UserCmd cmd, PlayerState playerState)
    {
        // Check if we're trying to fire.
        if (cmd.ActionWasReleased(PlayerInputSynchronization.IN_FIRE, Input.LastUserCommand))
        {
            Debug.Log("Fired!");
        }
        else
        {
            Debug.Log("NOt fired");
        }

        if(m_TargetController)
        {
            return m_TargetController.StateUpdate(cmd, playerState); //Move our ship
        }
        
        return null;
    }

    public void Update()
    {
        
    }

}