using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MoMa
{
    [RequireComponent(typeof(MovementController))]
    public class AnimationController : MonoBehaviour
    {
        private const string PlayerTag = "Player"; // The Tag that the Player's GameObject has in the game

        private IDictionary<Bone.Type, Transform> _bones;
        private RuntimeComponent _rc;
        private MovementController _mc;
        private Follower _follower;
        private Animation.Clip _clip = new Animation.Clip();
        private Trajectory _trajectory = new Trajectory();

        // Find and attach to the Model
        void Start()
        {
            // Find Model Object and attach to it
            this._mc = this.GetComponent(typeof(MovementController)) as MovementController;
            GameObject model = GameObject.FindWithTag(PlayerTag);

            if (model == null || this._mc == null)
            {
                Debug.LogError("Unable to find Model or MovementController");
                throw new Exception("Unable to find Model or MovementController");
            }

            AttachToTransforms(model);

            // Initialize RuntimeComponent
            this._rc = new RuntimeComponent();

            // If Follower exists, attach to it
            this._follower = this.GetComponent(typeof(Follower)) as Follower;

            // Initialize Trajectory's past to the initial position
            for (int i = 0; i < Trajectory.Snippet.PastPoints; i++)
            {
                this._trajectory.points.Add(new Trajectory.Point(0f, 0f));
            }
        }

        public void Update()
        {
            Debug.Log("Update()");

            // If the current Clip is over, request a new one
            if (this._clip.isOver())
            {
                Trajectory.Snippet snippet = GetCurrentSnippet();
                this._clip = this._rc.QueryClip(snippet);
                this._follower.Draw(snippet);
            }

            // Play the next Frame of the Clip
            Frame frame = this._clip.Step();

            foreach (Bone.Type bone in this._bones.Keys)
            {
                // This changes the rig to the one used by Rokoko
                //this._bones[bone].SetPositionAndRotation(curFrame.boneDataDict[bone].position, curFrame.boneDataDict[bone].rotation);

                // This keeps the rig's proportions
                this._bones[bone].rotation = frame.boneDataDict[bone].rotation;
            }
        }

        private Trajectory.Snippet GetCurrentSnippet()
        {
            Trajectory.Snippet snippet;
            int futureFramesNumber = Feature.FramesPerPoint * Trajectory.Snippet.FuturePoints;

            // Get simulated future
            List<Vector3> futureFrames = this._mc.GetFuture(futureFramesNumber);

            // Convert the (many) Frames to (few) Point and add them to the Trajectory
            for (int i=0; i < Trajectory.Snippet.FuturePoints; i++)
            {
                Trajectory.Point point = Trajectory.Point.getMedianPoint(futureFrames.GetRange(i * Feature.FramesPerPoint, Feature.FramesPerPoint));
                this._trajectory.points.Add(point);
            }

            // Compute the Trajectory Snippet
            snippet = this._trajectory.GetSnippet(
                Trajectory.Snippet.PastPoints,
                this._bones[Bone.Type.hips].position,
                this._bones[Bone.Type.hips].rotation
                );

            // Remove future Points from Trajectory
            this._trajectory.points.RemoveRange(Trajectory.Snippet.PastPoints, Trajectory.Snippet.FuturePoints);

            return snippet;
        }

        #region Rig-attaching methods
        private void AttachToTransforms(GameObject model)
        {
            // Create an empty Dictionary
            this._bones = new Dictionary<Bone.Type, Transform>();

            #region Load all _bones
            // Load core
            this._bones.Add(Bone.Type.root, model.transform.GetChild(0));
            this._bones.Add(Bone.Type.hips, this._bones[Bone.Type.root].GetChild(0));

            // Load left foot
            this._bones.Add(Bone.Type.leftThigh, this._bones[Bone.Type.hips].GetChild(0));
            this._bones.Add(Bone.Type.leftShin, this._bones[Bone.Type.leftThigh].GetChild(0));
            this._bones.Add(Bone.Type.leftFoot, this._bones[Bone.Type.leftShin].GetChild(0));
            this._bones.Add(Bone.Type.leftToe, this._bones[Bone.Type.leftFoot].GetChild(0));
            this._bones.Add(Bone.Type.leftToeTip, this._bones[Bone.Type.leftToe].GetChild(0));

            // Load right foot
            this._bones.Add(Bone.Type.rightThigh, this._bones[Bone.Type.hips].GetChild(1));
            this._bones.Add(Bone.Type.rightShin, this._bones[Bone.Type.rightThigh].GetChild(0));
            this._bones.Add(Bone.Type.rightFoot, this._bones[Bone.Type.rightShin].GetChild(0));
            this._bones.Add(Bone.Type.rightToe, this._bones[Bone.Type.rightFoot].GetChild(0));
            this._bones.Add(Bone.Type.rightToeTip, this._bones[Bone.Type.rightToe].GetChild(0));

            // Load spine
            this._bones.Add(Bone.Type.spine1, this._bones[Bone.Type.hips].GetChild(2));
            this._bones.Add(Bone.Type.spine2, this._bones[Bone.Type.spine1].GetChild(0));
            this._bones.Add(Bone.Type.spine3, this._bones[Bone.Type.spine2].GetChild(0));
            this._bones.Add(Bone.Type.spine4, this._bones[Bone.Type.spine3].GetChild(0));

            // Load head
            this._bones.Add(Bone.Type.neck, this._bones[Bone.Type.spine4].GetChild(1));
            this._bones.Add(Bone.Type.head, this._bones[Bone.Type.neck].GetChild(0));

            // Load left arm
            this._bones.Add(Bone.Type.leftShoulder, this._bones[Bone.Type.spine4].GetChild(0));
            this._bones.Add(Bone.Type.leftArm, this._bones[Bone.Type.leftShoulder].GetChild(0));
            this._bones.Add(Bone.Type.leftForeArm, this._bones[Bone.Type.leftArm].GetChild(0));
            this._bones.Add(Bone.Type.leftHand, this._bones[Bone.Type.leftForeArm].GetChild(0));

            // Load right arm
            this._bones.Add(Bone.Type.rightShoulder, this._bones[Bone.Type.spine4].GetChild(2));
            this._bones.Add(Bone.Type.rightArm, this._bones[Bone.Type.rightShoulder].GetChild(0));
            this._bones.Add(Bone.Type.rightForeArm, this._bones[Bone.Type.rightArm].GetChild(0));
            this._bones.Add(Bone.Type.rightHand, this._bones[Bone.Type.rightForeArm].GetChild(0));

            #endregion
        }

        private void AttachToNewtonTransforms(GameObject model)
        {
            // Create an empty Dictionary
            this._bones = new Dictionary<Bone.Type, Transform>();

            #region Load all _bones
            // Load core
            this._bones.Add(Bone.Type.root, model.transform);
            this._bones.Add(Bone.Type.hips, this._bones[Bone.Type.root].GetChild(0));

            // Load left foot
            this._bones.Add(Bone.Type.leftThigh, this._bones[Bone.Type.hips].GetChild(0));
            this._bones.Add(Bone.Type.leftShin, this._bones[Bone.Type.leftThigh].GetChild(0));
            this._bones.Add(Bone.Type.leftFoot, this._bones[Bone.Type.leftShin].GetChild(0));
            this._bones.Add(Bone.Type.leftToe, this._bones[Bone.Type.leftFoot].GetChild(0));
            this._bones.Add(Bone.Type.leftToeTip, this._bones[Bone.Type.leftToe].GetChild(0));

            // Load right foot
            this._bones.Add(Bone.Type.rightThigh, this._bones[Bone.Type.hips].GetChild(1));
            this._bones.Add(Bone.Type.rightShin, this._bones[Bone.Type.rightThigh].GetChild(0));
            this._bones.Add(Bone.Type.rightFoot, this._bones[Bone.Type.rightShin].GetChild(0));
            this._bones.Add(Bone.Type.rightToe, this._bones[Bone.Type.rightFoot].GetChild(0));
            this._bones.Add(Bone.Type.rightToeTip, this._bones[Bone.Type.rightToe].GetChild(0));

            // Load spine
            this._bones.Add(Bone.Type.spine1, this._bones[Bone.Type.hips].GetChild(2));
            this._bones.Add(Bone.Type.spine2, this._bones[Bone.Type.spine1].GetChild(0));
            this._bones.Add(Bone.Type.spine3, this._bones[Bone.Type.spine2].GetChild(0));
            this._bones.Add(Bone.Type.spine4, this._bones[Bone.Type.spine3].GetChild(0));

            // Load head
            this._bones.Add(Bone.Type.neck, this._bones[Bone.Type.spine4].GetChild(1));
            this._bones.Add(Bone.Type.head, this._bones[Bone.Type.neck].GetChild(0));

            // Load left arm
            this._bones.Add(Bone.Type.leftShoulder, this._bones[Bone.Type.spine4].GetChild(0));
            this._bones.Add(Bone.Type.leftArm, this._bones[Bone.Type.leftShoulder].GetChild(0));
            this._bones.Add(Bone.Type.leftForeArm, this._bones[Bone.Type.leftArm].GetChild(0));
            this._bones.Add(Bone.Type.leftHand, this._bones[Bone.Type.leftForeArm].GetChild(0));

            // Load right arm
            this._bones.Add(Bone.Type.rightShoulder, this._bones[Bone.Type.spine4].GetChild(2));
            this._bones.Add(Bone.Type.rightArm, this._bones[Bone.Type.rightShoulder].GetChild(0));
            this._bones.Add(Bone.Type.rightForeArm, this._bones[Bone.Type.rightArm].GetChild(0));
            this._bones.Add(Bone.Type.rightHand, this._bones[Bone.Type.rightForeArm].GetChild(0));

            #endregion
        }
        #endregion
    }
}