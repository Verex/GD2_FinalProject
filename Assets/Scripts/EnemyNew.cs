using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyNew : NetworkBehaviour {

    [SerializeField]
    private float maxHP = 5;
    [SyncVar]
    private float currentHP = 1;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audiosource;
    [SerializeField] private AudioClip sound_explode;

    // Use this for initialization
    void Start () {
        currentHP = maxHP;
    }
	
	// Update is called once per frame
	void Update () {
		
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
        audiosource.PlayOneShot(sound_explode, 0.7f);
        animator.Play("explosion");
        yield return new WaitForSeconds(1f);
        NetworkServer.Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("ouch");  
        if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log("ouch");
            TakeDamage(1);
        }
    }

}
