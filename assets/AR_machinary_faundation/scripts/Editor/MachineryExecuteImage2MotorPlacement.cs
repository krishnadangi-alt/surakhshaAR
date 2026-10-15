using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public static class MachineryExecuteImage2MotorPlacement
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
    private const string OutputFolder = "Builds/Verification/Machinery";

    [MenuItem("Machinery/Execute Image 2 Motor Placement")]
    public static void ExecuteMotorPlacement()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene: {ScenePath}");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("FINAL IMAGE 2 DRIVE MOTOR PLACEMENT EXECUTION");
        sb.AppendLine("==========================================================");

        // 1. RECORD ALL SCENE TRANSFORMS BEFORE MODIFICATION
        var allTransformsBefore = new Dictionary<string, TransformData>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            RecordTransformTree(rootGo.transform, allTransformsBefore);
        }
        sb.AppendLine($"Recorded {allTransformsBefore.Count} transform states before modification.");

        // 2. FIND DRIVEMOTOR
        GameObject driveMotor = GameObject.Find("DriveMotor");
        if (driveMotor == null)
        {
            sb.AppendLine("ERROR: DriveMotor not found in scene!");
            File.WriteAllText("final_image2_motor_report.txt", sb.ToString());
            return;
        }

        sb.AppendLine("\n--- BEFORE MOTOR STATE ---");
        sb.AppendLine($"  DriveMotor World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, LocalScale: {driveMotor.transform.localScale.ToString("F4")}");

        // 3. APPLY EXACT IMAGE 2 HORIZONTAL & ELEVATED DRIVE END PLACEMENT
        // DriveMotor Root: World Pos (2.5200, 1.0096, 0.0104), Rot Euler(0, 0, 90), Scale (0.4200, 0.4200, 0.4200)
        // Shaft centerline Y = 1.0096m, Z = 0.0104m matching conveyor drive pulley.
        Vector3 targetPos = new Vector3(2.5200f, 1.0096f, 0.0104f);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, 90f);
        Vector3 targetScale = new Vector3(0.4200f, 0.4200f, 0.4200f);

        driveMotor.transform.position = targetPos;
        driveMotor.transform.rotation = targetRot;
        driveMotor.transform.localScale = targetScale;

        // Configure child MotorBasePlate under DriveMotor to form grounded support stand under elevated motor
        Transform motorBase = driveMotor.transform.Find("MotorBasePlate");
        if (motorBase != null)
        {
            motorBase.position = new Vector3(2.5200f, 0.3741f, 0.0104f);
            motorBase.rotation = Quaternion.Euler(0f, 0f, 0f);
            motorBase.localScale = new Vector3(0.7500f / 0.4200f, 0.7481f / 0.4200f, 0.5500f / 0.4200f);
        }

        // Configure child Coupling under DriveMotor connecting motor shaft to conveyor drive shaft
        Transform coupling = driveMotor.transform.Find("Coupling");
        if (coupling != null)
        {
            coupling.position = new Vector3(2.2750f, 1.0096f, 0.0104f);
            coupling.rotation = Quaternion.Euler(0f, 0f, 90f);
            coupling.localScale = new Vector3(0.1200f / 0.4200f, 0.2500f / 0.4200f, 0.1200f / 0.4200f);
        }

        EditorUtility.SetDirty(driveMotor);

        sb.AppendLine("\n--- AFTER MOTOR STATE (IMAGE 2 MATCH) ---");
        sb.AppendLine($"  DriveMotor World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, LocalScale: {driveMotor.transform.localScale.ToString("F4")}");
        if (motorBase != null)
        {
            sb.AppendLine($"  MotorBasePlate World Pos: {motorBase.position.ToString("F4")}, Rot: {motorBase.eulerAngles.ToString("F4")}, LossyScale: {motorBase.lossyScale.ToString("F4")}");
        }
        if (coupling != null)
        {
            sb.AppendLine($"  Coupling World Pos: {coupling.position.ToString("F4")}, Rot: {coupling.eulerAngles.ToString("F4")}, LossyScale: {coupling.lossyScale.ToString("F4")}");
        }

        // 4. PERFORM STRICT AUDIT OF ALL OTHER SCENE TRANSFORMS
        sb.AppendLine("\n--- STRICT AUDIT OF ALL OTHER SCENE TRANSFORMS ---");
        List<string> unintendedChanges = new List<string>();
        
        var allTransformsAfter = new Dictionary<string, TransformData>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            RecordTransformTree(rootGo.transform, allTransformsAfter);
        }

        foreach (var kvp in allTransformsBefore)
        {
            string path = kvp.Key;
            // Ignore DriveMotor and its internal sub-children
            if (path == "MachineryScenario/DriveMotor" || path.StartsWith("MachineryScenario/DriveMotor/"))
            {
                continue;
            }

            if (!allTransformsAfter.ContainsKey(path))
            {
                unintendedChanges.Add($"DELETED: {path}");
            }
            else
            {
                var before = kvp.Value;
                var after = allTransformsAfter[path];
                if (!before.Matches(after))
                {
                    unintendedChanges.Add($"CHANGED: {path} | Pos: {before.position.ToString("F4")} -> {after.position.ToString("F4")}, Rot: {before.rotation.ToString("F4")} -> {after.rotation.ToString("F4")}, Scale: {before.scale.ToString("F4")} -> {after.scale.ToString("F4")}");
                }
            }
        }

        if (unintendedChanges.Count == 0)
        {
            sb.AppendLine("Strict Audit Result: PASSED - ONLY DriveMotor changed!");
        }
        else
        {
            sb.AppendLine($"Strict Audit Result: FAILED - {unintendedChanges.Count} unintended changes detected!");
            foreach (var change in unintendedChanges)
            {
                sb.AppendLine($"  [UNINTENDED] {change}");
            }
        }

        // 5. SAVE SCENE
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("Scene saved successfully.");

        // 6. RENDER 4 VERIFICATION PNGs
        RenderVerificationPNGs();

        File.WriteAllText("final_image2_motor_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void RenderVerificationPNGs()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo == null) camGo = GameObject.Find("Main Camera");
        Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;

        GameObject tempCamGo = new GameObject("TempImage2VerificationCam");
        Camera tempCam = tempCamGo.AddComponent<Camera>();
        if (mainCam != null) tempCam.CopyFrom(mainCam);

        tempCam.clearFlags = CameraClearFlags.SolidColor;
        tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        tempCam.nearClipPlane = 0.05f;
        tempCam.farClipPlane = 100f;

        int width = 1280;
        int height = 720;
        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        void Capture(Vector3 eyePos, Vector3 lookAt, string filename)
        {
            tempCam.transform.position = eyePos;
            tempCam.transform.LookAt(lookAt);
            tempCam.targetTexture = rt;
            tempCam.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            string savePath = Path.Combine(OutputFolder, filename);
            File.WriteAllBytes(savePath, bytes);
            Debug.Log($"Rendered verification PNG: {savePath}");
        }

        // SHOT1_Full_Conveyor_And_Motor.png
        Capture(new Vector3(0.5f, 2.0f, -3.8f), new Vector3(0.5f, 0.8f, 0f), "SHOT1_Full_Conveyor_And_Motor.png");

        // SHOT2_Motor_DriveEnd_CloseUp.png
        Capture(new Vector3(2.3f, 1.4f, -1.2f), new Vector3(2.35f, 0.90f, 0.00f), "SHOT2_Motor_DriveEnd_CloseUp.png");

        // SHOT3_Side_Shaft_Axis_View.png
        Capture(new Vector3(2.35f, 1.01f, -2.2f), new Vector3(2.35f, 1.01f, 0.00f), "SHOT3_Side_Shaft_Axis_View.png");

        // SHOT4_Angled_Reference_Match.png
        Capture(new Vector3(3.4f, 1.4f, -2.0f), new Vector3(2.2f, 0.90f, 0f), "SHOT4_Angled_Reference_Match.png");

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(tempCamGo);
    }

    private static void RecordTransformTree(Transform t, Dictionary<string, TransformData> dict)
    {
        string path = GetFullPath(t.gameObject);
        dict[path] = new TransformData(t);
        for (int i = 0; i < t.childCount; i++)
        {
            RecordTransformTree(t.GetChild(i), dict);
        }
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    private struct TransformData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public TransformData(Transform t)
        {
            position = t.position;
            rotation = t.eulerAngles;
            scale = t.lossyScale;
        }

        public bool Matches(TransformData other)
        {
            return Vector3.Distance(position, other.position) < 0.0001f &&
                   Vector3.Distance(rotation, other.rotation) < 0.001f &&
                   Vector3.Distance(scale, other.scale) < 0.0001f;
        }
    }
}
