using UnityEngine;
using System.Collections;

namespace MoMa
{
    public class Feature
    {
        // Animation.Frames per Trajectory.Point
        // The lower the number, the denser the Trajectory points will be.
        // That means closer approximation of the actual Trajectory and slower search for a Snippet
        public const int FramesPerPoint = 5;

        // Trajectory.Points per Feature
        // The lower the number, the more Features we will have
        // That means that we cover more intermediate Snippets, at the cost of having more to search through
        public const int PointsPerFeature = 2;

        public readonly int frameNum;
        public readonly Trajectory.Snippet snippet;
        public readonly Pose pose;

        public int cooldownTimer = 0;

        public Feature(int frameNum, Trajectory.Snippet snippet, Pose pose)
        {
            this.frameNum = frameNum;
            this.snippet = snippet;
            this.pose = pose;
        }
    }
}