using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoMa
{
    public class Trajectory
    {
        // Animation.Frames per Trajectory.Point
        // The lower the number, the denser the Trajectory points will be.
        // That means closer approximation of the actual Trajectory and slower search for a Snippet
        public const int FramesPerPoint = 4;

        public List<Point> points = new List<Point>();

        public Snippet GetLocalSnippet(
            int presentFrame,
            Vector3 presentPosition,
            Quaternion presentRotation)
        {
            // Validate input
            if (presentFrame < Snippet.PastPoints-1 || presentFrame > points.Count - Snippet.FuturePoints)
            {
                Debug.LogError("Attempt to create a Snippet the exceedes the past or the future limit");
                throw new Exception("Attempt to create a Snippet the exceedes the past or the future limit");
            }

            // Build the new Snippet
            Snippet snippet = new Snippet();

            for (int i = 0; i < Snippet.Size; i++)
            {
                // Compute the position of the points relative to the present position and rotation
                // Create a Point at the current position
                int addingFrame = presentFrame - Snippet.PastPoints + 1 + i;
                Vector3 destination = new Vector3(this.points[addingFrame].x, 0, this.points[addingFrame].z);

                // Move it to the root
                destination.x -= presentPosition.x;
                destination.z -= presentPosition.z;

                // Rotate it to face upwards
                destination = Quaternion.Inverse(presentRotation) * destination;

                // Store the relative point to the snippet
                snippet.points[i] = new Point(destination.x, destination.z);
            }

            return snippet;
        }

        public override string ToString()
        {
            string s = "Trajectory: {";

            if (this.points.Count > 0)
            {
                s += this.points[0];
            }

            foreach(Point p in this.points.GetRange(1, this.points.Count-1))
            {
                s += ", " + p;
            }

            return s + "}";
        }

        public class Point
        {
            public const int Decimals = 4;

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
                this.x = (float)Math.Round(x, Decimals);
                this.z = (float)Math.Round(z, Decimals);
            }

            public Point(Vector2 v)
            {
                this.x = (float)Math.Round(v.x, Decimals);
                this.z = (float)Math.Round(v.y, Decimals);
            }

            public override string ToString()
            {
                return "[" + this.x + ", " + this.z + "]";
            }
        }

        public class Snippet
        {
            public const int FuturePoints = 15;
            public const int PastPoints = 10;
            public const int Size = FuturePoints + PastPoints;

            public Point[] points = new Point[Size];

            // Alternative, currently not in use
            public float CalcDiffExp(Snippet candidate)
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

            public float CalcDiff(Snippet candidate)
            {
                // In case any Snippet has null Points, the difference is infinite
                float diff = 0f;

                // Diff of past Points
                for (int i = 0; i < Size; i++)
                {
                    diff += (this.points[i] == null) ||
                        (candidate.points[i] == null) ?
                            Mathf.Infinity :
                            (this.points[i] - candidate.points[i]).magnitude;
                }

                return diff;
            }

            public override string ToString()
            {
                // Print start
                string s = "Snippet: {";

                // Print past Points
                for (int i=0; i < PastPoints - 1; i++)
                {
                    s += this.points[i] + ", ";
                }

                // Print seperator
                s += this.points[PastPoints - 1] + " || ";

                // Print future Points
                for (int i = PastPoints; i < Size - 1; i++)
                {
                    s += this.points[i] + ", ";
                }

                // Print end
                s += this.points[Size - 1] + "}";

                return s;
            }

        }
    }
}
