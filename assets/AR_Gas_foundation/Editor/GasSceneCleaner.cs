#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasSceneCleaner
{
    [MenuItem("SurakshaAR/Clean and Setup Gas Scene")]
    public static void CleanAndSetupGasScene()
    {
        Debug.Log("==================================================");
        Debug.Log("[GAS SCENE CLEANER] Cleaning Gas Scene Hierarchy");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            root = new GameObject("GasScenarioRoot");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
        }

        // Remove pre-placed children so user doesn't start inside cylinder
        int childCount = root.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = root.transform.GetChild(i);
            Debug.Log($"[GAS SCENE CLEANER] Removing pre-placed scene object: '{child.name}'");
            Object.DestroyImmediate(child.gameObject);
        }

        // Setup GasEnvironmentBuilder
        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder == null) builder = root.AddComponent<GasEnvironmentBuilder>();

        GameObject cylinderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab");
        GameObject detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab");
        GameObject valvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/industrial-valve/source/VALVE.fbx");

        SerializedObject soBuilder = new SerializedObject(builder);
        soBuilder.FindProperty("gasCylinderPrefab").objectReferenceValue = cylinderPrefab;
        soBuilder.FindProperty("gasDetectorPrefab").objectReferenceValue = detectorPrefab;
        soBuilder.FindProperty("pipelineValvePrefab").objectReferenceValue = valvePrefab;
        soBuilder.FindProperty("cylinderScale").floatValue = 0.75f;
        soBuilder.FindProperty("detectorScale").floatValue = 0.35f;
        soBuilder.FindProperty("detectorLocalOffset").vector3Value = new Vector3(0.35f, 0.01f, 0.08f);
        soBuilder.ApplyModifiedProperties();

        // Setup GasARPlacement references
        GasARPlacement placement = Object.FindFirstObjectByType<GasARPlacement>();
        if (placement != null)
        {
            SerializedObject soPlacement = new SerializedObject(placement);
            soPlacement.FindProperty("gasEnvironmentRoot").objectReferenceValue = root.transform;
            soPlacement.FindProperty("environmentBuilder").objectReferenceValue = builder;
            soPlacement.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[GAS SCENE CLEANER] SUCCESS! Scene cleaned and saved cleanly.");
        Debug.Log("==================================================");
    }
}
#endif
