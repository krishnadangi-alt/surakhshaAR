#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneBuilder
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string ModelsFolder = "Assets/AR_machinary_faundation/3d/Raw";
        private const string PrefabsFolder = "Assets/AR_machinary_faundation/prefabs";

        [MenuItem("SurakshaAR/Machinery/Build and Populate Machinery Scene")]
        public static void BuildMachineryScene()
        {
            Debug.Log("[MachinerySceneBuilder] Starting Machinery Scene Assembly...");

            // 1. Ensure Prefabs Directory exists
            if (!AssetDatabase.IsValidFolder(PrefabsFolder))
            {
                AssetDatabase.CreateFolder("Assets/AR_machinary_faundation", "prefabs");
            }

            // 2. Open Scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachinerySceneBuilder] Failed to open scene at {ScenePath}");
                return;
            }

            // 3. Find or Create Root GameObject MachineryScenario
            GameObject rootObj = GameObject.Find("MachineryScenario");
            if (rootObj == null)
            {
                rootObj = new GameObject("MachineryScenario");
            }

            // Make root object active so it is fully visible in Unity Editor Scene View
            rootObj.SetActive(true);

            // Clear old children
            int childCount = rootObj.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
            }

            // Helper function to create prefab from GLB and instantiate in scene
            GameObject InstantiateModel(string modelName, string instanceName, Transform parent, Vector3 pos, Vector3 rot, Vector3 scale)
            {
                string glbPath = $"{ModelsFolder}/{modelName}.glb";
                GameObject glbAsset = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (glbAsset == null)
                {
                    Debug.LogError($"[MachinerySceneBuilder] Model asset not found at: {glbPath}");
                    return null;
                }

                // Create or load prefab asset in prefabs folder
                string prefabPath = $"{PrefabsFolder}/{instanceName}.prefab";
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null)
                {
                    prefabAsset = PrefabUtility.SaveAsPrefabAsset(glbAsset, prefabPath);
                    Debug.Log($"[MachinerySceneBuilder] Created prefab at: {prefabPath}");
                }

                // Instantiate prefab instance into scene under parent
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, parent);
                if (instance != null)
                {
                    instance.name = instanceName;
                    instance.transform.localPosition = pos;
                    instance.transform.localEulerAngles = rot;
                    instance.transform.localScale = scale;
                    instance.SetActive(true);
                    Debug.Log($"[MachinerySceneBuilder] Instantiated {instanceName} under {parent.name}");
                }
                return instance;
            }

            // 4. Instantiate required hierarchy:
            // ConveyorAssembly (Main Conveyor Belt)
            InstantiateModel("industrial_conveyor_belt", "ConveyorAssembly", rootObj.transform,
                new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // DriveMotor
            InstantiateModel("electric_motor", "DriveMotor", rootObj.transform,
                new Vector3(-0.85f, 0.45f, -0.45f), new Vector3(0f, 90f, 0f), new Vector3(0.4f, 0.4f, 0.4f));

            // JamArea (Coal wedged in conveyor)
            InstantiateModel("coal", "JamArea", rootObj.transform,
                new Vector3(0.1f, 0.55f, 0f), new Vector3(15f, 30f, 0f), new Vector3(0.35f, 0.35f, 0.35f));

            // EmergencyStop Button
            InstantiateModel("emergency_stop_button", "EmergencyStop", rootObj.transform,
                new Vector3(-0.65f, 0.85f, 0.55f), new Vector3(0f, 0f, 0f), new Vector3(0.18f, 0.18f, 0.18f));

            // MainIsolator Switch
            InstantiateModel("switch", "MainIsolator", rootObj.transform,
                new Vector3(1.2f, 0.95f, 0.6f), new Vector3(0f, -90f, 0f), new Vector3(0.3f, 0.3f, 0.3f));

            // LOTOAssembly parent container
            GameObject lotoAssemblyObj = new GameObject("LOTOAssembly");
            lotoAssemblyObj.transform.SetParent(rootObj.transform, false);
            lotoAssemblyObj.transform.localPosition = new Vector3(1.4f, 0f, -0.6f);
            lotoAssemblyObj.transform.localEulerAngles = Vector3.zero;
            lotoAssemblyObj.transform.localScale = Vector3.one;

            // Children under LOTOAssembly:
            // Hasp
            InstantiateModel("hasp_and_staple", "Hasp", lotoAssemblyObj.transform,
                new Vector3(-0.2f, 0.82f, 0.1f), new Vector3(90f, 0f, 0f), new Vector3(0.15f, 0.15f, 0.15f));

            // PadlockAndTag
            InstantiateModel("lockout_tagout_locks_with_key_and_tag", "PadlockAndTag", lotoAssemblyObj.transform,
                new Vector3(0.2f, 0.82f, 0.1f), new Vector3(0f, 0f, 0f), new Vector3(0.25f, 0.25f, 0.25f));

            // SteelWorkstationTable
            InstantiateModel("industrial_steel_table", "SteelWorkstationTable", lotoAssemblyObj.transform,
                new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // ControlPanel Box
            InstantiateModel("industrial_control_panel_box", "ControlPanel", rootObj.transform,
                new Vector3(1.2f, 0.7f, 0.6f), new Vector3(0f, -90f, 0f), new Vector3(0.5f, 0.5f, 0.5f));

            // FeedHopper
            InstantiateModel("hopper", "FeedHopper", rootObj.transform,
                new Vector3(0.85f, 0.9f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.6f, 0.6f, 0.6f));

            // 5. Save Scene and Assets
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MachinerySceneBuilder] SUCCESS: MachineryConveyorJam.unity successfully populated with {rootObj.transform.childCount} real prefab instances!");
        }
    }
}
#endif
