using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MoMa
{
    [System.Serializable]
    public class Animation
    {
        public readonly string animationName;

        // Each feature refers to a set of frames
        public List<Frame> frameList = new List<Frame>();
        public List<Feature> featureList = new List<Feature>();

        public Animation(string animationName)
        {
            this.animationName = animationName;
        }

        public void ComputeFeatures()
        {
            // 1. Find fitted trajectory for the whole animation
            Trajectory trajectory = ComputeFittedTrajectory();

            // 2. Compute Features
            for (
                int pointNum = Trajectory.Snippet.PastPoints;   // Left padding for past Points
                pointNum < trajectory.points.Count - Trajectory.Snippet.FuturePoints;   // Right padding for future Points
                pointNum += Feature.PointsPerFeature   // Add 1 Feature every FeaturePeriod Points
                )
            {
                // Find the first Frame of the current Point(s)
                int frameNum = pointNum * Feature.FramesPerPoint;

                // Built new Feature
                this.featureList.Add( new Feature(
                    frameNum,

                    // Compute the Trajectory Snippet relative to the current Frame
                    trajectory.GetSnippet(
                        pointNum,
                        this.frameList[frameNum].boneDataDict[Bone.Type.hips].position,
                        this.frameList[frameNum].boneDataDict[Bone.Type.hips].rotation
                        ),

                    // Compute the Pose according to the current Frame
                    new Pose(this.frameList[frameNum])
                    )
                );
            }
        }

        private Trajectory ComputeFittedTrajectory()
        {
            Trajectory fittedTrajectory = new Trajectory();

            // Currently, it starts at the end of the median of the sample
            for (int frameNum = 0; frameNum < this.frameList.Count - Feature.FramesPerPoint; frameNum += Feature.FramesPerPoint)
            {
                // Find the median Point of all the frames in the current sample
                Trajectory.Point point = Trajectory.Point.getMedianPoint(
                    this.frameList.GetRange(frameNum, Feature.FramesPerPoint).ConvertAll(
                        f => f.boneDataDict[Bone.Type.hips].position
                        )
                    );

                fittedTrajectory.points.Add(point);
            }

            return fittedTrajectory;
        }

        public class Clip
        {
            private Frame[] _frames;
            private int _currentFrame = 0;

            public Clip() {
                this._frames = new Frame[0];
            }

            public Clip(List<Frame> frameList)
            {
                this._frames = new Frame[frameList.Count];

                for (int i=0; i < frameList.Count; i++)
                {
                    this._frames[i] = frameList[i];
                }
            }

            public Frame Step()
            {
                // Return the current Frame and increase the Frame counter or null
                return _currentFrame < this._frames.Length ?
                    this._frames[this._currentFrame++] :
                    null;
            }

            public bool isOver()
            {
                return this._currentFrame == this._frames.Length;
            }
        }
    }
}
