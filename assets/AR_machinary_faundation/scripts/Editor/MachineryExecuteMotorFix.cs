using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public static class MachineryExecuteMotorFix
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
    private const string OutputFolder = "Builds/Verification/Machinery";

    [MenuItem("Machinery/Execute Exact Motor Alignment")]
    public static void RunMotorFix()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene: {ScenePath}");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("PRECISION MOTOR REPAIR & ALIGNMENT EXECUTION");
        sb.AppendLine("==========================================================");

        // 1. RECORD LOCKED OBJECT TRANSFORMS BEFORE MODIFICATION
        string[] lockedObjectNames = new string[]
        {
            "FeedHopper",
            "ConveyorAssembly",
            "ConveyorFrame",
            "ConveyorBelt",
            "BeltRollers",
            "JamArea",
            "CoalGroup",
            "ControlPanel",
            "EmergencyStop",
            "MainIsolator",
            "SteelWorkstationTable",
            "Hasp",
            "PadlockAndTag",
            "Environment",
            "Floor",
            "XR Origin",
            "AR Session",
            "Main Camera"
        };

        Dictionary<string, TransformState> initialLockedStates = new Dictionary<string, TransformState>();
        foreach (var name in lockedObjectNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                initialLockedStates[name] = new TransformState(go.transform);
                sb.AppendLine($"[LOCKED RECORD] '{name}' -> Pos: {go.transform.position.ToString("F4")}, Rot: {go.transform.eulerAngles.ToString("F4")}, Scale: {go.transform.lossyScale.ToString("F4")}");
            }
        }

        // 2. FIND DRIVE MOTOR AND COMPONENTS
        GameObject driveMotor = GameObject.Find("DriveMotor");
        if (driveMotor == null)
        {
            Debug.LogError("DriveMotor not found in scene!");
            return;
        }

        Transform motorBase = driveMotor.transform.Find("MotorBasePlate");
        Transform coupling = driveMotor.transform.Find("Coupling");

        if (motorBase == null || coupling == null)
        {
            Debug.LogError($"Child component missing! MotorBasePlate: {motorBase != null}, Coupling: {coupling != null}");
            return;
        }

        sb.AppendLine("\n--- BEFORE MOTOR ALIGNMENT ---");
        sb.AppendLine($"DriveMotor World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, Scale: {driveMotor.transform.lossyScale.ToString("F4")}");
        sb.AppendLine($"MotorBasePlate World Pos: {motorBase.position.ToString("F4")}, Rot: {motorBase.eulerAngles.ToString("F4")}, Scale: {motorBase.lossyScale.ToString("F4")}");
        sb.AppendLine($"Coupling World Pos: {coupling.position.ToString("F4")}, Rot: {coupling.eulerAngles.ToString("F4")}, Scale: {coupling.lossyScale.ToString("F4")}");

        // 3. GEOMETRY-BASED ALIGNMENT CALCULATION
        // Conveyor Drive shaft axis center: Y = 1.0096, Z = 0.0104. Head Pulley end X = 2.1518.
        // Motor shaft axis must be collinear with conveyor drive shaft axis.
        // Motor position: X = 2.5000, Y = 1.0096, Z = 0.0104.
        // Rotation: (0, 90, 0) so motor shaft points in -X direction toward conveyor drive end.
        
        Vector3 targetMotorPos = new Vector3(2.5000f, 1.0096f, 0.0104f);
        Quaternion targetMotorRot = Quaternion.Euler(0f, 90f, 0f);
        Vector3 targetMotorScale = new Vector3(0.4200f, 0.4200f, 0.4200f);

        driveMotor.transform.position = targetMotorPos;
        driveMotor.transform.rotation = targetMotorRot;
        driveMotor.transform.localScale = targetMotorScale;

        // MotorBasePlate local configuration underneath motor body
        // Local Y of -0.9800 relative to Motor Root (Y=1.0096, scale=0.42) puts Base World Y at 0.5980m (right under motor feet).
        motorBase.transform.localPosition = new Vector3(0.0000f, -0.9800f, 0.0000f);
        motorBase.transform.localRotation = Quaternion.Euler(0.0000f, 270.0000f, 0.0000f);
        motorBase.transform.localScale = new Vector3(0.8500f, 0.1500f, 0.7500f);

        // Coupling local configuration bridging motor shaft and conveyor shaft
        // Local Z of -0.6800 relative to Motor Root (Y=1.0096, scale=0.42) puts Coupling World X at 2.2144m (between conveyor X=2.1518 and motor shaft X=2.40).
        coupling.transform.localPosition = new Vector3(0.0000f, 0.0000f, -0.6800f);
        coupling.transform.localRotation = Quaternion.Euler(0.0000f, 270.0000f, 90.0000f);
        coupling.transform.localScale = new Vector3(0.1800f, 0.3200f, 0.1800f);

        EditorUtility.SetDirty(driveMotor);
        EditorUtility.SetDirty(motorBase.gameObject);
        EditorUtility.SetDirty(coupling.gameObject);

        sb.AppendLine("\n--- AFTER MOTOR ALIGNMENT ---");
        sb.AppendLine($"DriveMotor World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, Scale: {driveMotor.transform.lossyScale.ToString("F4")}");
        sb.AppendLine($"MotorBasePlate World Pos: {motorBase.position.ToString("F4")}, Rot: {motorBase.eulerAngles.ToString("F4")}, Scale: {motorBase.lossyScale.ToString("F4")}");
        sb.AppendLine($"Coupling World Pos: {coupling.position.ToString("F4")}, Rot: {coupling.eulerAngles.ToString("F4")}, Scale: {coupling.lossyScale.ToString("F4")}");

        // 4. VERIFY ALL LOCKED OBJECT TRANSFORMS REMAIN UNCHANGED
        sb.AppendLine("\n--- VERIFYING LOCKED OBJECT TRANSFORMS ---");
        bool allLockedIntact = true;
        foreach (var kvp in initialLockedStates)
        {
            GameObject go = GameObject.Find(kvp.Key);
            if (go != null)
            {
                TransformState current = new TransformState(go.transform);
                if (!kvp.Value.Matches(current))
                {
                    allLockedIntact = false;
                    sb.AppendLine($"[CRITICAL ERROR] Locked object '{kvp.Key}' transform CHANGED!");
                    sb.AppendLine($"  Before: Pos={kvp.Value.position}, Rot={kvp.Value.rotation}, Scale={kvp.Value.scale}");
                    sb.AppendLine($"  After:  Pos={current.position}, Rot={current.rotation}, Scale={current.scale}");
                }
                else
                {
                    sb.AppendLine($"[VERIFIED UNCHANGED] '{kvp.Key}'");
                }
            }
        }
        sb.AppendLine($"All Locked Objects Intact: {allLockedIntact}");

        // 5. SAVE SCENE
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("\nScene saved successfully.");

        // 6. RENDER VERIFICATION SCREENSHOTS
        RenderVerificationScreenshots();

        File.WriteAllText("exact_motor_repair_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void RenderVerificationScreenshots()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo == null) camGo = GameObject.Find("Main Camera");
        Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;

        GameObject tempCamGo = new GameObject("TempVerificationCam");
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
            Debug.Log($"Rendered screenshot: {savePath}");
        }

        // SHOT 1: Full scene showing conveyor + motor
        Capture(new Vector3(0.5f, 2.0f, -3.8f), new Vector3(0.5f, 0.8f, 0f), "SHOT1_Full_Conveyor_And_Motor.png");

        // SHOT 2: Close-up showing motor + base + coupling + conveyor drive
        Capture(new Vector3(2.3f, 1.4f, -1.2f), new Vector3(2.35f, 0.95f, 0.01f), "SHOT2_Motor_DriveEnd_CloseUp.png");

        // SHOT 3: Side/axis view proving shaft alignment
        Capture(new Vector3(2.35f, 1.01f, -2.0f), new Vector3(2.35f, 1.01f, 0.01f), "SHOT3_Side_Shaft_Axis_View.png");

        // SHOT 4: Angled view matching IMAGE 1 reference
        Capture(new Vector3(3.2f, 1.8f, -2.2f), new Vector3(2.1f, 0.8f, 0f), "SHOT4_Angled_Reference_Match.png");

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(tempCamGo);
    }

    private struct TransformState
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public TransformState(Transform t)
        {
            position = t.position;
            rotation = t.eulerAngles;
            scale = t.lossyScale;
        }

        public bool Matches(TransformState other)
        {
            return Vector3.Distance(position, other.position) < 0.001f &&
                   Vector3.Distance(rotation, other.rotation) < 0.01f &&
                   Vector3.Distance(scale, other.scale) < 0.001f;
        }
    }
}
