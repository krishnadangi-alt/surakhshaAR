#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class InspectSceneRoots
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Inspect Scene Roots")]
        public static void InspectRoots()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory("scratch");
            using (StreamWriter sw = new StreamWriter("scratch/scene_roots.txt", false))
            {
                sw.WriteLine($"=== SCENE ROOTS IN {ScenePath} ===");
                GameObject[] roots = scene.GetRootGameObjects();
                foreach (var r in roots)
                {
                    sw.WriteLine($"Root GameObject: '{r.name}', Active={r.activeSelf}, ChildCount={r.transform.childCount}");
                    foreach (Transform c in r.transform)
                    {
                        sw.WriteLine($"   Child: '{c.name}', Active={c.gameObject.activeSelf}");
                    }
                }
            }
            Debug.Log("[InspectSceneRoots] Saved to scratch/scene_roots.txt");
        }
    }
}
#endif
