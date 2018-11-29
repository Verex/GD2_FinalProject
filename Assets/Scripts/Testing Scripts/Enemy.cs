using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour {

    //distance of the raycast
    [SerializeField]
    private float rayDistance;

    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    private float maxHP;

    [SerializeField]
    private Slider healthbar;

    [SerializeField]
    private CanvasGroup canvasgroup;

    [SyncVar]
    private float currentHP;

    private int layerMask;
    private bool hit;
    private Animator animator;

    void Start () {
        //does not collide outside of player layer
        layerMask = (LayerMask.GetMask("Player"));

        StartCoroutine(Firing());

        currentHP = maxHP;

        animator = GetComponent<Animator>();
    }
	
	void Update () {

        if (currentHP > 0)
        {
            //raycasting
            Debug.DrawRay(transform.position, Vector2.down * 4f, Color.yellow);
            RaycastHit2D raycast = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, layerMask);
            if (raycast.collider)
            {
                Debug.DrawRay(transform.position, Vector2.down * raycast.distance, Color.red);
                hit = true;
            }

            healthbar.value = currentHP / maxHP;
        }
        else
        {
            canvasgroup.alpha = 0f;

        }

    }


    public IEnumerator Firing()
    {
        while (true)
        {
            while (hit)
            {
                Debug.Log("Fire");
                CmdFire();
                hit = false;
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    [Command]
    void CmdFire()
    {

        //current position
        Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
        //down 
        var direction = Vector2.down;
        //instantiate
        GameObject projectile = (GameObject)Instantiate(bulletPrefab, myPos, Quaternion.identity);
        //give velocity
        projectile.GetComponent<Rigidbody2D>().velocity = direction * 5.0f;
        //spawn
        NetworkServer.Spawn(projectile);

        //destroy the bullet after 1 second
        Destroy(projectile, 1.0f);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player Bullet"))
        {
            Debug.Log("ow");
            Destroy(collision.gameObject);
            TakeDamage(1);
            Debug.Log(currentHP);
        }
    }

    public void TakeDamage(int amount)
    {
        //Add a check for “isServer” to the TakeDamage function, so that damage will only be applied on the Server.
        if (!isServer)
        {
            return;
        }

        currentHP -= amount;
        if (currentHP <= 0)
        {
            StartCoroutine(Death());
        }

    }

    private IEnumerator Death()
    {
        
        animator.Play("defexplosion");
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }

}
