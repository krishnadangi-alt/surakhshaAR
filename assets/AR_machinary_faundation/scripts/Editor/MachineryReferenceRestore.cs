#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryReferenceRestore
    {
        private const string ApprovedSceneV5Path = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_REFERENCE_ALIGNED_FINAL_V5.unity";
        private const string ApprovedSceneV4Path = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_REFERENCE_ALIGNED_FINAL_V4.unity";
        private const string ApprovedSceneV3Path = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_REFERENCE_ALIGNED_FINAL_V3.unity";
        private const string BeforeBackupPath = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_BEFORE_REALISTIC_FINAL_FIX_BACKUP.unity";
        private const string WorkingScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Restore Reference Aligned Scene (V5 Final Approved)", false, 10)]
        public static void RestoreReferenceAlignedSceneV5()
        {
            string targetBackup = File.Exists(ApprovedSceneV5Path) ? ApprovedSceneV5Path : (File.Exists(ApprovedSceneV4Path) ? ApprovedSceneV4Path : ApprovedSceneV3Path);
            if (!File.Exists(targetBackup))
            {
                Debug.LogWarning($"[SurakshaAR Restore] Approved backup scene not found at '{targetBackup}'. Executing V5 master fix.");
                MachineryMasterV5Fixer.ExecuteV5MasterFix();
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Restore Machinery Scene (V5 Approved)",
                "Are you sure you want to restore the reference-aligned Machinery scene (V5 Final Approved Realistic Setup)?\n\nThis will replace the active scene with the approved snapshot.",
                "Restore Scene",
                "Cancel"
            );

            if (!confirm) return;

            CopyAndLoadScene(targetBackup, WorkingScenePath);
            Debug.Log("[SurakshaAR Restore] SUCCESS: Machinery scene restored to V5 reference-aligned state.");
        }

        [MenuItem("SurakshaAR/Machinery/Restore Pre-Fix Backup Scene", false, 11)]
        public static void RestoreBeforeFixBackupScene()
        {
            if (!File.Exists(BeforeBackupPath))
            {
                Debug.LogError($"[SurakshaAR Restore] Pre-fix backup scene not found at '{BeforeBackupPath}'.");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Restore Pre-Fix Machinery Scene",
                "Are you sure you want to restore the initial scene state before V5 master fix?",
                "Restore Pre-Fix",
                "Cancel"
            );

            if (!confirm) return;

            CopyAndLoadScene(BeforeBackupPath, WorkingScenePath);
            Debug.Log("[SurakshaAR Restore] SUCCESS: Machinery scene restored to pre-fix backup state.");
        }

        private static void CopyAndLoadScene(string sourcePath, string targetPath)
        {
            File.Copy(sourcePath, targetPath, true);
            AssetDatabase.Refresh();
            UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
    }
}
#endif
