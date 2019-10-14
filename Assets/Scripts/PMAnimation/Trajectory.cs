using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trajectory
{
    public List<TrajectoryPoint> points;

    public Trajectory()
    {
        this.points = new List<TrajectoryPoint>();
    }

    public void PushPastFrame(TrajectoryPoint tp)
    {
        this.points.Insert(0, tp);
    }
    public void PushFutureFrame(TrajectoryPoint tp)
    {
        this.points.Add(tp);
    }
}
