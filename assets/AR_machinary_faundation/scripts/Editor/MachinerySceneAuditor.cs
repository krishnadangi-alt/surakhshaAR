using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneAuditor
    {
        [MenuItem("SurakshaAR/Audit Scene State")]
        public static void AuditScene()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== MACHINERY CONVEYOR JAM SCENE AUDIT ===");
            sb.AppendLine($"Scene: {scene.path}");
            sb.AppendLine($"Root GameObject Count: {scene.rootCount}");
            sb.AppendLine();

            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                DumpGameObject(root, sb, 0);
            }

            string logPath = Path.Combine(Application.dataPath, "../Builds/Verification/UnityScene/scene_audit.txt");
            string dir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(logPath, sb.ToString());
            Debug.Log($"Scene Audit Written to {logPath}");
        }

        private static void DumpGameObject(GameObject go, StringBuilder sb, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            Transform t = go.transform;
            Renderer rend = go.GetComponent<Renderer>();
            string boundsInfo = "";
            if (rend != null)
            {
                Bounds b = rend.bounds;
                boundsInfo = $" | Bounds Center: {b.center.ToString("F2")}, Size: {b.size.ToString("F2")}";
            }

            sb.AppendLine($"{indent}- [{go.name}] Active: {go.activeSelf} | Pos: {t.localPosition.ToString("F3")} | Rot: {t.localEulerAngles.ToString("F1")} | Scale: {t.localScale.ToString("F3")}{boundsInfo}");

            for (int i = 0; i < t.childCount; i++)
            {
                DumpGameObject(t.GetChild(i).gameObject, sb, indentLevel + 1);
            }
        }
    }
}
