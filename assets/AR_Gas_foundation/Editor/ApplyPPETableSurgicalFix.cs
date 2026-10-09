using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class ApplyPPETableSurgicalFix
{
    [MenuItem("Tools/Apply PPE Table Surgical Fix")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // STEP 0: CREATE TIMESTAMPED BACKUP FIRST
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_PPETableFix.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"=== STEP 0: BACKUP CREATED AT: {backupPath} ===");

        // Open main scene
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        if (ppeStation == null)
        {
            Debug.LogError("COULD NOT FIND 'PPE_Preparation_Station'!");
            return;
        }

        // STEP 1: MOVE ENTIRE PPE TABLE FARTHER LEFT (Away from gas cylinder at X=-0.25)
        // Target Position: (-1.65, 0.00, 0.20)
        ppeStation.transform.position = new Vector3(-1.6500f, 0.0000f, 0.2000f);
        Debug.Log($"STEP 1: MOVED PPE TABLE STATION TO: {ppeStation.transform.position.ToString("F4")}");

        // STEP 2: APPLY REALISTIC BROWN WOOD MATERIAL TO WORKBENCH
        GameObject workbenchMesh = GameObject.Find("workbench_hd_mesh");
        if (workbenchMesh != null)
        {
            Renderer wbRenderer = workbenchMesh.GetComponentInChildren<Renderer>();
            if (wbRenderer != null)
            {
                Material woodMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood.mat");
                if (woodMat == null)
                {
                    woodMat = new Material(Shader.Find("Standard"));
                    woodMat.name = "MAT_Workbench_Wood";
                    woodMat.color = new Color(0.42f, 0.24f, 0.12f, 1.0f); // Warm industrial workshop brown wood
                    woodMat.SetFloat("_Glossiness", 0.25f);
                    woodMat.SetFloat("_Metallic", 0.05f);
                    AssetDatabase.CreateAsset(woodMat, "Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood.mat");
                    AssetDatabase.SaveAssets();
                }

                Material steelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Structural_Steel.mat");
                if (steelMat == null) steelMat = woodMat;

                wbRenderer.sharedMaterials = new Material[] { woodMat, steelMat };
                Debug.Log($"STEP 2: APPLIED REALISTIC BROWN WOOD MATERIAL TO WORKBENCH: {woodMat.name}");
            }
        }

        // STEP 3 & 4: ARRANGE PPE NEATLY ON TOP OF THE WORKBENCH SURFACE
        // Find top surface Y by inspecting workbench mesh bounds
        Bounds wbBounds = workbenchMesh.GetComponentInChildren<Renderer>().bounds;
        Debug.Log($"Workbench World Bounds: Center={wbBounds.center}, Min={wbBounds.min}, Max={wbBounds.max}");

        // Top surface of table is at wbBounds.max.y
        float topSurfaceY = wbBounds.max.y;
        
        // 1. Safety Helmet
        Transform helmet = ppeStation.transform.Find("Safety_Helmet");
        if (helmet != null)
        {
            helmet.localPosition = new Vector3(-0.3500f, 0.8800f, 0.0000f);
            helmet.localRotation = Quaternion.Euler(0, 45, 0);
            helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);
            Renderer r = helmet.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = topSurfaceY - r.bounds.min.y;
                helmet.position += new Vector3(0, dy, 0);
            }
        }

        // 2. Safety Goggles
        Transform goggles = ppeStation.transform.Find("Safety_Goggles");
        if (goggles != null)
        {
            goggles.localPosition = new Vector3(-0.1000f, 0.8250f, 0.0000f);
            goggles.localRotation = Quaternion.Euler(0, 0, 0);
            goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);
            Renderer r = goggles.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = topSurfaceY - r.bounds.min.y;
                goggles.position += new Vector3(0, dy, 0);
            }
        }

        // 3. Work Gloves Left
        Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
        if (glovesLeft != null)
        {
            glovesLeft.localPosition = new Vector3(0.1200f, 0.7850f, -0.0500f);
            glovesLeft.localRotation = Quaternion.Euler(0, 0, 0);
            glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);
            Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = topSurfaceY - r.bounds.min.y;
                glovesLeft.position += new Vector3(0, dy, 0);
            }
        }

        // 4. Work Gloves Right
        Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
        if (glovesRight != null)
        {
            glovesRight.localPosition = new Vector3(0.3200f, 0.7850f, -0.0500f);
            glovesRight.localRotation = Quaternion.Euler(0, 180, 0);
            glovesRight.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);
            Renderer r = glovesRight.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = topSurfaceY - r.bounds.min.y;
                glovesRight.position += new Vector3(0, dy, 0);
            }
        }

        // STEP 5: PLACE SAFETY BOOTS PROPERLY ON THE LOWER SHELF (UPRIGHT, SOLES FACING DOWN FLAT ON SHELF SURFACE)
        // Position at local Y = 0.4800 so the soles rest flat directly on top of the lower wooden shelf bars
        
        // 1. Safety Boots Left
        Transform bootsLeft = ppeStation.transform.Find("Safety_Boots_Left");
        if (bootsLeft != null)
        {
            bootsLeft.localPosition = new Vector3(-0.2000f, 0.3500f, 0.0000f);
            bootsLeft.localRotation = Quaternion.Euler(0, 90, 0); // Soles facing down flat on lower shelf
            bootsLeft.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        }

        // 2. Safety Boots Right
        Transform bootsRight = ppeStation.transform.Find("Safety_Boots_Right");
        if (bootsRight != null)
        {
            bootsRight.localPosition = new Vector3(0.2000f, 0.3500f, 0.0000f);
            bootsRight.localRotation = Quaternion.Euler(0, 90, 0); // Soles facing down flat on lower shelf
            bootsRight.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        }

        Debug.Log("STEPS 3, 4, 5: ARRANGED HELMET, GOGGLES, GLOVES ON TOP SHELF & BOOTS FLAT ON LOWER SHELF (Y=0.3500)");

        // SAVE MODIFIED SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"SAVED SCENE TO: {mainScenePath}");

        // RENDER 2 MANDATORY VERIFICATION SCREENSHOTS
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // 1. FULL WORKSTATION SCREENSHOT
        RenderView(cam, Path.Combine(artifactDir, "FULL_WORKSTATION_PPE_FIX.png"), new Vector3(0.30f, 1.00f, -3.30f), Quaternion.Euler(0, 0, 0), 45f);

        // 2. PPE TABLE CLOSE-UP SCREENSHOT
        RenderView(cam, Path.Combine(artifactDir, "PPE_TABLE_CLOSEUP_FIX.png"), new Vector3(-1.6500f, 0.50f, -1.50f), Quaternion.Euler(0, 0, 0), 42f);
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
