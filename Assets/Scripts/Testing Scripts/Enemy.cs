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
    private float maxHP;

    [SerializeField]
    private Slider healthbar;

    [SerializeField]
    private CanvasGroup canvasgroup;

    [SyncVar]
    private float currentHP = 1;

    private int layerMask;
    private bool hit;
    private Animator animator;

    void Start () {
        //does not collide outside of player layer
        layerMask = (LayerMask.GetMask("Player"));
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

            //healthbar.value = currentHP / maxHP;
        }
        else
        {
            //canvasgroup.alpha = 0f;

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
        NetworkServer.Destroy(this.gameObject);
    }

}
