using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TestHelmetAndGlovesFix
{
    [MenuItem("Tools/Test Helmet And Gloves Fix")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        if (ppeStation == null) return;

        // REMOVE ANY ADDED EXTRA DETECTOR
        Transform extraDet = ppeStation.transform.Find("PPE_Gas_Detector");
        if (extraDet != null)
        {
            Object.DestroyImmediate(extraDet.gameObject);
        }

        float tableSurfaceY = ppeStation.transform.position.y + 0.7500f;

        // 1. SAFETY HELMET (Dome UP, resting flat on tabletop)
        Transform helmet = ppeStation.transform.Find("Safety_Helmet");
        if (helmet != null)
        {
            helmet.gameObject.SetActive(true);
            helmet.localPosition = new Vector3(-0.3500f, 0.7500f, 0.0000f);
            // Test rotation for upright dome UP: (0, 45, 0) in original FBX was rotated inside child.
            // Let's check child localRotations of Helmet:
            // Helmet.fbx has localRotation (90, 0, 0), child Helmet has (270, 0, 0).
            // Parent rotation (0, 0, 0) or (0, 45, 0)
            helmet.localRotation = Quaternion.Euler(0f, 45f, 0f);
            helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

            Renderer r = helmet.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                helmet.position += new Vector3(0, dy, 0);
                Debug.Log($"Helmet bounds center: {r.bounds.center}, min: {r.bounds.min}, max: {r.bounds.max}");
            }
        }

        // 2. SAFETY GOGGLES (Resting flat on tabletop)
        Transform goggles = ppeStation.transform.Find("Safety_Goggles");
        if (goggles != null)
        {
            goggles.gameObject.SetActive(true);
            goggles.localPosition = new Vector3(-0.1000f, 0.7500f, 0.0000f);
            goggles.localRotation = Quaternion.Euler(0f, 0f, 0f);
            goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

            Renderer r = goggles.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                goggles.position += new Vector3(0, dy, 0);
            }
        }

        // 3. SAFETY GLOVES (Work_Gloves_Left = EXACTLY TWO GLOVES lying flat)
        Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
        if (glovesLeft != null)
        {
            glovesLeft.gameObject.SetActive(true);
            glovesLeft.localPosition = new Vector3(0.1500f, 0.7500f, -0.0200f);
            glovesLeft.localRotation = Quaternion.Euler(0f, 0f, 0f); // Original flat rotation
            glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

            Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                glovesLeft.position += new Vector3(0, dy, 0);
            }
        }

        // HIDE Work_Gloves_Right so count is EXACTLY 2 GLOVES
        Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
        if (glovesRight != null)
        {
            glovesRight.gameObject.SetActive(false);
        }

        // 4. BOOTS ON LOWER SHELF
        Transform bootsLeft = ppeStation.transform.Find("Safety_Boots_Left");
        if (bootsLeft != null)
        {
            bootsLeft.gameObject.SetActive(true);
            bootsLeft.localPosition = new Vector3(-0.1800f, 0.3500f, 0.0000f);
            bootsLeft.localRotation = Quaternion.Euler(0, 90, 0);
            bootsLeft.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        }

        Transform bootsRight = ppeStation.transform.Find("Safety_Boots_Right");
        if (bootsRight != null)
        {
            bootsRight.gameObject.SetActive(true);
            bootsRight.localPosition = new Vector3(0.1800f, 0.3500f, 0.0000f);
            bootsRight.localRotation = Quaternion.Euler(0, 90, 0);
            bootsRight.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // PPE TABLE CLOSE-UP SCREENSHOT (FRONT VIEW)
        RenderView(cam, Path.Combine(artifactDir, "PPE_TABLE_CLOSEUP_FIX.png"), new Vector3(-1.6500f, 0.50f, -1.50f), Quaternion.Euler(0, 0, 0), 42f);

        // FULL WORKSTATION SCREENSHOT
        RenderView(cam, Path.Combine(artifactDir, "FULL_WORKSTATION_PPE_FIX.png"), new Vector3(0.30f, 1.00f, -3.30f), Quaternion.Euler(0, 0, 0), 45f);
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
        Debug.Log($"Saved verification screenshot: {outputPath}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
