using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FindYellowModelDetails
{
    [MenuItem("Tools/Find Yellow Model Details")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        Debug.Log("=== DETAILED INSPECTION OF OBJECTS NEAR CONFINED SPACE EQUIPMENT ===");

        foreach (GameObject go in allObjects)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                Vector3 center = r.bounds.center;
                // Look for objects around X: 1.0 to 2.5, Y: 0.5 to 1.5
                if (center.x >= 1.0f && center.x <= 2.5f && center.y >= 0.5f && center.y <= 1.8f)
                {
                    string matNames = "";
                    foreach (Material m in r.sharedMaterials)
                    {
                        if (m != null)
                        {
                            Color c = Color.white;
                            if (m.HasProperty("_Color")) c = m.color;
                            else if (m.HasProperty("_BaseColor")) c = m.GetColor("_BaseColor");
                            matNames += $"{m.name} (Color: {c.ToString()}), ";
                        }
                    }
                    Debug.Log($"OBJ: '{go.name}' Path: {GetPath(go)} | Pos: {go.transform.position.ToString("F4")} | BoundsCenter: {r.bounds.center.ToString("F4")} Size: {r.bounds.size.ToString("F4")} Min: {r.bounds.min.ToString("F4")} Max: {r.bounds.max.ToString("F4")} | Materials: {matNames}");
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
