using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entum
{
    public class CharacterFacialData : ScriptableObject
    {

        [System.SerializableAttribute]
        public class SerializeHumanoidFace
        {
            public class MeshAndBlendshape
            {
                public string path;
                public float[] blendShapes;
            }

            public int BlendShapeNum()
            {
                return smeshes.Count == 0 ? 0 : smeshes.Sum(t => t.blendShapes.Length);
            }

            //Number of frames
            public int frameCount;

            //Elapsed time since the start of recording. Countermeasures against processing omissions
            public float time;

            public SerializeHumanoidFace(SerializeHumanoidFace serializeHumanoidFace)
            {
                for (int i = 0; i < serializeHumanoidFace.smeshes.Count; i++)
                {
                    smeshes.Add(serializeHumanoidFace.smeshes[i]);
                    Array.Copy(serializeHumanoidFace.smeshes[i].blendShapes, smeshes[i].blendShapes, serializeHumanoidFace.smeshes[i].blendShapes.Length);
                }
                frameCount = serializeHumanoidFace.frameCount;
                time = serializeHumanoidFace.time;
            }

            //Even in a single frame, mouth mesh, eye mesh, etc. enter here individually
            public List<MeshAndBlendshape> smeshes = new List<MeshAndBlendshape>();
            public SerializeHumanoidFace() { }
        }

        public List<SerializeHumanoidFace> facials = new List<SerializeHumanoidFace>();
    }
}