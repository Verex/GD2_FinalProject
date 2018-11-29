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
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private Vector3 m_ProjectileOffset;
    [SerializeField] private AudioSource audiosource;
    [SerializeField] private AudioClip sound_shoot;

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

        // Assign reference to this player.
        sc.Owner = this;

        //Initialize Player Network Transform with the ship controller's position
        var pnt = GetComponent<PlayerNetworkTransform>();
        pnt.Initialize(sc.transform.position);
        //TODO(Any): Maybe create an OnShipPossessed event or something

        if (!isServer)
        {
            sc.GetComponent<DynamicNetworkTransform>().enabled = false; //Disable client's dynamic network transform
        }

    }

    public void MoveShip(Vector3 position) //Move our ship here
    {
        if (m_TargetController == null)
        {
            return;
        }
        m_TargetController.TargetPosition = position;
    }

    public Vector3 CurrentShipPosition
    {
        get
        {
            return m_TargetController.transform.position;
        }
    }

    /*
        ProcessUserCmd
        Parameters: 
        1) UserCmd cmd
            This is the usercmd that we're going to process, this code should only be called on the server instance of the object
        TODO(Jake): Allow the local player to process their own usercmd upon creation AKA Client-Sided Prediction 
        https://en.wikipedia.org/wiki/Client-side_prediction
     */
    public PlayerState ProcessUserCmd(UserCmd cmd, PlayerState playerState, float dt)
    {
        if (isServer)
        {
            // Check if we're trying to fire.
            if (cmd.ActionWasReleased(PlayerInputSynchronization.IN_FIRE, Input.LastUserCommand))
            {
                GameObject projectile = Instantiate(
                    m_ProjectilePrefab, 
                    m_TargetController.transform.position + m_ProjectileOffset, 
                    m_ProjectilePrefab.transform.rotation);

                NetworkServer.Spawn(projectile);
                audiosource.PlayOneShot(sound_shoot, 0.3f);
            }
            else
            {
                //Debug.Log("NOt fired");
            }
        }

        if (m_TargetController)
        {
            return m_TargetController.StateUpdate(cmd, playerState, dt); //Move our ship
        }

        return null;
    }

}