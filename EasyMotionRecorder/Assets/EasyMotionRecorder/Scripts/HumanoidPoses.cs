/**
[EasyMotionRecorder]

Copyright (c) 2018 Duo.inc

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Entum
{
    [Serializable]
    public class MotionDataSettings
    {
        public enum Rootbonesystem
        {
            Hipbone,
            Objectroot
        }

        /// <summary>
        ///Muscle mapping for Humanoid
        /// </summary>
        public static Dictionary<string, string> traitPropMap = new Dictionary<string, string>
        {
            {"Left Thumb 1 Stretched", "LeftHand.Thumb.1 Stretched"},
            {"Left Thumb Spread", "LeftHand.Thumb.Spread"},
            {"Left Thumb 2 Stretched", "LeftHand.Thumb.2 Stretched"},
            {"Left Thumb 3 Stretched", "LeftHand.Thumb.3 Stretched"},
            {"Left Index 1 Stretched", "LeftHand.Index.1 Stretched"},
            {"Left Index Spread", "LeftHand.Index.Spread"},
            {"Left Index 2 Stretched", "LeftHand.Index.2 Stretched"},
            {"Left Index 3 Stretched", "LeftHand.Index.3 Stretched"},
            {"Left Middle 1 Stretched", "LeftHand.Middle.1 Stretched"},
            {"Left Middle Spread", "LeftHand.Middle.Spread"},
            {"Left Middle 2 Stretched", "LeftHand.Middle.2 Stretched"},
            {"Left Middle 3 Stretched", "LeftHand.Middle.3 Stretched"},
            {"Left Ring 1 Stretched", "LeftHand.Ring.1 Stretched"},
            {"Left Ring Spread", "LeftHand.Ring.Spread"},
            {"Left Ring 2 Stretched", "LeftHand.Ring.2 Stretched"},
            {"Left Ring 3 Stretched", "LeftHand.Ring.3 Stretched"},
            {"Left Little 1 Stretched", "LeftHand.Little.1 Stretched"},
            {"Left Little Spread", "LeftHand.Little.Spread"},
            {"Left Little 2 Stretched", "LeftHand.Little.2 Stretched"},
            {"Left Little 3 Stretched", "LeftHand.Little.3 Stretched"},
            {"Right Thumb 1 Stretched", "RightHand.Thumb.1 Stretched"},
            {"Right Thumb Spread", "RightHand.Thumb.Spread"},
            {"Right Thumb 2 Stretched", "RightHand.Thumb.2 Stretched"},
            {"Right Thumb 3 Stretched", "RightHand.Thumb.3 Stretched"},
            {"Right Index 1 Stretched", "RightHand.Index.1 Stretched"},
            {"Right Index Spread", "RightHand.Index.Spread"},
            {"Right Index 2 Stretched", "RightHand.Index.2 Stretched"},
            {"Right Index 3 Stretched", "RightHand.Index.3 Stretched"},
            {"Right Middle 1 Stretched", "RightHand.Middle.1 Stretched"},
            {"Right Middle Spread", "RightHand.Middle.Spread"},
            {"Right Middle 2 Stretched", "RightHand.Middle.2 Stretched"},
            {"Right Middle 3 Stretched", "RightHand.Middle.3 Stretched"},
            {"Right Ring 1 Stretched", "RightHand.Ring.1 Stretched"},
            {"Right Ring Spread", "RightHand.Ring.Spread"},
            {"Right Ring 2 Stretched", "RightHand.Ring.2 Stretched"},
            {"Right Ring 3 Stretched", "RightHand.Ring.3 Stretched"},
            {"Right Little 1 Stretched", "RightHand.Little.1 Stretched"},
            {"Right Little Spread", "RightHand.Little.Spread"},
            {"Right Little 2 Stretched", "RightHand.Little.2 Stretched"},
            {"Right Little 3 Stretched", "RightHand.Little.3 Stretched"},
        };
    }



    /// <summary>
    /// Contents of motion data
    /// </summary>
    public class HumanoidPoses : ScriptableObject
    {
#if UNITY_EDITOR
        //Output as a Generic anim file
        [ContextMenu("Export as Generic animation clips")]
        public void ExportGenericAnim()
        {
            clip = new AnimationClip { frameRate = 30 };
            AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = false });

            var bones = serializeHumanoidPoses[0].humanoidBones;
            for (int i = 0; i < bones.Count; i++)
            {
                var positionCurveX = new AnimationCurve();
                var positionCurveY = new AnimationCurve();
                var positionCurveZ = new AnimationCurve();
                var rotationCurveX = new AnimationCurve();
                var rotationCurveY = new AnimationCurve();
                var rotationCurveZ = new AnimationCurve();
                var rotationCurveW = new AnimationCurve();

                foreach (var p in serializeHumanoidPoses)
                {
                    positionCurveX.AddKey(p.time, p.humanoidBones[i].localPosition.x);
                    positionCurveY.AddKey(p.time, p.humanoidBones[i].localPosition.y);
                    positionCurveZ.AddKey(p.time, p.humanoidBones[i].localPosition.z);
                    rotationCurveX.AddKey(p.time, p.humanoidBones[i].localRotation.x);
                    rotationCurveY.AddKey(p.time, p.humanoidBones[i].localRotation.y);
                    rotationCurveZ.AddKey(p.time, p.humanoidBones[i].localRotation.z);
                    rotationCurveW.AddKey(p.time, p.humanoidBones[i].localRotation.w);
                }

                //pathは階層
                //http://mebiustos.hatenablog.com/entry/2015/09/16/230000
                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalPosition.x"
                    }, positionCurveX);
                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalPosition.y"
                    }, positionCurveY);
                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalPosition.z"
                    }, positionCurveZ);

                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalRotation.x"
                    }, rotationCurveX);
                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalRotation.y"
                    }, rotationCurveY);
                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalRotation.z"
                    }, rotationCurveZ);
                AnimationUtility.SetEditorCurve(clip,
                    new EditorCurveBinding
                    {
                        path = serializeHumanoidPoses[0].humanoidBones[i].name,
                        type = typeof(Transform),
                        propertyName = "m_LocalRotation.w"
                    }, rotationCurveW);
            }

            clip.EnsureQuaternionContinuity();

            var path = string.Format("Assets/Resources/RecordMotion_{0:yyyy_MM_dd_HH_mm_ss}_Generic.anim", DateTime.Now);
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(clip, uniqueAssetPath);
            AssetDatabase.SaveAssets();
        }

        //Output as a humanoid anim file.
        [ContextMenu("Export as Humanoid animation clips")]
        public void ExportHumanoidAnim()
        {
            clip = new AnimationClip { frameRate = 30 };
            AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = false });


            // body position
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.bodyPosition.x);
                    curveY.AddKey(item.time, item.bodyPosition.y);
                    curveZ.AddKey(item.time, item.bodyPosition.z);
                }

                const string muscleX = "RootT.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RootT.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RootT.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
            }
            // Leftfoot position
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.leftFootIKPos.x);
                    curveY.AddKey(item.time, item.leftFootIKPos.y);
                    curveZ.AddKey(item.time, item.leftFootIKPos.z);
                }

                const string muscleX = "LeftFootT.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "LeftFootT.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "LeftFootT.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
            }
            // Rightfoot position
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.rightfootIKPos.x);
                    curveY.AddKey(item.time, item.rightfootIKPos.y);
                    curveZ.AddKey(item.time, item.rightfootIKPos.z);
                }

                const string muscleX = "RightFootT.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RightFootT.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RightFootT.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
            }
            // body rotation
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.bodyRotation.x);
                    curveY.AddKey(item.time, item.bodyRotation.y);
                    curveZ.AddKey(item.time, item.bodyRotation.z);
                    curveW.AddKey(item.time, item.bodyRotation.w);
                }

                const string muscleX = "RootQ.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RootQ.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RootQ.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
                const string muscleW = "RootQ.w";
                clip.SetCurve("", typeof(Animator), muscleW, curveW);
            }
            // Leftfoot rotation
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.leftFootIKRot.x);
                    curveY.AddKey(item.time, item.leftFootIKRot.y);
                    curveZ.AddKey(item.time, item.leftFootIKRot.z);
                    curveW.AddKey(item.time, item.leftFootIKRot.w);
                }

                const string muscleX = "LeftFootQ.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "LeftFootQ.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "LeftFootQ.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
                const string muscleW = "LeftFootQ.w";
                clip.SetCurve("", typeof(Animator), muscleW, curveW);
            }
            // Rightfoot rotation
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.rightfootIKRot.x);
                    curveY.AddKey(item.time, item.rightfootIKRot.y);
                    curveZ.AddKey(item.time, item.rightfootIKRot.z);
                    curveW.AddKey(item.time, item.rightfootIKRot.w);
                }

                const string muscleX = "RightFootQ.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RightFootQ.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RightFootQ.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
                const string muscleW = "RightFootQ.w";
                clip.SetCurve("", typeof(Animator), muscleW, curveW);
            }

            // muscles
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                var curve = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curve.AddKey(item.time, item.muscles[i]);
                }

                var muscle = HumanTrait.MuscleName[i];
                if (MotionDataSettings.traitPropMap.ContainsKey(muscle))
                {
                    muscle = MotionDataSettings.traitPropMap[muscle];
                }

                clip.SetCurve("", typeof(Animator), muscle, curve);
            }

            clip.EnsureQuaternionContinuity();

            var path = string.Format("Assets/Resources/RecordMotion_{0:yyyy_MM_dd_HH_mm_ss}_Humanoid.anim", DateTime.Now);
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

            // AssetDatabase.CreateAsset(clip, uniqueAssetPath);
            // AssetDatabase.SaveAssets();
        }
#endif

        public void ExportHumanoidAnimRuntime()
        {
#if UNITY_EDITOR
            clip = new AnimationClip { frameRate = 30 };
            AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = false });


            // body position
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.bodyPosition.x);
                    curveY.AddKey(item.time, item.bodyPosition.y);
                    curveZ.AddKey(item.time, item.bodyPosition.z);
                }

                const string muscleX = "RootT.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RootT.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RootT.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
            }
            // Leftfoot position
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.leftFootIKPos.x);
                    curveY.AddKey(item.time, item.leftFootIKPos.y);
                    curveZ.AddKey(item.time, item.leftFootIKPos.z);
                }

                const string muscleX = "LeftFootT.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "LeftFootT.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "LeftFootT.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
            }
            // Rightfoot position
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.rightfootIKPos.x);
                    curveY.AddKey(item.time, item.rightfootIKPos.y);
                    curveZ.AddKey(item.time, item.rightfootIKPos.z);
                }

                const string muscleX = "RightFootT.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RightFootT.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RightFootT.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
            }
            // body rotation
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.bodyRotation.x);
                    curveY.AddKey(item.time, item.bodyRotation.y);
                    curveZ.AddKey(item.time, item.bodyRotation.z);
                    curveW.AddKey(item.time, item.bodyRotation.w);
                }

                const string muscleX = "RootQ.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RootQ.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RootQ.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
                const string muscleW = "RootQ.w";
                clip.SetCurve("", typeof(Animator), muscleW, curveW);
            }
            // Leftfoot rotation
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.leftFootIKRot.x);
                    curveY.AddKey(item.time, item.leftFootIKRot.y);
                    curveZ.AddKey(item.time, item.leftFootIKRot.z);
                    curveW.AddKey(item.time, item.leftFootIKRot.w);
                }

                const string muscleX = "LeftFootQ.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "LeftFootQ.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "LeftFootQ.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
                const string muscleW = "LeftFootQ.w";
                clip.SetCurve("", typeof(Animator), muscleW, curveW);
            }
            // Rightfoot rotation
            {
                var curveX = new AnimationCurve();
                var curveY = new AnimationCurve();
                var curveZ = new AnimationCurve();
                var curveW = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curveX.AddKey(item.time, item.rightfootIKRot.x);
                    curveY.AddKey(item.time, item.rightfootIKRot.y);
                    curveZ.AddKey(item.time, item.rightfootIKRot.z);
                    curveW.AddKey(item.time, item.rightfootIKRot.w);
                }

                const string muscleX = "RightFootQ.x";
                clip.SetCurve("", typeof(Animator), muscleX, curveX);
                const string muscleY = "RightFootQ.y";
                clip.SetCurve("", typeof(Animator), muscleY, curveY);
                const string muscleZ = "RightFootQ.z";
                clip.SetCurve("", typeof(Animator), muscleZ, curveZ);
                const string muscleW = "RightFootQ.w";
                clip.SetCurve("", typeof(Animator), muscleW, curveW);
            }

            // muscles
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                var curve = new AnimationCurve();
                foreach (var item in serializeHumanoidPoses)
                {
                    curve.AddKey(item.time, item.muscles[i]);
                }

                var muscle = HumanTrait.MuscleName[i];
                if (MotionDataSettings.traitPropMap.ContainsKey(muscle))
                {
                    muscle = MotionDataSettings.traitPropMap[muscle];
                }

                clip.SetCurve("", typeof(Animator), muscle, curve);
            }

            clip.EnsureQuaternionContinuity();

            var path = string.Format("Assets/Resources/RecordMotion_{0:yyyy_MM_dd_HH_mm_ss}_Humanoid.anim", DateTime.Now);
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(clip, uniqueAssetPath);
            AssetDatabase.SaveAssets();
#endif
        }

        [Serializable]
        public class SerializeHumanoidPose
        {
            public Vector3 bodyRootPosition;
            public Quaternion bodyRootRotation;

            public Vector3 bodyPosition;
            public Quaternion bodyRotation;
            public Vector3 leftFootIKPos;
            public Quaternion leftFootIKRot;
            public Vector3 rightfootIKPos;
            public Quaternion rightfootIKRot;

            public float[] muscles;

            //Number of frames
            public int frameCount;

            //Elapsed time since the start of recording. Countermeasures against processing omissions
            public float time;

            [Serializable]
            public class HumanoidBone
            {
                public string name;
                public Vector3 localPosition;
                public Quaternion localRotation;

                private static Dictionary<Transform, string> pathCache = new Dictionary<Transform, string>();

                private static string BuildRelativePath(Transform root, Transform target)
                {
                    var path = "";
                    pathCache.TryGetValue(target, out path);
                    if (path != null) return path;

                    var current = target;
                    while (true)
                    {
                        if (current == null) throw new Exception(target.name + "teeth" + root.name + "Not a child of");
                        if (current == root) break;

                        path = (path == "") ? current.name : current.name + "/" + path;

                        current = current.parent;
                    }

                    pathCache.Add(target, path);

                    return path;
                }

                public void Set(Transform root, Transform t)
                {
                    name = BuildRelativePath(root, t);

                    localPosition = t.localPosition;
                    localRotation = t.localRotation;
                }
            }

            public List<HumanoidBone> humanoidBones = new List<HumanoidBone>();

            //CSV serialization
            public string SerializeCSV()
            {
                StringBuilder sb = new StringBuilder();
                SerializeVector3(sb, bodyRootPosition);
                SerializeQuaternion(sb, bodyRootRotation);
                SerializeVector3(sb, bodyPosition);
                SerializeQuaternion(sb, bodyRotation);
                foreach (var muscle in muscles)
                {
                    sb.Append(muscle);
                    sb.Append(",");
                }
                sb.Append(frameCount);
                sb.Append(",");
                sb.Append(time);
                sb.Append(",");
                foreach (var humanoidBone in humanoidBones)
                {
                    sb.Append(humanoidBone.name);
                    sb.Append(",");
                    SerializeVector3(sb, humanoidBone.localPosition);
                    SerializeQuaternion(sb, humanoidBone.localRotation);
                }
                sb.Length = sb.Length - 1; //Remove last comma
                return sb.ToString();
            }

            private static void SerializeVector3(StringBuilder sb, Vector3 vec)
            {
                sb.Append(vec.x);
                sb.Append(",");
                sb.Append(vec.y);
                sb.Append(",");
                sb.Append(vec.z);
                sb.Append(",");
            }

            private static void SerializeQuaternion(StringBuilder sb, Quaternion q)
            {
                sb.Append(q.x);
                sb.Append(",");
                sb.Append(q.y);
                sb.Append(",");
                sb.Append(q.z);
                sb.Append(",");
                sb.Append(q.w);
                sb.Append(",");
            }

            //CSV deserialization
            public void DeserializeCSV(string str)
            {
                string[] dataString = str.Split(',');
                bodyRootPosition = DeserializeVector3(dataString, 0);
                bodyRootRotation = DeserializeQuaternion(dataString, 3);
                bodyPosition = DeserializeVector3(dataString, 7);
                bodyRotation = DeserializeQuaternion(dataString, 10);
                muscles = new float[HumanTrait.MuscleCount];
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    muscles[i] = float.Parse(dataString[i + 14]);
                }
                frameCount = int.Parse(dataString[14 + HumanTrait.MuscleCount]);
                time = float.Parse(dataString[15 + HumanTrait.MuscleCount]);
                var boneValues = Enum.GetValues(typeof(HumanBodyBones)) as HumanBodyBones[];
                for (int i = 0; i < boneValues.Length; i++)
                {
                    int startIndex = 16 + HumanTrait.MuscleCount + (i * 8);
                    if (dataString.Length <= startIndex)
                    {
                        break;
                    }

                    HumanoidBone bone = new HumanoidBone();
                    bone.name = dataString[startIndex];
                    bone.localPosition = DeserializeVector3(dataString, startIndex + 1);
                    bone.localRotation = DeserializeQuaternion(dataString, startIndex + 4);
                }
            }

            private static Vector3 DeserializeVector3(IList<string> str, int startIndex)
            {
                return new Vector3(float.Parse(str[startIndex]), float.Parse(str[startIndex + 1]), float.Parse(str[startIndex + 2]));
            }

            private static Quaternion DeserializeQuaternion(IList<string> str, int startIndex)
            {
                return new Quaternion(float.Parse(str[startIndex]), float.Parse(str[startIndex + 1]), float.Parse(str[startIndex + 2]), float.Parse(str[startIndex + 3]));
            }
        }

        public List<SerializeHumanoidPose> serializeHumanoidPoses = new List<SerializeHumanoidPose>();
        public AnimationClip clip;// { get; set; }
    }
}
