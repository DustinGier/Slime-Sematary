using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Enemy Hurt Box") {
            SlimeBehaviour hitSlime = col.gameObject.GetComponentInParent(typeof(SlimeBehaviour)) as SlimeBehaviour;
            
            hitSlime.hitByPlayer(1f);
        }
    }
}
