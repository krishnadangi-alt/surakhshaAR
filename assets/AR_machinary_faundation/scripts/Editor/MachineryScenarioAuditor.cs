using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryScenarioAuditor
    {
        [MenuItem("SurakshaAR/Audit Scenario Summary")]
        public static void AuditSummary()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== MACHINERY SCENARIO DIRECT CHILDREN SUMMARY ===");

            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null)
            {
                sb.AppendLine("ERROR: MachineryScenario not found!");
            }
            else
            {
                sb.AppendLine($"MachineryScenario Transform: Pos={scenario.transform.position.ToString("F3")}, Rot={scenario.transform.eulerAngles.ToString("F1")}, Scale={scenario.transform.localScale.ToString("F3")}");
                sb.AppendLine($"Child Count: {scenario.transform.childCount}");
                sb.AppendLine();

                for (int i = 0; i < scenario.transform.childCount; i++)
                {
                    Transform child = scenario.transform.GetChild(i);
                    Renderer rend = child.GetComponentInChildren<Renderer>();
                    string boundsStr = "No Renderer";
                    if (rend != null)
                    {
                        Bounds b = rend.bounds;
                        boundsStr = $"Bounds Center={b.center.ToString("F2")}, Size={b.size.ToString("F2")}";
                    }

                    sb.AppendLine($"[{i+1}] '{child.name}' | Active={child.gameObject.activeSelf} | LocalPos={child.localPosition.ToString("F3")} | LocalRot={child.localEulerAngles.ToString("F1")} | LocalScale={child.localScale.ToString("F3")} | Children={child.childCount} | {boundsStr}");
                }
            }

            string logPath = Path.Combine(Application.dataPath, "../Builds/Verification/UnityScene/scenario_summary.txt");
            string dir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(logPath, sb.ToString());
            Debug.Log($"Scenario Summary Written to {logPath}");
        }
    }
}
