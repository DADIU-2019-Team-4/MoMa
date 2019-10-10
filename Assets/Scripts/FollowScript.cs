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
    public float speed;
    static int numOfFutureFrames = 5;
    private Vector3[] futureVelocities = new Vector3[numOfFutureFrames];
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
        //target = objectToFollow.transform;
       // Vector3 inputVector = Vector3.Normalize(objectToFollow.transform.position - transform.position);
        Vector3 inputVector = GetComponent<MovementController>().GetInput();
        //velocity = inputVector * 8 * Time.deltaTime;
        float newX = Mathf.SmoothDamp(transform.position.x, transform.position.x + inputVector.x, ref velocity.x, dampTime *Time.deltaTime);
        float newY = Mathf.SmoothDamp(transform.position.y, transform.position.y + inputVector.y, ref velocity.y, dampTime * Time.deltaTime);
        float newZ = Mathf.SmoothDamp(transform.position.z, transform.position.z + inputVector.z, ref velocity.z, dampTime * Time.deltaTime);
        
        //transform.position = new Vector3(newX, newY, newZ);
        transform.position += velocity*speed;
        //Debug.Log(velocity*speed);

     
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
        
        CalculateFuturePositions(inputVector);
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

    public void CalculateFuturePositions(Vector3 inputVector)
    {
        Vector3 futurePos = transform.position;
        Vector3 futureVel = velocity;

        for(int i =0; i< numOfFutureFrames; i++)
        {
            float newX = Mathf.SmoothDamp(transform.position.x, transform.position.x + inputVector.x, ref futureVel.x, dampTime * Time.deltaTime);
            float newY = Mathf.SmoothDamp(transform.position.y, transform.position.y + inputVector.y, ref futureVel.y, dampTime * Time.deltaTime);
            float newZ = Mathf.SmoothDamp(transform.position.z, transform.position.z + inputVector.z, ref futureVel.z, dampTime * Time.deltaTime);

            //transform.position = new Vector3(newX, newY, newZ);
            futureVelocities[i] = futurePos = futurePos + futureVel * speed;
        }

        Debug.Log(futureVelocities[0].ToString());
    }

    
}
