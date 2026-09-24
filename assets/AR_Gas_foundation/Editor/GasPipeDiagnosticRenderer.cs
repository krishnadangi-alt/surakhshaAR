#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class GasPipeDiagnosticRenderer
{
    [MenuItem("SurakshaAR/Diagnose Gas Pipe Assets")]
    public static void DiagnoseAssets()
    {
        string outputDir = Path.Combine(Application.dataPath, "AR_Gas_foundation/Validation");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        string[] assetPaths = new string[]
        {
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb",
            "Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_animated.glb",
            "Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/source/pipes.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow.obj",
            "Assets/industrial-valve/source/VALVE.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/ball_valve.glb"
        };

        foreach (string path in assetPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[PIPE_DIAGNOSTIC] Could not load {path}");
                continue;
            }

            GameObject stage = new GameObject("DiagStage");
            GameObject inst = Object.Instantiate(prefab, stage.transform, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;

            Renderer[] renderers = inst.GetComponentsInChildren<Renderer>();
            Bounds combined = new Bounds();
            bool hasBounds = false;

            foreach (var r in renderers)
            {
                if (!hasBounds) { combined = r.bounds; hasBounds = true; }
                else combined.Encapsulate(r.bounds);
                Debug.Log($"[PIPE_DIAGNOSTIC] {prefab.name} -> Child {r.name}: Bounds Center={r.bounds.center}, Size={r.bounds.size}, Min={r.bounds.min}, Max={r.bounds.max}");
            }

            Debug.Log($"[PIPE_DIAGNOSTIC_SUMMARY] {prefab.name}: Total Renderers={renderers.Length}, Combined Size={combined.size}, Combined Center={combined.center}");

            Object.DestroyImmediate(stage);
        }
    }
}
#endif
