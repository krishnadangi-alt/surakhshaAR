#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class InspectHierarchyAndMaterials
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/hierarchy_inspection.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect Hierarchy and Renderers")]
        public static void Inspect()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SURAKSHAAR — FULL HIERARCHY & RENDERER INSPECTION");
            sb.AppendLine("Scene: " + ScenePath);
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                sb.AppendLine("ERROR: MachineryScenario root missing!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            // Inspect ConveyorAssembly
            Transform conveyorTr = masterRoot.transform.Find("ConveyorAssembly");
            if (conveyorTr != null)
            {
                sb.AppendLine("--- CONVEYOR ASSEMBLY HIERARCHY ---");
                InspectTransform(conveyorTr, sb, 0);
            }
            else
            {
                sb.AppendLine("ConveyorAssembly NOT FOUND!");
            }

            // Inspect SteelWorkstationTable
            Transform tableTr = masterRoot.transform.Find("SteelWorkstationTable");
            if (tableTr != null)
            {
                sb.AppendLine("\n--- STEEL WORKSTATION TABLE HIERARCHY ---");
                InspectTransform(tableTr, sb, 0);
            }
            else
            {
                sb.AppendLine("SteelWorkstationTable NOT FOUND!");
            }

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[InspectHierarchy] Log written to " + LogPath);
        }

        private static void InspectTransform(Transform tr, StringBuilder sb, int indent)
        {
            string indentStr = new string(' ', indent * 2);
            MeshRenderer mr = tr.GetComponent<MeshRenderer>();
            MeshFilter mf = tr.GetComponent<MeshFilter>();

            string rendererInfo = "";
            if (mr != null)
            {
                string matName = (mr.sharedMaterial != null) ? mr.sharedMaterial.name : "NULL_MAT";
                string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "NO_MESH";
                int vertCount = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.vertexCount : 0;
                rendererInfo = $" | Renderer: enabled={mr.enabled}, mat={matName}, mesh={meshName}, verts={vertCount}, boundsCenter={mr.bounds.center}, boundsSize={mr.bounds.size}";
            }

            sb.AppendLine($"{indentStr}- {tr.name} (activeSelf={tr.gameObject.activeSelf}){rendererInfo}");

            foreach (Transform child in tr)
            {
                InspectTransform(child, sb, indent + 1);
            }
        }
    }
}
#endif
