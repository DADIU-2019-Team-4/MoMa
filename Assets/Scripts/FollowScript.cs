using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FollowScript : MonoBehaviour
{
    public GameObject objectToFollow;
    public GameObject Dot;
    private Rigidbody rb;
    private Vector3 velocity;
    private float dampTime = 0.3f;
    private Transform target;
    static int numOfPastVelocities = 5;
    private Vector3[] pastVelocities = new Vector3[numOfPastVelocities];
    private Queue<GameObject> dots = new Queue<GameObject>();
    private float timerValue = 0.2f;
    private float currentTimer;

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

        transform.position = new Vector3(newX, newY, newZ);

        for (int i = numOfPastVelocities-1; i > 0; i--)
        {
            pastVelocities[i] = pastVelocities[i - 1];
        }

        if (currentTimer > timerValue)
        {
            GameObject dot = Instantiate(Dot);
            dot.transform.position = new Vector3(pastVelocities[0].x, 0.1f, pastVelocities[0].z);
            dots.Enqueue(dot);

            if (dots.Count == numOfPastVelocities)
            {
                Destroy(dots.First());
                dots.Dequeue();
            }

            currentTimer = 0;
        }
        else
        {
            currentTimer += Time.deltaTime;
        }

        pastVelocities[0] = (new Vector3(newX, newY, newZ));

    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(50, 50, 50));
        }
    }
}
