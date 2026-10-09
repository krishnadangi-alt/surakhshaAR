#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class TestElbowMeshEndPoints
{
    [MenuItem("SurakhshaAR/Test Elbow Mesh Endpoints")]
    public static void Test()
    {
        StringBuilder sb = new StringBuilder();
        string elbowPath = "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_converted.fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(elbowPath);
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.position = Vector3.zero;
        inst.transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);

        MeshFilter mf = inst.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Vector3[] verts = mf.sharedMesh.vertices;
            sb.AppendLine($"Total vertices: {verts.Length}");
            
            // Log local min/max of raw mesh vertices
            Vector3 minV = verts[0], maxV = verts[0];
            foreach (var v in verts)
            {
                minV = Vector3.Min(minV, v);
                maxV = Vector3.Max(maxV, v);
            }
            sb.AppendLine($"Raw Mesh Local Min={minV.ToString("F4")}, Max={maxV.ToString("F4")}");

            // Test under Euler(90, 0, 0), Euler(0, 0, 90), Euler(90, 90, 0), etc.
            Vector3[] testEulers = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(90, 0, 0),
                new Vector3(90, 90, 0),
                new Vector3(90, 180, 0),
                new Vector3(90, 270, 0),
                new Vector3(0, 0, 90),
                new Vector3(0, 0, 180),
                new Vector3(0, 0, 270)
            };

            foreach (var euler in testEulers)
            {
                Quaternion q = Quaternion.Euler(euler);
                Vector3 tMin = q * verts[0], tMax = q * verts[0];
                foreach (var v in verts)
                {
                    Vector3 tv = q * (Vector3.Scale(v, new Vector3(0.045f, 0.045f, 0.045f)));
                    tMin = Vector3.Min(tMin, tv);
                    tMax = Vector3.Max(tMax, tv);
                }
                sb.AppendLine($"Euler {euler}: World-scaled Min={tMin.ToString("F4")}, Max={tMax.ToString("F4")}");
            }
        }
        Object.DestroyImmediate(inst);

        File.WriteAllText(@"C:\SurakhshaAR\ElbowEndpoints.txt", sb.ToString());
    }
}
#endif
