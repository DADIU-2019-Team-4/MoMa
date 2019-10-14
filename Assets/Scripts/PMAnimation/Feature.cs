using UnityEngine;
using System.Collections;

public class Feature
{
    // Position of limbs relative to the root
    // TODO: Add hands and velocity (hands, feet and root!)
    public Vector3 leftFootRelativePosition;
    public Vector3 rightFootRelativePosition;
    public Trajectory localTrajectory;
    public int cooldownTimer = 0;
}
