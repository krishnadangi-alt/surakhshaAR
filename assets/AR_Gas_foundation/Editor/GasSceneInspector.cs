#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasSceneInspector
{
    [MenuItem("SurakshaAR/Inspect Gas Scene Objects")]
    public static void InspectGasScene()
    {
        string logPath = "C:/SurakhshaAR/gas_scene_inspection.txt";
        using (StreamWriter writer = new StreamWriter(logPath, false))
        {
            writer.WriteLine("==================================================");
            writer.WriteLine("SURAKSHAAR GAS SCENE INSPECTION REPORT");
            writer.WriteLine("==================================================");

            string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
            if (!File.Exists(scenePath))
            {
                writer.WriteLine($"[SCENE NOT FOUND] {scenePath}");
                return;
            }

            AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceUpdate);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            writer.WriteLine($"Active Scene: {scene.name}");
            
            GameObject root = GameObject.Find("GasScenarioRoot");
            if (root != null)
            {
                writer.WriteLine($"\n[GAS SCENARIO ROOT FOUND] Children count: {root.transform.childCount}");
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Transform child = root.transform.GetChild(i);
                    writer.WriteLine($"\n  Child [{i}]: '{child.name}', Active={child.gameObject.activeSelf}, LocalPos={child.localPosition.ToString("F3")}, LocalRot={child.localEulerAngles.ToString("F3")}, LocalScale={child.localScale.ToString("F3")}");
                    DumpSubtree(child, writer, "     ");
                }

                writer.WriteLine("\n[RENDERERS & MESHES UNDER ROOT]");
                Renderer[] rnds = root.GetComponentsInChildren<Renderer>(true);
                writer.WriteLine($"Total Renderers under root: {rnds.Length}");
                foreach (var r in rnds)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "N/A";
                    string matName = r.sharedMaterial != null ? r.sharedMaterial.name : "NULL";
                    writer.WriteLine($"  GO: '{r.gameObject.name}', Path='{GetPath(r.transform)}', Active={r.gameObject.activeInHierarchy}, Enabled={r.enabled}, Mesh='{meshName}', Mat='{matName}', BoundsCenter={r.bounds.center.ToString("F3")}, BoundsSize={r.bounds.size.ToString("F3")}");
                }
            }
            else
            {
                writer.WriteLine("[ERROR] GasScenarioRoot NOT found!");
            }

            writer.WriteLine("\n==================================================");
            writer.WriteLine("END OF INSPECTION REPORT");
            writer.WriteLine("==================================================");
        }

        Debug.Log("[GasSceneInspector] Inspection finished. Log saved to C:/SurakhshaAR/gas_scene_inspection.txt");
    }

    private static void DumpSubtree(Transform parent, StreamWriter writer, string indent)
    {
        foreach (Transform child in parent)
        {
            writer.WriteLine($"{indent}Child: '{child.name}', Active={child.gameObject.activeSelf}, LocalPos={child.localPosition.ToString("F3")}, LocalScale={child.localScale.ToString("F3")}");
            DumpSubtree(child, writer, indent + "  ");
        }
    }

    private static string GetPath(Transform current)
    {
        if (current.parent == null)
            return current.name;
        return GetPath(current.parent) + "/" + current.name;
    }
}
#endif
