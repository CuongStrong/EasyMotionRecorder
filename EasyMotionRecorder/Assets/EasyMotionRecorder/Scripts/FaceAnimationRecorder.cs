using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/**
[EasyMotionRecorder]

Copyright (c) 2018 Duo.inc

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

namespace Entum
{
    /// <summary>
    /// A class that records the movement of Blendshape
    /// Since there is a high possibility that lip sync will be added later and AudioClip will be added on the Timeline.
    /// It is possible to register the Blendshape name to be exclusive.
    /// </summary>
    [RequireComponent(typeof(MotionDataRecorder))]
    public class FaceAnimationRecorder : MonoBehaviour
    {
        [Header("Set to true if you want to record facial expressions at the same time.")]
        [SerializeField] private bool recordFaceBlendshapes = false;

        [Header("If you don't want to record lip sync, put the morph name here Example: face_mouse_e etc.")]
        [SerializeField] private List<string> exclusiveBlendshapeNames;

        private MotionDataRecorder motionDataRecorder;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;

        private CharacterFacialData characterFacialData = null;

        private bool recording = false;
        private int frameCount = 0;
        private CharacterFacialData.SerializeHumanoidFace pastSerializeHumanoidFace = new CharacterFacialData.SerializeHumanoidFace();
        private float recordedTime = 0f;


        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        // IEnumerator Start()
        // {
        //     yield return new WaitForSeconds(2);

        //     Debug.Log("================================= DataSave.Instance.clips.Count: " + DataSave.Instance.clips2.Count);
        // }

        private void OnEnable()
        {
            ModelLoader.OnModelChanged += OnModelChanged;

            motionDataRecorder = GetComponent<MotionDataRecorder>();
            motionDataRecorder.OnRecordStart += RecordStart;
            motionDataRecorder.OnRecordEnd += RecordEnd;
        }

        void OnModelChanged(GameObject model)
        {
            skinnedMeshRenderers = GetSkinnedMeshRenderers(model.GetComponentInChildren<Animator>());
            if (skinnedMeshRenderers == null)
            {
                Debug.LogError("Missing skinnedMeshRenderers");
            }
        }

        SkinnedMeshRenderer[] GetSkinnedMeshRenderers(Animator root)
        {
            var helper = root;
            var renderers = helper.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<SkinnedMeshRenderer> smeshList = new List<SkinnedMeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var rend = renderers[i];
                var cnt = rend.sharedMesh.blendShapeCount;
                if (cnt > 0)
                {
                    smeshList.Add(rend);
                }
            }

            return smeshList.ToArray();
        }

        private void OnDisable()
        {
            if (recording)
            {
                RecordEnd();
                recording = false;
            }

            ModelLoader.OnModelChanged -= OnModelChanged;

            if (motionDataRecorder != null)
            {
                motionDataRecorder.OnRecordStart -= RecordStart;
                motionDataRecorder.OnRecordEnd -= RecordEnd;
            }
        }

        /// <summary>
        /// Start recording
        /// </summary>
        private void RecordStart()
        {
            if (recordFaceBlendshapes == false)
            {
                return;
            }

            if (recording)
            {
                return;
            }

            if (skinnedMeshRenderers.Length == 0)
            {
                Debug.LogError("No face animation is recorded because the face mesh is not specified.");
                return;
            }

            Debug.Log("FaceAnimationRecorder record start");
            recording = true;
            recordedTime = 0f;
            frameCount = 0;
            characterFacialData = ScriptableObject.CreateInstance<CharacterFacialData>();
        }

        /// <summary>
        /// End of recording
        /// </summary>
        private void RecordEnd()
        {
            if (recordFaceBlendshapes == false)
            {
                return;
            }

            if (skinnedMeshRenderers.Length == 0)
            {
                Debug.LogError("I didn't record the face animation because the face mesh wasn't specified.");
                if (recording == true)
                {
                    Debug.LogAssertion("Unexpected execution!!!!");
                }
            }
            else
            {
                //WriteAnimationFileToScriptableObject();
                // ExportFacialAnimationClip(motionDataRecorder.characterAnimator, motionDataRecorder.humanoidPoses.clip, characterFacialData);
            }

            Debug.Log("FaceAnimationRecorder record end");

            recording = false;
        }

        public void ExportFacialAnimationClip()
        {
            ExportFacialAnimationClip(motionDataRecorder.characterAnimator, motionDataRecorder.humanoidPoses.clip, characterFacialData);
        }


        private void WriteAnimationFileToScriptableObject()
        {
#if UNITY_EDITOR
            MotionDataRecorder.SafeCreateDirectory("Assets/Resources");

            string path = AssetDatabase.GenerateUniqueAssetPath(
                "Assets/Resources/RecordMotion_ face" + motionDataRecorder.characterAnimator.name +
                DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") +
                ".asset");

            if (characterFacialData == null)
            {
                Debug.LogError("記録されたFaceデータがnull");
            }
            else
            {
                AssetDatabase.CreateAsset(characterFacialData, path);
                AssetDatabase.Refresh();
            }

            recordedTime = 0f;
            frameCount = 0;
#endif
        }

        //The one that checks if there is a difference in the frame.
        private bool IsSame(CharacterFacialData.SerializeHumanoidFace a, CharacterFacialData.SerializeHumanoidFace b)
        {
            if (a == null || b == null || a.smeshes.Count == 0 || b.smeshes.Count == 0)
            {
                return false;
            }

            if (a.BlendShapeNum() != b.BlendShapeNum())
            {
                return false;
            }

            return !a.smeshes.Where((t1, i) =>
                t1.blendShapes.Where((t, j) => Mathf.Abs(t - b.smeshes[i].blendShapes[j]) > 1).Any()).Any();
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                ExportFacialAnimationClipTest();
            }

            if (!recording)
            {
                return;
            }

            recordedTime += Time.deltaTime;

            var serializeHumanoidFace = new CharacterFacialData.SerializeHumanoidFace();
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                var meshAndBlendshape = new CharacterFacialData.SerializeHumanoidFace.MeshAndBlendshape();
                meshAndBlendshape.path = skinnedMeshRenderers[i].name;
                meshAndBlendshape.blendShapes = new float[skinnedMeshRenderers[i].sharedMesh.blendShapeCount];

                for (int j = 0; j < skinnedMeshRenderers[i].sharedMesh.blendShapeCount; j++)
                {
                    var tname = skinnedMeshRenderers[i].sharedMesh.GetBlendShapeName(j);

                    var useThis = true;

                    foreach (var item in exclusiveBlendshapeNames)
                    {
                        if (item.IndexOf(tname, StringComparison.Ordinal) >= 0)
                        {
                            useThis = false;
                        }
                    }

                    if (useThis)
                    {
                        meshAndBlendshape.blendShapes[j] = skinnedMeshRenderers[i].GetBlendShapeWeight(j);
                    }
                }

                serializeHumanoidFace.smeshes.Add(meshAndBlendshape);
            }

            if (!IsSame(serializeHumanoidFace, pastSerializeHumanoidFace))
            {
                serializeHumanoidFace.frameCount = frameCount;
                serializeHumanoidFace.time = recordedTime;

                characterFacialData.facials.Add(serializeHumanoidFace);
                pastSerializeHumanoidFace = new CharacterFacialData.SerializeHumanoidFace(serializeHumanoidFace);
            }

            frameCount++;
        }


        /// <summary>
        /// Write with the recorded data as Animator
        /// </summary>
        /// <param name="root"></param>
        /// <param name="facial"></param>
        void ExportFacialAnimationClip(Animator root, AnimationClip animclip, CharacterFacialData facial)
        {
#if UNITY_EDITOR
            if (animclip == null)
                animclip = new AnimationClip();

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                var pathsb = new StringBuilder().Append(skinnedMeshRenderers[i].transform.name);
                var trans = skinnedMeshRenderers[i].transform;

                while (trans.parent != null && trans.parent != root.transform)
                {
                    trans = trans.parent;
                    pathsb.Insert(0, "/").Insert(0, trans.name);
                }

                //The path contains the base name of the Blend shape
                //U_CHAR_1: Something like SkinnedMeshRenderer
                var path = pathsb.ToString();

                //An Animation Curve is generated for each individual Blend shape of the individual mesh.
                for (var j = 0; j < skinnedMeshRenderers[i].sharedMesh.blendShapeCount; j++)
                {
                    // var curveBinding = new EditorCurveBinding();
                    // curveBinding.type = typeof(SkinnedMeshRenderer);
                    // curveBinding.path = path;
                    // curveBinding.propertyName = "blendShape." + skinnedMeshRenderers[i].sharedMesh.GetBlendShapeName(j);
                    AnimationCurve curve = new AnimationCurve();

                    float pastBlendshapeWeight = -1;
                    for (int k = 0; k < characterFacialData.facials.Count; k++)
                    {
                        float time = 0;
                        if (k > 0)
                        {
                            time = facial.facials[k - 1].time;
                        }

                        for (float ind = time; ind < facial.facials[k].time; ind += 0.1f)
                        {
                            if (k > 0)
                            {
                                curve.AddKey(ind,
                                    characterFacialData.facials[k - 1].smeshes[i]
                                        .blendShapes[j]);
                            }
                            else
                            {
                                curve.AddKey(ind,
                                    characterFacialData.facials[0].smeshes[i].blendShapes[j]);
                            }
                        }

                        curve.AddKey(facial.facials[k].time,
                            characterFacialData.facials[k].smeshes[i].blendShapes[j]);

                    }


                    // AnimationUtility.SetEditorCurve(animclip, curveBinding, curve);
                    animclip.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + skinnedMeshRenderers[i].sharedMesh.GetBlendShapeName(j), curve);
                }
            }

            MotionDataRecorder.SafeCreateDirectory("Assets/Resources");

            var outputPath = "Assets/Resources/FaceRecordMotion_" + motionDataRecorder.characterAnimator.name + "_" +
                             DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "_Clip.anim";

            Debug.Log("outputPath:" + outputPath);
            AssetDatabase.CreateAsset(animclip, AssetDatabase.GenerateUniqueAssetPath(outputPath));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // DataSave.Instance.clip3 = animclip;//.Add(animclip);
            // SaveGameManager.Instance.Save();

            // Debug.Log("================================= DataSave.Instance.clips.Count: " + DataSave.Instance.clips2.Count);
#endif
        }

        /// <summary>
        /// Animator and a test to write with recorded data
        /// </summary>
        /// <param name="root"></param>
        /// <param name="facial"></param>
        void ExportFacialAnimationClipTest()
        {
#if UNITY_EDITOR
            var animclip = new AnimationClip();

            var mesh = skinnedMeshRenderers;

            for (int i = 0; i < mesh.Length; i++)
            {
                var pathsb = new StringBuilder().Append(mesh[i].transform.name);
                var trans = mesh[i].transform;
                while (trans.parent != null && trans.parent != motionDataRecorder.characterAnimator.transform)
                {
                    trans = trans.parent;
                    pathsb.Insert(0, "/").Insert(0, trans.name);
                }

                var path = pathsb.ToString();

                for (var j = 0; j < mesh[i].sharedMesh.blendShapeCount; j++)
                {
                    var curveBinding = new EditorCurveBinding();
                    curveBinding.type = typeof(SkinnedMeshRenderer);
                    curveBinding.path = path;
                    curveBinding.propertyName = "blendShape." + mesh[i].sharedMesh.GetBlendShapeName(j);
                    AnimationCurve curve = new AnimationCurve();


                    //Hit the key in the transition of 0 → 100 → 0 for all Blend shapes
                    curve.AddKey(0, 0);
                    curve.AddKey(1, 100);
                    curve.AddKey(2, 0);

                    Debug.Log("path: " + curveBinding.path + "\r\nname: " + curveBinding.propertyName + " val:");

                    AnimationUtility.SetEditorCurve(animclip, curveBinding, curve);
                }
            }

            AssetDatabase.CreateAsset(animclip,
                AssetDatabase.GenerateUniqueAssetPath("Assets/" + motionDataRecorder.characterAnimator.name +
                                                      "_facial_ClipTest.anim"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif

        }
    }
}
