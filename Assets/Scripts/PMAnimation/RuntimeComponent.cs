using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RuntimeComponent : MonoBehaviour
{
    private const float RecalculationThreshold = 0.5f; // The maximum diff of two Trajectories before recalculating the Animation
    private const int CooldownTime = 5; // Number of frames that a Frame is on cooldown after being played
    private const int CandidateFramesSize = 20; // Number of candidate frames for a transition (tradeoff: fidelity/speed)
    private const string PlayerTag = "Player"; // The Tag that the Player's GameObject has in the game

    private GameObject player;
    private Animation anim;
    private IDictionary<BoneType, Transform> bones;
    private List<Feature> onCooldown = new List<Feature>();
    private Trajectory currentTrajectory;
    private int currentFrame = 0;
    private int intervalTimer = 0;  // Updates on every Update() and resets when it reaches Interval. Then QueryNewFrame() is called

    void Start()
    {
        // Find Player's Object and attach to it
        this.player = GameObject.FindWithTag(PlayerTag);

        if (this.player == null)
        {
            Debug.LogError("Unable to find Player");
            throw new Exception("Unable to find Player");
        }
        
        AttachToTransforms();
        //AttachToNewtonTransforms();

        // TODO: This should happen offline. Instead we only need to open its result
        //Packer p = new Packer("throwing", "take-1_DEFAULT_C36");
        Packer p = new Packer("throwing", "Sample project_scene-1_WalkSimple_DEFAULT_E46", 10);
        this.anim = p.Pack();
    }

    public void Update()
    {
        Debug.Log("Update()");

        // 1. Get the desired Trajectory
        currentTrajectory = GetCurrentTrajectory();
        
        // 2. If needed, recalculate Animation (Trajectories diverge, end of Animation reached, input changed the Trajectory)
        if (TrajectoryDiff(currentTrajectory, anim.featureList[currentFrame].localTrajectory) > RecalculationThreshold)
        {
            Debug.Log("Interval complete; Querying new Frame");
            QueryNewFrame();
            intervalTimer = 0;
        }

        // 3. Play the current Frame of the Animation
        AdvanceAnimation();
        intervalTimer++;
    }

    private Trajectory GetCurrentTrajectory()
    {
        //Trajectory currentTrajectory = this.player.GetComponent<SimplisticEmulator>().getTrajectory();
        return null;
    }

    private void QueryNewFrame()
    {
        Feature currentFeature = this.anim.featureList[this.currentFrame];
        // TODO: Instead of searching a list every time, implement some kind of SortedList
        List<Tuple<float, Feature, int>> bestCandidateFeature = new List<Tuple<float, Feature, int>>();
        Tuple<float, Feature, int> winnerCandidate;
        float minPoseDiff;

        // 1. Update cooldowns
        ReduceCooldowns(this.intervalTimer);

        // 2. Search Feature list for candidate Frames (comparing Trajectories)
        for (int offset = Packer.PastFrames; offset < this.anim.featureList.Count - Packer.FutureFrames; offset++)
        {
            // Get Feature for the current Frame
            Feature candidateFeature = this.anim.featureList[offset];

            // Consider only active Frames (not on cooldown)
            if (candidateFeature.cooldownTimer == 0)
            {
                // A. Find diff to current 
                float diff = TrajectoryDiff(currentTrajectory, candidateFeature.localTrajectory);

                // B. Add candidate Feature to the best candidates list
                bestCandidateFeature.Add(Tuple.Create(diff, candidateFeature, offset));

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
                    return;
                }
                else if (bestCandidateFeature.Count > CandidateFramesSize)
                {
                    bestCandidateFeature.GetRange(0, CandidateFramesSize);
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
        minPoseDiff = PoseDiff(winnerCandidate.Item2, currentFeature);

        foreach (Tuple<float, Feature, int>  currentCandidate in bestCandidateFeature)
        {
            float currentPoseDiff = PoseDiff(winnerCandidate.Item2, currentFeature);

            if (currentPoseDiff < minPoseDiff)
            {
                minPoseDiff = currentPoseDiff;
                winnerCandidate = currentCandidate;
            }
        }

        // 4. Tranition to the new Frame
        this.currentFrame = winnerCandidate.Item3;
    }

    private void AdvanceAnimation()
    {
        Frame curFrame = anim.frameList[currentFrame];
        Feature curFeature = anim.featureList[currentFrame];

        foreach (BoneType bone in this.bones.Keys)
        {
            // This changes the rig to the one used by Rokoko
            //this.bones[bone].SetPositionAndRotation(curFrame.boneDataDict[bone].position, curFrame.boneDataDict[bone].rotation);

            // This keeps the rig's proportions
            this.bones[bone].rotation = curFrame.boneDataDict[bone].rotation;
        }

        // Set current Frame on cooldown (if current Frame is not one padding ones (at the start and at the end of the Animation clip))
        if (curFeature != null)
        {
            onCooldown.Add(curFeature);
            curFeature.cooldownTimer = CooldownTime;
        }

        // Update current Frame number
        currentFrame = (currentFrame + 1) % anim.frameList.Count;
    }

    private float TrajectoryDiff(Trajectory currentTrajectory, Trajectory candidateTrajectory)
    {
        float diff = 0f;

        // No weights
        for (int i = 0; i < currentTrajectory.points.Count; ++i)
            diff += Mathf.Pow(
                currentTrajectory.points[i].point.magnitude -
                candidateTrajectory.points[i].point.magnitude, 2);

        // Using weights
        //float[] weights = new float[currentFeature.localTrajectory.points.Count];
        //for (int i = 0; i < currentFeature.localTrajectory.points.Count; ++i)
        //    diff += weights[i] * Mathf.Pow(currentFeature.localTrajectory.points[i].point.magnitude -
        //        candidateFeature.localTrajectory.points[i].point.magnitude, 2);

        return diff;
    }

    private float PoseDiff(Feature currentFeature, Feature candidateFeature)
    {
        return 0f;
    }

    private void ReduceCooldowns(int value)
    {
        // Traverse thee List in reverse order to remove elements at the same time
        for (int i = onCooldown.Count - 1; i >= 0; i--)
        {
            Feature feature = onCooldown[i];

            // Reduce the timer by the amount af frames passed since last reduction
            feature.cooldownTimer = (feature.cooldownTimer > value) ?
                feature.cooldownTimer - value :
                0;

            // When the counter reaches 0, remove the feature from the onCooldown list
            if (feature.cooldownTimer == 0)
            {
                onCooldown.Remove(feature);
            }
        }
    }

    #region Rig-attaching methods
    private void AttachToTransforms()
    {
        // Create an empty Dictionary
        this.bones = new Dictionary<BoneType, Transform>();

        #region Load all bones
        // Load core
        this.bones.Add(BoneType.root, this.player.transform.GetChild(0));
        this.bones.Add(BoneType.hips, this.bones[BoneType.root].GetChild(0));

        // Load left foot
        this.bones.Add(BoneType.leftThigh, this.bones[BoneType.hips].GetChild(0));
        this.bones.Add(BoneType.leftShin, this.bones[BoneType.leftThigh].GetChild(0));
        this.bones.Add(BoneType.leftFoot, this.bones[BoneType.leftShin].GetChild(0));
        this.bones.Add(BoneType.leftToe, this.bones[BoneType.leftFoot].GetChild(0));
        this.bones.Add(BoneType.leftToeTip, this.bones[BoneType.leftToe].GetChild(0));

        // Load right foot
        this.bones.Add(BoneType.rightThigh, this.bones[BoneType.hips].GetChild(1));
        this.bones.Add(BoneType.rightShin, this.bones[BoneType.rightThigh].GetChild(0));
        this.bones.Add(BoneType.rightFoot, this.bones[BoneType.rightShin].GetChild(0));
        this.bones.Add(BoneType.rightToe, this.bones[BoneType.rightFoot].GetChild(0));
        this.bones.Add(BoneType.rightToeTip, this.bones[BoneType.rightToe].GetChild(0));

        // Load spine
        this.bones.Add(BoneType.spine1, this.bones[BoneType.hips].GetChild(2));
        this.bones.Add(BoneType.spine2, this.bones[BoneType.spine1].GetChild(0));
        this.bones.Add(BoneType.spine3, this.bones[BoneType.spine2].GetChild(0));
        this.bones.Add(BoneType.spine4, this.bones[BoneType.spine3].GetChild(0));

        // Load head
        this.bones.Add(BoneType.neck, this.bones[BoneType.spine4].GetChild(1));
        this.bones.Add(BoneType.head, this.bones[BoneType.neck].GetChild(0));

        // Load left arm
        this.bones.Add(BoneType.leftShoulder, this.bones[BoneType.spine4].GetChild(0));
        this.bones.Add(BoneType.leftArm, this.bones[BoneType.leftShoulder].GetChild(0));
        this.bones.Add(BoneType.leftForeArm, this.bones[BoneType.leftArm].GetChild(0));
        this.bones.Add(BoneType.leftHand, this.bones[BoneType.leftForeArm].GetChild(0));

        // Load right arm
        this.bones.Add(BoneType.rightShoulder, this.bones[BoneType.spine4].GetChild(2));
        this.bones.Add(BoneType.rightArm, this.bones[BoneType.rightShoulder].GetChild(0));
        this.bones.Add(BoneType.rightForeArm, this.bones[BoneType.rightArm].GetChild(0));
        this.bones.Add(BoneType.rightHand, this.bones[BoneType.rightForeArm].GetChild(0));

        #endregion  
    }

    private void AttachToNewtonTransforms()
    {
        // Create an empty Dictionary
        this.bones = new Dictionary<BoneType, Transform>();

        #region Load all bones
        // Load core
        this.bones.Add(BoneType.root, this.player.transform);
        this.bones.Add(BoneType.hips, this.bones[BoneType.root].GetChild(0));

        // Load left foot
        this.bones.Add(BoneType.leftThigh, this.bones[BoneType.hips].GetChild(0));
        this.bones.Add(BoneType.leftShin, this.bones[BoneType.leftThigh].GetChild(0));
        this.bones.Add(BoneType.leftFoot, this.bones[BoneType.leftShin].GetChild(0));
        this.bones.Add(BoneType.leftToe, this.bones[BoneType.leftFoot].GetChild(0));
        this.bones.Add(BoneType.leftToeTip, this.bones[BoneType.leftToe].GetChild(0));

        // Load right foot
        this.bones.Add(BoneType.rightThigh, this.bones[BoneType.hips].GetChild(1));
        this.bones.Add(BoneType.rightShin, this.bones[BoneType.rightThigh].GetChild(0));
        this.bones.Add(BoneType.rightFoot, this.bones[BoneType.rightShin].GetChild(0));
        this.bones.Add(BoneType.rightToe, this.bones[BoneType.rightFoot].GetChild(0));
        this.bones.Add(BoneType.rightToeTip, this.bones[BoneType.rightToe].GetChild(0));

        // Load spine
        this.bones.Add(BoneType.spine1, this.bones[BoneType.hips].GetChild(2));
        this.bones.Add(BoneType.spine2, this.bones[BoneType.spine1].GetChild(0));
        this.bones.Add(BoneType.spine3, this.bones[BoneType.spine2].GetChild(0));
        this.bones.Add(BoneType.spine4, this.bones[BoneType.spine3].GetChild(0));

        // Load head
        this.bones.Add(BoneType.neck, this.bones[BoneType.spine4].GetChild(1));
        this.bones.Add(BoneType.head, this.bones[BoneType.neck].GetChild(0));

        // Load left arm
        this.bones.Add(BoneType.leftShoulder, this.bones[BoneType.spine4].GetChild(0));
        this.bones.Add(BoneType.leftArm, this.bones[BoneType.leftShoulder].GetChild(0));
        this.bones.Add(BoneType.leftForeArm, this.bones[BoneType.leftArm].GetChild(0));
        this.bones.Add(BoneType.leftHand, this.bones[BoneType.leftForeArm].GetChild(0));

        // Load right arm
        this.bones.Add(BoneType.rightShoulder, this.bones[BoneType.spine4].GetChild(2));
        this.bones.Add(BoneType.rightArm, this.bones[BoneType.rightShoulder].GetChild(0));
        this.bones.Add(BoneType.rightForeArm, this.bones[BoneType.rightArm].GetChild(0));
        this.bones.Add(BoneType.rightHand, this.bones[BoneType.rightForeArm].GetChild(0));

        #endregion  
    }
    #endregion
}
