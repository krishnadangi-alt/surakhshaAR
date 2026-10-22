using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Text;
using System.IO;

public static class MachineryCreateIsolatedTestScene
{
    private const string TargetScenePath = "Assets/AR_machinary_faundation/scenes/MachineryAndroidTest.unity";
    private const string MachineryPrefabPath = "Assets/AR_machinary_faundation/prefabs/MachineryScenario.prefab";

    [MenuItem("Machinery/Create Isolated Android AR Test Scene")]
    public static void CreateTestScene()
    {
        Debug.Log("[TestScene] Creating isolated MachineryAndroidTest.unity scene...");

        // 1. Create new empty scene for isolated Android test
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Directional Light
        GameObject lightGo = new GameObject("Directional Light");
        Light lightComp = lightGo.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.2f;
        lightComp.color = Color.white;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 3. AR Session
        GameObject arSessionGo = new GameObject("AR Session");
        arSessionGo.AddComponent<ARSession>();
        arSessionGo.AddComponent<ARInputManager>();

        // 4. XR Origin
        GameObject xrOriginGo = new GameObject("XR Origin");
        var xrOrigin = xrOriginGo.AddComponent<Unity.XR.CoreUtils.XROrigin>();
        var planeMgr = xrOriginGo.AddComponent<ARPlaneManager>();
        var raycastMgr = xrOriginGo.AddComponent<ARRaycastManager>();
        var anchorMgr = xrOriginGo.AddComponent<ARAnchorManager>();
        var testCtrl = xrOriginGo.AddComponent<SurakshaAR.Machinery.MachineryIsolatedTestController>();

        // 5. Camera Offset & Main Camera
        GameObject camOffsetGo = new GameObject("Camera Offset");
        camOffsetGo.transform.SetParent(xrOriginGo.transform, false);

        GameObject mainCamGo = new GameObject("Main Camera");
        mainCamGo.transform.SetParent(camOffsetGo.transform, false);
        mainCamGo.tag = "MainCamera";

        Camera mainCam = mainCamGo.AddComponent<Camera>();
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0f, 0f, 0f, 1f);
        mainCam.nearClipPlane = 0.1f;
        mainCam.farClipPlane = 50f;

        mainCamGo.AddComponent<AudioListener>();
        var camMgr = mainCamGo.AddComponent<ARCameraManager>();
        var camBg = mainCamGo.AddComponent<ARCameraBackground>();
        var poseDriver = mainCamGo.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();

        xrOrigin.CameraFloorOffsetObject = camOffsetGo;
        xrOrigin.Camera = mainCam;

        // 6. MachineryARPlacementRoot & Approved Machinery Scenario Prefab
        GameObject placementRoot = new GameObject("MachineryARPlacementRoot");
        
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MachineryPrefabPath);
        if (prefabAsset != null)
        {
            GameObject scenarioInst = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            scenarioInst.name = "MachineryScenario";
            scenarioInst.transform.SetParent(placementRoot.transform, false);
            Debug.Log("[TestScene] Successfully instantiated approved MachineryScenario prefab.");
        }
        else
        {
            Debug.LogError($"[TestScene] Failed to load MachineryScenario prefab at: {MachineryPrefabPath}");
        }

        placementRoot.SetActive(false); // MANDATORY REQUIREMENT: HIDDEN ON STARTUP

        // Connect controller references
        var so = new SerializedObject(testCtrl);
        so.FindProperty("machineryPlacementRoot").objectReferenceValue = placementRoot;
        so.FindProperty("raycastManager").objectReferenceValue = raycastMgr;
        so.FindProperty("planeManager").objectReferenceValue = planeMgr;
        so.FindProperty("arSession").objectReferenceValue = arSessionGo.GetComponent<ARSession>();
        so.FindProperty("cameraManager").objectReferenceValue = camMgr;
        so.FindProperty("mainCamera").objectReferenceValue = mainCam;

        // 7. Build Minimal Test UI
        GameObject canvasGo = new GameObject("Test UI Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Status Panel (Top Center)
        GameObject statusGo = new GameObject("StatusText");
        statusGo.transform.SetParent(canvasGo.transform, false);
        Text statusTxt = statusGo.AddComponent<Text>();
        statusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusTxt.fontSize = 28;
        statusTxt.alignment = TextAnchor.MiddleCenter;
        statusTxt.color = Color.yellow;
        statusTxt.text = "MACHINERY AR TEST\nScan floor to detect surface.";

        RectTransform statusRt = statusGo.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0.1f, 0.82f);
        statusRt.anchorMax = new Vector2(0.9f, 0.96f);
        statusRt.offsetMin = Vector2.zero;
        statusRt.offsetMax = Vector2.zero;

        // Reset Button (Bottom Center)
        GameObject btnGo = new GameObject("ResetButton");
        btnGo.transform.SetParent(canvasGo.transform, false);
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.55f, 0.9f, 0.9f);
        Button resetBtn = btnGo.AddComponent<Button>();

        GameObject btnTxtGo = new GameObject("Text");
        btnTxtGo.transform.SetParent(btnGo.transform, false);
        Text btnTxt = btnTxtGo.AddComponent<Text>();
        btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnTxt.fontSize = 24;
        btnTxt.alignment = TextAnchor.MiddleCenter;
        btnTxt.color = Color.white;
        btnTxt.text = "RESET AR PLACEMENT";

        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.25f, 0.04f);
        btnRt.anchorMax = new Vector2(0.75f, 0.12f);
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;

        RectTransform btnTxtRt = btnTxtGo.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero;
        btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero;
        btnTxtRt.offsetMax = Vector2.zero;

        // Diagnostic Panel (Top Left)
        GameObject diagGo = new GameObject("DiagnosticPanel");
        diagGo.transform.SetParent(canvasGo.transform, false);
        Image diagImg = diagGo.AddComponent<Image>();
        diagImg.color = new Color(0f, 0f, 0f, 0.65f);

        RectTransform diagRt = diagGo.GetComponent<RectTransform>();
        diagRt.anchorMin = new Vector2(0.02f, 0.70f);
        diagRt.anchorMax = new Vector2(0.48f, 0.98f);
        diagRt.offsetMin = Vector2.zero;
        diagRt.offsetMax = Vector2.zero;

        // Diag Text
        GameObject diagTxtGo = new GameObject("DiagText");
        diagTxtGo.transform.SetParent(diagGo.transform, false);
        Text diagTxt = diagTxtGo.AddComponent<Text>();
        diagTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        diagTxt.fontSize = 16;
        diagTxt.alignment = TextAnchor.UpperLeft;
        diagTxt.color = Color.green;
        diagTxt.text = "AR Diagnostics Loading...";

        RectTransform diagTxtRt = diagTxtGo.GetComponent<RectTransform>();
        diagTxtRt.anchorMin = new Vector2(0.05f, 0.05f);
        diagTxtRt.anchorMax = new Vector2(0.95f, 0.95f);
        diagTxtRt.offsetMin = Vector2.zero;
        diagTxtRt.offsetMax = Vector2.zero;

        // Connect UI references
        so.FindProperty("statusText").objectReferenceValue = statusTxt;
        so.FindProperty("resetButton").objectReferenceValue = resetBtn;
        so.FindProperty("diagnosticPanel").objectReferenceValue = diagGo;
        so.FindProperty("diagPermissionText").objectReferenceValue = diagTxt;
        so.FindProperty("diagSessionText").objectReferenceValue = diagTxt;
        so.FindProperty("diagFrameText").objectReferenceValue = diagTxt;
        so.FindProperty("diagPlaneText").objectReferenceValue = diagTxt;
        so.FindProperty("diagStatusText").objectReferenceValue = diagTxt;

        so.ApplyModifiedProperties();

        // 8. Save scene
        EditorSceneManager.SaveScene(newScene, TargetScenePath);
        Debug.Log($"[TestScene] Isolated scene successfully created and saved at: {TargetScenePath}");
    }
}
