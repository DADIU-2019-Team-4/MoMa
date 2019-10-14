using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packer
{
    // It is assumed that the Motion Capture csv files will be in the Resources/MoCapData folder
    private const string DefaultDir = "MoCapData";
    public const int PastFrames = 5;
    public const int FutureFrames = 10;

    private string animationName;
    private string filename;
    public int skippedFrames;

    public Packer(string animationName, string filename, int skippedFrames)
    {
        this.animationName = animationName;
        this.filename = filename;
        this.skippedFrames = skippedFrames;
    }

    public Animation Pack()
    {
        // Initialize an empty Animation
        Animation anim = new Animation(animationName);

        // Load the raw Animation data from the specified file
        this.LoadRawAnimationFromFile(anim, filename);

        // Compute the feature Frames
        this.ComputeFeatures(anim);

        return anim;
    }

    #region Private Utilities
    private void LoadRawAnimationFromFile(Animation anim, string filename)
    {
        Debug.Log("Loading Motion from file \"" + filename + "\"");

        // Open given file and split the lines
        TextAsset moCapAsset = Resources.Load<TextAsset>(DefaultDir + "/" + filename);

        if (moCapAsset == null)
        {
            Debug.LogError("Unable to load MoCap file: " + filename);
            return;
        }

        string[] data = moCapAsset.text.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        );

        // Read Bone names (Frame 0)
        var bonesLine = data[0];
        var bones = bonesLine.Split(',');

        // Every SkippedFrames, add a frame to the current Motion (excl. first line (Titles) and last line (empty))
        for (var currentFrame = 1; currentFrame < data.Length - 1; currentFrame += this.skippedFrames)
        {
            // Pass the current timestamp to the Frame constructor
            var dataFrame = data[currentFrame].Split(',');
            Frame newFrame = new Frame(float.Parse(dataFrame[0]));

            // Every bone has data in 7 columns (3 position, 4 rotation)
            foreach (BoneType bt in Enum.GetValues(typeof(BoneType)))
            {
                int off = (int)bt * 7;
                BoneData currentBoneData = new BoneData(
                    bt,
                    float.Parse(dataFrame[off + 1]),    // Start from 1 because of the Timestamp on 0
                    float.Parse(dataFrame[off + 2]),
                    float.Parse(dataFrame[off + 3]),
                    float.Parse(dataFrame[off + 4]),
                    float.Parse(dataFrame[off + 5]),
                    float.Parse(dataFrame[off + 6]),
                    float.Parse(dataFrame[off + 7])
                );

                // Create the new Frame
                newFrame.boneDataDict.Add(bt, currentBoneData);
            }

            // Add the new Frame to the current Motion
            anim.addFrame(newFrame);
        }

        Debug.Log(
            anim.frameList.Count == 0 ?
            "Motion contains 0 frames" :
            "Motion loaded successfully"
            );

        // Unload Asset to free Memory
        Resources.UnloadAsset(moCapAsset);
    }

    private void ComputeFeatures(Animation anim)
    {
        int frameNum = 0;
        Trajectory trajectory;

        // 0. Find fitted trajectory for the whole motion
        trajectory = ComputeFittedTrajectory(anim);

        // 1. Add empty features for the pastFrames padding
        while (frameNum < PastFrames)
        {
            anim.addFeature(null);
            frameNum++;
        }

        // 2. Compute Features for all inbetween Frames
        while (frameNum < anim.frameList.Count - FutureFrames)
        {
            // Get current Frame
            Frame frame = anim.frameList[frameNum];

            // Compute the Trajectory relative to the root
            Vector2 rootOffset = new Vector2(frame.boneDataDict[BoneType.hips].position.x, frame.boneDataDict[BoneType.hips].position.z);
            Quaternion rootRotation = frame.boneDataDict[BoneType.hips].rotation;
            Trajectory localTrajectory = ComputeLocalTrajectory(trajectory, rootOffset, rootRotation, frameNum);

            // Built Feature
            Feature feature = new Feature();
            feature.localTrajectory = localTrajectory;

            // Add Feature
            anim.addFeature(feature);
            frameNum++;
        }

        // 3. Add empty features for the futureFrames padding
        while (frameNum < anim.frameList.Count)
        {
            anim.addFeature(null);
            frameNum++;
        }
    }

    // TODO: This is a dummy. Implement the actual fitting functionality
    private Trajectory ComputeFittedTrajectory(Animation anim)
    {
        Trajectory fittedTrajectory = new Trajectory();

        for (int frameNum = 0; frameNum<anim.frameList.Count; frameNum++)
        {
            TrajectoryPoint point = new TrajectoryPoint(
                new Vector2(
                    anim.frameList[frameNum].boneDataDict[BoneType.hips].position.x,
                    anim.frameList[frameNum].boneDataDict[BoneType.hips].position.z
                    ));
            fittedTrajectory.PushFutureFrame(point);
        }

        return fittedTrajectory;
    }

    // TODO: The way the trajectory is computed can surely be optimized
    private Trajectory ComputeLocalTrajectory(Trajectory trajectory, Vector2 rootOffset, Quaternion rootRotation, int presentFrameNum)
    {
        Trajectory localTrajectory = new Trajectory();

        // 1. Compute local past
        for (int frameNum = presentFrameNum-1; frameNum > presentFrameNum - PastFrames - 1; frameNum--)
        {
            // Compute the position of the point relative to the present point
            TrajectoryPoint point = new TrajectoryPoint(trajectory.points[frameNum].point);
            point.point -= rootOffset;
            point.point.Rotate(Quaternion.Inverse(rootRotation).eulerAngles.z); // TODO: This might be z instead of x
            localTrajectory.PushPastFrame(point);
        }

        // 2. Compute local future
        for (int frameNum = presentFrameNum; frameNum < presentFrameNum + FutureFrames; frameNum++)
        {
            // Compute the position of the point relative to the present point
            TrajectoryPoint point = new TrajectoryPoint(trajectory.points[frameNum].point);
            point.point -= rootOffset;
            point.point.Rotate(Quaternion.Inverse(rootRotation).eulerAngles.z); // TODO: This might be z instead of x
            localTrajectory.PushFutureFrame(point);
        }

        return localTrajectory;
    }
    #endregion
}
