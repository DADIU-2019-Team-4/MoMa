using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Animation
{
    // The frameList and the featureList are corresponding to one another (featureList[x] refers to frameList[x])
    public string animationName;
    public List<Frame> frameList;
    public List<Feature> featureList;

    public Animation(string animationName)
    {
        this.animationName = animationName;
        this.frameList = new List<Frame>();
        this.featureList = new List<Feature>();
    }

    public void addFrame(Frame newFrame)
    {
        frameList.Add(newFrame);
    }

    public void addFeature(Feature newFeature)
    {
        featureList.Add(newFeature);
    }
}
