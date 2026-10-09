using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class IdentifyGiantGreyObject
{
    [MenuItem("Tools/Identify Giant Grey Object")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"=== TOTAL RENDERERS IN SCENE: {allRenderers.Length} ===");

        Vector3 tablePos = new Vector3(-1.65f, 0.5f, 0.20f);
        foreach (var r in allRenderers)
        {
            float dist = Vector3.Distance(r.bounds.center, tablePos);
            if (dist < 2.5f)
            {
                Debug.Log($"NEAR TABLE: GameObject '{r.name}' (Active: {r.gameObject.activeInHierarchy}) | Parent: {(r.transform.parent != null ? r.transform.parent.name : "null")} | Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}");
            }
        }
    }
}
