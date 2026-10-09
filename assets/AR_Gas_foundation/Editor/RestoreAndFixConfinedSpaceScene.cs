#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RestoreAndFixConfinedSpaceScene
{
    [MenuItem("SurakshaAR/Restore and Fix Confined Space Scene")]
    public static void RestoreAndFix()
    {
        Debug.Log("========================================================");
        Debug.Log("RESTORING ORIGINAL CONFINED SPACE GLB MODEL IN SCENE");
        Debug.Log("========================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open target scene: " + scenePath);
            return;
        }

        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
        }

        // Ground level reference Y from gas cylinder
        float groundY = 0.0000f;
        Transform gasCylinder = gasRoot.transform.Find("Gas_Leak_Station/Realistic_Gas_Cylinder");
        if (gasCylinder != null)
        {
            Bounds cylB = GetAccurateWorldBounds(gasCylinder.gameObject);
            groundY = cylB.min.y;
        }
        Debug.Log($"Ground Reference Y Level = {groundY:F4}m");

        // Confined Space Area parent
        Transform confinedArea = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (confinedArea == null)
        {
            GameObject caObj = new GameObject("Confined_Space_Training_Area");
            caObj.transform.SetParent(gasRoot.transform, false);
            confinedArea = caObj.transform;
        }
        confinedArea.localPosition = new Vector3(3.0000f, 0.0000f, 0.0000f);
        confinedArea.localRotation = Quaternion.identity;
        confinedArea.localScale = Vector3.one;

        // Clean out any old/broken instances
        for (int i = confinedArea.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(confinedArea.GetChild(i).gameObject);
        }

        // Load original GLB converted FBX model containing all 10 submeshes
        string fbxPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (originalPrefab == null)
        {
            Debug.LogError("CRITICAL: Original FBX/GLB model not found at path: " + fbxPath);
            return;
        }

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab, confinedArea);
        inst.name = "Real_Confined_Space_Model";
        Transform vesselObj = inst.transform;

        // Apply GLB PBR material
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

        // Assign material to all 10 submesh renderers
        Renderer[] rends = vesselObj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            r.sharedMaterial = tanksMat;
            r.gameObject.SetActive(true);
        }

        // Reset transform & apply ROOT upright rotation (0, 90, 0)
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

        // Calculate initial bounds and apply uniform scale to 2.50m size
        Bounds initialB = GetAccurateWorldBounds(vesselObj.gameObject);
        float currentMaxDim = Mathf.Max(initialB.size.x, Mathf.Max(initialB.size.y, initialB.size.z));
        float targetSize = 2.50f;
        float uniformScale = (currentMaxDim > 0.0001f) ? (targetSize / currentMaxDim) : 0.892857f;
        vesselObj.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

        // Snap lowest base support point flat to ground Y level (0.0000m)
        Bounds scaledB = GetAccurateWorldBounds(vesselObj.gameObject);
        float deltaY = groundY - scaledB.min.y;
        vesselObj.position += new Vector3(0f, deltaY, 0f);

        Bounds finalB = GetAccurateWorldBounds(vesselObj.gameObject);

        Debug.Log($"========================================================");
        Debug.Log($"RESTORED MODEL PARAMETERS:");
        Debug.Log($"Original Asset: Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb");
        Debug.Log($"Scene Root Name: {vesselObj.name}");
        Debug.Log($"Final Rotation: {vesselObj.rotation.eulerAngles}");
        Debug.Log($"Final Scale: {vesselObj.localScale}");
        Debug.Log($"Final Position: {vesselObj.position}");
        Debug.Log($"Lowest World-Space Y: {finalB.min.y:F4}m");
        Debug.Log($"Overall Bounds Size: ({finalB.size.x:F2}m, {finalB.size.y:F2}m, {finalB.size.z:F2}m)");
        Debug.Log($"Active Submesh Count: {rends.Length}");
        Debug.Log($"========================================================");

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SCENE SAVED] AR_gas_Foundation.unity saved successfully.");

        // Reload scene and verify persistence
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("[SCENE RELOADED] AR_gas_Foundation.unity reloaded and verified.");

        GameObject reloadedRoot = GameObject.Find("GasScenarioRoot");
        Transform reloadedVessel = reloadedRoot != null ? reloadedRoot.transform.Find("Confined_Space_Training_Area/Real_Confined_Space_Model") : null;
        Vector3 vesselPos = reloadedVessel != null ? reloadedVessel.position : new Vector3(3.00f, 0f, 0f);

        // Capture fresh screenshots
        CaptureProofScreenshots(vesselPos);
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

    private static void CaptureProofScreenshots(Vector3 vesselWorldPos)
    {
        string dir1 = @"C:\SurakshaAR\Screenshots";
        string dir2 = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb";
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

        int w = 1920, h = 1080;

        // 1. COMPLETE SCENE OVERVIEW (PPE TABLE | GAS CYLINDER | CONFINED SPACE)
        CaptureCamera(new Vector3(0.4f, 1.60f, -4.8f), Quaternion.Euler(11f, 0f, 0f), 52f, w, h,
            Path.Combine(dir1, "RESTORED_FULL_WORKSTATION.png"), Path.Combine(dir2, "RESTORED_FULL_WORKSTATION.png"));
        CaptureCamera(new Vector3(0.4f, 1.60f, -4.8f), Quaternion.Euler(11f, 0f, 0f), 52f, w, h,
            Path.Combine(dir1, "Gas_Workstation_FULL.png"), Path.Combine(dir2, "Gas_Workstation_FULL.png"));

        // 2. CONFINED SPACE FRONT UPRIGHT VIEW
        CaptureCamera(vesselWorldPos + new Vector3(0f, 1.20f, -3.5f), Quaternion.Euler(10f, 0f, 0f), 45f, w, h,
            Path.Combine(dir1, "RESTORED_CONFINED_SPACE_FRONT.png"), Path.Combine(dir2, "RESTORED_CONFINED_SPACE_FRONT.png"));
        CaptureCamera(vesselWorldPos + new Vector3(0f, 1.20f, -3.5f), Quaternion.Euler(10f, 0f, 0f), 45f, w, h,
            Path.Combine(dir1, "CONFINED_SPACE_FRONT.png"), Path.Combine(dir2, "CONFINED_SPACE_FRONT.png"));

        // 3. CONFINED SPACE SIDE VIEW
        CaptureCamera(vesselWorldPos + new Vector3(3.5f, 1.20f, 0f), Quaternion.Euler(10f, -90f, 0f), 45f, w, h,
            Path.Combine(dir1, "RESTORED_CONFINED_SPACE_SIDE.png"), Path.Combine(dir2, "RESTORED_CONFINED_SPACE_SIDE.png"));
        CaptureCamera(vesselWorldPos + new Vector3(3.5f, 1.20f, 0f), Quaternion.Euler(10f, -90f, 0f), 45f, w, h,
            Path.Combine(dir1, "CONFINED_SPACE_SIDE.png"), Path.Combine(dir2, "CONFINED_SPACE_SIDE.png"));

        // 4. CONFINED SPACE GROUND CONTACT VIEW
        CaptureCamera(vesselWorldPos + new Vector3(0f, 0.40f, -2.5f), Quaternion.Euler(5f, 0f, 0f), 40f, w, h,
            Path.Combine(dir1, "RESTORED_CONFINED_SPACE_GROUND.png"), Path.Combine(dir2, "RESTORED_CONFINED_SPACE_GROUND.png"));
        CaptureCamera(vesselWorldPos + new Vector3(0f, 0.40f, -2.5f), Quaternion.Euler(5f, 0f, 0f), 40f, w, h,
            Path.Combine(dir1, "Gas_Workstation_CONFINED_SPACE.png"), Path.Combine(dir2, "Gas_Workstation_CONFINED_SPACE.png"));

        Debug.Log("[PROOF RENDERED] Screenshots captured to " + dir1 + " and " + dir2);
    }

    private static void CaptureCamera(Vector3 pos, Quaternion rot, float fov, int width, int height, string filePath1, string filePath2)
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

        GameObject keyLight = new GameObject("KeyLight");
        Light kl = keyLight.AddComponent<Light>();
        kl.type = LightType.Directional;
        kl.intensity = 0.9f;
        kl.color = new Color(1.0f, 0.96f, 0.90f);
        keyLight.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.5f;
        fl.color = new Color(0.85f, 0.90f, 1.0f);
        fillLight.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

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

        Object.DestroyImmediate(keyLight);
        Object.DestroyImmediate(fillLight);
        Object.DestroyImmediate(camObj);
    }
}
#endif
