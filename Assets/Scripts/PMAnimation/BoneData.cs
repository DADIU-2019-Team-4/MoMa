using UnityEngine;
using System.Collections;

[System.Serializable]
public class BoneData
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 velocity;

    //Debug
    public BoneType boneType;
    public int boneIndex;
    public Vector3 position;
    public Quaternion rotation;

    public BoneData(BoneType boneType, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
    {
        this.boneType = boneType;
        this.position = new Vector3(posX, posY, posZ);
        this.rotation = new Quaternion(rotX, rotY, rotZ, rotW);

        // TODO : Set the velocity to some high order polynomial of the actual velocity graph
        this.velocity = new Vector3();
    }
}
