#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Text;

public class GasAssetDiagnosticScript
{
    private static StringBuilder sb = new StringBuilder();

    [MenuItem("SurakshaAR/Run Diagnostic")]
    public static void RunDiagnostic()
    {
        sb.Clear();
        Log("==================================================");
        Log("[GAS ASSET & SCENE DIAGNOSTIC START]");
        Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Log("\n--- SCENE ROOT OBJECTS ---");
        GameObject[] rootObjs = scene.GetRootGameObjects();
        foreach (var obj in rootObjs)
        {
            Log($"Root Object: '{obj.name}' active={obj.activeSelf} childCount={obj.transform.childCount}");
            PrintHierarchy(obj.transform, "  ");
        }

        Log("\n--- PREFAB ASSET DIAGNOSTICS ---");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow.obj");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj");
        InspectPrefab("Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/confined_space_tank_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/ventilation_fan_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PPE/workbench.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_helmet_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_goggles_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PPE/Prefabs/work_gloves_real.prefab");
        InspectPrefab("Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_boots_real.prefab");

        Log("==================================================");
        Log("[GAS ASSET & SCENE DIAGNOSTIC END]");
        Log("==================================================");

        File.WriteAllText("C:/SurakhshaAR/diag_output.txt", sb.ToString());
        Debug.Log("[DIAGNOSTIC COMPLETE] Wrote output to C:/SurakhshaAR/diag_output.txt");
    }

    private static void Log(string msg)
    {
        sb.AppendLine(msg);
        Debug.Log(msg);
    }

    private static void PrintHierarchy(Transform t, string indent)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            Renderer r = child.GetComponent<Renderer>();
            string rInfo = r != null ? $" [Rend: mesh={GetMeshName(child)}, localBounds={r.bounds.size.ToString("F3")}]" : "";
            Log($"{indent}- '{child.name}' active={child.gameObject.activeSelf}{rInfo}");
            if (child.childCount > 0)
            {
                PrintHierarchy(child, indent + "  ");
            }
        }
    }

    private static void InspectPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Log($"\n[PREFAB MISSING] Path: {path}");
            return;
        }

        Log($"\n>>> PREFAB: '{prefab.name}' ({path})");
        GameObject instance = Object.Instantiate(prefab);
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        Log($"  Total Renderers: {renderers.Length}");
        foreach (var r in renderers)
        {
            MeshFilter mf = r.GetComponent<MeshFilter>();
            string mName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "N/A";
            string matNames = "";
            foreach (var m in r.sharedMaterials)
            {
                matNames += (m != null ? m.name : "null") + ", ";
            }
            Log($"   Renderer: '{r.gameObject.name}' Mesh='{mName}' Active={r.gameObject.activeSelf} Enabled={r.enabled} LocalPos={r.transform.localPosition.ToString("F4")} LocalScale={r.transform.localScale.ToString("F4")} WorldBoundsSize={r.bounds.size.ToString("F4")} Materials=[{matNames}]");
        }
        Object.DestroyImmediate(instance);
    }

    private static string GetMeshName(Transform t)
    {
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null) return mf.sharedMesh.name;
        SkinnedMeshRenderer smr = t.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null) return smr.sharedMesh.name;
        return "N/A";
    }
}
#endif
