/**
[EasyMotionRecorder]

Copyright (c) 2018 Duo.inc

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using System.IO;

namespace Entum
{
    /// <summary>
    /// Play the motion data spit out in CSV
    /// </summary>
    public class MotionDataPlayerCSV : MotionDataPlayer
    {
        [SerializeField, Tooltip("Ending with a slash")]
        private string recordedDirectory;

        [SerializeField, Tooltip("Extension")]
        private string recordedFileName;

        // Use this for initialization
        private void Start()
        {
            if (string.IsNullOrEmpty(recordedDirectory))
            {
                recordedDirectory = Application.streamingAssetsPath + "/";
            }

            string motionCSVPath = recordedDirectory + recordedFileName;
            LoadCSVData(motionCSVPath);
        }

        //Create recordedMotionData from CSV
        private void LoadCSVData(string motionDataPath)
        {
            //Exit if the file does not exist
            if (!File.Exists(motionDataPath))
            {
                return;
            }

            recordedHumanoidPoses = ScriptableObject.CreateInstance<HumanoidPoses>();

            FileStream fs = null;
            StreamReader sr = null;

            //File reading
            try
            {
                fs = new FileStream(motionDataPath, FileMode.Open);
                sr = new StreamReader(fs);

                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    var seriHumanPose = new HumanoidPoses.SerializeHumanoidPose();
                    if (line != "")
                    {
                        seriHumanPose.DeserializeCSV(line);
                        recordedHumanoidPoses.serializeHumanoidPoses.Add(seriHumanPose);
                    }
                }
                sr.Close();
                fs.Close();
                sr = null;
                fs = null;
            }
            catch (System.Exception e)
            {
                Debug.LogError("File reading failed! " + e.Message + e.StackTrace);
            }

            if (sr != null)
            {
                sr.Close();
            }

            if (fs != null)
            {
                fs.Close();
            }
        }
    }
}