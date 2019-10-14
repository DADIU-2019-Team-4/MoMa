using UnityEngine;
using System.Collections;

public class TrajectoryPoint
{
    public Vector2 point;

    public TrajectoryPoint(Vector2 point)
    {
        this.point = new Vector2();
        this.point.x = point.x;
        this.point.y = point.y;
    }
}
