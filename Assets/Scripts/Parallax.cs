using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    public Transform near;
    public Transform far;

    public float nearFactor = 0.9f;
    public float farFactor = 0.8f;

    private Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = gameObject.transform; 
    }

    // Update is called once per frame
    void Update()
    {
        near.position = new Vector3(cam.position.x * nearFactor, cam.position.y * nearFactor, near.position.z);
        far.position = new Vector3(cam.position.x * farFactor, cam.position.y * farFactor, far.position.z);
    }
}
