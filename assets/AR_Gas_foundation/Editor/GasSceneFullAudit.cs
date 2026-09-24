#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;

public class GasSceneFullAudit
{
    [MenuItem("SurakshaAR/Audit Gas Scene Full Hierarchy")]
    public static void PerformFullAudit()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine("[GAS SCENE COMPREHENSIVE RUNTIME AUDIT]");
        sb.AppendLine("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        sb.AppendLine($"\nOpened Scene: {scene.name} (Path: {scene.path})");

        // 1. ALL CAMERAS
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"\n--- CAMERAS ({cameras.Length}) ---");
        foreach (var cam in cameras)
        {
            sb.AppendLine($"Camera GO: '{cam.gameObject.name}', Active={cam.gameObject.activeInHierarchy}, Pos={cam.transform.position}, Rot={cam.transform.eulerAngles}, FOV={cam.fieldOfView}, Near={cam.nearClipPlane}, Far={cam.farClipPlane}");
        }

        // 2. ALL CANVASES & UI
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"\n--- CANVASES ({canvases.Length}) ---");
        foreach (var c in canvases)
        {
            sb.AppendLine($"Canvas GO: '{c.gameObject.name}', Active={c.gameObject.activeInHierarchy}, RenderMode={c.renderMode}, WorldRot={c.transform.eulerAngles}, Scale={c.transform.lossyScale}");
            CanvasScaler scaler = c.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                sb.AppendLine($"  - CanvasScaler: mode={scaler.uiScaleMode}, refRes={scaler.referenceResolution}");
            }
        }

        // 3. ALL LINE RENDERERS (Check for White Line Artifact)
        LineRenderer[] lines = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"\n--- LINE RENDERERS ({lines.Length}) ---");
        foreach (var l in lines)
        {
            sb.AppendLine($"LineRenderer GO: '{l.gameObject.name}', Active={l.gameObject.activeInHierarchy}, Enabled={l.enabled}, PositionsCount={l.positionCount}, WorldSpace={l.useWorldSpace}");
            for (int i = 0; i < Mathf.Min(l.positionCount, 10); i++)
            {
                sb.AppendLine($"  - Pos[{i}] = {l.GetPosition(i)}");
            }
            if (l.sharedMaterial != null)
            {
                sb.AppendLine($"  - Material: {l.sharedMaterial.name}, Color={l.sharedMaterial.color}");
            }
        }

        // 4. SCENE ROOT & WORKSTATION HIERARCHY
        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            sb.AppendLine("\nERROR: GasScenarioRoot NOT found in scene!");
        }
        else
        {
            sb.AppendLine($"\n--- GAS SCENARIO ROOT ({root.name}) ---");
            sb.AppendLine($"Root Pos: {root.transform.position}, Rot: {root.transform.eulerAngles}, Scale: {root.transform.lossyScale}");
            
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            sb.AppendLine($"Total Renderers under GasScenarioRoot: {renderers.Length}");
            foreach (var r in renderers)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "N/A";
                string matNames = "";
                foreach (var m in r.sharedMaterials)
                {
                    matNames += (m != null ? m.name : "NULL") + "; ";
                }
                sb.AppendLine($"  Renderer: '{r.gameObject.name}', Active={r.gameObject.activeInHierarchy}, Enabled={r.enabled}, Mesh='{meshName}', BoundsCenter={r.bounds.center}, BoundsSize={r.bounds.size}, Mats=[{matNames}]");
            }
        }

        string outputPath = Path.Combine(Application.dataPath, "../gas_scene_full_audit.log");
        File.WriteAllText(outputPath, sb.ToString());
        Debug.Log($"[GAS AUDIT COMPLETE] Written to {outputPath}");
    }
}
#endif
