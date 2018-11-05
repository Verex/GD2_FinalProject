using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

enum ClientState {
	SERVER = 0,
	CLIENT = 1,
	HOST = 2
};
public class ServerManager : MonoBehaviour {
	private NetworkManager m_NetworkManager;

	[SerializeField] private ClientState m_ClientState;

	private bool m_Startup = true;

	// Use this for initialization
	void Start () {
		m_NetworkManager = GetComponent<NetworkManager>();
	}

	// Update is called once per frame
	void Update () {
		if(m_Startup) {
			switch(m_ClientState) {
				case ClientState.CLIENT: m_NetworkManager.StartClient(); break;
				case ClientState.SERVER: m_NetworkManager.StartServer(); break;
				case ClientState.HOST: m_NetworkManager.StartHost(); break;
			}
			m_Startup = false;
		}
	}
}

