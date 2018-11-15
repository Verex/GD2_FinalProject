using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipSpawn : NetworkBehaviour
{
	// Not to be confused with network player id (Spawning only).
	[SerializeField] public int playerID;

	public override void OnStartServer()
	{
		// Register this spawn.
		NetworkHandler.Instance.RegisterShipSpawn(playerID, transform);
	}
}
