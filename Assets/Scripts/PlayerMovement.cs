using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 movementdirection;
    public float speed = 10;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movementdirection = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            movementdirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movementdirection += Vector3.left;

        }
        if (Input.GetKey(KeyCode.D))
        {
            movementdirection += Vector3.right;

        }
        if (Input.GetKey(KeyCode.S))
        {
            movementdirection += Vector3.back;

        }

        transform.position += movementdirection * speed * Time.deltaTime;
    }
}
