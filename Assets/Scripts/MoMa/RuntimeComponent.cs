﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoMa
{
    public class RuntimeComponent
    {
        // Fine-tuning
        public const float RecalculationThreshold = 0.2f; // The maximum diff of two Trajectories before recalculating the Animation
        //public const float RecalculationThreshold = Mathf.Infinity; // The maximum diff of two Trajectories before recalculating the Animation
        public const int CooldownTime = 0; // Number of frames that a Frame is on cooldown after being played
        public const int CandidateFramesSize = 50; // Number of candidate frames for a transition (tradeoff: fidelity/speed)
        public const int ClipBlendPoints = 0; // Each Animation Clip is blended with the next one for smoother transition. The are both played for this num of Frames

        // Frame/Point/Feature ratios
        // FeaturePoints % FeatureEveryPoints should be 0
        public const int SkipFrames = 3;  // Take 1 Frame every SkipFrames in the Animation file
        public const int FeaturePoints = 3;  // Trajectory.Points per Feature. The lower the number, the shorter time the Feature covers
        public const int FeaturePastPoints = 2;  // The number of Points in the past that is used in a Snippet. The lower the number, the lower the fidelity
        public const int FeatureEveryPoints = 2;  // Trajectory.Points per Feature. The lower the nuber, the shorter time the Feature covers
        // FramesPerPoint % 2 should be 0
        public const int FramesPerPoint = 4;    // Animation.Frames per Trajectory.Point. The lower the number, the denser the Trajectory points will be.

        public const int FramesPerFeature = FramesPerPoint * FeaturePoints;  // Animation.Frames per Feature
        public const int FeatureStep = FeaturePoints / FeatureEveryPoints;  // Features overlap generally. This is the distance between two matching Features.
        public const int SnippetSize = FeaturePoints + FeaturePastPoints;

        private List<Animation> _anim = new List<Animation>();
        private List<Feature> _onCooldown = new List<Feature>();
        private int _currentAnimation = 0;
        private int _currentFeature = 0;
        private Animation.Clip _currentClip;
        private FollowerComponent _fc;

        public RuntimeComponent(FollowerComponent fc)
        {
            // TODO: This should happen offline. Instead we only need to open its result
            //this._anim.Add(Packer.Pack("walk", "MoCapData", "walk_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("jog", "MoCapData", "jog3_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("acceleration", "MoCapData", "acceleration_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("run", "MoCapData", "Copy of run1_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("walk_continuous", "MoCapData", "walk_continuous2_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("circle_left", "MoCapData", "circle_left_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("circle_right", "MoCapData", "circle_right_DEFAULT_FIX"));

            // TODO: This exists for dubugging. Maybe it needs to be removed.
            this._fc = fc;
        }
        
        public Animation.Clip QueryClip(Trajectory.Snippet currentSnippet)
        {
            // 1. Reduce cooldowns (after the previous Clip has finished)
            ReduceCooldowns();

            // 2. Check if the next Clip is fitting (or the first one, if we reach the end)
            // The next Clip is NOT necesserily the product of the next Feature
            this._currentFeature = (this._currentFeature + FeatureStep) % this._anim[this._currentAnimation].featureList.Count;

            float diff = currentSnippet.CalcDiff(this._anim[this._currentAnimation].featureList[this._currentFeature].snippet);

            if (diff > RecalculationThreshold)
            {
                (this._currentAnimation, this._currentFeature) = QueryFeature(currentSnippet);

                //Debug.Log("Recalculating");
                //Debug.Log("File: " + this._currentAnimation + " Clip: " +  this._currentFeature);
            }
            else
            {
                Debug.Log("Not Recalculating");
            }

            // 3. Construct the Clip, blend it with the current one and return it
            Animation.Clip nextClip = new Animation.Clip(
                this._anim[this._currentAnimation].frameList.GetRange(
                    this._anim[this._currentAnimation].featureList[this._currentFeature].frameNum,
                    FramesPerPoint * (FeaturePoints + ClipBlendPoints)
                    )
                );

            nextClip.BlendWith(_currentClip);
            _currentClip = nextClip;

            // 4. Put the current Feature on cooldown
            PutOnCooldown(this._anim[this._currentAnimation].featureList[this._currentFeature]);

            return nextClip;

        }

        private (int, int) QueryFeature(Trajectory.Snippet currentSnippet)
        {
            List<CandidateFeature> candidateFeatures = new List<CandidateFeature>();
            Tuple<float, CandidateFeature> winnerFeature = new Tuple<float, CandidateFeature>(Mathf.Infinity, null);
            Pose currentPose = this._anim[this._currentAnimation].featureList[this._currentFeature].pose;
            float maxPosePositionDiff = 0;
            float maxPoseVelocityDiff = 0;

            // TODO remove
            this._fc.DrawPath(currentSnippet);

            // 1. Find the Clips with the most fitting Trajectories
            for (int i=0; i < this._anim.Count; i++)
            {
                for (int j = 0; j < this._anim[i].featureList.Count; j++)
                {
                    Feature feature = this._anim[i].featureList[j];

                    // Consider only active Frames (not on cooldown)
                    if (feature.cooldownTimer == 0)
                    {
                        // A. Add candidate Feature to the best candidates list
                        CandidateFeature candidateFeature = new CandidateFeature(
                            feature, currentSnippet.CalcDiff(feature.snippet), i, j
                            );
                        candidateFeatures.Add(candidateFeature);

                        // B. Sort candidates based on their diff
                        candidateFeatures.Sort(
                            (firstObj, secondObj) =>
                            {
                                return firstObj.trajectoryDiff.CompareTo(secondObj.trajectoryDiff);
                            }
                        );

                        // C. Keep only a predefined number of best candidates
                        if (candidateFeatures.Count <= 0)
                        {
                            Debug.LogError("Unable to find any Animation Frame to transition to");
                            return (0, 0);
                        }
                        else if (candidateFeatures.Count > CandidateFramesSize)
                        {
                            candidateFeatures.RemoveRange(CandidateFramesSize, candidateFeatures.Count - CandidateFramesSize);
                        }
                    }
                }
            }

            // 2. Compute the difference in Pose for each Clip (position and velocity)
            for (int i = 0; i < candidateFeatures.Count; i++)
            {
                (float posePositionDiff, float poseVelocityDiff) = currentPose.CalcDiff(candidateFeatures[i].feature.pose);
                candidateFeatures[i].posePositionDiff = posePositionDiff;
                candidateFeatures[i].poseVelocityDiff = poseVelocityDiff;

                // Keep the maximum values of the differences, in order to normalise
                maxPosePositionDiff = posePositionDiff > maxPosePositionDiff ?
                     posePositionDiff :
                     maxPosePositionDiff;

                maxPoseVelocityDiff = poseVelocityDiff > maxPoseVelocityDiff ?
                    poseVelocityDiff :
                    maxPoseVelocityDiff;

                // TODO remove
                this._fc.DrawAlternativePath(candidateFeatures[i].feature.snippet, i, candidateFeatures[i].trajectoryDiff);
            }

            // 3. Normalize and add differences
            for (int i=0; i < candidateFeatures.Count; i++)
            {
                candidateFeatures[i].posePositionDiff /= maxPosePositionDiff;
                candidateFeatures[i].poseVelocityDiff /= maxPoseVelocityDiff;

                float totalPostDiff = candidateFeatures[i].posePositionDiff + candidateFeatures[i].poseVelocityDiff;

                winnerFeature = winnerFeature.Item1 > totalPostDiff ?
                    new Tuple<float, CandidateFeature>(totalPostDiff, candidateFeatures[i]) :
                    winnerFeature;
            }

            Debug.Log("Starting animation: " + this._anim[winnerFeature.Item2.animationNum].animationName);

            // 4. Return the Feature's index
            return (winnerFeature.Item2.animationNum, winnerFeature.Item2.clipNum);
        }

        private void PutOnCooldown(Feature feature)
        {
            _onCooldown.Add(feature);
            feature.cooldownTimer = CooldownTime;
        }

        private void ReduceCooldowns()
        {
            // Traverse the List in reverse order to remove elements at the same time
            for (int i = _onCooldown.Count - 1; i >= 0; i--)
            {
                Feature feature = _onCooldown[i];

                // Reduce the timer by the amount af frames passed since last reduction
                feature.cooldownTimer = (feature.cooldownTimer > 0) ?
                    feature.cooldownTimer - 1 :
                    0;

                // When the counter reaches 0, remove the feature from the _onCooldown list
                if (feature.cooldownTimer == 0)
                {
                    _onCooldown.Remove(feature);
                }
            }
        }

        private class CandidateFeature
        {
            public Feature feature;
            public float trajectoryDiff;
            public float posePositionDiff;
            public float poseVelocityDiff;
            public int animationNum;
            public int clipNum;

            public CandidateFeature(Feature feature, float trajectoryDiff, int animationNum, int clipNum)
            {
                this.feature = feature;
                this.trajectoryDiff = trajectoryDiff;
                this.animationNum = animationNum;
                this.clipNum = clipNum;
                this.posePositionDiff = Mathf.Infinity;
                this.poseVelocityDiff = Mathf.Infinity;
        }
        }
    }
}
