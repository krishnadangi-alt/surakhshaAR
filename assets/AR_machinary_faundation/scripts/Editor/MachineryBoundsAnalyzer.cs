#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryBoundsAnalyzer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/model_bounds_analysis.log";

        [System.Serializable]
        public struct ModelBoundsData
        {
            public string objectName;
            public Vector3 unscaledSize;
            public Vector3 unscaledMin;
            public Vector3 unscaledMax;
            public Vector3 unscaledCenter;
            public int rendererCount;
        }

        [MenuItem("SurakshaAR/Machinery/Analyze Model Bounds")]
        public static void AnalyzeBounds()
        {
            Directory.CreateDirectory("scratch");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryBoundsAnalyzer] Could not open scene at {ScenePath}");
                return;
            }

            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null)
            {
                Debug.LogError("[MachineryBoundsAnalyzer] MachineryScenario root not found!");
                return;
            }

            using (StreamWriter writer = new StreamWriter(LogPath, false))
            {
                writer.WriteLine("=== MACHINERY MODEL RENDERER BOUNDS ANALYSIS ===");
                writer.WriteLine($"Timestamp: {System.DateTime.Now}");
                writer.WriteLine();

                string[] targetNames = new string[]
                {
                    "ConveyorAssembly",
                    "DriveMotor",
                    "JamArea",
                    "EmergencyStop",
                    "MainIsolator",
                    "ControlPanel",
                    "FeedHopper",
                    "SteelWorkstationTable",
                    "Hasp",
                    "PadlockAndTag"
                };

                foreach (string tName in targetNames)
                {
                    GameObject targetGo = GameObject.Find(tName);
                    if (targetGo == null)
                    {
                        writer.WriteLine($"[MISSING] {tName} not found in scene.");
                        continue;
                    }

                    // Save original transform
                    Vector3 origPos = targetGo.transform.position;
                    Quaternion origRot = targetGo.transform.rotation;
                    Vector3 origScale = targetGo.transform.localScale;

                    // Reset for raw mesh bounds measurement
                    targetGo.transform.rotation = Quaternion.identity;
                    targetGo.transform.localScale = Vector3.one;

                    Renderer[] renderers = targetGo.GetComponentsInChildren<Renderer>(true);
                    if (renderers.Length == 0)
                    {
                        writer.WriteLine($"[NO RENDERERS] {tName}");
                        // Restore
                        targetGo.transform.position = origPos;
                        targetGo.transform.rotation = origRot;
                        targetGo.transform.localScale = origScale;
                        continue;
                    }

                    Bounds combinedBounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }

                    // Shift to local offset relative to root
                    Vector3 localCenter = combinedBounds.center - targetGo.transform.position;

                    writer.WriteLine($"OBJECT: {tName}");
                    writer.WriteLine($"  Renderers: {renderers.Length}");
                    writer.WriteLine($"  Raw Unscaled Size: ({combinedBounds.size.x:F3}, {combinedBounds.size.y:F3}, {combinedBounds.size.z:F3})");
                    writer.WriteLine($"  Raw Unscaled Min : ({combinedBounds.min.x:F3}, {combinedBounds.min.y:F3}, {combinedBounds.min.z:F3})");
                    writer.WriteLine($"  Raw Unscaled Max : ({combinedBounds.max.x:F3}, {combinedBounds.max.y:F3}, {combinedBounds.max.z:F3})");
                    writer.WriteLine($"  Raw Local Center : ({localCenter.x:F3}, {localCenter.y:F3}, {localCenter.z:F3})");
                    writer.WriteLine($"  Current Scale    : ({origScale.x:F3}, {origScale.y:F3}, {origScale.z:F3})");
                    writer.WriteLine($"  Current Position : ({origPos.x:F3}, {origPos.y:F3}, {origPos.z:F3})");
                    writer.WriteLine();

                    // Restore
                    targetGo.transform.position = origPos;
                    targetGo.transform.rotation = origRot;
                    targetGo.transform.localScale = origScale;
                }
            }

            Debug.Log($"[MachineryBoundsAnalyzer] Analysis complete! Results written to {LogPath}");
        }
    }
}
#endif
