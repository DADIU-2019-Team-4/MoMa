using UnityEngine;
using System.Collections;

namespace MoMa
{
    public class Pose
    {
        // Position of limbs relative to the root
        // TODO: Add hands and velocity (hands, feet and root!)
        public Vector3 leftFootPosition;
        public Vector3 rightFootPosition;
        public Vector3 leftHandPosition;
        public Vector3 rightHandPosition;

        public Pose(Frame frame)
        {
            this.leftFootPosition = frame.boneDataDict[Bone.Type.leftFoot].position;
            this.rightFootPosition = frame.boneDataDict[Bone.Type.rightFoot].position;
            this.leftHandPosition = frame.boneDataDict[Bone.Type.leftHand].position;
            this.rightHandPosition = frame.boneDataDict[Bone.Type.rightHand].position;
        }

        public float CalcDiff(Pose candidateFeature)
        {
            float diff = 0f;

            diff += Mathf.Pow((this.leftFootPosition - candidateFeature.leftFootPosition).magnitude, 2);
            diff += Mathf.Pow((this.rightFootPosition - candidateFeature.rightFootPosition).magnitude, 2);
            diff += Mathf.Pow((this.leftHandPosition - candidateFeature.leftHandPosition).magnitude, 2);
            diff += Mathf.Pow((this.rightHandPosition - candidateFeature.rightHandPosition).magnitude, 2);

            return diff;
        }

        }
}
