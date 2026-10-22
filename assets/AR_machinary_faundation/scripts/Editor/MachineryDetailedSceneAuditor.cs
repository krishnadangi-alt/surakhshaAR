#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryDetailedSceneAuditor
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/scene_audit_details.txt";

        [MenuItem("SurakshaAR/Machinery/Audit Scene Labels & Duplicates")]
        public static void AuditScene()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — DETAILED SCENE AUDIT (TEXT, LABELS, ARROWS & HOPPERS)");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("ERROR: Scene failed to open!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            sb.AppendLine($"Total GameObjects in Scene: {allObjects.Length}\n");

            sb.AppendLine("--- 1. TEXT / LABELS / CANVAS / ARROWS DETECTED ---");
            int textCount = 0;
            foreach (var go in allObjects)
            {
                string nameLower = go.name.ToLower();
                TextMesh tm = go.GetComponent<TextMesh>();
                TMP_Text tmp = go.GetComponent<TMP_Text>();
                Canvas canvas = go.GetComponent<Canvas>();

                bool isLabelOrText = (tm != null || tmp != null || canvas != null ||
                                     nameLower.Contains("label") || nameLower.Contains("text") ||
                                     nameLower.Contains("arrow") || nameLower.Contains("marker") ||
                                     nameLower.Contains("sign") || nameLower.Contains("title") ||
                                     nameLower.Contains("instruction") || nameLower.Contains("tag_ui"));

                if (isLabelOrText)
                {
                    textCount++;
                    string compTypes = "";
                    if (tm != null) compTypes += $"TextMesh('{tm.text}'); ";
                    if (tmp != null) compTypes += $"TMP_Text('{tmp.text}'); ";
                    if (canvas != null) compTypes += "Canvas; ";
                    sb.AppendLine($"  [{textCount}] '{go.name}' | Active={go.activeSelf} | Parent='{(go.transform.parent != null ? go.transform.parent.name : "ROOT")}' | Pos={go.transform.position} | Components: {compTypes}");
                }
            }

            sb.AppendLine($"\nTotal Text/Label/Arrow Objects Found: {textCount}");

            sb.AppendLine("\n--- 2. HOPPERS DETECTED IN SCENE ---");
            int hopperCount = 0;
            foreach (var go in allObjects)
            {
                string nameLower = go.name.ToLower();
                if (nameLower.Contains("hopper"))
                {
                    hopperCount++;
                    MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
                    sb.AppendLine($"  [{hopperCount}] '{go.name}' | Active={go.activeSelf} | Parent='{(go.transform.parent != null ? go.transform.parent.name : "ROOT")}' | Pos={go.transform.position} | RenderersCount={renderers.Length}");
                }
            }

            sb.AppendLine($"\nTotal Hopper Objects Found: {hopperCount}");

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[SceneAuditor] Audit log written to " + LogPath);
        }
    }
}
#endif
