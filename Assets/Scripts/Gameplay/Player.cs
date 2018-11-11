using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;


[RequireComponent(typeof(PlayerInputSynchronization))]
class Player : MonoBehaviour
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

    public void Update()
    {
        //Accumulate mouse samples, keyboard samples, etc.

    }
    public void FixedUpdate()
    {
        UserCmd newCommand = m_Input.CreateUserCmd();
        //Fill user command
        m_Input.PipeUserCommand(newCommand); //Pipe new command to server
    }
}