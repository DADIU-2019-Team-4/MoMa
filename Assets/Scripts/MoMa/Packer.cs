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
    }
}
