using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;


[RequireComponent(typeof(PlayerInputSynchronization))]
public class Player : MonoBehaviour
{

    //This is the target ship controller, it can be set in the editor or overrided at runtime. 
    [SerializeField] private ShipController m_TargetController;

    private PlayerInputSynchronization m_Input;

    public void Awake()
    {
        m_Input = GetComponent<PlayerInputSynchronization>();
    }

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
    public void ProcessUserCmd(UserCmd cmd)
    {
        if (cmd.ActionPressed(PlayerInputSynchronization.IN_FIRE))
        {
            //Fire!
        }
        if(cmd.ActionPressed(PlayerInputSynchronization.IN_ACCELERATE))
        {
            //Accelerate!
            Debug.Log("Accelerate!");
        }
    }

    public void Update()
    {

    }

}