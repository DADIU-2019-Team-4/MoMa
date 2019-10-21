using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoMa
{
    [System.Serializable]
    public class Frame
    {
        public float timestamp;
        public IDictionary<Bone.Type, Bone.Data> boneDataDict;

        public Frame(float timestamp)
        {
            this.timestamp = timestamp;
            this.boneDataDict = new Dictionary<Bone.Type, Bone.Data>();
        }
    }
}