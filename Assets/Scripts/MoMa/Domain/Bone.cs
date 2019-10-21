using UnityEngine;
using System.Collections;

public class Bone
{
    // This is the Rokoko Newton format (not Mixamo)
    public enum Type
    {
        root,
        hips,
        leftThigh,
        leftShin,
        leftFoot,
        leftToe,
        leftToeTip,
        rightThigh,
        rightShin,
        rightFoot,
        rightToe,
        rightToeTip,
        spine1,
        spine2,
        spine3,
        spine4,
        leftShoulder,
        leftArm,
        leftForeArm,
        leftHand,
        neck,
        head,
        rightShoulder,
        rightArm,
        rightForeArm,
        rightHand
    }

    [System.Serializable]
    public class Data
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        //Debug
        public Type boneType;

        public Data(Type boneType, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
        {
            this.boneType = boneType;
            this.position = new Vector3(posX, posY, posZ);
            this.rotation = new Quaternion(rotX, rotY, rotZ, rotW);

            // TODO : Set the velocity to some high order polynomial of the actual velocity graph
            this.velocity = new Vector3();
        }
    }
}
