using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TempPlayer : NetworkBehaviour {

    //messy temp code just so i can test enemy stuff 

    public float speed;
    public float bulletspeed;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    float x;
    float z;

    void Start () {
        bulletSpawn = this.transform;

        if (!isLocalPlayer)
        {
            return;
        }
        if (isLocalPlayer)
        {
            StartCoroutine(slowUpdate());
        }
    }
	
	void Update () {
		
	}

    public IEnumerator slowUpdate()
    {
        while (true)
        {
            x = Input.GetAxis("Horizontal") * Time.deltaTime * 10f;
            z = Input.GetAxis("Vertical") * Time.deltaTime * 10f;

            CmdMove(x, z);

            yield return new WaitForSeconds(0.05f);

            if (Input.GetAxisRaw("Jump") > 0)
            {
                CmdFire();
            }
        }
    }

    [Command]
    void CmdMove(float fx, float fz)
    {
        this.GetComponent<Rigidbody2D>().velocity = new Vector3(fx, fz, 0) * speed;
    }

    [Command]
    void CmdFire()
    {

        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        //up
        bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y + 1, bullet.transform.position.z);
        bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 1, 0) * bulletspeed;

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }

}
