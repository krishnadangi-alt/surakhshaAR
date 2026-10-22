#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryInspectGLTFNodeTransforms
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/gltf_node_transforms_report.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect GLTF Node Transforms")]
        public static void Inspect()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — GLTF NODE TRANSFORMS & MESH OFFSET INSPECTOR");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("ERROR: Cannot open scene at " + ScenePath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                sb.AppendLine("ERROR: MachineryScenario missing!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            foreach (Transform child in masterRoot.transform)
            {
                sb.AppendLine($"=========================================================================");
                sb.AppendLine($"MODEL NODE: '{child.name}' | LocalPos={child.localPosition} | WorldPos={child.position} | Scale={child.localScale}");

                MeshRenderer[] renderers = child.GetComponentsInChildren<MeshRenderer>(true);
                sb.AppendLine($"Total Sub-Mesh Renderers: {renderers.Length}");

                foreach (var r in renderers)
                {
                    if (!r.enabled) continue;
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    Mesh mesh = mf ? mf.sharedMesh : null;

                    sb.AppendLine($"  Sub-Mesh: '{r.name}' | ActiveHierarchy={r.gameObject.activeInHierarchy} | RendererEnabled={r.enabled}");
                    sb.AppendLine($"    LocalPos={r.transform.localPosition} | WorldPos={r.transform.position}");
                    sb.AppendLine($"    WorldBoundsCenter={r.bounds.center} | WorldBoundsSize={r.bounds.size}");
                    if (mesh != null)
                    {
                        sb.AppendLine($"    MeshLocalBoundsCenter={mesh.bounds.center} | MeshLocalBoundsSize={mesh.bounds.size}");
                    }
                }
                sb.AppendLine();
            }

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[InspectGLTF] Log written to " + LogPath);
        }
    }
}
#endif
