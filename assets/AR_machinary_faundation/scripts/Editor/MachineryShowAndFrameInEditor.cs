#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    /// <summary>
    /// Restores editor visibility and frames the complete Machinery setup in the Unity Editor Scene View.
    /// Strictly non-destructive: NEVER modifies transforms, NEVER rebuilds, and NEVER overwrites the locked scene file.
    /// Menu: SurakshaAR > Machinery > Show and Frame Machinery in Editor
    /// </summary>
    public static class MachineryShowAndFrameInEditor
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Show and Frame Machinery in Editor", priority = 1)]
        public static void ShowAndFrameMachinery()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.path != ScenePath)
            {
                currentScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log($"[MachineryShowAndFrameInEditor] Opened main scene: {ScenePath}");
            }

            GameObject[] roots = currentScene.GetRootGameObjects();
            GameObject scenario = null;
            foreach (var r in roots)
            {
                if (r.name == "MachineryScenario")
                {
                    scenario = r;
                    break;
                }
            }

            if (scenario == null)
            {
                Debug.LogError("[MachineryShowAndFrameInEditor] Could not locate 'MachineryScenario' root in active scene!");
                return;
            }

            // Ensure scenario is active in editor session so it is visible in Scene View
            if (!scenario.activeSelf)
            {
                scenario.SetActive(true);
                Debug.Log("[MachineryShowAndFrameInEditor] Activated 'MachineryScenario' in Editor session for visual inspection.");
            }

            // Expand and select ConveyorAssembly
            Transform conveyor = scenario.transform.Find("ConveyorAssembly");
            GameObject targetSelection = conveyor != null ? conveyor.gameObject : scenario;
            Selection.activeGameObject = targetSelection;
            EditorGUIUtility.PingObject(targetSelection);

            // Frame Scene View camera
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null && SceneView.sceneViews.Count > 0)
            {
                view = SceneView.sceneViews[0] as SceneView;
            }

            if (view != null)
            {
                Vector3 pivot = new Vector3(0.5f, 1.2f, -0.2f);
                Quaternion rotation = Quaternion.Euler(18f, -20f, 0f);
                float size = 4.2f;

                view.orthographic = false;
                view.LookAt(pivot, rotation, size);
                view.Repaint();
                Debug.Log($"[MachineryShowAndFrameInEditor] SceneView framed on Machinery (pivot={pivot}, size={size})");
            }

            Debug.Log("[MachineryShowAndFrameInEditor] COMPLETE: Machinery setup visible, selectable, and framed in Scene View. (Scene file remains locked on disk).");
        }
    }
}
#endif
