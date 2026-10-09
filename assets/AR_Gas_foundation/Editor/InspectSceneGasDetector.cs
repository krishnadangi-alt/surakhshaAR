using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectSceneGasDetector
{
    [MenuItem("Tools/Inspect Scene Gas Detector")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"=== SEARCHING SCENE FOR EXISTING DETECTORS ===");
        foreach (var go in allObjects)
        {
            if (go.name.ToLower().Contains("detector"))
            {
                Renderer r = go.GetComponent<Renderer>();
                Debug.Log($"SCENE DETECTOR: '{go.name}' (Active: {go.activeInHierarchy}) | Parent: {(go.transform.parent != null ? go.transform.parent.name : "null")} | Pos: {go.transform.position.ToString("F4")}, Scale: {go.transform.localScale.ToString("F4")}");
                if (r != null)
                {
                    Debug.Log($"   Renderer Bounds Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}, min.y: {r.bounds.min.y}");
                }
            }
        }
    }
}
