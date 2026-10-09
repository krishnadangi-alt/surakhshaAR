using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class FastPPESurgicalFix
{
    [MenuItem("Tools/Fast PPE Surgical Fix")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // 1. BACKUP FIRST
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_PPEFix.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"=== BACKUP CREATED AT: {backupPath} ===");

        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        if (ppeStation == null)
        {
            Debug.LogError("PPE_Preparation_Station not found!");
            return;
        }

        // Keep table at left position X = -1.650
        ppeStation.transform.position = new Vector3(-1.6500f, 0.0000f, 0.2000f);

        // 2. REALISTIC BROWN WOODEN WORKBENCH
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
                    woodMat.color = new Color(0.42f, 0.24f, 0.12f, 1.0f); // Warm workshop brown wood
                    woodMat.SetFloat("_Glossiness", 0.22f);
                    woodMat.SetFloat("_Metallic", 0.01f);
                    AssetDatabase.CreateAsset(woodMat, "Assets/AR_Gas_foundation/Materials/MAT_Workbench_Wood.mat");
                    AssetDatabase.SaveAssets();
                }

                Material steelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Workbench_Steel_Frame.mat");
                if (steelMat == null) steelMat = woodMat;

                wbRenderer.sharedMaterials = new Material[] { woodMat, steelMat };
            }
        }

        float tableSurfaceY = ppeStation.transform.position.y + 0.7500f;

        // 3. EXACTLY ONE SAFETY HELMET (Sitting naturally on tabletop)
        Transform helmet = ppeStation.transform.Find("Safety_Helmet");
        if (helmet != null)
        {
            helmet.gameObject.SetActive(true);
            helmet.localPosition = new Vector3(-0.3500f, 0.8800f, 0.0000f);
            helmet.localRotation = Quaternion.Euler(0f, 45f, 0f);
            helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

            Renderer r = helmet.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                helmet.position += new Vector3(0, dy, 0);
            }
        }

        // 4. EXACTLY ONE SAFETY GOGGLES (Sitting naturally on tabletop)
        Transform goggles = ppeStation.transform.Find("Safety_Goggles");
        if (goggles != null)
        {
            goggles.gameObject.SetActive(true);
            goggles.localPosition = new Vector3(-0.1000f, 0.8250f, 0.0000f);
            goggles.localRotation = Quaternion.Euler(0f, 0f, 0f);
            goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

            Renderer r = goggles.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                goggles.position += new Vector3(0, dy, 0);
            }
        }

        // 5. EXACTLY TWO SAFETY GLOVES (Work_Gloves_Left pair lying flat, Work_Gloves_Right hidden)
        Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
        if (glovesLeft != null)
        {
            glovesLeft.gameObject.SetActive(true);
            glovesLeft.localPosition = new Vector3(0.1800f, 0.7850f, -0.0500f);
            glovesLeft.localRotation = Quaternion.Euler(0f, 0f, 0f); // Resting flat on tabletop
            glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

            Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float dy = tableSurfaceY - r.bounds.min.y;
                glovesLeft.position += new Vector3(0, dy, 0);
            }
        }

        // HIDE Work_Gloves_Right TO GUARANTEE EXACTLY TWO GLOVES (1 PAIR)
        Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
        if (glovesRight != null)
        {
            glovesRight.gameObject.SetActive(false);
        }

        // Remove any added temporary extra objects
        Transform extraDet = ppeStation.transform.Find("PPE_Gas_Detector");
        if (extraDet != null)
        {
            Object.DestroyImmediate(extraDet.gameObject);
        }

        // 6. EXACTLY ONE PAIR OF SAFETY BOOTS (Soles flat on lower shelf)
        Transform bootsLeft = ppeStation.transform.Find("Safety_Boots_Left");
        if (bootsLeft != null)
        {
            bootsLeft.gameObject.SetActive(true);
            bootsLeft.localPosition = new Vector3(-0.2000f, 0.3500f, 0.0000f);
            bootsLeft.localRotation = Quaternion.Euler(0, 90, 0);
            bootsLeft.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        }

        Transform bootsRight = ppeStation.transform.Find("Safety_Boots_Right");
        if (bootsRight != null)
        {
            bootsRight.gameObject.SetActive(true);
            bootsRight.localPosition = new Vector3(0.2000f, 0.3500f, 0.0000f);
            bootsRight.localRotation = Quaternion.Euler(0, 90, 0);
            bootsRight.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        }

        // SAVE MODIFIED SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"SAVED SCENE TO: {mainScenePath}");

        // TAKE ONE CLOSE FRONT SCREENSHOT OF THE PPE TABLE ONLY
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        // Aimed directly at PPE table center (-1.65, 0.45, 0.20) from front Z = -1.25 with 36deg FOV
        RenderView(cam, Path.Combine(artifactDir, "PPE_TABLE_CLOSEUP_FIX.png"), new Vector3(-1.6500f, 0.45f, -1.25f), Quaternion.Euler(0, 0, 0), 36f);
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
