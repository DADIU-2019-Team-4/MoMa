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
    public float dampTime = 0.3f;
    private Transform target;
    static int numOfPastVelocities = 50;
    private Vector3[] pastVelocities = new Vector3[numOfPastVelocities];
    public float speed;
    static int numOfFutureFrames = 50;
    private Vector3[] futureVelocities = new Vector3[numOfFutureFrames];
    private List<GameObject> pastDots = new List<GameObject>();
    private List<GameObject> futureDots = new List<GameObject>();
    private float timerValue = 0.2f;
    private float currentTimer;
    private int framePerDot = 5;

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
        Debug.Log(inputVector.ToString());
        //Debug.Log(inputVector.ToString());
        //Debug.Log(velocity);
        //velocity = inputVector * 8 * Time.deltaTime;

        SavePastPositions(inputVector);    
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

    public void DrawPastDots()
    {
        foreach (GameObject pastDot in pastDots)
        {
            Destroy(pastDot);
        }

        pastDots.Clear();

        for (int i = 0; i < numOfPastVelocities / framePerDot; i++)
        {
            GameObject dot = Instantiate(Dot);
            pastDots.Add(dot);
            dot.transform.position = new Vector3(pastVelocities[framePerDot * i].x, 0.1f, pastVelocities[framePerDot * i].z);
        }
    }

    public void DrawFutureDots()
    {
        foreach (GameObject futureDot in futureDots)
        {
            Destroy(futureDot);
        }

        futureDots.Clear();

        for (int i = 0; i < numOfFutureFrames / framePerDot; i++)
        {
            GameObject dot = Instantiate(Dot);
            futureDots.Add(dot);
            dot.transform.position = new Vector3(futureVelocities[framePerDot * i].x, 0.1f, futureVelocities[framePerDot * i].z);
        }
    }

    public void SavePastPositions(Vector3 inputVector)
    {
        Vector3.SmoothDamp(transform.position, (transform.position + inputVector), ref velocity, dampTime );
        
        if (inputVector == Vector3.zero)
            velocity = Vector3.zero;
        
        transform.position += velocity * speed;


        for (int i = numOfPastVelocities - 1; i > 0; i--)
        {
            pastVelocities[i] = pastVelocities[i - 1];
        }

        DrawPastDots();
        pastVelocities[0] = (transform.position);
    }

    public void CalculateFuturePositions(Vector3 inputVector)
    {
        Vector3 futurePos = transform.position;
        Vector3 futureVel = velocity;

        for(int i =0; i< numOfFutureFrames; i++)
        {
            Vector3.SmoothDamp(futurePos, futurePos + inputVector, ref futureVel, dampTime);
            
            futurePos = futurePos + futureVel * speed;
            futureVelocities[i] = futurePos;
        }
        DrawFutureDots();
    }

    
}
