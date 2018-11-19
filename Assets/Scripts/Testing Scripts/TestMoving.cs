using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoving : MonoBehaviour {

    public float speed;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Vector2.up * 0.04f);
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector2.right * speed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(-Vector2.right * speed);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector2.up * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-Vector2.up * speed);
        }

    }
}
