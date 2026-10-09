using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class ApplyFinalVisualFix
{
    [MenuItem("Tools/Apply Final Visual Fix")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // 1. BACKUP CURRENT SCENE FIRST (Timestamped backup as required by instructions)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_DavitLifelineFix.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"=== TIMESTAMPED BACKUP CREATED AT: {backupPath} ===");

        // Open main scene
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        GameObject yellowLifeline = GameObject.Find("Self_Retracting_Lifeline");
        if (yellowLifeline == null)
        {
            Debug.LogError("COULD NOT FIND 'Self_Retracting_Lifeline'!");
            return;
        }

        GameObject area = GameObject.Find("Confined_Space_Training_Area");

        // Load steel material for support structure
        Material steelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Pipeline_Steel.mat");
        if (steelMat == null) steelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Structural_Steel.mat");

        // Find or recreate Lifeline_Support_Davit_Arm container
        GameObject supportRoot = GameObject.Find("Lifeline_Support_Davit_Arm");
        if (supportRoot != null)
        {
            Object.DestroyImmediate(supportRoot);
        }

        supportRoot = new GameObject("Lifeline_Support_Davit_Arm");
        supportRoot.transform.SetParent(area.transform, true);

        // 2. BUILD PROPORTIONAL DAVIT SUPPORT ARM STRUCTURE MATCHING REFERENCE IMAGE media_1790581365082.png
        
        // Vertical Post: rising up from top-right corner of caged frame (X=2.450, Z=0.200)
        GameObject vPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vPost.name = "Vertical_Support_Post";
        vPost.transform.SetParent(supportRoot.transform, false);
        vPost.transform.position = new Vector3(2.4500f, 1.2000f, 0.2000f);
        vPost.transform.localScale = new Vector3(0.0380f, 0.5800f, 0.0380f);
        if (steelMat != null) vPost.GetComponent<Renderer>().sharedMaterial = steelMat;

        // Top Horizontal Support Arm: extending left over vessel dome center (X=2.2475)
        GameObject hArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hArm.name = "Horizontal_Support_Arm";
        hArm.transform.SetParent(supportRoot.transform, false);
        hArm.transform.position = new Vector3(2.3487f, 1.4800f, 0.2000f);
        hArm.transform.localScale = new Vector3(0.2405f, 0.0380f, 0.0380f);
        if (steelMat != null) hArm.GetComponent<Renderer>().sharedMaterial = steelMat;

        // Diagonal Gusset Brace: supporting the post-to-arm corner neatly behind yellow unit
        GameObject dBrace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dBrace.name = "Diagonal_Support_Brace";
        dBrace.transform.SetParent(supportRoot.transform, false);
        dBrace.transform.position = new Vector3(2.4050f, 1.4050f, 0.2000f);
        dBrace.transform.rotation = Quaternion.Euler(0, 0, 45);
        dBrace.transform.localScale = new Vector3(0.1200f, 0.0250f, 0.0250f);
        if (steelMat != null) dBrace.GetComponent<Renderer>().sharedMaterial = steelMat;

        // 3. SUSPEND EXISTING YELLOW LIFELINE FROM SUPPORT ARM OVERHANG
        // Yellow Lifeline positioned at X=2.2475, Y=1.2650, Z=0.2000
        // Top carabiner loop connects directly to underside of horizontal support arm
        yellowLifeline.transform.position = new Vector3(2.2475f, 1.2650f, 0.2000f);
        yellowLifeline.transform.rotation = Quaternion.identity;

        // 4. VERTICAL HANGING CONNECTOR LINE
        // Extends downward from bottom hook of yellow unit (Y ≈ 1.07) to vessel top anchor (Y ≈ 0.90)
        GameObject hangingLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hangingLine.name = "Lifeline_Hanging_Connector";
        hangingLine.transform.SetParent(supportRoot.transform, false);
        hangingLine.transform.position = new Vector3(2.2475f, 0.9850f, 0.2000f);
        hangingLine.transform.localScale = new Vector3(0.0100f, 0.0850f, 0.0100f); // Sleek vertical connector rod/chain
        if (steelMat != null) hangingLine.GetComponent<Renderer>().sharedMaterial = steelMat;

        // 5. SAVE MODIFIED SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"=== SAVED SCENE TO: {mainScenePath} ===");

        // 6. RENDER 3 MANDATORY VERIFICATION SCREENSHOTS
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // 1. FULL FRONT VIEW
        RenderView(cam, Path.Combine(artifactDir, "FINAL_FULL_FRONT_VIEW.png"), new Vector3(0.50f, 1.00f, -3.20f), Quaternion.Euler(0, 0, 0), 45f);

        // 2. CONFINED-SPACE CLOSE-UP VIEW (Matching reference media_1790581365082.png)
        RenderView(cam, Path.Combine(artifactDir, "FINAL_CONFINED_SPACE_CLOSEUP.png"), new Vector3(1.70f, 0.95f, -2.10f), Quaternion.Euler(0, 0, 0), 38f);

        // 3. YELLOW LIFELINE CLOSE-UP VIEW
        RenderView(cam, Path.Combine(artifactDir, "FINAL_YELLOW_LIFELINE_CLOSEUP.png"), new Vector3(2.2475f, 1.25f, -1.00f), Quaternion.Euler(0, 0, 0), 35f);
    }

    private static void RenderView(Camera cam, string outputPath, Vector3 pos, Quaternion rot, float fov)
    {
        Vector3 origPos = cam.transform.position;
        Quaternion origRot = cam.transform.rotation;
        float origFov = cam.fieldOfView;

        cam.transform.position = pos;
        cam.transform.rotation = rot;
        cam.fieldOfView = fov;

        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        File.WriteAllBytes(outputPath, screenShot.EncodeToPNG());
        Debug.Log($"Saved final verification screenshot: {outputPath}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
