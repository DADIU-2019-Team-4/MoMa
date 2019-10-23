using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoMa
{
    public class RuntimeComponent
    {
        private const float RecalculationThreshold = 0.7f; // The maximum diff of two Trajectories before recalculating the Animation
        private const int CooldownTime = 5; // Number of frames that a Frame is on cooldown after being played
        private const int CandidateFramesSize = 20; // Number of candidate frames for a transition (tradeoff: fidelity/speed)

        private List<Animation> _anim = new List<Animation>();
        private List<Feature> _onCooldown = new List<Feature>();
        private int _currentAnimation = 0;
        private int _currentFeature = 0;

        public RuntimeComponent()
        {
            // TODO: This should happen offline. Instead we only need to open its result
            //this._anim = Packer.Pack("throwing", "MoCapData", "take-1_DEFAULT_C36", 5, 10, 10);
            //this._anim = Packer.Pack("throwing", "MoCapData", "Sample project_scene-1_WalkSimple_DEFAULT_E46");

            this._anim.Add(Packer.Pack("walk", "MoCapData", "walk_DEFAULT_FIX"));
            this._anim.Add(Packer.Pack("jog", "MoCapData", "jog3_DEFAULT_FIX"));
            //this._anim.Add(Packer.Pack("circle_left", "MoCapData", "circle_left_DEFAULT_FIX"));
            //this._anim.Add(Packer.Pack("circle_right", "MoCapData", "circle_right_DEFAULT_FIX"));
        }

        public Animation.Clip QueryClip(Trajectory.Snippet currentSnippet)
        {
            // 1. Reduce cooldowns (after the previous Clip has finished)
            ReduceCooldowns();

            // 2. Check if the next Clip is fitting (or the first one, if we reach the ennd)
            this._currentFeature = (this._currentFeature + 1) % this._anim[this._currentAnimation].featureList.Count;

            float diff = currentSnippet.CalcDiff(this._anim[this._currentAnimation].featureList[this._currentFeature].snippet);

            if (diff > RecalculationThreshold)
            {
                Debug.Log("Recalculating");
                (this._currentAnimation, this._currentFeature) = QueryFeature(currentSnippet);
            }
            else
            {
                Debug.Log("Not Recalculating");
            }

            // 3. Construct the Clip and return it
            return new Animation.Clip(
                this._anim[this._currentAnimation].frameList.GetRange(
                    this._anim[this._currentAnimation].featureList[this._currentFeature].frameNum, 
                    Feature.PointsPerFeature * Feature.FramesPerPoint
                    )
                );
        }

        private (int, int) QueryFeature(Trajectory.Snippet currentSnippet)
        {
            // TODO: Instead of searching a list every time, implement some kind of SortedList
            List<Tuple<float, Feature, int, int>> bestCandidateFeature = new List<Tuple<float, Feature, int, int>>();
            Tuple<float, Feature, int, int> winnerCandidate;
            Pose currentPose = this._anim[this._currentAnimation].featureList[this._currentFeature].pose;
            float minPoseDiff = 0f;

            // 1. Search each Animation
            for (int i=0; i < this._anim.Count; i++)
            {
                Animation anim = this._anim[i];

                // 2. Search Feature list for candidate Frames (comparing Trajectories)
                for (int j = 0; j < anim.featureList.Count; j++)
                {
                    Feature candidateFeature = anim.featureList[j];

                    // Consider only active Frames (not on cooldown)
                    if (candidateFeature.cooldownTimer == 0)
                    {
                        // A. Find diff to current 
                        float diff = currentSnippet.CalcDiff(candidateFeature.snippet);

                        // B. Add candidate Feature to the best candidates list
                        bestCandidateFeature.Add(Tuple.Create(diff, candidateFeature, i, j));

                        // C. Sort candidates based on their diff
                        bestCandidateFeature.Sort(
                            (firstObj, secondObj) =>
                            {
                                return firstObj.Item1.CompareTo(secondObj.Item1);
                            }
                        );

                        // D. Keep only a predefined number of best candidates
                        if (bestCandidateFeature.Count <= 0)
                        {
                            Debug.LogError("Unable to find any Animation Frame to transition to");
                            return (0, 0);
                        }
                        else if (bestCandidateFeature.Count > CandidateFramesSize)
                        {
                            bestCandidateFeature.RemoveRange(CandidateFramesSize, bestCandidateFeature.Count - CandidateFramesSize);
                        }
                    }
                }
            }

            // 3. Search candidate Frames for the one with the most fitting pose
            if (bestCandidateFeature.Count <= 0)
            {
                Debug.LogError("Unable to find any Frame to transition to");
                throw new Exception("Unable to find any Frame to transition to");
            }

            winnerCandidate = bestCandidateFeature[0];
            minPoseDiff = currentPose.CalcDiff(winnerCandidate.Item2.pose);

            foreach (Tuple<float, Feature, int, int> currentCandidate in bestCandidateFeature)
            {
                float currentPoseDiff = currentPose.CalcDiff(winnerCandidate.Item2.pose);

                if (currentPoseDiff < minPoseDiff)
                {
                    minPoseDiff = currentPoseDiff;
                    winnerCandidate = currentCandidate;
                }
            }

            //Debug.Log("Starting animation: " + this._anim[winnerCandidate.Item3].animationName + " #" + winnerCandidate.Item4 + "/" + this._anim[this._currentAnimation].featureList.Count);
            Debug.Log("Starting animation: " + this._anim[winnerCandidate.Item3].animationName);

            // 4. Return the Feature's index
            return (winnerCandidate.Item3, winnerCandidate.Item4);
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
    }
}
