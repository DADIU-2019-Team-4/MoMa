using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoMa
{
    public class Trajectory
    {
        public List<Point> points = new List<Point>();

        public Snippet GetLocalSnippet(
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
            public float magnitude
            {
                get { return (float)Math.Sqrt(this.x * this.x + this.z * this.z); }
            }

            public static Point operator +(Point a, Point b)
                => new Point(a.x + b.x, a.z + b.z);
            public static Point operator -(Point a, Point b)
                => new Point(a.x - b.x, a.z - b.z);

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

            public Point(Vector2 v)
            {
                this.x = v.x;
                this.z = v.y;
            }

            public override string ToString()
            {
                return "[" + this.x + ", " + this.z + "]";
            }
        }

        public class Snippet
        {
            public const int FuturePoints = 10;
            public const int PastPoints = 10;

            public Point[] points = new Point[PastPoints + FuturePoints];

            public float CalcDiff(Snippet candidate)
            {
                // In case any Snippet has null Points, the difference is infinite
                float diff = 0f;
                int totalWeight = 0;

                // Diff of past Points
                for (int i = 0; i < PastPoints; i++)
                {
                    int weight = 2^i;
                    totalWeight += weight;

                    diff += (this.points[i] == null) ||
                        (candidate.points[i] == null) ?
                            Mathf.Infinity :
                            (this.points[i] - candidate.points[i]).magnitude * weight;
                }

                // Diff of future Points
                for (int i = 0; i < FuturePoints; i++)
                {
                    int weight = 2^i;
                    totalWeight += weight;

                    diff +=
                        (this.points[PastPoints + FuturePoints - 1 - i] == null) ||
                        (candidate.points[PastPoints + FuturePoints - 1 - i] == null) ?
                            Mathf.Infinity :
                            (this.points[i] - candidate.points[i]).magnitude * weight;
                }

                return diff / totalWeight;
            }
        }
    }
}
