#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FixConfinedSpaceOBJ
{
    [MenuItem("SurakshaAR/Fix Confined Space OBJ")]
    public static void ExecuteFix()
    {
        Debug.Log("==========================================================================");
        Debug.Log("STARTING CONFINED SPACE OBJ SCENE FIX");
        Debug.Log("==========================================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"CRITICAL: Could not open scene at path {scenePath}");
            return;
        }

        // 1. Locate Root objects & Reference Objects (Gas Cylinder and PPE Workstation)
        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
            Undo.RegisterCreatedObjectUndo(gasRoot, "Create GasScenarioRoot");
        }

        // Find Red Gas Cylinder in scene as spatial reference
        GameObject cylinderObj = GameObject.Find("gas_cylinder");
        if (cylinderObj == null) cylinderObj = GameObject.Find("gas_cylinder_real");
        if (cylinderObj == null) cylinderObj = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderObj == null)
        {
            Transform cylT = gasRoot.transform.Find("Gas_Leak_Station/Realistic_Gas_Cylinder");
            if (cylT != null) cylinderObj = cylT.gameObject;
        }

        Vector3 cylPos = Vector3.zero;
        Bounds cylBounds = new Bounds();
        float groundY = 0f;

        if (cylinderObj != null)
        {
            cylBounds = CalculateWorldBounds(cylinderObj);
            cylPos = cylinderObj.transform.position;
            groundY = cylBounds.min.y;
            Debug.Log($"[CYLINDER REF] Found cylinder '{cylinderObj.name}' at pos {cylPos}, bounds size {cylBounds.size}, ground Y = {groundY:F4}");
        }
        else
        {
            Debug.LogWarning("[CYLINDER REF] Red gas cylinder object not found by direct search. Searching all objects...");
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.ToLower().Contains("cylinder"))
                {
                    cylinderObj = go;
                    cylBounds = CalculateWorldBounds(cylinderObj);
                    cylPos = cylinderObj.transform.position;
                    groundY = cylBounds.min.y;
                    Debug.Log($"[CYLINDER REF] Found matching cylinder '{go.name}' at pos {cylPos}, ground Y = {groundY:F4}");
                    break;
                }
            }
        }

        // Find PPE Workstation in scene as left reference
        GameObject ppeObj = GameObject.Find("workbench_hd");
        if (ppeObj == null) ppeObj = GameObject.Find("PPE_Workstation");
        if (ppeObj == null) ppeObj = GameObject.Find("PPE_Station");
        Vector3 ppePos = ppeObj != null ? ppeObj.transform.position : new Vector3(-2.0f, 0f, 0f);
        Debug.Log($"[PPE REF] Workbench/PPE Pos = {ppePos}");

        // 2. DISABLE / REMOVE ALL BROKEN / DUPLICATE CONFINED SPACE OBJECTS IN SCENE
        Debug.Log("--- SEARCHING & REMOVING EXISTING CONFINED SPACE INSTANCES ---");
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            string lowerName = go.name.ToLower();
            if ((lowerName.Contains("confined") || lowerName.Contains("vessel") || lowerName.Contains("industrial_tanks"))
                && go.transform.parent != null
                && go.transform.parent.name == "Confined_Space_Training_Area")
            {
                Debug.Log($"[REMOVE] Destroying old instance in Confined_Space_Training_Area: {go.name}");
                Object.DestroyImmediate(go);
            }
            else if ((lowerName.Equals("real_confined_space_model") || lowerName.Equals("confined_space") || lowerName.Equals("confinedspace") || lowerName.Equals("industrial_tanks_and_equipment"))
                && go.transform.root == go.transform)
            {
                Debug.Log($"[DISABLE] Disabling root stray instance: {go.name}");
                go.SetActive(false);
            }
        }

        // Clean Confined_Space_Training_Area parent object
        Transform confinedArea = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (confinedArea == null)
        {
            GameObject caObj = new GameObject("Confined_Space_Training_Area");
            caObj.transform.SetParent(gasRoot.transform, false);
            confinedArea = caObj.transform;
        }

        for (int i = confinedArea.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(confinedArea.GetChild(i).gameObject);
        }

        confinedArea.localPosition = Vector3.zero;
        confinedArea.localRotation = Quaternion.identity;
        confinedArea.localScale = Vector3.one;

        // 3. INSTANTIATE THE ORIGINAL DOWNLOADED CONFINED SPACE OBJ
        string objAssetPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj";
        GameObject originalOBJAsset = AssetDatabase.LoadAssetAtPath<GameObject>(objAssetPath);
        if (originalOBJAsset == null)
        {
            Debug.LogError($"CRITICAL ERROR: Could not load OBJ asset at {objAssetPath}");
            return;
        }

        Debug.Log($"[OBJ LOADED] Loaded original OBJ asset: {originalOBJAsset.name} from {objAssetPath}");

        // Instantiate OBJ directly under Confined_Space_Training_Area
        GameObject sceneInstance = (GameObject)PrefabUtility.InstantiatePrefab(originalOBJAsset, confinedArea);
        sceneInstance.name = "Real_Confined_Space_OBJ";

        // 4. CHECK & RECONNECT MATERIALS IF NEEDED
        Material matTank = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Vessel_Tank.mat");
        Material matSign = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Safety_Sign.mat");
        Material matValve = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Valve_Red.mat");

        Renderer[] renderers = sceneInstance.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[RENDERERS] Found {renderers.Length} renderer(s) in instantiated OBJ.");

        foreach (var r in renderers)
        {
            r.gameObject.SetActive(true);
            Material[] mats = r.sharedMaterials;
            bool needsFix = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || mats[i].shader == null || mats[i].name.Contains("InternalError") || mats[i].name.Contains("Default"))
                {
                    needsFix = true;
                    break;
                }
            }

            if (needsFix || mats.Length == 0)
            {
                Debug.Log($"[MATERIAL FIX] Assigning original materials to renderer: {r.name}");
                if (mats.Length == 3 && matTank != null && matSign != null && matValve != null)
                {
                    r.sharedMaterials = new Material[] { matTank, matSign, matValve };
                }
                else if (matTank != null)
                {
                    Material[] newMats = new Material[Mathf.Max(1, mats.Length)];
                    for (int i = 0; i < newMats.Length; i++) newMats[i] = matTank;
                    r.sharedMaterials = newMats;
                }
            }
        }

        // 5. DETERMINE UPRIGHT ROTATION, UNIFORM SCALE, AND POSITIONING
        sceneInstance.transform.localPosition = Vector3.zero;
        sceneInstance.transform.localRotation = Quaternion.identity;
        sceneInstance.transform.localScale = Vector3.one;

        // Inspect unscaled bounds at different rotations
        Vector3 rot0 = EvaluateBoundsForRotation(sceneInstance, new Vector3(0f, 0f, 0f));
        Vector3 rotX90 = EvaluateBoundsForRotation(sceneInstance, new Vector3(-90f, 0f, 0f));
        Vector3 rotX90Pos = EvaluateBoundsForRotation(sceneInstance, new Vector3(90f, 0f, 0f));

        Debug.Log($"[BOUNDS TEST] Euler(0,0,0) -> size ({rot0.x:F3}, {rot0.y:F3}, {rot0.z:F3})");
        Debug.Log($"[BOUNDS TEST] Euler(-90,0,0) -> size ({rotX90.x:F3}, {rotX90.y:F3}, {rotX90.z:F3})");
        Debug.Log($"[BOUNDS TEST] Euler(90,0,0) -> size ({rotX90Pos.x:F3}, {rotX90Pos.y:F3}, {rotX90Pos.z:F3})");

        // Pick rotation that maximizes vertical height (Y) while maintaining upright geometry
        Vector3 chosenEuler = new Vector3(-90f, 0f, 0f);
        if (rot0.y > rotX90.y && rot0.y > rotX90Pos.y)
        {
            chosenEuler = Vector3.zero;
        }

        sceneInstance.transform.localEulerAngles = chosenEuler;
        Bounds rotatedBounds = CalculateWorldBounds(sceneInstance);
        Debug.Log($"[CHOSEN ROTATION] Euler = {chosenEuler}, Rotated Bounds Size = ({rotatedBounds.size.x:F3}, {rotatedBounds.size.y:F3}, {rotatedBounds.size.z:F3})");

        // STEP 5 — UNIFORM SCALE: CLEARLY BIGGER THAN RED GAS CYLINDER
        // Target height ~2.40m (larger than red cylinder ~1.1m)
        float currentHeight = rotatedBounds.size.y;
        float targetHeight = 2.50f;
        float uniformScale = (currentHeight > 0.001f) ? (targetHeight / currentHeight) : 1.0f;

        sceneInstance.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
        Debug.Log($"[UNIFORM SCALE] Scale = {uniformScale:F4} (Target Height = {targetHeight:F2}m)");

        // STEP 6 — GROUND LEVEL POSITIONING
        Bounds scaledBounds = CalculateWorldBounds(sceneInstance);
        float currentLowestY = scaledBounds.min.y;
        float yAdjustment = groundY - currentLowestY;

        // STEP 7 — POSITION TO THE RIGHT OF RED GAS CYLINDER
        float targetX = cylPos.x + 2.80f;
        float targetZ = cylPos.z;
        float targetY = sceneInstance.transform.position.y + yAdjustment;

        sceneInstance.transform.position = new Vector3(targetX, targetY, targetZ);

        // Recalculate final world bounds
        Bounds finalBounds = CalculateWorldBounds(sceneInstance);
        float finalLowestY = finalBounds.min.y;

        Vector3 finalPos = sceneInstance.transform.position;
        Vector3 finalRot = sceneInstance.transform.eulerAngles;
        Vector3 finalScale = sceneInstance.transform.localScale;
        string instanceName = sceneInstance.name;
        bool isVisible = sceneInstance.activeInHierarchy;

        Debug.Log("==========================================================================");
        Debug.Log("CONFINED SPACE FIX APPLIED SUCCESSFULLY:");
        Debug.Log($"OBJ PATH: {objAssetPath}");
        Debug.Log($"SCENE INSTANCE: {instanceName}");
        Debug.Log($"ROOT ROTATION: {finalRot}");
        Debug.Log($"UNIFORM SCALE: {finalScale}");
        Debug.Log($"ROOT POSITION: {finalPos}");
        Debug.Log($"LOWEST WORLD Y: {finalLowestY:F4}m");
        Debug.Log($"BOUNDS SIZE: ({finalBounds.size.x:F2}m, {finalBounds.size.y:F2}m, {finalBounds.size.z:F2}m)");
        Debug.Log($"VISIBLE IN SCENE: {(isVisible ? "YES" : "NO")}");
        Debug.Log("==========================================================================");

        // 6. SAVE SCENE & RELOAD TO VERIFY PERSISTENCE
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        Debug.Log($"[SAVE SCENE] Scene save result: {saved}");

        // Reload scene
        var reloadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject reloadedRoot = GameObject.Find("GasScenarioRoot");
        Transform reloadedOBJ = reloadedRoot != null ? reloadedRoot.transform.Find("Confined_Space_Training_Area/Real_Confined_Space_OBJ") : null;

        if (reloadedOBJ != null && reloadedOBJ.gameObject.activeInHierarchy)
        {
            Debug.Log("[VERIFICATION] Reloaded scene successfully! Instance is present and active.");
            // 7. CAPTURE PROOF SCREENSHOTS using reloaded transform
            CaptureProofScreenshots(reloadedOBJ.position, cylPos, ppePos);
        }
        else
        {
            Debug.LogError("[VERIFICATION FAILED] Could not find active Real_Confined_Space_OBJ after scene reload!");
        }
    }

    private static Vector3 EvaluateBoundsForRotation(GameObject obj, Vector3 euler)
    {
        obj.transform.localEulerAngles = euler;
        Bounds b = CalculateWorldBounds(obj);
        return b.size;
    }

    private static Bounds CalculateWorldBounds(GameObject obj)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
        {
            b.Encapsulate(rends[i].bounds);
        }
        return b;
    }

    private static void CaptureProofScreenshots(Vector3 vesselPos, Vector3 cylPos, Vector3 ppePos)
    {
        string dir1 = @"C:\SurakshaAR\Screenshots";
        string dir2 = @"C:\Users\anjan\.gemini\antigravity\brain\c728ff39-ec61-4697-b390-b7629a5ba242";
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

        int w = 1920, h = 1080;

        // Camera positioning for Full Workstation Overview:
        // PPE Workstation (Left) | Red Gas Cylinder (Center) | Confined Space (Right)
        Vector3 midPoint = new Vector3((ppePos.x + vesselPos.x) * 0.5f, (ppePos.y + vesselPos.y) * 0.5f, (ppePos.z + vesselPos.z) * 0.5f);
        Vector3 overviewCamPos = midPoint + new Vector3(0f, 1.8f, -4.5f);
        Quaternion overviewCamRot = Quaternion.Euler(12f, 0f, 0f);

        CaptureCamera(overviewCamPos, overviewCamRot, 55f, w, h,
            Path.Combine(dir1, "CONFINED_SPACE_WORKSTATION_OVERVIEW.png"),
            Path.Combine(dir2, "CONFINED_SPACE_WORKSTATION_OVERVIEW.png"));

        // Close-up shot of Confined Space front
        Vector3 frontCamPos = vesselPos + new Vector3(0f, 1.3f, -3.2f);
        Quaternion frontCamRot = Quaternion.Euler(10f, 0f, 0f);

        CaptureCamera(frontCamPos, frontCamRot, 45f, w, h,
            Path.Combine(dir1, "CONFINED_SPACE_FRONT_UPRIGHT.png"),
            Path.Combine(dir2, "CONFINED_SPACE_FRONT_UPRIGHT.png"));

        Debug.Log($"[SCREENSHOTS GENERATED] Proof screenshots saved to {dir1} and {dir2}");
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
        kl.intensity = 1.0f;
        kl.color = new Color(1.0f, 0.96f, 0.90f);
        keyLight.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.6f;
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
