using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TestDavitSupportSetup
{
    [MenuItem("Tools/Test Davit Support Setup")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject yellowLifeline = GameObject.Find("Self_Retracting_Lifeline");
        if (yellowLifeline == null)
        {
            Debug.LogError("COULD NOT FIND Self_Retracting_Lifeline!");
            return;
        }

        GameObject area = GameObject.Find("Confined_Space_Training_Area");

        // Material for support post/arm matching structural steel / pipeline steel
        Material steelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Pipeline_Steel.mat");
        if (steelMat == null) steelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Structural_Steel.mat");

        // Find or create SupportArmStructure container under area or scene
        GameObject supportRoot = GameObject.Find("Lifeline_Support_Davit_Arm");
        if (supportRoot != null)
        {
            Object.DestroyImmediate(supportRoot);
        }

        supportRoot = new GameObject("Lifeline_Support_Davit_Arm");
        supportRoot.transform.SetParent(area.transform, true);

        // Define anchor positions based on reference image media_1790581365082.png:
        // Caged frame top right corner is around X = 2.45, Y = 0.95, Z = 0.20
        // Vertical post extends from Y = 0.90 to Y = 1.48
        // Horizontal arm extends from X = 2.45 left to X = 2.2475
        // Diagonal brace connects (2.45, 1.25) to (2.32, 1.48)

        // 1. Vertical Post
        GameObject vPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vPost.name = "Vertical_Support_Post";
        vPost.transform.SetParent(supportRoot.transform, false);
        vPost.transform.position = new Vector3(2.4500f, 1.2000f, 0.2000f);
        vPost.transform.localScale = new Vector3(0.0400f, 0.5800f, 0.0400f);
        if (steelMat != null) vPost.GetComponent<Renderer>().sharedMaterial = steelMat;

        // 2. Top Horizontal Support Arm
        GameObject hArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hArm.name = "Horizontal_Support_Arm";
        hArm.transform.SetParent(supportRoot.transform, false);
        hArm.transform.position = new Vector3(2.3487f, 1.4800f, 0.2000f);
        hArm.transform.localScale = new Vector3(0.2425f, 0.0400f, 0.0400f);
        if (steelMat != null) hArm.GetComponent<Renderer>().sharedMaterial = steelMat;

        // 3. Diagonal Support Brace
        GameObject dBrace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dBrace.name = "Diagonal_Support_Brace";
        dBrace.transform.SetParent(supportRoot.transform, false);
        dBrace.transform.position = new Vector3(2.3800f, 1.3500f, 0.2000f);
        dBrace.transform.rotation = Quaternion.Euler(0, 0, 45);
        dBrace.transform.localScale = new Vector3(0.1800f, 0.0300f, 0.0300f);
        if (steelMat != null) dBrace.GetComponent<Renderer>().sharedMaterial = steelMat;

        // Position Yellow Lifeline suspended directly below horizontal arm overhang tip (X=2.2475, Y=1.2800)
        // Top carabiner loop at Y = 1.46 (hooks into bottom of horizontal arm at Y=1.46)
        yellowLifeline.transform.position = new Vector3(2.2475f, 1.2500f, 0.2000f);

        // 4. Create hanging connector chain/lanyard from bottom of yellow lifeline to top vessel anchor
        // Yellow lifeline bottom hook is around Y = 1.06
        // Vessel top anchor is around Y = 0.90
        GameObject chain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        chain.name = "Lifeline_Hanging_Chain";
        chain.transform.SetParent(supportRoot.transform, false);
        chain.transform.position = new Vector3(2.2475f, 0.9800f, 0.2000f);
        chain.transform.localScale = new Vector3(0.0120f, 0.0800f, 0.0120f); // 16cm vertical chain link
        if (steelMat != null) chain.GetComponent<Renderer>().sharedMaterial = steelMat;

        // Render preview screenshots
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // 1. FULL FRONT VIEW (Same framing as reference media_1790581365082.png)
        RenderCam(cam, Path.Combine(artifactDir, "DAVIT_TEST_FULL_FRONT.png"), new Vector3(0.50f, 1.00f, -3.20f), Quaternion.Euler(0, 0, 0), 45f);

        // 2. CONFINED SPACE CLOSE-UP VIEW (Matching reference media_1790581365082.png exactly!)
        RenderCam(cam, Path.Combine(artifactDir, "DAVIT_TEST_CONFINED_CLOSEUP.png"), new Vector3(1.70f, 0.95f, -2.10f), Quaternion.Euler(0, 0, 0), 38f);

        // 3. YELLOW LIFELINE CLOSE-UP VIEW
        RenderCam(cam, Path.Combine(artifactDir, "DAVIT_TEST_YELLOW_CLOSEUP.png"), new Vector3(2.2475f, 1.25f, -1.00f), Quaternion.Euler(0, 0, 0), 35f);
    }

    private static void RenderCam(Camera cam, string path, Vector3 pos, Quaternion rot, float fov)
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

        File.WriteAllBytes(path, screenShot.EncodeToPNG());
        Debug.Log($"Saved preview render: {path}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
