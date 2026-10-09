using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class ApplyFinalPPERealismFix
{
    [MenuItem("Tools/Apply Final PPE Realism Fix")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // STEP 1: CREATE TIMESTAMPED BACKUP FIRST
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

        // STEP 2: WORKBENCH REALISTIC WOOD MATERIAL
        GameObject workbenchMesh = GameObject.Find("workbench_hd_mesh");
        if (workbenchMesh != null)
        {
            Renderer wbRenderer = workbenchMesh.GetComponentInChildren<Renderer>();
            if (wbRenderer != null)
            {
                // Submesh 0: Tabletop surface & lower shelf wooden planks (Rich warm oak wood)
                Material woodTopMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood_Top.mat");
                if (woodTopMat == null)
                {
                    woodTopMat = new Material(Shader.Find("Standard"));
                    woodTopMat.name = "MAT_Workbench_Wood_Top";
                    woodTopMat.color = new Color(0.42f, 0.25f, 0.12f, 1.0f); // Warm workshop oak wood
                    woodTopMat.SetFloat("_Glossiness", 0.22f);
                    woodTopMat.SetFloat("_Metallic", 0.01f);
                    AssetDatabase.CreateAsset(woodTopMat, "Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood_Top.mat");
                }

                // Submesh 1: Solid wooden legs & frame (Darker solid timber)
                Material woodFrameMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood_Frame.mat");
                if (woodFrameMat == null)
                {
                    woodFrameMat = new Material(Shader.Find("Standard"));
                    woodFrameMat.name = "MAT_Workbench_Wood_Frame";
                    woodFrameMat.color = new Color(0.30f, 0.18f, 0.09f, 1.0f); // Dark industrial timber
                    woodFrameMat.SetFloat("_Glossiness", 0.18f);
                    woodFrameMat.SetFloat("_Metallic", 0.01f);
                    AssetDatabase.CreateAsset(woodFrameMat, "Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood_Frame.mat");
                }

                wbRenderer.sharedMaterials = new Material[] { woodTopMat, woodFrameMat };
                AssetDatabase.SaveAssets();
            }
        }

        // Top surface of table is at World Y = 0.7500
        float tableSurfaceY = ppeStation.transform.position.y + 0.7500f;

        // 1. SAFETY HELMET (EXACTLY ONE: Sitting right-side-up flat on tabletop, dome UP)
        Transform helmet = ppeStation.transform.Find("Safety_Helmet");
        if (helmet != null)
        {
            helmet.gameObject.SetActive(true);
            helmet.localPosition = new Vector3(-0.4000f, 0.7500f, 0.0000f);
            helmet.localRotation = Quaternion.Euler(0f, 45f, 0f); // Right-side-up, dome pointing up, brim flat on table
            helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

            Renderer r = helmet.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                helmet.position += new Vector3(0, dy, 0);
            }
        }

        // 2. SAFETY GOGGLES (EXACTLY ONE: Resting flat on tabletop)
        Transform goggles = ppeStation.transform.Find("Safety_Goggles");
        if (goggles != null)
        {
            goggles.gameObject.SetActive(true);
            goggles.localPosition = new Vector3(-0.1800f, 0.7500f, 0.0000f);
            goggles.localRotation = Quaternion.Euler(0f, 15f, 0f);
            goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

            Renderer r = goggles.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                goggles.position += new Vector3(0, dy, 0);
            }
        }

        // 3. SAFETY GLOVES (EXACTLY TWO: Lying horizontally flat on tabletop)
        Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
        if (glovesLeft != null)
        {
            glovesLeft.gameObject.SetActive(true);
            glovesLeft.localPosition = new Vector3(0.0800f, 0.7500f, -0.0200f);
            glovesLeft.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Lying horizontally flat on tabletop
            glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

            Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                glovesLeft.position += new Vector3(0, dy, 0);
            }
        }

        // Ensure Work_Gloves_Right is INACTIVE so visible count = EXACTLY 2 GLOVES
        Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
        if (glovesRight != null)
        {
            glovesRight.gameObject.SetActive(false);
        }

        // 4. OTHER PPE ITEM (Portable Handheld Gas Detector on Tabletop)
        Transform detectorOnTable = ppeStation.transform.Find("PPE_Gas_Detector");
        if (detectorOnTable == null)
        {
            GameObject detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab");
            if (detectorPrefab == null) detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab");
            
            if (detectorPrefab != null)
            {
                GameObject detObj = (GameObject)PrefabUtility.InstantiatePrefab(detectorPrefab, ppeStation.transform);
                detObj.name = "PPE_Gas_Detector";
                detectorOnTable = detObj.transform;
            }
        }

        if (detectorOnTable != null)
        {
            detectorOnTable.gameObject.SetActive(true);
            detectorOnTable.localPosition = new Vector3(0.3800f, 0.7500f, 0.0000f);
            detectorOnTable.localRotation = Quaternion.Euler(0f, -30f, 0f);
            detectorOnTable.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);

            Renderer r = detectorOnTable.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                detectorOnTable.position += new Vector3(0, dy, 0);
            }
        }

        // 5. SAFETY BOOTS (EXACTLY ONE PAIR: Upright with soles flat on lower shelf)
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

        Debug.Log("FINISHED APPLYING FINAL PPE REALISM AND PHYSICAL PLACEMENT FIX");

        // SAVE MODIFIED SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"SAVED SCENE TO: {mainScenePath}");

        // RENDER 2 MANDATORY VERIFICATION SCREENSHOTS
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // 1. PPE TABLE CLOSE-UP SCREENSHOT (FRONT VIEW)
        RenderView(cam, Path.Combine(artifactDir, "PPE_TABLE_CLOSEUP_FIX.png"), new Vector3(-1.6500f, 0.60f, -1.40f), Quaternion.Euler(0, 0, 0), 40f);

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
