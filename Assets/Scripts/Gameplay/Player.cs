using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

class Player : NetworkBehaviour {

    //This is the target ship controller, it can be set in the editor or overrided at runtime. 
    [SerializeField] private ShipController m_TargetController;

    public void Possess(ShipController sc) {
        Assert.IsNotNull(sc, "Cannot possess a null ship controller, stupid");
        m_TargetController = sc; //Possess target ship controller

        //TODO(Any): Maybe create an OnShipPossessed event or something
    }

    public void Update() {
        //Accumulate mouse samples, keyboard samples, etc.

    }
    public void FixedUpdate() {
        //Process input, serialize and send to server to be handled by NetworkTransform
        var networkHandler = NetworkHandler.Instance;

    }
} 