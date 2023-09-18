using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kicker : MonoBehaviour
{
    Transform player;
    PlayerController playerController;

    [SerializeField]
    float kickStrength = 50f;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.parent;
        playerController = player.GetComponent<PlayerController>();
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ball" && playerController.IsKicking)
        {
            Vector2 kickDirection = (transform.position - collision.gameObject.transform.position).normalized;
            collision.rigidbody.AddForce(-kickDirection * kickStrength, ForceMode2D.Impulse);
        }
            
    }
}
