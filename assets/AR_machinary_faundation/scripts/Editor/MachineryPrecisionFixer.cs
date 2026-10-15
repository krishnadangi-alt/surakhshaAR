#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPrecisionFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Execute Minimum Precision Correction")]
        public static void ExecuteMinimumPrecisionFix()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPrecisionFixer] STARTING MINIMUM PRECISION CORRECTION PASS...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[PrecisionFixer] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null) masterRoot = new GameObject("MachineryScenario");

            // Locate Hasp & Padlock if parented under LOTOAssembly root
            GameObject lotoRoot = GameObject.Find("LOTOAssembly");
            if (lotoRoot != null)
            {
                Transform haspTr = lotoRoot.transform.Find("Hasp");
                if (haspTr != null) haspTr.SetParent(masterRoot.transform, true);

                Transform padTr = lotoRoot.transform.Find("PadlockAndTag");
                if (padTr != null) padTr.SetParent(masterRoot.transform, true);
            }

            Transform controlPanel = masterRoot.transform.Find("ControlPanel");
            Transform eStop = masterRoot.transform.Find("EmergencyStop");
            Transform isolator = masterRoot.transform.Find("MainIsolator");
            Transform table = masterRoot.transform.Find("SteelWorkstationTable");
            Transform hasp = masterRoot.transform.Find("Hasp");
            Transform padlock = masterRoot.transform.Find("PadlockAndTag");

            // MINIMUM TARGETED ADJUSTMENT ONLY:
            // 1. Separate Safety Station components to eliminate internal overlap
            if (controlPanel != null)
            {
                controlPanel.position = new Vector3(-0.75f, 0.85f, -1.35f);
                Debug.Log($"[Precision Fix] Shifted ControlPanel to {controlPanel.position}");
            }

            if (eStop != null)
            {
                eStop.position = new Vector3(0.0f, 1.15f, -1.30f);
                Debug.Log($"[Precision Fix] Preserved EmergencyStop position at {eStop.position}");
            }

            if (isolator != null)
            {
                isolator.position = new Vector3(0.75f, 1.15f, -1.35f);
                Debug.Log($"[Precision Fix] Shifted MainIsolator to {isolator.position}");
            }

            // 2. Ensure LOTO equipment sits flat on top of table surface
            if (table != null)
            {
                Bounds tBounds = CalculateMeshBounds(table.gameObject);
                float tableTopY = tBounds.max.y > 0.1f ? tBounds.max.y : 1.10f;

                if (hasp != null)
                {
                    hasp.position = new Vector3(table.position.x - 0.15f, tableTopY + 0.015f, table.position.z);
                    hasp.localEulerAngles = new Vector3(90, 0, 0);
                }

                if (padlock != null)
                {
                    padlock.position = new Vector3(table.position.x + 0.15f, tableTopY + 0.015f, table.position.z);
                    padlock.localEulerAngles = new Vector3(0, 45, 0);
                }

                Debug.Log($"[Precision Fix] LOTO Hasp and Padlock positioned on table top at Y={tableTopY + 0.015f:F3}");
            }

            // OVERLAP EVALUATION
            Debug.Log("--- PRECISION OVERLAP EVALUATION ---");
            Dictionary<string, Bounds> majorBounds = new Dictionary<string, Bounds>();
            string[] majorCheckNodes = new string[] { "ConveyorAssembly", "FeedHopper", "DriveMotor", "ControlPanel", "EmergencyStop", "MainIsolator", "SteelWorkstationTable" };
            foreach (var nodeName in majorCheckNodes)
            {
                Transform tr = masterRoot.transform.Find(nodeName);
                if (tr != null)
                {
                    Bounds b = CalculateMeshBounds(tr.gameObject);
                    if (b.size.magnitude > 0.01f) majorBounds[nodeName] = b;
                }
            }

            int overlapCount = 0;
            List<string> keys = new List<string>(majorBounds.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                for (int j = i + 1; j < keys.Count; j++)
                {
                    string k1 = keys[i];
                    string k2 = keys[j];
                    if ((k1 == "FeedHopper" && k2 == "ConveyorAssembly") || (k2 == "FeedHopper" && k1 == "ConveyorAssembly"))
                        continue;

                    if (majorBounds[k1].Intersects(majorBounds[k2]))
                    {
                        overlapCount++;
                        Debug.LogWarning($"[Overlap Detected] {k1} vs {k2}");
                    }
                    else
                    {
                        Debug.Log($"[Separation Verified] {k1} vs {k2}: SEPARATED cleanly.");
                    }
                }
            }

            Debug.Log($"[Summary Result] Total Major Overlaps = {overlapCount}");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            var reloadScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            bool sceneValid = reloadScene.IsValid();
            Debug.Log($"[Persistence Result] Scene saved and reloaded. Valid = {sceneValid}");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPrecisionFixer] PRECISION CORRECTION PASS COMPLETE.");
            Debug.Log("=========================================================================");
        }

        private static Bounds CalculateMeshBounds(GameObject go)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                if (!hasFirst) { b = r.bounds; hasFirst = true; }
                else { b.Encapsulate(r.bounds); }
            }
            return b;
        }
    }
}
#endif
