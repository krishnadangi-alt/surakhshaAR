using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TestPPERealismAndPlacement
{
    [MenuItem("Tools/Test PPE Realism And Placement")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // STEP 1: CREATE BACKUP FIRST
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_PPECorrection.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"=== STEP 1: BACKUP CREATED AT: {backupPath} ===");

        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        if (ppeStation == null)
        {
            Debug.LogError("PPE_Preparation_Station not found!");
            return;
        }

        // Table position fixed at X = -1.6500 (Clean gap from cylinder at X = -0.2500)
        ppeStation.transform.position = new Vector3(-1.6500f, 0.0000f, 0.2000f);

        // WORKBENCH MATERIALS
        GameObject workbenchMesh = GameObject.Find("workbench_hd_mesh");
        if (workbenchMesh != null)
        {
            Renderer wbRenderer = workbenchMesh.GetComponentInChildren<Renderer>();
            if (wbRenderer != null)
            {
                // Submesh 0: Planks/Tabletop
                Material woodTopMat = new Material(Shader.Find("Standard"));
                woodTopMat.name = "MAT_Workbench_Wood_Top";
                woodTopMat.color = new Color(0.44f, 0.26f, 0.13f, 1.0f); // Warm workshop oak wood
                woodTopMat.SetFloat("_Glossiness", 0.35f);
                woodTopMat.SetFloat("_Metallic", 0.02f);

                // Submesh 1: Steel Legs/Frame
                Material steelFrameMat = new Material(Shader.Find("Standard"));
                steelFrameMat.name = "MAT_Workbench_Steel_Frame";
                steelFrameMat.color = new Color(0.20f, 0.22f, 0.24f, 1.0f); // Dark industrial steel frame
                steelFrameMat.SetFloat("_Glossiness", 0.50f);
                steelFrameMat.SetFloat("_Metallic", 0.65f);

                wbRenderer.sharedMaterials = new Material[] { woodTopMat, steelFrameMat };
            }
        }

        // Table top surface is at local Y = 0.7500 (World Y = 0.7500)

        // 1. SAFETY HELMET (Resting flat upright on tabletop)
        Transform helmet = ppeStation.transform.Find("Safety_Helmet");
        if (helmet != null)
        {
            helmet.gameObject.SetActive(true);
            // Orient helmet so brim bottom rests flat on table
            helmet.localPosition = new Vector3(-0.3800f, 0.7520f, 0.0000f);
            helmet.localRotation = Quaternion.Euler(-90f, 45f, 0f);
            helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

            // Fine-tune helmet Y using bounds
            Renderer r = helmet.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float targetY = ppeStation.transform.position.y + 0.7500f;
                float deltaY = targetY - r.bounds.min.y;
                helmet.position += new Vector3(0, deltaY, 0);
            }
        }

        // 2. SAFETY GOGGLES (Resting flat on tabletop)
        Transform goggles = ppeStation.transform.Find("Safety_Goggles");
        if (goggles != null)
        {
            goggles.gameObject.SetActive(true);
            goggles.localPosition = new Vector3(-0.1200f, 0.7500f, 0.0000f);
            goggles.localRotation = Quaternion.Euler(0f, 15f, 0f);
            goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

            Renderer r = goggles.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float targetY = ppeStation.transform.position.y + 0.7500f;
                float deltaY = targetY - r.bounds.min.y;
                goggles.position += new Vector3(0, deltaY, 0);
            }
        }

        // 3. EXACTLY TWO SAFETY GLOVES (One Pair via Work_Gloves_Left)
        Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
        if (glovesLeft != null)
        {
            glovesLeft.gameObject.SetActive(true);
            glovesLeft.localPosition = new Vector3(0.1800f, 0.7500f, -0.0200f);
            glovesLeft.localRotation = Quaternion.Euler(0f, -10f, 0f);
            glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

            Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float targetY = ppeStation.transform.position.y + 0.7500f;
                float deltaY = targetY - r.bounds.min.y;
                glovesLeft.position += new Vector3(0, deltaY, 0);
            }
        }

        // HIDE EXTRA GLOVES MODEL (Work_Gloves_Right) TO ENSURE EXACTLY TWO GLOVES ON TABLE
        Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
        if (glovesRight != null)
        {
            glovesRight.gameObject.SetActive(false);
        }

        // 4. LOWER SHELF BOOTS (Upright side-by-side with soles flat on lower shelf)
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

        // SAVE MODIFIED SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"SAVED SCENE TO: {mainScenePath}");

        // RENDER VERIFICATION SCREENSHOTS
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // 1. PPE TABLE CLOSE-UP SCREENSHOT (FRONT VIEW)
        RenderView(cam, Path.Combine(artifactDir, "PPE_TABLE_CLOSEUP_FIX.png"), new Vector3(-1.6500f, 0.55f, -1.35f), Quaternion.Euler(0, 0, 0), 40f);

        // 2. FULL WORKSTATION SCREENSHOT
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
