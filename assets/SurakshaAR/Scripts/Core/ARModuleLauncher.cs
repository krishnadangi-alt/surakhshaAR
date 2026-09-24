using SurakshaAR.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Core
{
    /// <summary>
    /// The ONLY script allowed to interact with AR scenes. It never
    /// edits or references internal scripts inside
    /// Assets/AR_Fire_Foundation — it only loads/unloads scenes by
    /// name through Unity's public SceneManager API, and toggles
    /// the SurakshaAR UI overlay around that load.
    /// </summary>
    public class ARModuleLauncher : MonoBehaviour
    {
        public static ARModuleLauncher Instance { get; private set; }

        /// <summary>True while an AR scene is loaded additively on top of the app.</summary>
        public bool IsArActive => _arSceneActive;

        private string _loadedArSceneName;
        private bool _trainingCompleted;
        private bool _arSceneActive;
        private Camera _uiCamera;

        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            // The AR layer raises this public event when a training run is
            // completed. We only observe it (no protected-file changes) so the
            // app can route the worker to Assessment on exit instead of back
            // to Scenario Selection.
            TrainingEventManager.OnTrainingCompleted += OnArTrainingCompleted;
        }

        private void OnDestroy()
        {
            TrainingEventManager.OnTrainingCompleted -= OnArTrainingCompleted;
        }

        private void OnArTrainingCompleted()
        {
            _trainingCompleted = true;
        }

        private void Update()
        {
            // Allow Android hardware/gesture back or Escape to return from AR.
            // NOTE: project uses the new Input System only (activeInputHandler=1),
            // so we use Keyboard.current instead of the legacy Input class.
            if (_arSceneActive &&
                Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitCurrentARScene();
            }
        }

        /// <summary>
        /// Attempts to launch the AR scene for the given module.
        /// Returns false if the module has no AR scene built yet.
        /// </summary>
        public bool TryLaunchModule(ModuleData module)
        {
            if (module == null) return false;

            if (!module.isImplemented || string.IsNullOrEmpty(module.arSceneName))
            {
                return false;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                StartCoroutine(RequestCameraAndLaunchRoutine(module));
                return true;
            }
#endif

            ExecuteLaunch(module);
            return true;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private System.Collections.IEnumerator RequestCameraAndLaunchRoutine(ModuleData module)
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            float waitTimer = 0f;
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) && waitTimer < 3.0f)
            {
                waitTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                ExecuteLaunch(module);
            }
            else
            {
                Debug.LogError("[ARModuleLauncher] Camera permission denied. Cannot start AR scenario without camera feed.");
                AppManager.Instance?.UIManager?.SetUIVisible(true);
            }
        }
#endif

        private void ExecuteLaunch(ModuleData module)
        {
            _loadedArSceneName = module.arSceneName;
            _trainingCompleted = false;
            AppManager.Instance.UIManager.SetUIVisible(false);
            _arSceneActive = true;

            // Disable UI camera before AR scene loads to avoid touch raycast conflicts.
            _uiCamera = Camera.main;
            if (_uiCamera != null)
            {
                _uiCamera.gameObject.SetActive(false);
            }

            // Ensure XR Loader is initialized and subsystems are running
            if (UnityEngine.XR.Management.XRGeneralSettings.Instance != null &&
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager != null)
            {
                var mgr = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
                if (mgr.activeLoader == null)
                {
                    mgr.InitializeLoaderSync();
                }
                if (mgr.activeLoader != null)
                {
                    mgr.StartSubsystems();
                }
            }

            SceneManager.LoadScene(_loadedArSceneName, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Unloads the currently active AR scene and restores
        /// the SurakshaAR UI on the Scenario Selection screen.
        /// </summary>
        public void ExitCurrentARScene()
        {
            if (!_arSceneActive || string.IsNullOrEmpty(_loadedArSceneName))
            {
                return;
            }

            // Stop XR subsystems upon exiting AR mode
            if (UnityEngine.XR.Management.XRGeneralSettings.Instance != null &&
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager != null)
            {
                var mgr = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
                if (mgr.activeLoader != null)
                {
                    mgr.StopSubsystems();
                    mgr.DeinitializeLoader();
                }
            }

            SceneManager.UnloadSceneAsync(_loadedArSceneName);
            _arSceneActive = false;
            _loadedArSceneName = null;

            // Hand camera and audio ownership back to the UI scene.
            if (_uiCamera != null)
            {
                _uiCamera.gameObject.SetActive(true);
                _uiCamera = null;
            }

            bool completed = _trainingCompleted;
            _trainingCompleted = false;

            if (completed)
            {
                // Worker finished the training: continue to Assessment.
                AppManager.Instance.ReturnFromARToAssessment();
            }
            else
            {
                // Worker pressed back mid-training: return to Scenario Selection.
                AppManager.Instance.ReturnFromARToScenarioSelection();
            }
        }
    }
}
