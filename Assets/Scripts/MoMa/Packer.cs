using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    public class Packer
    {
        public static Animation Pack(string animationName, string directory, string filename)
        {
            // Initialize an empty Animation
            Animation anim = new Animation(animationName);

            // Load the raw Animation data from the specified file
            Packer.LoadRawAnimationFromFile(anim, directory, filename);

            // Compute the velocity of every Bone in every Frame
            ComputeVelocities(anim);

            // Compute the feature Frames
            anim.ComputeFeatures();

            return anim;
        }

        private static void LoadRawAnimationFromFile(Animation anim, string directory, string filename)
        {
            Debug.Log("Loading Motion from file \"" + filename + "\"");

            // Open given file and split the lines
            TextAsset moCapAsset = Resources.Load<TextAsset>(directory + "/" + filename);

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

            // Add all frames to the current Animation (excl. first line (Titles) and last line (empty))
            for (var currentFrame = 1; currentFrame < data.Length - 1; currentFrame++)
            {
                // Pass the current timestamp to the Frame constructor
                var dataFrame = data[currentFrame].Split(',');
                Frame newFrame = new Frame(float.Parse(dataFrame[0]));

                // Every bone has data in 7 columns (3 position, 4 rotation)
                foreach (Bone.Type bt in Enum.GetValues(typeof(Bone.Type)))
                {
                    int off = (int)bt * 7;
                    Bone.Data currentBoneData = new Bone.Data(
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
                anim.frameList.Add(newFrame);
            }

            Debug.Log("Motion contains " + anim.frameList.Count + " frames");

            // Unload Asset to free Memory
            Resources.UnloadAsset(moCapAsset);
        }

        private static void ComputeVelocities(Animation anim)
        {
            // Validate input
            if (anim.frameList.Count < 2)
            {
                Debug.LogError("The Animation does not have enough Frames to compute velocities");
                throw new Exception("The Animation does not have enough Frames to compute velocities");
            }

            // For every bone of the Animation
            foreach (Bone.Type bt in Enum.GetValues(typeof(Bone.Type)))
            {
                // Find the position of the first frame
                Vector3 lastPosition = anim.frameList[0].boneDataDict[bt].position;

                foreach (Frame frame in anim.frameList)
                {
                    frame.boneDataDict[bt].velocity = (frame.boneDataDict[bt].position - lastPosition);
                }

                // Set the velocity of the first Frame equal to the one in the second
                // That is because it is currently 0 and it is probably very close to the one in the second Frame
                anim.frameList[0].boneDataDict[bt].velocity = anim.frameList[1].boneDataDict[bt].velocity;
            }
        }
    }
}
