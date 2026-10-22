#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPreservationFinalVerifier
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Execute Preservation & Final Verification")]
        public static void ExecuteTargetedCorrectionAndVerification()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPreservationFinalVerifier] STARTING PRECISION CORRECTION & VERIFICATION...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[PreservationVerifier] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
                Debug.Log("[PreservationVerifier] Created MachineryScenario root.");
            }

            // 1. LOCATE OR RE-PARENT HASP & PADLOCK
            Transform haspTr = masterRoot.transform.Find("Hasp");
            Transform padlockTr = masterRoot.transform.Find("PadlockAndTag");

            if (haspTr == null || padlockTr == null)
            {
                // Search scene for any existing Hasp / Padlock objects
                GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var go in allObjects)
                {
                    string lName = go.name.ToLower();
                    if (haspTr == null && (lName.Contains("hasp") || lName.Contains("staple")))
                    {
                        if (go.transform.parent != masterRoot.transform)
                        {
                            go.transform.SetParent(masterRoot.transform, true);
                        }
                        go.name = "Hasp";
                        haspTr = go.transform;
                        Debug.Log($"[PreservationVerifier] Located and parented Hasp object '{go.name}'.");
                    }
                    else if (padlockTr == null && (lName.Contains("padlock") || lName.Contains("lock_and_tag") || lName.Contains("tag")))
                    {
                        if (go.transform.parent != masterRoot.transform)
                        {
                            go.transform.SetParent(masterRoot.transform, true);
                        }
                        go.name = "PadlockAndTag";
                        padlockTr = go.transform;
                        Debug.Log($"[PreservationVerifier] Located and parented PadlockAndTag object '{go.name}'.");
                    }
                }
            }

            // If still missing, check GLB models or prefabs to ensure required 10 categories exist
            if (haspTr == null)
            {
                string[] glbGuids = AssetDatabase.FindAssets("hasp_and_staple t:Model");
                if (glbGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(glbGuids[0]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, masterRoot.transform);
                        inst.name = "Hasp";
                        inst.transform.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
                        haspTr = inst.transform;
                        Debug.Log("[PreservationVerifier] Instantiated missing Hasp model from asset.");
                    }
                }
            }

            if (padlockTr == null)
            {
                string[] glbGuids = AssetDatabase.FindAssets("padlock t:Model");
                if (glbGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(glbGuids[0]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, masterRoot.transform);
                        inst.name = "PadlockAndTag";
                        inst.transform.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
                        padlockTr = inst.transform;
                        Debug.Log("[PreservationVerifier] Instantiated missing PadlockAndTag model from asset.");
                    }
                }
            }

            // 2. PRECISION ADJUSTMENT FOR OVERLAPPING SAFETY & LOTO COMPONENTS ONLY
            Transform controlPanel = masterRoot.transform.Find("ControlPanel");
            Transform eStop = masterRoot.transform.Find("EmergencyStop");
            Transform isolator = masterRoot.transform.Find("MainIsolator");
            Transform table = masterRoot.transform.Find("SteelWorkstationTable");

            // ControlPanel: shift left to X = -0.75m
            if (controlPanel != null)
            {
                controlPanel.position = new Vector3(-0.75f, 0.85f, -1.35f);
                Debug.Log($"[PreservationVerifier] ControlPanel positioned at {controlPanel.position}");
            }

            // EmergencyStop: preserve at X = 0.0m
            if (eStop != null)
            {
                eStop.position = new Vector3(0.0f, 1.15f, -1.30f);
                Debug.Log($"[PreservationVerifier] EmergencyStop preserved at {eStop.position}");
            }

            // MainIsolator: shift right to X = +0.80m (mesh bounds center X=0.76m, size X=0.86m -> range +0.33m to +1.19m)
            if (isolator != null)
            {
                isolator.position = new Vector3(0.80f, 1.15f, -1.35f);
                Debug.Log($"[PreservationVerifier] MainIsolator positioned at {isolator.position}");
            }

            // SteelWorkstationTable: position at X = +2.10m, Z = -1.45m (mesh bounds range +1.655m to +2.545m)
            if (table != null)
            {
                table.position = new Vector3(2.10f, 0.0f, -1.45f);
                Bounds tBounds = CalculateMeshBounds(table.gameObject);
                float tableTopY = tBounds.max.y > 0.1f ? tBounds.max.y : 1.10f;

                if (haspTr != null)
                {
                    haspTr.position = new Vector3(1.95f, tableTopY + 0.015f, -1.45f);
                    haspTr.localEulerAngles = new Vector3(90f, 0f, 0f);
                }

                if (padlockTr != null)
                {
                    padlockTr.position = new Vector3(2.25f, tableTopY + 0.015f, -1.45f);
                    padlockTr.localEulerAngles = new Vector3(0f, 45f, 0f);
                }

                Debug.Log($"[PreservationVerifier] SteelWorkstationTable positioned at {table.position}, tableTopY={tableTopY:F3}");
            }

            // 3. COMPLETE REQUIRED OBJECT CHECKLIST (10 CATEGORIES)
            string[] requiredNodes = new string[]
            {
                "ConveyorAssembly",
                "FeedHopper",
                "DriveMotor",
                "ControlPanel",
                "EmergencyStop",
                "MainIsolator",
                "SteelWorkstationTable",
                "Hasp",
                "PadlockAndTag",
                "JamArea"
            };

            Debug.Log("--- REQUIRED OBJECT CHECKLIST & TRANSFORMS ---");
            int missingCount = 0;
            foreach (var nodeName in requiredNodes)
            {
                Transform tr = masterRoot.transform.Find(nodeName);
                if (tr != null)
                {
                    Bounds b = CalculateMeshBounds(tr.gameObject);
                    Debug.Log($"[PRESENT] '{nodeName}' | Active: {tr.gameObject.activeSelf} | Pos: {tr.position} | Scale: {tr.localScale} | Bounds: {b.size}");
                }
                else
                {
                    missingCount++;
                    Debug.LogError($"[MISSING] Node '{nodeName}' NOT FOUND under MachineryScenario!");
                }
            }

            // 4. OVERLAP EVALUATION
            Debug.Log("--- PAIRWISE BOUNDS OVERLAP EVALUATION ---");
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
                        continue; // intentional spout alignment

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

            Debug.Log($"[Inspection Summary] Missing Nodes: {missingCount} | Major Overlaps: {overlapCount}");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            var reloadScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            bool sceneValid = reloadScene.IsValid();
            Debug.Log($"[Persistence Verification] Scene saved and reloaded. Valid = {sceneValid}");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPreservationFinalVerifier] CORRECTION & VERIFICATION COMPLETE.");
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
