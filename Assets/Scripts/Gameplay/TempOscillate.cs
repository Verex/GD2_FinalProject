using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TempOscillate : NetworkBehaviour {

    [SerializeField]
    private float delta = 1.5f;  // Amount to move left and right from the start point
    [SerializeField]
    private float speed = 2.0f;

    private Vector3 startPos;

    void Start()
    {
        if (isServer)
        {
            startPos = transform.position;
            StartCoroutine(slowUpdate());
        }

    }

    [Server]
    public IEnumerator slowUpdate()
    {
        while (true)
        {
            Vector3 v = startPos;
            v.x += delta * Mathf.Sin(Time.time * speed);
            transform.position = v;
            yield return new WaitForSeconds(0.01f);
        }
    }
	
}
