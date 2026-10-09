using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectGloveMeshDetails
{
    [MenuItem("Tools/Inspect Glove Mesh Details")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        if (ppeStation == null) return;

        Transform gLeft = ppeStation.transform.Find("Work_Gloves_Left");
        Transform gRight = ppeStation.transform.Find("Work_Gloves_Right");

        Debug.Log("=== GLOVES LEFT HIERARCHY & BOUNDS ===");
        if (gLeft != null) PrintRenderers(gLeft);

        Debug.Log("=== GLOVES RIGHT HIERARCHY & BOUNDS ===");
        if (gRight != null) PrintRenderers(gRight);
    }

    private static void PrintRenderers(Transform t)
    {
        Renderer[] renderers = t.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"Object '{t.name}' has {renderers.Length} Renderers in hierarchy:");
        foreach (var r in renderers)
        {
            MeshFilter mf = r.GetComponent<MeshFilter>();
            string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "no-mesh";
            Debug.Log($"  - Renderer on '{r.name}' (Active: {r.gameObject.activeSelf}, ActiveInHierarchy: {r.gameObject.activeInHierarchy}) | Mesh: {meshName} | World Bounds Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}");
        }
    }
}
