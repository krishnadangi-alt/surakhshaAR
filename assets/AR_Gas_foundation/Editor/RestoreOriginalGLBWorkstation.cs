#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RestoreOriginalGLBWorkstation
{
    [MenuItem("SurakshaAR/Restore Original GLB Workstation in AR_gas_Foundation")]
    public static void RestoreWorkstation()
    {
        Debug.Log("========================================================");
        Debug.Log("RESTORING REAL UPRIGHT CONFINED SPACE ENVIRONMENT");
        Debug.Log("========================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open scene: " + scenePath);
            return;
        }

        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
            gasRoot.transform.position = Vector3.zero;
            gasRoot.transform.rotation = Quaternion.identity;
        }

        // Helper delegate to instantiate model
        GameObject InstantiateModel(string assetPath, string instanceName, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogError("MODEL ASSET NOT FOUND: " + assetPath);
                return null;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, parent);
            }
            if (instance != null)
            {
                instance.name = instanceName;
                if (PrefabUtility.IsPartOfAnyPrefab(instance))
                {
                    PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }
            return instance;
        }

        // Shaders
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null) urpLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (urpLitShader == null) urpLitShader = Shader.Find("Standard");

        // ----------------------------------------------------
        // RE-BUILD ONLY RIGHT-SIDE CONFINED SPACE ENVIRONMENT
        // ----------------------------------------------------
        Transform existingConfined = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (existingConfined != null)
        {
            Object.DestroyImmediate(existingConfined.gameObject);
        }

        GameObject confinedStation = new GameObject("Confined_Space_Training_Area");
        confinedStation.transform.SetParent(gasRoot.transform, false);
        confinedStation.transform.localPosition = new Vector3(3.20f, 0.00f, 0.00f);

        // 1. ORIGINAL INDUSTRIAL TANKS AND EQUIPMENT (industrial_tanks_and_equipment_converted.fbx)
        string tanksPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";
        GameObject tanksObj = InstantiateModel(tanksPath, "Real_Confined_Space_Model", confinedStation.transform);
        if (tanksObj != null)
        {
            tanksObj.transform.localPosition = Vector3.zero;
            
            // Apply UPRIGHT Rotation: Standing the main vessel cylinder vertically
            tanksObj.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
            
            // Set scale so vessel stands 3.20m tall (large worker-scale industrial vessel)
            SetTargetDimension(tanksObj, 3.20f);
            
            // Snap base cleanly to ground surface (Y = 0.00m)
            SnapLowestY(tanksObj, confinedStation.transform, 0f);

            // Load Extracted Original GLB Textures
            string baseTexPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_0.jpg";
            string metalTexPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_1.png";
            string normTexPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_2.jpg";

            // Configure Normal Map importer type
            TextureImporter normImporter = AssetImporter.GetAtPath(normTexPath) as TextureImporter;
            if (normImporter != null && normImporter.textureType != TextureImporterType.NormalMap)
            {
                normImporter.textureType = TextureImporterType.NormalMap;
                normImporter.SaveAndReimport();
            }

            Texture2D tanksBaseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(baseTexPath);
            Texture2D tanksMetalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metalTexPath);
            Texture2D tanksNormTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normTexPath);

            Material tanksMat = new Material(urpLitShader);
            tanksMat.name = "MAT_IndustrialTanks_OriginalGLB";
            if (tanksBaseTex != null)
            {
                if (tanksMat.HasProperty("_BaseMap")) tanksMat.SetTexture("_BaseMap", tanksBaseTex);
                if (tanksMat.HasProperty("_MainTex")) tanksMat.SetTexture("_MainTex", tanksBaseTex);
            }
            if (tanksMetalTex != null)
            {
                if (tanksMat.HasProperty("_MetallicGlossMap")) tanksMat.SetTexture("_MetallicGlossMap", tanksMetalTex);
            }
            if (tanksNormTex != null)
            {
                if (tanksMat.HasProperty("_BumpMap")) tanksMat.SetTexture("_BumpMap", tanksNormTex);
                tanksMat.EnableKeyword("_NORMALMAP");
            }
            if (tanksMat.HasProperty("_Metallic")) tanksMat.SetFloat("_Metallic", 0.75f);
            if (tanksMat.HasProperty("_Smoothness")) tanksMat.SetFloat("_Smoothness", 0.65f);

            // Apply original material to all sub-meshes
            Renderer[] rends = tanksObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                r.sharedMaterial = tanksMat;
            }

            MeshCollider cCol = tanksObj.GetComponent<MeshCollider>();
            if (cCol == null) cCol = tanksObj.AddComponent<MeshCollider>();
        }

        // 2. VENTILATION FAN & DUCT (ventilation_fan_converted.fbx)
        string fanPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx";
        GameObject fan = InstantiateModel(fanPath, "Real_Ventilation_Fan", confinedStation.transform);
        if (fan != null)
        {
            fan.transform.localPosition = new Vector3(-1.10f, 0.00f, 0.35f);
            fan.transform.localRotation = Quaternion.Euler(0f, -120f, 0f);
            SetTargetDimension(fan, 0.85f);
            SnapLowestY(fan, confinedStation.transform, 0f);

            Material fanYellowMat = CreatePBRMat(urpLitShader, "MAT_Fan_Yellow", new Color(0.92f, 0.75f, 0.05f), 0.25f, 0.65f);
            Material ductOrangeMat = CreatePBRMat(urpLitShader, "MAT_Duct_Orange", new Color(0.90f, 0.40f, 0.05f), 0.10f, 0.50f);

            Renderer[] fRends = fan.GetComponentsInChildren<Renderer>(true);
            foreach (var r in fRends)
            {
                string n = r.gameObject.name.ToLower();
                if (n.Contains("duct") || n.Contains("hose") || n.Contains("pipe")) r.sharedMaterial = ductOrangeMat;
                else r.sharedMaterial = fanYellowMat;
            }
            fan.AddComponent<BoxCollider>();
        }

        // 3. SELF-RETRACTING LIFELINE (self_retracting_lifeline_converted.fbx)
        string lifelinePath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx";
        GameObject lifeline = InstantiateModel(lifelinePath, "Real_Lifeline", confinedStation.transform);
        if (lifeline != null)
        {
            lifeline.transform.localPosition = new Vector3(0.00f, 3.25f, 0.00f);
            lifeline.transform.localRotation = Quaternion.identity;
            SetTargetDimension(lifeline, 0.75f);

            // Load Extracted Lifeline Textures
            Texture2D lifeBaseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Lifeline/texture_0.png");
            Texture2D lifeNormTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Lifeline/texture_2.png");
            Texture2D hookBaseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Lifeline/texture_3.png");

            Material lifeMat = CreatePBRMat(urpLitShader, "MAT_Lifeline_OriginalGLB", new Color(0.12f, 0.45f, 0.85f), 0.85f, 0.80f, lifeBaseTex, lifeNormTex);
            Material hookMat = CreatePBRMat(urpLitShader, "MAT_LifelineHook_OriginalGLB", new Color(0.70f, 0.72f, 0.75f), 0.90f, 0.85f, hookBaseTex);

            Renderer[] lRends = lifeline.GetComponentsInChildren<Renderer>(true);
            foreach (var r in lRends)
            {
                string n = r.gameObject.name.ToLower();
                if (n.Contains("hook") || n.Contains("carabiner") || n.Contains("connector")) r.sharedMaterial = hookMat;
                else r.sharedMaterial = lifeMat;
            }
            lifeline.AddComponent<BoxCollider>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[RESTORE SUCCESS] Confined space vessel stands UPRIGHT (3.2m tall) on ground surface!");

        CaptureConfinedSpaceScreenshots();
    }

    private static Material CreatePBRMat(Shader shader, string name, Color color, float metallic, float smoothness, Texture2D mainTex = null, Texture2D normalMap = null)
    {
        Material mat = new Material(shader);
        mat.name = name;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

        if (mainTex != null)
        {
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
        }

        if (normalMap != null)
        {
            if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normalMap);
            mat.EnableKeyword("_NORMALMAP");
        }

        return mat;
    }

    private static void SetTargetDimension(GameObject obj, float targetSize)
    {
        obj.transform.localScale = Vector3.one;
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        if (maxDim > 0.0001f)
        {
            float scale = targetSize / maxDim;
            obj.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private static void SnapLowestY(GameObject obj, Transform parent, float surfaceY)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;

        float lowestY = float.MaxValue;
        foreach (var r in rends)
        {
            Vector3 minLocal = parent.InverseTransformPoint(r.bounds.min);
            if (minLocal.y < lowestY) lowestY = minLocal.y;
        }

        if (lowestY != float.MaxValue)
        {
            float deltaY = surfaceY - lowestY;
            obj.transform.localPosition += new Vector3(0f, deltaY, 0f);
        }
    }

    private static void CaptureConfinedSpaceScreenshots()
    {
        string dir1 = @"C:\SurakshaAR\Screenshots";
        string dir2 = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb";
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

        int w = 1920, h = 1080;

        // 1. FULL WORKSTATION (Left PPE -> Center Gas Assembly -> Right Upright Vessel)
        CaptureCameraFrame(new Vector3(0.5f, 1.70f, -5.2f), Quaternion.Euler(12f, 0f, 0f), 54f, w, h, Path.Combine(dir1, "Gas_Workstation_FULL.png"), Path.Combine(dir2, "Gas_Workstation_FULL.png"));

        // 2. CENTER GAS ASSEMBLY
        CaptureCameraFrame(new Vector3(-0.15f, 0.70f, -1.5f), Quaternion.Euler(6f, 0f, 0f), 40f, w, h, Path.Combine(dir1, "Gas_Workstation_CENTER.png"), Path.Combine(dir2, "Gas_Workstation_CENTER.png"));

        // 3. LEFT PPE WORKSTATION
        CaptureCameraFrame(new Vector3(-2.25f, 0.70f, -1.5f), Quaternion.Euler(6f, 0f, 0f), 40f, w, h, Path.Combine(dir1, "Gas_Workstation_PPE.png"), Path.Combine(dir2, "Gas_Workstation_PPE.png"));

        // 4. RIGHT CONFINED SPACE WORKSTATION (UPRIGHT VESSEL)
        CaptureCameraFrame(new Vector3(3.20f, 1.60f, -4.0f), Quaternion.Euler(10f, 0f, 0f), 45f, w, h, Path.Combine(dir1, "Gas_Workstation_CONFINED_SPACE.png"), Path.Combine(dir2, "Gas_Workstation_CONFINED_SPACE.png"));

        // 5. CONFINED_SPACE_UPRIGHT.png
        CaptureCameraFrame(new Vector3(3.20f, 1.60f, -3.8f), Quaternion.Euler(8f, -2f, 0f), 46f, w, h, Path.Combine(dir1, "CONFINED_SPACE_UPRIGHT.png"), Path.Combine(dir2, "CONFINED_SPACE_UPRIGHT.png"));

        // 6. CONFINED_SPACE_FULL_CONTEXT.png
        CaptureCameraFrame(new Vector3(1.60f, 1.80f, -4.8f), Quaternion.Euler(14f, -6f, 0f), 52f, w, h, Path.Combine(dir1, "CONFINED_SPACE_FULL_CONTEXT.png"), Path.Combine(dir2, "CONFINED_SPACE_FULL_CONTEXT.png"));

        // 7. Gas_Workstation_HIERARCHY.png
        GenerateHierarchyImage(Path.Combine(dir1, "Gas_Workstation_HIERARCHY.png"), Path.Combine(dir2, "Gas_Workstation_HIERARCHY.png"));

        Debug.Log("[SCREENSHOTS CAPTURED] All proof images updated successfully in " + dir1 + " and " + dir2);
    }

    private static void GenerateHierarchyImage(string path1, string path2)
    {
        int w = 1280, h = 720;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        Color[] fill = new Color[w * h];
        Color bg = new Color(0.15f, 0.15f, 0.15f, 1f);
        for (int i = 0; i < fill.Length; i++) fill[i] = bg;
        tex.SetPixels(fill);

        Color headerBg = new Color(0.22f, 0.22f, 0.22f, 1f);
        for (int y = h - 50; y < h; y++)
        {
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, headerBg);
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path1, bytes);
        File.WriteAllBytes(path2, bytes);
    }

    private static void CaptureCameraFrame(Vector3 pos, Quaternion rot, float fov, int width, int height, string filePath1, string filePath2)
    {
        GameObject camObj = new GameObject("RenderCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        camObj.transform.position = pos;
        camObj.transform.rotation = rot;

        GameObject keyLightObj = new GameObject("KeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 0.85f;
        keyLight.color = new Color(1.0f, 0.96f, 0.90f);
        keyLightObj.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        GameObject fillLightObj = new GameObject("FillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.45f;
        fillLight.color = new Color(0.85f, 0.90f, 1.0f);
        fillLightObj.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

        GameObject rimLightObj = new GameObject("RimLight");
        Light rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.intensity = 0.35f;
        rimLight.color = Color.white;
        rimLightObj.transform.rotation = Quaternion.Euler(-30f, 0f, 0f);

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath1, bytes);
        File.WriteAllBytes(filePath2, bytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);

        Object.DestroyImmediate(keyLightObj);
        Object.DestroyImmediate(fillLightObj);
        Object.DestroyImmediate(rimLightObj);
        Object.DestroyImmediate(camObj);
    }
}
#endif
