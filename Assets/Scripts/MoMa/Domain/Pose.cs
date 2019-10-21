using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoMa
{
    public class Pose
    {
        // Position of limbs relative to the root
        public IDictionary<Bone.Type, Limb> limbDataDict = new Dictionary<Bone.Type, Limb>();

        public Pose(Frame frame)
        {
            foreach (Bone.Type bone in new Bone.Type[] { Bone.Type.hips, Bone.Type.leftFoot, Bone.Type.rightFoot, Bone.Type.leftHand, Bone.Type.rightHand} )
            {
                limbDataDict.Add(bone, new Limb(frame.boneDataDict[bone].position, frame.boneDataDict[bone].velocity));
            }
        }

        public float CalcDiff(Pose candidate)
        {
            float diff = 0f;

            foreach (Bone.Type bone in limbDataDict.Keys)
            {
                diff += Mathf.Pow(this.limbDataDict[bone].position.magnitude - candidate.limbDataDict[bone].position.magnitude, 2);
                diff += Mathf.Pow(this.limbDataDict[bone].velocity.magnitude - candidate.limbDataDict[bone].velocity.magnitude, 2);
            }

            return diff;
        }

        public class Limb
        {
            public Vector3 position;
            public Vector3 velocity;

            public Limb(Vector3 position, Vector3 velocity)
            {
                this.position = position;
                this.velocity = velocity;
            }
        }
    }
}
