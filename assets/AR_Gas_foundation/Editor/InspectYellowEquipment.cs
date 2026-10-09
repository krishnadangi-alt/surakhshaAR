using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectYellowEquipment
{
    [MenuItem("Tools/Inspect Yellow Equipment")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        Debug.Log($"=== TOTAL GAMEOBJECTS IN SCENE: {allObjects.Length} ===");

        foreach (GameObject go in allObjects)
        {
            string name = go.name.ToLower();
            if (name.Contains("yellow") || name.Contains("hoist") || name.Contains("crane") || name.Contains("winch") || name.Contains("tripod") || name.Contains("pulley") || name.Contains("structure"))
            {
                Debug.Log($"FOUND CANDIDATE: '{go.name}' Path: {GetPath(go)} Pos: {go.transform.position.ToString("F4")} Rot: {go.transform.eulerAngles.ToString("F4")} Scale: {go.transform.localScale.ToString("F4")} Active: {go.activeInHierarchy}");
                Renderer r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    Debug.Log($"  -> Renderer bounds center: {r.bounds.center.ToString("F4")}, size: {r.bounds.size.ToString("F4")}, min: {r.bounds.min.ToString("F4")}, max: {r.bounds.max.ToString("F4")}");
                }
            }
        }

        // Also search for confined space vessel / equipment
        foreach (GameObject go in allObjects)
        {
            string name = go.name.ToLower();
            if (name.Contains("confined") || name.Contains("vessel") || name.Contains("tank") || name.Contains("equipment"))
            {
                Debug.Log($"FOUND EQUIPMENT: '{go.name}' Path: {GetPath(go)} Pos: {go.transform.position.ToString("F4")} Rot: {go.transform.eulerAngles.ToString("F4")} Scale: {go.transform.localScale.ToString("F4")}");
                Renderer r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    Debug.Log($"  -> Renderer bounds center: {r.bounds.center.ToString("F4")}, size: {r.bounds.size.ToString("F4")}, min: {r.bounds.min.ToString("F4")}, max: {r.bounds.max.ToString("F4")}");
                }
            }
        }
    }

    private static string GetPath(GameObject obj)
    {
        string path = obj.name;
        Transform t = obj.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
