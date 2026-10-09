using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectPPEStation
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string LogPath = @"C:\SurakhshaAR\ppe_inspection_log.txt";

    public static void Run()
    {
        using (var sw = new StreamWriter(LogPath, false))
        {
            sw.WriteLine("=== INSPECTION OF PPE PREPARATION STATION ===");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sw.WriteLine("Failed to open scene: " + ScenePath);
                return;
            }

            GameObject station = GameObject.Find("PPE_Preparation_Station");
            if (station == null)
            {
                sw.WriteLine("PPE_Preparation_Station NOT FOUND");
                return;
            }

            sw.WriteLine($"PPE Station World Pos: {station.transform.position:F4}");
            InspectTransform(station.transform, 0, sw);
        }
    }

    private static void InspectTransform(Transform t, int depth, StreamWriter sw)
    {
        string indent = new string(' ', depth * 2);
        Renderer r = t.GetComponent<Renderer>();
        MeshFilter mf = t.GetComponent<MeshFilter>();
        string rInfo = "";
        if (r != null)
        {
            rInfo = $" [Renderer: {r.GetType().Name}, Enabled={r.enabled}, Bounds min={r.bounds.min:F4} max={r.bounds.max:F4}]";
            if (r.sharedMaterials != null)
            {
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var m = r.sharedMaterials[i];
                    string texName = (m != null && m.mainTexture != null) ? m.mainTexture.name : "null";
                    string matName = m != null ? m.name : "null";
                    string shaderName = (m != null && m.shader != null) ? m.shader.name : "null";
                    Color col = m != null ? m.color : Color.clear;
                    rInfo += $"\n{indent}    Mat[{i}]: {matName} (Shader: {shaderName}, Tex: {texName}, Color: {col:F2}, Metallic: {(m != null && m.HasProperty("_Metallic") ? m.GetFloat("_Metallic") : -1):F2}, Smooth: {(m != null && m.HasProperty("_Glossiness") ? m.GetFloat("_Glossiness") : -1):F2})";
                }
            }
        }
        string mfInfo = "";
        if (mf != null && mf.sharedMesh != null)
        {
            mfInfo = $" [Mesh: {mf.sharedMesh.name}, Verts: {mf.sharedMesh.vertexCount}, Submeshes: {mf.sharedMesh.subMeshCount}, Bounds: min={mf.sharedMesh.bounds.min:F4} max={mf.sharedMesh.bounds.max:F4}]";
        }

        sw.WriteLine($"{indent}- {t.name} (Active={t.gameObject.activeSelf}, LocalPos={t.localPosition:F4}, LocalRot={t.localRotation.eulerAngles:F1}, LocalScale={t.localScale:F4}){rInfo}{mfInfo}");

        for (int i = 0; i < t.childCount; i++)
        {
            InspectTransform(t.GetChild(i), depth + 1, sw);
        }
    }
}
