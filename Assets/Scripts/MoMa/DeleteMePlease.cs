using UnityEngine;
using System.Collections;
using MoMa;

public class DeleteMePlease : MonoBehaviour
{
    private const float HipsScale = 0.2f;
    private const float LeftHandScale = 0.3f;
    private const float RightHandScale = 0.1f;

    private GameObject _dot;

    void Start()
    {
        Test3();
    }

    private void Test1 ()
    {
        Vector3 hipsP = new Vector3(-0.06526659f, 1.8399509f, 1.979511f);
        Vector3 leftHandP = new Vector3(0.02013097f, 1.9441389f, 1.842651f);
        Vector3 rightHandP = new Vector3(-0.1699411f, 1.9123331f, 2.293236f);
        Quaternion hipsQ = new Quaternion(-0.01348178f, -0.9996644f, -0.002493551f, -0.0219817f);

        Debug.Log("Rotation: " + hipsQ);
        Debug.Log("Inverse rotation: " + Quaternion.Inverse(hipsQ));
        Debug.Log("Rotation: " + hipsQ.eulerAngles);
        Debug.Log("Inverse rotation: " + Quaternion.Inverse(hipsQ).eulerAngles);

        // Create originals
        CreateDot(hipsP, HipsScale);
        CreateDot(leftHandP, LeftHandScale);
        CreateDot(rightHandP, RightHandScale);


        leftHandP = hipsQ * (leftHandP - hipsP);
        rightHandP = hipsQ * (rightHandP - hipsP);
        hipsP = hipsQ * (hipsP - hipsP);

        // Create originals
        CreateDot(hipsP, HipsScale);
        CreateDot(leftHandP, LeftHandScale);
        CreateDot(rightHandP, RightHandScale);
    }

    private void Test2()
    {
        Debug.Log("Test2");

        Trajectory t = new Trajectory();
        t.points.Add(new Trajectory.Point(1f, 1f));
        t.points.Add(new Trajectory.Point(2f, 2f));
        t.points.Add(new Trajectory.Point(3f, 3f));

        foreach (Trajectory.Point p in t.points)
        {
            CreateDot(new Vector3(p.x, 0, p.z), LeftHandScale);
        }

        Trajectory.Snippet s = t.GetLocalSnippet(1, new Vector3(2f, 0, 2f), Quaternion.Euler(0, 45, 0)); ;

        foreach (Trajectory.Point p in s.points)
        {
            CreateDot(new Vector3(p.x, 0, p.z), HipsScale);
        }
    }

    private void Test3()
    {
        Debug.Log("Test2");

        Trajectory t = new Trajectory();
        t.points.Add(new Trajectory.Point(7.8197f, 0f));
        t.points.Add(new Trajectory.Point(7.9391f, 0f));
        t.points.Add(new Trajectory.Point(8.0584f, 0f));
        t.points.Add(new Trajectory.Point(8.1777f, 0f));
        t.points.Add(new Trajectory.Point(8.2971f, 0f));
        t.points.Add(new Trajectory.Point(8.4164f, 0f));
        t.points.Add(new Trajectory.Point(8.5357f, 0f));
        t.points.Add(new Trajectory.Point(8.6551f, 0f));
        t.points.Add(new Trajectory.Point(8.7744f, 0f));
        t.points.Add(new Trajectory.Point(8.8938f, 0f));

        t.points.Add(new Trajectory.Point(8.9938f, 0f));
        t.points.Add(new Trajectory.Point(9.1f, 0f));
        t.points.Add(new Trajectory.Point(9.2f, 0f));
        t.points.Add(new Trajectory.Point(9.3f, 0f));
        t.points.Add(new Trajectory.Point(9.4f, 0f));
        t.points.Add(new Trajectory.Point(9.5f, 0f));
        t.points.Add(new Trajectory.Point(9.6f, 0f));
        t.points.Add(new Trajectory.Point(9.7f, 0f));
        t.points.Add(new Trajectory.Point(9.8f, 0f));
        t.points.Add(new Trajectory.Point(9.9f, 0f));
        t.points.Add(new Trajectory.Point(10.0f, 0f));
        t.points.Add(new Trajectory.Point(10.1f, 0f));
        t.points.Add(new Trajectory.Point(10.2f, 0f));
        t.points.Add(new Trajectory.Point(10.3f, 0f));
        t.points.Add(new Trajectory.Point(10.4f, 0f));


        foreach (Trajectory.Point p in t.points)
        {
            CreateDot(new Vector3(p.x, 0, p.z), 0.05f);
        }

        Trajectory.Snippet s = t.GetLocalSnippet(9, new Vector3(8.8938f, 0, 0f), Quaternion.Euler(0, 0, 0)); ;

        foreach (Trajectory.Point p in s.points)
        {
            CreateDot(new Vector3(p.x, 0, p.z), 0.07f);
        }
    }

    private GameObject CreateDot(Vector3 position, float scale)
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.localPosition = position;
        dot.transform.localScale = new Vector3(scale, scale, scale);

        return dot;
    }
}
