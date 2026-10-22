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
    public static class MachineryUnpackAndStage
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string Raw3dDir = "Assets/AR_machinary_faundation/3d/Raw";

        [MenuItem("SurakshaAR/Machinery/Unpack and Stage Native Scene Hierarchy")]
        public static void UnpackAndStage()
        {
            Debug.Log("[MachineryUnpackAndStage] Starting native hierarchy staging with complete unpacking...");

            // 1. Open Scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryUnpackAndStage] Failed to open scene at {ScenePath}");
                return;
            }

            // 2. Fix Main Camera
            GameObject mainCamGo = GameObject.FindWithTag("MainCamera");
            if (mainCamGo == null) mainCamGo = GameObject.Find("Main Camera");
            if (mainCamGo == null)
            {
                Debug.LogError("[MachineryUnpackAndStage] Main Camera not found in scene!");
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

            // 3. Fix XR Origin
            GameObject xrOriginGo = GameObject.Find("XR Origin");
            if (xrOriginGo != null)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(xrOriginGo);
                GameObject camOffset = GameObject.Find("Camera Offset");

                XROrigin[] xrOrigins = xrOriginGo.GetComponents<XROrigin>();
                XROrigin primaryOrigin = null;
                if (xrOrigins.Length > 0)
                {
                    primaryOrigin = xrOrigins[0];
                    for (int i = 1; i < xrOrigins.Length; i++)
                    {
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
                }

                if (xrOriginGo.GetComponent<ARPlaneManager>() == null) xrOriginGo.AddComponent<ARPlaneManager>();
                if (xrOriginGo.GetComponent<ARRaycastManager>() == null) xrOriginGo.AddComponent<ARRaycastManager>();
            }

            // 4. Create / Populate MachineryScenario Hierarchy
            GameObject rootObj = GameObject.Find("MachineryScenario");
            if (rootObj == null)
            {
                rootObj = new GameObject("MachineryScenario");
            }
            rootObj.transform.position = Vector3.zero;
            rootObj.transform.rotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;
            rootObj.SetActive(true);

            // Clear old children completely
            for (int i = rootObj.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
            }

            GameObject AddUnpackedGLB(string glbFileName, string objectName, Transform parent, Vector3 pos, Vector3 rot, Vector3 scale)
            {
                string glbPath = $"{Raw3dDir}/{glbFileName}.glb";
                GameObject glbAsset = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (glbAsset == null)
                {
                    Debug.LogError($"[MachineryUnpackAndStage] GLB asset missing at: {glbPath}");
                    return null;
                }

                // Instantiate prefab
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(glbAsset, parent);
                if (instance == null)
                {
                    instance = Object.Instantiate(glbAsset, parent);
                }

                if (instance != null)
                {
                    // Completely unpack prefab so GameObjects become native scene hierarchy objects
                    if (PrefabUtility.IsPartOfAnyPrefab(instance))
                    {
                        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }

                    instance.name = objectName;
                    instance.transform.SetParent(parent, false);
                    instance.transform.localPosition = pos;
                    instance.transform.localEulerAngles = rot;
                    instance.transform.localScale = scale;
                    instance.SetActive(true);

                    // Ensure all renderers are enabled and have visible materials
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        r.enabled = true;
                    }
                    Debug.Log($"[MachineryUnpackAndStage] Unpacked {objectName} (Renderers: {renderers.Length}) under {parent.name}");
                }
                return instance;
            }

            // 1. ConveyorAssembly
            AddUnpackedGLB("industrial_conveyor_belt", "ConveyorAssembly", rootObj.transform,
                new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 2. DriveMotor
            AddUnpackedGLB("electric_motor", "DriveMotor", rootObj.transform,
                new Vector3(-0.85f, 0.45f, -0.45f), new Vector3(0f, 90f, 0f), new Vector3(0.4f, 0.4f, 0.4f));

            // 3. JamArea
            AddUnpackedGLB("coal", "JamArea", rootObj.transform,
                new Vector3(0.1f, 0.55f, 0f), new Vector3(15f, 30f, 0f), new Vector3(0.35f, 0.35f, 0.35f));

            // 4. EmergencyStop
            AddUnpackedGLB("emergency_stop_button", "EmergencyStop", rootObj.transform,
                new Vector3(-0.65f, 0.85f, 0.55f), new Vector3(0f, 0f, 0f), new Vector3(0.18f, 0.18f, 0.18f));

            // 5. MainIsolator
            AddUnpackedGLB("switch", "MainIsolator", rootObj.transform,
                new Vector3(1.2f, 0.95f, 0.6f), new Vector3(0f, -90f, 0f), new Vector3(0.3f, 0.3f, 0.3f));

            // 6. ControlPanel
            AddUnpackedGLB("industrial_control_panel_box", "ControlPanel", rootObj.transform,
                new Vector3(1.2f, 0.7f, 0.6f), new Vector3(0f, -90f, 0f), new Vector3(0.5f, 0.5f, 0.5f));

            // 7. FeedHopper
            AddUnpackedGLB("hopper", "FeedHopper", rootObj.transform,
                new Vector3(0.85f, 0.9f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.6f, 0.6f, 0.6f));

            // 8. SteelWorkstationTable
            AddUnpackedGLB("industrial_steel_table", "SteelWorkstationTable", rootObj.transform,
                new Vector3(-1.2f, 0f, -1.2f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

            // 9 & 10 under LOTOAssembly
            GameObject lotoObj = new GameObject("LOTOAssembly");
            lotoObj.transform.SetParent(rootObj.transform, false);
            lotoObj.transform.localPosition = new Vector3(-1.2f, 0f, -1.2f);
            lotoObj.transform.localEulerAngles = Vector3.zero;
            lotoObj.transform.localScale = Vector3.one;

            AddUnpackedGLB("hasp_and_staple", "Hasp", lotoObj.transform,
                new Vector3(-0.2f, 0.82f, 0.1f), new Vector3(90f, 0f, 0f), new Vector3(0.15f, 0.15f, 0.15f));

            AddUnpackedGLB("lockout_tagout_locks_with_key_and_tag", "PadlockAndTag", lotoObj.transform,
                new Vector3(0.2f, 0.82f, 0.1f), new Vector3(0f, 0f, 0f), new Vector3(0.25f, 0.25f, 0.25f));

            // Connect MachineryARPlacementController target
            MachineryARPlacementController placementCtrl = Object.FindFirstObjectByType<MachineryARPlacementController>();
            if (placementCtrl != null)
            {
                SerializedObject serializedCtrl = new SerializedObject(placementCtrl);
                SerializedProperty targetProp = serializedCtrl.FindProperty("machineryScenarioTarget");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = rootObj;
                    serializedCtrl.ApplyModifiedProperties();
                }
            }

            // 5. Save Scene & Assets
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Renderer[] renderers = rootObj.GetComponentsInChildren<Renderer>(true);
            int childCount = rootObj.transform.childCount;
            Debug.Log($"[MachineryUnpackAndStage] SUCCESS! MachineryScenario populated with {childCount} explicit child GameObjects and {renderers.Length} total renderers!");
        }
    }
}
#endif
