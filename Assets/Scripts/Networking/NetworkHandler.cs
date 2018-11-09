using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkHandler : NetworkManager
{

    //Singleton Design Pattern =) 
    private static NetworkHandler s_Instance;
    public static NetworkHandler Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = GameObject.FindObjectOfType(
                    typeof(NetworkHandler)
                ) as NetworkHandler; //Initialize our network handler
            }
            return s_Instance;
        }
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}