#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasWorkstationRealAssetSetup
{
    [MenuItem("SurakshaAR/Perform Real Asset Setup And Capture")]
    public static void PerformSetup()
    {
        Debug.Log("==================================================");
        Debug.Log("[START] Gas Workstation Real Asset Setup & Prefab Creation");
        Debug.Log("==================================================");

        string ppePrefabsDir = "Assets/AR_Gas_foundation/3d/PPE/Prefabs";
        string confinedPrefabsDir = "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs";

        if (!Directory.Exists(ppePrefabsDir)) Directory.CreateDirectory(ppePrefabsDir);
        if (!Directory.Exists(confinedPrefabsDir)) Directory.CreateDirectory(confinedPrefabsDir);

        AssetDatabase.Refresh();

        // 1. Create Prefabs from converted FBX assets
        CreatePrefabFromFbx("Assets/AR_Gas_foundation/3d/PPE/safety_goggles_converted.fbx", Path.Combine(ppePrefabsDir, "safety_goggles_real.prefab"));
        CreatePrefabFromFbx("Assets/AR_Gas_foundation/3d/PPE/work_gloves_converted.fbx", Path.Combine(ppePrefabsDir, "work_gloves_real.prefab"));
        CreatePrefabFromFbx("Assets/AR_Gas_foundation/3d/PPE/work_boot_converted.fbx", Path.Combine(ppePrefabsDir, "safety_boots_real.prefab"));
        CreatePrefabFromFbx("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx", Path.Combine(confinedPrefabsDir, "ventilation_fan_real.prefab"));
        CreatePrefabFromFbx("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx", Path.Combine(confinedPrefabsDir, "retracting_lifeline_real.prefab"));

        AssetDatabase.Refresh();

        // 2. Open Gas Scene
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
        }

        // Find or create station containers
        Transform industrialEnv = gasRoot.transform.Find("IndustrialEnvironment");
        if (industrialEnv == null)
        {
            GameObject envObj = new GameObject("IndustrialEnvironment");
            envObj.transform.SetParent(gasRoot.transform, false);
            industrialEnv = envObj.transform;
        }

        Transform ppeStation = industrialEnv.Find("PPE_Preparation_Station");
        if (ppeStation == null)
        {
            GameObject ppeObj = new GameObject("PPE_Preparation_Station");
            ppeObj.transform.SetParent(industrialEnv, false);
            ppeStation = ppeObj.transform;
            ppeStation.localPosition = new Vector3(-2.2f, 0f, 0f);
        }

        Transform confinedStation = industrialEnv.Find("Confined_Space_Training_Area");
        if (confinedStation == null)
        {
            GameObject confObj = new GameObject("Confined_Space_Training_Area");
            confObj.transform.SetParent(industrialEnv, false);
            confinedStation = confObj.transform;
            confinedStation.localPosition = new Vector3(2.5f, 0f, 0f);
        }

        // Remove any substitute/placeholder/duplicate objects under PPE or ConfinedSpace
        RemoveSubstitutes(ppeStation);
        RemoveSubstitutes(confinedStation);

        // 3. Ensure Scene Instances of Prefabs are placed properly
        PlaceOrUpdatePrefabInScene(Path.Combine(ppePrefabsDir, "safety_goggles_real.prefab"), ppeStation, "Safety_Goggles_Real", new Vector3(0.1f, 0.88f, 0.05f), new Vector3(0f, 45f, 0f), new Vector3(0.012f, 0.012f, 0.012f));
        PlaceOrUpdatePrefabInScene(Path.Combine(ppePrefabsDir, "work_gloves_real.prefab"), ppeStation, "Work_Gloves_Real", new Vector3(-0.35f, 0.87f, 0.05f), new Vector3(0f, 15f, 0f), new Vector3(0.015f, 0.015f, 0.015f));
        PlaceOrUpdatePrefabInScene(Path.Combine(ppePrefabsDir, "safety_boots_real.prefab"), ppeStation, "Safety_Boots_Real", new Vector3(-0.45f, 0.02f, 0.35f), new Vector3(0f, 75f, 0f), new Vector3(0.015f, 0.015f, 0.015f));
        
        PlaceOrUpdatePrefabInScene(Path.Combine(confinedPrefabsDir, "ventilation_fan_real.prefab"), confinedStation, "Axial_Ventilation_Fan_Real", new Vector3(-0.85f, 0.02f, -0.4f), new Vector3(0f, -30f, 0f), new Vector3(0.015f, 0.015f, 0.015f));
        PlaceOrUpdatePrefabInScene(Path.Combine(confinedPrefabsDir, "retracting_lifeline_real.prefab"), confinedStation, "Self_Retracting_Lifeline_Real", new Vector3(0.65f, 1.45f, 0.2f), new Vector3(0f, 0f, 0f), new Vector3(0.015f, 0.015f, 0.015f));

        // Save modified scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[SUCCESS] All 5 real GLB-derived prefabs successfully created and instantiated in AR_gas_Foundation.unity");

        // 4. Capture proof screenshots
        GasWorkstationScreenshotCapturer.CaptureScreenshots();
        Debug.Log("[FINISHED] Setup & Screenshot capture complete.");
        Debug.Log("==================================================");
    }

    private static void CreatePrefabFromFbx(string fbxPath, string prefabPath)
    {
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError($"[SETUP ERROR] FBX asset not found at {fbxPath}");
            return;
        }

        GameObject tempObj = Object.Instantiate(fbxAsset);
        tempObj.name = Path.GetFileNameWithoutExtension(prefabPath);

        PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
        Object.DestroyImmediate(tempObj);
        Debug.Log($"[PREFAB CREATED] -> {prefabPath}");
    }

    private static void PlaceOrUpdatePrefabInScene(string prefabPath, Transform parent, string instanceName, Vector3 localPos, Vector3 localRot, Vector3 localScale)
    {
        // Check if object already exists
        Transform existing = parent.Find(instanceName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabAsset == null)
        {
            Debug.LogError($"[SETUP ERROR] Cannot instantiate missing prefab at {prefabPath}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, parent);
        instance.name = instanceName;
        instance.transform.localPosition = localPos;
        instance.transform.localRotation = Quaternion.Euler(localRot);
        instance.transform.localScale = localScale;
        Debug.Log($"[INSTANTIATED IN SCENE] {instanceName} under {parent.name} at {localPos}");
    }

    private static void RemoveSubstitutes(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            string name = child.name.ToLower();
            // Remove any old placeholder/substitute objects that are not the original approved HD obj or new real prefabs
            if (name.Contains("substitute") || name.Contains("placeholder") || name.Contains("fake") || name.Contains("primitive") || name.Contains("defaultasset"))
            {
                Debug.Log($"[REMOVED SUBSTITUTE] {child.name} from {parent.name}");
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
#endif
