using UnityEditor;
using UnityEngine;

public class MeshBoundsInspector
{
    public static void InspectAllBounds()
    {
        string[] paths = new string[] {
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb",
            "Assets/AR_Gas_foundation/3d/GasDetector/h2s_gas_detector.glb",
            "Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab"
        };

        foreach (var p in paths)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (obj != null)
            {
                GameObject temp = Object.Instantiate(obj);
                temp.transform.position = Vector3.zero;
                temp.transform.rotation = Quaternion.identity;
                temp.transform.localScale = Vector3.one;

                Renderer[] rends = temp.GetComponentsInChildren<Renderer>(true);
                if (rends.Length > 0)
                {
                    Bounds b = rends[0].bounds;
                    for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                    Debug.Log($"[MESH BOUNDS] Asset: {p} | Size: {b.size} | Min: {b.min} | Max: {b.max} | Center: {b.center}");
                }
                else
                {
                    Debug.LogWarning($"[MESH BOUNDS] Asset has NO renderers: {p}");
                }
                Object.DestroyImmediate(temp);
            }
            else
            {
                Debug.LogError($"[MESH BOUNDS] Could not load asset at path: {p}");
            }
        }
    }
}
