#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    /// <summary>
    /// Master Editor-only visibility restoration for the approved Machinery setup.
    /// Strictly non-destructive: NEVER modifies transforms, NEVER rebuilds, and NEVER overwrites the locked scene file.
    /// Menu: SurakshaAR > Machinery > Restore Original Machinery Editor Visibility
    /// </summary>
    [InitializeOnLoad]
    public static class MachineryEditorVisibilityMaster
    {
        private const string MainScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        static MachineryEditorVisibilityMaster()
        {
            EditorApplication.delayCall += OnDomainReload;
        }

        private static void OnDomainReload()
        {
            ApplyEditorVisibility();
        }

        [MenuItem("SurakshaAR/Machinery/Restore Original Machinery Editor Visibility", priority = 0)]
        public static void ApplyEditorVisibility()
        {
            // 1. Ensure the correct scene is open
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.path != MainScenePath)
            {
                currentScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
                Debug.Log($"[MachineryEditorVisibilityMaster] Opened correct main scene: {MainScenePath}");
            }

            // 2. Find MachineryScenario
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null)
            {
                var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allGOs)
                {
                    if (go.scene == currentScene && go.name == "MachineryScenario")
                    {
                        scenario = go;
                        break;
                    }
                }
            }

            if (scenario == null)
            {
                Debug.LogError("[MachineryEditorVisibilityMaster] Could not find MachineryScenario in scene!");
                return;
            }

            // 3. Temporarily activate MachineryScenario in the current Editor session
            if (!scenario.activeSelf)
            {
                scenario.SetActive(true);
                Debug.Log("[MachineryEditorVisibilityMaster] Temporarily activated MachineryScenario for Editor session.");
            }

            // 4. Select ConveyorAssembly or MachineryScenario
            Transform conveyor = scenario.transform.Find("ConveyorAssembly");
            GameObject targetSelection = conveyor != null ? conveyor.gameObject : scenario;
            Selection.activeGameObject = targetSelection;

            // Expand hierarchy in Hierarchy window
            EditorGUIUtility.PingObject(targetSelection);

            // 5. Open and dock Inspector window if needed
            var inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            if (inspectorType != null)
            {
                EditorWindow.GetWindow(inspectorType, false, "Inspector", false);
            }

            // 6. Frame the complete Machinery in Scene View
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null && SceneView.sceneViews.Count > 0)
            {
                view = SceneView.sceneViews[0] as SceneView;
            }

            if (view != null)
            {
                Vector3 pivot = new Vector3(0.5f, 1.1f, 0.0f);
                Quaternion rotation = Quaternion.Euler(16.0f, -22.0f, 0.0f);
                float size = 4.5f;

                view.orthographic = false;
                view.LookAt(pivot, rotation, size);
                view.Repaint();
                Debug.Log($"[MachineryEditorVisibilityMaster] Framed complete Machinery in SceneView (pivot={pivot}, size={size}).");
            }

            EditorApplication.RepaintHierarchyWindow();
            Debug.Log("[MachineryEditorVisibilityMaster] SUCCESS: Complete Machinery visible and framed in Editor. SCENE FILE NOT SAVED (Locked master preserved).");
        }
    }
}
#endif
