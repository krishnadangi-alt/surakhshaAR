#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Text;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryVsFireInspector
    {
        private const string FireScenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
        private const string MachineryScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputLog = "Builds/Verification/UnityScene/fire_vs_machinery_comparison.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect Fire vs Machinery Setup")]
        public static void CompareSetups()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== FIRE VS MACHINERY SCENE SETUP COMPARISON ===");

            // Inspect Fire Scene
            var fireScene = EditorSceneManager.OpenScene(FireScenePath, OpenSceneMode.Single);
            if (fireScene.IsValid())
            {
                sb.AppendLine($"\nFIRE SCENE: {FireScenePath}");
                GameObject[] fireRoots = fireScene.GetRootGameObjects();
                foreach (var r in fireRoots)
                {
                    sb.AppendLine($"Root: '{r.name}', Active={r.activeSelf}, Layer={LayerMask.LayerToName(r.layer)}, Pos={r.transform.position}, Scale={r.transform.localScale}");
                    MeshRenderer[] renderers = r.GetComponentsInChildren<MeshRenderer>(true);
                    sb.AppendLine($"  Total Renderers: {renderers.Length}");
                    if (renderers.Length > 0)
                    {
                        Bounds b = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                        sb.AppendLine($"  Combined Bounds Center: {b.center}, Size: {b.size}");
                    }
                }
            }

            // Inspect Machinery Scene
            var machScene = EditorSceneManager.OpenScene(MachineryScenePath, OpenSceneMode.Single);
            if (machScene.IsValid())
            {
                sb.AppendLine($"\nMACHINERY SCENE: {MachineryScenePath}");
                GameObject[] machRoots = machScene.GetRootGameObjects();
                foreach (var r in machRoots)
                {
                    sb.AppendLine($"Root: '{r.name}', Active={r.activeSelf}, Layer={LayerMask.LayerToName(r.layer)}, Pos={r.transform.position}, Scale={r.transform.localScale}");
                    MeshRenderer[] renderers = r.GetComponentsInChildren<MeshRenderer>(true);
                    sb.AppendLine($"  Total Renderers: {renderers.Length}");
                    if (renderers.Length > 0)
                    {
                        Bounds b = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                        sb.AppendLine($"  Combined Bounds Center: {b.center}, Size: {b.size}");
                    }
                }
            }

            Directory.CreateDirectory("Builds/Verification/UnityScene");
            File.WriteAllText(OutputLog, sb.ToString());
            Debug.Log($"[MachineryVsFireInspector] Comparison written to {OutputLog}");
        }
    }
}
#endif
