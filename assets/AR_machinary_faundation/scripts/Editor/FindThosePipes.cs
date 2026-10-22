#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class FindThosePipes
    {
        static FindThosePipes()
        {
            EditorApplication.delayCall += Run;
        }

        [MenuItem("SurakshaAR/Find Those Pipes")]
        public static void Run()
        {
            using (StreamWriter sw = new StreamWriter("C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda/scratch/all_active_renderers.txt"))
            {
                sw.WriteLine("=== ALL ACTIVE RENDERERS IN SCENE ===");
                Renderer[] allR = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (Renderer r in allR)
                {
                    string path = r.name;
                    Transform p = r.transform.parent;
                    while (p != null)
                    {
                        path = p.name + " / " + path;
                        p = p.parent;
                    }

                    string matNames = "";
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m != null) matNames += m.name + " (" + (m.mainTexture != null ? m.mainTexture.name : "no_tex") + "), ";
                    }

                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "no_mesh";

                    sw.WriteLine($"Obj: {path}");
                    sw.WriteLine($"  Bounds: center={r.bounds.center}, size={r.bounds.size}");
                    sw.WriteLine($"  Mesh: {meshName} | Mats: {matNames}");
                    sw.WriteLine();
                }
            }
            Debug.Log("[FindThosePipes] Done dumping to all_active_renderers.txt");
        }
    }
}
#endif
