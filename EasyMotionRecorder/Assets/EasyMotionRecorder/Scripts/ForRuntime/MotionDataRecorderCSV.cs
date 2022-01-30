/**
[EasyMotionRecorder]

Copyright (c) 2018 Duo.inc

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using System;
using System.IO;

namespace Entum
{
    /// <summary>
    /// A class that records motion data in CSV
    /// Can be recorded even at runtime
    /// </summary>
    [DefaultExecutionOrder(31000)]
    public class MotionDataRecorderCSV : MotionDataRecorder
    {
        [SerializeField, Tooltip("Ending with a slash")]
        private string outputDirectory;

        [SerializeField, Tooltip("Extension")]
        private string outputFileName;

        protected override void WriteAnimationFile()
        {
            //File open
            string directoryStr = outputDirectory;
            if (directoryStr == "")
            {
                //Auto-config directory
                directoryStr = Application.streamingAssetsPath + "/";

                if (!Directory.Exists(directoryStr))
                {
                    Directory.CreateDirectory(directoryStr);
                }
            }

            string fileNameStr = outputFileName;
            if (fileNameStr == "")
            {
                //Automatic configuration file name
                fileNameStr = string.Format("motion_{0:yyyy_MM_dd_HH_mm_ss}.csv", DateTime.Now);
            }

            FileStream fs = new FileStream(directoryStr + fileNameStr, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            foreach (var pose in humanoidPoses.serializeHumanoidPoses)
            {
                string seriStr = pose.SerializeCSV();
                sw.WriteLine(seriStr);
            }

            //File close
            try
            {
                sw.Close();
                fs.Close();
                sw = null;
                fs = null;
            }
            catch (Exception e)
            {
                Debug.LogError("File export failed! " + e.Message + e.StackTrace);
            }

            if (sw != null)
            {
                sw.Close();
            }

            if (fs != null)
            {
                fs.Close();
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            recordedTime = 0f;
            frameIndex = 0;
        }
    }
}