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
            Quaternion presentRotation)
        {
            // Validate input
            if (presentFrame < RuntimeComponent.FeaturePastPoints - 1 || presentFrame > points.Count - RuntimeComponent.FeaturePoints)
            {
                Debug.LogError("Attempt to create a Snippet the exceedes the past or the future limit");
                throw new Exception("Attempt to create a Snippet the exceedes the past or the future limit");
            }

            // Find present position
            Vector2 presentPosition = this.points[presentFrame].position;

            // Build the new Snippet
            Snippet snippet = new Snippet();

            for (int i = 0; i < RuntimeComponent.SnippetSize; i++)
            {
                // Compute the position of the points relative to the present position and rotation
                // Create a Point at the current position
                int addingFrame = presentFrame - RuntimeComponent.FeaturePastPoints + 1 + i;
                Vector3 destination3D = new Vector3(this.points[addingFrame].position.x, 0, this.points[addingFrame].position.y);

                // Move it to the root
                destination3D.x -= presentPosition.x;
                destination3D.z -= presentPosition.y;

                // Rotate it to face upwards
                destination3D = Quaternion.Inverse(presentRotation) * destination3D;

                // Store the relative point to the snippet
                snippet.points[i] = new Point(destination3D.GetXZVector2());
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

            public Vector2 position;

            public float magnitude
            {
                get { return position.magnitude; }
            }

            public static Point operator +(Point a, Point b)
                => new Point(a.position + b.position);

            public static Point operator -(Point a, Point b)
                => new Point(a.position - b.position);

            public static Point getMedianPoint(List<Vector2> points)
            {
                Vector2 position = new Vector2(0f, 0f);

                // Accumulate 
                foreach (Vector2 currentPoint in points)
                {
                    position += currentPoint;
                }

                position /= points.Count;

                return new Point(position);
            }

            public Point(Vector2 v)
            {
                this.position.x = (float)Math.Round(v.x, Decimals);
                this.position.y = (float)Math.Round(v.y, Decimals);
            }

            public override string ToString()
            {
                return "[" + this.position.x + ", " + this.position.y + "]";
            }
        }

        public class Snippet
        {
            public Point[] points = new Point[RuntimeComponent.SnippetSize];

            // Alternative, currently not in use
            public float CalcDiffExp(Snippet candidate)
            {
                // In case any Snippet has null Points, the difference is infinite
                float diff = 0f;
                int totalWeight = 0;

                // Diff of past Points
                for (int i = 0; i < RuntimeComponent.FeaturePastPoints; i++)
                {
                    int weight = 2^i;
                    totalWeight += weight;

                    diff += (this.points[i] == null) ||
                        (candidate.points[i] == null) ?
                            Mathf.Infinity :
                            (this.points[i] - candidate.points[i]).magnitude * weight;
                }

                // Diff of future Points
                for (int i = 0; i < RuntimeComponent.FeaturePoints; i++)
                {
                    int weight = 2^i;
                    totalWeight += weight;

                    diff +=
                        (this.points[RuntimeComponent.SnippetSize - 1 - i] == null) ||
                        (candidate.points[RuntimeComponent.SnippetSize - 1 - i] == null) ?
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
                for (int i = 0; i < RuntimeComponent.SnippetSize; i++)
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
                for (int i=0; i < RuntimeComponent.FeaturePastPoints - 1; i++)
                {
                    s += this.points[i] + ", ";
                }

                // Print seperator
                s += this.points[RuntimeComponent.FeaturePastPoints - 1] + " || ";

                // Print future Points
                for (int i = RuntimeComponent.FeaturePastPoints; i < RuntimeComponent.SnippetSize - 1; i++)
                {
                    s += this.points[i] + ", ";
                }

                // Print end
                s += this.points[RuntimeComponent.SnippetSize - 1] + "}";

                return s;
            }

        }
    }
}
