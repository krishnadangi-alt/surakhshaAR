#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Text;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneInspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputLog = "Builds/Verification/UnityScene/diagnostic_report.txt";

        [MenuItem("SurakshaAR/Machinery/Run Scene Diagnostic")]
        public static void RunDiagnostic()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== MACHINERY SCENE DIAGNOSTIC REPORT ===");
            sb.AppendLine($"Scene: {scene.path}");

            GameObject root = GameObject.Find("MachineryScenario");
            if (root == null)
            {
                sb.AppendLine("ERROR: MachineryScenario root not found!");
                File.WriteAllText(OutputLog, sb.ToString());
                return;
            }

            string[] targetObjects = new string[]
            {
                "ConveyorAssembly",
                "DriveMotor",
                "JamArea",
                "FeedHopper",
                "EmergencyStop",
                "MainIsolator",
                "ControlPanel",
                "SteelWorkstationTable",
                "Hasp",
                "PadlockAndTag"
            };

            foreach (string name in targetObjects)
            {
                GameObject go = GameObject.Find(name);
                if (go == null)
                {
                    // Check under LOTOAssembly
                    Transform t = root.transform.Find($"LOTOAssembly/{name}");
                    if (t != null) go = t.gameObject;
                }
                if (go == null)
                {
                    // Check under root
                    Transform t = root.transform.Find(name);
                    if (t != null) go = t.gameObject;
                }

                sb.AppendLine($"\n--- Object: {name} ---");
                if (go == null)
                {
                    sb.AppendLine("  STATUS: MISSING!");
                    continue;
                }

                sb.AppendLine($"  Path: {GetGameObjectPath(go)}");
                sb.AppendLine($"  Local Position: {go.transform.localPosition}");
                sb.AppendLine($"  Local Rotation: {go.transform.localEulerAngles}");
                sb.AppendLine($"  Local Scale: {go.transform.localScale}");
                sb.AppendLine($"  World Position: {go.transform.position}");
                sb.AppendLine($"  World Lossy Scale: {go.transform.lossyScale}");

                MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
                sb.AppendLine($"  MeshRenderers Count: {renderers.Length}");

                if (renderers.Length > 0)
                {
                    Bounds combinedBounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }
                    sb.AppendLine($"  World Bounds Center: {combinedBounds.center}");
                    sb.AppendLine($"  World Bounds Size (WxHxD): {combinedBounds.size}");
                    sb.AppendLine($"  World Bounds Min (Lowest Y={combinedBounds.min.y}): {combinedBounds.min}");
                    sb.AppendLine($"  World Bounds Max (Highest Y={combinedBounds.max.y}): {combinedBounds.max}");

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        MeshFilter mf = renderers[i].GetComponent<MeshFilter>();
                        string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "N/A";
                        sb.AppendLine($"    Renderer [{i}]: {renderers[i].name} (Mesh: {meshName}), LocalBounds Size: {(mf != null && mf.sharedMesh != null ? mf.sharedMesh.bounds.size.ToString() : "N/A")}, Enabled={renderers[i].enabled}");
                    }
                }
                else
                {
                    sb.AppendLine("  NO MESH RENDERERS FOUND!");
                }
            }

            Directory.CreateDirectory("Builds/Verification/UnityScene");
            File.WriteAllText(OutputLog, sb.ToString());
            Debug.Log($"[MachinerySceneInspector] Diagnostic written to {OutputLog}");
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    }
}
#endif
