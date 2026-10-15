#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryCreateRealARScene
    {
        public const string ARScenePath = "Assets/AR_machinary_faundation/scenes/MachineryARExperience.unity";
        public const string PrefabPath = "Assets/AR_machinary_faundation/prefabs/MachineryScenario_SourceOfTruth.prefab";

        [MenuItem("SurakshaAR/Machinery/2. Generate Separate Machinery AR Experience Scene", priority = 20)]
        public static void BuildARScene()
        {
            Debug.Log("[MachineryAR] Creating separate dedicated AR scene: " + ARScenePath);

            // 1. Create brand new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Directional Light
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.color = Color.white;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 3. AR Session
            GameObject arSessionGo = new GameObject("AR Session");
            var arSession = arSessionGo.AddComponent<ARSession>();
            arSessionGo.AddComponent<ARInputManager>();

            // 4. XR Origin
            GameObject xrOriginGo = new GameObject("XR Origin");
            var xrOrigin = xrOriginGo.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            var planeMgr = xrOriginGo.AddComponent<ARPlaneManager>();
            planeMgr.requestedDetectionMode = PlaneDetectionMode.Horizontal;

            var raycastMgr = xrOriginGo.AddComponent<ARRaycastManager>();
            var anchorMgr = xrOriginGo.AddComponent<ARAnchorManager>();
            var arController = xrOriginGo.AddComponent<MachineryRealARController>();

            // 5. Camera Offset & Camera
            GameObject camOffsetGo = new GameObject("Camera Offset");
            camOffsetGo.transform.SetParent(xrOriginGo.transform, false);

            GameObject camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(camOffsetGo.transform, false);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;

            var camMgr = camGo.AddComponent<ARCameraManager>();
            camGo.AddComponent<ARCameraBackground>();
            var poseDriver = camGo.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            camGo.AddComponent<AudioListener>();

            // Setup input bindings on TrackedPoseDriver
            if (poseDriver != null)
            {
                var posAction = poseDriver.positionInput.action;
                if (posAction != null)
                {
                    posAction.AddBinding("<HandheldARInputDevice>/devicePosition");
                    posAction.AddBinding("<XRHMD>/centerEyePosition");
                }
                var rotAction = poseDriver.rotationInput.action;
                if (rotAction != null)
                {
                    rotAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
                    rotAction.AddBinding("<XRHMD>/centerEyeRotation");
                }
            }

            xrOrigin.CameraFloorOffsetObject = camOffsetGo;
            xrOrigin.Camera = cam;

            // 6. Placement Root and Complete Machinery Setup
            GameObject placementRoot = new GameObject("MachineryARPlacementRoot");
            placementRoot.transform.position = Vector3.zero;
            placementRoot.transform.rotation = Quaternion.identity;
            placementRoot.transform.localScale = Vector3.one * 0.35f;

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefabAsset == null)
            {
                // Fallback to standard prefab if SourceOfTruth not yet created
                prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_machinary_faundation/prefabs/MachineryScenario.prefab");
            }

            if (prefabAsset != null)
            {
                GameObject scenarioInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                scenarioInstance.name = "MachineryScenario";
                scenarioInstance.transform.SetParent(placementRoot.transform, false);
                // Center the equipment on placement root:
                // Components span X from -1.85 to +3.90 (center = +1.025), Z from -1.35 to +1.56 (center = +0.105), Y base = 0.0
                scenarioInstance.transform.localPosition = new Vector3(-1.025f, 0.0f, -0.105f);
                scenarioInstance.transform.localRotation = Quaternion.identity;
                scenarioInstance.transform.localScale = Vector3.one;
                Debug.Log($"[MachineryAR] Successfully instantiated complete machinery hierarchy ({scenarioInstance.transform.childCount} direct children).");
            }
            else
            {
                Debug.LogError("[MachineryAR] CRITICAL: Machinery prefab could not be loaded!");
            }

            // MANDATORY REQUIREMENT: HIDDEN ON STARTUP
            placementRoot.SetActive(false);

            // 7. Minimal AR UI
            GameObject canvasGo = new GameObject("Machinery_AR_UI_Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400);
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.GetComponent<RectTransform>().localScale = Vector3.one;

            // EventSystem if not present
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Top Status Banner
            GameObject bannerGo = new GameObject("StatusBanner");
            bannerGo.transform.SetParent(canvasGo.transform, false);
            var bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.05f, 0.88f);
            bannerRect.anchorMax = new Vector2(0.95f, 0.96f);
            bannerRect.offsetMin = Vector2.zero;
            bannerRect.offsetMax = Vector2.zero;

            var bannerImage = bannerGo.AddComponent<Image>();
            bannerImage.color = new Color(0.05f, 0.08f, 0.12f, 0.82f); // Sleek dark glassmorphism

            // Status Text
            GameObject textGo = new GameObject("StatusText");
            textGo.transform.SetParent(bannerGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 8);
            textRect.offsetMax = new Vector2(-16, -8);

            Text statusText = textGo.AddComponent<Text>();
            statusText.text = "Scan the floor to place the machinery";
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (statusText.font == null) statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.fontSize = 32;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.white;

            // Reset Button (Upper Right)
            GameObject resetBtnGo = new GameObject("ResetButton");
            resetBtnGo.transform.SetParent(canvasGo.transform, false);
            var resetRect = resetBtnGo.AddComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.72f, 0.82f);
            resetRect.anchorMax = new Vector2(0.95f, 0.86f);
            resetRect.offsetMin = Vector2.zero;
            resetRect.offsetMax = Vector2.zero;

            var resetImg = resetBtnGo.AddComponent<Image>();
            resetImg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            Button resetBtn = resetBtnGo.AddComponent<Button>();

            GameObject btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(resetBtnGo.transform, false);
            var btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            Text btnText = btnTextGo.AddComponent<Text>();
            btnText.text = "Reset Placement";
            btnText.font = statusText.font;
            btnText.fontSize = 24;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;

            resetBtnGo.SetActive(false); // Initially hidden

            // 8. Connect Serialized Properties on Controller
            SerializedObject so = new SerializedObject(arController);
            so.FindProperty("machineryPlacementRoot").objectReferenceValue = placementRoot;
            so.FindProperty("raycastManager").objectReferenceValue = raycastMgr;
            so.FindProperty("planeManager").objectReferenceValue = planeMgr;
            so.FindProperty("anchorManager").objectReferenceValue = anchorMgr;
            so.FindProperty("arSession").objectReferenceValue = arSession;
            so.FindProperty("cameraManager").objectReferenceValue = camMgr;
            so.FindProperty("mainCamera").objectReferenceValue = cam;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("resetButton").objectReferenceValue = resetBtn;
            so.FindProperty("placementScale").floatValue = 0.35f;

            Material indicatorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_machinary_faundation/materials/mat_ar_placement_indicator.mat");
            if (indicatorMat != null)
            {
                so.FindProperty("indicatorMaterial").objectReferenceValue = indicatorMat;
            }
            so.ApplyModifiedProperties();

            // 9. Save Scene
            Directory.CreateDirectory(Path.GetDirectoryName(ARScenePath));
            EditorSceneManager.SaveScene(scene, ARScenePath);
            Debug.Log($"[MachineryAR] Scene successfully created and saved to: {ARScenePath}");

            // 10. Configure EditorBuildSettings to include the separate AR scene as Scene 0
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ARScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log($"[MachineryAR] Configured Build Settings with active scene: {ARScenePath}");

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
