using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScript : MonoBehaviour
{
    public GameObject objectToFollow;
    private Rigidbody rb;
    private Vector3 velocity;
    public float dampTime = 0.3f;
    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocity = new Vector3();
        
    }

    // Update is called once per frame
    void Update()
    {
        target = objectToFollow.transform;
        float newX = Mathf.SmoothDamp(transform.position.x, objectToFollow.transform.position.x, ref velocity.x, dampTime);
        float newY = Mathf.SmoothDamp(transform.position.y, objectToFollow.transform.position.y, ref velocity.y, dampTime);
        float newZ = Mathf.SmoothDamp(transform.position.z, objectToFollow.transform.position.z, ref velocity.z, dampTime);

        transform.position = new Vector3(newX,newY,newZ);

        Debug.DrawLine(transform.position, target.position);
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(50,50,50));
        }
    }
}
