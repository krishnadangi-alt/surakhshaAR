using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// BatchMode entry point for the master fix.
/// Called via: Unity.exe -batchmode -executeMethod MachineryBatchFix.Run
/// </summary>
public class MachineryBatchFix
{
    private const string SCENE_PATH = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

    public static void Run()
    {
        Debug.Log("=== MachineryBatchFix.Run() START ===");

        // Load the scene
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open scene: " + SCENE_PATH);
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("Scene loaded: " + scene.name);

        // Run the master fixer
        var fixer = new MachineryFinalMasterFixer();
        fixer.Execute();

        // Save the scene
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        Debug.Log("Scene saved: " + saved);

        // Create a backup copy as FINAL
        string srcPath = SCENE_PATH;
        string dstPath = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_REFERENCE_ALIGNED_FINAL.unity";
        if (File.Exists(srcPath))
        {
            File.Copy(srcPath, dstPath, true);
            Debug.Log("Final backup saved: " + dstPath);
        }

        Debug.Log("=== MachineryBatchFix.Run() COMPLETE ===");
        EditorApplication.Exit(0);
    }
}
