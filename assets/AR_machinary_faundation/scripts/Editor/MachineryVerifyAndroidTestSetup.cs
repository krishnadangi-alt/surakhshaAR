using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Text;
using System.IO;

public static class MachineryVerifyAndroidTestSetup
{
    private const string TestScenePath = "Assets/AR_machinary_faundation/scenes/MachineryAndroidTest.unity";
    private const string SourceScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

    [MenuItem("Machinery/Verify Isolated Android Test Setup")]
    public static void Verify()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ISOLATED TEST SCENE VERIFICATION ===");

        // 1. Verify MachineryConveyorJam.unity is untouched
        var sourceSceneInfo = new FileInfo(SourceScenePath);
        sb.AppendLine($"Source Scene: {SourceScenePath} (Size: {sourceSceneInfo.Length} bytes, LastModified: {sourceSceneInfo.LastWriteTime})");

        // 2. Open MachineryAndroidTest.unity
        var scene = EditorSceneManager.OpenScene(TestScenePath, OpenSceneMode.Single);
        sb.AppendLine($"Opened Test Scene: {scene.name} (Valid: {scene.IsValid()})");

        var testCtrl = Object.FindFirstObjectByType<SurakshaAR.Machinery.MachineryIsolatedTestController>();
        if (testCtrl != null)
        {
            var so = new SerializedObject(testCtrl);
            var rootProp = so.FindProperty("machineryPlacementRoot");
            var rootObj = rootProp?.objectReferenceValue as GameObject;
            sb.AppendLine($"MachineryIsolatedTestController: FOUND on {testCtrl.gameObject.name}");
            sb.AppendLine($"  - machineryPlacementRoot: {(rootObj != null ? rootObj.name + " (activeSelf=" + rootObj.activeSelf + ")" : "NULL!")}");

            if (rootObj != null)
            {
                sb.AppendLine($"  - Children of placementRoot: {rootObj.transform.childCount}");
                for (int i = 0; i < rootObj.transform.childCount; i++)
                {
                    var child = rootObj.transform.GetChild(i);
                    sb.AppendLine($"      - {child.name} (Active: {child.gameObject.activeSelf}, ChildCount: {child.childCount})");
                }
            }
        }
        else
        {
            sb.AppendLine("MachineryIsolatedTestController: MISSING!");
        }

        var cam = Camera.main;
        if (cam != null)
        {
            sb.AppendLine($"Main Camera: FOUND ({cam.name})");
            sb.AppendLine($"  - Clear Flags: {cam.clearFlags}");
            sb.AppendLine($"  - Background: {cam.backgroundColor}");
            sb.AppendLine($"  - ARCameraManager: {(cam.GetComponent<ARCameraManager>() != null ? "YES" : "NO")}");
            sb.AppendLine($"  - ARCameraBackground: {(cam.GetComponent<ARCameraBackground>() != null ? "YES" : "NO")}");
            sb.AppendLine($"  - TrackedPoseDriver: {(cam.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>() != null ? "YES" : "NO")}");
        }
        else
        {
            sb.AppendLine("Main Camera: MISSING!");
        }

        File.WriteAllText("test_scene_verification_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }
}
