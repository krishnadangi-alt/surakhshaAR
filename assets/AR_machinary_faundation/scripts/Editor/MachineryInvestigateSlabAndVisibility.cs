#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryInvestigateSlabAndVisibility
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/fresh_visibility_check.txt";

        [MenuItem("SurakshaAR/Machinery/Investigate Fresh Visibility")]
        public static void Investigate()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — FRESH SLAB AND SCENE VISIBILITY DIAGNOSTIC");
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
                sb.AppendLine("CRITICAL: 'MachineryScenario' root NOT FOUND in scene!");
            }
            else
            {
                sb.AppendLine($"'MachineryScenario' Details: ActiveSelf={masterRoot.activeSelf}, Pos={masterRoot.transform.position}, Scale={masterRoot.transform.localScale}");
                sb.AppendLine("CHILDREN OF MachineryScenario:");
                foreach (Transform child in masterRoot.transform)
                {
                    MeshRenderer[] childR = child.GetComponentsInChildren<MeshRenderer>(true);
                    int activeR = 0;
                    foreach (var r in childR) if (r.enabled && r.gameObject.activeInHierarchy) activeR++;

                    Bounds b = new Bounds();
                    bool hasB = false;
                    foreach (var r in childR)
                    {
                        if (r.enabled && r.gameObject.activeInHierarchy)
                        {
                            if (!hasB) { b = r.bounds; hasB = true; }
                            else { b.Encapsulate(r.bounds); }
                        }
                    }

                    sb.AppendLine($"  Child: '{child.name}' | ActiveSelf={child.gameObject.activeSelf} | Pos={child.position} | TotalRenderers={childR.Length} | EnabledInHierarchy={activeR} | BoundsCenter={(hasB ? b.center.ToString() : "N/A")} | BoundsSize={(hasB ? b.size.ToString() : "N/A")}");
                }
            }

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[InvestigateFresh] Report written to " + LogPath);
        }
    }
}
#endif
