#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFixAndStageScene
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string Raw3dDir = "Assets/AR_machinary_faundation/3d/Raw";

        [MenuItem("SurakshaAR/Machinery/Fix and Stage Machinery Scene")]
        public static void FixAndStage()
        {
            Debug.Log("[MachineryFixAndStage] Starting comprehensive scene fix and staging...");

            // 1. Open Scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryFixAndStage] Failed to open scene at {ScenePath}");
                return;
            }

            // 2. Fix Main Camera
            GameObject mainCamGo = GameObject.FindWithTag("MainCamera");
            if (mainCamGo == null) mainCamGo = GameObject.Find("Main Camera");
            if (mainCamGo == null)
            {
                Debug.LogError("[MachineryFixAndStage] Main Camera not found in scene!");
                return;
            }

            Camera cam = mainCamGo.GetComponent<Camera>();
            if (cam == null) cam = mainCamGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 20f;
            cam.depth = 0;

            if (mainCamGo.GetComponent<AudioListener>() == null) mainCamGo.AddComponent<AudioListener>();
            
            ARCameraManager camMgr = mainCamGo.GetComponent<ARCameraManager>();
            if (camMgr == null) camMgr = mainCamGo.AddComponent<ARCameraManager>();
            camMgr.autoFocusRequested = true;

            if (mainCamGo.GetComponent<ARCameraBackground>() == null) mainCamGo.AddComponent<ARCameraBackground>();

            TrackedPoseDriver tpd = mainCamGo.GetComponent<TrackedPoseDriver>();
            if (tpd == null) tpd = mainCamGo.AddComponent<TrackedPoseDriver>();

            Debug.Log("[MachineryFixAndStage] Main Camera components verified.");

            // 3. Fix XR Origin
            GameObject xrOriginGo = GameObject.Find("XR Origin");
            if (xrOriginGo == null)
            {
                Debug.LogError("[MachineryFixAndStage] XR Origin GameObject not found!");
                return;
            }

            // Remove any missing/corrupted script components
            int removedMissing = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(xrOriginGo);
            if (removedMissing > 0)
            {
                Debug.Log($"[MachineryFixAndStage] Removed {removedMissing} missing/corrupted MonoBehaviours from XR Origin.");
            }

            GameObject camOffset = GameObject.Find("Camera Offset");

            XROrigin[] xrOrigins = xrOriginGo.GetComponents<XROrigin>();
            XROrigin primaryOrigin = null;
            if (xrOrigins.Length > 0)
            {
                primaryOrigin = xrOrigins[0];
                for (int i = 1; i < xrOrigins.Length; i++)
                {
                    Debug.LogWarning($"[MachineryFixAndStage] Removing duplicate XROrigin component #{i}");
                    Object.DestroyImmediate(xrOrigins[i]);
                }
            }
            else
            {
                primaryOrigin = xrOriginGo.AddComponent<XROrigin>();
            }

            if (primaryOrigin != null)
            {
                primaryOrigin.Camera = cam;
                primaryOrigin.CameraFloorOffsetObject = camOffset;
                primaryOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                Debug.Log($"[MachineryFixAndStage] Configured XROrigin with Camera={cam.name}");
            }

            if (xrOriginGo.GetComponent<ARPlaneManager>() == null) xrOriginGo.AddComponent<ARPlaneManager>();
            if (xrOriginGo.GetComponent<ARRaycastManager>() == null) xrOriginGo.AddComponent<ARRaycastManager>();

            MachineryARPlacementController placementCtrl = xrOriginGo.GetComponent<MachineryARPlacementController>();
            if (placementCtrl == null) placementCtrl = xrOriginGo.AddComponent<MachineryARPlacementController>();

            MachineryARRuntimeDiagnostics diagnostics = xrOriginGo.GetComponent<MachineryARRuntimeDiagnostics>();
            if (diagnostics == null) diagnostics = xrOriginGo.AddComponent<MachineryARRuntimeDiagnostics>();

            // 4. Create / Populate MachineryScenario Hierarchy with REAL prefabs and renderers
            GameObject rootObj = GameObject.Find("MachineryScenario");
            if (rootObj == null)
            {
                rootObj = new GameObject("MachineryScenario");
            }
            rootObj.transform.position = Vector3.zero;
            rootObj.transform.rotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;
            rootObj.SetActive(true);

            // Clear old empty children
            for (int i = rootObj.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
            }

            GameObject InstantiateGLBModel(string modelName, string instanceName, Transform parent, Vector3 pos, Vector3 rot, Vector3 scale)
            {
                string glbPath = $"{Raw3dDir}/{modelName}.glb";
                GameObject glbAsset = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (glbAsset == null)
                {
                    Debug.LogError($"[MachineryFixAndStage] Model GLB asset not found at: {glbPath}");
                    return null;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(glbAsset, parent);
                if (instance == null)
                {
                    instance = Object.Instantiate(glbAsset, parent);
                }

                if (instance != null)
                {
                    instance.name = instanceName;
                    instance.transform.localPosition = pos;
                    instance.transform.localEulerAngles = rot;
                    instance.transform.localScale = scale;
                    instance.SetActive(true);

                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                    Debug.Log($"[MachineryFixAndStage] Instantiated {instanceName} (Renderers: {renderers.Length}) under {parent.name}");
                }
                return instance;
            }

            // 1. ConveyorAssembly
            InstantiateGLBModel("industrial_conveyor_belt", "ConveyorAssembly", rootObj.transform,
                new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 2. DriveMotor
            InstantiateGLBModel("electric_motor", "DriveMotor", rootObj.transform,
                new Vector3(-1.5f, 0.4f, 0f), new Vector3(0f, 90f, 0f), new Vector3(1f, 1f, 1f));

            // 3. JamArea (Coal wedged in conveyor)
            InstantiateGLBModel("coal", "JamArea", rootObj.transform,
                new Vector3(0f, 0.6f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.8f, 0.8f, 0.8f));

            // 4. EmergencyStop
            InstantiateGLBModel("emergency_stop_button", "EmergencyStop", rootObj.transform,
                new Vector3(0.2f, 0.8f, -0.9f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 5. MainIsolator
            InstantiateGLBModel("switch", "MainIsolator", rootObj.transform,
                new Vector3(1.2f, 0.6f, -1.0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 6. ControlPanel
            InstantiateGLBModel("industrial_control_panel_box", "ControlPanel", rootObj.transform,
                new Vector3(0.8f, 0.5f, -1.0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 7. FeedHopper
            InstantiateGLBModel("hopper", "FeedHopper", rootObj.transform,
                new Vector3(1.5f, 1.2f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 8. SteelWorkstationTable
            InstantiateGLBModel("industrial_steel_table", "SteelWorkstationTable", rootObj.transform,
                new Vector3(-1.2f, 0f, -1.2f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 9 & 10 under LOTOAssembly
            GameObject lotoObj = new GameObject("LOTOAssembly");
            lotoObj.transform.SetParent(rootObj.transform, false);
            lotoObj.transform.localPosition = new Vector3(-1.2f, 0.85f, -1.2f);
            lotoObj.transform.localEulerAngles = Vector3.zero;
            lotoObj.transform.localScale = Vector3.one;

            InstantiateGLBModel("hasp_and_staple", "Hasp", lotoObj.transform,
                new Vector3(0f, 0.05f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            InstantiateGLBModel("lockout_tagout_locks_with_key_and_tag", "PadlockAndTag", lotoObj.transform,
                new Vector3(0.15f, 0.05f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // Connect MachineryARPlacementController target reference using SerializedObject
            if (placementCtrl != null)
            {
                SerializedObject serializedCtrl = new SerializedObject(placementCtrl);
                SerializedProperty targetProp = serializedCtrl.FindProperty("machineryScenarioTarget");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = rootObj;
                    serializedCtrl.ApplyModifiedProperties();
                    Debug.Log("[MachineryFixAndStage] Connected MachineryScenario target to MachineryARPlacementController.");
                }
            }

            // 5. Save Scene and Assets
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Renderer[] totalRenderers = rootObj.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[MachineryFixAndStage] SUCCESS: Scene fully fixed and saved! Total renderers under MachineryScenario: {totalRenderers.Length}");
        }
    }
}
#endif
