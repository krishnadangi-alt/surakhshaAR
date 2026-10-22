using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class MachinerySceneViewFramer
    {
        static MachinerySceneViewFramer()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            FrameReferenceView();
        }

        [MenuItem("SurakshaAR/Frame Reference View")]
        public static void FrameReferenceView()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.path != scenePath)
            {
                currentScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
            {
                sceneView = SceneView.sceneViews[0] as SceneView;
            }

            if (sceneView != null)
            {
                Vector3 pivot = new Vector3(0.35f, 1.0f, -0.5f);
                Quaternion rotation = Quaternion.Euler(18.0f, -15.0f, 0.0f);
                float size = 3.8f;

                sceneView.orthographic = false;
                sceneView.LookAt(pivot, rotation, size);
                sceneView.Repaint();
                Debug.Log($"[MachinerySceneViewFramer] SceneView framed to reference view: pivot={pivot}, size={size}");
            }
        }

        [MenuItem("SurakshaAR/Reload Scene From Disk")]
        public static void ReloadSceneFromDisk()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            FrameReferenceView();
            Debug.Log("[MachinerySceneViewFramer] Reloaded scene from disk and framed view.");
        }
    }
}
