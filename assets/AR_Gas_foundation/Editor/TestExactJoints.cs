#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class TestExactJoints
{
    [MenuItem("SurakhshaAR/Test Exact Joints")]
    public static void Test()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== EXACT JOINTS AUDIT ===");

        // 1. Pressure Gauge Rotations
        string gaugePath = "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_converted.fbx";
        GameObject gaugePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gaugePath);
        GameObject gInst = (GameObject)PrefabUtility.InstantiatePrefab(gaugePrefab);
        gInst.transform.localScale = new Vector3(0.0488f, 0.0488f, 0.0488f);

        Vector3[] gRots = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(180, 0, 0),
            new Vector3(0, 180, 0),
            new Vector3(90, 0, 0),
            new Vector3(-90, 0, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, -90, 0)
        };

        foreach (var r in gRots)
        {
            gInst.transform.rotation = Quaternion.Euler(r);
            Renderer[] rends = gInst.GetComponentsInChildren<Renderer>(true);
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            sb.AppendLine($"Gauge Rot {r}: Bounds Min={b.min.ToString("F3")}, Max={b.max.ToString("F3")}, Center={b.center.ToString("F3")}");
        }
        Object.DestroyImmediate(gInst);

        // 2. Valve Rotations & Flange Positions
        string valvePath = "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_valve_converted.fbx";
        GameObject valvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(valvePath);
        GameObject vInst = (GameObject)PrefabUtility.InstantiatePrefab(valvePrefab);
        vInst.transform.localScale = new Vector3(0.00160f, 0.00160f, 0.00160f);

        Vector3[] vRots = new Vector3[]
        {
            new Vector3(0, 90, 0),
            new Vector3(0, -90, 0),
            new Vector3(180, 90, 0),
            new Vector3(0, 90, 180)
        };

        foreach (var r in vRots)
        {
            vInst.transform.rotation = Quaternion.Euler(r);
            Renderer[] rends = vInst.GetComponentsInChildren<Renderer>(true);
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            sb.AppendLine($"Valve Rot {r}: Bounds Min={b.min.ToString("F3")}, Max={b.max.ToString("F3")}, Center={b.center.ToString("F3")}");
        }
        Object.DestroyImmediate(vInst);

        File.WriteAllText(@"C:\SurakhshaAR\ExactJointsAudit.txt", sb.ToString());
    }
}
#endif
