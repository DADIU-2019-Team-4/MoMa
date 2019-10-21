using UnityEngine;
using System.Collections;

namespace MoMa
{
    public class Pose
    {
        // Position of limbs relative to the root
        // TODO: Add hands and velocity (hands, feet and root!)
        public Vector3 leftFootRelativePosition;
        public Vector3 rightFootRelativePosition;

        public Pose(Frame frame)
        {
            this.leftFootRelativePosition = frame.boneDataDict[Bone.Type.leftFoot].position;
            this.rightFootRelativePosition = frame.boneDataDict[Bone.Type.rightFoot].position;
        }

        public float CalcDiff(Pose candidateFeature)
        {
            float diff = 0f;

            diff += Mathf.Pow((this.leftFootRelativePosition - candidateFeature.leftFootRelativePosition).magnitude, 2);
            diff += Mathf.Pow((this.rightFootRelativePosition - candidateFeature.rightFootRelativePosition).magnitude, 2);

            return diff;
        }

        }
}
