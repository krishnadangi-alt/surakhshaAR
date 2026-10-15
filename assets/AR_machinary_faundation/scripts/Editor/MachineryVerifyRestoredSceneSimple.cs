using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class MachineryVerifyRestoredSceneSimple
{
    [MenuItem("Machinery/Verify Restored Scene Simple")]
    public static void VerifyRestoredScene()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText("restore_simple_verification.txt", "SUCCESS: Restored scene opened and re-saved cleanly in Unity Editor.");
            Debug.Log("[RestoreVerification] Restored scene opened and saved successfully.");
        }
        else
        {
            File.WriteAllText("restore_simple_verification.txt", "FAILED: Restored scene could not be opened.");
            Debug.LogError("[RestoreVerification] Failed to open restored scene.");
        }
    }
}
