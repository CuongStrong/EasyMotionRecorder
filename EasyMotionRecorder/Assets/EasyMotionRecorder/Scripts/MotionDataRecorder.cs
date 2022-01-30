/**
[EasyMotionRecorder]

Copyright (c) 2018 Duo.inc

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using System;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Entum
{
    /// <summary>
    /// Motion data recording class
    /// As for the script execution order, I want to get the attitude after VRIK processing is finished.
    /// Maximum value = 32000 is specified
    /// </summary>
    [DefaultExecutionOrder(32000)]
    public class MotionDataRecorder : MonoBehaviour
    {
        [SerializeField] private KeyCode recordStartKey = KeyCode.R;
        [SerializeField] private KeyCode recordStopKey = KeyCode.X;
        [SerializeField] private Animator animator;
        [SerializeField] private bool recording;
        [SerializeField] protected int frameIndex;


        [SerializeField, Tooltip("Normally, there is no problem with OBJECT ROOT. Please change for special equipment")]
        private MotionDataSettings.Rootbonesystem rootBoneSystem = MotionDataSettings.Rootbonesystem.Objectroot;

        [SerializeField, Tooltip("This parameter is not used when rootBoneSystem is OBJECTROOT.")]
        private HumanBodyBones targetRootBone = HumanBodyBones.Hips;

        [SerializeField] private HumanBodyBones leftFootHumanBodyBones = HumanBodyBones.LeftFoot;
        [SerializeField] private HumanBodyBones rightFootHumanBodyBones = HumanBodyBones.RightFoot;

        public HumanoidPoses humanoidPoses { get; private set; }
        protected float recordedTime;

        private HumanPose currentHumanPose;
        private HumanPoseHandler humanPoseHandler;
        public Action OnRecordStart;
        public Action OnRecordEnd;

        private void OnEnable()
        {
            ModelLoader.OnModelChanged += OnModelChanged;
        }

        private void OnDisable()
        {
            ModelLoader.OnModelChanged -= OnModelChanged;
        }

        void OnModelChanged(GameObject model)
        {
            animator = model.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogError("Animator is not set in MotionDataRecorder. Delete MotionDataRecorder.");
                Destroy(this);
                return;
            }

            humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        }

        private void Update()
        {
            if (Input.GetKeyDown(recordStartKey))
            {
                RecordStart();
            }

            if (Input.GetKeyDown(recordStopKey))
            {
                RecordEnd();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log((DataSave.Instance.clip3 == null) + " ================================================");
                SetClip(DataSave.Instance.clip3, "Emote_DustOffShoulders_CMF");


                // vrmController.SetAnimationController(null);
            }
        }

        private void SetClip(AnimationClip animationClip, string state)
        {
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            // animatorOverrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
            animatorOverrideController[state] = animationClip;
            animator.runtimeAnimatorController = animatorOverrideController;
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (!recording)
            {
                return;
            }

            recordedTime += Time.deltaTime;

            //Get the Humanoid posture of the current frame
            humanPoseHandler.GetHumanPose(ref currentHumanPose);

            //Write the acquired posture in poses
            var serializeHumanoidPose = new HumanoidPoses.SerializeHumanoidPose();

            switch (rootBoneSystem)
            {
                case MotionDataSettings.Rootbonesystem.Objectroot:
                    serializeHumanoidPose.bodyRootPosition = animator.transform.localPosition;
                    serializeHumanoidPose.bodyRootRotation = animator.transform.localRotation;
                    break;

                case MotionDataSettings.Rootbonesystem.Hipbone:
                    serializeHumanoidPose.bodyRootPosition = animator.GetBoneTransform(targetRootBone).position;
                    serializeHumanoidPose.bodyRootRotation = animator.GetBoneTransform(targetRootBone).rotation;
                    Debug.LogWarning(animator.GetBoneTransform(targetRootBone).position);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var bodyTQ = new TQ(currentHumanPose.bodyPosition, currentHumanPose.bodyRotation);
            var leftFootTQ = new TQ(animator.GetBoneTransform(leftFootHumanBodyBones).position, animator.GetBoneTransform(leftFootHumanBodyBones).rotation);
            var rightFootTQ = new TQ(animator.GetBoneTransform(rightFootHumanBodyBones).position, animator.GetBoneTransform(rightFootHumanBodyBones).rotation);
            leftFootTQ = AvatarUtility.GetIKGoalTQ(animator.avatar, animator.humanScale, AvatarIKGoal.LeftFoot, bodyTQ, leftFootTQ);
            rightFootTQ = AvatarUtility.GetIKGoalTQ(animator.avatar, animator.humanScale, AvatarIKGoal.RightFoot, bodyTQ, rightFootTQ);

            serializeHumanoidPose.bodyPosition = bodyTQ.t;
            serializeHumanoidPose.bodyRotation = bodyTQ.q;
            serializeHumanoidPose.leftFootIKPos = leftFootTQ.t;
            serializeHumanoidPose.leftFootIKRot = leftFootTQ.q;
            serializeHumanoidPose.rightfootIKPos = rightFootTQ.t;
            serializeHumanoidPose.rightfootIKRot = rightFootTQ.q;

            serializeHumanoidPose.frameCount = frameIndex;
            serializeHumanoidPose.muscles = new float[currentHumanPose.muscles.Length];
            serializeHumanoidPose.time = recordedTime;

            for (int i = 0; i < serializeHumanoidPose.muscles.Length; i++)
            {
                serializeHumanoidPose.muscles[i] = currentHumanPose.muscles[i];
            }

            SetHumanBoneTransformToHumanoidPoses(animator, ref serializeHumanoidPose);

            humanoidPoses.serializeHumanoidPoses.Add(serializeHumanoidPose);
            frameIndex++;
        }

        /// <summary>
        ///Start recording
        /// </summary>
        private void RecordStart()
        {
            if (recording)
            {
                return;
            }

            humanoidPoses = ScriptableObject.CreateInstance<HumanoidPoses>();

            if (OnRecordStart != null)
            {
                OnRecordStart();
            }

            OnRecordEnd += WriteAnimationFile;
            recording = true;
            recordedTime = 0f;
            frameIndex = 0;
        }

        /// <summary>
        /// Recording end
        /// </summary>
        private void RecordEnd()
        {
            if (!recording)
            {
                return;
            }


            if (OnRecordEnd != null)
            {
                OnRecordEnd();
            }

            OnRecordEnd -= WriteAnimationFile;
            recording = false;
        }

        private static void SetHumanBoneTransformToHumanoidPoses(Animator animator, ref HumanoidPoses.SerializeHumanoidPose pose)
        {
            HumanBodyBones[] values = Enum.GetValues(typeof(HumanBodyBones)) as HumanBodyBones[];
            foreach (HumanBodyBones b in values)
            {
                if (b < 0 || b >= HumanBodyBones.LastBone)
                {
                    continue;
                }

                Transform t = animator.GetBoneTransform(b);
                if (t != null)
                {
                    var bone = new HumanoidPoses.SerializeHumanoidPose.HumanoidBone();
                    bone.Set(animator.transform, t);
                    pose.humanoidBones.Add(bone);
                }
            }
        }

        protected virtual void WriteAnimationFile()
        {
#if UNITY_EDITOR
            SafeCreateDirectory("Assets/Resources");

            var path = string.Format("Assets/Resources/RecordMotion_{0}{1:yyyy_MM_dd_HH_mm_ss}.asset", animator.name, DateTime.Now);
            var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(humanoidPoses, uniqueAssetPath);
            AssetDatabase.Refresh();

            recordedTime = 0f;
            frameIndex = 0;
#endif

            humanoidPoses.ExportHumanoidAnimRuntime();
        }

        /// <summary>
        /// If the directory does not exist in the specified path
        /// Create all directories and subdirectories
        /// </summary>
        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            return Directory.Exists(path) ? null : Directory.CreateDirectory(path);
        }
        public Animator characterAnimator
        {
            get { return animator; }
        }

        public class TQ
        {
            public TQ(Vector3 translation, Quaternion rotation)
            {
                t = translation;
                q = rotation;
            }
            public Vector3 t;
            public Quaternion q;
            // Scale should always be 1,1,1
        }
        public class AvatarUtility
        {
            static public TQ GetIKGoalTQ(Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, TQ animatorBodyPositionRotation, TQ skeletonTQ)
            {
                int humanId = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
                if (humanId == (int)HumanBodyBones.LastBone)
                    throw new InvalidOperationException("Invalid human id.");
                MethodInfo methodGetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodGetAxisLength == null)
                    throw new InvalidOperationException("Cannot find GetAxisLength method.");
                MethodInfo methodGetPostRotation = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodGetPostRotation == null)
                    throw new InvalidOperationException("Cannot find GetPostRotation method.");
                Quaternion postRotation = (Quaternion)methodGetPostRotation.Invoke(avatar, new object[] { humanId });
                var goalTQ = new TQ(skeletonTQ.t, skeletonTQ.q * postRotation);
                if (avatarIKGoal == AvatarIKGoal.LeftFoot || avatarIKGoal == AvatarIKGoal.RightFoot)
                {
                    // Here you could use animator.leftFeetBottomHeight or animator.rightFeetBottomHeight rather than GetAxisLenght
                    // Both are equivalent but GetAxisLength is the generic way and work for all human bone
                    float axislength = (float)methodGetAxisLength.Invoke(avatar, new object[] { humanId });
                    Vector3 footBottom = new Vector3(axislength, 0, 0);
                    goalTQ.t += (goalTQ.q * footBottom);
                }
                // IK goal are in avatar body local space
                Quaternion invRootQ = Quaternion.Inverse(animatorBodyPositionRotation.q);
                goalTQ.t = invRootQ * (goalTQ.t - animatorBodyPositionRotation.t);
                goalTQ.q = invRootQ * goalTQ.q;
                goalTQ.t /= humanScale;

                return goalTQ;
            }
            static public HumanBodyBones HumanIDFromAvatarIKGoal(AvatarIKGoal avatarIKGoal)
            {
                HumanBodyBones humanId = HumanBodyBones.LastBone;
                switch (avatarIKGoal)
                {
                    case AvatarIKGoal.LeftFoot: humanId = HumanBodyBones.LeftFoot; break;
                    case AvatarIKGoal.RightFoot: humanId = HumanBodyBones.RightFoot; break;
                    case AvatarIKGoal.LeftHand: humanId = HumanBodyBones.LeftHand; break;
                    case AvatarIKGoal.RightHand: humanId = HumanBodyBones.RightHand; break;
                }
                return humanId;
            }
        }
    }
}
