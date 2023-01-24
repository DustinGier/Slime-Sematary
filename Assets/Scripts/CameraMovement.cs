using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    public float minimumPlayerDistanceToBounds = 0.5f;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pPos = player.position;

        float cameraHeight = 2 * cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        Vector2 newCenter = new Vector2(transform.position.x, transform.position.y);

        Vector2 topLeft = new Vector2(transform.position.x - cameraWidth / 2, transform.position.y + cameraHeight / 2);
        Vector2 bottomRight = new Vector2(transform.position.x + cameraWidth / 2, transform.position.y - cameraHeight / 2);

        if (pPos.x < topLeft.x + minimumPlayerDistanceToBounds) newCenter.x = pPos.x - minimumPlayerDistanceToBounds + cameraWidth / 2;
        else if (pPos.x > bottomRight.x - minimumPlayerDistanceToBounds) newCenter.x = pPos.x + minimumPlayerDistanceToBounds - cameraWidth / 2;

        if (pPos.y < bottomRight.y + minimumPlayerDistanceToBounds) newCenter.y = pPos.y - minimumPlayerDistanceToBounds + cameraHeight / 2;
        else if (pPos.y > topLeft.y - minimumPlayerDistanceToBounds) newCenter.y = pPos.y + minimumPlayerDistanceToBounds - cameraHeight / 2;

        //Update camera
        transform.position = new Vector3(newCenter.x, newCenter.y, transform.position.z);
    }
}
