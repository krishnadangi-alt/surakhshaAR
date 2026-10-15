#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryInspectHopperSubmeshes
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/hopper_submesh_analysis.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect Hopper Submeshes")]
        public static void Inspect()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — FEED HOPPER SUBMESH ANALYSIS");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            Transform hopperTr = masterRoot.transform.Find("FeedHopper");

            sb.AppendLine($"FeedHopper Transform: Pos={hopperTr.position}, LocalPos={hopperTr.localPosition}, Scale={hopperTr.localScale}");

            MeshRenderer[] renderers = hopperTr.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"Total MeshRenderers in FeedHopper: {renderers.Length}");

            int validCount = 0;
            int giantCount = 0;

            foreach (var r in renderers)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                Mesh m = mf ? mf.sharedMesh : null;

                float sizeMag = r.bounds.size.magnitude;
                if (sizeMag > 5.0f || r.transform.localPosition.magnitude > 10.0f)
                {
                    giantCount++;
                    sb.AppendLine($"  [GIANT/OFFSET] '{r.name}' | Parent='{r.transform.parent.name}' | LocalPos={r.transform.localPosition} | BoundsCenter={r.bounds.center} | BoundsSize={r.bounds.size} | SizeMag={sizeMag:F2}");
                }
                else
                {
                    validCount++;
                    sb.AppendLine($"  [VALID PHYSICAL] '{r.name}' | Parent='{r.transform.parent.name}' | LocalPos={r.transform.localPosition} | BoundsCenter={r.bounds.center} | BoundsSize={r.bounds.size} | SizeMag={sizeMag:F2}");
                }
            }

            sb.AppendLine($"\nSummary: Valid Physical Submeshes={validCount}, Giant/Offset Submeshes={giantCount}");
            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[InspectHopper] Log written to " + LogPath);
        }
    }
}
#endif
