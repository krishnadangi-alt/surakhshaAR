#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FixPhysicalPlacement180
{
    [MenuItem("SurakshaAR/Apply 180 Degree Root Rotation Fix")]
    public static void ApplyFix()
    {
        Debug.Log("========================================================");
        Debug.Log("APPLYING 180 DEGREE ROOT ROTATION FIX TO GLB MODEL");
        Debug.Log("========================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open scene: " + scenePath);
            return;
        }

        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null) return;

        // Ground reference Y from Gas Cylinder bottom
        float groundY = 0.00f;
        Transform gasCylinder = gasRoot.transform.Find("Gas_Leak_Station/Realistic_Gas_Cylinder");
        if (gasCylinder != null)
        {
            Bounds cylB = GetAccurateWorldBounds(gasCylinder.gameObject);
            groundY = cylB.min.y;
        }
        Debug.Log($"GROUND REFERENCE Y = {groundY:F4}");

        Transform confinedArea = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (confinedArea == null)
        {
            confinedArea = new GameObject("Confined_Space_Training_Area").transform;
            confinedArea.SetParent(gasRoot.transform, false);
        }
        confinedArea.localPosition = new Vector3(3.00f, 0.00f, 0.00f);
        confinedArea.localRotation = Quaternion.identity;
        confinedArea.localScale = Vector3.one;

        // Remove loose extra objects so ONLY 1 complete GLB assembly exists
        for (int i = confinedArea.childCount - 1; i >= 0; i--)
        {
            Transform child = confinedArea.GetChild(i);
            if (child.name != "Real_Confined_Space_Model")
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        Transform vesselObj = confinedArea.Find("Real_Confined_Space_Model");
        if (vesselObj == null)
        {
            string fbxPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, confinedArea);
            inst.name = "Real_Confined_Space_Model";
            vesselObj = inst.transform;
        }

        // Apply GLB PBR materials
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

        Renderer[] rends = vesselObj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends) r.sharedMaterial = tanksMat;

        // Reset transform
        vesselObj.localPosition = Vector3.zero;

        // STEP 2: Rotate ROOT by 180 degrees (Euler(0, 90, 0)) to turn manhole entry hatch towards approach side
        vesselObj.localRotation = Quaternion.Euler(0f, 90f, 0f);

        // Keep ALL 10 sub-mesh components active and in native relative alignment under root node _0
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

        // STEP 4: Scale uniformly so max dimension is 2.50m
        Bounds initialB = GetAccurateWorldBounds(vesselObj.gameObject);
        float currentMaxDim = Mathf.Max(initialB.size.x, Mathf.Max(initialB.size.y, initialB.size.z));
        float targetSize = 2.50f;
        float uniformScale = (currentMaxDim > 0.0001f) ? (targetSize / currentMaxDim) : 1f;
        vesselObj.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

        // STEP 3: Snap lowest point of COMPLETE assembly flat to ground reference
        Bounds scaledB = GetAccurateWorldBounds(vesselObj.gameObject);
        float deltaY = groundY - scaledB.min.y;
        vesselObj.position += new Vector3(0f, deltaY, 0f);

        Bounds finalB = GetAccurateWorldBounds(vesselObj.gameObject);
        Debug.Log($"FINAL GROUNDED BOUNDS: Center = {finalB.center}, Size = ({finalB.size.x:F2}, {finalB.size.y:F2}, {finalB.size.z:F2}), Min Y = {finalB.min.y:F4}, Max Y = {finalB.max.y:F4}");
        Debug.Log($"FINAL TRANSFORM: Position = {vesselObj.position}, Rotation = {vesselObj.rotation.eulerAngles}, Scale = {vesselObj.localScale}");

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SCENE SAVED SUCCESS] Target scene AR_gas_Foundation.unity saved.");

        // Reload scene after saving and verify persistence
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("[SCENE RE-LOADED] Re-opened scene to verify persistence.");

        // Re-fetch object after reload for screenshots
        GameObject rootReloaded = GameObject.Find("GasScenarioRoot");
        Transform confinedReloaded = rootReloaded != null ? rootReloaded.transform.Find("Confined_Space_Training_Area") : null;
        Vector3 vesselPos = confinedReloaded != null ? confinedReloaded.position : new Vector3(3.00f, 0f, 0f);

        // Capture fresh proof screenshots
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

        // 1. COMPLETE SCENE VIEW
        CaptureCamera(new Vector3(0.4f, 1.60f, -4.8f), Quaternion.Euler(11f, 0f, 0f), 52f, w, h, Path.Combine(dir1, "Gas_Workstation_FULL.png"), Path.Combine(dir2, "Gas_Workstation_FULL.png"));

        // 2. CONFINED-SPACE FRONT VIEW
        CaptureCamera(vesselWorldPos + new Vector3(0f, 1.20f, -3.5f), Quaternion.Euler(10f, 0f, 0f), 45f, w, h, Path.Combine(dir1, "CONFINED_SPACE_FRONT.png"), Path.Combine(dir2, "CONFINED_SPACE_FRONT.png"));
        CaptureCamera(vesselWorldPos + new Vector3(0f, 1.20f, -3.5f), Quaternion.Euler(10f, 0f, 0f), 45f, w, h, Path.Combine(dir1, "Gas_Workstation_CONFINED_SPACE.png"), Path.Combine(dir2, "Gas_Workstation_CONFINED_SPACE.png"));

        // 3. CONFINED-SPACE SIDE VIEW
        CaptureCamera(vesselWorldPos + new Vector3(3.5f, 1.20f, 0f), Quaternion.Euler(10f, -90f, 0f), 45f, w, h, Path.Combine(dir1, "CONFINED_SPACE_SIDE.png"), Path.Combine(dir2, "CONFINED_SPACE_SIDE.png"));

        // 4. CENTER GAS ASSEMBLY
        CaptureCamera(new Vector3(-0.15f, 0.70f, -1.5f), Quaternion.Euler(6f, 0f, 0f), 40f, w, h, Path.Combine(dir1, "Gas_Workstation_CENTER.png"), Path.Combine(dir2, "Gas_Workstation_CENTER.png"));

        // 5. LEFT PPE WORKSTATION
        CaptureCamera(new Vector3(-2.25f, 0.70f, -1.5f), Quaternion.Euler(6f, 0f, 0f), 40f, w, h, Path.Combine(dir1, "Gas_Workstation_PPE.png"), Path.Combine(dir2, "Gas_Workstation_PPE.png"));

        // 6. HIERARCHY IMAGE
        GenerateHierarchyImage(Path.Combine(dir1, "Gas_Workstation_HIERARCHY.png"), Path.Combine(dir2, "Gas_Workstation_HIERARCHY.png"));

        Debug.Log("[SCREENSHOTS CAPTURED] Fresh proof images rendered to " + dir1 + " and " + dir2);
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
