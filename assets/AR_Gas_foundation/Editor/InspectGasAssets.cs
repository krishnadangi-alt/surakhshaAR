#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class InspectGasAssets
{
    [MenuItem("SurakshaAR/Inspect Detector & Cylinder Submeshes")]
    public static void InspectAssets()
    {
        GameObject glbDetector = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb");
        if (glbDetector != null)
        {
            Debug.Log("=== DETECTOR SUBMESH HIERARCHY ===");
            DumpChildren(glbDetector.transform, 0);
        }

        GameObject glbCylinder = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb");
        if (glbCylinder != null)
        {
            Debug.Log("=== CYLINDER SUBMESH HIERARCHY ===");
            DumpChildren(glbCylinder.transform, 0);
        }

        GameObject fbxValve = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/industrial-valve/source/VALVE.fbx");
        if (fbxValve != null)
        {
            Debug.Log("=== VALVE.FBX SUBMESH HIERARCHY ===");
            DumpChildren(fbxValve.transform, 0);
        }
    }

    private static void DumpChildren(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        Renderer r = t.GetComponent<Renderer>();
        string rInfo = "";
        if (r != null)
        {
            rInfo = $" [Renderer: {r.GetType().Name}, Materials: {r.sharedMaterials.Length}, Bounds: {r.bounds.size:F4}]";
            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                var mat = r.sharedMaterials[i];
                rInfo += $"\n{indent}   Mat[{i}]: {(mat != null ? mat.name : "null")}";
            }
        }
        Debug.Log($"{indent}- {t.name}{rInfo}");
        for (int i = 0; i < t.childCount; i++)
        {
            DumpChildren(t.GetChild(i), depth + 1);
        }
    }
}
#endif
