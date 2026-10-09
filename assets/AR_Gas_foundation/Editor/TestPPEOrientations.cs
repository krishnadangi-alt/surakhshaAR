using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class TestPPEOrientations
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject station = GameObject.Find("PPE_Preparation_Station");

        string[] names = new[] { "Safety_Boots_Left", "Safety_Helmet", "Safety_Goggles", "Work_Gloves_Left", "Safety_Vest" };

        foreach (var name in names)
        {
            Transform t = station.transform.Find(name);
            if (t == null) continue;
            MeshFilter mf = t.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            Vector3[] verts = mf.sharedMesh.vertices;
            Vector3 min = verts[0], max = verts[0];
            foreach (var v in verts)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
            Debug.Log($"[PPE MESH] {name}: raw mesh bounds min={min.ToString("F3")}, max={max.ToString("F3")}, size={(max - min).ToString("F3")}");
            Debug.Log($"[PPE MESH] {name}: parent chain rotations:");
            Transform cur = mf.transform;
            while (cur != t.parent && cur != null)
            {
                Debug.Log($"   -> '{cur.name}' localRot={cur.localRotation.eulerAngles.ToString("F1")}");
                cur = cur.parent;
            }
        }
    }
}
