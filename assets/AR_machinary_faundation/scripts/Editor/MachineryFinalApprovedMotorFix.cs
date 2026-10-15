using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// MOTOR-ONLY final fix to match Image 2 approved state.
/// Touches ONLY DriveMotor (and its two existing children: MotorBasePlate, Coupling).
/// Records full transform audit to prove NO other object changes.
/// </summary>
public static class MachineryFinalApprovedMotorFix
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
    private const string OutputFolder = "Builds/Verification/Machinery";

    [MenuItem("Machinery/Execute Final Approved Motor Fix (Image 2)")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Could not open scene.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("FINAL APPROVED MOTOR FIX — IMAGE 2 STATE");
        sb.AppendLine("==========================================================");

        // ── 1. RECORD ALL TRANSFORMS BEFORE ANY CHANGE ───────────────────────
        var beforeStates = new Dictionary<string, TransformSnap>();
        foreach (var rootGo in scene.GetRootGameObjects())
            SnapTree(rootGo.transform, beforeStates);
        sb.AppendLine($"Snapped {beforeStates.Count} transforms before modification.");

        // ── 2. LOCATE DRIVEMOTOR ──────────────────────────────────────────────
        GameObject motor = GameObject.Find("DriveMotor");
        if (motor == null)
        {
            sb.AppendLine("ERROR: DriveMotor not found!");
            File.WriteAllText("final_approved_motor_report.txt", sb.ToString());
            return;
        }

        sb.AppendLine($"\n[BEFORE] DriveMotor Pos: {motor.transform.position.ToString("F4")}  " +
                      $"Rot: {motor.transform.eulerAngles.ToString("F4")}  " +
                      $"Scale: {motor.transform.localScale.ToString("F4")}");

        // ── 3. INSPECT CONVEYOR DRIVE END VIA EXISTING DriveShaftCoupling ────
        // DriveShaftCoupling is a child of MotorMountExtensionFrame inside ConveyorAssembly.
        // Its world position (2.3478, 0.9900, 0.0100) defines the exact shaft axis.
        GameObject driveShaftCoupling = GameObject.Find("DriveShaftCoupling");
        float shaftY = 0.9900f;
        float shaftZ = 0.0100f;

        if (driveShaftCoupling != null)
        {
            shaftY = driveShaftCoupling.transform.position.y;
            shaftZ = driveShaftCoupling.transform.position.z;
            sb.AppendLine($"\nDriveShaftCoupling world pos: {driveShaftCoupling.transform.position.ToString("F4")}");
        }
        else
        {
            sb.AppendLine("\nDriveShaftCoupling not found — using hardcoded shaft Y=0.990, Z=0.010");
        }

        // ── 4. APPLY IMAGE-2 APPROVED MOTOR TRANSFORM ────────────────────────
        // Root placed so motor shaft centerline coincides with DriveShaftCoupling axis.
        // Euler(0, 0, 90) → motor body lies horizontal along X-axis,
        // shaft at -X direction faces towards conveyor head.
        motor.transform.position   = new Vector3(2.5200f, shaftY, shaftZ);
        motor.transform.rotation   = Quaternion.Euler(0f, 0f, 90f);
        motor.transform.localScale = new Vector3(0.4200f, 0.4200f, 0.4200f);

        // ── 5. ADJUST CHILD MotorBasePlate (grounded stand under motor) ──────
        Transform motorBase = motor.transform.Find("MotorBasePlate");
        if (motorBase != null)
        {
            // Stand spans from floor (Y=0) up to just below motor feet (Y≈shaftY–0.6355)
            float standHeight = shaftY - 0.1385f;   // motor feet MinY offset from root when Euler(0,0,90)
            float standCenterY = standHeight / 2f;
            motorBase.position   = new Vector3(2.5200f, standCenterY, shaftZ);
            motorBase.rotation   = Quaternion.Euler(0f, 0f, 0f);
            motorBase.localScale = new Vector3(0.7500f / 0.4200f, standHeight / 0.4200f, 0.5500f / 0.4200f);
            sb.AppendLine($"MotorBasePlate → Pos:{motorBase.position.ToString("F4")} " +
                          $"Scale:{motorBase.lossyScale.ToString("F4")}");
        }

        // ── 6. ADJUST CHILD Coupling (on shaft axis between motor and conveyor) ─
        Transform coupling = motor.transform.Find("Coupling");
        if (coupling != null)
        {
            coupling.position   = new Vector3(2.2750f, shaftY, shaftZ);
            coupling.rotation   = Quaternion.Euler(0f, 0f, 90f);
            coupling.localScale = new Vector3(0.1200f / 0.4200f, 0.2500f / 0.4200f, 0.1200f / 0.4200f);
            sb.AppendLine($"Coupling        → Pos:{coupling.position.ToString("F4")} " +
                          $"Scale:{coupling.lossyScale.ToString("F4")}");
        }

        EditorUtility.SetDirty(motor);

        sb.AppendLine($"\n[AFTER]  DriveMotor Pos: {motor.transform.position.ToString("F4")}  " +
                      $"Rot: {motor.transform.eulerAngles.ToString("F4")}  " +
                      $"Scale: {motor.transform.localScale.ToString("F4")}");

        // ── 7. STRICT TRANSFORM AUDIT ────────────────────────────────────────
        var afterStates = new Dictionary<string, TransformSnap>();
        foreach (var rootGo in scene.GetRootGameObjects())
            SnapTree(rootGo.transform, afterStates);

        sb.AppendLine("\n--- STRICT TRANSFORM AUDIT ---");
        var violations = new List<string>();
        foreach (var kvp in beforeStates)
        {
            string path = kvp.Key;
            // Allowed: DriveMotor root + its two existing children
            if (path == "MachineryScenario/DriveMotor" ||
                path.StartsWith("MachineryScenario/DriveMotor/"))
                continue;

            if (!afterStates.TryGetValue(path, out var after)) continue;
            if (!kvp.Value.Matches(after))
                violations.Add($"CHANGED: {path}");
        }

        if (violations.Count == 0)
            sb.AppendLine("Audit: PASSED — ONLY DriveMotor branch changed.");
        else
        {
            sb.AppendLine($"Audit: FAILED — {violations.Count} unexpected changes:");
            foreach (var v in violations) sb.AppendLine($"  {v}");
        }

        // ── 8. SAVE ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("Scene saved.");

        // ── 9. RENDER 4 VERIFICATION PNGs ────────────────────────────────────
        RenderVerification(shaftY, shaftZ);

        File.WriteAllText("final_approved_motor_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private static void RenderVerification(float shaftY, float shaftZ)
    {
        if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);

        GameObject cgo = GameObject.Find("Main Camera");
        Camera refCam = cgo != null ? cgo.GetComponent<Camera>() : Camera.main;

        GameObject tcgo = new GameObject("__VerCam");
        Camera cam = tcgo.AddComponent<Camera>();
        if (refCam != null) cam.CopyFrom(refCam);
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.12f, 0.14f, 0.18f, 1f);
        cam.nearClipPlane    = 0.05f;
        cam.farClipPlane     = 100f;

        const int W = 1280, H = 720;
        var rt  = new RenderTexture(W, H, 24);
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);

        void Snap(Vector3 eye, Vector3 target, string name)
        {
            cam.transform.position = eye;
            cam.transform.LookAt(target);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(OutputFolder, name), tex.EncodeToPNG());
        }

        Snap(new Vector3(0.5f,  2.0f, -3.8f), new Vector3(0.5f,  0.8f,  0f),      "SHOT1_Full_Conveyor_And_Motor.png");
        Snap(new Vector3(2.3f,  1.4f, -1.2f), new Vector3(2.35f, shaftY, shaftZ),  "SHOT2_Motor_DriveEnd_CloseUp.png");
        Snap(new Vector3(2.35f, shaftY, -2.2f),new Vector3(2.35f, shaftY, 0f),      "SHOT3_Side_Shaft_Axis_View.png");
        Snap(new Vector3(3.4f,  1.4f, -2.0f), new Vector3(2.2f,  shaftY, 0f),      "SHOT4_Angled_Reference_Match.png");

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(tcgo);
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
            Vector3.Distance(pos,   o.pos)   < 0.0001f &&
            Vector3.Distance(rot,   o.rot)   < 0.001f  &&
            Vector3.Distance(scale, o.scale) < 0.0001f;
    }
}
