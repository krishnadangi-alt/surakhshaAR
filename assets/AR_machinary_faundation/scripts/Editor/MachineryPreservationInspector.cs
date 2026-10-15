#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPreservationInspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Inspect Preservation State (Read Only)")]
        public static void InspectSceneForPreservation()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPreservationInspector] READ-ONLY SCENE INSPECTION PASS...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[PreservationInspector] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                Debug.LogError("[PreservationInspector] MachineryScenario root object missing!");
                return;
            }

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

            Debug.Log("--- CURRENT OBJECT TRANSFORMS & BOUNDS ---");
            foreach (var nodeName in requiredNodes)
            {
                Transform tr = masterRoot.transform.Find(nodeName);
                if (tr != null)
                {
                    Bounds b = CalculateMeshBounds(tr.gameObject);
                    Debug.Log($"Node: '{nodeName}' | Active: {tr.gameObject.activeSelf} | Pos: {tr.position} | Rot: {tr.eulerAngles} | Scale: {tr.localScale} | Bounds Center: {b.center} | Bounds Size: {b.size}");
                }
                else
                {
                    Debug.LogWarning($"Node: '{nodeName}' NOT FOUND under MachineryScenario!");
                }
            }

            // Check overlap pairs
            Debug.Log("--- READ-ONLY BOUNDS OVERLAP EVALUATION ---");
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

            Debug.Log($"[Summary Result] Total Major Overlaps = {overlapCount}");
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPreservationInspector] INSPECTION COMPLETE — NO CHANGES MADE.");
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
