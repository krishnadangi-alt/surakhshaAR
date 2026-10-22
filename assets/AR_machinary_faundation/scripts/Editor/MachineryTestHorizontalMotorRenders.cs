using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public static class MachineryTestHorizontalMotorRenders
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
    private const string OutputFolder = "Builds/Verification/Machinery/MOTOR_FIX_V2";

    [MenuItem("Machinery/Execute Horizontal Motor Fix V2")]
    public static void ExecuteFixV2()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene: {ScenePath}");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("HORIZONTAL MOTOR REPAIR & GROUNDED BASE V2 EXECUTION");
        sb.AppendLine("==========================================================");

        // 1. RECORD LOCKED OBJECT TRANSFORMS
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
            }
        }

        // 2. FIND MOTOR OBJECTS
        GameObject driveMotor = GameObject.Find("DriveMotor");
        if (driveMotor == null)
        {
            Debug.LogError("DriveMotor not found!");
            return;
        }

        Transform motorBase = driveMotor.transform.Find("MotorBasePlate");
        Transform coupling = driveMotor.transform.Find("Coupling");

        sb.AppendLine($"[OLD MOTOR STATE]");
        sb.AppendLine($"  DriveMotor World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, Scale: {driveMotor.transform.lossyScale.ToString("F4")}");
        sb.AppendLine($"  MotorBasePlate World Pos: {motorBase.position.ToString("F4")}, Rot: {motorBase.eulerAngles.ToString("F4")}, Scale: {motorBase.lossyScale.ToString("F4")}");
        sb.AppendLine($"  Coupling World Pos: {coupling.position.ToString("F4")}, Rot: {coupling.eulerAngles.ToString("F4")}, Scale: {coupling.lossyScale.ToString("F4")}");

        // 3. APPLY HORIZONTAL MOTOR ROTATION & ALIGNMENT
        // DriveMotor Root Position: X = 2.5200, Y = 1.0096, Z = 0.0104 (Matching Shaft Y=1.0096, Z=0.0104)
        // Root Rotation: Euler(0, 0, 90) -> Motor body lies HORIZONTALLY along X axis (length 0.84m).
        // Shaft extends towards -X direction (Conveyor Drive End at X = 2.1518m).
        
        Vector3 targetMotorPos = new Vector3(2.5200f, 1.0096f, 0.0104f);
        Quaternion targetMotorRot = Quaternion.Euler(0f, 0f, 90f);
        Vector3 targetMotorScale = new Vector3(0.4200f, 0.4200f, 0.4200f);

        driveMotor.transform.position = targetMotorPos;
        driveMotor.transform.rotation = targetMotorRot;
        driveMotor.transform.localScale = targetMotorScale;

        // Grounded MotorBasePlate:
        // Ground Y = 0.0000m. Motor bottom Y = 0.7481m.
        // Base plate center Y = 0.3741m, Y height = 0.7481m.
        // World position: (2.5200, 0.3741, 0.0104). World scale: (0.7500, 0.7481, 0.5500).
        // Convert to local relative to driveMotor (pos=2.5200, 1.0096, 0.0104, rot=(0,0,90), scale=0.42):
        
        motorBase.transform.position = new Vector3(2.5200f, 0.3741f, 0.0104f);
        motorBase.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        // Set lossy scale by adjusting local scale
        motorBase.transform.localScale = new Vector3(0.7500f / 0.4200f, 0.7481f / 0.4200f, 0.5500f / 0.4200f);

        // Coupling:
        // Positioned on shaft axis (Y=1.0096, Z=0.0104) at X = 2.2750m (between conveyor drive X=2.1518 and motor shaft front X=2.40).
        coupling.transform.position = new Vector3(2.2750f, 1.0096f, 0.0104f);
        coupling.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        coupling.transform.localScale = new Vector3(0.1200f / 0.4200f, 0.2500f / 0.4200f, 0.1200f / 0.4200f);

        EditorUtility.SetDirty(driveMotor);
        EditorUtility.SetDirty(motorBase.gameObject);
        EditorUtility.SetDirty(coupling.gameObject);

        sb.AppendLine($"\n[NEW MOTOR STATE]");
        sb.AppendLine($"  DriveMotor World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, Scale: {driveMotor.transform.lossyScale.ToString("F4")}");
        sb.AppendLine($"  MotorBasePlate World Pos: {motorBase.position.ToString("F4")}, Rot: {motorBase.eulerAngles.ToString("F4")}, Scale: {motorBase.lossyScale.ToString("F4")}");
        sb.AppendLine($"  Coupling World Pos: {coupling.position.ToString("F4")}, Rot: {coupling.eulerAngles.ToString("F4")}, Scale: {coupling.lossyScale.ToString("F4")}");

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
                }
            }
        }
        sb.AppendLine($"All Locked Objects Intact: {allLockedIntact}");

        // 5. SAVE SCENE
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("Scene saved successfully.");

        // 6. RENDER MOTOR_FIX_V2 PNG VERIFICATION FILES
        RenderV2Screenshots();

        File.WriteAllText("motor_fix_v2_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void RenderV2Screenshots()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo == null) camGo = GameObject.Find("Main Camera");
        Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;

        GameObject tempCamGo = new GameObject("TempVerificationCamV2");
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
            Debug.Log($"Rendered screenshot V2: {savePath}");
        }

        // 01_Motor_Overall_V2.png
        Capture(new Vector3(0.5f, 2.0f, -3.8f), new Vector3(0.5f, 0.8f, 0f), "01_Motor_Overall_V2.png");

        // 02_Motor_CloseUp_V2.png
        Capture(new Vector3(2.3f, 1.4f, -1.2f), new Vector3(2.35f, 0.85f, 0.01f), "02_Motor_CloseUp_V2.png");

        // 03_Motor_Side_Axis_V2.png
        Capture(new Vector3(2.35f, 1.01f, -2.2f), new Vector3(2.35f, 0.85f, 0.01f), "03_Motor_Side_Axis_V2.png");

        // 04_Motor_Angled_Reference_V2.png
        Capture(new Vector3(3.4f, 1.6f, -2.0f), new Vector3(2.2f, 0.75f, 0f), "04_Motor_Angled_Reference_V2.png");

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
