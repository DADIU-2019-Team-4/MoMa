using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoMa
{
    public class Trajectory
    {
        public List<Point> points = new List<Point>();

        public Snippet GetSnippet(
            int presentFrame,
            Vector3 presentPosition,
            Quaternion presentRotation)
        {
            // Validate input
            if (presentFrame < Snippet.PastPoints || presentFrame > points.Count - Snippet.FuturePoints)
            {
                Debug.LogError("Attempt to create a Snippet the exceedes the past or the future limit");
                throw new Exception("Attempt to create a Snippet the exceedes the past or the future limit");
            }

            // Build the new Snippet
            Snippet snippet = new Snippet();

            for (int i = 0; i < Snippet.FuturePoints + Snippet.PastPoints; i++)
            {
                // Compute the position of the points relative to the present position and rotation
                // Create a Point at the current position
                int addingFrame = presentFrame - Snippet.PastPoints + i;
                Vector2 destination = new Vector2(this.points[addingFrame].x, this.points[addingFrame].z);

                // Move it to the root
                destination.x -= presentPosition.x;
                destination.y -= presentPosition.z; // Mind y and z

                // Ratate it to face upwards
                destination.Rotate(Quaternion.Inverse(presentRotation).eulerAngles.z); // TODO: This might be z instead of x

                // Store the relative point to the snippet
                snippet.points[i] = new Point(destination.x, destination.y);
            }

            return snippet;
        }

        public class Point
        {
            public float x;
            public float z;

            public static Point getMedianPoint(List<Vector3> points)
            {
                Point point = new Point(0f, 0f);

                // Accumulate 
                foreach (Vector3 currentPoint in points)
                {
                    point.x += currentPoint.x;
                    point.z += currentPoint.z;
                }

                point.x /= points.Count;
                point.z /= points.Count;

                return point;
            }

            public Point(float x, float z)
            {
                this.x = x;
                this.z = z;
            }
        }

        public class Snippet
        {
            public const int FuturePoints = 15;
            public const int PastPoints = 10;

            public Point[] points = new Point[PastPoints + FuturePoints];

            public float CalcDiff(Snippet candidate)
            {
                // In case any Snippet has null Points, the difference is infinite
                float diff = 0f;

                // No weights
                for (int i = 0; i < this.points.Length; i++)
                    diff += (this.points[i]==null) || (candidate.points[i]==null) ?
                        Mathf.Infinity :
                        Mathf.Pow(
                            (this.points[i].x - candidate.points[i].x) -
                            (this.points[i].z - candidate.points[i].z)
                            , 2);

                // Using weights
                //float[] weights = new float[currentFeature.localTrajectory.points.Count];
                //for (int i = 0; i < currentFeature.localTrajectory.points.Count; ++i)
                //    diff += weights[i] * Mathf.Pow(currentFeature.localTrajectory.points[i].point.magnitude -
                //        candidateFeature.localTrajectory.points[i].point.magnitude, 2);

                return diff;
            }
        }
    }
}
