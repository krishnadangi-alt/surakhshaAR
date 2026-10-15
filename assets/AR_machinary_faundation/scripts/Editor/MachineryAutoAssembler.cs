#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryAutoAssembler
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string Raw3DFolder = "Assets/AR_machinary_faundation/3d/Raw";
        private const string PrefabsFolder = "Assets/AR_machinary_faundation/prefabs";

        [MenuItem("SurakshaAR/Machinery/Populate Real 3D Scene Now")]
        public static void EnsureMachineryScenePopulated()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                if (System.IO.File.Exists(ScenePath))
                {
                    activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
                else
                {
                    return;
                }
            }

            GameObject root = GameObject.Find("MachineryScenario");
            if (root == null)
            {
                root = new GameObject("MachineryScenario");
            }

            // Always ensure root is active so models are visible in Unity Scene View
            root.SetActive(true);

            // Check if root already has real mesh renderers
            MeshRenderer[] existingRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
            if (existingRenderers != null && existingRenderers.Length >= 5)
            {
                // Already fully populated with 3D mesh models
                return;
            }

            Debug.Log("[MachineryAutoAssembler] Populating MachineryScenario with real 3D models...");

            // Clear old empty transform children
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            }

            GameObject AddModel(string glbFileName, string nodeName, Transform parent, Vector3 localPos, Vector3 localRot, Vector3 localScale)
            {
                string glbPath = $"{Raw3DFolder}/{glbFileName}.glb";
                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (modelPrefab == null)
                {
                    Debug.LogError($"[MachineryAutoAssembler] Missing 3D GLB model at: {glbPath}");
                    return null;
                }

                // Create prefab asset in prefabs directory if needed
                if (!AssetDatabase.IsValidFolder(PrefabsFolder))
                {
                    AssetDatabase.CreateFolder("Assets/AR_machinary_faundation", "prefabs");
                }
                string prefabPath = $"{PrefabsFolder}/{nodeName}.prefab";
                GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (savedPrefab == null)
                {
                    savedPrefab = PrefabUtility.SaveAsPrefabAsset(modelPrefab, prefabPath);
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab != null ? savedPrefab : modelPrefab, parent);
                if (instance != null)
                {
                    instance.name = nodeName;
                    instance.transform.localPosition = localPos;
                    instance.transform.localEulerAngles = localRot;
                    instance.transform.localScale = localScale;
                    instance.SetActive(true);
                }
                return instance;
            }

            // Populate Main Assembly
            AddModel("industrial_conveyor_belt", "ConveyorAssembly", root.transform, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            AddModel("electric_motor", "DriveMotor", root.transform, new Vector3(-0.85f, 0.45f, -0.45f), new Vector3(0, 90, 0), new Vector3(0.4f, 0.4f, 0.4f));
            AddModel("coal", "JamArea", root.transform, new Vector3(0.1f, 0.55f, 0), new Vector3(15, 30, 0), new Vector3(0.35f, 0.35f, 0.35f));
            AddModel("emergency_stop_button", "EmergencyStop", root.transform, new Vector3(-0.65f, 0.85f, 0.55f), new Vector3(0, 0, 0), new Vector3(0.18f, 0.18f, 0.18f));
            AddModel("switch", "MainIsolator", root.transform, new Vector3(1.2f, 0.95f, 0.6f), new Vector3(0, -90, 0), new Vector3(0.3f, 0.3f, 0.3f));
            AddModel("industrial_control_panel_box", "ControlPanel", root.transform, new Vector3(1.2f, 0.7f, 0.6f), new Vector3(0, -90, 0), new Vector3(0.5f, 0.5f, 0.5f));
            AddModel("hopper", "FeedHopper", root.transform, new Vector3(0.85f, 0.9f, 0), new Vector3(0, 0, 0), new Vector3(0.6f, 0.6f, 0.6f));

            // LOTO Assembly Sub-hierarchy
            GameObject lotoParent = new GameObject("LOTOAssembly");
            lotoParent.transform.SetParent(root.transform, false);
            lotoParent.transform.localPosition = new Vector3(1.4f, 0, -0.6f);
            lotoParent.transform.localEulerAngles = Vector3.zero;
            lotoParent.transform.localScale = Vector3.one;

            AddModel("hasp_and_staple", "Hasp", lotoParent.transform, new Vector3(-0.2f, 0.82f, 0.1f), new Vector3(90, 0, 0), new Vector3(0.15f, 0.15f, 0.15f));
            AddModel("lockout_tagout_locks_with_key_and_tag", "PadlockAndTag", lotoParent.transform, new Vector3(0.2f, 0.82f, 0.1f), new Vector3(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f));
            AddModel("industrial_steel_table", "SteelWorkstationTable", lotoParent.transform, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();

            Debug.Log("[MachineryAutoAssembler] MachineryConveyorJam scene assembly COMPLETE and SAVED.");
        }
    }
}
#endif
