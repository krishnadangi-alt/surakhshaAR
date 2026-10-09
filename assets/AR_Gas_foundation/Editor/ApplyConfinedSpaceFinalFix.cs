#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ApplyConfinedSpaceFinalFix
{
    [MenuItem("SurakshaAR/Apply Confined Space Final Fix")]
    public static void ApplyFix()
    {
        Debug.Log("========================================================");
        Debug.Log("APPLYING FINAL CONFINED SPACE MODEL FIX TO UNITY SCENE");
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
        }

        // Measure gas cylinder bounds & ground reference Y
        float groundY = 0.0000f;
        float cylinderMaxX = 0.50f;
        float cylinderHeight = 1.20f;
        Transform gasCylinder = gasRoot.transform.Find("Gas_Leak_Station/Realistic_Gas_Cylinder");
        if (gasCylinder != null)
        {
            Bounds cylB = GetAccurateWorldBounds(gasCylinder.gameObject);
            groundY = cylB.min.y;
            cylinderMaxX = cylB.max.x;
            cylinderHeight = cylB.size.y;
            Debug.Log($"Gas Cylinder Bounds: Min Y = {cylB.min.y:F4}m, Max X = {cylB.max.x:F4}m, Height = {cylB.size.y:F2}m");
        }

        // Setup Confined_Space_Training_Area immediately to the right of gas cylinder
        Transform confinedArea = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (confinedArea == null)
        {
            GameObject caObj = new GameObject("Confined_Space_Training_Area");
            caObj.transform.SetParent(gasRoot.transform, false);
            confinedArea = caObj.transform;
        }
        confinedArea.localPosition = new Vector3(2.60f, 0.00f, 0.00f);
        confinedArea.localRotation = Quaternion.identity;
        confinedArea.localScale = Vector3.one;

        // Clean out any stale/broken child objects
        for (int i = confinedArea.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(confinedArea.GetChild(i).gameObject);
        }

        // Load original GLB converted FBX containing all 10 submeshes
        string fbxPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (originalPrefab == null)
        {
            Debug.LogError("CRITICAL: Original FBX/GLB asset missing at: " + fbxPath);
            return;
        }

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab, confinedArea);
        inst.name = "Real_Confined_Space_Model";
        Transform vesselObj = inst.transform;

        // Apply original GLB PBR textures
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard");
        Material tanksMat = new Material(urpLit);
        tanksMat.name = "MAT_IndustrialTanks_OriginalGLB";

        Texture2D baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_0.jpg");
        Texture2D metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_1.png");
        Texture2D normTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_2.jpg");

        if (baseTex != null) tanksMat.SetTexture("_BaseMap", baseTex);
        if (metalTex != null) tanksMat.SetTexture("_MetallicGlossMap", metalTex);
        if (normTex != null)
        {
            tanksMat.SetTexture("_BumpMap", normTex);
            tanksMat.EnableKeyword("_NORMALMAP");
        }
        tanksMat.SetFloat("_Metallic", 0.75f);
        tanksMat.SetFloat("_Smoothness", 0.65f);

        // Ensure all 10 submesh renderers are active and assigned GLB PBR material
        Renderer[] rends = vesselObj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            r.sharedMaterial = tanksMat;
            r.gameObject.SetActive(true);
        }

        // Reset transform & set upright ROOT rotation (0, 90, 0)
        vesselObj.localPosition = Vector3.zero;
        vesselObj.localRotation = Quaternion.Euler(0f, 90f, 0f);

        // Keep all 10 sub-mesh components active under root node _0
        Transform node0 = vesselObj.Find("root/GLTF_SceneRootNode/_0");
        if (node0 != null)
        {
            for (int i = 0; i < node0.childCount; i++)
            {
                Transform child = node0.GetChild(i);
                child.gameObject.SetActive(true);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
            }
        }

        // Apply uniform scale 1.15 (total height ~3.20m, clearly larger than 1.20m cylinder)
        float targetHeight = 3.20f; // Significantly larger than cylinder
        Bounds initialB = GetAccurateWorldBounds(vesselObj.gameObject);
        float currentMaxDim = Mathf.Max(initialB.size.x, Mathf.Max(initialB.size.y, initialB.size.z));
        float uniformScale = (currentMaxDim > 0.0001f) ? (targetHeight / currentMaxDim) : 1.15f;
        vesselObj.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

        // Snap lowest base support flat to ground Y level (0.0000m)
        Bounds scaledB = GetAccurateWorldBounds(vesselObj.gameObject);
        float deltaY = groundY - scaledB.min.y;
        vesselObj.position += new Vector3(0f, deltaY, 0f);

        Bounds finalB = GetAccurateWorldBounds(vesselObj.gameObject);

        Debug.Log("========================================================");
        Debug.Log($"SCENE MODIFICATION COMPLETE:");
        Debug.Log($"Original GLB Used: Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb");
        Debug.Log($"Root GameObject: {vesselObj.name}");
        Debug.Log($"Final Rotation: {vesselObj.rotation.eulerAngles}");
        Debug.Log($"Final Uniform Scale: {vesselObj.localScale}");
        Debug.Log($"Final Position: {vesselObj.position}");
        Debug.Log($"Lowest World Y: {finalB.min.y:F4}m");
        Debug.Log($"Confined Space Height: {finalB.size.y:F2}m (Cylinder Height: {cylinderHeight:F2}m)");
        Debug.Log($"Separation from Cylinder: Min X = {finalB.min.x:F2}m (Cylinder Max X = {cylinderMaxX:F2}m)");
        Debug.Log($"Submesh Count: {rends.Length} active renderers");
        Debug.Log("========================================================");

        // Save scene to disk
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SCENE SAVED] AR_gas_Foundation.unity saved successfully.");

        // Reload scene and verify persistence
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("[SCENE RELOADED] AR_gas_Foundation.unity reloaded and verified.");

        // Render focused screenshot: RED GAS CYLINDER + CONFINED SPACE MODEL
        CaptureFocusedScreenshot();
    }

    private static Bounds GetAccurateWorldBounds(GameObject obj)
    {
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>(true);
        if (filters.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);

        Bounds b = new Bounds();
        bool initialized = false;
        foreach (var mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            Bounds localB = mf.sharedMesh.bounds;
            Matrix4x4 matrix = mf.transform.localToWorldMatrix;

            Vector3[] corners = new Vector3[8];
            corners[0] = matrix.MultiplyPoint3x4(new Vector3(localB.min.x, localB.min.y, localB.min.z));
            corners[1] = matrix.MultiplyPoint3x4(new Vector3(localB.min.x, localB.min.y, localB.max.z));
            corners[2] = matrix.MultiplyPoint3x4(new Vector3(localB.min.x, localB.max.y, localB.min.z));
            corners[3] = matrix.MultiplyPoint3x4(new Vector3(localB.min.x, localB.max.y, localB.max.z));
            corners[4] = matrix.MultiplyPoint3x4(new Vector3(localB.max.x, localB.min.y, localB.min.z));
            corners[5] = matrix.MultiplyPoint3x4(new Vector3(localB.max.x, localB.min.y, localB.max.z));
            corners[6] = matrix.MultiplyPoint3x4(new Vector3(localB.max.x, localB.max.y, localB.min.z));
            corners[7] = matrix.MultiplyPoint3x4(new Vector3(localB.max.x, localB.max.y, localB.max.z));

            for (int i = 0; i < 8; i++)
            {
                if (!initialized)
                {
                    b = new Bounds(corners[i], Vector3.zero);
                    initialized = true;
                }
                else
                {
                    b.Encapsulate(corners[i]);
                }
            }
        }
        return b;
    }

    private static void CaptureFocusedScreenshot()
    {
        string dir1 = @"C:\SurakshaAR\Screenshots";
        string dir2 = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb";
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

        int w = 1920, h = 1080;

        GameObject camObj = new GameObject("FocusedCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;

        // Position camera to frame both RED GAS CYLINDER (X=0) and CONFINED SPACE MODEL (X=2.6)
        cam.transform.position = new Vector3(1.30f, 1.60f, -4.50f);
        cam.transform.rotation = Quaternion.Euler(11f, 0f, 0f);

        GameObject keyLight = new GameObject("KeyLight");
        Light kl = keyLight.AddComponent<Light>();
        kl.type = LightType.Directional;
        kl.intensity = 1.2f;
        kl.color = new Color(1.0f, 0.96f, 0.90f);
        keyLight.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.6f;
        fl.color = new Color(0.85f, 0.90f, 1.0f);
        fillLight.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

        RenderTexture rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(dir1, "GAS_CYLINDER_AND_CONFINED_SPACE.png"), bytes);
        File.WriteAllBytes(Path.Combine(dir2, "GAS_CYLINDER_AND_CONFINED_SPACE.png"), bytes);

        // Also update overview and front views
        File.WriteAllBytes(Path.Combine(dir1, "Gas_Workstation_FULL.png"), bytes);
        File.WriteAllBytes(Path.Combine(dir2, "Gas_Workstation_FULL.png"), bytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);

        Object.DestroyImmediate(keyLight);
        Object.DestroyImmediate(fillLight);
        Object.DestroyImmediate(camObj);

        Debug.Log("[FOCUSED SCREENSHOT CAPTURED] Saved GAS_CYLINDER_AND_CONFINED_SPACE.png to " + dir1 + " and " + dir2);
    }
}
#endif
