#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.XR;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneStager
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string PrefabDir = "Assets/AR_machinary_faundation/prefabs";
        private const string Raw3dDir = "Assets/AR_machinary_faundation/3d/Raw";
        private const string MaterialDir = "Assets/AR_machinary_faundation/materials";
        private const string OutputDir = "Builds";

        public struct ModelMapping
        {
            public string gameObjectName;
            public string glbFileName;
            public string materialName;
            public Vector3 localPosition;
            public Vector3 localRotation;
            public Vector3 localScale;
        }

        [MenuItem("SurakshaAR/Machinery/Stage Machinery Scene (Editor View Fix)")]
        public static void StageScene()
        {
            Debug.Log("[MachinerySceneStager] Starting full native Unity Editor scene staging...");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachinerySceneStager] Failed to open scene at {ScenePath}");
                return;
            }

            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);
            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

            // 1. Verify and setup Main Camera components to match Fire reference 1-to-1
            GameObject mainCamGo = GameObject.FindWithTag("MainCamera");
            if (mainCamGo == null) mainCamGo = GameObject.Find("Main Camera");

            if (mainCamGo != null)
            {
                if (mainCamGo.GetComponent<Camera>() == null) mainCamGo.AddComponent<Camera>();
                if (mainCamGo.GetComponent<AudioListener>() == null) mainCamGo.AddComponent<AudioListener>();
                if (mainCamGo.GetComponent<ARCameraManager>() == null) mainCamGo.AddComponent<ARCameraManager>();
                if (mainCamGo.GetComponent<ARCameraBackground>() == null) mainCamGo.AddComponent<ARCameraBackground>();
                if (mainCamGo.GetComponent<TrackedPoseDriver>() == null) mainCamGo.AddComponent<TrackedPoseDriver>();

                Camera cam = mainCamGo.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = Color.black;
                    cam.nearClipPlane = 0.1f;
                    cam.farClipPlane = 20f;
                }

                Debug.Log("[MachinerySceneStager] Verified Main Camera components: Camera, AudioListener, ARCameraManager, ARCameraBackground, TrackedPoseDriver.");
            }

            // 2. Verify and setup XR Origin components to match Fire reference 1-to-1
            GameObject xrOriginGo = GameObject.Find("XR Origin");
            if (xrOriginGo != null)
            {
                if (xrOriginGo.GetComponent<ARPlaneManager>() == null)
                {
                    xrOriginGo.AddComponent<ARPlaneManager>();
                    Debug.Log("[MachinerySceneStager] Added ARPlaneManager to XR Origin.");
                }

                if (xrOriginGo.GetComponent<ARRaycastManager>() == null)
                {
                    xrOriginGo.AddComponent<ARRaycastManager>();
                    Debug.Log("[MachinerySceneStager] Added ARRaycastManager to XR Origin.");
                }

                MachineryARPlacementController placementCtrl = xrOriginGo.GetComponent<MachineryARPlacementController>();
                if (placementCtrl == null)
                {
                    placementCtrl = xrOriginGo.AddComponent<MachineryARPlacementController>();
                    Debug.Log("[MachinerySceneStager] Added MachineryARPlacementController to XR Origin.");
                }

                MachineryARRuntimeDiagnostics diagnostics = xrOriginGo.GetComponent<MachineryARRuntimeDiagnostics>();
                if (diagnostics == null)
                {
                    diagnostics = xrOriginGo.AddComponent<MachineryARRuntimeDiagnostics>();
                    Debug.Log("[MachinerySceneStager] Added MachineryARRuntimeDiagnostics to XR Origin.");
                }
            }

            // 3. Find or create MachineryScenario root
            GameObject scenarioRoot = GameObject.Find("MachineryScenario");
            if (scenarioRoot == null)
            {
                scenarioRoot = new GameObject("MachineryScenario");
            }
            scenarioRoot.transform.position = Vector3.zero;
            scenarioRoot.transform.rotation = Quaternion.identity;
            scenarioRoot.transform.localScale = Vector3.one;
            scenarioRoot.SetActive(true);

            // Destroy existing children under MachineryScenario to build clean native hierarchy
            int childCount = scenarioRoot.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(scenarioRoot.transform.GetChild(i).gameObject);
            }

            ModelMapping[] mappings = new ModelMapping[]
            {
                new ModelMapping {
                    gameObjectName = "ConveyorAssembly",
                    glbFileName = "industrial_conveyor_belt.glb",
                    materialName = "mat_conveyor.mat",
                    localPosition = new Vector3(0f, 0f, 0f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ModelMapping {
                    gameObjectName = "DriveMotor",
                    glbFileName = "electric_motor.glb",
                    materialName = "mat_motor.mat",
                    localPosition = new Vector3(-1.5f, 0.4f, 0f),
                    localRotation = new Vector3(0f, 90f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ModelMapping {
                    gameObjectName = "JamArea",
                    glbFileName = "coal.glb",
                    materialName = "mat_coal.mat",
                    localPosition = new Vector3(0f, 0.6f, 0f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                },
                new ModelMapping {
                    gameObjectName = "EmergencyStop",
                    glbFileName = "emergency_stop_button.glb",
                    materialName = "mat_estop.mat",
                    localPosition = new Vector3(0.2f, 0.8f, -0.9f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ModelMapping {
                    gameObjectName = "MainIsolator",
                    glbFileName = "switch.glb",
                    materialName = "mat_isolator.mat",
                    localPosition = new Vector3(1.2f, 0.6f, -1.0f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ModelMapping {
                    gameObjectName = "ControlPanel",
                    glbFileName = "industrial_control_panel_box.glb",
                    materialName = "mat_control_panel.mat",
                    localPosition = new Vector3(0.8f, 0.5f, -1.0f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ModelMapping {
                    gameObjectName = "FeedHopper",
                    glbFileName = "hopper.glb",
                    materialName = "mat_hopper.mat",
                    localPosition = new Vector3(1.5f, 1.2f, 0f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                },
                new ModelMapping {
                    gameObjectName = "SteelWorkstationTable",
                    glbFileName = "industrial_steel_table.glb",
                    materialName = "mat_steel_table.mat",
                    localPosition = new Vector3(-1.2f, 0f, -1.2f),
                    localRotation = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            };

            foreach (var map in mappings)
            {
                CreateAndStageObject(scenarioRoot, map);
            }

            // Create LOTOAssembly parent under scenarioRoot
            GameObject lotoAssembly = new GameObject("LOTOAssembly");
            lotoAssembly.transform.SetParent(scenarioRoot.transform, false);
            lotoAssembly.transform.localPosition = new Vector3(-1.2f, 0.85f, -1.2f);
            lotoAssembly.transform.localRotation = Quaternion.identity;
            lotoAssembly.transform.localScale = Vector3.one;

            ModelMapping haspMap = new ModelMapping
            {
                gameObjectName = "Hasp",
                glbFileName = "hasp_and_staple.glb",
                materialName = "mat_hasp.mat",
                localPosition = new Vector3(0f, 0.05f, 0f),
                localRotation = new Vector3(0f, 0f, 0f),
                localScale = new Vector3(1f, 1f, 1f)
            };
            CreateAndStageObject(lotoAssembly, haspMap);

            ModelMapping lockMap = new ModelMapping
            {
                gameObjectName = "PadlockAndTag",
                glbFileName = "lockout_tagout_locks_with_key_and_tag.glb",
                materialName = "mat_padlock_tag.mat",
                localPosition = new Vector3(0.15f, 0.05f, 0f),
                localRotation = new Vector3(0f, 0f, 0f),
                localScale = new Vector3(1f, 1f, 1f)
            };
            CreateAndStageObject(lotoAssembly, lockMap);

            // Connect MachineryARPlacementController target reference
            if (xrOriginGo != null)
            {
                MachineryARPlacementController placementCtrl = xrOriginGo.GetComponent<MachineryARPlacementController>();
                if (placementCtrl != null)
                {
                    SerializedObject serializedCtrl = new SerializedObject(placementCtrl);
                    SerializedProperty targetProp = serializedCtrl.FindProperty("machineryScenarioTarget");
                    if (targetProp != null)
                    {
                        targetProp.objectReferenceValue = scenarioRoot;
                        serializedCtrl.ApplyModifiedProperties();
                        Debug.Log("[MachinerySceneStager] Connected machineryScenarioTarget to MachineryARPlacementController.");
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MachinerySceneStager] SUCCESS! MachineryConveyorJam scene staged natively and saved.");
        }

        private static void CreateAndStageObject(GameObject parent, ModelMapping map)
        {
            string glbPath = Path.Combine(Raw3dDir, map.glbFileName);
            GameObject glbAsset = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);

            GameObject instance = null;
            if (glbAsset != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(glbAsset);
                instance.name = map.gameObjectName;
            }
            else
            {
                Debug.LogWarning($"[MachinerySceneStager] GLB asset not found at {glbPath}, creating GameObject '{map.gameObjectName}'");
                instance = new GameObject(map.gameObjectName);
            }

            instance.transform.SetParent(parent.transform, false);
            instance.transform.localPosition = map.localPosition;
            instance.transform.localRotation = Quaternion.Euler(map.localRotation);
            instance.transform.localScale = map.localScale;

            string matPath = Path.Combine(MaterialDir, map.materialName);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = true;
                if (mat != null)
                {
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    r.sharedMaterials = mats;
                }
            }

            string prefabPath = Path.Combine(PrefabDir, map.gameObjectName + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Debug.Log($"[MachinerySceneStager] Staged '{map.gameObjectName}' (Renderers: {renderers.Length}) -> Prefab saved at {prefabPath}");
        }
    }
}
#endif
