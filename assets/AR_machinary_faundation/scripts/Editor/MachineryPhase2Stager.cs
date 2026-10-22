#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPhase2Stager
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Stage Phase 2 Machinery Scene for Build")]
        public static void StagePhase2Scene()
        {
            Debug.Log("[Phase 2 Stager] Staging MachineryConveyorJam scene for Android Build...");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Phase 2 Stager] Cannot open scene at {ScenePath}");
                return;
            }

            // Remove inspection grid container if present
            GameObject inspectionGrid = GameObject.Find("MachineryModelInspection");
            if (inspectionGrid != null)
            {
                Object.DestroyImmediate(inspectionGrid);
            }

            // Ensure MachineryScenario root is present & active
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario != null)
            {
                scenario.SetActive(true);
            }

            // Ensure MachinerySession script object is active
            GameObject session = GameObject.Find("MachinerySession");
            if (session != null)
            {
                session.SetActive(true);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Phase 2 Stager] MachineryConveyorJam scene successfully staged and saved!");
        }
    }
}
#endif
