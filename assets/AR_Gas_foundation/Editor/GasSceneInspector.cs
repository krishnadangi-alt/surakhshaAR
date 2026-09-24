#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GasSceneInspector
{
    [MenuItem("SurakshaAR/Inspect Gas Scene Objects")]
    public static void InspectGasScene()
    {
        string logPath = "C:/SurakhshaAR/gas_scene_inspection.txt";
        using (StreamWriter writer = new StreamWriter(logPath, false))
        {
            writer.WriteLine("==================================================");
            writer.WriteLine("SURAKSHAAR GAS SCENE INSPECTION REPORT");
            writer.WriteLine("==================================================");

            string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            writer.WriteLine($"Active Scene: {scene.name}");

            // 1. ALL CAMERAS
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            writer.WriteLine($"\n[CAMERAS: {cameras.Length}]");
            foreach (var cam in cameras)
            {
                writer.WriteLine($"Camera Name: '{cam.gameObject.name}', Active={cam.gameObject.activeInHierarchy}, Pos={cam.transform.position.ToString("F3")}, Rot={cam.transform.eulerAngles.ToString("F3")}, FOV={cam.fieldOfView}, Near={cam.nearClipPlane}, Far={cam.farClipPlane}");
            }

            // 2. ALL CANVASES
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            writer.WriteLine($"\n[CANVASES: {canvases.Length}]");
            foreach (var c in canvases)
            {
                writer.WriteLine($"Canvas Name: '{c.gameObject.name}', Active={c.gameObject.activeInHierarchy}, Mode={c.renderMode}, Rot={c.transform.eulerAngles.ToString("F3")}, Scale={c.transform.lossyScale.ToString("F3")}");
                foreach (Transform child in c.transform)
                {
                    writer.WriteLine($"   Child UI: '{child.name}', Active={child.gameObject.activeInHierarchy}, LocalPos={child.localPosition.ToString("F3")}, LocalRot={child.localEulerAngles.ToString("F3")}");
                }
            }

            // 3. ALL LINE RENDERERS (Check for White Line Artifact)
            LineRenderer[] lines = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            writer.WriteLine($"\n[LINE RENDERERS: {lines.Length}]");
            foreach (var l in lines)
            {
                writer.WriteLine($"LineRenderer: '{l.gameObject.name}' (Parent: '{(l.transform.parent != null ? l.transform.parent.name : "ROOT")}'), Active={l.gameObject.activeInHierarchy}, Enabled={l.enabled}, PosCount={l.positionCount}, WorldSpace={l.useWorldSpace}");
                for (int i = 0; i < Mathf.Min(l.positionCount, 10); i++)
                {
                    writer.WriteLine($"   Pos[{i}] = {l.GetPosition(i).ToString("F3")}");
                }
            }

            // 4. GAS SCENARIO ROOT & INDUSTRIAL WORKSTATION HIERARCHY
            GameObject root = GameObject.Find("GasScenarioRoot");
            if (root != null)
            {
                writer.WriteLine($"\n[GAS SCENARIO ROOT: '{root.name}']");
                writer.WriteLine($"Root Pos={root.transform.position.ToString("F3")}, Rot={root.transform.eulerAngles.ToString("F3")}, Scale={root.transform.lossyScale.ToString("F3")}");

                writer.WriteLine("\n[DIRECT CHILDREN]");
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Transform child = root.transform.GetChild(i);
                    writer.WriteLine($"Child [{i}]: '{child.name}', Active={child.gameObject.activeInHierarchy}, LocalPos={child.localPosition.ToString("F3")}, LocalRot={child.localEulerAngles.ToString("F3")}, LocalScale={child.localScale.ToString("F3")}");
                }

                writer.WriteLine("\n[RENDERERS & MESHES UNDER ROOT]");
                Renderer[] rnds = root.GetComponentsInChildren<Renderer>(true);
                foreach (var r in rnds)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "N/A";
                    string matName = r.sharedMaterial != null ? r.sharedMaterial.name : "NULL";
                    writer.WriteLine($"  GO: '{r.gameObject.name}', Active={r.gameObject.activeInHierarchy}, Enabled={r.enabled}, Mesh='{meshName}', Mat='{matName}', BoundsCenter={r.bounds.center.ToString("F3")}, BoundsSize={r.bounds.size.ToString("F3")}");
                }
            }
            else
            {
                writer.WriteLine("\n[ERROR] GasScenarioRoot NOT found in scene!");
            }

            writer.WriteLine("\n==================================================");
            writer.WriteLine("END OF INSPECTION REPORT");
            writer.WriteLine("==================================================");
        }

        Debug.Log("[GasSceneInspector] Inspection finished. Log saved to C:/SurakhshaAR/gas_scene_inspection.txt");
    }
}
#endif
