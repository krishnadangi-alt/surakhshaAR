using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class InspectTripodTopBeam
{
    [MenuItem("Tools/Inspect Tripod Top Beam")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject yellowObj = GameObject.Find("Self_Retracting_Lifeline");
        GameObject areaObj = GameObject.Find("Confined_Space_Training_Area");

        Debug.Log("=== INSPECTING ALL RENDERERS NEAR X=2.25 ===");
        Renderer[] renderers = areaObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.bounds.center.x >= 2.0f && r.bounds.center.x <= 2.5f)
            {
                Debug.Log($"Obj: '{r.gameObject.name}' | Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}, MinY: {r.bounds.min.y:F4}, MaxY: {r.bounds.max.y:F4}");
            }
        }

        // Now let's test Y positions for Self_Retracting_Lifeline so Carabiner.002 (the top loop) hooks cleanly into the top mounting point!
        // Carabiner.002 local offset in Lifeline is around Y = +0.2175 relative to Lifeline root Y.
        // Or Hook (bottom hook) local offset is around Y = -0.1875.
        // If Carabiner top loop is mounted at top beam (e.g. Y = 1.05..1.20):
        // Let's render close-up screenshots focusing specifically on (X=2.25, Y=1.0..1.3, Z=0.2) from camera at (2.25, 1.10, -1.0)

        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

        float[] candidateY = new float[] { 0.85f, 0.90f, 0.95f, 0.98f, 1.00f, 1.02f, 1.05f, 1.08f, 1.10f };

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        foreach (float y in candidateY)
        {
            yellowObj.transform.position = new Vector3(2.2475f, y, 0.2000f);

            // Render camera view focused at X=2.25, Z=0.20
            RenderCam(cam, Path.Combine(artifactDir, $"TRIPOD_CLOSEUP_Y_{y:F2}.png"), new Vector3(2.2475f, 1.05f, -0.90f), Quaternion.Euler(0, 0, 0), 45f);
            RenderCam(cam, Path.Combine(artifactDir, $"FULL_WORKSTATION_Y_{y:F2}.png"), new Vector3(0.50f, 1.00f, -3.20f), Quaternion.Euler(0, 0, 0), 45f);
        }
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

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
