/**
[EasyMotionRecorder]

Copyright (c) 2018 Duo.inc

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using System;

namespace Entum
{
    /// <summary>
    /// Motion data playback class
    /// Script Execution Order for shaking objects such as SpringBone, DynamicBone, BulletPhysicsImpl 20000, etc.
    /// Please make it a large value.
    /// DefaultExecutionOrder (11000) is intended to slow down the processing order compared to VRIK series.
    /// </summary>
    [DefaultExecutionOrder(11000)]
    public class MotionDataPlayer : MonoBehaviour
    {
        [SerializeField] private KeyCode playStartKey = KeyCode.S;
        [SerializeField] private KeyCode playStopKey = KeyCode.T;

        [SerializeField] protected HumanoidPoses recordedHumanoidPoses;
        [SerializeField] private Animator animator;


        [SerializeField, Tooltip("Specify the playback start frame. If it is 0, it starts from the beginning of the file.")]
        private int startFrame;

        [SerializeField] private bool playing;
        [SerializeField] private int frameIndex;

        [SerializeField, Tooltip("Normally, there is no problem with OBJECT ROOT. Please change for special equipment")]
        private MotionDataSettings.Rootbonesystem rootBoneSystem = MotionDataSettings.Rootbonesystem.Objectroot;

        [SerializeField, Tooltip("This parameter is not used when rootBoneSystem is OBJECTROOT.")]
        private HumanBodyBones targetRootBone = HumanBodyBones.Hips;

        private HumanPoseHandler humanPoseHandler;
        private Action OnPlayFinish;
        private float playingTime;

        private void OnEnable()
        {
            ModelLoader.OnModelChanged += OnModelChanged;
            OnPlayFinish += StopMotion;
        }

        private void OnDisable()
        {
            ModelLoader.OnModelChanged -= OnModelChanged;
            OnPlayFinish -= StopMotion;
        }

        void OnModelChanged(GameObject model)
        {
            animator = model.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogError("Animator is not set in MotionDataPlayer. Remove MotionDataPlayer.");
                Destroy(this);
                return;
            }

            humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(playStartKey))
            {
                PlayMotion();
            }

            if (Input.GetKeyDown(playStopKey))
            {
                StopMotion();
            }
        }

        private void LateUpdate()
        {
            if (!playing)
            {
                return;
            }

            playingTime += Time.deltaTime;
            SetHumanPose();
        }

        /// <summary>
        /// Start motion data playback
        /// </summary>
        private void PlayMotion()
        {
            if (playing)
            {
                return;
            }

            if (recordedHumanoidPoses == null)
            {
                Debug.LogError("Recorded motion data is not specified. Does not play.");
                return;
            }


            playingTime = startFrame * (Time.deltaTime / 1f);
            frameIndex = startFrame;
            playing = true;
        }

        /// <summary>
        ///Motion data playback ends. Called automatically even when the number of frames is the last
        /// </summary>
        private void StopMotion()
        {
            if (!playing)
            {
                return;
            }

            playingTime = 0f;
            frameIndex = startFrame;
            playing = false;
        }

        private void SetHumanPose()
        {
            var humanPose = new HumanPose();
            humanPose.muscles = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].muscles;
            humanPoseHandler.SetHumanPose(ref humanPose);
            humanPose.bodyPosition = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].bodyPosition;
            humanPose.bodyRotation = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].bodyRotation;

            switch (rootBoneSystem)
            {
                case MotionDataSettings.Rootbonesystem.Objectroot:
                    //_animator.transform.localPosition = RecordedMotionData.Poses[_frameIndex].BodyRootPosition;
                    //_animator.transform.localRotation = RecordedMotionData.Poses[_frameIndex].BodyRootRotation;
                    break;

                case MotionDataSettings.Rootbonesystem.Hipbone:
                    humanPose.bodyPosition = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].bodyPosition;
                    humanPose.bodyRotation = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].bodyRotation;

                    animator.GetBoneTransform(targetRootBone).position = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].bodyRootPosition;
                    animator.GetBoneTransform(targetRootBone).rotation = recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].bodyRootRotation;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            //Adjusting the playback speed of motion data that has been processed
            if (playingTime > recordedHumanoidPoses.serializeHumanoidPoses[frameIndex].time)
            {
                frameIndex++;
            }

            if (frameIndex == recordedHumanoidPoses.serializeHumanoidPoses.Count - 1)
            {
                if (OnPlayFinish != null)
                {
                    OnPlayFinish();
                }
            }
        }
    }
}