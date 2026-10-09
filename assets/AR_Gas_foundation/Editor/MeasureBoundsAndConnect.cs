using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MeasureBoundsAndConnect
{
    [MenuItem("Tools/Measure Bounds And Connect")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene("Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity", OpenSceneMode.Single);

        string[] targets = new string[]
        {
            "Realistic_Gas_Cylinder",
            "Real_Gas_Regulator",
            "high-pressure_regulator",
            "Real_Pressure_Gauge",
            "pressure_gauge",
            "Horizontal_Pipe_1",
            "Isolation_Valve_3D",
            "Horizontal_Pipe_2",
            "Pipe_Elbow_3D",
            "Downstream_Pipe_3D",
            "Confined_Space_Vessel_Tank",
            "Self_Retracting_Lifeline",
            "Lifeline",
            "Axial_Ventilation_Fan_Real",
            "Multi_Gas_Detector"
        };

        Debug.Log("=== MESH BOUNDS MEASUREMENTS ===");
        foreach (var name in targets)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                var mfs = go.GetComponentsInChildren<MeshFilter>();
                foreach (var mf in mfs)
                {
                    if (mf.sharedMesh != null)
                    {
                        var b = mf.sharedMesh.bounds;
                        Vector3 worldSize = Vector3.Scale(b.size, mf.transform.lossyScale);
                        Debug.Log($"[{go.name} -> {mf.name}] LocalBoundsSize:{b.size.ToString("F4")} LossyScale:{mf.transform.lossyScale.ToString("F4")} WorldSize:{worldSize.ToString("F4")} Pos:{mf.transform.position.ToString("F3")}");
                    }
                }
            }
        }
    }
}
