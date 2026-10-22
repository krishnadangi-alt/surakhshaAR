#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class InspectConveyorStructure
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/conveyor_structure_diag.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect Conveyor Structure")]
        public static void DiagConveyor()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== CONVEYOR ASSEMBLY DIAGNOSTICS ===");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                sb.AppendLine("ERROR: MachineryScenario root missing!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            Transform conveyorTr = masterRoot.transform.Find("ConveyorAssembly");
            if (conveyorTr == null)
            {
                sb.AppendLine("ERROR: ConveyorAssembly missing!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            MeshRenderer[] renderers = conveyorTr.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"Total MeshRenderers in ConveyorAssembly: {renderers.Length}");

            Bounds combinedBounds = new Bounds();
            bool hasFirst = false;

            foreach (var r in renderers)
            {
                sb.AppendLine($"Renderer: {r.gameObject.name}, enabled={r.enabled}, activeInHierarchy={r.gameObject.activeInHierarchy}");
                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Bounds localB = mf.sharedMesh.bounds;
                    sb.AppendLine($"  - Mesh: {mf.sharedMesh.name}, localBounds center={localB.center}, size={localB.size}");
                    
                    // Transform local bounds to world bounds manually
                    Bounds worldB = r.bounds;
                    sb.AppendLine($"  - Renderer worldBounds center={worldB.center}, size={worldB.size}, min={worldB.min}, max={worldB.max}");

                    if (worldB.size.magnitude > 0.001f)
                    {
                        if (!hasFirst) { combinedBounds = worldB; hasFirst = true; }
                        else { combinedBounds.Encapsulate(worldB); }
                    }
                }
            }

            sb.AppendLine($"\nCOMBINED WORLD BOUNDS: Center={combinedBounds.center}, Size={combinedBounds.size}, Min={combinedBounds.min}, Max={combinedBounds.max}");
            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[DiagConveyor] Written to " + LogPath);
        }
    }
}
#endif
