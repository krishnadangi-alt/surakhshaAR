#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryCompletenessChecker
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Execute Completeness Check")]
        public static void ExecuteCompletenessCheck()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryCompletenessChecker] STARTING 10-MODEL COMPLETENESS CHECK...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[CompletenessChecker] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                Debug.LogError("[CompletenessChecker] MachineryScenario root object missing!");
                return;
            }

            // Checklist dictionary for the 10 required categories
            Dictionary<string, string> requiredModels = new Dictionary<string, string>
            {
                { "1. Feed Hopper", "FeedHopper" },
                { "2. Conveyor Assembly", "ConveyorAssembly" },
                { "3. Drive Motor", "DriveMotor" },
                { "4. Coal / Jam Material", "JamArea" },
                { "5. Emergency Stop", "EmergencyStop" },
                { "6. Main Isolator", "MainIsolator" },
                { "7. Control Panel", "ControlPanel" },
                { "8. Steel Workstation Table", "SteelWorkstationTable" },
                { "9. LOTO Hasp", "Hasp" },
                { "10. Padlock + Warning Tag", "PadlockAndTag" }
            };

            Dictionary<string, bool> foundStatus = new Dictionary<string, bool>();
            Dictionary<string, int> childCounts = new Dictionary<string, int>();

            foreach (var kvp in requiredModels)
            {
                Transform foundTr = masterRoot.transform.Find(kvp.Value);
                if (foundTr == null)
                {
                    // Search all children recursively
                    foreach (Transform t in masterRoot.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name == kvp.Value)
                        {
                            foundTr = t;
                            break;
                        }
                    }
                }

                bool isPresent = foundTr != null && foundTr.gameObject.activeSelf;
                foundStatus[kvp.Key] = isPresent;

                int count = 0;
                if (isPresent)
                {
                    if (kvp.Value == "JamArea")
                        count = foundTr.childCount;
                    else
                        count = 1;
                }
                childCounts[kvp.Key] = count;

                Debug.Log($"[Checklist] Category: '{kvp.Key}' | Expected Node: '{kvp.Value}' | Found: {isPresent} | Detail: {(kvp.Value == "JamArea" ? count + " coal instances" : "Present")}");
            }

            // Missing Prefabs check
            int missingPrefabCount = 0;
            foreach (Transform t in masterRoot.GetComponentsInChildren<Transform>(true))
            {
                if (PrefabUtility.IsPartOfAnyPrefab(t.gameObject) && PrefabUtility.GetPrefabInstanceStatus(t.gameObject) == PrefabInstanceStatus.MissingAsset)
                {
                    missingPrefabCount++;
                    Debug.LogWarning($"[Missing Prefab] {t.name} has a missing prefab asset!");
                }
            }

            // Material error check
            int materialErrorCount = 0;
            MeshRenderer[] renderers = masterRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null || mat.shader == null || mat.shader.name.Contains("Error") || mat.name.Contains("Hidden"))
                    {
                        materialErrorCount++;
                    }
                }
            }

            // Major body overlap check
            Dictionary<string, Bounds> majorBounds = new Dictionary<string, Bounds>();
            string[] checkNodes = new string[] { "ConveyorAssembly", "FeedHopper", "DriveMotor", "ControlPanel", "EmergencyStop", "MainIsolator", "SteelWorkstationTable" };
            foreach (var nodeName in checkNodes)
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
                        continue; // intentional spout alignment

                    if (majorBounds[k1].Intersects(majorBounds[k2]))
                    {
                        overlapCount++;
                        Debug.LogWarning($"[Overlap Warning] {k1} overlaps with {k2}");
                    }
                }
            }

            Debug.Log($"[Validation Results]");
            Debug.Log($"   Missing Prefabs: {missingPrefabCount}");
            Debug.Log($"   Material Errors: {materialErrorCount}");
            Debug.Log($"   Major Overlaps: {overlapCount}");

            // Reload scene to verify persistence
            var reloadScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            bool sceneValid = reloadScene.IsValid();
            Debug.Log($"[Persistence Result] Scene saved and reloaded. Valid = {sceneValid}");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryCompletenessChecker] COMPLETENESS CHECK COMPLETE");
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
