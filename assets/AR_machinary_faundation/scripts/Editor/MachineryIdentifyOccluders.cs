#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryIdentifyOccluders
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/occluder_analysis.txt";

        [MenuItem("SurakshaAR/Machinery/Identify Occluding Renderers")]
        public static void IdentifyOccluders()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — IDENTIFY OCCLUDING MESH RENDERERS");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("ERROR: Failed to open scene");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            Vector3 camPos = new Vector3(1.2f, 4.5f, -7.5f);
            MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            sb.AppendLine($"Total MeshRenderers in scene: {allRenderers.Length}");

            int occluderCount = 0;
            foreach (var r in allRenderers)
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;

                Bounds b = r.bounds;
                float distToCam = Vector3.Distance(b.center, camPos);
                bool containsCam = b.Contains(camPos);
                float mag = b.size.magnitude;

                if (containsCam || mag > 5.0f || distToCam < 2.0f)
                {
                    string path = r.name;
                    Transform p = r.transform.parent;
                    while (p != null)
                    {
                        path = p.name + "/" + path;
                        p = p.parent;
                    }

                    sb.AppendLine($"[OCCLUDER FOUND] Path='{path}' | BoundsCenter={b.center} | BoundsSize={b.size} | SizeMag={mag:F2} | DistToCam={distToCam:F2} | ContainsCam={containsCam}");
                    occluderCount++;
                }
            }

            sb.AppendLine($"\nTotal Occluder Renderers Found: {occluderCount}");
            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[MachineryIdentifyOccluders] Done. Report written to " + LogPath);
        }
    }
}
#endif
