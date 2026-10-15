#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFullMeshAndShaderInspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/full_mesh_and_shader_report.txt";

        [MenuItem("SurakshaAR/Machinery/Full Mesh And Shader Inspector")]
        public static void InspectFull()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — FULL MESH, SHADER, LAYER & RENDERER INSPECTION REPORT");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("CRITICAL ERROR: Cannot open scene at " + ScenePath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                sb.AppendLine("CRITICAL ERROR: 'MachineryScenario' GameObject NOT FOUND in scene!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            sb.AppendLine($"MachineryScenario Root: ActiveSelf={masterRoot.activeSelf}, ActiveHierarchy={masterRoot.activeInHierarchy}, Pos={masterRoot.transform.position}, LossyScale={masterRoot.transform.lossyScale}, Layer={masterRoot.layer} ({LayerMask.LayerToName(masterRoot.layer)})\n");

            Transform[] allTransforms = masterRoot.GetComponentsInChildren<Transform>(true);
            sb.AppendLine($"Total Transform Nodes in Hierarchy: {allTransforms.Length}\n");

            int activeGameObjects = 0;
            int meshFilterCount = 0;
            int validMeshCount = 0;
            int meshRendererCount = 0;
            int enabledRendererCount = 0;

            foreach (var t in allTransforms)
            {
                GameObject go = t.gameObject;
                if (go.activeInHierarchy) activeGameObjects++;

                MeshFilter mf = go.GetComponent<MeshFilter>();
                MeshRenderer mr = go.GetComponent<MeshRenderer>();

                if (mf != null)
                {
                    meshFilterCount++;
                    if (mf.sharedMesh != null) validMeshCount++;
                }

                if (mr != null)
                {
                    meshRendererCount++;
                    if (mr.enabled && go.activeInHierarchy) enabledRendererCount++;
                }

                if (mf != null || mr != null)
                {
                    sb.AppendLine($"-------------------------------------------------------------------------");
                    sb.AppendLine($"Node: '{GetPath(t, masterRoot.transform)}'");
                    sb.AppendLine($"  ActiveSelf={go.activeSelf}, ActiveHierarchy={go.activeInHierarchy}, Layer={go.layer} ({LayerMask.LayerToName(go.layer)})");
                    sb.AppendLine($"  LocalPos={t.localPosition}, WorldPos={t.position}");
                    sb.AppendLine($"  LocalScale={t.localScale}, LossyScale={t.lossyScale}");

                    if (mf != null)
                    {
                        if (mf.sharedMesh != null)
                        {
                            sb.AppendLine($"  MeshFilter: MeshName='{mf.sharedMesh.name}', Vertices={mf.sharedMesh.vertexCount}, SubMeshes={mf.sharedMesh.subMeshCount}, LocalBounds={mf.sharedMesh.bounds.center} size={mf.sharedMesh.bounds.size}");
                        }
                        else
                        {
                            sb.AppendLine($"  MeshFilter: NULL / MISSING MESH!");
                        }
                    }

                    if (mr != null)
                    {
                        sb.AppendLine($"  MeshRenderer: Enabled={mr.enabled}, WorldBounds={mr.bounds.center} size={mr.bounds.size}");
                        Material[] mats = mr.sharedMaterials;
                        sb.AppendLine($"  Materials Count: {mats.Length}");
                        for (int m = 0; m < mats.Length; m++)
                        {
                            Material mat = mats[m];
                            if (mat != null)
                            {
                                string shaderName = mat.shader != null ? mat.shader.name : "NULL SHADER";
                                Color col = mat.HasProperty("_Color") ? mat.color : (mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.black);
                                sb.AppendLine($"    Mat[{m}]: Name='{mat.name}', Shader='{shaderName}', Color={col}");
                            }
                            else
                            {
                                sb.AppendLine($"    Mat[{m}]: NULL MATERIAL!");
                            }
                        }
                    }
                }
            }

            sb.AppendLine("\n=========================================================================");
            sb.AppendLine("SUMMARY:");
            sb.AppendLine($"Active GameObjects in Hierarchy: {activeGameObjects}");
            sb.AppendLine($"Total MeshFilters: {meshFilterCount} (Valid Mesh Assigned: {validMeshCount})");
            sb.AppendLine($"Total MeshRenderers: {meshRendererCount} (Enabled & Active: {enabledRendererCount})");
            sb.AppendLine("=========================================================================");

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[FullMeshShaderInspector] Audit report written to " + LogPath);
        }

        private static string GetPath(Transform target, Transform root)
        {
            if (target == root) return target.name;
            string path = target.name;
            Transform curr = target.parent;
            while (curr != null && curr != root)
            {
                path = curr.name + "/" + path;
                curr = curr.parent;
            }
            return path;
        }
    }
}
#endif
