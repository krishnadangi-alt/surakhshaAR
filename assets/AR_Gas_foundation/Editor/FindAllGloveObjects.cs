using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FindAllGloveObjects
{
    [MenuItem("Tools/Find All Glove Objects")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"=== TOTAL GAMEOBJECTS IN SCENE: {allObjects.Length} ===");

        foreach (var go in allObjects)
        {
            if (go.name.ToLower().Contains("glove") || go.name.ToLower().Contains("clove"))
            {
                Renderer r = go.GetComponent<Renderer>();
                Debug.Log($"FOUND GLOVE OBJ: '{go.name}' | ActiveInHierarchy: {go.activeInHierarchy} | Parent: {(go.transform.parent != null ? go.transform.parent.name : "null")} | Renderer: {(r != null ? r.GetType().Name : "none")}");
            }
        }
    }
}
