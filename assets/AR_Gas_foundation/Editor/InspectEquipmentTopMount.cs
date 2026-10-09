using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class InspectEquipmentTopMount
{
    [MenuItem("Tools/Inspect Equipment Top Mount")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject vessel = GameObject.Find("Confined_Space_Vessel_Tank");
        GameObject area = GameObject.Find("Confined_Space_Training_Area");

        Debug.Log("=== CONFINED SPACE STRUCTURE BOUNDS INSPECTION ===");
        if (vessel != null)
        {
            Renderer[] rList = vessel.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rList)
            {
                Debug.Log($"Vessel Child: {r.gameObject.name} | BoundsCenter: {r.bounds.center.ToString("F4")} Size: {r.bounds.size.ToString("F4")} MinY: {r.bounds.min.y:F4} MaxY: {r.bounds.max.y:F4}");
            }
        }

        // Also render wider camera views showing both the vessel top and yellow model at various Y and X positions
        GameObject yellowModel = GameObject.Find("Self_Retracting_Lifeline");
        Debug.Log($"YELLOW MODEL CURRENT WORLD POS: {yellowModel.transform.position.ToString("F4")}");

        // Let's render wide-angle closeups of yellow model together with the equipment top
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

        // View 1: Wide view of confined space top area with yellow model at original position Y=1.085
        RenderCameraView(cam, Path.Combine(artifactDir, "YELLOW_MOUNT_ORIGINAL.png"), new Vector3(1.90f, 0.90f, -1.20f), Quaternion.Euler(5, 0, 0), 45f);

        // Test Y = 0.70, 0.75, 0.80, 0.85, 0.90, 0.95, 1.00
        float[] testY = new float[] { 0.65f, 0.70f, 0.75f, 0.80f, 0.85f, 0.90f, 0.95f, 1.00f };
        for (int i = 0; i < testY.Length; i++)
        {
            float posY = testY[i];
            yellowModel.transform.position = new Vector3(2.2475f, posY, 0.2000f);
            RenderCameraView(cam, Path.Combine(artifactDir, $"YELLOW_MOUNT_Y_{posY:F2}.png"), new Vector3(1.90f, 0.90f, -1.20f), Quaternion.Euler(5, 0, 0), 45f);
        }
    }

    private static void RenderCameraView(Camera cam, string outputPath, Vector3 pos, Quaternion rot, float fov)
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

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
        Debug.Log($"Saved view screenshot: {outputPath}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
