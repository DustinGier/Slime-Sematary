using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSight : MonoBehaviour
{

    SlimeBehaviour thisSlime;

    void Start() {
        thisSlime = gameObject.GetComponentInParent(typeof(SlimeBehaviour)) as SlimeBehaviour;
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Player") thisSlime.setSeesPlayer(true);
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "Player") thisSlime.setSeesPlayer(false);
    }
}
