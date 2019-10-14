using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Frame
{
    public float timestamp;
    public IDictionary<BoneType, BoneData> boneDataDict;

    public Frame(float timestamp)
    {
        this.timestamp = timestamp;
        this.boneDataDict = new Dictionary<BoneType, BoneData>();
    }
}
