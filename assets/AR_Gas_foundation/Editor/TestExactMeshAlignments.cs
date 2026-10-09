using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class TestExactMeshAlignments
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Test Exact Mesh Alignments")]
    public static void Test()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== MESH ALIGNMENT AUDIT ===");

        TestMesh("isolation_valve_hd.obj", "Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj", sb);
        TestMesh("pipe_elbow_hd.obj", "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_hd.obj", sb);
        TestMesh("gas_regulator_hd.obj", "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj", sb);
        TestMesh("pressure_gauge_hd.obj", "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj", sb);

        string logPath = Path.Combine(ArtifactDir, "mesh_alignments_audit.txt");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log("Mesh alignments audit written to " + logPath);
    }

    private static void TestMesh(string label, string assetPath, StringBuilder sb)
    {
        sb.AppendLine($"\n--- {label} ({assetPath}) ---");
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            sb.AppendLine("  NOT FOUND!");
            return;
        }

        GameObject inst = UnityEngine.Object.Instantiate(prefab);
        MeshFilter[] mfs = inst.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in mfs)
        {
            if (mf.sharedMesh == null) continue;
            Mesh m = mf.sharedMesh;
            sb.AppendLine($"  Child '{mf.gameObject.name}' Pos: {mf.transform.localPosition} Rot: {mf.transform.localEulerAngles} Scale: {mf.transform.localScale}");
            sb.AppendLine($"    Mesh Bounds Center: {m.bounds.center} Extents: {m.bounds.extents} Min: {m.bounds.min} Max: {m.bounds.max}");

            // Measure vertex extents in world space under 0 rotation
            Vector3[] verts = m.vertices;
            if (verts.Length > 0)
            {
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                foreach (var v in verts)
                {
                    Vector3 wv = mf.transform.TransformPoint(v);
                    min = Vector3.Min(min, wv);
                    max = Vector3.Max(max, wv);
                }
                sb.AppendLine($"    World Vert Bounds (scale 1, rot 0): Min={min}, Max={max}, Size={max - min}, Center={(min + max) / 2f}");
            }
        }
        UnityEngine.Object.DestroyImmediate(inst);
    }
}
