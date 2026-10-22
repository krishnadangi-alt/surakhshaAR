using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Text;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Connects MachineryARPlacementController to MachineryScenario target,
/// ensures AR camera background and ARAnchorManager are active,
/// and verifies ZERO machinery transform changes.
/// </summary>
public static class MachineryRuntimeCameraAndPlacementFix
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

    [MenuItem("Machinery/Apply Camera and Placement Fix")]
    public static void Apply()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== MACHINERY AR CAMERA & PLACEMENT FIX ===");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Could not open MachineryConveyorJam.unity");
            return;
        }

        // 1. Record all transforms before
        var beforeSnaps = new Dictionary<string, TransformSnap>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            SnapTree(rootGo.transform, beforeSnaps);
        }
        sb.AppendLine($"Snapped {beforeSnaps.Count} scene transforms before modification.");

        // 2. Assign machineryScenarioTarget on MachineryARPlacementController
        GameObject scenario = GameObject.Find("MachineryScenario");
        var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();

        if (xrOrigin != null)
        {
            var placementCtrl = xrOrigin.GetComponent<SurakshaAR.Machinery.MachineryARPlacementController>();
            if (placementCtrl != null && scenario != null)
            {
                var so = new SerializedObject(placementCtrl);
                var targetProp = so.FindProperty("machineryScenarioTarget");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = scenario;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(placementCtrl);
                    sb.AppendLine("  -> Assigned MachineryScenario to MachineryARPlacementController.machineryScenarioTarget!");
                }
            }

            // Ensure ARAnchorManager exists on XR Origin
            var anchorMgr = xrOrigin.GetComponent<ARAnchorManager>();
            if (anchorMgr == null)
            {
                anchorMgr = xrOrigin.gameObject.AddComponent<ARAnchorManager>();
                EditorUtility.SetDirty(xrOrigin.gameObject);
                sb.AppendLine("  -> Added ARAnchorManager component to XR Origin.");
            }
            anchorMgr.enabled = true;

            // Ensure Camera Offset & Main Camera are active
            Transform camOffset = xrOrigin.transform.Find("Camera Offset");
            if (camOffset != null)
            {
                camOffset.gameObject.SetActive(true);
                Transform mainCamT = camOffset.Find("Main Camera");
                if (mainCamT != null)
                {
                    mainCamT.gameObject.SetActive(true);
                    mainCamT.tag = "MainCamera";

                    Camera cam = mainCamT.GetComponent<Camera>();
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0f, 0f, 0f, 1f);
                        cam.nearClipPlane = 0.1f;
                        cam.farClipPlane = 50f;
                        EditorUtility.SetDirty(cam);
                    }

                    var camMgr = mainCamT.GetComponent<ARCameraManager>();
                    if (camMgr == null) camMgr = mainCamT.gameObject.AddComponent<ARCameraManager>();
                    camMgr.enabled = true;

                    var camBg = mainCamT.GetComponent<ARCameraBackground>();
                    if (camBg == null) camBg = mainCamT.gameObject.AddComponent<ARCameraBackground>();
                    camBg.enabled = true;

                    var poseDriver = mainCamT.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                    if (poseDriver == null) poseDriver = mainCamT.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                    poseDriver.enabled = true;
                    EditorUtility.SetDirty(mainCamT.gameObject);
                }
            }
        }

        // 3. Audit transform changes on machinery objects
        var afterSnaps = new Dictionary<string, TransformSnap>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            SnapTree(rootGo.transform, afterSnaps);
        }

        sb.AppendLine("\n--- STRICT MACHINERY TRANSFORM AUDIT ---");
        var violations = new List<string>();
        foreach (var kvp in beforeSnaps)
        {
            string path = kvp.Key;
            if (path.StartsWith("MachineryScenario/"))
            {
                if (!afterSnaps.TryGetValue(path, out var after))
                {
                    violations.Add($"DELETED: {path}");
                    continue;
                }
                if (!kvp.Value.Matches(after))
                {
                    violations.Add($"CHANGED: {path}");
                }
            }
        }

        if (violations.Count == 0)
        {
            sb.AppendLine("Audit PASSED: ZERO machinery transforms were modified.");
        }
        else
        {
            sb.AppendLine($"Audit FAILED: {violations.Count} unexpected transform changes:");
            foreach (var v in violations) sb.AppendLine($"  {v}");
        }

        // 4. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("Scene saved successfully.");

        File.WriteAllText("machinery_camera_fix_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void SnapTree(Transform t, Dictionary<string, TransformSnap> dict)
    {
        dict[FullPath(t.gameObject)] = new TransformSnap(t);
        for (int i = 0; i < t.childCount; i++) SnapTree(t.GetChild(i), dict);
    }

    private static string FullPath(GameObject go)
    {
        string p = go.name;
        var parent = go.transform.parent;
        while (parent != null) { p = parent.name + "/" + p; parent = parent.parent; }
        return p;
    }

    private struct TransformSnap
    {
        public Vector3 pos, rot, scale;
        public TransformSnap(Transform t) { pos = t.position; rot = t.eulerAngles; scale = t.lossyScale; }
        public bool Matches(TransformSnap o) =>
            Vector3.Distance(pos, o.pos) < 0.0001f &&
            Vector3.Distance(rot, o.rot) < 0.001f &&
            Vector3.Distance(scale, o.scale) < 0.0001f;
    }
}
