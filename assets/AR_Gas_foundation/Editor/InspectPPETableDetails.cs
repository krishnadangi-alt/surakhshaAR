using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectPPETableDetails
{
    [MenuItem("Tools/Inspect PPE Table Details")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        if (ppeStation == null)
        {
            Debug.LogError("PPE_Preparation_Station not found!");
            return;
        }

        Debug.Log($"=== PPE_Preparation_Station World Pos: {ppeStation.transform.position.ToString("F4")}, Rot: {ppeStation.transform.rotation.eulerAngles.ToString("F4")}, Scale: {ppeStation.transform.localScale.ToString("F4")} ===");

        PrintHierarchy(ppeStation.transform, 0);
    }

    private static void PrintHierarchy(Transform t, int indent)
    {
        string prefix = new string('-', indent * 2);
        Renderer r = t.GetComponent<Renderer>();
        MeshFilter mf = t.GetComponent<MeshFilter>();
        
        string info = $"{prefix} '{t.name}' (Active: {t.gameObject.activeSelf}) | Pos: {t.localPosition.ToString("F4")}, Rot: {t.localRotation.eulerAngles.ToString("F4")}, Scale: {t.localScale.ToString("F4")}";
        if (mf != null && mf.sharedMesh != null)
        {
            info += $" | Mesh: {mf.sharedMesh.name} (Bounds center: {mf.sharedMesh.bounds.center.ToString("F4")}, min: {mf.sharedMesh.bounds.min.ToString("F4")}, max: {mf.sharedMesh.bounds.max.ToString("F4")})";
        }
        if (r != null)
        {
            info += $" | WorldBounds min: {r.bounds.min.ToString("F4")}, max: {r.bounds.max.ToString("F4")}";
            info += $" | Materials ({r.sharedMaterials.Length}): ";
            foreach (var mat in r.sharedMaterials)
            {
                info += mat != null ? $"{mat.name}, " : "null, ";
            }
        }
        Debug.Log(info);

        for (int i = 0; i < t.childCount; i++)
        {
            PrintHierarchy(t.GetChild(i), indent + 1);
        }
    }
}
