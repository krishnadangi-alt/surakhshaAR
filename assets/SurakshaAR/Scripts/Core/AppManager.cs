using System.Collections.Generic;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Application-wide singleton. Lives on the "AppRoot" GameObject in
    /// Main.unity, marked DontDestroyOnLoad so it survives the
    /// additive load/unload of AR scenes.
    /// </summary>
    [RequireComponent(typeof(UIManager))]
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; set; }

        public UIManager UIManager { get; private set; }
        public UIController UIController { get; private set; }
        public AppState AppState { get; set; }
        public LocalizationManager Localization { get; set; }
        public FontManager FontManager { get; set; }

        public void InitializeForTesting(AppLanguage language = AppLanguage.Hindi)
        {
            Instance = this;
            AppState = GetComponent<AppState>() ?? gameObject.AddComponent<AppState>();
            FontManager = FontManager.Instance;
            Localization = new LocalizationManager();
            Localization.SetLanguage(language);
            CurrentProfile = UserProfileData.Load();
            Modules = ModuleCatalog.BuildDefaultCatalog();
        }
        public ARModuleLauncher ARLauncher { get; private set; }
        public AudioManager Audio { get; private set; }
        public AssessmentTelemetryManager Telemetry { get; private set; }
        public OfflineDataStore OfflineStore { get; private set; }
        public OfflineSyncManager SyncManager { get; private set; }

        public UserProfileData CurrentProfile { get; private set; }
        public List<ModuleData> Modules { get; private set; }

        [Header("Splash timing")]
        [Tooltip("Seconds the splash screen stays visible before auto-advancing.")]
        public float splashDurationSeconds = 2.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

            UIManager = GetComponent<UIManager>() ?? gameObject.AddComponent<UIManager>();
            UIController = GetComponent<UIController>() ?? gameObject.AddComponent<UIController>();
            AppState = GetComponent<AppState>() ?? gameObject.AddComponent<AppState>();
            ARLauncher = GetComponent<ARModuleLauncher>() ?? gameObject.AddComponent<ARModuleLauncher>();
            Audio = GetComponent<AudioManager>() ?? gameObject.AddComponent<AudioManager>();
            OfflineStore = GetComponent<OfflineDataStore>() ?? gameObject.AddComponent<OfflineDataStore>();
            SyncManager = GetComponent<OfflineSyncManager>() ?? gameObject.AddComponent<OfflineSyncManager>();
            Telemetry = GetComponent<AssessmentTelemetryManager>() ?? gameObject.AddComponent<AssessmentTelemetryManager>();

            FontManager = FontManager.Instance;
            Localization = new LocalizationManager();
            Localization.OnLanguageChanged += (lang) =>
            {
                FontManager.EnsureFallbackChains();
            };

            CurrentProfile = UserProfileData.Load();
            Modules = ModuleCatalog.BuildDefaultCatalog();
        }

        private void Start()
        {
            UIManager.ShowScreen(ScreenId.Splash);
        }

        public ModuleData GetModule(ModuleId id)
        {
            return Modules.Find(m => m.id == id);
        }

        public void SaveProfile(UserProfileData profile)
        {
            CurrentProfile = profile;
            CurrentProfile.Save();
        }

        /// <summary>
        /// Called by ARModuleLauncher when returning from an AR scenario
        /// mid-training (worker pressed Back). Returns to Home Dashboard
        /// so they can choose to retry via the Module Detail screen.
        /// </summary>
        public void ReturnFromARToScenarioSelection()
        {
            UIManager.SetUIVisible(true);
            UIManager.ShowScreen(ScreenId.HomeDashboard);
        }

        /// <summary>
        /// Called by ARModuleLauncher when the worker completed the AR
        /// training successfully and exited — moves the worker straight
        /// into the Assessment screen.
        /// </summary>
        public void ReturnFromARToAssessment()
        {
            UIManager.SetUIVisible(true);
            UIManager.ShowScreen(ScreenId.Assessment);
        }

        public void ReturnFromARToModuleSelection()
        {
            ReturnFromARToScenarioSelection();
        }
    }
}
