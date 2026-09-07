# SurakshaAR Member-1 Complete Codebase

Generated: 09/07/2026 00:21:09

---

## C:\project\surakshaAR\assets\app\Assessment\Data\FireQuestions.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Assessment\Scripts\AssessmentEventBridge.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Assessment\Scripts\AssessmentManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Assessment\Scripts\QuestionManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Assessment\Scripts\ScoreManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Certificate\Scripts\CertificateManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Certificate\Scripts\CertificateVerification.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Certificate\Scripts\QRCodeGenerator.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Core\Data\AssessmentData.cs

```csharp
using System;

namespace SurakshaAR.Data
{
    [Serializable]
    public class AssessmentData
    {
        public int assessmentId;
        public int workerId;
        public int moduleId;
        public float overallScore;
        public bool passed;
        public string completedAt;
    }
}

```

---

## C:\project\surakshaAR\assets\app\Core\Data\TrainingData.cs

```csharp
using System;

namespace SurakshaAR.Data
{
    [Serializable]
    public class TrainingData
    {
        public int moduleId;
        public string moduleCode;
        public string stage;
        public string status;
        public float progress;
        public string lastUpdated;
    }
}

```

---

## C:\project\surakshaAR\assets\app\Core\Data\UserData.cs

```csharp
using System;

namespace SurakshaAR.Data
{
    [Serializable]
    public class UserData
    {
        public int workerId;
        public string name;
        public string employeeId;
        public string role;
        public string language;
        public string lastLogin;
    }
}

```

---

## C:\project\surakshaAR\assets\app\Core\Scripts\AppEvents.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Core\Scripts\AppManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Core\Scripts\AppState.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Core\Scripts\LocalizationManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Core\Scripts\ScreenManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Core\Scripts\UserSession.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Events\AppEvents.cs

```csharp
using UnityEngine;

namespace SurakshaAR.Events
{
    [System.Serializable]
    public class AppEvents
    {
        public static System.Action OnAppStarted;
        public static System.Action OnLanguageChanged;
        public static System.Action OnUserLoggedIn;
        public static System.Action OnUserLoggedOut;
        public static System.Action OnSceneLoaded;
        public static System.Action<bool> OnConnectionStatusChanged;

        public static void RaiseAppStarted() => OnAppStarted?.Invoke();
        public static void RaiseLanguageChanged() => OnLanguageChanged?.Invoke();
        public static void RaiseUserLoggedIn() => OnUserLoggedIn?.Invoke();
        public static void RaiseUserLoggedOut() => OnUserLoggedOut?.Invoke();
        public static void RaiseSceneLoaded() => OnSceneLoaded?.Invoke();
        public static void RaiseConnectionStatusChanged(bool isOnline) => OnConnectionStatusChanged?.Invoke(isOnline);
    }
}

```

---

## C:\project\surakshaAR\assets\app\Events\AssessmentEvent.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Events\AssessmentEvents.cs

```csharp
namespace SurakshaAR.Events
{
    public class AssessmentEvents
    {
        public static System.Action OnAssessmentStarted;
        public static System.Action OnAssessmentCompleted;
        public static System.Action<float> OnScoreUpdated;
        public static System.Action<bool> OnPassFail;
        public static System.Action<string> OnEventLogged;

        public static void RaiseAssessmentStarted() => OnAssessmentStarted?.Invoke();
        public static void RaiseAssessmentCompleted() => OnAssessmentCompleted?.Invoke();
        public static void RaiseScoreUpdated(float score) => OnScoreUpdated?.Invoke(score);
        public static void RaisePassFail(bool passed) => OnPassFail?.Invoke(passed);
        public static void RaiseEventLogged(string eventType) => OnEventLogged?.Invoke(eventType);
    }
}

```

---

## C:\project\surakshaAR\assets\app\Events\CertificateEvents.cs

```csharp
namespace SurakshaAR.Events
{
    public class CertificateEvents
    {
        public static System.Action OnCertificateGenerated;
        public static System.Action OnCertificateVerified;
        public static System.Action<string> OnCertificateDownloaded;

        public static void RaiseCertificateGenerated() => OnCertificateGenerated?.Invoke();
        public static void RaiseCertificateVerified() => OnCertificateVerified?.Invoke();
        public static void RaiseCertificateDownloaded(string id) => OnCertificateDownloaded?.Invoke(id);
    }
}

```

---

## C:\project\surakshaAR\assets\app\Events\EventDispatcher.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Events
{
    /// <summary>
    /// EventDispatcher — Centralized event system for decoupled communication.
    /// </summary>
    public class EventDispatcher : MonoBehaviour
    {
        public static EventDispatcher Instance { get; private set; }

        private Dictionary<string, List<System.Action<object>>> listeners = new Dictionary<string, List<System.Action<object>>>();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddListener(string eventName, System.Action<object> callback)
        {
            if (!listeners.ContainsKey(eventName))
                listeners[eventName] = new List<System.Action<object>>();
            listeners[eventName].Add(callback);
        }

        public void RemoveListener(string eventName, System.Action<object> callback)
        {
            if (listeners.ContainsKey(eventName))
                listeners[eventName].Remove(callback);
        }

        public void Raise(string eventName, object data = null)
        {
            if (listeners.ContainsKey(eventName))
            {
                foreach (var listener in listeners[eventName])
                    listener?.Invoke(data);
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Events\TrainingEvent.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Events\TrainingEvents.cs

```csharp
namespace SurakshaAR.Events
{
    public class TrainingEvents
    {
        public static System.Action OnTrainingStarted;
        public static System.Action OnTrainingCompleted;
        public static System.Action OnPracticeStarted;
        public static System.Action OnPracticeCompleted;
        public static System.Action<string> OnScenarioLoaded;

        public static void RaiseTrainingStarted() => OnTrainingStarted?.Invoke();
        public static void RaiseTrainingCompleted() => OnTrainingCompleted?.Invoke();
        public static void RaisePracticeStarted() => OnPracticeStarted?.Invoke();
        public static void RaisePracticeCompleted() => OnPracticeCompleted?.Invoke();
        public static void RaiseScenarioLoaded(string name) => OnScenarioLoaded?.Invoke(name);
    }
}

```

---

## C:\project\surakshaAR\assets\app\Events\UserEvent.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Data\FireScenarioData.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Data\FireTrainingState.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\ExtinguisherInteraction.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\ExtinguisherPickup.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\FireAlarmController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\FireAssessmentBridge.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\FireController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\FireScenarioManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\FireTrainingManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\HoseInteraction.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Features\FireSafety\Scripts\SafetyPinController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Offline\LocalStorage.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Offline\OfflineManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Offline\OfflineTrainingData.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\Scripts\Certificate\CertificateManager.cs

```csharp
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Certificate
{
    /// <summary>
    /// CertificateManager — Handles certificate generation and display.
    /// Integrates with the backend certificate API.
    /// </summary>
    public class CertificateManager : MonoBehaviour
    {
        [Header("API")]
        public AssessmentApiClient apiClient;

        [Header("Certificate Data")]
        public int workerId = -1;
        public int moduleId = 1;
        public string workerName = "";
        public string moduleName = "Fire Safety";
        public float score = 0f;
        public string certificateNumber = "";

        [Header("UI")]
        public GameObject certificatePanel;
        public UnityEngine.UIElements.Label certNameLabel;
        public UnityEngine.UIElements.Label certModuleLabel;
        public UnityEngine.UIElements.Label certScoreLabel;
        public UnityEngine.UIElements.Label certNumberLabel;
        public UnityEngine.UIElements.Label certDateLabel;

        public void GenerateCertificate(string name, string module, float finalScore)
        {
            workerName = name;
            moduleName = module;
            score = finalScore;
            certificateNumber = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            DisplayCertificate();
        }

        private void DisplayCertificate()
        {
            if (certificatePanel != null) certificatePanel.SetActive(true);
            if (certNameLabel != null) certNameLabel.text = workerName;
            if (certModuleLabel != null) certModuleLabel.text = moduleName;
            if (certScoreLabel != null) certScoreLabel.text = string.Format("Score: {0:F1}", score);
            if (certNumberLabel != null) certNumberLabel.text = "No: " + certificateNumber;
            if (certDateLabel != null) certDateLabel.text = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Certificate\CertificateVerification.cs

```csharp
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Certificate
{
    /// <summary>
    /// CertificateVerification — Verifies certificate validity via backend API.
    /// </summary>
    public class CertificateVerification : MonoBehaviour
    {
        public AssessmentApiClient apiClient;

        public void VerifyCertificate(string certificateNumber, System.Action<bool, string> callback)
        {
            if (apiClient == null)
            {
                callback?.Invoke(false, "No API client available");
                return;
            }

            // Verification would call the backend API
            // For now, check local cache
            string cached = PlayerPrefs.GetString("last_certificate", "");
            if (!string.IsNullOrEmpty(cached))
            {
                callback?.Invoke(true, "Certificate verified locally");
            }
            else
            {
                callback?.Invoke(false, "Certificate not found");
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Certificate\QRCodeGenerator.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace SurakshaAR.Certificate
{
    /// <summary>
    /// QRCodeGenerator — Creates a visual QR-like code for certificate verification.
    /// Simple texture-based pattern (placeholder for real QR library).
    /// </summary>
    public class QRCodeGenerator : MonoBehaviour
    {
        [Header("QR Display")]
        public VisualElement qrContainer;
        public int qrSize = 200;

        public void GenerateQR(string data)
        {
            if (qrContainer == null) return;
            qrContainer.Clear();

            // Create a simple visual pattern based on data hash
            int hash = data.GetHashCode();
            VisualElement grid = new VisualElement();
            grid.style.width = qrSize;
            grid.style.height = qrSize;
            grid.style.backgroundColor = Color.white;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.flexDirection = FlexDirection.Row;

            int cells = 16;
            int cellSize = qrSize / cells;
            for (int i = 0; i < cells * cells; i++)
            {
                VisualElement cell = new VisualElement();
                cell.style.width = cellSize - 1;
                cell.style.height = cellSize - 1;
                cell.style.backgroundColor = ((hash >> (i % 32)) & 1) == 1 ? Color.black : Color.white;
                grid.Add(cell);
            }

            qrContainer.Add(grid);
        }
    }
}


```

---

## C:\project\surakshaAR\assets\app\Scripts\Core\AppManager.cs

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Core
{
    /// <summary>
    /// AppManager — Application lifecycle and scene management.
    /// Persistent across scenes. Handles app state and navigation.
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        [Header("App State")]
        public string CurrentLanguage = "en";
        public int WorkerId = -1;
        public string WorkerName = "";
        public bool IsLoggedIn = false;

        [Header("Scenes")]
        public string bootScene = "Boot";
        public string homeScene = "Home";
        public string fireSafetyScene = "FireSafety";
        public string gasSafetyScene = "GasSafety";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerPrefs();
        }

        private void LoadPlayerPrefs()
        {
            CurrentLanguage = PlayerPrefs.GetString("app_language", "en");
            WorkerId = PlayerPrefs.GetInt("worker_id", -1);
            WorkerName = PlayerPrefs.GetString("worker_name", "");
            IsLoggedIn = PlayerPrefs.GetInt("is_logged_in", 0) == 1;
        }

        public void SetLanguage(string lang)
        {
            CurrentLanguage = lang;
            PlayerPrefs.SetString("app_language", lang);
            PlayerPrefs.Save();
        }

        public void SetWorker(int id, string name)
        {
            WorkerId = id;
            WorkerName = name;
            IsLoggedIn = true;
            PlayerPrefs.SetInt("worker_id", id);
            PlayerPrefs.SetString("worker_name", name);
            PlayerPrefs.SetInt("is_logged_in", 1);
            PlayerPrefs.Save();
        }

        public void Logout()
        {
            WorkerId = -1;
            WorkerName = "";
            IsLoggedIn = false;
            PlayerPrefs.DeleteKey("worker_id");
            PlayerPrefs.DeleteKey("worker_name");
            PlayerPrefs.DeleteKey("is_logged_in");
            PlayerPrefs.Save();
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void QuitApp()
        {
            Application.Quit();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Core\AppState.cs

```csharp
namespace SurakshaAR.Core
{
    /// <summary>
    /// AppState — Represents the current application state.
    /// </summary>
    public enum AppScreen
    {
        Boot,
        Splash,
        LanguageSelect,
        Login,
        Home,
        ModuleDetails,
        ScenarioSelection,
        ARTraining,
        Assessment,
        Result,
        Certificate,
        Progress,
        Profile,
        Offline
    }

    public enum TrainingMode
    {
        Training,
        Practice,
        Assessment
    }

    [System.Serializable]
    public class AppState
    {
        public AppScreen CurrentScreen = AppScreen.Splash;
        public TrainingMode CurrentMode = TrainingMode.Training;
        public int CurrentModuleId = 1;
        public bool IsOffline = false;
        public bool IsARReady = false;

        public void NavigateTo(AppScreen screen)
        {
            CurrentScreen = screen;
        }

        public void SetMode(TrainingMode mode)
        {
            CurrentMode = mode;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Core\UserSession.cs

```csharp
using System;

namespace SurakshaAR.Core
{
    /// <summary>
    /// UserSession — Represents the current worker session.
    /// </summary>
    [Serializable]
    public class UserSession
    {
        public int WorkerId;
        public string WorkerName;
        public string EmployeeId;
        public string Role;
        public string Language;
        public DateTime LoginTime;
        public bool IsActive;

        public UserSession()
        {
            WorkerId = -1;
            WorkerName = "";
            EmployeeId = "";
            Role = "";
            Language = "en";
            LoginTime = DateTime.UtcNow;
            IsActive = false;
        }

        public void StartSession(int id, string name, string employeeId = "", string role = "")
        {
            WorkerId = id;
            WorkerName = name;
            EmployeeId = employeeId;
            Role = role;
            LoginTime = DateTime.UtcNow;
            IsActive = true;
        }

        public void EndSession()
        {
            IsActive = false;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Integration\SimpleJSON.cs

```csharp
namespace SurakshaAR.Integration
{
    /// <summary>
    /// Lightweight JSON parser for localization (avoids external dependencies).
    /// </summary>
    public static class SimpleJSON
    {
        public static JSONNode Parse(string json)
        {
            return JSONNode.Parse(json);
        }
    }

    public class JSONNode
    {
        private System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>();

        public static JSONNode Parse(string json)
        {
            JSONNode node = new JSONNode();
            if (string.IsNullOrEmpty(json)) return node;

            // Minimal JSON parser for flat key-value objects
            json = json.Trim();
            if (json.StartsWith("{")) json = json.Substring(1);
            if (json.EndsWith("}")) json = json.Substring(0, json.Length - 1);

            bool inString = false;
            bool escape = false;
            string currentKey = "";
            string currentValue = "";
            bool readingKey = true;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (escape)
                {
                    if (readingKey) currentKey += c; else currentValue += c;
                    escape = false;
                    continue;
                }
                if (c == '\\') { escape = true; continue; }
                if (c == '"') { inString = !inString; continue; }
                if (!inString)
                {
                    if (c == ':') { readingKey = false; continue; }
                    if (c == ',')
                    {
                        if (!string.IsNullOrEmpty(currentKey))
                            node.dict[currentKey.Trim('"')] = currentValue.Trim().Trim('"');
                        currentKey = ""; currentValue = ""; readingKey = true;
                        continue;
                    }
                }
                if (readingKey) currentKey += c; else currentValue += c;
            }
            if (!string.IsNullOrEmpty(currentKey))
                node.dict[currentKey.Trim('"')] = currentValue.Trim().Trim('"');

            return node;
        }

        public JSONNode this[string key]
        {
            get { return dict.ContainsKey(key) ? new JSONNode(key, dict[key]) : null; }
        }

        public string Value
        {
            get { return dict.ContainsKey("__value") ? dict["__value"] : ""; }
        }

        public JSONNode(string k, string v)
        {
            dict["__value"] = v;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Localization\LocalizationManager.cs

```csharp
using UnityEngine;

namespace SurakshaAR.Localization
{
    /// <summary>
    /// LocalizationManager — JSON-based localization for English, Hindi, Santali.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Language Files")]
        public TextAsset englishJSON;
        public TextAsset hindiJSON;
        public TextAsset santaliJSON;

        private string currentLanguage = "en";
        private SimpleJSON.JSONNode currentDict;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(PlayerPrefs.GetString("app_language", "en"));
        }

        public void SetLanguage(string code)
        {
            currentLanguage = code;
            LoadLanguage(code);
            PlayerPrefs.SetString("app_language", code);
            PlayerPrefs.Save();
        }

        private void LoadLanguage(string code)
        {
            TextAsset asset = null;
            switch (code)
            {
                case "hi": asset = hindiJSON; break;
                case "sat": asset = santaliJSON; break;
                default: asset = englishJSON; break;
            }

            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                currentDict = SimpleJSON.JSON.Parse(asset.text);
            }
        }

        public string GetString(string key)
        {
            if (currentDict == null) return key;
            var node = currentDict[key];
            return node != null ? node.Value : key;
        }

        public string GetCurrentLanguage() => currentLanguage;
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Offline\LocalStorage.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Offline
{
    /// <summary>
    /// LocalStorage — File-based local storage for offline worker data.
    /// Stores assessment events, training progress, and scenario state.
    /// </summary>
    public static class LocalStorage
    {
        private const string ASSESSMENT_PREFIX = "local_assessment_";
        private const string PROGRESS_KEY = "local_progress";
        private const string SETTINGS_KEY = "local_settings";

        public static void SaveAssessment(string id, string json)
        {
            PlayerPrefs.SetString(ASSESSMENT_PREFIX + id, json);
            PlayerPrefs.Save();
        }

        public static string GetAssessment(string id)
        {
            return PlayerPrefs.GetString(ASSESSMENT_PREFIX + id, "");
        }

        public static List<string> GetAllPendingAssessments()
        {
            List<string> results = new List<string>();
            // PlayerPrefs doesn't support key enumeration, so we track IDs separately
            string ids = PlayerPrefs.GetString("pending_assessment_ids", "");
            if (string.IsNullOrEmpty(ids)) return results;

            foreach (string id in ids.Split(','))
            {
                if (string.IsNullOrEmpty(id)) continue;
                string json = PlayerPrefs.GetString(ASSESSMENT_PREFIX + id, "");
                if (!string.IsNullOrEmpty(json)) results.Add(json);
            }
            return results;
        }

        public static void AddPendingAssessmentId(string id)
        {
            string ids = PlayerPrefs.GetString("pending_assessment_ids", "");
            if (!ids.Contains(id))
            {
                ids += (string.IsNullOrEmpty(ids) ? "" : ",") + id;
                PlayerPrefs.SetString("pending_assessment_ids", ids);
                PlayerPrefs.Save();
            }
        }

        public static void RemovePendingAssessmentId(string id)
        {
            string ids = PlayerPrefs.GetString("pending_assessment_ids", "");
            List<string> list = new List<string>(ids.Split(','));
            list.Remove(id);
            PlayerPrefs.SetString("pending_assessment_ids", string.Join(",", list.ToArray()));
            PlayerPrefs.DeleteKey(ASSESSMENT_PREFIX + id);
            PlayerPrefs.Save();
        }

        public static void SaveProgress(string json)
        {
            PlayerPrefs.SetString(PROGRESS_KEY, json);
            PlayerPrefs.Save();
        }

        public static string GetProgress()
        {
            return PlayerPrefs.GetString(PROGRESS_KEY, "");
        }

        public static void SaveSettings(string language, int workerId, string workerName)
        {
            string json = JsonUtility.ToJson(new WorkerSettings
            {
                language = language,
                workerId = workerId,
                workerName = workerName
            });
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
        }

        public static WorkerSettings GetSettings()
        {
            string json = PlayerPrefs.GetString(SETTINGS_KEY, "");
            if (string.IsNullOrEmpty(json)) return new WorkerSettings();
            return JsonUtility.FromJson<WorkerSettings>(json);
        }

        [System.Serializable]
        public class WorkerSettings
        {
            public string language = "en";
            public int workerId = -1;
            public string workerName = "";
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Offline\OfflineTrainingData.cs

```csharp
using System;
using UnityEngine;

namespace SurakshaAR.Offline
{
    /// <summary>
    /// OfflineTrainingData — Data structure for storing training content locally.
    /// </summary>
    [Serializable]
    public class OfflineTrainingData
    {
        public string moduleCode;
        public TrainingContent[] content;
    }

    [Serializable]
    public class TrainingContent
    {
        public string id;
        public string type; // "instruction", "hint", "voice"
        public string textEnglish;
        public string textHindi;
        public string textSantali;
        public string audioClipPath;
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Offline\SyncService.cs

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Offline
{
    /// <summary>
    /// SyncService — Synchronizes locally stored data with the backend.
    /// Handles offline-to-online transition.
    /// </summary>
    public class SyncService : MonoBehaviour
    {
        public static SyncService Instance { get; private set; }

        [Header("API")]
        public AssessmentApiClient apiClient;

        [Header("Settings")]
        public float syncIntervalSeconds = 30f;

        private bool isSyncing = false;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(SyncLoop());
        }

        private IEnumerator SyncLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(syncIntervalSeconds);
                if (!isSyncing) yield return TrySyncPendingAssessments();
            }
        }

        private IEnumerator TrySyncPendingAssessments()
        {
            if (apiClient == null) yield break;

            var pending = LocalStorage.GetAllPendingAssessments();
            if (pending.Count == 0) yield break;

            isSyncing = true;
            Debug.Log($"[SyncService] Syncing {pending.Count} pending assessments...");

            foreach (string json in pending)
            {
                var assessment = JsonUtility.FromJson<ApiContracts.AssessmentCreate>(json);
                if (assessment == null) continue;

                bool done = false;
                bool success = false;

                yield return apiClient.SubmitAssessment(assessment,
                    result =>
                    {
                        done = true;
                        success = true;
                        Debug.Log($"[SyncService] Synced assessment. Score: {result.overall_score}");
                    },
                    error =>
                    {
                        done = true;
                        success = false;
                        Debug.LogWarning($"[SyncService] Sync failed: {error}");
                    });

                // Remove from pending on success
                if (success)
                {
                    string pendingId = assessment.events[0].timestamp;
                    LocalStorage.RemovePendingAssessmentId(pendingId);
                }

                if (!done) yield break; // Stop syncing if request hangs
            }

            isSyncing = false;
        }

        public void QueueForSync(string assessmentJson, string assessmentId)
        {
            LocalStorage.SaveAssessment(assessmentId, assessmentJson);
            LocalStorage.AddPendingAssessmentId(assessmentId);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\Training\FireTrainingMode.cs

```csharp
using UnityEngine;
using SurakshaAR.Assessment;

namespace SurakshaAR.Training
{
    /// <summary>
    /// FireTrainingMode — Controls training, practice, and assessment modes for fire safety.
    /// Integrates with the existing FireScenarioFlowManager.
    /// </summary>
    public class FireTrainingMode : MonoBehaviour
    {
        public enum Mode { Training, Practice, Assessment }

        [Header("Mode")]
        public Mode currentMode = Mode.Training;

        [Header("References")]
        public AssessmentManager assessmentManager;
        public GameObject trainingUI;
        public GameObject[] trainingLabels;
        public GameObject[] trainingArrows;
        public GameObject[] hintObjects;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnTrainingStarted;
        public UnityEngine.Events.UnityEvent OnAssessmentStarted;
        public UnityEngine.Events.UnityEvent OnPracticeStarted;

        public void StartTraining()
        {
            currentMode = Mode.Training;
            EnableTrainingAids(true);
            OnTrainingStarted?.Invoke();
            Debug.Log("[FireTrainingMode] Training mode started.");
        }

        public void StartPractice()
        {
            currentMode = Mode.Practice;
            EnableTrainingAids(true);
            OnPracticeStarted?.Invoke();
            Debug.Log("[FireTrainingMode] Practice mode started.");
        }

        public void StartAssessment()
        {
            currentMode = Mode.Assessment;
            EnableTrainingAids(false);
            if (assessmentManager != null) assessmentManager.BeginAssessment();
            OnAssessmentStarted?.Invoke();
            Debug.Log("[FireTrainingMode] Assessment mode started.");
        }

        public void EndAssessment()
        {
            if (assessmentManager != null) assessmentManager.EndAssessment();
            Debug.Log("[FireTrainingMode] Assessment ended.");
        }

        private void EnableTrainingAids(bool enable)
        {
            if (trainingUI != null) trainingUI.SetActive(enable);
            foreach (var label in trainingLabels) if (label != null) label.SetActive(enable);
            foreach (var arrow in trainingArrows) if (arrow != null) arrow.SetActive(enable);
            foreach (var hint in hintObjects) if (hint != null) hint.SetActive(enable);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\HomeController.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using SurakshaAR.Core;

namespace SurakshaAR.UI
{
    /// <summary>
    /// HomeController — Worker home screen with module selection dashboard.
    /// Shows available safety training modules as cards.
    /// </summary>
    public class HomeController : MonoBehaviour
    {
        [Header("UI")]
        public UIDocument uiDocument;

        private VisualElement root;

        private void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            BuildHomeUI();
        }

        private void BuildHomeUI()
        {
            root.style.backgroundColor = new Color(0.05f, 0.1f, 0.2f);
            root.style.paddingLeft = 24;
            root.style.paddingRight = 24;
            root.style.paddingTop = 40;
            root.style.paddingBottom = 24;

            // Header
            string greeting = "नमस्ते";
            if (AppManager.Instance != null)
            {
                if (AppManager.Instance.CurrentLanguage == "en")
                    greeting = "Hello";
                else if (AppManager.Instance.CurrentLanguage == "sat")
                    greeting = "Johar";
            }

            Label header = new Label($"{greeting}, {(AppManager.Instance != null ? AppManager.Instance.WorkerName : "Worker")}");
            header.style.fontSize = 24;
            header.style.color = Color.white;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 8;
            root.Add(header);

            Label subtitle = new Label("Select a Safety Module");
            subtitle.style.fontSize = 16;
            subtitle.style.color = new Color(0.7f, 0.7f, 0.8f);
            subtitle.style.marginBottom = 24;
            root.Add(subtitle);

            // Module cards
            AddModuleCard(
                "Fire Safety",
                "🔥",
                "Fire & Explosion Response",
                new Color(0.8f, 0.2f, 0.1f),
                () =>
                {
                    if (AppManager.Instance != null)
                        AppManager.Instance.LoadScene(AppManager.Instance.fireSafetyScene);
                });

            AddModuleCard(
                "Gas Safety",
                "⚠️",
                "Gas Leak & Confined Space",
                new Color(0.9f, 0.6f, 0.1f),
                () =>
                {
                    if (AppManager.Instance != null)
                        AppManager.Instance.LoadScene(AppManager.Instance.gasSafetyScene);
                });

            AddModuleCard(
                "Progress",
                "📊",
                "View Training Progress",
                new Color(0.2f, 0.4f, 0.7f),
                () => { /* TODO: Progress screen */ });

            AddModuleCard(
                "Profile",
                "👤",
                "Worker Profile & Settings",
                new Color(0.3f, 0.5f, 0.3f),
                () => { /* TODO: Profile screen */ });
        }

        private void AddModuleCard(string title, string icon, string desc, Color color, Action onClick)
        {
            Button card = new Button();
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;
            card.style.height = 80;
            card.style.marginBottom = 12;
            card.style.backgroundColor = color;
            card.style.borderTopLeftRadius = 12;
            card.style.borderTopRightRadius = 12;
            card.style.borderBottomLeftRadius = 12;
            card.style.borderBottomRightRadius = 12;
            card.style.paddingLeft = 20;
            card.style.paddingRight = 20;

            Label iconLabel = new Label(icon);
            iconLabel.style.fontSize = 32;
            iconLabel.style.marginRight = 16;
            card.Add(iconLabel);

            VisualElement textContainer = new VisualElement();
            textContainer.style.flexGrow = 1;
            card.Add(textContainer);

            Label titleLabel = new Label(title);
            titleLabel.style.fontSize = 18;
            titleLabel.style.color = Color.white;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            textContainer.Add(titleLabel);

            Label descLabel = new Label(desc);
            descLabel.style.fontSize = 13;
            descLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            textContainer.Add(descLabel);

            card.clicked += () => onClick?.Invoke();
            root.Add(card);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\LanguageController.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using SurakshaAR.Core;

namespace SurakshaAR.UI
{
    /// <summary>
    /// LanguageController — Language selection screen (English, Hindi, Santali).
    /// </summary>
    public class LanguageController : MonoBehaviour
    {
        [Header("UI")]
        public UIDocument uiDocument;

        private VisualElement root;

        private void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            BuildLanguageUI();
        }

        private void BuildLanguageUI()
        {
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.backgroundColor = new Color(0.05f, 0.1f, 0.2f);
            root.style.paddingLeft = 40;
            root.style.paddingRight = 40;

            Label title = new Label("Select Your Language / अपनी भाषा चुनें");
            title.style.fontSize = 24;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 32;
            title.style.whiteSpace = WhiteSpace.Normal;
            root.Add(title);

            AddLanguageButton("English", "en");
            AddLanguageButton("हिन्दी", "hi");
            AddLanguageButton("संताली", "sat");
        }

        private void AddLanguageButton(string display, string code)
        {
            Button btn = new Button();
            btn.text = display;
            btn.style.fontSize = 20;
            btn.style.height = 56;
            btn.style.marginBottom = 12;
            btn.style.backgroundColor = new Color(0.2f, 0.4f, 0.7f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 8;
            btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8;
            btn.style.borderBottomRightRadius = 8;

            string capturedCode = code;
            btn.clicked += () =>
            {
                if (AppManager.Instance != null)
                {
                    AppManager.Instance.SetLanguage(capturedCode);
                    AppManager.Instance.LoadScene("Login");
                }
            };

            root.Add(btn);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\LoginController.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using SurakshaAR.Core;

namespace SurakshaAR.UI
{
    /// <summary>
    /// LoginController — Worker login/profile screen.
    /// Simple name + ID entry for MVP.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [Header("UI")]
        public UIDocument uiDocument;

        private VisualElement root;
        private TextField nameField;
        private TextField idField;
        private Label errorLabel;

        private void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            BuildLoginUI();
        }

        private void BuildLoginUI()
        {
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.backgroundColor = new Color(0.05f, 0.1f, 0.2f);
            root.style.paddingLeft = 40;
            root.style.paddingRight = 40;

            Label title = new Label("Worker Login");
            title.style.fontSize = 28;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 24;
            root.Add(title);

            Label nameLabel = new Label("Name / नाम");
            nameLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            nameLabel.style.fontSize = 14;
            nameLabel.style.marginBottom = 4;
            root.Add(nameLabel);

            nameField = new TextField();
            nameField.value = PlayerPrefs.GetString("worker_name", "");
            nameField.style.fontSize = 18;
            nameField.style.height = 44;
            nameField.style.marginBottom = 16;
            root.Add(nameField);

            Label idLabel = new Label("Employee ID / कर्मचारी पहचान");
            idLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            idLabel.style.fontSize = 14;
            idLabel.style.marginBottom = 4;
            root.Add(idLabel);

            idField = new TextField();
            idField.value = PlayerPrefs.GetInt("worker_id", 0) > 0 ? PlayerPrefs.GetInt("worker_id", 0).ToString() : "";
            idField.style.fontSize = 18;
            idField.style.height = 44;
            idField.style.marginBottom = 16;
            root.Add(idField);

            errorLabel = new Label("");
            errorLabel.style.color = new Color(1f, 0.4f, 0.4f);
            errorLabel.style.fontSize = 14;
            errorLabel.style.marginBottom = 8;
            errorLabel.style.display = DisplayStyle.None;
            root.Add(errorLabel);

            Button loginBtn = new Button();
            loginBtn.text = "Login / प्रवेश";
            loginBtn.style.fontSize = 18;
            loginBtn.style.height = 50;
            loginBtn.style.backgroundColor = new Color(0.2f, 0.6f, 0.3f);
            loginBtn.style.color = Color.white;
            loginBtn.style.borderTopLeftRadius = 8;
            loginBtn.style.borderTopRightRadius = 8;
            loginBtn.style.borderBottomLeftRadius = 8;
            loginBtn.style.borderBottomRightRadius = 8;

            loginBtn.clicked += OnLoginClicked;
            root.Add(loginBtn);
        }

        private void OnLoginClicked()
        {
            string name = nameField.value.Trim();
            string idText = idField.value.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(idText))
            {
                errorLabel.text = "Please enter name and ID.";
                errorLabel.style.display = DisplayStyle.Flex;
                return;
            }

            int workerId;
            if (!int.TryParse(idText, out workerId))
            {
                workerId = Mathf.Abs(idText.GetHashCode());
            }

            if (AppManager.Instance != null)
            {
                AppManager.Instance.SetWorker(workerId, name);
                AppManager.Instance.LoadScene(AppManager.Instance.homeScene);
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\OfflineController.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using SurakshaAR.Core;

namespace SurakshaAR.UI
{
    public class OfflineController : MonoBehaviour
    {
        public UIDocument uiDocument;
        private VisualElement root;

        private void Start()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            BuildUI();
        }

        private void BuildUI()
        {
            root.style.backgroundColor = new Color(0.05f, 0.1f, 0.2f);
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.paddingLeft = 40;
            root.style.paddingRight = 40;

            Label title = new Label("Offline Mode");
            title.style.fontSize = 28;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 16;
            root.Add(title);

            Label msg = new Label("You are currently offline.
Training content is still available.
Connect to sync your progress.");
            msg.style.fontSize = 16;
            msg.style.color = new Color(0.8f, 0.8f, 0.8f);
            msg.style.whiteSpace = WhiteSpace.Normal;
            msg.style.marginBottom = 24;
            root.Add(msg);

            Button retryBtn = new Button();
            retryBtn.text = "Retry Connection";
            retryBtn.style.fontSize = 16;
            retryBtn.style.height = 48;
            retryBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.7f);
            retryBtn.style.color = Color.white;
            retryBtn.style.borderTopLeftRadius = 8;
            retryBtn.style.borderTopRightRadius = 8;
            retryBtn.style.borderBottomLeftRadius = 8;
            retryBtn.style.borderBottomRightRadius = 8;
            retryBtn.clicked += () => { if (AppManager.Instance != null) AppManager.Instance.LoadScene(AppManager.Instance.homeScene); };
            root.Add(retryBtn);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\ResultController.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using SurakshaAR.Core;
using SurakshaAR.Networking;

namespace SurakshaAR.UI
{
    public class ResultController : MonoBehaviour
    {
        [Header("UI")]
        public UIDocument uiDocument;

        [Header("API")]
        public AssessmentApiClient apiClient;

        private VisualElement root;
        private Label statusLabel;
        private Label scoreLabel;
        private VisualElement competencyList;
        private Button retryButton;
        private Button certificateButton;
        private VisualElement loadingOverlay;

        private void Start()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            BuildHeader();
            BuildLoadingOverlay();
            LoadResult();
        }

        private void BuildHeader()
        {
            root.style.backgroundColor = new Color(0.05f, 0.1f, 0.2f);
            root.style.paddingLeft = 24;
            root.style.paddingRight = 24;
            root.style.paddingTop = 40;
            root.style.alignItems = Align.Center;

            statusLabel = new Label("---");
            statusLabel.style.fontSize = 32;
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statusLabel.style.marginBottom = 16;
            root.Add(statusLabel);

            scoreLabel = new Label("---");
            scoreLabel.style.fontSize = 20;
            scoreLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            scoreLabel.style.marginBottom = 24;
            root.Add(scoreLabel);

            Label compTitle = new Label("Competency Breakdown");
            compTitle.style.fontSize = 16;
            compTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            compTitle.style.color = new Color(0.7f, 0.7f, 0.7f);
            compTitle.style.marginBottom = 8;
            root.Add(compTitle);

            competencyList = new VisualElement();
            competencyList.style.marginBottom = 24;
            root.Add(competencyList);

            retryButton = new Button();
            retryButton.text = "Retry Assessment";
            StyleButton(retryButton, new Color(0.8f, 0.3f, 0.1f));
            retryButton.style.marginBottom = 8;
            retryButton.clicked += () => { if (AppManager.Instance != null) AppManager.Instance.LoadScene(AppManager.Instance.fireSafetyScene); };
            root.Add(retryButton);

            certificateButton = new Button();
            certificateButton.text = "View Certificate";
            StyleButton(certificateButton, new Color(0.2f, 0.6f, 0.3f));
            certificateButton.style.marginBottom = 8;
            certificateButton.style.display = DisplayStyle.None;
            certificateButton.clicked += () => { if (AppManager.Instance != null) AppManager.Instance.LoadScene("Certificate"); };
            root.Add(certificateButton);

            Button homeButton = new Button();
            homeButton.text = "Back to Home";
            StyleButton(homeButton, new Color(0.2f, 0.4f, 0.7f));
            homeButton.clicked += () => { if (AppManager.Instance != null) AppManager.Instance.LoadScene(AppManager.Instance.homeScene); };
            root.Add(homeButton);
        }

        private void StyleButton(Button btn, Color color)
        {
            btn.style.fontSize = 16;
            btn.style.height = 48;
            btn.style.backgroundColor = color;
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 8;
            btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8;
            btn.style.borderBottomRightRadius = 8;
        }

        private void BuildLoadingOverlay()
        {
            loadingOverlay = new VisualElement();
            loadingOverlay.style.position = Position.Absolute;
            loadingOverlay.style.top = 0; loadingOverlay.style.left = 0;
            loadingOverlay.style.right = 0; loadingOverlay.style.bottom = 0;
            loadingOverlay.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            loadingOverlay.style.alignItems = Align.Center;
            loadingOverlay.style.justifyContent = Justify.Center;
            loadingOverlay.style.display = DisplayStyle.Flex;
            Label loadingLabel = new Label("Loading results...");
            loadingLabel.style.fontSize = 18;
            loadingLabel.style.color = Color.white;
            loadingOverlay.Add(loadingLabel);
            root.Add(loadingOverlay);
        }

        private void LoadResult()
        {
            string cached = PlayerPrefs.GetString("last_assessment_result", "");
            if (!string.IsNullOrEmpty(cached))
            {
                var result = JsonUtility.FromJson<ApiContracts.AssessmentResultOut>(cached);
                DisplayResult(result);
                loadingOverlay.style.display = DisplayStyle.None;
            }
            else
            {
                loadingOverlay.style.display = DisplayStyle.None;
                ShowNoResult();
            }
        }

        public void DisplayResult(ApiContracts.AssessmentResultOut result)
        {
            if (result.passed)
            {
                statusLabel.text = "PASSED";
                statusLabel.style.color = new Color(0.3f, 0.9f, 0.4f);
                certificateButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                statusLabel.text = "NEEDS RETRAINING";
                statusLabel.style.color = new Color(0.9f, 0.4f, 0.3f);
                certificateButton.style.display = DisplayStyle.None;
            }

            scoreLabel.text = string.Format("Overall Score: {0:F1} / 100", result.overall_score);

            competencyList.Clear();
            if (result.competency_scores != null)
            {
                foreach (var kvp in result.competency_scores)
                    AddCompetencyBar(kvp.Key, kvp.Value);
            }
        }

        private void AddCompetencyBar(string name, float score)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 6;

            Label nameLabel = new Label(FormatCompetencyName(name));
            nameLabel.style.fontSize = 13;
            nameLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            nameLabel.style.width = 140;
            row.Add(nameLabel);

            VisualElement barBg = new VisualElement();
            barBg.style.height = 8;
            barBg.style.flexGrow = 1;
            barBg.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            barBg.style.borderTopLeftRadius = 4;
            barBg.style.borderTopRightRadius = 4;
            barBg.style.borderBottomLeftRadius = 4;
            barBg.style.borderBottomRightRadius = 4;
            barBg.style.marginRight = 8;

            VisualElement barFill = new VisualElement();
            barFill.style.height = Length.Percent(100);
            barFill.style.width = Length.Percent(Mathf.Clamp01(score / 100f) * 100f);
            barFill.style.backgroundColor = score >= 70f ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.9f, 0.5f, 0.2f);
            barFill.style.borderTopLeftRadius = 4;
            barFill.style.borderTopRightRadius = 4;
            barFill.style.borderBottomLeftRadius = 4;
            barFill.style.borderBottomRightRadius = 4;
            barBg.Add(barFill);
            row.Add(barBg);

            Label sLabel = new Label(score.ToString("F0"));
            sLabel.style.fontSize = 12;
            sLabel.style.color = Color.white;
            sLabel.style.width = 30;
            row.Add(sLabel);

            competencyList.Add(row);
        }

        private string FormatCompetencyName(string raw)
        {
            string formatted = raw.Replace("_", " ");
            if (formatted.Length > 0)
                formatted = char.ToUpper(formatted[0]) + formatted.Substring(1);
            return formatted;
        }

        private void ShowNoResult()
        {
            statusLabel.text = "No Results";
            statusLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            scoreLabel.text = "Complete an assessment to see results.";
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\ScreenManager.cs

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace SurakshaAR.UI
{
    /// <summary>
    /// ScreenManager — Handles screen transitions with fade in/out.
    /// Attach to a persistent UI root GameObject.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        [Header("UI Document")]
        public UIDocument uiDocument;

        [Header("Transition")]
        public float fadeDuration = 0.3f;

        private VisualElement root;
        private VisualElement fadeOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            BuildFadeOverlay();
        }

        private void BuildFadeOverlay()
        {
            fadeOverlay = new VisualElement();
            fadeOverlay.style.position = Position.Absolute;
            fadeOverlay.style.top = 0;
            fadeOverlay.style.left = 0;
            fadeOverlay.style.right = 0;
            fadeOverlay.style.bottom = 0;
            fadeOverlay.style.backgroundColor = Color.black;
            fadeOverlay.style.opacity = 0;
            fadeOverlay.style.display = DisplayStyle.None;
            root.Add(fadeOverlay);
        }

        public void ShowScreen(VisualElement screen)
        {
            if (root == null) return;
            for (int i = 0; i < root.childCount; i++)
            {
                if (root[i] != fadeOverlay)
                    root[i].style.display = DisplayStyle.None;
            }
            screen.style.display = DisplayStyle.Flex;
        }

        public IEnumerator TransitionTo(VisualElement targetScreen)
        {
            fadeOverlay.style.display = DisplayStyle.Flex;
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.style.opacity = elapsed / fadeDuration;
                yield return null;
            }
            ShowScreen(targetScreen);
            elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.style.opacity = 1f - (elapsed / fadeDuration);
                yield return null;
            }
            fadeOverlay.style.display = DisplayStyle.None;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\Scripts\UI\SplashController.cs

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using SurakshaAR.Core;

namespace SurakshaAR.UI
{
    /// <summary>
    /// SplashController — Displays splash screen, then navigates to language selection.
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("UI")]
        public UIDocument uiDocument;

        [Header("Timing")]
        public float splashDuration = 2.5f;

        [Header("Localization")]
        public string appName = "SURAKSHAAR";
        public string tagline = "AI-Powered Safety Training";

        private VisualElement root;
        private Label appNameLabel;
        private Label taglineLabel;

        private void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            BuildSplashUI();
            StartCoroutine(SplashSequence());
        }

        private void BuildSplashUI()
        {
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.backgroundColor = new Color(0.05f, 0.1f, 0.2f);

            appNameLabel = new Label(appName);
            appNameLabel.style.fontSize = 42;
            appNameLabel.style.color = Color.white;
            appNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            appNameLabel.style.marginBottom = 16;
            root.Add(appNameLabel);

            taglineLabel = new Label(tagline);
            taglineLabel.style.fontSize = 18;
            taglineLabel.style.color = new Color(0.7f, 0.8f, 1f);
            root.Add(taglineLabel);
        }

        private IEnumerator SplashSequence()
        {
            yield return new WaitForSeconds(splashDuration);

            if (AppManager.Instance != null)
            {
                if (AppManager.Instance.IsLoggedIn)
                    AppManager.Instance.LoadScene(AppManager.Instance.homeScene);
                else
                    AppManager.Instance.LoadScene("LanguageSelect");
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\AppUIController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ARTrainingUIController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\AssessmentController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\CertificateController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\FireModuleController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\HomeController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\LanguageController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\LoginController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ModuleDetailsController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\OfflineController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ProfileController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ProgressController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ResultController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ScenarioController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\ScreenManager.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\SplashController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\app\UI\Scripts\UIController.cs

```csharp

```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\3d\ARPlacement.cs

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARPlacement : MonoBehaviour
{
    [Header("Complete Fire Scenario")]
    public GameObject fireScenario;

    [Header("Scenario Scale")]
    [Range(0.1f, 10f)]
    public float scenarioScale = 1f;

    [Header("Placement")]
    public bool usePrefabRotation = true;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    private GameObject spawnedScenario;

    private bool scenarioPlaced = false;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "ARPlacement requires ARRaycastManager on XR Origin."
            );
        }

        if (planeManager == null)
        {
            Debug.LogError(
                "ARPlacement requires ARPlaneManager on XR Origin."
            );
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "ARPlacement: FireScenario is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        // Once placed, NEVER reposition the scenario.
        if (scenarioPlaced)
            return;

        if (raycastManager == null)
            return;

        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        Vector2 screenPosition = touch.screenPosition;

        if (raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            PlaceScenario(hitPose);
        }
    }

    private void PlaceScenario(Pose hitPose)
    {
        if (fireScenario == null)
        {
            Debug.LogError(
                "ARPlacement: FireScenario is not assigned."
            );
            return;
        }

        Quaternion rotation;

        if (usePrefabRotation)
        {
            rotation = fireScenario.transform.rotation;
        }
        else
        {
            rotation = Quaternion.Euler(
                0f,
                hitPose.rotation.eulerAngles.y,
                0f
            );
        }

        // Create the scenario exactly where the user tapped.
        spawnedScenario = Instantiate(
            fireScenario,
            hitPose.position,
            rotation
        );

        // Make sure the spawned copy is visible.
        spawnedScenario.SetActive(true);

        // Apply scenario scale.
        spawnedScenario.transform.localScale =
            Vector3.one * scenarioScale;

        // Add AR Anchor to the COMPLETE scenario.
        ARAnchor anchor =
            spawnedScenario.GetComponent<ARAnchor>();

        if (anchor == null)
        {
            anchor =
                spawnedScenario.AddComponent<ARAnchor>();
        }

        // Hide the original template.
        fireScenario.SetActive(false);

        // Placement is now finished.
        scenarioPlaced = true;

        // Hide detected planes.
        HidePlanes();

        Debug.Log("training_started");
        Debug.Log(
            "FireScenario placed and anchored successfully."
        );
    }

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        planeManager.enabled = false;
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scenes\networking\ApiContracts.cs

```csharp
using System;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Data-transfer objects that exactly match the SurakshaAR backend API contract
    /// (see docs/api/API.md). Field names are intentionally snake_case to mirror the
    /// JSON keys so Unity's JsonUtility can (de)serialize them 1:1. Values are the
    /// fixed contract - change the backend and this file together.
    /// </summary>
    public static class ApiContracts
    {
        /// <summary>Standard error shape returned by the backend.</summary>
        [Serializable]
        public sealed class ApiError
        {
            public string detail;
        }

        // ------------------------------------------------------------------
        // Workers
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class WorkerCreate
        {
            public string name;
            public string employee_id;
            public string role;
        }

        [Serializable]
        public sealed class WorkerOut
        {
            public int id;
            public string name;
            public string employee_id;
            public string role;
            public string created_at;
        }

        // ------------------------------------------------------------------
        // Modules
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class ModuleOut
        {
            public int id;
            public string code;
            public string name;
            public string description;
        }

        [Serializable]
        public sealed class ModuleListOut
        {
            public ModuleOut[] modules;
        }

        // ------------------------------------------------------------------
        // Progress
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class ProgressCreate
        {
            public int worker_id;
            public int module_id;
            public string stage;
            public string status;
        }

        [Serializable]
        public sealed class ProgressOut
        {
            public int worker_id;
            public int module_id;
            public string stage;
            public string status;
            public string updated_at;
        }

        [Serializable]
        public sealed class ProgressItemOut
        {
            public int module_id;
            public string module_code;
            public string module_name;
            public string stage;
            public string status;
            public string last_updated;
        }

        [Serializable]
        public sealed class ProgressListOut
        {
            public int worker_id;
            public ProgressItemOut[] progress;
        }

        // ------------------------------------------------------------------
        // Assessments (event-based; scored server-side by the ML engine)
        // ------------------------------------------------------------------

        /// <summary>
        /// A single behavioural event from the VR/AR session. Only event_type is
        /// contractually required; event-specific fields are forwarded as-is.
        /// JsonUtility always serialises all fields, so unused ones are sent as
        /// empty/false - the backend's AssessmentEvent schema allows extra fields.
        /// </summary>
        [Serializable]
        public sealed class AssessmentEvent
        {
            public string event_type;
            public string timestamp;
            public bool correct;
            public string[] items;
            public string severity;   // wrong_action: minor | major
            public string action;
            public string hazard_type;
            public string reason;     // critical_action
            public string route;      // evacuation_started
            public string direction;  // evacuation_started (gas: upwind)
            public string completion_status; // assessment_completed
        }

        [Serializable]
        public sealed class AssessmentCreate
        {
            public int worker_id;
            public int module_id;
            public string scenario_type; // optional; "" -> derived from module code
            public int attempt_number;   // 0 -> auto-incremented server-side
            public AssessmentEvent[] events;
        }

        [Serializable]
        public sealed class CompetencyScoreOut
        {
            public string name;
            public float score;
            public bool passed;
            public float pass_threshold;
        }

        [Serializable]
        public sealed class WeaknessOut
        {
            public string competency_name;
            public float score;
            public float threshold;
            public string severity; // severe | moderate | mild
            public string reason;
            public string[] affected_aspects;
        }

        [Serializable]
        public sealed class RetrainingModuleOut
        {
            public string module_id;
            public string name;
            public string description;
            public int estimated_duration_minutes;
            public string difficulty_level;
            public string[] competencies_addressed;
            public string reason;
        }

        [Serializable]
        public sealed class RetrainingPlanOut
        {
            public string scenario_type;
            public RetrainingModuleOut[] recommended_modules;
            public int total_estimated_duration_minutes;
            public bool time_limit_exceeded;
            public int weaknesses_addressed;
            public int total_weaknesses;
        }

        /// <summary>
        /// Mirror of the backend's competency_scores JSON *object*. JsonUtility
        /// cannot deserialise dictionaries, but the competency key set is fixed
        /// per scenario (see ml/competency/scoring/config.py), so we model the map
        /// as a flat class. Missing keys deserialise to null.
        /// </summary>
        [Serializable]
        public sealed class CompetencyScoreMap
        {
            // fire + gas
            public CompetencyScoreOut hazard_identification;
            public CompetencyScoreOut ppe_selection;
            public CompetencyScoreOut equipment_use;
            // fire only
            public CompetencyScoreOut procedure_compliance;
            public CompetencyScoreOut decision_making;
            // gas only
            public CompetencyScoreOut evacuation;
            public CompetencyScoreOut emergency_response;

            public int Count()
            {
                int n = 0;
                if (hazard_identification != null) n++;
                if (ppe_selection != null) n++;
                if (equipment_use != null) n++;
                if (procedure_compliance != null) n++;
                if (decision_making != null) n++;
                if (evacuation != null) n++;
                if (emergency_response != null) n++;
                return n;
            }
        }

        [Serializable]
        public sealed class AssessmentOut
        {
            public int id;
            public int worker_id;
            public int module_id;
            public int attempt_number;
            public string scenario_type;
            public float score;
            public bool passed;
            public string pass_reason;
            public WeaknessOut[] weaknesses;
            public CompetencyScoreMap competency_scores;
            public string[] critical_errors;
            public string created_at;
        }

        [Serializable]
        public sealed class AssessmentHistoryOut
        {
            public int worker_id;
            public AssessmentOut[] assessments;
        }
        // ------------------------------------------------------------------
        // Sync (offline session data)
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class SyncSession
        {
            public string type;
            public int module_id;
            public float score;
            public bool passed;
            public string[] weaknesses;
            public string occurred_at;
        }

        [Serializable]
        public sealed class SyncCreate
        {
            public int worker_id;
            public string device_id;
            public SyncSession[] sessions;
        }

        [Serializable]
        public sealed class SyncOut
        {
            public int sync_id;
            public int worker_id;
            public string synced_at;
            public int sessions_synced;
        }

        [Serializable]
        public sealed class SyncStatusOut
        {
            public int worker_id;
            public string last_synced_at;
            public int pending_sessions;
        }

        // ------------------------------------------------------------------
        // Certificates
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class CertificateCreate
        {
            public int worker_id;
            public int module_id;
        }

        [Serializable]
        public sealed class CertificateOut
        {
            public int id;
            public string certificate_number;
            public int worker_id;
            public int module_id;
            public string issued_at;
            public string valid_until;
            public string status;
        }

        [Serializable]
        public sealed class CertificateListOut
        {
            public int worker_id;
            public CertificateOut[] certificates;
        }

        [Serializable]
        public sealed class CertificateVerifyOut
        {
            public string certificate_number;
            public bool valid;
            public string worker_name;
            public string module_name;
            public string issued_at;
            public string valid_until;
            public string status;
        }

        // ------------------------------------------------------------------
        // Vision / ML (docs/api/API.md #18, #19)
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class PPECheckRequest
        {
            public string image_base64;
            public string[] required_ppe;
            public string mode; // "auto" (default) | "mock" | "model"
        }

        [Serializable]
        public sealed class PPEDetectionOut
        {
            public string item;
            public float confidence;
        }

        [Serializable]
        public sealed class PPECheckOut
        {
            public string status; // ok | low_confidence | model_error | disabled
            public bool ppe_ok;
            public PPEDetectionOut[] detections;
            public string[] missing_items;
            public float confidence;
            public bool fallback_used;
            public string message;
            public string[] required_ppe;
        }

        [Serializable]
        public sealed class VisionStatusOut
        {
            public string status;
            public string module;
            public string version;
            public string mode;
            public string model_path;
            public bool model_loaded;
            public bool fallback_enabled;
            public string[] supported_items;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scenes\networking\AssessmentEvents.cs

```csharp
using System;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Factories for well-formed assessment events that the backend ML competency
    /// engine (ml/competency) understands. Build an assessment by collecting these
    /// in chronological order and submitting them with SubmissionAssessment.
    ///
    /// Event naming and field contracts follow docs/api/API.md -> "Assessment
    /// Events (ML Competency Engine)" and ml/competency/scoring/engine.py.
    /// </summary>
    public static class AssessmentEvents
    {
        /// <summary>hazard_identified - correct = worker spotted the hazard.</summary>
        public static ApiContracts.AssessmentEvent HazardIdentified(bool correct, string hazardType)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "hazard_identified",
                hazard_type = hazardType,
                correct = correct,
            };
        }

        /// <summary>ppe_selected - correct + the chosen items.</summary>
        public static ApiContracts.AssessmentEvent PpeSelected(bool correct, params string[] items)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "ppe_selected",
                items = items ?? new string[0],
                correct = correct,
            };
        }

        /// <summary>equipment_selected - correct choice of tool / equipment.</summary>
        public static ApiContracts.AssessmentEvent EquipmentSelected(bool correct, string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "equipment_selected",
                action = action,
                correct = correct,
            };
        }

        /// <summary>evacuation_started - fire: route; gas: upwind direction.</summary>
        public static ApiContracts.AssessmentEvent EvacuationStarted(bool correct, string routeOrDirection)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "evacuation_started",
                route = routeOrDirection,
                direction = routeOrDirection,
                correct = correct,
            };
        }

        /// <summary>emergency_procedure (gas only) - e.g. alert_supervisor.</summary>
        public static ApiContracts.AssessmentEvent EmergencyProcedure(bool correct, string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "emergency_procedure",
                action = action,
                correct = correct,
            };
        }

        /// <summary>wrong_action - minor or major mistake (no hint mode in assessment).</summary>
        public static ApiContracts.AssessmentEvent WrongAction(string severity)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "wrong_action",
                severity = severity,
            };
        }

        /// <summary>
        /// critical_action - a safety violation. Triggers automatic FAIL on the
        /// server regardless of all other scores.
        /// </summary>
        public static ApiContracts.AssessmentEvent CriticalAction(string action, string reason)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "critical_action",
                action = action,
                reason = reason,
            };
        }

        /// <summary>Stamps an event with the current UTC time (ISO 8601).</summary>
        public static ApiContracts.AssessmentEvent Timestamped(ApiContracts.AssessmentEvent e)
        {
            e.timestamp = DateTime.UtcNow.ToString("o");
            return e;
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scenes\networking\BackendSyncExample.cs

```csharp
using System.Collections;
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Example MonoBehaviour that demonstrates the full worker-app integration:
    /// health check -> worker registration -> progress -> event-based assessment
    /// (scored server-side by the ML competency engine) -> retraining plan ->
    /// vision PPE check -> offline-session sync.
    ///
    /// Attach it to the same GameObject that has SurakshaApiClient (or it will
    /// add one automatically).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BackendSyncExample : MonoBehaviour
    {
        [Header("Worker")]
        [SerializeField] private string workerName = "Ramesh Kumar";
        [SerializeField] private string employeeId = "EMP001";
        [SerializeField] private string role = "Fire Safety Worker";
        [SerializeField]
        [Tooltip("Used when the worker already exists (POST /workers returns 409).")]
        private int existingWorkerId = 0;

        [Header("Module / Scenario")]
        [Tooltip("1 = fire, 2 = gas")]
        [SerializeField] private int moduleId = 1;

        [Header("Assessment")]
        [Tooltip("false = submit a failing assessment (critical error) so the retraining plan is populated.")]
        [SerializeField] private bool passScenario = true;

        [Header("Vision (PPE check)")]
        [SerializeField] private bool runPpeCheck = true;

        [Header("Sync")]
        [SerializeField] private string deviceId = "device-xyz-789";

        // A minimal valid PNG (1x1) used purely to exercise the PPE check
        // pipeline end-to-end in mock mode (no camera capture in this example).
        private const string SamplePngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk" +
            "+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        private SurakshaApiClient api;
        private int workerId;

        private void Start()
        {
            api = SurakshaApiClient.Instance;
            if (api == null)
            {
                api = gameObject.AddComponent<SurakshaApiClient>();
            }

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // 1) Backend reachable?
            bool healthy = false;
            yield return api.CheckHealth(result =>
            {
                if (!result.Success)
                {
                    Debug.LogError("[SurakshaAR] Backend unreachable: " + result.Error);
                    return;
                }

                healthy = true;
                Debug.Log("[SurakshaAR] Backend health: OK");
            });

            if (!healthy)
            {
                yield break;
            }

            // 2) Register the worker (fall back to existingWorkerId on duplicate).
            bool hasWorker = false;
            var create = new ApiContracts.WorkerCreate
            {
                name = workerName,
                employee_id = employeeId,
                role = role,
            };

            yield return api.CreateWorker(create, result =>
            {
                if (result.Success)
                {
                    workerId = result.Data.id;
                    hasWorker = true;
                    Debug.Log("[SurakshaAR] Registered worker #" + workerId);
                }
                else if (result.StatusCode == 409 && existingWorkerId > 0)
                {
                    workerId = existingWorkerId;
                    hasWorker = true;
                    Debug.Log("[SurakshaAR] Worker already exists, using id " + workerId);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Worker registration failed: " + result.Error);
                }
            });

            if (!hasWorker)
            {
                yield break;
            }

            // 3) Record progress for the training workflow stage.
            var progress = new ApiContracts.ProgressCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                stage = "assess",
                status = "in_progress",
            };

            yield return api.UpdateProgress(progress, result =>
            {
                if (result.Success)
                {
                    Debug.Log("[SurakshaAR] Progress updated: " + result.Data.stage + " / " + result.Data.status);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Progress update failed: " + result.Error);
                }
            });

            // 4) Submit the event-based assessment (scored server-side by the ML engine).
            int assessmentId = 0;
            var assessment = new ApiContracts.AssessmentCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                events = BuildScenarioEvents(passScenario),
            };

            yield return api.SubmitAssessment(assessment, result =>
            {
                if (result.Success)
                {
                    assessmentId = result.Data.id;
                    Debug.Log("[SurakshaAR] Assessment #" + assessmentId + " score=" +
                              result.Data.score.ToString("0.0") + " passed=" + result.Data.passed);
                    Debug.Log("[SurakshaAR] Pass reason: " + result.Data.pass_reason);
                    LogCompetencyScores(result.Data.competency_scores);
                    LogWeaknesses(result.Data.weaknesses);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Assessment submission failed: " + result.Error);
                }
            });

            // 5) Fetch the targeted retraining plan (empty for a passing run).
            if (assessmentId > 0)
            {
                yield return api.GetRetrainingPlan(assessmentId, result =>
                {
                    if (result.Success)
                    {
                        Debug.Log("[SurakshaAR] Retraining plan: " +
                                  result.Data.weaknesses_addressed + "/" + result.Data.total_weaknesses +
                                  " weaknesses addressed in " + result.Data.total_estimated_duration_minutes + " min");
                        foreach (var module in result.Data.recommended_modules)
                        {
                            Debug.Log("[SurakshaAR]   -> " + module.name + " (" + module.estimated_duration_minutes + " min) " + module.reason);
                        }
                    }
                    else
                    {
                        Debug.LogError("[SurakshaAR] Retraining plan fetch failed: " + result.Error);
                    }
                });
            }

            // 6) Vision: PPE detection check (mock fallback until a model exists).
            if (runPpeCheck)
            {
                var ppe = new ApiContracts.PPECheckRequest
                {
                    image_base64 = SamplePngBase64,
                    required_ppe = new[] { "helmet", "safety_vest" },
                    mode = "auto",
                };

                yield return api.CheckPPE(ppe, result =>
                {
                    if (result.Success)
                    {
                        Debug.Log("[SurakshaAR] PPE check: status=" + result.Data.status +
                                  " ppe_ok=" + result.Data.ppe_ok +
                                  " fallback_used=" + result.Data.fallback_used);
                    }
                    else
                    {
                        Debug.LogError("[SurakshaAR] PPE check failed: " + result.Error);
                    }
                });
            }

            // 7) Sync an offline session to the backend.
            var sync = new ApiContracts.SyncCreate
            {
                worker_id = workerId,
                device_id = deviceId,
                sessions = new[]
                {
                    new ApiContracts.SyncSession
                    {
                        type = "assessment",
                        module_id = moduleId,
                        score = passScenario ? 90.0f : 39.0f,
                        passed = passScenario,
                        weaknesses = new string[0],
                        occurred_at = System.DateTime.UtcNow.ToString("o"),
                    },
                },
            };

            yield return api.SyncSessions(sync, result =>
            {
                if (result.Success)
                {
                    Debug.Log("[SurakshaAR] Synced " + result.Data.sessions_synced + " offline session(s)");
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Sync failed: " + result.Error);
                }
            });

            Debug.Log("[SurakshaAR] Integration demo finished.");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Build a minimal, well-formed scenario. The passing run covers all
        /// competencies; the failing run appends a critical safety violation so
        /// the server fails the assessment regardless of the other scores.
        /// </summary>
        private ApiContracts.AssessmentEvent[] BuildScenarioEvents(bool pass)
        {
            return new[]
            {
                AssessmentEvents.HazardIdentified(true, "electrical_fire"),
                AssessmentEvents.PpeSelected(true, "helmet", "gloves", "jacket"),
                AssessmentEvents.EquipmentSelected(true, "grab_extinguisher"),
                AssessmentEvents.EvacuationStarted(true, "north_exit"),
                // Append the safety violation for the failing run.
                pass
                    ? new ApiContracts.AssessmentEvent { event_type = "assessment_completed", completion_status = "success" }
                    : AssessmentEvents.CriticalAction("re_entered_unsafe_area", "CRITICAL: Re-entered the fire zone without clearance"),
            };
        }

        private static void LogCompetencyScores(ApiContracts.CompetencyScoreMap map)
        {
            if (map == null)
            {
                return;
            }

            LogScore(map.hazard_identification);
            LogScore(map.ppe_selection);
            LogScore(map.equipment_use);
            LogScore(map.procedure_compliance);
            LogScore(map.decision_making);
            LogScore(map.evacuation);
            LogScore(map.emergency_response);
        }

        private static void LogScore(ApiContracts.CompetencyScoreOut score)
        {
            if (score != null)
            {
                Debug.Log("[SurakshaAR]   competency " + score.name + ": " +
                          score.score.ToString("0.0") + " (pass=" + score.passed + ")");
            }
        }

        private static void LogWeaknesses(ApiContracts.WeaknessOut[] weaknesses)
        {
            if (weaknesses == null || weaknesses.Length == 0)
            {
                Debug.Log("[SurakshaAR]   weaknesses: none");
                return;
            }

            foreach (var w in weaknesses)
            {
                Debug.Log("[SurakshaAR]   weakness " + w.competency_name + ": " +
                          w.score.ToString("0.0") + " (" + w.severity + ") " + w.reason);
            }
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scenes\networking\SurakshaApiClient.cs

```csharp
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Networking
{
    /// <summary>Result of a request to the SurakshaAR backend.</summary>
    public sealed class ApiResult<T>
    {
        public bool Success { get; internal set; }
        public long StatusCode { get; internal set; }
        public T Data { get; internal set; }
        public string RawBody { get; internal set; }
        public string Error { get; internal set; }
    }

    /// <summary>
    /// Coroutine-based HTTP client for the SurakshaAR backend API (docs/api/API.md).
    /// Attach one instance to a persistent GameObject (e.g. the scene's XR Origin or
    /// a dedicated "Backend" GameObject). All methods start a coroutine on this
    /// MonoBehaviour and invoke the completion callback on the main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SurakshaApiClient : MonoBehaviour
    {
        public const string DefaultBaseUrl = "http://127.0.0.1:8000/api/v1";

        public static SurakshaApiClient Instance { get; private set; }

        [Header("Backend")]
        [Tooltip("Base URL of the SurakshaAR backend API (no trailing slash).")]
        [SerializeField] private string baseUrl = DefaultBaseUrl;

        [Tooltip("Request timeout in seconds.")]
        [SerializeField, Min(1)] private int timeoutSeconds = 15;

        public string BaseUrl => baseUrl;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ------------------------------------------------------------------
        // Low-level GET / POST
        // ------------------------------------------------------------------

        private IEnumerator GetJson<T>(string path, Action<ApiResult<T>> onDone)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl(path)))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                Finish(request, onDone);
            }
        }

        private IEnumerator PostJson<TReq, TRes>(string path, TReq payload, Action<ApiResult<TRes>> onDone)
        {
            string json = JsonUtility.ToJson(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(BuildUrl(path), "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                Finish(request, onDone);
            }
        }

        private string BuildUrl(string path)
        {
            string url = baseUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = DefaultBaseUrl;
            }

            url = url.TrimEnd('/');
            return path.StartsWith("/", StringComparison.Ordinal) ? url + path : url + "/" + path;
        }

        private static void Finish<T>(UnityWebRequest request, Action<ApiResult<T>> onDone)
        {
            ApiResult<T> result = new ApiResult<T>
            {
                Success = request.result == UnityWebRequest.Result.Success,
                StatusCode = request.responseCode,
                RawBody = request.downloadHandler != null ? request.downloadHandler.text : string.Empty,
            };

            if (result.Success)
            {
                try
                {
                    result.Data = JsonUtility.FromJson<T>(result.RawBody);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = "Failed to parse response: " + ex.Message;
                }
            }
            else
            {
                result.Error = ExtractError(request);
            }

            if (onDone != null)
            {
                onDone(result);
            }
        }

        private static string ExtractError(UnityWebRequest request)
        {
            string body = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    ApiContracts.ApiError error = JsonUtility.FromJson<ApiContracts.ApiError>(body);
                    if (error != null && !string.IsNullOrEmpty(error.detail))
                    {
                        return error.detail;
                    }
                }
                catch (Exception)
                {
                    // Fall through to the generic message below.
                }
            }

            return !string.IsNullOrEmpty(request.error)
                ? request.error
                : "HTTP " + request.responseCode;
        }

        // ------------------------------------------------------------------
        // Health
        // ------------------------------------------------------------------

        /// <summary>GET /health - returns true when the backend is reachable.</summary>
        public Coroutine CheckHealth(Action<ApiResult<bool>> onDone)
        {
            return StartCoroutine(GetJson<bool>("/health", onDone));
        }

        // ------------------------------------------------------------------
        // Workers (docs/api/API.md #1, #2)
        // ------------------------------------------------------------------

        public Coroutine CreateWorker(ApiContracts.WorkerCreate payload, Action<ApiResult<ApiContracts.WorkerOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.WorkerCreate, ApiContracts.WorkerOut>("/workers", payload, onDone));
        }

        public Coroutine GetWorker(int workerId, Action<ApiResult<ApiContracts.WorkerOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.WorkerOut>("/workers/" + workerId, onDone));
        }

        // ------------------------------------------------------------------
        // Modules (docs/api/API.md #3)
        // ------------------------------------------------------------------

        public Coroutine ListModules(Action<ApiResult<ApiContracts.ModuleListOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.ModuleListOut>("/modules", onDone));
        }

        // ------------------------------------------------------------------
        // Progress (docs/api/API.md #4, #5)
        // ------------------------------------------------------------------

        public Coroutine GetProgress(int workerId, Action<ApiResult<ApiContracts.ProgressListOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.ProgressListOut>("/progress/" + workerId, onDone));
        }

        public Coroutine UpdateProgress(ApiContracts.ProgressCreate payload, Action<ApiResult<ApiContracts.ProgressOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.ProgressCreate, ApiContracts.ProgressOut>("/progress", payload, onDone));
        }

        // ------------------------------------------------------------------
        // Assessments (docs/api/API.md #6, #7, #8)
        // ------------------------------------------------------------------

        public Coroutine SubmitAssessment(ApiContracts.AssessmentCreate payload, Action<ApiResult<ApiContracts.AssessmentOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.AssessmentCreate, ApiContracts.AssessmentOut>("/assessments", payload, onDone));
        }

        public Coroutine GetAssessmentHistory(int workerId, Action<ApiResult<ApiContracts.AssessmentHistoryOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.AssessmentHistoryOut>("/assessments/" + workerId, onDone));
        }

        public Coroutine GetLatestAssessment(int workerId, Action<ApiResult<ApiContracts.AssessmentOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.AssessmentOut>("/assessments/" + workerId + "/latest", onDone));
        }

        // ------------------------------------------------------------------
        // Sync (docs/api/API.md #9, #10)
        // ------------------------------------------------------------------

        public Coroutine SyncSessions(ApiContracts.SyncCreate payload, Action<ApiResult<ApiContracts.SyncOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.SyncCreate, ApiContracts.SyncOut>("/sync", payload, onDone));
        }

        public Coroutine GetSyncStatus(int workerId, Action<ApiResult<ApiContracts.SyncStatusOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.SyncStatusOut>("/sync/status/" + workerId, onDone));
        }

        // ------------------------------------------------------------------
        // Certificates (docs/api/API.md #11, #12, #13)
        // ------------------------------------------------------------------

        public Coroutine IssueCertificate(ApiContracts.CertificateCreate payload, Action<ApiResult<ApiContracts.CertificateOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.CertificateCreate, ApiContracts.CertificateOut>("/certificates", payload, onDone));
        }

        public Coroutine GetWorkerCertificates(int workerId, Action<ApiResult<ApiContracts.CertificateListOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.CertificateListOut>("/certificates/" + workerId, onDone));
        }

        public Coroutine VerifyCertificate(string certificateNumber, Action<ApiResult<ApiContracts.CertificateVerifyOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.CertificateVerifyOut>("/certificates/verify/" + Uri.EscapeDataString(certificateNumber), onDone));
        }

        // ------------------------------------------------------------------
        // ML Competency - Retraining (docs/api/API.md #17)
        // ------------------------------------------------------------------

        public Coroutine GetRetrainingPlan(int assessmentId, Action<ApiResult<ApiContracts.RetrainingPlanOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.RetrainingPlanOut>("/assessments/" + assessmentId + "/retraining-plan", onDone));
        }

        // ------------------------------------------------------------------
        // Vision / ML - PPE detection (docs/api/API.md #18, #19)
        // ------------------------------------------------------------------

        public Coroutine CheckPPE(ApiContracts.PPECheckRequest payload, Action<ApiResult<ApiContracts.PPECheckOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.PPECheckRequest, ApiContracts.PPECheckOut>("/vision/ppe-check", payload, onDone));
        }

        public Coroutine GetVisionStatus(Action<ApiResult<ApiContracts.VisionStatusOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.VisionStatusOut>("/vision/status", onDone));
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\AlarmInteraction.cs

```csharp
// Stub only - the live implementation of AlarmInteraction lives in:
//     Assets/AR_Fire_foundation/scripts/Scenario/AlarmInteraction.cs

```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\AR_raycast_manager.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class FireScenarioARPlacement : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    [Header("Fire Scenario")]
    public Transform fireScenario;

    [Header("Placement Indicator")]
    public float circleSize = 0.35f;
    public float circleHeight = 0.015f;
    public float circleThickness = 0.025f;
    public int circleSegments = 64;

    [Header("Circle Appearance")]
    public Color circleColor = new Color(0f, 0.5f, 1f, 0.9f);

    [Header("Behaviour")]
    public bool useARAnchor = true;
    public bool hidePlanesAfterPlacement = true;

    // =====================================================
    // EVENTS
    // =====================================================

    /// <summary>Raised once, the moment the scenario is placed & locked.</summary>
    public UnityEvent OnScenarioPlaced = new UnityEvent();

    /// <summary>Raised when SetPlacementActive is called.</summary>
    public UnityEvent<bool> OnPlacementActiveChanged = new UnityEvent<bool>();

    public bool IsPlaced { get; private set; }

    public bool PlacementActive { get; private set; } = true;

    private GameObject placementCircle;
    private LineRenderer circleRenderer;
    private ARPlaneManager planeManager;

    private static List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    /// <summary>
    /// Enable/disable the "blue circle + tap to place" system.
    /// The FlowManager keeps it disabled during the intro card and
    /// disables it permanently after the scenario is placed.
    /// </summary>
    public void SetPlacementActive(bool active)
    {
        PlacementActive = active;

        if (!active && placementCircle != null)
        {
            placementCircle.SetActive(false);
        }

        if (OnPlacementActiveChanged != null)
        {
            OnPlacementActiveChanged.Invoke(active);
        }
    }


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // Find AR Camera automatically
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main;
        }

        // Find Raycast Manager automatically
        if (raycastManager == null)
        {
            raycastManager =
                FindFirstObjectByType<ARRaycastManager>();
        }

        // Plane manager lives on the same XR Origin
        if (planeManager == null)
        {
            planeManager =
                GetComponent<ARPlaneManager>();
        }

        if (raycastManager == null)
        {
            Debug.LogError(
                "ARRaycastManager not found!"
            );

            return;
        }

        if (arCamera == null)
        {
            Debug.LogError(
                "AR Camera not assigned!"
            );

            return;
        }

        // Fire scenario can also be found by name (works when inactive).
        if (fireScenario == null)
        {
            GameObject found = GameObject.Find("FireScenario");
            if (found != null)
            {
                fireScenario = found.transform;
            }
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "Fire Scenario is not assigned!"
            );

            return;
        }

        // Scenario must NOT follow camera
        fireScenario.gameObject.SetActive(false);

        // Create blue placement circle
        CreatePlacementCircle();
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        // Once placed, do absolutely nothing.
        //
        // The scenario remains fixed in the real world.

        if (IsPlaced)
            return;

        if (!PlacementActive)
            return;

        UpdatePlacementCircle();

        HandleTouch();
    }


    // =====================================================
    // CREATE BLUE CIRCLE
    // =====================================================

    private void CreatePlacementCircle()
    {
        placementCircle =
            new GameObject("PlacementCircle");

        circleRenderer =
            placementCircle.AddComponent<LineRenderer>();

        circleRenderer.loop = true;

        circleRenderer.positionCount =
            circleSegments;

        circleRenderer.startWidth =
            circleThickness;

        circleRenderer.endWidth =
            circleThickness;

        circleRenderer.useWorldSpace = true;

        circleRenderer.material =
            CreateCircleMaterial();

        circleRenderer.startColor =
            circleColor;

        circleRenderer.endColor =
            circleColor;

        placementCircle.SetActive(false);
    }


    // =====================================================
    // CREATE SIMPLE MATERIAL FOR CIRCLE
    // =====================================================

    private Material CreateCircleMaterial()
    {
        Shader shader =
            Shader.Find("Sprites/Default");

        if (shader == null)
        {
            shader =
                Shader.Find("Unlit/Color");
        }

        Material material =
            new Material(shader);

        material.color =
            circleColor;

        return material;
    }


    // =====================================================
    // UPDATE FLOOR CIRCLE
    // =====================================================

    private void UpdatePlacementCircle()
    {
        if (arCamera == null ||
            raycastManager == null)
        {
            return;
        }

        // Screen center of phone
        Vector2 screenCenter =
            new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );

        // Raycast only against detected horizontal planes
        if (raycastManager.Raycast(
            screenCenter,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose =
                hits[0].pose;

            placementCircle.SetActive(true);

            DrawCircle(hitPose);
        }
        else
        {
            placementCircle.SetActive(false);
        }
    }


    // =====================================================
    // DRAW CIRCLE ON FLOOR
    // =====================================================

    private void DrawCircle(Pose pose)
    {
        Vector3 center =
            pose.position +
            pose.up * circleHeight;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle =
                ((float)i / circleSegments) *
                Mathf.PI * 2f;

            float x =
                Mathf.Cos(angle) *
                circleSize;

            float z =
                Mathf.Sin(angle) *
                circleSize;

            Vector3 point =
                center +
                pose.right * x +
                pose.forward * z;

            circleRenderer.SetPosition(
                i,
                point
            );
        }
    }


    // =====================================================
    // NEW INPUT SYSTEM - TOUCH
    // =====================================================

    private void HandleTouch()
    {
        // ---------------- Android touch ----------------
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press
                .wasPressedThisFrame)
        {
            Vector2 touchPosition =
                Touchscreen.current.primaryTouch.position
                    .ReadValue();

            TryPlaceScenario(touchPosition);
            return;
        }

        // ---------------- Editor mouse ----------------
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            TryPlaceScenario(mousePosition);
        }
    }


    // =====================================================
    // PLACE SCENARIO
    // =====================================================

    private void TryPlaceScenario(
        Vector2 screenPosition
    )
    {
        if (raycastManager == null)
            return;

        // Check whether the worker tapped a detected floor
        if (!raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            return;
        }

        Pose placementPose =
            hits[0].pose;

        PlaceScenario(
            placementPose
        );
    }


    // =====================================================
    // FINAL SCENARIO PLACEMENT
    // =====================================================

    private void PlaceScenario(
        Pose placementPose
    )
    {
        if (fireScenario == null)
            return;

        // =================================================
        // POSITION
        // =================================================

        fireScenario.position =
            placementPose.position;


        // =================================================
        // CAPTURE CAMERA FORWARD DIRECTION
        // ONLY AT THIS MOMENT
        // =================================================

        Vector3 cameraForward =
            arCamera.transform.forward;

        // Keep direction parallel to the floor.
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude >
            0.001f)
        {
            cameraForward.Normalize();

            fireScenario.rotation =
                Quaternion.LookRotation(
                    cameraForward,
                    placementPose.up
                );
        }
        else
        {
            // Fallback if camera direction
            // cannot be calculated.
            fireScenario.rotation =
                Quaternion.LookRotation(
                    placementPose.forward,
                    placementPose.up
                );
        }


        // =================================================
        // SHOW COMPLETE SCENARIO
        // =================================================

        fireScenario.gameObject.SetActive(true);


        // =================================================
        // HIDE PLACEMENT CIRCLE
        // =================================================

        placementCircle.SetActive(false);


        // =================================================
        // WORLD LOCK - REAL-WORLD ANCHOR
        // =================================================

        if (useARAnchor)
        {
            AddAnchorToScenario();
        }

        // =================================================
        // HIDE DETECTED PLANES (optional)
        // =================================================

        if (hidePlanesAfterPlacement)
        {
            HidePlanes();
        }


        // =================================================
        // LOCK PLACEMENT FOREVER
        // =================================================

        IsPlaced = true;

        Debug.Log(
            "FIRE SCENARIO PLACED AND LOCKED."
        );

        if (OnScenarioPlaced != null)
        {
            OnScenarioPlaced.Invoke();
        }
    }


    // =====================================================
    // ADD AR ANCHOR (world locking)
    // =====================================================

    private void AddAnchorToScenario()
    {
        if (fireScenario == null)
            return;

        ARAnchor anchor =
            fireScenario.GetComponent<ARAnchor>();

        if (anchor == null)
        {
            try
            {
                anchor =
                    fireScenario.gameObject.AddComponent<ARAnchor>();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    "FireScenarioARPlacement: could not add ARAnchor - " +
                    ex.Message
                );
            }
        }
    }


    // =====================================================
    // HIDE PLANES (after placement)
    // =====================================================

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        if (planeManager.trackables == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        planeManager.enabled = false;
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\DryPowderSpray.cs

```csharp
using UnityEngine;

public class DryPowderSpray : MonoBehaviour
{
    [Header("Spray Point")]
    public Transform sprayPoint;

    private ParticleSystem spray;

    public bool IsSpraying { get; private set; }

    private void Awake()
    {
        spray = GetComponent<ParticleSystem>();

        if (spray == null)
        {
            Debug.LogError("DryPowderSpray: Particle System not found.");
            return;
        }

        IsSpraying = false;

        spray.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    public void StartSpray()
    {
        if (spray == null)
            return;

        // --------------------------------
        // CONNECT SPRAY TO SPRAY POINT
        // --------------------------------

        if (sprayPoint != null)
        {
            transform.SetPositionAndRotation(
                sprayPoint.position,
                sprayPoint.rotation
            );
        }

        if (!spray.isPlaying)
            spray.Play();

        IsSpraying = true;

        Debug.Log("SPRAY STARTED");
    }

    public void StopSpray()
    {
        if (spray == null)
            return;

        spray.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        IsSpraying = false;

        Debug.Log("SPRAY STOPPED");
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\ExtingguisherGripInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ExtinguisherGripInteraction : MonoBehaviour
{
    [Header("Safety Pin")]
    public GameObject pin;

    [Header("Powder Spray")]
    public DryPowderSpray powderSpray;

    [Header("Spray Audio")]
    public AudioSource sprayAudio;

    public bool IsGripHeld { get; private set; }

    /// <summary>Raised when the grip is pressed and the spray starts.</summary>
    public UnityEvent OnSprayStarted = new UnityEvent();

    /// <summary>Raised when the grip is released and the spray stops.</summary>
    public UnityEvent OnSprayStopped = new UnityEvent();

    /// <summary>
    /// Raised when the worker tries to squeeze the handle while the
    /// safety pin is still in place (wrong order).
    /// </summary>
    public UnityEvent OnPinRemovalRequired = new UnityEvent();

    private Camera arCamera;

    private void Awake()
    {
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError(
                "ExtinguisherGripInteraction: Main Camera not found."
            );
        }
    }

    private void Start()
    {
        // Audio should only play while the grip is held.
        if (sprayAudio != null)
        {
            sprayAudio.playOnAwake = false;
            sprayAudio.loop = true;
            sprayAudio.Stop();
        }

        // Make sure the spray is stopped when the scene starts.
        if (powderSpray != null)
        {
            powderSpray.StopSpray();
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        StopGrip();
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (arCamera == null)
            return;

        // No finger on screen.
        if (Touch.activeTouches.Count == 0)
        {
            if (IsGripHeld)
                StopGrip();

            return;
        }

        Touch touch = Touch.activeTouches[0];

        // -----------------------------------------
        // FINGER PRESSED
        // -----------------------------------------

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(
                touch.screenPosition
            );

            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                // Allows the Grip itself OR a child collider
                // of the Grip object to activate the handle.
                if (hit.collider.gameObject == gameObject ||
                    hit.collider.transform.IsChildOf(transform))
                {
                    StartGrip();
                    break;
                }
            }
        }

        // -----------------------------------------
        // FINGER RELEASED
        // -----------------------------------------

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            if (IsGripHeld)
                StopGrip();
        }
    }

    private void StartGrip()
    {
        // -----------------------------------------
        // SAFETY PIN CHECK
        // -----------------------------------------

        if (pin != null && pin.activeSelf)
        {
            Debug.Log("wrong_action");
            Debug.Log("Remove the safety pin first.");

            if (OnPinRemovalRequired != null)
            {
                OnPinRemovalRequired.Invoke();
            }

            return;
        }

        // Already spraying.
        if (IsGripHeld)
            return;

        IsGripHeld = true;

        Debug.Log("critical_action");
        Debug.Log("Grip held - spray activated.");

        // -----------------------------------------
        // START EVENT
        // -----------------------------------------

        if (OnSprayStarted != null)
        {
            OnSprayStarted.Invoke();
        }

        // -----------------------------------------
        // START POWDER
        // -----------------------------------------

        if (powderSpray != null)
        {
            powderSpray.StartSpray();
        }
        else
        {
            Debug.LogError(
                "ExtinguisherGripInteraction: Powder Spray is not assigned."
            );
        }

        // -----------------------------------------
        // START AUDIO
        // -----------------------------------------

        if (sprayAudio != null &&
            !sprayAudio.isPlaying)
        {
            sprayAudio.Play();
        }
    }

    private void StopGrip()
    {
        if (!IsGripHeld)
            return;

        IsGripHeld = false;

        // -----------------------------------------
        // STOP POWDER
        // -----------------------------------------

        if (powderSpray != null)
        {
            powderSpray.StopSpray();
        }

        // -----------------------------------------
        // STOP AUDIO
        // -----------------------------------------

        if (sprayAudio != null &&
            sprayAudio.isPlaying)
        {
            sprayAudio.Stop();
        }

        Debug.Log("Grip released - spray stopped.");

        if (OnSprayStopped != null)
        {
            OnSprayStopped.Invoke();
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\ExtinguisherAutoMove.cs

```csharp
using UnityEngine;
using System.Collections;

public class ExtinguisherAutoMove : MonoBehaviour
{
    [Header("Extinguisher Target")]
    public Transform extinguisherTarget;

    [Header("Movement")]
    public float moveDuration = 1f;

    private bool hasMoved = false;

    public void MoveToTarget()
    {
        if (hasMoved)
            return;

        if (extinguisherTarget == null)
        {
            Debug.LogError("Extinguisher Target is not assigned.");
            return;
        }

        hasMoved = true;
        StartCoroutine(MoveExtinguisher());
    }

    private IEnumerator MoveExtinguisher()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = extinguisherTarget.position;

        // Keep whatever rotation FireExt already has.
        Quaternion fixedRotation = transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            transform.rotation = fixedRotation;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = fixedRotation;

        Debug.Log("Extinguisher moved and kept correct direction.");
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\ExtinguisherPickup.cs

```csharp
using UnityEngine;
using UnityEngine.Events;

public class ExtinguisherPickup : MonoBehaviour
{
    [Header("AR Camera")]
    public Transform arCamera;

    [Header("Position While Holding")]
    public Vector3 holdPosition =
        new Vector3(0f, -0.20f, 0.80f);

    [Header("Rotation While Holding")]
    public Vector3 holdRotation =
        new Vector3(0f, 0f, 0f);

    [Header("Hose References")]
    public Transform hose;

    public Transform hosePivot;

    public Transform sprayPoint;

    // Spray point position relative to hose.
    private Vector3 sprayPointLocalPosition;

    private bool isHeld = false;

    /// <summary>
    /// Raised the moment the extinguisher is attached to the AR camera.
    /// </summary>
    public UnityEvent OnAttachedToCamera = new UnityEvent();


    void Awake()
    {
        // Find AR Camera
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        // Automatically find hose
        if (hose == null)
        {
            hose = transform.Find("Hose");
        }

        // Automatically find hose pivot
        if (hosePivot == null)
        {
            hosePivot = transform.Find("hosepivot");
        }

        // Automatically find spray point
        if (sprayPoint == null)
        {
            sprayPoint = transform.Find("spraypoint");
        }

        // =================================================
        // CALCULATE SPRAY POINT POSITION RELATIVE TO HOSE
        // =================================================

        if (hose != null && sprayPoint != null)
        {
            sprayPointLocalPosition =
                hose.InverseTransformPoint(
                    sprayPoint.position
                );
        }
        else
        {
            Debug.LogError(
                "Hose or Spray Point is missing!"
            );
        }
    }


    // =====================================================
    // CALLED BY ExtinguisherDisplayPickup
    // =====================================================

    public void AttachToCamera()
    {
        if (isHeld)
            return;

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (arCamera == null)
        {
            Debug.LogError(
                "AR Camera not found!"
            );
            return;
        }

        if (hose == null)
        {
            Debug.LogError(
                "Hose reference is missing!"
            );
            return;
        }

        if (hosePivot == null)
        {
            Debug.LogError(
                "Hose Pivot reference is missing!"
            );
            return;
        }

        if (sprayPoint == null)
        {
            Debug.LogError(
                "Spray Point reference is missing!"
            );
            return;
        }

        isHeld = true;

        // =================================================
        // ATTACH COMPLETE ORIGINAL TO AR CAMERA
        // =================================================

        transform.SetParent(
            arCamera,
            true
        );

        // =================================================
        // POSITION
        // =================================================

        transform.localPosition =
            holdPosition;

        // =================================================
        // ROTATION WHILE HOLDING
        // =================================================

        transform.localRotation =
            Quaternion.Euler(
                holdRotation
            );

        // =================================================
        // RECALCULATE SPRAY POINT
        // =================================================

        UpdateSprayPoint();

        Debug.Log(
            "ORIGINAL EXTINGUISHER ATTACHED + " +
            "POSITION + ROTATION APPLIED"
        );

        if (OnAttachedToCamera != null)
        {
            OnAttachedToCamera.Invoke();
        }
    }


    // =====================================================
    // SPRAY POINT CALCULATION
    // =====================================================

    public void UpdateSprayPoint()
    {
        if (hose == null ||
            sprayPoint == null)
        {
            return;
        }

        // Keep spray point at its original position
        // relative to the hose.

        sprayPoint.position =
            hose.TransformPoint(
                sprayPointLocalPosition
            );
    }


    // =====================================================
    // OPTIONAL: CALL THIS IF HOSE IS MOVED LATER
    // =====================================================

    public void RecalculateSprayPoint()
    {
        UpdateSprayPoint();
    }


    public bool IsHeld()
    {
        return isHeld;
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\FireEquipmentInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FireEquipmentInteraction : MonoBehaviour
{
    [Header("Complete Fire Scenario")]
    public GameObject fireScenario;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private readonly System.Collections.Generic.List<ARRaycastHit> hits =
        new System.Collections.Generic.List<ARRaycastHit>();

    private bool scenarioPlaced = false;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "FireEquipmentInteraction: ARRaycastManager not found."
            );
        }

        if (planeManager == null)
        {
            Debug.LogError(
                "FireEquipmentInteraction: ARPlaneManager not found."
            );
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "FireEquipmentInteraction: Fire Scenario is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        if (fireScenario != null)
        {
            fireScenario.SetActive(false);
        }
    }

    private void Update()
    {
        // Scenario can only be placed once.
        if (scenarioPlaced)
            return;

        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        bool foundPlane = raycastManager.Raycast(
            touch.screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        if (!foundPlane)
            return;

        PlaceScenario(hits[0].pose);
    }

    private void PlaceScenario(Pose hitPose)
    {
        if (fireScenario == null)
            return;

        // ---------------------------------------
        // POSITION
        // ---------------------------------------

        fireScenario.transform.position = hitPose.position;

        // ---------------------------------------
        // ROTATION
        // Keep the scenario upright.
        // ---------------------------------------

        fireScenario.transform.rotation =
            Quaternion.Euler(
                0f,
                hitPose.rotation.eulerAngles.y,
                0f
            );

        // ---------------------------------------
        // ACTIVATE SCENARIO
        // ---------------------------------------

        fireScenario.SetActive(true);

        // ---------------------------------------
        // ADD ANCHOR
        // ---------------------------------------

        ARAnchor anchor =
            fireScenario.GetComponent<ARAnchor>();

        if (anchor == null)
        {
            anchor = fireScenario.AddComponent<ARAnchor>();
        }

        // ---------------------------------------
        // MARK AS PLACED
        // ---------------------------------------

        scenarioPlaced = true;

        // ---------------------------------------
        // HIDE DETECTED PLANES
        // ---------------------------------------

        HidePlanes();

        Debug.Log(
            "COMPLETE FIRE SCENARIO PLACED AND ANCHORED."
        );
    }

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        planeManager.enabled = false;
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\fireext_display.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ExtinguisherDisplayPickup : MonoBehaviour
{
    [Header("Original Functional Extinguisher")]
    public GameObject originalExtinguisher;

    [Header("AR Camera")]
    public Transform arCamera;

    private bool pickedUp = false;

    /// <summary>
    /// Raised the moment the worker taps the display (fake) extinguisher.
    /// The hidden original is then attached to the AR camera.
    /// </summary>
    public UnityEvent OnPickedUp = new UnityEvent();

    void Start()
    {
        // Find AR Camera automatically
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        // Original is hidden when the app starts
        if (originalExtinguisher != null)
        {
            originalExtinguisher.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "Original Extinguisher is not assigned!"
            );
        }
    }

    void Update()
    {
        if (pickedUp)
            return;

        // =================================================
        // NEW INPUT SYSTEM - TOUCH
        // =================================================

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            TryPickup(touchPosition);

            return;
        }

        // =================================================
        // NEW INPUT SYSTEM - MOUSE
        // Editor testing only
        // =================================================

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            TryPickup(mousePosition);
        }
    }

    void TryPickup(Vector2 screenPosition)
    {
        Camera cam = null;

        if (arCamera != null)
        {
            cam = arCamera.GetComponent<Camera>();
        }

        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam == null)
        {
            Debug.LogError("Camera not found!");
            return;
        }

        Ray ray =
            cam.ScreenPointToRay(screenPosition);

        RaycastHit[] hits =
            Physics.RaycastAll(ray, 100f);

        foreach (RaycastHit hit in hits)
        {
            Transform hitObject = hit.transform;

            // Display object or any child
            if (hitObject == transform ||
                hitObject.IsChildOf(transform))
            {
                Pickup();

                return;
            }
        }
    }

    void Pickup()
    {
        if (pickedUp)
            return;

        if (originalExtinguisher == null)
        {
            Debug.LogError(
                "Original Extinguisher is not assigned!"
            );
            return;
        }

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (arCamera == null)
        {
            Debug.LogError(
                "AR Camera is missing!"
            );
            return;
        }

        // Get the original's pickup script
        ExtinguisherPickup pickup =
            originalExtinguisher.GetComponent<ExtinguisherPickup>();

        if (pickup == null)
        {
            Debug.LogError(
                "ExtinguisherPickup is missing " +
                "from FireExt_Original!"
            );
            return;
        }

        pickedUp = true;

        // Give original the AR camera
        pickup.arCamera = arCamera;

        // Show original
        originalExtinguisher.SetActive(true);

        // Let original attach itself
        pickup.AttachToCamera();

        // Hide display AFTER original is activated
        gameObject.SetActive(false);

        Debug.Log(
            "DISPLAY TAPPED -> ORIGINAL EXTINGUISHER ACTIVATED"
        );

        if (OnPickedUp != null)
        {
            OnPickedUp.Invoke();
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\FireExtinguishable1.cs

```csharp
using UnityEngine;
using UnityEngine.Events;

public class FireExtinguishable : MonoBehaviour
{
    public enum DetectionMode
    {
        /// <summary>Uses ParticleSystem collision events (needs the collision
        /// module enabled on the fire particles and on the spray).</summary>
        PhysicsCollision,

        /// <summary>Uses aim-based check: spray direction + distance toward
        /// the fire. Works reliably on any Android device.</summary>
        AimCheck,

        /// <summary>Both methods (recommended).</summary>
        Both
    }

    [Header("Fire Settings")]
    public float extinguishTime = 10.0f;

    [Header("Fire Particle")]
    public ParticleSystem fireParticle;

    [Header("Powder Detection")]
    public float powderContactTimeout = 0.2f;

    [Header("Detection")]
    public DetectionMode detectionMode = DetectionMode.Both;

    /// <summary>Which powder spray feeds the fire. Auto-found if empty.</summary>
    public DryPowderSpray powderSpray;

    /// <summary>Max distance the spray can reach the fire.</summary>
    public float sprayRange = 5f;

    /// <summary>Max angle between the spray direction and the fire.</summary>
    public float maxAimAngle = 20f;

    /// <summary>
    /// Raised once, the moment the fire is fully extinguished.
    /// </summary>
    public UnityEvent OnExtinguished = new UnityEvent();

    public bool IsExtinguished { get; private set; }

    /// <summary>True while powder is currently reaching the fire.</summary>
    public bool IsBeingSprayed
    {
        get
        {
            return !IsExtinguished &&
                   lastPowderHitTime >= 0f &&
                   Time.time - lastPowderHitTime <= powderContactTimeout;
        }
    }

    /// <summary>0..1 how far the continuous spraying has progressed.</summary>
    public float SprayProgress01
    {
        get
        {
            if (extinguishTime <= 0f)
                return 1f;

            return Mathf.Clamp01(
                powderHitTime / extinguishTime
            );
        }
    }

    private float powderHitTime = 0f;
    private float lastPowderHitTime = -1f;
    private float nextPowderSearchTime = -1f;

    private void Update()
    {
        if (IsExtinguished)
            return;

        EnsurePowderSprayReference();

        // Check whether powder has recently hit the fire.
        bool beingSprayed = IsBeingSprayed;

        // Aim check is the robust default - it works without
        // configuring collision modules on the particle systems.
        if (!beingSprayed &&
            (detectionMode == DetectionMode.AimCheck ||
             detectionMode == DetectionMode.Both))
        {
            if (CheckAimedAtFire())
            {
                beingSprayed = true;
                lastPowderHitTime = Time.time;
            }
        }

        if (beingSprayed)
        {
            powderHitTime += Time.deltaTime;

            if (powderHitTime >= extinguishTime)
            {
                ExtinguishFire();
            }
        }
        else
        {
            // Powder stopped reaching the fire - reset progress.
            powderHitTime = 0f;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (IsExtinguished)
            return;

        if (detectionMode == DetectionMode.AimCheck)
            return;

        // -----------------------------------------
        // CHECK COLLIDING OBJECT
        // -----------------------------------------

        DryPowderSpray powder =
            other.GetComponent<DryPowderSpray>();

        // If the component isn't directly on the
        // collided object, check its parent.
        if (powder == null)
        {
            powder =
                other.GetComponentInParent<DryPowderSpray>();
        }

        if (powder == null)
            return;

        // Make sure this is an actively spraying system.
        if (!powder.IsSpraying)
            return;

        lastPowderHitTime = Time.time;

        Debug.Log("Powder hit fire.");
    }


    // =====================================================
    // AIM-BASED DETECTION (robust fallback)
    // =====================================================

    private void EnsurePowderSprayReference()
    {
        if (powderSpray != null)
            return;

        // The sprayer only becomes active AFTER the worker picks up the
        // extinguisher, so we must retry the search periodically.
        if (Time.time < nextPowderSearchTime)
            return;

        nextPowderSearchTime = Time.time + 0.5f;

        if (detectionMode == DetectionMode.AimCheck ||
            detectionMode == DetectionMode.Both)
        {
            powderSpray = FindFirstObjectByType<DryPowderSpray>();
        }
    }

    private bool CheckAimedAtFire()
    {
        if (powderSpray == null || !powderSpray.IsSpraying)
            return false;

        if (powderSpray.sprayPoint == null)
            return false;

        Vector3 sprayOrigin = powderSpray.sprayPoint.position;
        Vector3 sprayDirection = powderSpray.sprayPoint.forward;

        Vector3 fireAnchor =
            fireParticle != null
                ? fireParticle.transform.position
                : transform.position;

        Vector3 toFire = fireAnchor - sprayOrigin;

        float distance = toFire.magnitude;

        if (distance > sprayRange)
            return false;

        // Accept both +forward and -forward: different extinguisher
        // models may emit powder along either axis of their spray point.
        float angleForward = Vector3.Angle(sprayDirection, toFire);
        float angleBack = Vector3.Angle(-sprayDirection, toFire);

        return Mathf.Min(angleForward, angleBack) <= maxAimAngle;
    }


    // =====================================================
    // EXTINGUISH
    // =====================================================

    private void ExtinguishFire()
    {
        if (IsExtinguished)
            return;

        IsExtinguished = true;

        // -----------------------------------------
        // STOP FIRE PARTICLE
        // -----------------------------------------

        if (fireParticle != null)
        {
            fireParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        Debug.Log("critical_action");
        Debug.Log("Fire extinguished!");

        if (OnExtinguished != null)
        {
            OnExtinguished.Invoke();
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\FirePinInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FirePinInteraction : MonoBehaviour
{
    [Header("Extinguisher Movement")]
    public ExtinguisherAutoMove extinguisherAutoMove;

    private Camera arCamera;
    private bool pinRemoved = false;

    /// <summary>
    /// Raised the moment the safety pin is removed.
    /// </summary>
    public UnityEvent OnPinRemoved = new UnityEvent();

    private void Start()
    {
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("FirePinInteraction: AR Camera not found.");
        }

        if (extinguisherAutoMove == null)
        {
            Debug.LogWarning(
                "FirePinInteraction: Extinguisher Auto Move is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (pinRemoved)
            return;

        if (arCamera == null)
            return;

        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        Ray ray = arCamera.ScreenPointToRay(
            touch.screenPosition
        );

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                RemovePin();
                break;
            }
        }
    }

    private void RemovePin()
    {
        if (pinRemoved)
            return;

        pinRemoved = true;

        // Remove the visible safety pin
        gameObject.SetActive(false);

        Debug.Log("equipment_selected");
        Debug.Log("Safety pin removed. Extinguisher is READY.");

        if (OnPinRemoved != null)
        {
            OnPinRemoved.Invoke();
        }

        // Automatically move extinguisher
        if (extinguisherAutoMove != null)
        {
            extinguisherAutoMove.MoveToTarget();
        }
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\FireScenarioAlinment.cs

```csharp
using UnityEngine;

public class FireScenarioAlignment : MonoBehaviour
{
    [Header("SCENARIO ROOT")]
    public Transform scenarioRoot;

    [Header("OBJECT REFERENCES")]

    public Transform electricalBox;

    public Transform fireExtinguisher;

    public Transform fireAlarm;

    public Transform exitSign;

    public Transform extinguisherDisplay;


    // =====================================================
    // ANCHOR POINTS (optional, for visual alignment)
    // =====================================================

    [Header("ANCHOR POINTS (optional)")]

    public Transform electricalBoxPoint;

    public Transform extinguisherPoint;

    public Transform fireAlarmPoint;

    public Transform exitPoint;


    // =====================================================
    // ELECTRICAL BOX
    // =====================================================

    [Header("ELECTRICAL BOX")]

    public Vector3 electricalBoxPosition =
        new Vector3(0f, 0f, 3f);

    public Vector3 electricalBoxRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // FIRE EXTINGUISHER (display - the one that is picked up)
    // =====================================================

    [Header("FIRE EXTINGUISHER")]

    // Right side + slightly behind the electrical box
    public Vector3 fireExtinguisherPosition =
        new Vector3(2.5f, 1.0f, 1.8f);

    public Vector3 fireExtinguisherRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // FIRE ALARM
    // =====================================================

    [Header("FIRE ALARM")]

    // Near the extinguisher
    public Vector3 fireAlarmPosition =
        new Vector3(0f, 1.2f, 0.5f);

    public Vector3 fireAlarmRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // EXIT
    // =====================================================

    [Header("EXIT")]

    // Right side and farther away
    public Vector3 exitSignPosition =
        new Vector3(4.5f, 2.0f, 6.0f);

    public Vector3 exitSignRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // DISPLAY EXTINGUISHER
    // =====================================================

    [Header("EXTINGUISHER DISPLAY")]

    public Vector3 extinguisherDisplayPosition =
        new Vector3(2.5f, 1.0f, 1.8f);

    public Vector3 extinguisherDisplayRotation =
        new Vector3(0f, 0f, 0f);


    [Header("BEHAVIOUR")]

    public bool applyOnEnable = true;

    public bool syncAnchors = true;

    private bool appliedFromStart;

    private void OnEnable()
    {
        // Robust against execution-order issues:
        // alignment is applied whenever the scenario becomes active.
        if (applyOnEnable)
        {
            ApplyAlignment();
        }
    }

    private void Start()
    {
        if (!appliedFromStart)
        {
            ApplyAlignment();

            appliedFromStart = true;
        }
    }


    // =====================================================
    // APPLY ALIGNMENT
    // =====================================================

    public void ApplyAlignment()
    {
        SetTransform(
            electricalBox,
            electricalBoxPosition,
            electricalBoxRotation
        );

        SetTransform(
            fireExtinguisher,
            fireExtinguisherPosition,
            fireExtinguisherRotation
        );

        SetTransform(
            fireAlarm,
            fireAlarmPosition,
            fireAlarmRotation
        );

        SetTransform(
            exitSign,
            exitSignPosition,
            exitSignRotation
        );

        SetTransform(
            extinguisherDisplay,
            extinguisherDisplayPosition,
            extinguisherDisplayRotation
        );

        if (syncAnchors)
        {
            SyncAnchor(electricalBoxPoint, electricalBox);
            SyncAnchor(extinguisherPoint, fireExtinguisher);
            SyncAnchor(fireAlarmPoint, fireAlarm);
            SyncAnchor(exitPoint, exitSign);
        }
    }


    // =====================================================
    // SET OBJECT
    // =====================================================

    private void SetTransform(
        Transform target,
        Vector3 position,
        Vector3 rotation
    )
    {
        if (target == null)
        {
            Debug.LogWarning(
                "FireScenarioAlignment: " +
                "An object is not assigned."
            );

            return;
        }

        // IMPORTANT:
        // Do NOT change the parent.
        //
        // This preserves your existing hierarchy,
        // including Hose, spraypoint, hosepivot,
        // Fire particles, etc.

        target.localPosition =
            position;

        target.localRotation =
            Quaternion.Euler(
                rotation
            );
    }


    // =====================================================
    // SYNC ANCHOR (optional visual helpers)
    // =====================================================

    private void SyncAnchor(
        Transform anchor,
        Transform target
    )
    {
        if (anchor == null || target == null)
            return;

        anchor.position = target.position;
        anchor.rotation = target.rotation;
    }


    // =====================================================
    // UNITY INSPECTOR MENU
    // =====================================================

    [ContextMenu("Apply Alignment")]
    private void ApplyAlignmentFromInspector()
    {
        ApplyAlignment();
    }

    [ContextMenu("Apply Alignment + Anchors")]
    private void ApplyAlignmentAndAnchorsFromInspector()
    {
        syncAnchors = true;
        ApplyAlignment();
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\FireScenarioFlowManager.cs

```csharp
    using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


/// <summary>
/// FireScenarioFlowManager
/// ========================
/// The brain of the AR fire-safety training flow.
///
/// It drives the worker through the complete milestone:
///
///   1. INTRO card          -> tap anywhere
///   2. SCANNING hint       -> move phone, blue circle appears on floor
///   3. TAP ON circle       -> FireScenario placed + world-locked
///   4. Mission message     -> find fire extinguisher
///   5. Pick-up guide       -> tap display extinguisher (the fake)
///   6. Pin guide           -> remove safety pin
///   7. Aim + spray guide   -> hold handle, spray the fire
///   8. Spray timer         -> keep powder on fire until it goes out
///   9. SUCCESS card
///  10. COMPLETE card
///
/// It communicates only through public events raised by the existing
/// interaction scripts. It never moves/rotates/parents the scenario,
/// so the scenario stays fixed in the real world.
///
/// Attach this to the XR Origin (or any always-active object).
/// </summary>
public class FireScenarioFlowManager : MonoBehaviour
{
        public enum Stage
    {
        Intro,
        Scanning,
        MissionIntro,
        AlarmRequired,
        AlarmActivated,
        PickupGuide,
        PinGuide,
        AimGuide,
        Spraying,
        Success,
        Complete
    }

    [Header("Cross References")]
    public GameObject fireScenario;
    public FireScenarioARPlacement placement;
    public ExtinguisherDisplayPickup displayPickup;
    public ExtinguisherPickup originalPickup;
    public FirePinInteraction pinInteraction;
    public ExtinguisherGripInteraction gripInteraction;
    public FireExtinguishable fire;
    public FireScenarioUIController ui;

    [Header("Alarm (resolved at runtime)")]
    public AlarmInteraction alarmInteraction;
    public TapFeedbackZone tapFeedback;

    [Header("Step Indicator")]
    [Tooltip("Total numbered steps shown as STEP X / N.")]
    public int totalSteps = 6;


    [Header("Intro Card")]
    public string introTitle = "SURAKSHAAR";
    public string introBody =
        "AR FIRE-SAFETY TRAINING\n\n" +
        "In this experience a real floor is detected and an AR scenario is placed on it.\n" +
        "An ELECTRICAL FIRE breaks out.\nYour job: act safely and put the fire out.\n" +
        "\nFollow the cards at the bottom of the screen.";
    public string introFooter = "TAP ANYWHERE TO CONTINUE";

    [Header("Scanning")]
    public string scanHint =
        "Move your phone slowly to scan the floor.\n" +
        "When the BLUE circle appears, TAP it to place the scenario.";

        [Header("Mission")]
    public string missionHint =
        "ELECTRICAL FIRE DETECTED\n" +
        "An electrical box is on fire. ACTIVATE THE ALARM first,\n" +
        "then find the fire extinguisher and put the fire out.";
    public string alarmHint =
        "STEP 1 OF 6:\nACTIVATE THE ALARM.\nTap the red fire alarm to alert everyone.";
    public string extinguisherLockedHint =
        "Please activate the alarm first.";
    public string pickupHint =
        "STEP 2 OF 6:\nWalk to the FIRE EXTINGUISHER (on your right) and TAP it to pick it up.";


    [Header("Extinguisher")]
    public string pinHint =
        "STEP 1: Remove the SAFETY PIN from the extinguisher.";
    public string aimHint =
        "STEP 2: Aim the hose at the BASE of the fire.\n" +
        "STEP 3: PRESS AND HOLD the handle to spray.";
    public string pinBlockedHint = "Wrong order! Remove the safety pin first.";
    public string sprayHint =
        "KEEP SPRAYING!\nAim the powder at the base of the fire.";
    public string sprayMissHint =
        "The powder is not hitting the fire.\nAim the handle toward the fire.";

    [Header("Result")]
    public string successTitle = "FIRE EXTINGUISHED!";
    public string successBody =
        "Fire extinguisher procedure completed successfully.\n\n" +
        "✓ Alarm activated\n" +
        "✓ Extinguisher picked up\n" +
        "✓ Safety pin removed\n" +
        "✓ Hose aimed\n" +
        "✓ Fire extinguished";
    public string successFooter = "TAP ANYWHERE TO CONTINUE";

    public string completeTitle = "TRAINING COMPLETE";
    public string completeBody =
        "The fire is out and the area is safe.\n" +
        "Now walk calmly toward the EXIT sign and leave the area.\n\nWell done!";
    public string completeFooter = "TAP TO CLOSE";

    [Header("Timing")]
    public float messageHoldTime = 3.5f;
    public float temporaryHintTime = 2.5f;

    public Stage CurrentStage => stage;

    private Stage stage = Stage.Intro;
    private bool subscribed;
    private Coroutine messageRoutine;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Start()
    {
        ResolveReferences();
        BuildUI();
        SubscribeEvents();
        BeginIntro();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void Update()
    {
        HandleCardTaps();
        UpdateSprayProgress();
    }

    // =====================================================
    // REFERENCE RESOLUTION
    // =====================================================

    private void ResolveReferences()
    {
        // The scenario is inactive until placement, so we use
        // GameObject.Find (which finds inactive objects as well).
        if (fireScenario == null)
        {
            GameObject found = GameObject.Find("FireScenario");
            if (found != null)
            {
                fireScenario = found;
            }
            else
            {
                Debug.LogError(
                    "FireScenarioFlowManager: FireScenario not found in scene."
                );
            }
        }

        if (placement == null)
        {
            placement = FindFirstObjectByType<FireScenarioARPlacement>();
            if (placement == null)
            {
                Debug.LogError(
                    "FireScenarioFlowManager: FireScenarioARPlacement not found on XR Origin."
                );
            }
        }

        if (fireScenario != null)
        {
            if (displayPickup == null)
            {
                displayPickup = fireScenario.GetComponentInChildren<ExtinguisherDisplayPickup>(true);
            }

            if (originalPickup == null)
            {
                originalPickup = fireScenario.GetComponentInChildren<ExtinguisherPickup>(true);
            }

            if (fire == null)
            {
                fire = fireScenario.GetComponentInChildren<FireExtinguishable>(true);
            }

            if (pinInteraction == null)
            {
                FirePinInteraction[] allPins =
                    fireScenario.GetComponentsInChildren<FirePinInteraction>(true);

                foreach (FirePinInteraction pin in allPins)
                {
                    if (originalPickup != null &&
                        pin.transform.IsChildOf(originalPickup.transform))
                    {
                        pinInteraction = pin;
                        break;
                    }
                }
            }

            if (gripInteraction == null)
            {
                ExtinguisherGripInteraction[] allGrips =
                    fireScenario.GetComponentsInChildren<ExtinguisherGripInteraction>(true);

                foreach (ExtinguisherGripInteraction grip in allGrips)
                {
                    if (originalPickup != null &&
                        grip.transform.IsChildOf(originalPickup.transform))
                    {
                        gripInteraction = grip;
                        break;
                    }
                }
            }
        }

                // =================================================
        // ALARM (resolved at runtime - not hard-wired in the scene)
        // =================================================
        if (fireScenario != null && alarmInteraction == null)
        {
            // Find the FireAlarm object under the scenario (it may be
            // parented under the display extinguisher currently).
            Transform alarmTransform =
                FindDeepChild(fireScenario.transform, "FireAlarm");

            if (alarmTransform != null)
            {
                // Re-parent under the scenario root so it is NOT destroyed
                // when the display extinguisher hides on pickup.
                // World position/rotation are preserved.
                alarmTransform.SetParent(fireScenario.transform, true);

                alarmInteraction =
                    alarmTransform.GetComponent<AlarmInteraction>();

                if (alarmInteraction == null)
                {
                    alarmInteraction = alarmTransform.gameObject
                        .AddComponent<AlarmInteraction>();
                }
            }
            else
            {
                alarmInteraction =
                    fireScenario.GetComponentInChildren<AlarmInteraction>(true);

                if (alarmInteraction == null)
                {
                    Debug.LogWarning(
                        "FireScenarioFlowManager: FireAlarm object not found - "
                        + "alarm step will proceed without a tappable alarm."
                    );
                }
            }
        }

                // =================================================
        // TAP-FEEDBACK ZONE (explains why the extinguisher is locked)
        // =================================================
                if (tapFeedback == null && displayPickup != null)
        {
            // Add to the display (fake) extinguisher if the helper is
            // not already present in the scene. The root TapFeedbackZone
            // detects taps on itself and its children, so placing it on
            // the display extinguisher covers the whole object.
            tapFeedback = displayPickup.GetComponent<TapFeedbackZone>();

            if (tapFeedback == null)
            {
                tapFeedback = displayPickup.gameObject
                    .AddComponent<TapFeedbackZone>();
            }
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "FireScenarioFlowManager: could not resolve FireScenario references."
            );
        }
    }

    // =====================================================
    // RUNTIME UTILITIES
    // =====================================================

    private Transform FindDeepChild(Transform aParent, string aName)
    {
        if (aParent == null)
            return null;

        foreach (Transform child in aParent)
        {
            if (child.name == aName)
                return child;

            Transform result = FindDeepChild(child, aName);
            if (result != null)
                return result;
        }

        return null;
    }

    // =====================================================
    // UI
    // =====================================================

    private void BuildUI()
    {
        if (ui != null)
            return;

        GameObject uiGO = new GameObject("FireScenarioUI");
        uiGO.transform.SetParent(transform, false);
        ui = uiGO.AddComponent<FireScenarioUIController>();
    }

    // =====================================================
    // EVENT SUBSCRIPTIONS
    // =====================================================

    private void SubscribeEvents()
    {
        if (subscribed)
            return;

        if (placement != null)
            placement.OnScenarioPlaced.AddListener(HandleScenarioPlaced);

        if (displayPickup != null)
            displayPickup.OnPickedUp.AddListener(HandleExtinguisherPickedUp);

        if (pinInteraction != null)
            pinInteraction.OnPinRemoved.AddListener(HandlePinRemoved);

        if (gripInteraction != null)
        {
            gripInteraction.OnSprayStarted.AddListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.AddListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.AddListener(HandlePinRemovalRequired);
        }

                if (fire != null)
            fire.OnExtinguished.AddListener(HandleFireExtinguished);

        if (alarmInteraction != null)
            alarmInteraction.OnAlarmActivated.AddListener(HandleAlarmActivated);

        subscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!subscribed)
            return;

        if (placement != null)
            placement.OnScenarioPlaced.RemoveListener(HandleScenarioPlaced);

        if (displayPickup != null)
            displayPickup.OnPickedUp.RemoveListener(HandleExtinguisherPickedUp);

        if (pinInteraction != null)
            pinInteraction.OnPinRemoved.RemoveListener(HandlePinRemoved);

        if (gripInteraction != null)
        {
            gripInteraction.OnSprayStarted.RemoveListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.RemoveListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.RemoveListener(HandlePinRemovalRequired);
        }

                if (fire != null)
            fire.OnExtinguished.RemoveListener(HandleFireExtinguished);

        if (alarmInteraction != null)
            alarmInteraction.OnAlarmActivated.RemoveListener(HandleAlarmActivated);

        subscribed = false;
    }

    // =====================================================
    // FLOW HANDLERS
    // =====================================================

        private void BeginIntro()
    {
        stage = Stage.Intro;

        if (placement != null)
            placement.SetPlacementActive(false);

        if (ui != null)
        {
            ui.HideHint();
            ui.HideProgress();
            ui.HideStep();
            ui.HideCard();
            ui.ShowCard(introTitle, introBody, introFooter);
        }
    }

    private void BeginScanning()
    {
        stage = Stage.Scanning;

        if (ui != null)
        {
            ui.HideCard();
            ui.ShowHint(scanHint);
            ui.ShowStep(1, totalSteps);
        }

        // Enable the blue circle + floor placement.
        if (placement != null)
            placement.SetPlacementActive(true);
    }

    private void HandleScenarioPlaced()
    {
        if (stage == Stage.Complete)
            return;

        // Lock placement forever - scenario is anchored in the real world.
        if (placement != null)
            placement.SetPlacementActive(false);

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.AlarmRequired);
        TrainingEventManager.RaiseScenarioPlaced();

        stage = Stage.MissionIntro;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMissionSequence());
    }

    private IEnumerator ShowMissionSequence()
    {
        if (ui == null)
            yield break;

        ui.HideHint();
        ui.HideStep();
        ui.HideCard();

        // Let the fire appear, then explain the mission.
        ui.ShowHint(missionHint);
        yield return new WaitForSeconds(messageHoldTime);

        ui.HideHint();

        // --------------------------------------------------
        // ALARM-FIRST RULE: wait for the worker to tap the alarm.
        // --------------------------------------------------
        stage = Stage.AlarmRequired;

        ui.ShowHint(alarmHint);
        ui.ShowStep(1, totalSteps);
        ui.ShowActionButton("ACTIVATE ALARM", ActivateAlarmFromUi);

        LockExtinguisherPickup(true);

        yield break; // Update() stays here until HandleAlarmActivated fires.
    }

    private void HandleAlarmActivated()
    {
        if (stage == Stage.Complete)
            return;

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.AlarmActivated);
        TrainingEventManager.RaiseAlarmActivated();

        stage = Stage.AlarmActivated;

        // Unlock the extinguisher pickup.
        LockExtinguisherPickup(false);

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        if (ui != null)
        {
            ui.HideActionButton();
            ui.HideHint();
            ui.ShowStep(2, totalSteps);
            ui.ShowHint(pickupHint);
        }

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.ExtinguisherRequired);
    }

    // UI bridge only: this calls the existing alarm interaction, which
    // continues to own its visuals, audio and one-time activation logic.
    private void ActivateAlarmFromUi()
    {
        if (stage != Stage.AlarmRequired)
            return;

        if (alarmInteraction != null)
        {
            alarmInteraction.Activate();
        }
        else if (ui != null)
        {
            ui.ShowHint("Tap the physical fire alarm to continue.");
        }
    }


        private void HandleExtinguisherPickedUp()
    {
        if (stage == Stage.Complete)
            return;

        // Only allow pickup once the alarm has been activated.
        if (stage != Stage.AlarmActivated && stage != Stage.ExtinguisherRequired)
        {
            if (ui != null)
                ui.ShowHint(extinguisherLockedHint);
            return;
        }

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.ExtinguisherPickedUp);
        TrainingEventManager.RaiseExtinguisherPickedUp();

        stage = Stage.PinGuide;

        if (ui != null)
        {
            ui.HideHint();
            ui.HideProgress();
            ui.ShowStep(3, totalSteps);
            ui.ShowHint(pinHint);
        }
    }

    private void HandlePinRemoved()
    {
        if (stage == Stage.Complete)
            return;

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.PinRemoved);
        TrainingEventManager.RaisePinRemoved();

        stage = Stage.AimGuide;

        if (ui != null)
        {
            ui.HideHint();
            ui.ShowStep(4, totalSteps);
            ui.ShowHint(aimHint);
        }
    }



        private void HandleSprayStarted()
    {
        if (stage == Stage.Complete)
            return;

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.ExtinguisherInUse);
        TrainingEventManager.RaiseExtinguisherUsed();

        stage = Stage.Spraying;

        if (ui != null)
        {
            ui.ShowHint(sprayHint);
            ui.ShowStep(5, totalSteps);
            ui.ShowProgress(0f,
                "Spraying the fire... 0.0s / " +
                (int)RequiredSpraySeconds() + "s");
        }
    }


    private void HandleSprayStopped()
    {
        // Nothing to do right now - UpdateSprayProgress shows the
        // "not hitting the fire" hint automatically.
    }

    private void HandlePinRemovalRequired()
    {
        // The worker pressed the handle before removing the safety pin.
        if (stage == Stage.PickupGuide || stage == Stage.PinGuide)
        {
            if (messageRoutine != null)
                StopCoroutine(messageRoutine);

            messageRoutine = StartCoroutine(
                ShowTemporaryHint(pinBlockedHint, temporaryHintTime));
        }
    }

        private void HandleFireExtinguished()
    {
        if (stage == Stage.Complete)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.FireExtinguished);
        TrainingEventManager.RaiseFireExtinguished();

        // Extinguishing the fire is the success condition. Record
        // completion here, rather than waiting for a navigation button.
        AppSession.EnsureExists().SetState(
            AppSession.TrainingState.Completed);
        TrainingEventManager.RaiseTrainingCompleted();

        stage = Stage.Success;

        if (ui != null)
        {
            ui.HideHint();
            ui.HideProgress();
            ui.HideStep();
            ui.HideCard();

            // Completion panel with STEP banner + Retry/Home buttons.
            ui.ShowStep(6, totalSteps);

            ui.ShowCompletion(successTitle + "\n\n" + successBody,
                OnRetryClicked,
                OnHomeClicked);

            ui.HasCompletionPanel = true;
        }
    }

    private void OnRetryClicked()
    {
        if (ui != null)
            ui.HideCompletion();

        AppSession.EnsureExists().ResetTrainingProgress();

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnHomeClicked()
    {
        if (ui != null)
            ui.HideCompletion();

        AppSession.EnsureExists().ResetTrainingProgress();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


        private void LockExtinguisherPickup(bool locked)
    {
        // Disable the display (fake) pickup component so the worker
        // cannot pick up the extinguisher before the alarm is activated.
        // The pickup SCRIPT is untouched - only its enabled state changes.
        if (displayPickup != null)
        {
            displayPickup.enabled = !locked;
        }

                // TapFeedbackZone gives friendly feedback while locked.
        if (tapFeedback != null)
        {
            tapFeedback.locked = locked;

            if (locked)
            {
                if (!tapFeedback.OnFeedbackRequested.Contains(
                    HandleExtinguisherLockedFeedback))
                {
                    tapFeedback.OnFeedbackRequested.AddListener(
                        HandleExtinguisherLockedFeedback);
                }
            }
            else
            {
                tapFeedback.OnFeedbackRequested.RemoveListener(
                    HandleExtinguisherLockedFeedback);
            }
        }
    }


    private void HandleExtinguisherLockedFeedback(string message)
    {
        if (ui != null)
        {
            ui.ShowHint(message);
            ui.ShowStep(1, totalSteps);
        }
    }

    private void UpdateSprayProgress()
    {
        if (stage != Stage.Spraying || ui == null || fire == null)
            return;

        float progress = fire.SprayProgress01;
        float total = fire.extinguishTime;
        float remaining = Mathf.Clamp(total - (progress * total), 0f, total);

        ui.ShowProgress(progress,
            string.Format(
                "Spraying the fire... {0:F1}s / {1:F0}s",
                remaining,
                total));

        if (fire.IsBeingSprayed)
        {
            ui.ShowHint(sprayHint);
        }
        else
        {
            ui.ShowHint(sprayMissHint);
        }
    }

    private float RequiredSpraySeconds()
    {
        return fire != null ? fire.extinguishTime : 10f;
    }

    private void HandleCardTaps()
    {
        if (ui == null || !ui.HasCard)
            return;

        if (!AnyTapPressedThisFrame())
            return;

        if (stage == Stage.Intro)
        {
            BeginScanning();
        }
        else if (stage == Stage.Success)
        {
            stage = Stage.Complete;
            ui.HideCard();
            ui.ShowCard(completeTitle, completeBody, completeFooter);
        }
        else if (stage == Stage.Complete)
        {
            ui.HideCard();
            Debug.Log("Fire training flow complete.");
        }
    }

    private IEnumerator ShowTemporaryHint(string text, float duration)
    {
        if (ui != null)
            ui.ShowHint(text);

        yield return new WaitForSeconds(duration);

        if (ui == null)
            yield break;

        switch (stage)
        {
            case Stage.PickupGuide:
                ui.ShowHint(pickupHint);
                break;
            case Stage.PinGuide:
                ui.ShowHint(pinHint);
                break;
            default:
                ui.HideHint();
                break;
        }
    }

    private bool AnyTapPressedThisFrame()
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }
}

```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\HoseInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class HoseInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform hosePivot;
    public Transform sprayPoint;

    [Header("Movement")]
    public bool enableHoseMovement = true;

    [Header("Touch Settings")]
    public float rotationSpeed = 0.35f;

    private Camera arCamera;

    private bool dragging = false;
    private Vector2 lastTouchPosition;

    private Quaternion startPivotRotation;
    private Quaternion startSprayRotation;

    private void Start()
    {
        arCamera = Camera.main;

        if (hosePivot != null)
            startPivotRotation = hosePivot.rotation;

        if (sprayPoint != null)
            startSprayRotation = sprayPoint.rotation;

        if (hosePivot == null)
        {
            Debug.LogWarning(
                "HoseInteraction: Hose Pivot is not assigned."
            );
        }

        if (sprayPoint == null)
        {
            Debug.LogWarning(
                "HoseInteraction: Spray Point is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (!enableHoseMovement)
            return;

        if (arCamera == null || hosePivot == null)
            return;

        if (Touch.activeTouches.Count == 0)
        {
            dragging = false;
            return;
        }

        Touch touch = Touch.activeTouches[0];

        // Finger touches the hose
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(
                touch.screenPosition
            );

            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform == transform ||
                    hit.collider.transform.IsChildOf(transform))
                {
                    dragging = true;
                    lastTouchPosition = touch.screenPosition;
                    break;
                }
            }
        }

        // Finger moves
        if (dragging &&
            touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 currentPosition = touch.screenPosition;

            Vector2 delta =
                currentPosition - lastTouchPosition;

            lastTouchPosition = currentPosition;

            RotateNozzle(delta);
        }

        // Finger released
        if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            dragging = false;
        }
    }

    private void RotateNozzle(Vector2 delta)
    {
        float horizontal =
            delta.x * rotationSpeed;

        float vertical =
            -delta.y * rotationSpeed;

        hosePivot.Rotate(
            Vector3.up,
            horizontal,
            Space.World
        );

        hosePivot.Rotate(
            Vector3.right,
            vertical,
            Space.Self
        );

        // Make spray direction follow nozzle
        if (sprayPoint != null)
        {
            sprayPoint.rotation = hosePivot.rotation;
        }
    }

    public void ResetHose()
    {
        if (hosePivot != null)
            hosePivot.rotation = startPivotRotation;

        if (sprayPoint != null)
            sprayPoint.rotation = startSprayRotation;

        dragging = false;
    }
}
```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\Interaction\TapFeedbackZone.cs

```csharp
// Stub only.
// The live implementation of TapFeedbackZone lives in:
//     Assets/AR_Fire_foundation/scripts/TapFeedbackZone.cs
//
// This duplicate root copy is intentionally left EMPTY (comment-only)
// so C# does not emit a "duplicate class definition" compilation error.


```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\Scenario\AlarmInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// AlarmInteraction
/// ================
/// Makes the existing FireAlarm object tappable.
///
/// The worker MUST tap the alarm right after the fire scenario is placed.
/// Until then, the extinguisher pickup stays locked by the
/// FireScenarioFlowManager.
///
/// The component is attached at runtime by FireScenarioFlowManager
/// (GameObject.Find("FireAlarm")), so no manual scene setup is needed.
/// A collider is added automatically if the prefab has none.
///
/// Raises: UnityEvent OnAlarmActivated. The flow manager is the single
/// owner of the training-event bridge, preventing duplicate event logs.
/// </summary>
public class AlarmInteraction : MonoBehaviour
{
    [Header("Feedback")]
    public Color activatedColor = new Color(0.1f, 1f, 0.2f, 1f);

    public AudioSource alarmAudio;

    /// <summary>Raised the moment the alarm is activated (once).</summary>
    public UnityEvent OnAlarmActivated = new UnityEvent();

    public bool IsActivated { get; private set; }

    private Camera arCamera;

    private void Start()
    {
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("AlarmInteraction: AR Camera not found.");
        }

        EnsureCollider();

        if (alarmAudio == null)
        {
            alarmAudio = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (IsActivated || arCamera == null)
        {
            return;
        }

        if (Touch.activeTouches.Count == 0)
        {
            return;
        }

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
        {
            return;
        }

        Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
            {
                Activate();
                break;
            }
        }
    }

    /// <summary>Activate the alarm (idempotent).</summary>
    public void Activate()
    {
        if (IsActivated)
        {
            return;
        }

        IsActivated = true;

        Debug.Log("ALARM ACTIVATED");

        ApplyActivatedVisuals();

        if (OnAlarmActivated != null)
        {
            OnAlarmActivated.Invoke();
        }
    }

    /// <summary>
    /// Makes sure the alarm can be tapped by giving it a collider
    /// sized from its renderers if the prefab has none.
    /// </summary>
    public void EnsureCollider()
    {
        if (GetComponentInChildren<Collider>(true) != null ||
            GetComponent<Collider>() != null)
        {
            return;
        }

        BoxCollider box = gameObject.AddComponent<BoxCollider>();

        Bounds bounds = CalculateBounds();

        if (bounds.size.sqrMagnitude > 0.0001f)
        {
            box.center = transform.InverseTransformPoint(bounds.center);
            box.size = transform.InverseTransformVector(bounds.size);
        }
        else
        {
            // Reasonable default for a wall alarm.
            box.size = new Vector3(0.3f, 0.3f, 0.15f);
        }

        Debug.Log("AlarmInteraction: added a BoxCollider to FireAlarm.");
    }

    private void ApplyActivatedVisuals()
    {
        if (alarmAudio != null)
        {
            alarmAudio.Play();
        }

        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = activatedColor;
        }
    }

    private Bounds CalculateBounds()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        bool hasBounds = false;

        foreach (Renderer render in renderers)
        {
            if (render is ParticleSystemRenderer)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = render.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(render.bounds);
            }
        }

        return bounds;
    }
}

```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\TapFeedbackZone.cs

```csharp
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// TapFeedbackZone
/// ===============
/// Small helper for the "alarm first" training rule.
///
/// While <see cref="locked"/> is true, tapping this zone shows a short
/// explanation ("Please activate the alarm first.") instead of silently
/// ignoring the tap. The real pickup logic is untouched - this script
/// never picks anything up, it only gives feedback.
///
/// Placed on the display (fake) extinguisher object.
/// The FlowManager unlocks (disables) this component when the alarm
/// is activated, so the existing pickup system takes over normally.
/// </summary>
public class TapFeedbackZone : MonoBehaviour
{
    [Header("Feedback")]
    [TextArea(2, 3)]
    public string message = "Please activate the alarm first.";

    [Tooltip("Minimum seconds between two feedback messages.")]
    public float cooldown = 1.5f;

    /// <summary>While true, taps produce feedback instead of nothing.</summary>
    public bool locked = true;

    /// <summary>
    /// Hook used by the FlowManager to surface the message in the AR UI.
    /// (UnityEvent so it can also be wired in the Inspector.)
    /// </summary>
    public UnityEngine.Events.UnityEvent<string> OnFeedbackRequested =
        new UnityEngine.Events.UnityEvent<string>();

    private Camera arCamera;
    private float lastFeedbackTime;

    private void Start()
    {
        arCamera = Camera.main;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (!locked || arCamera == null)
        {
            return;
        }

        if (Touch.activeTouches.Count == 0)
        {
            return;
        }

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
        {
            return;
        }

        if (Time.unscaledTime - lastFeedbackTime < cooldown)
        {
            return;
        }

        Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
            {
                lastFeedbackTime = Time.unscaledTime;

                Debug.Log("blocked_action: " + message);

                if (OnFeedbackRequested != null)
                {
                    OnFeedbackRequested.Invoke(message);
                }

                break;
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\AR_Fire_foundation\scripts\UI\FireScenarioUIController.cs

```csharp
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// FireScenarioUIController
/// ========================
/// Builds the entire AR training UI at runtime using Unity UI Toolkit.
///
/// Creates:
///   - Full-screen tap-catcher (for "tap anywhere" cards)
///   - Modal card (title + body + footer)
///   - Bottom hint bar
///   - Spray progress bar
///   - Step indicator
///
/// No prefab setup required. Attach to XR Origin or any always-active object.
/// The FlowManager calls ShowCard / ShowHint / ShowProgress / ShowStep.
/// </summary>
public class FireScenarioUIController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("USS Style Sheets (optional)")]
    public StyleSheet styleSheet;

    // Runtime-created elements
    private VisualElement root;
    private VisualElement tapCatcher;
    private VisualElement cardModal;
    private Label cardTitle;
    private Label cardBody;
    private Label cardFooter;
    private VisualElement hintBar;
    private Label hintLabel;
    private VisualElement progressBarContainer;
    private VisualElement progressBarFill;
    private Label progressLabel;
    private Label stepLabel;

    public bool HasCard => cardModal != null && cardModal.style.display == DisplayStyle.Flex;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        root = uiDocument.rootVisualElement;

        if (styleSheet != null)
        {
            root.styleSheets.Add(styleSheet);
        }

        BuildUI();
    }

    private void BuildUI()
    {
        // Full-screen tap catcher
        tapCatcher = new VisualElement { name = "tap-catcher" };
        tapCatcher.style.position = Position.Absolute;
        tapCatcher.style.top = 0;
        tapCatcher.style.left = 0;
        tapCatcher.style.right = 0;
        tapCatcher.style.bottom = 0;
        tapCatcher.style.backgroundColor = new Color(0, 0, 0, 0);
        root.Add(tapCatcher);

        // Card modal (centered)
        cardModal = new VisualElement { name = "card-modal" };
        cardModal.style.position = Position.Absolute;
        cardModal.style.top = Length.Percent(20);
        cardModal.style.left = Length.Percent(10);
        cardModal.style.right = Length.Percent(10);
        cardModal.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        cardModal.style.borderTopLeftRadius = 16;
        cardModal.style.borderTopRightRadius = 16;
        cardModal.style.borderBottomLeftRadius = 16;
        cardModal.style.borderBottomRightRadius = 16;
        cardModal.style.paddingLeft = 24;
        cardModal.style.paddingRight = 24;
        cardModal.style.paddingTop = 24;
        cardModal.style.paddingBottom = 24;
        cardModal.style.display = DisplayStyle.None;
        root.Add(cardModal);

        cardTitle = new Label { name = "card-title" };
        cardTitle.style.fontSize = 28;
        cardTitle.style.color = Color.white;
        cardTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        cardTitle.style.marginBottom = 12;
        cardTitle.style.whiteSpace = WhiteSpace.Normal;
        cardModal.Add(cardTitle);

        cardBody = new Label { name = "card-body" };
        cardBody.style.fontSize = 18;
        cardBody.style.color = new Color(0.9f, 0.9f, 0.9f);
        cardBody.style.marginBottom = 16;
        cardBody.style.whiteSpace = WhiteSpace.Normal;
        cardModal.Add(cardBody);

        // Hint bar (bottom)
        hintBar = new VisualElement { name = "hint-bar" };
        hintBar.style.position = Position.Absolute;
        hintBar.style.bottom = 0;
        hintBar.style.left = 0;
        hintBar.style.right = 0;
        hintBar.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        hintBar.style.paddingLeft = 20;
        hintBar.style.paddingRight = 20;
        hintBar.style.paddingTop = 16;
        hintBar.style.paddingBottom = 16;
        hintBar.style.display = DisplayStyle.None;
        root.Add(hintBar);

        hintLabel = new Label { name = "hint-label" };
        hintLabel.style.fontSize = 16;
        hintLabel.style.color = Color.white;
        hintLabel.style.whiteSpace = WhiteSpace.Normal;
        hintBar.Add(hintLabel);

        // Step indicator (top-right)
        stepLabel = new Label { name = "step-label" };
        stepLabel.style.position = Position.Absolute;
        stepLabel.style.top = 16;
        stepLabel.style.right = 16;
        stepLabel.style.fontSize = 14;
        stepLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
        stepLabel.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        stepLabel.style.paddingLeft = 12;
        stepLabel.style.paddingRight = 12;
        stepLabel.style.paddingTop = 6;
        stepLabel.style.paddingBottom = 6;
        stepLabel.style.borderTopLeftRadius = 12;
        stepLabel.style.borderTopRightRadius = 12;
        stepLabel.style.borderBottomLeftRadius = 12;
        stepLabel.style.borderBottomRightRadius = 12;
        stepLabel.style.display = DisplayStyle.None;
        root.Add(stepLabel);

        // Progress bar (above hint bar)
        progressBarContainer = new VisualElement { name = "progress-container" };
        progressBarContainer.style.position = Position.Absolute;
        progressBarContainer.style.bottom = 80;
        progressBarContainer.style.left = 20;
        progressBarContainer.style.right = 20;
        progressBarContainer.style.height = 12;
        progressBarContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        progressBarContainer.style.borderTopLeftRadius = 6;
        progressBarContainer.style.borderTopRightRadius = 6;
        progressBarContainer.style.borderBottomLeftRadius = 6;
        progressBarContainer.style.borderBottomRightRadius = 6;
        progressBarContainer.style.display = DisplayStyle.None;
        root.Add(progressBarContainer);

        progressBarFill = new VisualElement { name = "progress-fill" };
        progressBarFill.style.height = Length.Percent(100);
        progressBarFill.style.width = Length.Percent(0);
        progressBarFill.style.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        progressBarFill.style.borderTopLeftRadius = 6;
        progressBarFill.style.borderTopRightRadius = 6;
        progressBarFill.style.borderBottomLeftRadius = 6;
        progressBarFill.style.borderBottomRightRadius = 6;
        progressBarContainer.Add(progressBarFill);

        progressLabel = new Label { name = "progress-label" };
        progressLabel.style.position = Position.Absolute;
        progressLabel.style.bottom = 96;
        progressLabel.style.left = 20;
        progressLabel.style.fontSize = 12;
        progressLabel.style.color = Color.white;
        progressLabel.style.display = DisplayStyle.None;
        root.Add(progressLabel);
    }

    public void ShowCard(string title, string body, string footer)
    {
        if (cardModal == null) return;
        cardTitle.text = title;
        cardBody.text = body;
        cardFooter.text = footer;
        cardModal.style.display = DisplayStyle.Flex;
    }

    public void HideCard()
    {
        if (cardModal == null) return;
        cardModal.style.display = DisplayStyle.None;
    }

    public void ShowHint(string text)
    {
        if (hintBar == null) return;
        hintLabel.text = text;
        hintBar.style.display = DisplayStyle.Flex;
    }

    public void HideHint()
    {
        if (hintBar == null) return;
        hintBar.style.display = DisplayStyle.None;
    }

    public void ShowProgress(float progress01, string label)
    {
        if (progressBarContainer == null) return;
        progressBarContainer.style.display = DisplayStyle.Flex;
        progressBarFill.style.width = Length.Percent(progress01 * 100f);
        if (progressLabel != null)
        {
            progressLabel.style.display = DisplayStyle.Flex;
            progressLabel.text = label;
        }
    }

    public void HideProgress()
    {
        if (progressBarContainer == null) return;
        progressBarContainer.style.display = DisplayStyle.None;
        if (progressLabel != null)
            progressLabel.style.display = DisplayStyle.None;
    }

    public void ShowStep(int current, int total)
    {
        if (stepLabel == null) return;
        stepLabel.text = $"STEP {current} / {total}";
        stepLabel.style.display = DisplayStyle.Flex;
    }

    public void HideStep()
    {
        if (stepLabel == null) return;
        stepLabel.style.display = DisplayStyle.None;
    }
}
```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Data\FireScenarioData.cs

```csharp
using UnityEngine;

namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// FireScenarioData — Configurable data for the fire safety scenario.
    /// </summary>
    [CreateAssetMenu(fileName = "FireScenarioData", menuName = "SurakshaAR/Fire Scenario Data")]
    public class FireScenarioData : ScriptableObject
    {
        [Header("Hazard")]
        public string hazardType = "electrical_fire";
        public string hazardName = "Electrical Fire";

        [Header("Equipment")]
        public string[] requiredEquipment = { "fire_alarm", "co2_extinguisher" };
        public string correctExtinguisher = "co2";
        public string[] wrongExtinguishers = { "water", "foam" };

        [Header("Timing")]
        public float extinguishTime = 10f;
        public float alarmDelay = 2f;

        [Header("Scoring")]
        public float passThreshold = 70f;
    }
}

```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Data\FireTrainingState.cs

```csharp
namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// FireTrainingState — Tracks the current training state for fire safety.
    /// </summary>
    public enum FireTrainingMode
    {
        Training,
        Practice,
        Assessment
    }

    [System.Serializable]
    public class FireTrainingState
    {
        public FireTrainingMode Mode = FireTrainingMode.Training;
        public bool AlarmActivated = false;
        public bool ExtinguisherPickedUp = false;
        public bool PinRemoved = false;
        public bool FireExtinguished = false;
        public float SessionTime = 0f;
    }
}

```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Scripts\ExtinguisherInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// ExtinguisherInteraction — Handles extinguisher pickup and hold mechanics.
    /// Simplified wrapper around the existing ExtinguisherPickup.
    /// </summary>
    public class ExtinguisherInteraction : MonoBehaviour
    {
        public ExtinguisherPickup pickup;
        public UnityEvent OnPickedUp = new UnityEvent();
        private bool pickedUp = false;
        private Camera arCamera;

        private void Start() { arCamera = Camera.main; }
        private void OnEnable() { EnhancedTouchSupport.Enable(); }
        private void OnDisable() { EnhancedTouchSupport.Disable(); }

        private void Update()
        {
            if (pickedUp || arCamera == null || Touch.activeTouches.Count == 0) return;
            var touch = Touch.activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
            Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
            foreach (var hit in Physics.RaycastAll(ray))
            {
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                { Pickup(); break; }
            }
        }

        private void Pickup()
        {
            pickedUp = true;
            if (pickup != null) pickup.AttachToCamera();
            OnPickedUp?.Invoke();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Scripts\FireAlarmController.cs

```csharp
using UnityEngine;

namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// FireAlarmController — Controls the fire alarm audio and visual feedback.
    /// </summary>
    public class FireAlarmController : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource alarmAudio;
        public AudioClip alarmSound;

        [Header("Visual")]
        public Renderer alarmLight;
        public Color activeColor = Color.green;
        public Color inactiveColor = Color.red;

        private bool isActivated = false;

        public void Activate()
        {
            if (isActivated) return;
            isActivated = true;

            if (alarmAudio != null && alarmSound != null)
            {
                alarmAudio.clip = alarmSound;
                alarmAudio.loop = true;
                alarmAudio.Play();
            }

            if (alarmLight != null)
                alarmLight.material.color = activeColor;
        }

        public void Deactivate()
        {
            isActivated = false;
            if (alarmAudio != null) alarmAudio.Stop();
            if (alarmLight != null) alarmLight.material.color = inactiveColor;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Scripts\FireAssessmentBridge.cs

```csharp
using UnityEngine;
using SurakshaAR.Assessment;

namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// FireAssessmentBridge — Connects existing fire interaction scripts to AssessmentManager.
    /// Wires up: AlarmInteraction, FirePinInteraction, ExtinguisherGripInteraction, FireExtinguishable.
    /// </summary>
    public class FireAssessmentBridge : MonoBehaviour
    {
        public AssessmentManager assessmentManager;

        [Header("Fire Interactions")]
        public AlarmInteraction alarmInteraction;
        public FirePinInteraction pinInteraction;
        public ExtinguisherGripInteraction gripInteraction;
        public FireExtinguishable fireExtinguishable;

        private void OnEnable()
        {
            if (alarmInteraction != null) alarmInteraction.OnAlarmActivated.AddListener(OnAlarm);
            if (pinInteraction != null) pinInteraction.OnPinRemoved.AddListener(OnPinRemoved);
            if (gripInteraction != null)
            {
                gripInteraction.OnSprayStarted.AddListener(OnSprayStarted);
                gripInteraction.OnPinRemovalRequired.AddListener(OnPinRequired);
            }
            if (fireExtinguishable != null) fireExtinguishable.OnExtinguished.AddListener(OnExtinguished);
        }

        private void OnDisable()
        {
            if (alarmInteraction != null) alarmInteraction.OnAlarmActivated.RemoveListener(OnAlarm);
            if (pinInteraction != null) pinInteraction.OnPinRemoved.RemoveListener(OnPinRemoved);
            if (gripInteraction != null)
            {
                gripInteraction.OnSprayStarted.RemoveListener(OnSprayStarted);
                gripInteraction.OnPinRemovalRequired.RemoveListener(OnPinRequired);
            }
            if (fireExtinguishable != null) fireExtinguishable.OnExtinguished.RemoveListener(OnExtinguished);
        }

        private void OnAlarm()
        {
            if (assessmentManager != null) assessmentManager.RecordEquipmentSelected(true, "activate_alarm");
        }

        private void OnPinRemoved()
        {
            if (assessmentManager != null) assessmentManager.RecordEquipmentSelected(true, "remove_safety_pin");
        }

        private void OnSprayStarted()
        {
            if (assessmentManager != null) assessmentManager.RecordEquipmentSelected(true, "spray_extinguisher");
        }

        private void OnPinRequired()
        {
            if (assessmentManager != null) assessmentManager.RecordWrongAction(false);
        }

        private void OnExtinguished()
        {
            if (assessmentManager != null) assessmentManager.RecordEvacuation(true, "fire_extinguished");
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Scripts\FireController.cs

```csharp
using UnityEngine;

namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// FireController — Controls fire particle effects and extinguishing.
    /// Wrapper around FireExtinguishable for training mode.
    /// </summary>
    public class FireController : MonoBehaviour
    {
        [Header("Fire")]
        public ParticleSystem fireParticles;
        public ParticleSystem smokeParticles;

        [Header("Audio")]
        public AudioSource fireAudio;

        private bool isExtinguished = false;

        public void StartFire()
        {
            if (fireParticles != null) fireParticles.Play();
            if (smokeParticles != null) smokeParticles.Play();
            if (fireAudio != null) fireAudio.Play();
            isExtinguished = false;
        }

        public void ExtinguishFire()
        {
            if (isExtinguished) return;
            isExtinguished = true;

            if (fireParticles != null) fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (smokeParticles != null) smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (fireAudio != null) fireAudio.Stop();
        }

        public bool IsExtinguished() => isExtinguished;
    }
}

```

---

## C:\project\surakshaAR\assets\Features\FireSafety\Scripts\FireTrainingManager.cs

```csharp
using UnityEngine;

namespace SurakshaAR.FireSafety
{
    /// <summary>
    /// FireTrainingManager — High-level manager for fire safety training.
    /// Coordinates between training, practice, and assessment modes.
    /// </summary>
    public class FireTrainingManager : MonoBehaviour
    {
        public enum TrainingState { Idle, Training, Practice, Assessment, Complete }

        [Header("State")]
        public TrainingState currentState = TrainingState.Idle;

        [Header("Managers")]
        public FireTrainingMode trainingMode;
        public AssessmentManager assessmentManager;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnTrainingComplete;
        public UnityEngine.Events.UnityEvent OnAssessmentComplete;

        public void BeginTraining()
        {
            currentState = TrainingState.Training;
            if (trainingMode != null) trainingMode.StartTraining();
        }

        public void BeginPractice()
        {
            currentState = TrainingState.Practice;
            if (trainingMode != null) trainingMode.StartPractice();
        }

        public void BeginAssessment()
        {
            currentState = TrainingState.Assessment;
            if (trainingMode != null) trainingMode.StartAssessment();
        }

        public void CompleteAssessment()
        {
            currentState = TrainingState.Complete;
            if (trainingMode != null) trainingMode.EndAssessment();
            OnAssessmentComplete?.Invoke();
        }

        public void CompleteTraining()
        {
            currentState = TrainingState.Complete;
            OnTrainingComplete?.Invoke();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Data\GasScenarioData.cs

```csharp
using UnityEngine;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasScenarioData — Configurable data for the gas safety scenario.
    /// </summary>
    [CreateAssetMenu(fileName = "GasScenarioData", menuName = "SurakshaAR/Gas Scenario Data")]
    public class GasScenarioData : ScriptableObject
    {
        [Header("Hazard")]
        public string hazardType = "gas_leak";
        public string hazardName = "Natural Gas Leak";

        [Header("PPE")]
        public string[] requiredPPE = { "gas_mask", "gloves", "suit" };

        [Header("Evacuation")]
        public string correctDirection = "upwind";
        public string[] wrongDirections = { "downwind", "towards_source" };

        [Header("Scoring")]
        public float passThreshold = 70f;
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Data\GasTrainingState.cs

```csharp
namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasTrainingState — Tracks the current training state for gas safety.
    /// </summary>
    public enum GasTrainingMode
    {
        Training,
        Practice,
        Assessment
    }

    [System.Serializable]
    public class GasTrainingState
    {
        public GasTrainingMode Mode = GasTrainingMode.Training;
        public bool HazardIdentified = false;
        public bool PPESelected = false;
        public bool EvacuationComplete = false;
        public float SessionTime = 0f;
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Scripts\GasAssessmentBridge.cs

```csharp
using UnityEngine;
using SurakshaAR.Assessment;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasAssessmentBridge — Connects gas safety interactions to the AssessmentManager.
    /// </summary>
    public class GasAssessmentBridge : MonoBehaviour
    {
        public AssessmentManager assessmentManager;
        public GasLeakInteraction gasLeak;
        public PPEInteraction ppe;
        public GasEvacuationController evacuation;

        private void OnEnable()
        {
            if (gasLeak != null) gasLeak.OnHazardIdentified.AddListener(OnHazard);
            if (ppe != null) ppe.OnPPESelected.AddListener(OnPPE);
            if (evacuation != null) evacuation.OnEvacuationStarted.AddListener(OnEvacuation);
        }

        private void OnDisable()
        {
            if (gasLeak != null) gasLeak.OnHazardIdentified.RemoveListener(OnHazard);
            if (ppe != null) ppe.OnPPESelected.RemoveListener(OnPPE);
            if (evacuation != null) evacuation.OnEvacuationStarted.RemoveListener(OnEvacuation);
        }

        private void OnHazard()
        {
            if (assessmentManager != null) assessmentManager.RecordHazardIdentified(true);
        }

        private void OnPPE()
        {
            if (assessmentManager != null) assessmentManager.RecordEquipmentSelected(true, "ppe_selected");
        }

        private void OnEvacuation()
        {
            if (assessmentManager != null) assessmentManager.RecordEvacuation(true, "upwind");
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Scripts\GasEvacuationController.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasEvacuationController — Handles evacuation direction selection (upwind).
    /// </summary>
    public class GasEvacuationController : MonoBehaviour
    {
        public UnityEvent OnEvacuationStarted = new UnityEvent();
        private bool evacuated = false;
        private Camera arCamera;

        private void Start() { arCamera = Camera.main; }
        private void OnEnable() { EnhancedTouchSupport.Enable(); }
        private void OnDisable() { EnhancedTouchSupport.Disable(); }

        private void Update()
        {
            if (evacuated || arCamera == null || Touch.activeTouches.Count == 0) return;
            var touch = Touch.activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
            Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
            foreach (var hit in Physics.RaycastAll(ray))
            {
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                { Evacuate(); break; }
            }
        }

        private void Evacuate()
        {
            evacuated = true;
            Debug.Log("evacuation_started: upwind");
            OnEvacuationStarted?.Invoke();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Scripts\GasLeakInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasLeakInteraction — Makes the gas leak source tappable for hazard identification.
    /// </summary>
    public class GasLeakInteraction : MonoBehaviour
    {
        public UnityEvent OnHazardIdentified = new UnityEvent();
        private bool identified = false;
        private Camera arCamera;

        private void Start() { arCamera = Camera.main; }
        private void OnEnable() { EnhancedTouchSupport.Enable(); }
        private void OnDisable() { EnhancedTouchSupport.Disable(); }

        private void Update()
        {
            if (identified || arCamera == null || Touch.activeTouches.Count == 0) return;
            var touch = Touch.activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
            Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
            foreach (var hit in Physics.RaycastAll(ray))
            {
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                { Identify(); break; }
            }
        }

        private void Identify()
        {
            identified = true;
            Debug.Log("hazard_identified: gas_leak");
            OnHazardIdentified?.Invoke();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Scripts\GasSafetyManager.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasSafetyManager — Controls the Gas Leak & Confined Space AR training flow.
    /// Simpler than fire: hazard recognition → PPE → evacuation → assessment.
    /// </summary>
    public class GasSafetyManager : MonoBehaviour
    {
        public enum Stage
        {
            Intro,
            Scanning,
            MissionIntro,
            HazardRecognition,
            PPECheck,
            Evacuation,
            Success,
            Complete
        }

        [Header("Cross References")]
        public GameObject gasScenario;
        public GasScenarioARPlacement placement;
        public GasLeakInteraction gasLeak;
        public PPEInteraction ppeInteraction;
        public GasEvacuationController evacuation;
        public GameObject uiRoot;

        [Header("UI Text")]
        public string introTitle = "GAS SAFETY TRAINING";
        public string introBody = "A gas leak has been detected.\nIdentify the hazard, wear PPE, and evacuate safely.";
        public string scanHint = "Move your phone to scan the floor.\nTap the BLUE circle to place the scenario.";
        public string missionHint = "GAS LEAK DETECTED\n1. Identify the source\n2. Select proper PPE\n3. Evacuate upwind";

        public Stage CurrentStage { get; private set; }

        private void Start()
        {
            if (gasScenario != null) gasScenario.SetActive(false);
            ShowIntro();
        }

        private void Update()
        {
            HandleTaps();
        }

        private void ShowIntro()
        {
            CurrentStage = Stage.Intro;
            SetUIActive(true);
        }

        private void BeginScanning()
        {
            CurrentStage = Stage.Scanning;
            SetUIActive(false);
            if (placement != null) placement.SetPlacementActive(true);
        }

        private void HandleTaps()
        {
            if (Touch.activeTouches.Count == 0) return;
            var touch = Touch.activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

            if (CurrentStage == Stage.Intro)
                BeginScanning();
            else if (CurrentStage == Stage.Complete)
                SetUIActive(false);
        }

        public void OnScenarioPlaced()
        {
            CurrentStage = Stage.MissionIntro;
            SetUIActive(true);
        }

        private void SetUIActive(bool active)
        {
            if (uiRoot != null) uiRoot.SetActive(active);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Scripts\GasScenarioARPlacement.cs

```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// GasScenarioARPlacement — AR placement for gas safety scenario.
    /// Reuses the same pattern as fire placement.
    /// </summary>
    public class GasScenarioARPlacement : MonoBehaviour
    {
        public ARRaycastManager raycastManager;
        public Camera arCamera;
        public Transform gasScenario;
        public float circleSize = 0.35f;
        public Color circleColor = new Color(0f, 0.5f, 1f, 0.9f);

        public bool IsPlaced { get; private set; }
        public bool PlacementActive { get; private set; } = true;

        public UnityEvent OnScenarioPlaced = new UnityEvent();

        private GameObject placementCircle;
        private LineRenderer circleRenderer;
        private ARPlaneManager planeManager;
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

        public void SetPlacementActive(bool active)
        {
            PlacementActive = active;
            if (!active && placementCircle != null) placementCircle.SetActive(false);
        }

        private void Start()
        {
            if (arCamera == null) arCamera = Camera.main;
            if (raycastManager == null) raycastManager = FindFirstObjectByType<ARRaycastManager>();
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
            if (gasScenario != null) gasScenario.gameObject.SetActive(false);
            CreatePlacementCircle();
        }

        private void Update()
        {
            if (IsPlaced || !PlacementActive) return;
            UpdatePlacementCircle();
            CheckTouch();
        }

        private void CreatePlacementCircle()
        {
            placementCircle = new GameObject("Gas_Placement_Circle");
            circleRenderer = placementCircle.AddComponent<LineRenderer>();
            circleRenderer.useWorldSpace = true;
            circleRenderer.loop = true;
            circleRenderer.positionCount = 64;
            circleRenderer.startWidth = 0.025f;
            circleRenderer.endWidth = 0.025f;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            Material mat = new Material(shader);
            mat.color = circleColor;
            circleRenderer.material = mat;
        }

        private void UpdatePlacementCircle()
        {
            if (raycastManager == null || arCamera == null) return;
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = hits[0].pose;
                placementCircle.SetActive(true);
                Vector3 center = pose.position + Vector3.up * 0.015f;
                for (int i = 0; i < 64; i++)
                {
                    float angle = (float)i / 64f * Mathf.PI * 2f;
                    Vector3 point = center + pose.right * Mathf.Cos(angle) * circleSize + pose.forward * Mathf.Sin(angle) * circleSize;
                    circleRenderer.SetPosition(i, point);
                }
            }
            else
            {
                placementCircle.SetActive(false);
            }
        }

        private void CheckTouch()
        {
            if (Touchscreen.current == null) return;
            if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return;
            Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
            PlaceScenario(pos);
        }

        private void PlaceScenario(Vector2 screenPos)
        {
            if (raycastManager == null || !raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)) return;
            Pose pose = hits[0].pose;
            gasScenario.position = pose.position;
            Vector3 fwd = arCamera.transform.forward; fwd.y = 0;
            if (fwd.sqrMagnitude > 0.001f) { fwd.Normalize(); gasScenario.rotation = Quaternion.LookRotation(fwd, pose.up); }
            gasScenario.gameObject.SetActive(true);
            placementCircle.SetActive(false);
            IsPlaced = true;
            if (planeManager != null) { foreach (var p in planeManager.trackables) if (p != null) p.gameObject.SetActive(false); planeManager.enabled = false; }
            OnScenarioPlaced?.Invoke();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Features\GasSafety\Scripts\PPEInteraction.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace SurakshaAR.GasSafety
{
    /// <summary>
    /// PPEInteraction — Allows worker to select PPE items (mask, gloves, suit).
    /// </summary>
    public class PPEInteraction : MonoBehaviour
    {
        public UnityEvent OnPPESelected = new UnityEvent();
        public AudioSource selectAudio;
        private bool selected = false;
        private Camera arCamera;

        private void Start() { arCamera = Camera.main; }
        private void OnEnable() { EnhancedTouchSupport.Enable(); }
        private void OnDisable() { EnhancedTouchSupport.Disable(); }

        private void Update()
        {
            if (selected || arCamera == null || Touch.activeTouches.Count == 0) return;
            var touch = Touch.activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
            Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
            foreach (var hit in Physics.RaycastAll(ray))
            {
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                { Select(); break; }
            }
        }

        private void Select()
        {
            selected = true;
            if (selectAudio != null) selectAudio.Play();
            Debug.Log("ppe_selected");
            OnPPESelected?.Invoke();
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Scripts\Networking\ApiContracts.cs

```csharp
using System;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Data-transfer objects that exactly match the SurakshaAR backend API contract
    /// (see docs/api/API.md). Field names are intentionally snake_case to mirror the
    /// JSON keys so Unity's JsonUtility can (de)serialize them 1:1. Values are the
    /// fixed contract - change the backend and this file together.
    /// </summary>
    public static class ApiContracts
    {
        /// <summary>Standard error shape returned by the backend.</summary>
        [Serializable]
        public sealed class ApiError
        {
            public string detail;
        }

        // ------------------------------------------------------------------
        // Workers
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class WorkerCreate
        {
            public string name;
            public string employee_id;
            public string role;
        }

        [Serializable]
        public sealed class WorkerOut
        {
            public int id;
            public string name;
            public string employee_id;
            public string role;
            public string created_at;
        }

        // ------------------------------------------------------------------
        // Modules
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class ModuleOut
        {
            public int id;
            public string code;
            public string name;
            public string description;
        }

        [Serializable]
        public sealed class ModuleListOut
        {
            public ModuleOut[] modules;
        }

        // ------------------------------------------------------------------
        // Progress
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class ProgressCreate
        {
            public int worker_id;
            public int module_id;
            public string stage;
            public string status;
        }

        [Serializable]
        public sealed class ProgressOut
        {
            public int worker_id;
            public int module_id;
            public string stage;
            public string status;
            public string updated_at;
        }

        [Serializable]
        public sealed class ProgressItemOut
        {
            public int module_id;
            public string module_code;
            public string module_name;
            public string stage;
            public string status;
            public string last_updated;
        }

        [Serializable]
        public sealed class ProgressListOut
        {
            public int worker_id;
            public ProgressItemOut[] progress;
        }

        // ------------------------------------------------------------------
        // Assessments (event-based; scored server-side by the ML engine)
        // ------------------------------------------------------------------

        /// <summary>
        /// A single behavioural event from the VR/AR session. Only event_type is
        /// contractually required; event-specific fields are forwarded as-is.
        /// JsonUtility always serialises all fields, so unused ones are sent as
        /// empty/false - the backend's AssessmentEvent schema allows extra fields.
        /// </summary>
        [Serializable]
        public sealed class AssessmentEvent
        {
            public string event_type;
            public string timestamp;
            public bool correct;
            public string[] items;
            public string severity;   // wrong_action: minor | major
            public string action;
            public string hazard_type;
            public string reason;     // critical_action
            public string route;      // evacuation_started
            public string direction;  // evacuation_started (gas: upwind)
            public string completion_status; // assessment_completed
        }

        [Serializable]
        public sealed class AssessmentCreate
        {
            public int worker_id;
            public int module_id;
            public string scenario_type; // optional; "" -> derived from module code
            public int attempt_number;   // 0 -> auto-incremented server-side
            public AssessmentEvent[] events;
        }

        [Serializable]
        public sealed class CompetencyScoreOut
        {
            public string name;
            public float score;
            public bool passed;
            public float pass_threshold;
        }

        [Serializable]
        public sealed class WeaknessOut
        {
            public string competency_name;
            public float score;
            public float threshold;
            public string severity; // severe | moderate | mild
            public string reason;
            public string[] affected_aspects;
        }

        [Serializable]
        public sealed class RetrainingModuleOut
        {
            public string module_id;
            public string name;
            public string description;
            public int estimated_duration_minutes;
            public string difficulty_level;
            public string[] competencies_addressed;
            public string reason;
        }

        [Serializable]
        public sealed class RetrainingPlanOut
        {
            public string scenario_type;
            public RetrainingModuleOut[] recommended_modules;
            public int total_estimated_duration_minutes;
            public bool time_limit_exceeded;
            public int weaknesses_addressed;
            public int total_weaknesses;
        }

        /// <summary>
        /// Mirror of the backend's competency_scores JSON *object*. JsonUtility
        /// cannot deserialise dictionaries, but the competency key set is fixed
        /// per scenario (see ml/competency/scoring/config.py), so we model the map
        /// as a flat class. Missing keys deserialise to null.
        /// </summary>
        [Serializable]
        public sealed class CompetencyScoreMap
        {
            // fire + gas
            public CompetencyScoreOut hazard_identification;
            public CompetencyScoreOut ppe_selection;
            public CompetencyScoreOut equipment_use;
            // fire only
            public CompetencyScoreOut procedure_compliance;
            public CompetencyScoreOut decision_making;
            // gas only
            public CompetencyScoreOut evacuation;
            public CompetencyScoreOut emergency_response;

            public int Count()
            {
                int n = 0;
                if (hazard_identification != null) n++;
                if (ppe_selection != null) n++;
                if (equipment_use != null) n++;
                if (procedure_compliance != null) n++;
                if (decision_making != null) n++;
                if (evacuation != null) n++;
                if (emergency_response != null) n++;
                return n;
            }
        }

        [Serializable]
        public sealed class AssessmentOut
        {
            public int id;
            public int worker_id;
            public int module_id;
            public int attempt_number;
            public string scenario_type;
            public float score;
            public bool passed;
            public string pass_reason;
            public WeaknessOut[] weaknesses;
            public CompetencyScoreMap competency_scores;
            public string[] critical_errors;
            public string created_at;
        }

        [Serializable]
        public sealed class AssessmentHistoryOut
        {
            public int worker_id;
            public AssessmentOut[] assessments;
        }
        // ------------------------------------------------------------------
        // Sync (offline session data)
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class SyncSession
        {
            public string type;
            public int module_id;
            public float score;
            public bool passed;
            public string[] weaknesses;
            public string occurred_at;
        }

        [Serializable]
        public sealed class SyncCreate
        {
            public int worker_id;
            public string device_id;
            public SyncSession[] sessions;
        }

        [Serializable]
        public sealed class SyncOut
        {
            public int sync_id;
            public int worker_id;
            public string synced_at;
            public int sessions_synced;
        }

        [Serializable]
        public sealed class SyncStatusOut
        {
            public int worker_id;
            public string last_synced_at;
            public int pending_sessions;
        }

        // ------------------------------------------------------------------
        // Certificates
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class CertificateCreate
        {
            public int worker_id;
            public int module_id;
        }

        [Serializable]
        public sealed class CertificateOut
        {
            public int id;
            public string certificate_number;
            public int worker_id;
            public int module_id;
            public string issued_at;
            public string valid_until;
            public string status;
        }

        [Serializable]
        public sealed class CertificateListOut
        {
            public int worker_id;
            public CertificateOut[] certificates;
        }

        [Serializable]
        public sealed class CertificateVerifyOut
        {
            public string certificate_number;
            public bool valid;
            public string worker_name;
            public string module_name;
            public string issued_at;
            public string valid_until;
            public string status;
        }

        // ------------------------------------------------------------------
        // Vision / ML (docs/api/API.md #18, #19)
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class PPECheckRequest
        {
            public string image_base64;
            public string[] required_ppe;
            public string mode; // "auto" (default) | "mock" | "model"
        }

        [Serializable]
        public sealed class PPEDetectionOut
        {
            public string item;
            public float confidence;
        }

        [Serializable]
        public sealed class PPECheckOut
        {
            public string status; // ok | low_confidence | model_error | disabled
            public bool ppe_ok;
            public PPEDetectionOut[] detections;
            public string[] missing_items;
            public float confidence;
            public bool fallback_used;
            public string message;
            public string[] required_ppe;
        }

        [Serializable]
        public sealed class VisionStatusOut
        {
            public string status;
            public string module;
            public string version;
            public string mode;
            public string model_path;
            public bool model_loaded;
            public bool fallback_enabled;
            public string[] supported_items;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Scripts\Networking\AssessmentApiClient.cs

```csharp
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// AssessmentApiClient — Clean REST client for the SurakshaAR backend API.
    /// Follows docs/api/API.md exactly.
    /// </summary>
    public class AssessmentApiClient : MonoBehaviour
    {
        [Header("API Configuration")]
        public string baseUrl = "http://localhost:8000/api/v1";

        [Header("Request Timeout (seconds)")]
        public int timeoutSeconds = 30;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator HealthCheck(Action<string> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/health";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(request.downloadHandler.text);
                else
                    onError?.Invoke(request.error ?? "Health check failed");
            }
        }

        public IEnumerator CreateWorker(ApiContracts.WorkerCreate worker, Action<ApiContracts.WorkerOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/workers";
            string jsonBody = JsonUtility.ToJson(worker);
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.WorkerOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(ParseError(request));
            }
        }

        public IEnumerator GetWorker(int workerId, Action<ApiContracts.WorkerOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/workers/{workerId}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.WorkerOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(request.error ?? "Worker not found");
            }
        }

        public IEnumerator SubmitAssessment(ApiContracts.AssessmentCreate assessment, Action<ApiContracts.AssessmentResultOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/assessments";
            string jsonBody = JsonUtility.ToJson(assessment);
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.AssessmentResultOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(ParseError(request));
            }
        }

        public IEnumerator GetLatestAssessment(int workerId, int moduleId, Action<ApiContracts.AssessmentOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/assessments/{workerId}/latest?module_id={moduleId}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.AssessmentOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(request.error ?? "Assessment not found");
            }
        }

        public IEnumerator UpdateProgress(ApiContracts.ProgressCreate progress, Action<ApiContracts.ProgressOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/progress";
            string jsonBody = JsonUtility.ToJson(progress);
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.ProgressOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(ParseError(request));
            }
        }

        public IEnumerator SyncSessions(ApiContracts.SyncCreate sync, Action<ApiContracts.SyncOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/sync";
            string jsonBody = JsonUtility.ToJson(sync);
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.SyncOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(ParseError(request));
            }
        }

        public IEnumerator GetRetrainingPlan(int assessmentId, Action<ApiContracts.RetrainingPlanOut> onSuccess, Action<string> onError)
        {
            string url = $"{baseUrl}/assessments/{assessmentId}/retraining-plan";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(JsonUtility.FromJson<ApiContracts.RetrainingPlanOut>(request.downloadHandler.text));
                else
                    onError?.Invoke(request.error ?? "Retraining plan not found");
            }
        }

        private string ParseError(UnityWebRequest request)
        {
            try
            {
                ApiContracts.ApiError error = JsonUtility.FromJson<ApiContracts.ApiError>(request.downloadHandler.text);
                if (error != null && !string.IsNullOrEmpty(error.detail))
                    return error.detail;
            }
            catch { }
            return request.error ?? $"HTTP {(int)request.responseCode}";
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Scripts\Networking\AssessmentEventMapper.cs

```csharp
using System;
using SurakshaAR.Networking;

namespace SurakshaAR.Assessment
{
    /// <summary>
    /// AssessmentEventMapper
    /// =====================
    /// Maps AR interaction outcomes to well-formed assessment events
    /// that the backend ML competency engine understands.
    ///
    /// Uses the existing AssessmentEvents factories so event names
    /// and field contracts stay consistent with docs/api/API.md.
    /// </summary>
    public static class AssessmentEventMapper
    {
        /// <summary>Worker identified the electrical fire hazard.</summary>
        public static ApiContracts.AssessmentEvent HazardIdentified(bool correct)
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.HazardIdentified(correct, "electrical_fire"));
        }

        /// <summary>Worker selected the correct fire extinguisher.</summary>
        public static ApiContracts.AssessmentEvent EquipmentSelected(bool correct, string action)
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.EquipmentSelected(correct, action));
        }

        /// <summary>Worker activated the fire alarm.</summary>
        public static ApiContracts.AssessmentEvent AlarmActivated()
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.EquipmentSelected(true, "activate_alarm"));
        }

        /// <summary>Worker removed the safety pin.</summary>
        public static ApiContracts.AssessmentEvent PinRemoved()
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.EquipmentSelected(true, "remove_safety_pin"));
        }

        /// <summary>Worker evacuated / found the exit.</summary>
        public static ApiContracts.AssessmentEvent EvacuationStarted(bool correct, string route)
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.EvacuationStarted(correct, route));
        }

        /// <summary>Worker made a wrong action (e.g., squeezed handle before pin).</summary>
        public static ApiContracts.AssessmentEvent WrongAction(bool major)
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.WrongAction(major ? "major" : "minor"));
        }

        /// <summary>Worker committed a critical safety violation.</summary>
        public static ApiContracts.AssessmentEvent CriticalAction(string action, string reason)
        {
            return AssessmentEvents.Timestamped(
                AssessmentEvents.CriticalAction(action, reason));
        }

        /// <summary>Training session started.</summary>
        public static ApiContracts.AssessmentEvent TrainingStarted()
        {
            return AssessmentEvents.Timestamped(
                new ApiContracts.AssessmentEvent { event_type = "training_started" });
        }

        /// <summary>Assessment session started.</summary>
        public static ApiContracts.AssessmentEvent AssessmentStarted()
        {
            return AssessmentEvents.Timestamped(
                new ApiContracts.AssessmentEvent { event_type = "assessment_started" });
        }

        /// <summary>Assessment session completed.</summary>
        public static ApiContracts.AssessmentEvent AssessmentCompleted()
        {
            return AssessmentEvents.Timestamped(
                new ApiContracts.AssessmentEvent { event_type = "assessment_completed" });
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Scripts\Networking\AssessmentEvents.cs

```csharp
using System;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Factories for well-formed assessment events that the backend ML competency
    /// engine (ml/competency) understands. Build an assessment by collecting these
    /// in chronological order and submitting them with SubmissionAssessment.
    ///
    /// Event naming and field contracts follow docs/api/API.md -> "Assessment
    /// Events (ML Competency Engine)" and ml/competency/scoring/engine.py.
    /// </summary>
    public static class AssessmentEvents
    {
        /// <summary>hazard_identified - correct = worker spotted the hazard.</summary>
        public static ApiContracts.AssessmentEvent HazardIdentified(bool correct, string hazardType)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "hazard_identified",
                hazard_type = hazardType,
                correct = correct,
            };
        }

        /// <summary>ppe_selected - correct + the chosen items.</summary>
        public static ApiContracts.AssessmentEvent PpeSelected(bool correct, params string[] items)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "ppe_selected",
                items = items ?? new string[0],
                correct = correct,
            };
        }

        /// <summary>equipment_selected - correct choice of tool / equipment.</summary>
        public static ApiContracts.AssessmentEvent EquipmentSelected(bool correct, string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "equipment_selected",
                action = action,
                correct = correct,
            };
        }

        /// <summary>evacuation_started - fire: route; gas: upwind direction.</summary>
        public static ApiContracts.AssessmentEvent EvacuationStarted(bool correct, string routeOrDirection)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "evacuation_started",
                route = routeOrDirection,
                direction = routeOrDirection,
                correct = correct,
            };
        }

        /// <summary>emergency_procedure (gas only) - e.g. alert_supervisor.</summary>
        public static ApiContracts.AssessmentEvent EmergencyProcedure(bool correct, string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "emergency_procedure",
                action = action,
                correct = correct,
            };
        }

        /// <summary>wrong_action - minor or major mistake (no hint mode in assessment).</summary>
        public static ApiContracts.AssessmentEvent WrongAction(string severity)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "wrong_action",
                severity = severity,
            };
        }

        /// <summary>
        /// critical_action - a safety violation. Triggers automatic FAIL on the
        /// server regardless of all other scores.
        /// </summary>
        public static ApiContracts.AssessmentEvent CriticalAction(string action, string reason)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "critical_action",
                action = action,
                reason = reason,
            };
        }

        /// <summary>Stamps an event with the current UTC time (ISO 8601).</summary>
        public static ApiContracts.AssessmentEvent Timestamped(ApiContracts.AssessmentEvent e)
        {
            e.timestamp = DateTime.UtcNow.ToString("o");
            return e;
        }
    }
}
```

---

## C:\project\surakshaAR\assets\Scripts\Networking\AssessmentManager.cs

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Assessment
{
    /// <summary>
    /// AssessmentManager — Controls assessment mode for fire safety AR.
    /// Disables training hints, records behavioural events, submits to backend.
    /// </summary>
    public class AssessmentManager : MonoBehaviour
    {
        [Header("API Client")]
        public AssessmentApiClient apiClient;

        [Header("Worker Info")]
        public int workerId = 1;
        public int moduleId = 1;

        [Header("Training Elements to Disable")]
        public GameObject hintBar;
        public GameObject stepIndicator;
        public GameObject[] trainingLabels;
        public GameObject[] trainingArrows;

        [Header("State")]
        public bool IsAssessmentActive { get; private set; }

        private List<ApiContracts.AssessmentEvent> collectedEvents = new List<ApiContracts.AssessmentEvent>();

        public void BeginAssessment()
        {
            if (IsAssessmentActive) return;
            IsAssessmentActive = true;
            collectedEvents.Clear();
            DisableTrainingAids();
            RecordEvent(AssessmentEventMapper.AssessmentStarted());
            Debug.Log("[AssessmentManager] Assessment BEGAN.");
        }

        public void EndAssessment()
        {
            if (!IsAssessmentActive) return;
            IsAssessmentActive = false;
            RecordEvent(AssessmentEventMapper.AssessmentCompleted());
            SubmitEvents();
            Debug.Log("[AssessmentManager] Assessment ENDED.");
        }

        public void RecordEvent(ApiContracts.AssessmentEvent evt)
        {
            collectedEvents.Add(evt);
            Debug.Log($"[AssessmentManager] Event: {evt.event_type}");
        }

        public void RecordHazardIdentified(bool correct)
        {
            RecordEvent(AssessmentEventMapper.HazardIdentified(correct));
        }

        public void RecordEquipmentSelected(bool correct, string action)
        {
            RecordEvent(AssessmentEventMapper.EquipmentSelected(correct, action));
        }

        public void RecordWrongAction(bool major)
        {
            RecordEvent(AssessmentEventMapper.WrongAction(major));
        }

        public void RecordCriticalAction(string action, string reason)
        {
            RecordEvent(AssessmentEventMapper.CriticalAction(action, reason));
        }

        public void RecordEvacuation(bool correct, string route)
        {
            RecordEvent(AssessmentEventMapper.EvacuationStarted(correct, route));
        }

        private void SubmitEvents()
        {
            if (apiClient == null)
            {
                Debug.LogWarning("[AssessmentManager] No API client. Saved locally.");
                SaveEventsLocally();
                return;
            }

            var assessment = new ApiContracts.AssessmentCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                session_type = "assessment",
                events = collectedEvents.ToArray(),
            };

            StartCoroutine(apiClient.SubmitAssessment(assessment,
                result =>
                {
                    Debug.Log($"[AssessmentManager] Score: {result.overall_score}, Pass: {result.passed}");
                    PlayerPrefs.SetString("last_assessment_result", JsonUtility.ToJson(result));
                    PlayerPrefs.Save();
                },
                error =>
                {
                    Debug.LogWarning($"[AssessmentManager] API error: {error}. Saved locally.");
                    SaveEventsLocally();
                }));
        }

        private void SaveEventsLocally()
        {
            var session = new ApiContracts.AssessmentCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                session_type = "assessment",
                events = collectedEvents.ToArray(),
            };
            string json = JsonUtility.ToJson(session);
            PlayerPrefs.SetString($"pending_assessment_{DateTime.UtcNow.Ticks}", json);
            PlayerPrefs.Save();
        }

        private void DisableTrainingAids()
        {
            if (hintBar != null) hintBar.SetActive(false);
            if (stepIndicator != null) stepIndicator.SetActive(false);
            if (trainingLabels != null)
                foreach (var l in trainingLabels)
                    if (l != null) l.SetActive(false);
            if (trainingArrows != null)
                foreach (var a in trainingArrows)
                    if (a != null) a.SetActive(false);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\Scripts\Networking\BackendSyncExample.cs

```csharp
using System.Collections;
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Example MonoBehaviour that demonstrates the full worker-app integration:
    /// health check -> worker registration -> progress -> event-based assessment
    /// (scored server-side by the ML competency engine) -> retraining plan ->
    /// vision PPE check -> offline-session sync.
    ///
    /// Attach it to the same GameObject that has SurakshaApiClient (or it will
    /// add one automatically).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BackendSyncExample : MonoBehaviour
    {
        [Header("Worker")]
        [SerializeField] private string workerName = "Ramesh Kumar";
        [SerializeField] private string employeeId = "EMP001";
        [SerializeField] private string role = "Fire Safety Worker";
        [SerializeField]
        [Tooltip("Used when the worker already exists (POST /workers returns 409).")]
        private int existingWorkerId = 0;

        [Header("Module / Scenario")]
        [Tooltip("1 = fire, 2 = gas")]
        [SerializeField] private int moduleId = 1;

        [Header("Assessment")]
        [Tooltip("false = submit a failing assessment (critical error) so the retraining plan is populated.")]
        [SerializeField] private bool passScenario = true;

        [Header("Vision (PPE check)")]
        [SerializeField] private bool runPpeCheck = true;

        [Header("Sync")]
        [SerializeField] private string deviceId = "device-xyz-789";

        // A minimal valid PNG (1x1) used purely to exercise the PPE check
        // pipeline end-to-end in mock mode (no camera capture in this example).
        private const string SamplePngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk" +
            "+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        private SurakshaApiClient api;
        private int workerId;

        private void Start()
        {
            api = SurakshaApiClient.Instance;
            if (api == null)
            {
                api = gameObject.AddComponent<SurakshaApiClient>();
            }

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // 1) Backend reachable?
            bool healthy = false;
            yield return api.CheckHealth(result =>
            {
                if (!result.Success)
                {
                    Debug.LogError("[SurakshaAR] Backend unreachable: " + result.Error);
                    return;
                }

                healthy = true;
                Debug.Log("[SurakshaAR] Backend health: OK");
            });

            if (!healthy)
            {
                yield break;
            }

            // 2) Register the worker (fall back to existingWorkerId on duplicate).
            bool hasWorker = false;
            var create = new ApiContracts.WorkerCreate
            {
                name = workerName,
                employee_id = employeeId,
                role = role,
            };

            yield return api.CreateWorker(create, result =>
            {
                if (result.Success)
                {
                    workerId = result.Data.id;
                    hasWorker = true;
                    Debug.Log("[SurakshaAR] Registered worker #" + workerId);
                }
                else if (result.StatusCode == 409 && existingWorkerId > 0)
                {
                    workerId = existingWorkerId;
                    hasWorker = true;
                    Debug.Log("[SurakshaAR] Worker already exists, using id " + workerId);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Worker registration failed: " + result.Error);
                }
            });

            if (!hasWorker)
            {
                yield break;
            }

            // 3) Record progress for the training workflow stage.
            var progress = new ApiContracts.ProgressCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                stage = "assess",
                status = "in_progress",
            };

            yield return api.UpdateProgress(progress, result =>
            {
                if (result.Success)
                {
                    Debug.Log("[SurakshaAR] Progress updated: " + result.Data.stage + " / " + result.Data.status);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Progress update failed: " + result.Error);
                }
            });

            // 4) Submit the event-based assessment (scored server-side by the ML engine).
            int assessmentId = 0;
            var assessment = new ApiContracts.AssessmentCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                events = BuildScenarioEvents(passScenario),
            };

            yield return api.SubmitAssessment(assessment, result =>
            {
                if (result.Success)
                {
                    assessmentId = result.Data.id;
                    Debug.Log("[SurakshaAR] Assessment #" + assessmentId + " score=" +
                              result.Data.score.ToString("0.0") + " passed=" + result.Data.passed);
                    Debug.Log("[SurakshaAR] Pass reason: " + result.Data.pass_reason);
                    LogCompetencyScores(result.Data.competency_scores);
                    LogWeaknesses(result.Data.weaknesses);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Assessment submission failed: " + result.Error);
                }
            });

            // 5) Fetch the targeted retraining plan (empty for a passing run).
            if (assessmentId > 0)
            {
                yield return api.GetRetrainingPlan(assessmentId, result =>
                {
                    if (result.Success)
                    {
                        Debug.Log("[SurakshaAR] Retraining plan: " +
                                  result.Data.weaknesses_addressed + "/" + result.Data.total_weaknesses +
                                  " weaknesses addressed in " + result.Data.total_estimated_duration_minutes + " min");
                        foreach (var module in result.Data.recommended_modules)
                        {
                            Debug.Log("[SurakshaAR]   -> " + module.name + " (" + module.estimated_duration_minutes + " min) " + module.reason);
                        }
                    }
                    else
                    {
                        Debug.LogError("[SurakshaAR] Retraining plan fetch failed: " + result.Error);
                    }
                });
            }

            // 6) Vision: PPE detection check (mock fallback until a model exists).
            if (runPpeCheck)
            {
                var ppe = new ApiContracts.PPECheckRequest
                {
                    image_base64 = SamplePngBase64,
                    required_ppe = new[] { "helmet", "safety_vest" },
                    mode = "auto",
                };

                yield return api.CheckPPE(ppe, result =>
                {
                    if (result.Success)
                    {
                        Debug.Log("[SurakshaAR] PPE check: status=" + result.Data.status +
                                  " ppe_ok=" + result.Data.ppe_ok +
                                  " fallback_used=" + result.Data.fallback_used);
                    }
                    else
                    {
                        Debug.LogError("[SurakshaAR] PPE check failed: " + result.Error);
                    }
                });
            }

            // 7) Sync an offline session to the backend.
            var sync = new ApiContracts.SyncCreate
            {
                worker_id = workerId,
                device_id = deviceId,
                sessions = new[]
                {
                    new ApiContracts.SyncSession
                    {
                        type = "assessment",
                        module_id = moduleId,
                        score = passScenario ? 90.0f : 39.0f,
                        passed = passScenario,
                        weaknesses = new string[0],
                        occurred_at = System.DateTime.UtcNow.ToString("o"),
                    },
                },
            };

            yield return api.SyncSessions(sync, result =>
            {
                if (result.Success)
                {
                    Debug.Log("[SurakshaAR] Synced " + result.Data.sessions_synced + " offline session(s)");
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Sync failed: " + result.Error);
                }
            });

            Debug.Log("[SurakshaAR] Integration demo finished.");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Build a minimal, well-formed scenario. The passing run covers all
        /// competencies; the failing run appends a critical safety violation so
        /// the server fails the assessment regardless of the other scores.
        /// </summary>
        private ApiContracts.AssessmentEvent[] BuildScenarioEvents(bool pass)
        {
            return new[]
            {
                AssessmentEvents.HazardIdentified(true, "electrical_fire"),
                AssessmentEvents.PpeSelected(true, "helmet", "gloves", "jacket"),
                AssessmentEvents.EquipmentSelected(true, "grab_extinguisher"),
                AssessmentEvents.EvacuationStarted(true, "north_exit"),
                // Append the safety violation for the failing run.
                pass
                    ? new ApiContracts.AssessmentEvent { event_type = "assessment_completed", completion_status = "success" }
                    : AssessmentEvents.CriticalAction("re_entered_unsafe_area", "CRITICAL: Re-entered the fire zone without clearance"),
            };
        }

        private static void LogCompetencyScores(ApiContracts.CompetencyScoreMap map)
        {
            if (map == null)
            {
                return;
            }

            LogScore(map.hazard_identification);
            LogScore(map.ppe_selection);
            LogScore(map.equipment_use);
            LogScore(map.procedure_compliance);
            LogScore(map.decision_making);
            LogScore(map.evacuation);
            LogScore(map.emergency_response);
        }

        private static void LogScore(ApiContracts.CompetencyScoreOut score)
        {
            if (score != null)
            {
                Debug.Log("[SurakshaAR]   competency " + score.name + ": " +
                          score.score.ToString("0.0") + " (pass=" + score.passed + ")");
            }
        }

        private static void LogWeaknesses(ApiContracts.WeaknessOut[] weaknesses)
        {
            if (weaknesses == null || weaknesses.Length == 0)
            {
                Debug.Log("[SurakshaAR]   weaknesses: none");
                return;
            }

            foreach (var w in weaknesses)
            {
                Debug.Log("[SurakshaAR]   weakness " + w.competency_name + ": " +
                          w.score.ToString("0.0") + " (" + w.severity + ") " + w.reason);
            }
        }
    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\Benchmark01.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class Benchmark01 : MonoBehaviour
    {

        public int BenchmarkType = 0;

        public TMP_FontAsset TMProFont;
        public Font TextMeshFont;

        private TextMeshPro m_textMeshPro;
        private TextContainer m_textContainer;
        private TextMesh m_textMesh;

        private const string label01 = "The <#0050FF>count is: </color>{0}";
        private const string label02 = "The <color=#0050FF>count is: </color>";

        //private string m_string;
        //private int m_frame;

        private Material m_material01;
        private Material m_material02;



        IEnumerator Start()
        {



            if (BenchmarkType == 0) // TextMesh Pro Component
            {
                m_textMeshPro = gameObject.AddComponent<TextMeshPro>();
                m_textMeshPro.autoSizeTextContainer = true;

                //m_textMeshPro.anchorDampening = true;

                if (TMProFont != null)
                    m_textMeshPro.font = TMProFont;

                //m_textMeshPro.font = Resources.Load("Fonts & Materials/Anton SDF", typeof(TextMeshProFont)) as TextMeshProFont; // Make sure the Anton SDF exists before calling this...
                //m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/Anton SDF", typeof(Material)) as Material; // Same as above make sure this material exists.

                m_textMeshPro.fontSize = 48;
                m_textMeshPro.alignment = TextAlignmentOptions.Center;
                //m_textMeshPro.anchor = AnchorPositions.Center;
                m_textMeshPro.extraPadding = true;
                //m_textMeshPro.outlineWidth = 0.25f;
                //m_textMeshPro.fontSharedMaterial.SetFloat("_OutlineWidth", 0.2f);
                //m_textMeshPro.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
                //m_textMeshPro.lineJustification = LineJustificationTypes.Center;
                m_textMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
                //m_textMeshPro.lineLength = 60;
                //m_textMeshPro.characterSpacing = 0.2f;
                //m_textMeshPro.fontColor = new Color32(255, 255, 255, 255);

                m_material01 = m_textMeshPro.font.material;
                m_material02 = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Drop Shadow"); // Make sure the LiberationSans SDF exists before calling this...


            }
            else if (BenchmarkType == 1) // TextMesh
            {
                m_textMesh = gameObject.AddComponent<TextMesh>();

                if (TextMeshFont != null)
                {
                    m_textMesh.font = TextMeshFont;
                    m_textMesh.GetComponent<Renderer>().sharedMaterial = m_textMesh.font.material;
                }
                else
                {
                    m_textMesh.font = Resources.Load("Fonts/ARIAL", typeof(Font)) as Font;
                    m_textMesh.GetComponent<Renderer>().sharedMaterial = m_textMesh.font.material;
                }

                m_textMesh.fontSize = 48;
                m_textMesh.anchor = TextAnchor.MiddleCenter;

                //m_textMesh.color = new Color32(255, 255, 0, 255);
            }



            for (int i = 0; i <= 1000000; i++)
            {
                if (BenchmarkType == 0)
                {
                    m_textMeshPro.SetText(label01, i % 1000);
                    if (i % 1000 == 999)
                        m_textMeshPro.fontSharedMaterial = m_textMeshPro.fontSharedMaterial == m_material01 ? m_textMeshPro.fontSharedMaterial = m_material02 : m_textMeshPro.fontSharedMaterial = m_material01;



                }
                else if (BenchmarkType == 1)
                    m_textMesh.text = label02 + (i % 1000).ToString();

                yield return null;
            }


            yield return null;
        }


        /*
        void Update()
        {
            if (BenchmarkType == 0)
            {
                m_textMeshPro.text = (m_frame % 1000).ToString();
            }
            else if (BenchmarkType == 1)
            {
                m_textMesh.text = (m_frame % 1000).ToString();
            }

            m_frame += 1;
        }
        */
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\Benchmark01_UGUI.cs

```csharp
using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace TMPro.Examples
{
    
    public class Benchmark01_UGUI : MonoBehaviour
    {

        public int BenchmarkType = 0;

        public Canvas canvas;
        public TMP_FontAsset TMProFont;
        public Font TextMeshFont;

        private TextMeshProUGUI m_textMeshPro;
        //private TextContainer m_textContainer;
        private Text m_textMesh;

        private const string label01 = "The <#0050FF>count is: </color>";
        private const string label02 = "The <color=#0050FF>count is: </color>";

        //private const string label01 = "TextMesh <#0050FF>Pro!</color>  The count is: {0}";
        //private const string label02 = "Text Mesh<color=#0050FF>        The count is: </color>";

        //private string m_string;
        //private int m_frame;

        private Material m_material01;
        private Material m_material02;



        IEnumerator Start()
        {



            if (BenchmarkType == 0) // TextMesh Pro Component
            {
                m_textMeshPro = gameObject.AddComponent<TextMeshProUGUI>();
                //m_textContainer = GetComponent<TextContainer>();


                //m_textMeshPro.anchorDampening = true;

                if (TMProFont != null)
                    m_textMeshPro.font = TMProFont;

                //m_textMeshPro.font = Resources.Load("Fonts & Materials/Anton SDF", typeof(TextMeshProFont)) as TextMeshProFont; // Make sure the Anton SDF exists before calling this...           
                //m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/Anton SDF", typeof(Material)) as Material; // Same as above make sure this material exists.

                m_textMeshPro.fontSize = 48;
                m_textMeshPro.alignment = TextAlignmentOptions.Center;
                //m_textMeshPro.anchor = AnchorPositions.Center;
                m_textMeshPro.extraPadding = true;
                //m_textMeshPro.outlineWidth = 0.25f;
                //m_textMeshPro.fontSharedMaterial.SetFloat("_OutlineWidth", 0.2f);
                //m_textMeshPro.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
                //m_textMeshPro.lineJustification = LineJustificationTypes.Center;
                //m_textMeshPro.enableWordWrapping = true;    
                //m_textMeshPro.lineLength = 60;          
                //m_textMeshPro.characterSpacing = 0.2f;
                //m_textMeshPro.fontColor = new Color32(255, 255, 255, 255);

                m_material01 = m_textMeshPro.font.material;
                m_material02 = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - BEVEL"); // Make sure the LiberationSans SDF exists before calling this...  


            }
            else if (BenchmarkType == 1) // TextMesh
            {
                m_textMesh = gameObject.AddComponent<Text>();

                if (TextMeshFont != null)
                {
                    m_textMesh.font = TextMeshFont;
                    //m_textMesh.renderer.sharedMaterial = m_textMesh.font.material;
                }
                else
                {
                    //m_textMesh.font = Resources.Load("Fonts/ARIAL", typeof(Font)) as Font;
                    //m_textMesh.renderer.sharedMaterial = m_textMesh.font.material;
                }

                m_textMesh.fontSize = 48;
                m_textMesh.alignment = TextAnchor.MiddleCenter;

                //m_textMesh.color = new Color32(255, 255, 0, 255);    
            }



            for (int i = 0; i <= 1000000; i++)
            {
                if (BenchmarkType == 0)
                {
                    m_textMeshPro.text = label01 + (i % 1000);
                    if (i % 1000 == 999)
                        m_textMeshPro.fontSharedMaterial = m_textMeshPro.fontSharedMaterial == m_material01 ? m_textMeshPro.fontSharedMaterial = m_material02 : m_textMeshPro.fontSharedMaterial = m_material01;



                }
                else if (BenchmarkType == 1)
                    m_textMesh.text = label02 + (i % 1000).ToString();

                yield return null;
            }


            yield return null;
        }


        /*
        void Update()
        {
            if (BenchmarkType == 0)
            {
                m_textMeshPro.text = (m_frame % 1000).ToString();            
            }
            else if (BenchmarkType == 1)
            {
                m_textMesh.text = (m_frame % 1000).ToString();
            }

            m_frame += 1;
        }
        */
    }

}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\Benchmark02.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class Benchmark02 : MonoBehaviour
    {

        public int SpawnType = 0;
        public int NumberOfNPC = 12;

        public bool IsTextObjectScaleStatic;
        private TextMeshProFloatingText floatingText_Script;


        void Start()
        {

            for (int i = 0; i < NumberOfNPC; i++)
            {


                if (SpawnType == 0)
                {
                    // TextMesh Pro Implementation
                    GameObject go = new GameObject();
                    go.transform.position = new Vector3(Random.Range(-95f, 95f), 0.25f, Random.Range(-95f, 95f));

                    TextMeshPro textMeshPro = go.AddComponent<TextMeshPro>();

                    textMeshPro.autoSizeTextContainer = true;
                    textMeshPro.rectTransform.pivot = new Vector2(0.5f, 0);

                    textMeshPro.alignment = TextAlignmentOptions.Bottom;
                    textMeshPro.fontSize = 96;
                    textMeshPro.fontFeatures.Clear();

                    textMeshPro.color = new Color32(255, 255, 0, 255);
                    textMeshPro.text = "!";
                    textMeshPro.isTextObjectScaleStatic = IsTextObjectScaleStatic;

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 0;
                    floatingText_Script.IsTextObjectScaleStatic = IsTextObjectScaleStatic;
                }
                else if (SpawnType == 1)
                {
                    // TextMesh Implementation
                    GameObject go = new GameObject();
                    go.transform.position = new Vector3(Random.Range(-95f, 95f), 0.25f, Random.Range(-95f, 95f));

                    TextMesh textMesh = go.AddComponent<TextMesh>();
                    textMesh.font = Resources.Load<Font>("Fonts/ARIAL");
                    textMesh.GetComponent<Renderer>().sharedMaterial = textMesh.font.material;

                    textMesh.anchor = TextAnchor.LowerCenter;
                    textMesh.fontSize = 96;

                    textMesh.color = new Color32(255, 255, 0, 255);
                    textMesh.text = "!";

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 1;
                }
                else if (SpawnType == 2)
                {
                    // Canvas WorldSpace Camera
                    GameObject go = new GameObject();
                    Canvas canvas = go.AddComponent<Canvas>();
                    canvas.worldCamera = Camera.main;

                    go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    go.transform.position = new Vector3(Random.Range(-95f, 95f), 5f, Random.Range(-95f, 95f));

                    TextMeshProUGUI textObject = new GameObject().AddComponent<TextMeshProUGUI>();
                    textObject.rectTransform.SetParent(go.transform, false);

                    textObject.color = new Color32(255, 255, 0, 255);
                    textObject.alignment = TextAlignmentOptions.Bottom;
                    textObject.fontSize = 96;
                    textObject.text = "!";

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 0;
                }



            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\Benchmark03.cs

```csharp
using UnityEngine;
using System.Collections;
using UnityEngine.TextCore.LowLevel;


namespace TMPro.Examples
{

    public class Benchmark03 : MonoBehaviour
    {
        public enum BenchmarkType { TMP_SDF_MOBILE = 0, TMP_SDF__MOBILE_SSD = 1, TMP_SDF = 2, TMP_BITMAP_MOBILE = 3, TEXTMESH_BITMAP = 4 }

        public int NumberOfSamples = 100;
        public BenchmarkType Benchmark;

        public Font SourceFont;


        void Awake()
        {

        }


        void Start()
        {
            TMP_FontAsset fontAsset = null;

            // Create Dynamic Font Asset for the given font file.
            switch (Benchmark)
            {
                case BenchmarkType.TMP_SDF_MOBILE:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, 90, 9, GlyphRenderMode.SDFAA, 256, 256, AtlasPopulationMode.Dynamic);
                    break;
                case BenchmarkType.TMP_SDF__MOBILE_SSD:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, 90, 9, GlyphRenderMode.SDFAA, 256, 256, AtlasPopulationMode.Dynamic);
                    fontAsset.material.shader = Shader.Find("TextMeshPro/Mobile/Distance Field SSD");
                    break;
                case BenchmarkType.TMP_SDF:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, 90, 9, GlyphRenderMode.SDFAA, 256, 256, AtlasPopulationMode.Dynamic);
                    fontAsset.material.shader = Shader.Find("TextMeshPro/Distance Field");
                    break;
                case BenchmarkType.TMP_BITMAP_MOBILE:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, 90, 9, GlyphRenderMode.SMOOTH, 256, 256, AtlasPopulationMode.Dynamic);
                    break;
            }

            for (int i = 0; i < NumberOfSamples; i++)
            {
                switch (Benchmark)
                {
                    case BenchmarkType.TMP_SDF_MOBILE:
                    case BenchmarkType.TMP_SDF__MOBILE_SSD:
                    case BenchmarkType.TMP_SDF:
                    case BenchmarkType.TMP_BITMAP_MOBILE:
                        {
                            GameObject go = new GameObject();
                            go.transform.position = new Vector3(0, 1.2f, 0);

                            TextMeshPro textComponent = go.AddComponent<TextMeshPro>();
                            textComponent.font = fontAsset;
                            textComponent.fontSize = 128;
                            textComponent.text = "@";
                            textComponent.alignment = TextAlignmentOptions.Center;
                            textComponent.color = new Color32(255, 255, 0, 255);

                            if (Benchmark == BenchmarkType.TMP_BITMAP_MOBILE)
                                textComponent.fontSize = 132;

                        }
                        break;
                    case BenchmarkType.TEXTMESH_BITMAP:
                        {
                            GameObject go = new GameObject();
                            go.transform.position = new Vector3(0, 1.2f, 0);

                            TextMesh textMesh = go.AddComponent<TextMesh>();
                            textMesh.GetComponent<Renderer>().sharedMaterial = SourceFont.material;
                            textMesh.font = SourceFont;
                            textMesh.anchor = TextAnchor.MiddleCenter;
                            textMesh.fontSize = 130;

                            textMesh.color = new Color32(255, 255, 0, 255);
                            textMesh.text = "@";
                        }
                        break;
                }
            }
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\Benchmark04.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class Benchmark04 : MonoBehaviour
    {

        public int SpawnType = 0;

        public int MinPointSize = 12;
        public int MaxPointSize = 64;
        public int Steps = 4;

        private Transform m_Transform;
        //private TextMeshProFloatingText floatingText_Script;
        //public Material material;


        void Start()
        {
            m_Transform = transform;

            float lineHeight = 0;
            float orthoSize = Camera.main.orthographicSize = Screen.height / 2;
            float ratio = (float)Screen.width / Screen.height;

            for (int i = MinPointSize; i <= MaxPointSize; i += Steps)
            {
                if (SpawnType == 0)
                {
                    // TextMesh Pro Implementation
                    GameObject go = new GameObject("Text - " + i + " Pts");

                    if (lineHeight > orthoSize * 2) return;

                    go.transform.position = m_Transform.position + new Vector3(ratio * -orthoSize * 0.975f, orthoSize * 0.975f - lineHeight, 0);

                    TextMeshPro textMeshPro = go.AddComponent<TextMeshPro>();

                    //textMeshPro.fontSharedMaterial = material;
                    //textMeshPro.font = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TextMeshProFont)) as TextMeshProFont;
                    //textMeshPro.anchor = AnchorPositions.Left;
                    textMeshPro.rectTransform.pivot = new Vector2(0, 0.5f);

                    textMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
                    textMeshPro.extraPadding = true;
                    textMeshPro.isOrthographic = true;
                    textMeshPro.fontSize = i;

                    textMeshPro.text = i + " pts - Lorem ipsum dolor sit...";
                    textMeshPro.color = new Color32(255, 255, 255, 255);

                    lineHeight += i;
                }
                else
                {
                    // TextMesh Implementation
                    // Causes crashes since atlas needed exceeds 4096 X 4096
                    /*
                    GameObject go = new GameObject("Arial " + i);

                    //if (lineHeight > orthoSize * 2 * 0.9f) return;

                    go.transform.position = m_Transform.position + new Vector3(ratio * -orthoSize * 0.975f, orthoSize * 0.975f - lineHeight, 1);

                    TextMesh textMesh = go.AddComponent<TextMesh>();
                    textMesh.font = Resources.Load("Fonts/ARIAL", typeof(Font)) as Font;
                    textMesh.renderer.sharedMaterial = textMesh.font.material;
                    textMesh.anchor = TextAnchor.MiddleLeft;
                    textMesh.fontSize = i * 10;

                    textMesh.color = new Color32(255, 255, 255, 255);
                    textMesh.text = i + " pts - Lorem ipsum dolor sit...";

                    lineHeight += i;
                    */
                }
            }
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\CameraController.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    
    public class CameraController : MonoBehaviour
    {
        public enum CameraModes { Follow, Isometric, Free }

        private Transform cameraTransform;
        private Transform dummyTarget;

        public Transform CameraTarget;

        public float FollowDistance = 30.0f;
        public float MaxFollowDistance = 100.0f;
        public float MinFollowDistance = 2.0f;

        public float ElevationAngle = 30.0f;
        public float MaxElevationAngle = 85.0f;
        public float MinElevationAngle = 0f;

        public float OrbitalAngle = 0f;

        public CameraModes CameraMode = CameraModes.Follow;

        public bool MovementSmoothing = true;
        public bool RotationSmoothing = false;
        private bool previousSmoothing;

        public float MovementSmoothingValue = 25f;
        public float RotationSmoothingValue = 5.0f;

        public float MoveSensitivity = 2.0f;

        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 desiredPosition;
        private float mouseX;
        private float mouseY;
        private Vector3 moveVector;
        private float mouseWheel;

        // Controls for Touches on Mobile devices
        //private float prev_ZoomDelta;


        private const string event_SmoothingValue = "Slider - Smoothing Value";
        private const string event_FollowDistance = "Slider - Camera Zoom";


        void Awake()
        {
            if (QualitySettings.vSyncCount > 0)
                Application.targetFrameRate = 60;
            else
                Application.targetFrameRate = -1;

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
                Input.simulateMouseWithTouches = false;

            cameraTransform = transform;
            previousSmoothing = MovementSmoothing;
        }


        // Use this for initialization
        void Start()
        {
            if (CameraTarget == null)
            {
                // If we don't have a target (assigned by the player, create a dummy in the center of the scene).
                dummyTarget = new GameObject("Camera Target").transform;
                CameraTarget = dummyTarget;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            GetPlayerInput();


            // Check if we still have a valid target
            if (CameraTarget != null)
            {
                if (CameraMode == CameraModes.Isometric)
                {
                    desiredPosition = CameraTarget.position + Quaternion.Euler(ElevationAngle, OrbitalAngle, 0f) * new Vector3(0, 0, -FollowDistance);
                }
                else if (CameraMode == CameraModes.Follow)
                {
                    desiredPosition = CameraTarget.position + CameraTarget.TransformDirection(Quaternion.Euler(ElevationAngle, OrbitalAngle, 0f) * (new Vector3(0, 0, -FollowDistance)));
                }
                else
                {
                    // Free Camera implementation
                }

                if (MovementSmoothing == true)
                {
                    // Using Smoothing
                    cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredPosition, ref currentVelocity, MovementSmoothingValue * Time.fixedDeltaTime);
                    //cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, Time.deltaTime * 5.0f);
                }
                else
                {
                    // Not using Smoothing
                    cameraTransform.position = desiredPosition;
                }

                if (RotationSmoothing == true)
                    cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.LookRotation(CameraTarget.position - cameraTransform.position), RotationSmoothingValue * Time.deltaTime);
                else
                {
                    cameraTransform.LookAt(CameraTarget);
                }

            }

        }



        void GetPlayerInput()
        {
            moveVector = Vector3.zero;

            // Check Mouse Wheel Input prior to Shift Key so we can apply multiplier on Shift for Scrolling
            mouseWheel = Input.GetAxis("Mouse ScrollWheel");

            float touchCount = Input.touchCount;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || touchCount > 0)
            {
                mouseWheel *= 10;

                if (Input.GetKeyDown(KeyCode.I))
                    CameraMode = CameraModes.Isometric;

                if (Input.GetKeyDown(KeyCode.F))
                    CameraMode = CameraModes.Follow;

                if (Input.GetKeyDown(KeyCode.S))
                    MovementSmoothing = !MovementSmoothing;


                // Check for right mouse button to change camera follow and elevation angle
                if (Input.GetMouseButton(1))
                {
                    mouseY = Input.GetAxis("Mouse Y");
                    mouseX = Input.GetAxis("Mouse X");

                    if (mouseY > 0.01f || mouseY < -0.01f)
                    {
                        ElevationAngle -= mouseY * MoveSensitivity;
                        // Limit Elevation angle between min & max values.
                        ElevationAngle = Mathf.Clamp(ElevationAngle, MinElevationAngle, MaxElevationAngle);
                    }

                    if (mouseX > 0.01f || mouseX < -0.01f)
                    {
                        OrbitalAngle += mouseX * MoveSensitivity;
                        if (OrbitalAngle > 360)
                            OrbitalAngle -= 360;
                        if (OrbitalAngle < 0)
                            OrbitalAngle += 360;
                    }
                }

                // Get Input from Mobile Device
                if (touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;

                    // Handle elevation changes
                    if (deltaPosition.y > 0.01f || deltaPosition.y < -0.01f)
                    {
                        ElevationAngle -= deltaPosition.y * 0.1f;
                        // Limit Elevation angle between min & max values.
                        ElevationAngle = Mathf.Clamp(ElevationAngle, MinElevationAngle, MaxElevationAngle);
                    }


                    // Handle left & right 
                    if (deltaPosition.x > 0.01f || deltaPosition.x < -0.01f)
                    {
                        OrbitalAngle += deltaPosition.x * 0.1f;
                        if (OrbitalAngle > 360)
                            OrbitalAngle -= 360;
                        if (OrbitalAngle < 0)
                            OrbitalAngle += 360;
                    }

                }

                // Check for left mouse button to select a new CameraTarget or to reset Follow position
                if (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 300, 1 << 10 | 1 << 11 | 1 << 12 | 1 << 14))
                    {
                        if (hit.transform == CameraTarget)
                        {
                            // Reset Follow Position
                            OrbitalAngle = 0;
                        }
                        else
                        {
                            CameraTarget = hit.transform;
                            OrbitalAngle = 0;
                            MovementSmoothing = previousSmoothing;
                        }

                    }
                }


                if (Input.GetMouseButton(2))
                {
                    if (dummyTarget == null)
                    {
                        // We need a Dummy Target to anchor the Camera
                        dummyTarget = new GameObject("Camera Target").transform;
                        dummyTarget.position = CameraTarget.position;
                        dummyTarget.rotation = CameraTarget.rotation;
                        CameraTarget = dummyTarget;
                        previousSmoothing = MovementSmoothing;
                        MovementSmoothing = false;
                    }
                    else if (dummyTarget != CameraTarget)
                    {
                        // Move DummyTarget to CameraTarget
                        dummyTarget.position = CameraTarget.position;
                        dummyTarget.rotation = CameraTarget.rotation;
                        CameraTarget = dummyTarget;
                        previousSmoothing = MovementSmoothing;
                        MovementSmoothing = false;
                    }


                    mouseY = Input.GetAxis("Mouse Y");
                    mouseX = Input.GetAxis("Mouse X");

                    moveVector = cameraTransform.TransformDirection(mouseX, mouseY, 0);

                    dummyTarget.Translate(-moveVector, Space.World);

                }

            }

            // Check Pinching to Zoom in - out on Mobile device
            if (touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                float prevTouchDelta = (touch0PrevPos - touch1PrevPos).magnitude;
                float touchDelta = (touch0.position - touch1.position).magnitude;

                float zoomDelta = prevTouchDelta - touchDelta;

                if (zoomDelta > 0.01f || zoomDelta < -0.01f)
                {
                    FollowDistance += zoomDelta * 0.25f;
                    // Limit FollowDistance between min & max values.
                    FollowDistance = Mathf.Clamp(FollowDistance, MinFollowDistance, MaxFollowDistance);
                }


            }

            // Check MouseWheel to Zoom in-out
            if (mouseWheel < -0.01f || mouseWheel > 0.01f)
            {

                FollowDistance -= mouseWheel * 5.0f;
                // Limit FollowDistance between min & max values.
                FollowDistance = Mathf.Clamp(FollowDistance, MinFollowDistance, MaxFollowDistance);
            }


        }
    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\ChatController.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatController : MonoBehaviour {


    public TMP_InputField ChatInputField;

    public TMP_Text ChatDisplayOutput;

    public Scrollbar ChatScrollbar;

    void OnEnable()
    {
        ChatInputField.onSubmit.AddListener(AddToChatOutput);
    }

    void OnDisable()
    {
        ChatInputField.onSubmit.RemoveListener(AddToChatOutput);
    }


    void AddToChatOutput(string newText)
    {
        // Clear Input Field
        ChatInputField.text = string.Empty;

        var timeNow = System.DateTime.Now;

        string formattedInput = "[<#FFFF80>" + timeNow.Hour.ToString("d2") + ":" + timeNow.Minute.ToString("d2") + ":" + timeNow.Second.ToString("d2") + "</color>] " + newText;

        if (ChatDisplayOutput != null)
        {
            // No special formatting for first entry
            // Add line feed before each subsequent entries
            if (ChatDisplayOutput.text == string.Empty)
                ChatDisplayOutput.text = formattedInput;
            else
                ChatDisplayOutput.text += "\n" + formattedInput;
        }

        // Keep Chat input field active
        ChatInputField.ActivateInputField();

        // Set the scrollbar to the bottom when next text is submitted.
        ChatScrollbar.value = 0;
    }

}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\DropdownSample.cs

```csharp
using TMPro;
using UnityEngine;

public class DropdownSample: MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text = null;

	[SerializeField]
	private TMP_Dropdown dropdownWithoutPlaceholder = null;

	[SerializeField]
	private TMP_Dropdown dropdownWithPlaceholder = null;

	public void OnButtonClick()
	{
		text.text = dropdownWithPlaceholder.value > -1 ? "Selected values:\n" + dropdownWithoutPlaceholder.value + " - " + dropdownWithPlaceholder.value : "Error: Please make a selection";
	}
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\EnvMapAnimator.cs

```csharp
using UnityEngine;
using System.Collections;
using TMPro;

public class EnvMapAnimator : MonoBehaviour {

    //private Vector3 TranslationSpeeds;
    public Vector3 RotationSpeeds;
    private TMP_Text m_textMeshPro;
    private Material m_material;
    

    void Awake()
    {
        //Debug.Log("Awake() on Script called.");
        m_textMeshPro = GetComponent<TMP_Text>();
        m_material = m_textMeshPro.fontSharedMaterial;
    }

    // Use this for initialization
	IEnumerator Start ()
    {
        Matrix4x4 matrix = new Matrix4x4(); 
        
        while (true)
        {
            //matrix.SetTRS(new Vector3 (Time.time * TranslationSpeeds.x, Time.time * TranslationSpeeds.y, Time.time * TranslationSpeeds.z), Quaternion.Euler(Time.time * RotationSpeeds.x, Time.time * RotationSpeeds.y , Time.time * RotationSpeeds.z), Vector3.one);
             matrix.SetTRS(Vector3.zero, Quaternion.Euler(Time.time * RotationSpeeds.x, Time.time * RotationSpeeds.y , Time.time * RotationSpeeds.z), Vector3.one);

            m_material.SetMatrix("_EnvMatrix", matrix);

            yield return null;
        }
	}
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\ObjectSpin.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class ObjectSpin : MonoBehaviour
    {
        #pragma warning disable 0414
        public enum MotionType { Rotation, SearchLight, Translation };
        public MotionType Motion;

        public Vector3 TranslationDistance = new Vector3(5, 0, 0);
        public float TranslationSpeed = 1.0f;
        public float SpinSpeed = 5;
        public int RotationRange = 15;
        private Transform m_transform;

        private float m_time;
        private Vector3 m_prevPOS;
        private Vector3 m_initial_Rotation;
        private Vector3 m_initial_Position;
        private Color32 m_lightColor;

        void Awake()
        {
            m_transform = transform;
            m_initial_Rotation = m_transform.rotation.eulerAngles;
            m_initial_Position = m_transform.position;

            Light light = GetComponent<Light>();
            m_lightColor = light != null ? light.color : Color.black;
        }


        // Update is called once per frame
        void Update()
        {
            switch (Motion)
            {
                case MotionType.Rotation:
                    m_transform.Rotate(0, SpinSpeed * Time.deltaTime, 0);
                    break;
                case MotionType.SearchLight:
                    m_time += SpinSpeed * Time.deltaTime;
                    m_transform.rotation = Quaternion.Euler(m_initial_Rotation.x, Mathf.Sin(m_time) * RotationRange + m_initial_Rotation.y, m_initial_Rotation.z);
                    break;
                case MotionType.Translation:
                    m_time += TranslationSpeed * Time.deltaTime;

                    float x = TranslationDistance.x * Mathf.Cos(m_time);
                    float y = TranslationDistance.y * Mathf.Sin(m_time) * Mathf.Cos(m_time * 1f);
                    float z = TranslationDistance.z * Mathf.Sin(m_time);

                    m_transform.position = m_initial_Position + new Vector3(x, z, y);

                    // Drawing light patterns because they can be cool looking.
                    //if (Time.frameCount > 1)
                    //    Debug.DrawLine(m_transform.position, m_prevPOS, m_lightColor, 100f);

                    m_prevPOS = m_transform.position;
                    break;
            }
        }
    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\ShaderPropAnimator.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    
    public class ShaderPropAnimator : MonoBehaviour
    {

        private Renderer m_Renderer;
        private Material m_Material;

        public AnimationCurve GlowCurve;

        public float m_frame;

        void Awake()
        {
            // Cache a reference to object's renderer
            m_Renderer = GetComponent<Renderer>();

            // Cache a reference to object's material and create an instance by doing so.
            m_Material = m_Renderer.material;
        }

        void Start()
        {
            StartCoroutine(AnimateProperties());
        }

        IEnumerator AnimateProperties()
        {
            //float lightAngle;
            float glowPower;
            m_frame = Random.Range(0f, 1f);

            while (true)
            {
                //lightAngle = (m_Material.GetFloat(ShaderPropertyIDs.ID_LightAngle) + Time.deltaTime) % 6.2831853f;
                //m_Material.SetFloat(ShaderPropertyIDs.ID_LightAngle, lightAngle);

                glowPower = GlowCurve.Evaluate(m_frame);
                m_Material.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);

                m_frame += Time.deltaTime * Random.Range(0.2f, 0.3f);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\SimpleScript.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class SimpleScript : MonoBehaviour
    {

        private TextMeshPro m_textMeshPro;
        //private TMP_FontAsset m_FontAsset;

        private const string label = "The <#0050FF>count is: </color>{0:2}";
        private float m_frame;


        void Start()
        {
            // Add new TextMesh Pro Component
            m_textMeshPro = gameObject.AddComponent<TextMeshPro>();

            m_textMeshPro.autoSizeTextContainer = true;

            // Load the Font Asset to be used.
            //m_FontAsset = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
            //m_textMeshPro.font = m_FontAsset;

            // Assign Material to TextMesh Pro Component
            //m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/LiberationSans SDF - Bevel", typeof(Material)) as Material;
            //m_textMeshPro.fontSharedMaterial.EnableKeyword("BEVEL_ON");

            // Set various font settings.
            m_textMeshPro.fontSize = 48;

            m_textMeshPro.alignment = TextAlignmentOptions.Center;

            //m_textMeshPro.anchorDampening = true; // Has been deprecated but under consideration for re-implementation.
            //m_textMeshPro.enableAutoSizing = true;

            //m_textMeshPro.characterSpacing = 0.2f;
            //m_textMeshPro.wordSpacing = 0.1f;

            //m_textMeshPro.enableCulling = true;
            m_textMeshPro.textWrappingMode = TextWrappingModes.NoWrap;

            //textMeshPro.fontColor = new Color32(255, 255, 255, 255);
        }


        void Update()
        {
            m_textMeshPro.SetText(label, m_frame % 1000);
            m_frame += 1 * Time.deltaTime;
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\SkewTextExample.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class SkewTextExample : MonoBehaviour
    {

        private TMP_Text m_TextComponent;

        public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
        //public float AngleMultiplier = 1.0f;
        //public float SpeedMultiplier = 1.0f;
        public float CurveScale = 1.0f;
        public float ShearAmount = 1.0f;

        void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
        }


        void Start()
        {
            StartCoroutine(WarpText());
        }


        private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
        {
            AnimationCurve newCurve = new AnimationCurve();

            newCurve.keys = curve.keys;

            return newCurve;
        }


        /// <summary>
        ///  Method to curve text along a Unity animation curve.
        /// </summary>
        /// <param name="textComponent"></param>
        /// <returns></returns>
        IEnumerator WarpText()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            //Mesh mesh = m_TextComponent.textInfo.meshInfo[0].mesh;

            Vector3[] vertices;
            Matrix4x4 matrix;

            m_TextComponent.havePropertiesChanged = true; // Need to force the TextMeshPro Object to be updated.
            CurveScale *= 10;
            float old_CurveScale = CurveScale;
            float old_ShearValue = ShearAmount;
            AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);

            while (true)
            {
                if (!m_TextComponent.havePropertiesChanged && old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value && old_ShearValue == ShearAmount)
                {
                    yield return null;
                    continue;
                }

                old_CurveScale = CurveScale;
                old_curve = CopyAnimationCurve(VertexCurve);
                old_ShearValue = ShearAmount;

                m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

                TMP_TextInfo textInfo = m_TextComponent.textInfo;
                int characterCount = textInfo.characterCount;


                if (characterCount == 0) continue;

                //vertices = textInfo.meshInfo[0].vertices;
                //int lastVertexIndex = textInfo.characterInfo[characterCount - 1].vertexIndex;

                float boundsMinX = m_TextComponent.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
                float boundsMaxX = m_TextComponent.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;



                for (int i = 0; i < characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Get the index of the mesh used by this character.
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    vertices = textInfo.meshInfo[materialIndex].vertices;

                    // Compute the baseline mid point for each character
                    Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                    //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                    // Apply offset to adjust our pivot point.
                    vertices[vertexIndex + 0] += -offsetToMidBaseline;
                    vertices[vertexIndex + 1] += -offsetToMidBaseline;
                    vertices[vertexIndex + 2] += -offsetToMidBaseline;
                    vertices[vertexIndex + 3] += -offsetToMidBaseline;

                    // Apply the Shearing FX
                    float shear_value = ShearAmount * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * (textInfo.characterInfo[i].topRight.y - textInfo.characterInfo[i].baseLine), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (textInfo.characterInfo[i].baseLine - textInfo.characterInfo[i].bottomRight.y), 0, 0);

                    vertices[vertexIndex + 0] += -bottomShear;
                    vertices[vertexIndex + 1] += topShear;
                    vertices[vertexIndex + 2] += topShear;
                    vertices[vertexIndex + 3] += -bottomShear;


                    // Compute the angle of rotation for each character based on the animation curve
                    float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                    float x1 = x0 + 0.0001f;
                    float y0 = VertexCurve.Evaluate(x0) * CurveScale;
                    float y1 = VertexCurve.Evaluate(x1) * CurveScale;

                    Vector3 horizontal = new Vector3(1, 0, 0);
                    //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                    Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

                    float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                    Vector3 cross = Vector3.Cross(horizontal, tangent);
                    float angle = cross.z > 0 ? dot : 360 - dot;

                    matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                    vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    vertices[vertexIndex + 0] += offsetToMidBaseline;
                    vertices[vertexIndex + 1] += offsetToMidBaseline;
                    vertices[vertexIndex + 2] += offsetToMidBaseline;
                    vertices[vertexIndex + 3] += offsetToMidBaseline;
                }


                // Upload the mesh with the revised information
                m_TextComponent.UpdateVertexData();

                yield return null; // new WaitForSeconds(0.025f);
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TeleType.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class TeleType : MonoBehaviour
    {


        //[Range(0, 100)]
        //public int RevealSpeed = 50;

        private string label01 = "Example <sprite=2> of using <sprite=7> <#ffa000>Graphics Inline</color> <sprite=5> with Text in <font=\"Bangers SDF\" material=\"Bangers SDF - Drop Shadow\">TextMesh<#40a0ff>Pro</color></font><sprite=0> and Unity<sprite=1>";
        private string label02 = "Example <sprite=2> of using <sprite=7> <#ffa000>Graphics Inline</color> <sprite=5> with Text in <font=\"Bangers SDF\" material=\"Bangers SDF - Drop Shadow\">TextMesh<#40a0ff>Pro</color></font><sprite=0> and Unity<sprite=2>";


        private TMP_Text m_textMeshPro;


        void Awake()
        {
            // Get Reference to TextMeshPro Component
            m_textMeshPro = GetComponent<TMP_Text>();
            m_textMeshPro.text = label01;
            m_textMeshPro.textWrappingMode = TextWrappingModes.Normal;
            m_textMeshPro.alignment = TextAlignmentOptions.Top;



            //if (GetComponentInParent(typeof(Canvas)) as Canvas == null)
            //{
            //    GameObject canvas = new GameObject("Canvas", typeof(Canvas));
            //    gameObject.transform.SetParent(canvas.transform);
            //    canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            //    // Set RectTransform Size
            //    gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 300);
            //    m_textMeshPro.fontSize = 48;
            //}


        }


        IEnumerator Start()
        {

            // Force and update of the mesh to get valid information.
            m_textMeshPro.ForceMeshUpdate();


            int totalVisibleCharacters = m_textMeshPro.textInfo.characterCount; // Get # of Visible Character in text object
            int counter = 0;
            int visibleCount = 0;

            while (true)
            {
                visibleCount = counter % (totalVisibleCharacters + 1);

                m_textMeshPro.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?

                // Once the last character has been revealed, wait 1.0 second and start over.
                if (visibleCount >= totalVisibleCharacters)
                {
                    yield return new WaitForSeconds(1.0f);
                    m_textMeshPro.text = label02;
                    yield return new WaitForSeconds(1.0f);
                    m_textMeshPro.text = label01;
                    yield return new WaitForSeconds(1.0f);
                }

                counter += 1;

                yield return new WaitForSeconds(0.05f);
            }

            //Debug.Log("Done revealing the text.");
        }

    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TextConsoleSimulator.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    public class TextConsoleSimulator : MonoBehaviour
    {
        private TMP_Text m_TextComponent;
        private bool hasTextChanged;

        void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
        }


        void Start()
        {
            StartCoroutine(RevealCharacters(m_TextComponent));
            //StartCoroutine(RevealWords(m_TextComponent));
        }


        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        // Event received when the text object has changed.
        void ON_TEXT_CHANGED(Object obj)
        {
            hasTextChanged = true;
        }


        /// <summary>
        /// Method revealing the text one character at a time.
        /// </summary>
        /// <returns></returns>
        IEnumerator RevealCharacters(TMP_Text textComponent)
        {
            textComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = textComponent.textInfo;

            int totalVisibleCharacters = textInfo.characterCount; // Get # of Visible Character in text object
            int visibleCount = 0;

            while (true)
            {
                if (hasTextChanged)
                {
                    totalVisibleCharacters = textInfo.characterCount; // Update visible character count.
                    hasTextChanged = false; 
                }

                if (visibleCount > totalVisibleCharacters)
                {
                    yield return new WaitForSeconds(1.0f);
                    visibleCount = 0;
                }

                textComponent.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?

                visibleCount += 1;

                yield return null;
            }
        }


        /// <summary>
        /// Method revealing the text one word at a time.
        /// </summary>
        /// <returns></returns>
        IEnumerator RevealWords(TMP_Text textComponent)
        {
            textComponent.ForceMeshUpdate();

            int totalWordCount = textComponent.textInfo.wordCount;
            int totalVisibleCharacters = textComponent.textInfo.characterCount; // Get # of Visible Character in text object
            int counter = 0;
            int currentWord = 0;
            int visibleCount = 0;

            while (true)
            {
                currentWord = counter % (totalWordCount + 1);

                // Get last character index for the current word.
                if (currentWord == 0) // Display no words.
                    visibleCount = 0;
                else if (currentWord < totalWordCount) // Display all other words with the exception of the last one.
                    visibleCount = textComponent.textInfo.wordInfo[currentWord - 1].lastCharacterIndex + 1;
                else if (currentWord == totalWordCount) // Display last word and all remaining characters.
                    visibleCount = totalVisibleCharacters;

                textComponent.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?

                // Once the last character has been revealed, wait 1.0 second and start over.
                if (visibleCount >= totalVisibleCharacters)
                {
                    yield return new WaitForSeconds(1.0f);
                }

                counter += 1;

                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TextMeshProFloatingText.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class TextMeshProFloatingText : MonoBehaviour
    {
        public Font TheFont;

        private GameObject m_floatingText;
        private TextMeshPro m_textMeshPro;
        private TextMesh m_textMesh;

        private Transform m_transform;
        private Transform m_floatingText_Transform;
        private Transform m_cameraTransform;

        Vector3 lastPOS = Vector3.zero;
        Quaternion lastRotation = Quaternion.identity;

        public int SpawnType;
        public bool IsTextObjectScaleStatic;

        //private int m_frame = 0;

        static WaitForEndOfFrame k_WaitForEndOfFrame = new WaitForEndOfFrame();
        static WaitForSeconds[] k_WaitForSecondsRandom = new WaitForSeconds[]
        {
            new WaitForSeconds(0.05f), new WaitForSeconds(0.1f), new WaitForSeconds(0.15f), new WaitForSeconds(0.2f), new WaitForSeconds(0.25f),
            new WaitForSeconds(0.3f), new WaitForSeconds(0.35f), new WaitForSeconds(0.4f), new WaitForSeconds(0.45f), new WaitForSeconds(0.5f),
            new WaitForSeconds(0.55f), new WaitForSeconds(0.6f), new WaitForSeconds(0.65f), new WaitForSeconds(0.7f), new WaitForSeconds(0.75f),
            new WaitForSeconds(0.8f), new WaitForSeconds(0.85f), new WaitForSeconds(0.9f), new WaitForSeconds(0.95f), new WaitForSeconds(1.0f),
        };

        void Awake()
        {
            m_transform = transform;
            m_floatingText = new GameObject(this.name + " floating text");

            // Reference to Transform is lost when TMP component is added since it replaces it by a RectTransform.
            //m_floatingText_Transform = m_floatingText.transform;
            //m_floatingText_Transform.position = m_transform.position + new Vector3(0, 15f, 0);

            m_cameraTransform = Camera.main.transform;
        }

        void Start()
        {
            if (SpawnType == 0)
            {
                // TextMesh Pro Implementation
                m_textMeshPro = m_floatingText.AddComponent<TextMeshPro>();
                m_textMeshPro.rectTransform.sizeDelta = new Vector2(3, 3);

                m_floatingText_Transform = m_floatingText.transform;
                m_floatingText_Transform.position = m_transform.position + new Vector3(0, 15f, 0);

                //m_textMeshPro.fontAsset = Resources.Load("Fonts & Materials/JOKERMAN SDF", typeof(TextMeshProFont)) as TextMeshProFont; // User should only provide a string to the resource.
                //m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(Material)) as Material;

                m_textMeshPro.alignment = TextAlignmentOptions.Center;
                m_textMeshPro.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                m_textMeshPro.fontSize = 24;
                //m_textMeshPro.enableExtraPadding = true;
                //m_textMeshPro.enableShadows = false;
                m_textMeshPro.fontFeatures.Clear();
                m_textMeshPro.text = string.Empty;
                m_textMeshPro.isTextObjectScaleStatic = IsTextObjectScaleStatic;

                StartCoroutine(DisplayTextMeshProFloatingText());
            }
            else if (SpawnType == 1)
            {
                //Debug.Log("Spawning TextMesh Objects.");

                m_floatingText_Transform = m_floatingText.transform;
                m_floatingText_Transform.position = m_transform.position + new Vector3(0, 15f, 0);

                m_textMesh = m_floatingText.AddComponent<TextMesh>();
                m_textMesh.font = Resources.Load<Font>("Fonts/ARIAL");
                m_textMesh.GetComponent<Renderer>().sharedMaterial = m_textMesh.font.material;
                m_textMesh.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                m_textMesh.anchor = TextAnchor.LowerCenter;
                m_textMesh.fontSize = 24;

                StartCoroutine(DisplayTextMeshFloatingText());
            }
            else if (SpawnType == 2)
            {

            }

        }


        //void Update()
        //{
        //    if (SpawnType == 0)
        //    {
        //        m_textMeshPro.SetText("{0}", m_frame);
        //    }
        //    else
        //    {
        //        m_textMesh.text = m_frame.ToString();
        //    }
        //    m_frame = (m_frame + 1) % 1000;

        //}


        public IEnumerator DisplayTextMeshProFloatingText()
        {
            float CountDuration = 2.0f; // How long is the countdown alive.
            float starting_Count = Random.Range(5f, 20f); // At what number is the counter starting at.
            float current_Count = starting_Count;

            Vector3 start_pos = m_floatingText_Transform.position;
            Color32 start_color = m_textMeshPro.color;
            float alpha = 255;
            int int_counter = 0;


            float fadeDuration = 3 / starting_Count * CountDuration;

            while (current_Count > 0)
            {
                current_Count -= (Time.deltaTime / CountDuration) * starting_Count;

                if (current_Count <= 3)
                {
                    //Debug.Log("Fading Counter ... " + current_Count.ToString("f2"));
                    alpha = Mathf.Clamp(alpha - (Time.deltaTime / fadeDuration) * 255, 0, 255);
                }

                int_counter = (int)current_Count;
                m_textMeshPro.text = int_counter.ToString();
                //m_textMeshPro.SetText("{0}", (int)current_Count);

                m_textMeshPro.color = new Color32(start_color.r, start_color.g, start_color.b, (byte)alpha);

                // Move the floating text upward each update
                m_floatingText_Transform.position += new Vector3(0, starting_Count * Time.deltaTime, 0);

                // Align floating text perpendicular to Camera.
                if (!lastPOS.Compare(m_cameraTransform.position, 1000) || !lastRotation.Compare(m_cameraTransform.rotation, 1000))
                {
                    lastPOS = m_cameraTransform.position;
                    lastRotation = m_cameraTransform.rotation;
                    m_floatingText_Transform.rotation = lastRotation;
                    Vector3 dir = m_transform.position - lastPOS;
                    m_transform.forward = new Vector3(dir.x, 0, dir.z);
                }

                yield return k_WaitForEndOfFrame;
            }

            //Debug.Log("Done Counting down.");

            yield return k_WaitForSecondsRandom[Random.Range(0, 19)];

            m_floatingText_Transform.position = start_pos;

            StartCoroutine(DisplayTextMeshProFloatingText());
        }


        public IEnumerator DisplayTextMeshFloatingText()
        {
            float CountDuration = 2.0f; // How long is the countdown alive.
            float starting_Count = Random.Range(5f, 20f); // At what number is the counter starting at.
            float current_Count = starting_Count;

            Vector3 start_pos = m_floatingText_Transform.position;
            Color32 start_color = m_textMesh.color;
            float alpha = 255;
            int int_counter = 0;

            float fadeDuration = 3 / starting_Count * CountDuration;

            while (current_Count > 0)
            {
                current_Count -= (Time.deltaTime / CountDuration) * starting_Count;

                if (current_Count <= 3)
                {
                    //Debug.Log("Fading Counter ... " + current_Count.ToString("f2"));
                    alpha = Mathf.Clamp(alpha - (Time.deltaTime / fadeDuration) * 255, 0, 255);
                }

                int_counter = (int)current_Count;
                m_textMesh.text = int_counter.ToString();
                //Debug.Log("Current Count:" + current_Count.ToString("f2"));

                m_textMesh.color = new Color32(start_color.r, start_color.g, start_color.b, (byte)alpha);

                // Move the floating text upward each update
                m_floatingText_Transform.position += new Vector3(0, starting_Count * Time.deltaTime, 0);

                // Align floating text perpendicular to Camera.
                if (!lastPOS.Compare(m_cameraTransform.position, 1000) || !lastRotation.Compare(m_cameraTransform.rotation, 1000))
                {
                    lastPOS = m_cameraTransform.position;
                    lastRotation = m_cameraTransform.rotation;
                    m_floatingText_Transform.rotation = lastRotation;
                    Vector3 dir = m_transform.position - lastPOS;
                    m_transform.forward = new Vector3(dir.x, 0, dir.z);
                }

                yield return k_WaitForEndOfFrame;
            }

            //Debug.Log("Done Counting down.");

            yield return k_WaitForSecondsRandom[Random.Range(0, 20)];

            m_floatingText_Transform.position = start_pos;

            StartCoroutine(DisplayTextMeshFloatingText());
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TextMeshSpawner.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    
    public class TextMeshSpawner : MonoBehaviour
    {

        public int SpawnType = 0;
        public int NumberOfNPC = 12;

        public Font TheFont;

        private TextMeshProFloatingText floatingText_Script;

        void Awake()
        {

        }

        void Start()
        {

            for (int i = 0; i < NumberOfNPC; i++)
            {
                if (SpawnType == 0)
                {
                    // TextMesh Pro Implementation     
                    //go.transform.localScale = new Vector3(2, 2, 2);
                    GameObject go = new GameObject(); //"NPC " + i);
                    go.transform.position = new Vector3(Random.Range(-95f, 95f), 0.5f, Random.Range(-95f, 95f));

                    //go.transform.position = new Vector3(0, 1.01f, 0);
                    //go.renderer.castShadows = false;
                    //go.renderer.receiveShadows = false;
                    //go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    TextMeshPro textMeshPro = go.AddComponent<TextMeshPro>();
                    //textMeshPro.FontAsset = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TextMeshProFont)) as TextMeshProFont;
                    //textMeshPro.anchor = AnchorPositions.Bottom;
                    textMeshPro.fontSize = 96;

                    textMeshPro.text = "!";
                    textMeshPro.color = new Color32(255, 255, 0, 255);
                    //textMeshPro.Text = "!";


                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 0;
                }
                else
                {
                    // TextMesh Implementation
                    GameObject go = new GameObject(); //"NPC " + i);
                    go.transform.position = new Vector3(Random.Range(-95f, 95f), 0.5f, Random.Range(-95f, 95f));

                    //go.transform.position = new Vector3(0, 1.01f, 0);

                    TextMesh textMesh = go.AddComponent<TextMesh>();
                    textMesh.GetComponent<Renderer>().sharedMaterial = TheFont.material;
                    textMesh.font = TheFont;
                    textMesh.anchor = TextAnchor.LowerCenter;
                    textMesh.fontSize = 96;

                    textMesh.color = new Color32(255, 255, 0, 255);
                    textMesh.text = "!";

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 1;
                }
            }
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_DigitValidator.cs

```csharp
using UnityEngine;
using System;


namespace TMPro
{
    /// <summary>
    /// EXample of a Custom Character Input Validator to only allow digits from 0 to 9.
    /// </summary>
    [Serializable]
    //[CreateAssetMenu(fileName = "InputValidator - Digits.asset", menuName = "TextMeshPro/Input Validators/Digits", order = 100)]
    public class TMP_DigitValidator : TMP_InputValidator
    {
        // Custom text input validation function
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (ch >= '0' && ch <= '9')
            {
                text += ch;
                pos += 1;
                return ch;
            }

            return (char)0;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_ExampleScript_01.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


namespace TMPro.Examples
{

    public class TMP_ExampleScript_01 : MonoBehaviour
    {
        public enum objectType { TextMeshPro = 0, TextMeshProUGUI = 1 };

        public objectType ObjectType;
        public bool isStatic;

        private TMP_Text m_text;

        //private TMP_InputField m_inputfield;


        private const string k_label = "The count is <#0080ff>{0}</color>";
        private int count;

        void Awake()
        {
            // Get a reference to the TMP text component if one already exists otherwise add one.
            // This example show the convenience of having both TMP components derive from TMP_Text. 
            if (ObjectType == 0)
                m_text = GetComponent<TextMeshPro>() ?? gameObject.AddComponent<TextMeshPro>();
            else
                m_text = GetComponent<TextMeshProUGUI>() ?? gameObject.AddComponent<TextMeshProUGUI>();

            // Load a new font asset and assign it to the text object.
            m_text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");

            // Load a new material preset which was created with the context menu duplicate.
            m_text.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/Anton SDF - Drop Shadow");

            // Set the size of the font.
            m_text.fontSize = 120;

            // Set the text
            m_text.text = "A <#0080ff>simple</color> line of text.";

            // Get the preferred width and height based on the supplied width and height as opposed to the actual size of the current text container.
            Vector2 size = m_text.GetPreferredValues(Mathf.Infinity, Mathf.Infinity);

            // Set the size of the RectTransform based on the new calculated values.
            m_text.rectTransform.sizeDelta = new Vector2(size.x, size.y);
        }


        void Update()
        {
            if (!isStatic)
            {
                m_text.SetText(k_label, count % 1000);
                count += 1;
            }
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_FrameRateCounter.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class TMP_FrameRateCounter : MonoBehaviour
    {
        public float UpdateInterval = 5.0f;
        private float m_LastInterval = 0;
        private int m_Frames = 0;

        public enum FpsCounterAnchorPositions { TopLeft, BottomLeft, TopRight, BottomRight };

        public FpsCounterAnchorPositions AnchorPosition = FpsCounterAnchorPositions.TopRight;

        private string htmlColorTag;
        private const string fpsLabel = "{0:2}</color> <#8080ff>FPS \n<#FF8000>{1:2} <#8080ff>MS";

        private TextMeshPro m_TextMeshPro;
        private Transform m_frameCounter_transform;
        private Camera m_camera;

        private FpsCounterAnchorPositions last_AnchorPosition;

        void Awake()
        {
            if (!enabled)
                return;

            m_camera = Camera.main;
            Application.targetFrameRate = 9999;

            GameObject frameCounter = new GameObject("Frame Counter");

            m_TextMeshPro = frameCounter.AddComponent<TextMeshPro>();
            m_TextMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            m_TextMeshPro.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Overlay");


            m_frameCounter_transform = frameCounter.transform;
            m_frameCounter_transform.SetParent(m_camera.transform);
            m_frameCounter_transform.localRotation = Quaternion.identity;

            m_TextMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            m_TextMeshPro.fontSize = 24;
            //m_TextMeshPro.FontColor = new Color32(255, 255, 255, 128);
            //m_TextMeshPro.edgeWidth = .15f;
            //m_TextMeshPro.isOverlay = true;

            //m_TextMeshPro.FaceColor = new Color32(255, 128, 0, 0);
            //m_TextMeshPro.EdgeColor = new Color32(0, 255, 0, 255);
            //m_TextMeshPro.FontMaterial.renderQueue = 4000;

            //m_TextMeshPro.CreateSoftShadowClone(new Vector2(1f, -1f));

            Set_FrameCounter_Position(AnchorPosition);
            last_AnchorPosition = AnchorPosition;


        }

        void Start()
        {
            m_LastInterval = Time.realtimeSinceStartup;
            m_Frames = 0;
        }

        void Update()
        {
            if (AnchorPosition != last_AnchorPosition)
                Set_FrameCounter_Position(AnchorPosition);

            last_AnchorPosition = AnchorPosition;

            m_Frames += 1;
            float timeNow = Time.realtimeSinceStartup;

            if (timeNow > m_LastInterval + UpdateInterval)
            {
                // display two fractional digits (f2 format)
                float fps = m_Frames / (timeNow - m_LastInterval);
                float ms = 1000.0f / Mathf.Max(fps, 0.00001f);

                if (fps < 30)
                    htmlColorTag = "<color=yellow>";
                else if (fps < 10)
                    htmlColorTag = "<color=red>";
                else
                    htmlColorTag = "<color=green>";

                //string format = System.String.Format(htmlColorTag + "{0:F2} </color>FPS \n{1:F2} <#8080ff>MS",fps, ms);
                //m_TextMeshPro.text = format;

                m_TextMeshPro.SetText(htmlColorTag + fpsLabel, fps, ms);

                m_Frames = 0;
                m_LastInterval = timeNow;
            }
        }


        void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)
        {
            //Debug.Log("Changing frame counter anchor position.");
            m_TextMeshPro.margin = new Vector4(1f, 1f, 1f, 1f);

            switch (anchor_position)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopLeft;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(0, 1);
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(0, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomLeft;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(0, 0);
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(0, 0, 100.0f));
                    break;
                case FpsCounterAnchorPositions.TopRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopRight;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(1, 1);
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(1, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomRight;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(1, 0);
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(1, 0, 100.0f));
                    break;
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_PhoneNumberValidator.cs

```csharp
using UnityEngine;
using System.Collections;
using System;

namespace TMPro
{
    /// <summary>
    /// Example of a Custom Character Input Validator to only allow phone number in the (800) 555-1212 format.
    /// </summary>
    [Serializable]
    //[CreateAssetMenu(fileName = "InputValidator - Phone Numbers.asset", menuName = "TextMeshPro/Input Validators/Phone Numbers")]
    public class TMP_PhoneNumberValidator : TMP_InputValidator
    {
        // Custom text input validation function
        public override char Validate(ref string text, ref int pos, char ch)
        {
            Debug.Log("Trying to validate...");
            
            // Return unless the character is a valid digit
            if (ch < '0' && ch > '9') return (char)0;

            int length = text.Length;

            // Enforce Phone Number format for every character input.
            for (int i = 0; i < length + 1; i++)
            {
                switch (i)
                {
                    case 0:
                        if (i == length)
                            text = "(" + ch;
                        pos = 2;
                        break;
                    case 1:
                        if (i == length)
                            text += ch;
                        pos = 2;
                        break;
                    case 2:
                        if (i == length)
                            text += ch;
                        pos = 3;
                        break;
                    case 3:
                        if (i == length)
                            text += ch + ") ";
                        pos = 6;
                        break;
                    case 4:
                        if (i == length)
                            text += ") " + ch;
                        pos = 7;
                        break;
                    case 5:
                        if (i == length)
                            text += " " + ch;
                        pos = 7;
                        break;
                    case 6:
                        if (i == length)
                            text += ch;
                        pos = 7;
                        break;
                    case 7:
                        if (i == length)
                            text += ch;
                        pos = 8;
                        break;
                    case 8:
                        if (i == length)
                            text += ch + "-";
                        pos = 10;
                        break;
                    case 9:
                        if (i == length)
                            text += "-" + ch;
                        pos = 11;
                        break;
                    case 10:
                        if (i == length)
                            text += ch;
                        pos = 11;
                        break;
                    case 11:
                        if (i == length)
                            text += ch;
                        pos = 12;
                        break;
                    case 12:
                        if (i == length)
                            text += ch;
                        pos = 13;
                        break;
                    case 13:
                        if (i == length)
                            text += ch;
                        pos = 14;
                        break;
                }
            }

            return ch;
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_TextEventCheck.cs

```csharp
using UnityEngine;


namespace TMPro.Examples
{
    public class TMP_TextEventCheck : MonoBehaviour
    {

        public TMP_TextEventHandler TextEventHandler;

        private TMP_Text m_TextComponent;

        void OnEnable()
        {
            if (TextEventHandler != null)
            {
                // Get a reference to the text component
                m_TextComponent = TextEventHandler.GetComponent<TMP_Text>();
                
                TextEventHandler.onCharacterSelection.AddListener(OnCharacterSelection);
                TextEventHandler.onSpriteSelection.AddListener(OnSpriteSelection);
                TextEventHandler.onWordSelection.AddListener(OnWordSelection);
                TextEventHandler.onLineSelection.AddListener(OnLineSelection);
                TextEventHandler.onLinkSelection.AddListener(OnLinkSelection);
            }
        }


        void OnDisable()
        {
            if (TextEventHandler != null)
            {
                TextEventHandler.onCharacterSelection.RemoveListener(OnCharacterSelection);
                TextEventHandler.onSpriteSelection.RemoveListener(OnSpriteSelection);
                TextEventHandler.onWordSelection.RemoveListener(OnWordSelection);
                TextEventHandler.onLineSelection.RemoveListener(OnLineSelection);
                TextEventHandler.onLinkSelection.RemoveListener(OnLinkSelection);
            }
        }


        void OnCharacterSelection(char c, int index)
        {
            Debug.Log("Character [" + c + "] at Index: " + index + " has been selected.");
        }

        void OnSpriteSelection(char c, int index)
        {
            Debug.Log("Sprite [" + c + "] at Index: " + index + " has been selected.");
        }

        void OnWordSelection(string word, int firstCharacterIndex, int length)
        {
            Debug.Log("Word [" + word + "] with first character index of " + firstCharacterIndex + " and length of " + length + " has been selected.");
        }

        void OnLineSelection(string lineText, int firstCharacterIndex, int length)
        {
            Debug.Log("Line [" + lineText + "] with first character index of " + firstCharacterIndex + " and length of " + length + " has been selected.");
        }

        void OnLinkSelection(string linkID, string linkText, int linkIndex)
        {
            if (m_TextComponent != null)
            {
                TMP_LinkInfo linkInfo = m_TextComponent.textInfo.linkInfo[linkIndex];
            }
            
            Debug.Log("Link Index: " + linkIndex + " with ID [" + linkID + "] and Text \"" + linkText + "\" has been selected.");
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_TextEventHandler.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;


namespace TMPro
{

    public class TMP_TextEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class CharacterSelectionEvent : UnityEvent<char, int> { }

        [Serializable]
        public class SpriteSelectionEvent : UnityEvent<char, int> { }

        [Serializable]
        public class WordSelectionEvent : UnityEvent<string, int, int> { }

        [Serializable]
        public class LineSelectionEvent : UnityEvent<string, int, int> { }

        [Serializable]
        public class LinkSelectionEvent : UnityEvent<string, string, int> { }


        /// <summary>
        /// Event delegate triggered when pointer is over a character.
        /// </summary>
        public CharacterSelectionEvent onCharacterSelection
        {
            get { return m_OnCharacterSelection; }
            set { m_OnCharacterSelection = value; }
        }
        [SerializeField]
        private CharacterSelectionEvent m_OnCharacterSelection = new CharacterSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a sprite.
        /// </summary>
        public SpriteSelectionEvent onSpriteSelection
        {
            get { return m_OnSpriteSelection; }
            set { m_OnSpriteSelection = value; }
        }
        [SerializeField]
        private SpriteSelectionEvent m_OnSpriteSelection = new SpriteSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a word.
        /// </summary>
        public WordSelectionEvent onWordSelection
        {
            get { return m_OnWordSelection; }
            set { m_OnWordSelection = value; }
        }
        [SerializeField]
        private WordSelectionEvent m_OnWordSelection = new WordSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a line.
        /// </summary>
        public LineSelectionEvent onLineSelection
        {
            get { return m_OnLineSelection; }
            set { m_OnLineSelection = value; }
        }
        [SerializeField]
        private LineSelectionEvent m_OnLineSelection = new LineSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a link.
        /// </summary>
        public LinkSelectionEvent onLinkSelection
        {
            get { return m_OnLinkSelection; }
            set { m_OnLinkSelection = value; }
        }
        [SerializeField]
        private LinkSelectionEvent m_OnLinkSelection = new LinkSelectionEvent();



        private TMP_Text m_TextComponent;

        private Camera m_Camera;
        private Canvas m_Canvas;

        private int m_selectedLink = -1;
        private int m_lastCharIndex = -1;
        private int m_lastWordIndex = -1;
        private int m_lastLineIndex = -1;

        void Awake()
        {
            // Get a reference to the text component.
            m_TextComponent = gameObject.GetComponent<TMP_Text>();

            // Get a reference to the camera rendering the text taking into consideration the text component type.
            if (m_TextComponent.GetType() == typeof(TextMeshProUGUI))
            {
                m_Canvas = gameObject.GetComponentInParent<Canvas>();
                if (m_Canvas != null)
                {
                    if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        m_Camera = null;
                    else
                        m_Camera = m_Canvas.worldCamera;
                }
            }
            else
            {
                m_Camera = Camera.main;
            }
        }


        void LateUpdate()
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(m_TextComponent.rectTransform, Input.mousePosition, m_Camera))
            {
                #region Nearest Character
                /*int charIndex = TMP_TextUtilities.FindNearestCharacterOnLine(m_TextComponent, Input.mousePosition, 0, m_Camera, false);
                if (charIndex != -1 && charIndex != m_lastCharIndex)
                {
                    m_lastCharIndex = charIndex;
                }*/
                #endregion


                #region Example of Character or Sprite Selection
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(m_TextComponent, Input.mousePosition, m_Camera, true);
                if (charIndex != -1 && charIndex != m_lastCharIndex)
                {
                    m_lastCharIndex = charIndex;

                    TMP_TextElementType elementType = m_TextComponent.textInfo.characterInfo[charIndex].elementType;

                    // Send event to any event listeners depending on whether it is a character or sprite.
                    if (elementType == TMP_TextElementType.Character)
                        SendOnCharacterSelection(m_TextComponent.textInfo.characterInfo[charIndex].character, charIndex);
                    else if (elementType == TMP_TextElementType.Sprite)
                        SendOnSpriteSelection(m_TextComponent.textInfo.characterInfo[charIndex].character, charIndex);
                }
                #endregion


                #region Example of Word Selection
                // Check if Mouse intersects any words and if so assign a random color to that word.
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextComponent, Input.mousePosition, m_Camera);
                if (wordIndex != -1 && wordIndex != m_lastWordIndex)
                {
                    m_lastWordIndex = wordIndex;

                    // Get the information about the selected word.
                    TMP_WordInfo wInfo = m_TextComponent.textInfo.wordInfo[wordIndex];

                    // Send the event to any listeners.
                    SendOnWordSelection(wInfo.GetWord(), wInfo.firstCharacterIndex, wInfo.characterCount);
                }
                #endregion


                #region Example of Line Selection
                // Check if Mouse intersects any words and if so assign a random color to that word.
                int lineIndex = TMP_TextUtilities.FindIntersectingLine(m_TextComponent, Input.mousePosition, m_Camera);
                if (lineIndex != -1 && lineIndex != m_lastLineIndex)
                {
                    m_lastLineIndex = lineIndex;

                    // Get the information about the selected word.
                    TMP_LineInfo lineInfo = m_TextComponent.textInfo.lineInfo[lineIndex];

                    // Send the event to any listeners.
                    char[] buffer = new char[lineInfo.characterCount];
                    for (int i = 0; i < lineInfo.characterCount && i < m_TextComponent.textInfo.characterInfo.Length; i++)
                    {
                        buffer[i] = m_TextComponent.textInfo.characterInfo[i + lineInfo.firstCharacterIndex].character;
                    }

                    string lineText = new string(buffer);
                    SendOnLineSelection(lineText, lineInfo.firstCharacterIndex, lineInfo.characterCount);
                }
                #endregion


                #region Example of Link Handling
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, Input.mousePosition, m_Camera);

                // Handle new Link selection.
                if (linkIndex != -1 && linkIndex != m_selectedLink)
                {
                    m_selectedLink = linkIndex;

                    // Get information about the link.
                    TMP_LinkInfo linkInfo = m_TextComponent.textInfo.linkInfo[linkIndex];

                    // Send the event to any listeners.
                    SendOnLinkSelection(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkIndex);
                }
                #endregion
            }
            else
            {
                // Reset all selections given we are hovering outside the text container bounds.
                m_selectedLink = -1;
                m_lastCharIndex = -1;
                m_lastWordIndex = -1;
                m_lastLineIndex = -1;
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("OnPointerEnter()");
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("OnPointerExit()");
        }


        private void SendOnCharacterSelection(char character, int characterIndex)
        {
            if (onCharacterSelection != null)
                onCharacterSelection.Invoke(character, characterIndex);
        }

        private void SendOnSpriteSelection(char character, int characterIndex)
        {
            if (onSpriteSelection != null)
                onSpriteSelection.Invoke(character, characterIndex);
        }

        private void SendOnWordSelection(string word, int charIndex, int length)
        {
            if (onWordSelection != null)
                onWordSelection.Invoke(word, charIndex, length);
        }

        private void SendOnLineSelection(string line, int charIndex, int length)
        {
            if (onLineSelection != null)
                onLineSelection.Invoke(line, charIndex, length);
        }

        private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)
        {
            if (onLinkSelection != null)
                onLinkSelection.Invoke(linkID, linkText, linkIndex);
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_TextInfoDebugTool.cs

```csharp
using System;
using UnityEngine;
using System.Collections;
using UnityEditor;


namespace TMPro.Examples
{

    public class TMP_TextInfoDebugTool : MonoBehaviour
    {
        // Since this script is used for debugging, we exclude it from builds.
        // TODO: Rework this script to make it into an editor utility.
        #if UNITY_EDITOR
        public bool ShowCharacters;
        public bool ShowWords;
        public bool ShowLinks;
        public bool ShowLines;
        public bool ShowMeshBounds;
        public bool ShowTextBounds;
        [Space(10)]
        [TextArea(2, 2)]
        public string ObjectStats;

        [SerializeField]
        private TMP_Text m_TextComponent;

        private Transform m_Transform;
        private TMP_TextInfo m_TextInfo;

        private float m_ScaleMultiplier;
        private float m_HandleSize;


        void OnDrawGizmos()
        {
            if (m_TextComponent == null)
            {
                m_TextComponent = GetComponent<TMP_Text>();

                if (m_TextComponent == null)
                    return;
            }

            m_Transform = m_TextComponent.transform;

            // Get a reference to the text object's textInfo
            m_TextInfo = m_TextComponent.textInfo;

            // Update Text Statistics
            ObjectStats = "Characters: " + m_TextInfo.characterCount + "   Words: " + m_TextInfo.wordCount + "   Spaces: " + m_TextInfo.spaceCount + "   Sprites: " + m_TextInfo.spriteCount + "   Links: " + m_TextInfo.linkCount
                          + "\nLines: " + m_TextInfo.lineCount + "   Pages: " + m_TextInfo.pageCount;

            // Get the handle size for drawing the various
            m_ScaleMultiplier = m_TextComponent.GetType() == typeof(TextMeshPro) ? 1 : 0.1f;
            m_HandleSize = HandleUtility.GetHandleSize(m_Transform.position) * m_ScaleMultiplier;

            // Draw line metrics
            #region Draw Lines
            if (ShowLines)
                DrawLineBounds();
            #endregion

            // Draw word metrics
            #region Draw Words
            if (ShowWords)
                DrawWordBounds();
            #endregion

            // Draw character metrics
            #region Draw Characters
            if (ShowCharacters)
                DrawCharactersBounds();
            #endregion

            // Draw Quads around each of the words
            #region Draw Links
            if (ShowLinks)
                DrawLinkBounds();
            #endregion

            // Draw Quad around the bounds of the text
            #region Draw Bounds
            if (ShowMeshBounds)
                DrawBounds();
            #endregion

            // Draw Quad around the rendered region of the text.
            #region Draw Text Bounds
            if (ShowTextBounds)
                DrawTextBounds();
            #endregion
        }


        /// <summary>
        /// Method to draw a rectangle around each character.
        /// </summary>
        /// <param name="text"></param>
        void DrawCharactersBounds()
        {
            int characterCount = m_TextInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                // Draw visible as well as invisible characters
                TMP_CharacterInfo characterInfo = m_TextInfo.characterInfo[i];

                bool isCharacterVisible = i < m_TextComponent.maxVisibleCharacters &&
                                          characterInfo.lineNumber < m_TextComponent.maxVisibleLines &&
                                          i >= m_TextComponent.firstVisibleCharacter;

                if (m_TextComponent.overflowMode == TextOverflowModes.Page)
                    isCharacterVisible = isCharacterVisible && characterInfo.pageNumber + 1 == m_TextComponent.pageToDisplay;

                if (!isCharacterVisible)
                    continue;

                float dottedLineSize = 6;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bottomLeft = m_Transform.TransformPoint(characterInfo.bottomLeft);
                Vector3 topLeft = m_Transform.TransformPoint(new Vector3(characterInfo.topLeft.x, characterInfo.topLeft.y, 0));
                Vector3 topRight = m_Transform.TransformPoint(characterInfo.topRight);
                Vector3 bottomRight = m_Transform.TransformPoint(new Vector3(characterInfo.bottomRight.x, characterInfo.bottomRight.y, 0));

                // Draw character bounds
                if (characterInfo.isVisible)
                {
                    Color color = Color.green;
                    DrawDottedRectangle(bottomLeft, topRight, color);
                }
                else
                {
                    Color color = Color.grey;

                    float whiteSpaceAdvance = Math.Abs(characterInfo.origin - characterInfo.xAdvance) > 0.01f ? characterInfo.xAdvance : characterInfo.origin + (characterInfo.ascender - characterInfo.descender) * 0.03f;
                    DrawDottedRectangle(m_Transform.TransformPoint(new Vector3(characterInfo.origin, characterInfo.descender, 0)), m_Transform.TransformPoint(new Vector3(whiteSpaceAdvance, characterInfo.ascender, 0)), color, 4);
                }

                float origin = characterInfo.origin;
                float advance = characterInfo.xAdvance;
                float ascentline = characterInfo.ascender;
                float baseline = characterInfo.baseLine;
                float descentline = characterInfo.descender;

                //Draw Ascent line
                Vector3 ascentlineStart = m_Transform.TransformPoint(new Vector3(origin, ascentline, 0));
                Vector3 ascentlineEnd = m_Transform.TransformPoint(new Vector3(advance, ascentline, 0));

                Handles.color = Color.cyan;
                Handles.DrawDottedLine(ascentlineStart, ascentlineEnd, dottedLineSize);

                // Draw Cap Height & Mean line
                float capline = characterInfo.fontAsset == null ? 0 : baseline + characterInfo.fontAsset.faceInfo.capLine * characterInfo.scale;
                Vector3 capHeightStart = new Vector3(topLeft.x, m_Transform.TransformPoint(new Vector3(0, capline, 0)).y, 0);
                Vector3 capHeightEnd = new Vector3(topRight.x, m_Transform.TransformPoint(new Vector3(0, capline, 0)).y, 0);

                float meanline = characterInfo.fontAsset == null ? 0 : baseline + characterInfo.fontAsset.faceInfo.meanLine * characterInfo.scale;
                Vector3 meanlineStart = new Vector3(topLeft.x, m_Transform.TransformPoint(new Vector3(0, meanline, 0)).y, 0);
                Vector3 meanlineEnd = new Vector3(topRight.x, m_Transform.TransformPoint(new Vector3(0, meanline, 0)).y, 0);

                if (characterInfo.isVisible)
                {
                    // Cap line
                    Handles.color = Color.cyan;
                    Handles.DrawDottedLine(capHeightStart, capHeightEnd, dottedLineSize);

                    // Mean line
                    Handles.color = Color.cyan;
                    Handles.DrawDottedLine(meanlineStart, meanlineEnd, dottedLineSize);
                }

                //Draw Base line
                Vector3 baselineStart = m_Transform.TransformPoint(new Vector3(origin, baseline, 0));
                Vector3 baselineEnd = m_Transform.TransformPoint(new Vector3(advance, baseline, 0));

                Handles.color = Color.cyan;
                Handles.DrawDottedLine(baselineStart, baselineEnd, dottedLineSize);

                //Draw Descent line
                Vector3 descentlineStart = m_Transform.TransformPoint(new Vector3(origin, descentline, 0));
                Vector3 descentlineEnd = m_Transform.TransformPoint(new Vector3(advance, descentline, 0));

                Handles.color = Color.cyan;
                Handles.DrawDottedLine(descentlineStart, descentlineEnd, dottedLineSize);

                // Draw Origin
                Vector3 originPosition = m_Transform.TransformPoint(new Vector3(origin, baseline, 0));
                DrawCrosshair(originPosition, 0.05f / m_ScaleMultiplier, Color.cyan);

                // Draw Horizontal Advance
                Vector3 advancePosition = m_Transform.TransformPoint(new Vector3(advance, baseline, 0));
                DrawSquare(advancePosition, 0.025f / m_ScaleMultiplier, Color.yellow);
                DrawCrosshair(advancePosition, 0.0125f / m_ScaleMultiplier, Color.yellow);

                // Draw text labels for metrics
               if (m_HandleSize < 0.5f)
               {
                   GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
                   style.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
                   style.fontSize = 12;
                   style.fixedWidth = 200;
                   style.fixedHeight = 20;

                   Vector3 labelPosition;
                   float center = (origin + advance) / 2;

                   //float baselineMetrics = 0;
                   //float ascentlineMetrics = ascentline - baseline;
                   //float caplineMetrics = capline - baseline;
                   //float meanlineMetrics = meanline - baseline;
                   //float descentlineMetrics = descentline - baseline;

                   // Ascent Line
                   labelPosition = m_Transform.TransformPoint(new Vector3(center, ascentline, 0));
                   style.alignment = TextAnchor.UpperCenter;
                   Handles.Label(labelPosition, "Ascent Line", style);
                   //Handles.Label(labelPosition, "Ascent Line (" + ascentlineMetrics.ToString("f3") + ")" , style);

                   // Base Line
                   labelPosition = m_Transform.TransformPoint(new Vector3(center, baseline, 0));
                   Handles.Label(labelPosition, "Base Line", style);
                   //Handles.Label(labelPosition, "Base Line (" + baselineMetrics.ToString("f3") + ")" , style);

                   // Descent line
                   labelPosition = m_Transform.TransformPoint(new Vector3(center, descentline, 0));
                   Handles.Label(labelPosition, "Descent Line", style);
                   //Handles.Label(labelPosition, "Descent Line (" + descentlineMetrics.ToString("f3") + ")" , style);

                   if (characterInfo.isVisible)
                   {
                       // Cap Line
                       labelPosition = m_Transform.TransformPoint(new Vector3(center, capline, 0));
                       style.alignment = TextAnchor.UpperCenter;
                       Handles.Label(labelPosition, "Cap Line", style);
                       //Handles.Label(labelPosition, "Cap Line (" + caplineMetrics.ToString("f3") + ")" , style);

                       // Mean Line
                       labelPosition = m_Transform.TransformPoint(new Vector3(center, meanline, 0));
                       style.alignment = TextAnchor.UpperCenter;
                       Handles.Label(labelPosition, "Mean Line", style);
                       //Handles.Label(labelPosition, "Mean Line (" + ascentlineMetrics.ToString("f3") + ")" , style);

                       // Origin
                       labelPosition = m_Transform.TransformPoint(new Vector3(origin, baseline, 0));
                       style.alignment = TextAnchor.UpperRight;
                       Handles.Label(labelPosition, "Origin ", style);

                       // Advance
                       labelPosition = m_Transform.TransformPoint(new Vector3(advance, baseline, 0));
                       style.alignment = TextAnchor.UpperLeft;
                       Handles.Label(labelPosition, "  Advance", style);
                   }
               }
            }
        }


        /// <summary>
        /// Method to draw rectangles around each word of the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawWordBounds()
        {
            for (int i = 0; i < m_TextInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = m_TextInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bottomLeft = Vector3.zero;
                Vector3 topLeft = Vector3.zero;
                Vector3 bottomRight = Vector3.zero;
                Vector3 topRight = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                Color wordColor = Color.green;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = m_TextInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > m_TextComponent.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > m_TextComponent.maxVisibleLines ||
                                             (m_TextComponent.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != m_TextComponent.pageToDisplay) ? false : true;

                    // Track Max Ascender and Min Descender
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
                        topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                            bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                            bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                            topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                            // Draw Region
                            DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != m_TextInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);
                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                    }
                }

                //Debug.Log(wInfo.GetWord(m_TextMeshPro.textInfo.characterInfo));
            }


        }


        /// <summary>
        /// Draw rectangle around each of the links contained in the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawLinkBounds()
        {
            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            for (int i = 0; i < textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bottomLeft = Vector3.zero;
                Vector3 topLeft = Vector3.zero;
                Vector3 bottomRight = Vector3.zero;
                Vector3 topRight = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                Color32 linkColor = Color.cyan;

                // Iterate through each character of the link text
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > m_TextComponent.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > m_TextComponent.maxVisibleLines ||
                                             (m_TextComponent.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != m_TextComponent.pageToDisplay) ? false : true;

                    // Track Max Ascender and Min Descender
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
                        topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Link is one character
                        if (linkInfo.linkTextLength == 1)
                        {
                            isBeginRegion = false;

                            topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                            bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                            bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                            topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                            // Draw Region
                            DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Link
                    if (isBeginRegion && j == linkInfo.linkTextLength - 1)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Link is split on more than one line.
                    else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;
                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log(wInfo.GetWord(m_TextMeshPro.textInfo.characterInfo));
            }
        }


        /// <summary>
        /// Draw Rectangles around each lines of the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawLineBounds()
        {
            int lineCount = m_TextInfo.lineCount;

            for (int i = 0; i < lineCount; i++)
            {
                TMP_LineInfo lineInfo = m_TextInfo.lineInfo[i];
                TMP_CharacterInfo firstCharacterInfo = m_TextInfo.characterInfo[lineInfo.firstCharacterIndex];
                TMP_CharacterInfo lastCharacterInfo = m_TextInfo.characterInfo[lineInfo.lastCharacterIndex];

                bool isLineVisible = (lineInfo.characterCount == 1 && (firstCharacterInfo.character == 10 || firstCharacterInfo.character == 11 || firstCharacterInfo.character == 0x2028 || firstCharacterInfo.character == 0x2029)) ||
                                      i > m_TextComponent.maxVisibleLines ||
                                     (m_TextComponent.overflowMode == TextOverflowModes.Page && firstCharacterInfo.pageNumber + 1 != m_TextComponent.pageToDisplay) ? false : true;

                if (!isLineVisible) continue;

                float lineBottomLeft = firstCharacterInfo.bottomLeft.x;
                float lineTopRight = lastCharacterInfo.topRight.x;

                float ascentline = lineInfo.ascender;
                float baseline = lineInfo.baseline;
                float descentline = lineInfo.descender;

                float dottedLineSize = 12;

                // Draw line extents
                DrawDottedRectangle(m_Transform.TransformPoint(lineInfo.lineExtents.min), m_Transform.TransformPoint(lineInfo.lineExtents.max), Color.green, 4);

                // Draw Ascent line
                Vector3 ascentlineStart = m_Transform.TransformPoint(new Vector3(lineBottomLeft, ascentline, 0));
                Vector3 ascentlineEnd = m_Transform.TransformPoint(new Vector3(lineTopRight, ascentline, 0));

                Handles.color = Color.yellow;
                Handles.DrawDottedLine(ascentlineStart, ascentlineEnd, dottedLineSize);

                // Draw Base line
                Vector3 baseLineStart = m_Transform.TransformPoint(new Vector3(lineBottomLeft, baseline, 0));
                Vector3 baseLineEnd = m_Transform.TransformPoint(new Vector3(lineTopRight, baseline, 0));

                Handles.color = Color.yellow;
                Handles.DrawDottedLine(baseLineStart, baseLineEnd, dottedLineSize);

                // Draw Descent line
                Vector3 descentLineStart = m_Transform.TransformPoint(new Vector3(lineBottomLeft, descentline, 0));
                Vector3 descentLineEnd = m_Transform.TransformPoint(new Vector3(lineTopRight, descentline, 0));

                Handles.color = Color.yellow;
                Handles.DrawDottedLine(descentLineStart, descentLineEnd, dottedLineSize);

                // Draw text labels for metrics
                if (m_HandleSize < 1.0f)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                    style.fontSize = 12;
                    style.fixedWidth = 200;
                    style.fixedHeight = 20;
                    Vector3 labelPosition;

                    // Ascent Line
                    labelPosition = m_Transform.TransformPoint(new Vector3(lineBottomLeft, ascentline, 0));
                    style.padding = new RectOffset(0, 10, 0, 5);
                    style.alignment = TextAnchor.MiddleRight;
                    Handles.Label(labelPosition, "Ascent Line", style);

                    // Base Line
                    labelPosition = m_Transform.TransformPoint(new Vector3(lineBottomLeft, baseline, 0));
                    Handles.Label(labelPosition, "Base Line", style);

                    // Descent line
                    labelPosition = m_Transform.TransformPoint(new Vector3(lineBottomLeft, descentline, 0));
                    Handles.Label(labelPosition, "Descent Line", style);
                }
            }
        }


        /// <summary>
        /// Draw Rectangle around the bounds of the text object.
        /// </summary>
        void DrawBounds()
        {
            Bounds meshBounds = m_TextComponent.bounds;

            // Get Bottom Left and Top Right position of each word
            Vector3 bottomLeft = m_TextComponent.transform.position + meshBounds.min;
            Vector3 topRight = m_TextComponent.transform.position + meshBounds.max;

            DrawRectangle(bottomLeft, topRight, new Color(1, 0.5f, 0));
        }


        void DrawTextBounds()
        {
            Bounds textBounds = m_TextComponent.textBounds;

            Vector3 bottomLeft = m_TextComponent.transform.position + (textBounds.center - textBounds.extents);
            Vector3 topRight = m_TextComponent.transform.position + (textBounds.center + textBounds.extents);

            DrawRectangle(bottomLeft, topRight, new Color(0f, 0.5f, 0.5f));
        }


        // Draw Rectangles
        void DrawRectangle(Vector3 BL, Vector3 TR, Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawLine(new Vector3(BL.x, BL.y, 0), new Vector3(BL.x, TR.y, 0));
            Gizmos.DrawLine(new Vector3(BL.x, TR.y, 0), new Vector3(TR.x, TR.y, 0));
            Gizmos.DrawLine(new Vector3(TR.x, TR.y, 0), new Vector3(TR.x, BL.y, 0));
            Gizmos.DrawLine(new Vector3(TR.x, BL.y, 0), new Vector3(BL.x, BL.y, 0));
        }

        void DrawDottedRectangle(Vector3 bottomLeft, Vector3 topRight, Color color, float size = 5.0f)
        {
            Handles.color = color;
            Handles.DrawDottedLine(bottomLeft, new Vector3(bottomLeft.x, topRight.y, bottomLeft.z), size);
            Handles.DrawDottedLine(new Vector3(bottomLeft.x, topRight.y, bottomLeft.z), topRight, size);
            Handles.DrawDottedLine(topRight, new Vector3(topRight.x, bottomLeft.y, bottomLeft.z), size);
            Handles.DrawDottedLine(new Vector3(topRight.x, bottomLeft.y, bottomLeft.z), bottomLeft, size);
        }

        void DrawSolidRectangle(Vector3 bottomLeft, Vector3 topRight, Color color, float size = 5.0f)
        {
            Handles.color = color;
            Rect rect = new Rect(bottomLeft, topRight - bottomLeft);
            Handles.DrawSolidRectangleWithOutline(rect, color, Color.black);
        }

        void DrawSquare(Vector3 position, float size, Color color)
        {
            Handles.color = color;
            Vector3 bottomLeft = new Vector3(position.x - size, position.y - size, position.z);
            Vector3 topLeft = new Vector3(position.x - size, position.y + size, position.z);
            Vector3 topRight = new Vector3(position.x + size, position.y + size, position.z);
            Vector3 bottomRight = new Vector3(position.x + size, position.y - size, position.z);

            Handles.DrawLine(bottomLeft, topLeft);
            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
        }

        void DrawCrosshair(Vector3 position, float size, Color color)
        {
            Handles.color = color;

            Handles.DrawLine(new Vector3(position.x - size, position.y, position.z), new Vector3(position.x + size, position.y, position.z));
            Handles.DrawLine(new Vector3(position.x, position.y - size, position.z), new Vector3(position.x, position.y + size, position.z));
        }


        // Draw Rectangles
        void DrawRectangle(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
        }


        // Draw Rectangles
        void DrawDottedRectangle(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)
        {
            var cam = Camera.current;
            float dotSpacing = (cam.WorldToScreenPoint(br).x - cam.WorldToScreenPoint(bl).x) / 75f;
            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawDottedLine(bl, tl, dotSpacing);
            UnityEditor.Handles.DrawDottedLine(tl, tr, dotSpacing);
            UnityEditor.Handles.DrawDottedLine(tr, br, dotSpacing);
            UnityEditor.Handles.DrawDottedLine(br, bl, dotSpacing);
        }
        #endif
    }
}


```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_TextSelector_A.cs

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


namespace TMPro.Examples
{

    public class TMP_TextSelector_A : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TextMeshPro m_TextMeshPro;

        private Camera m_Camera;

        private bool m_isHoveringObject;
        private int m_selectedLink = -1;
        private int m_lastCharIndex = -1;
        private int m_lastWordIndex = -1;

        void Awake()
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshPro>();
            m_Camera = Camera.main;

            // Force generation of the text object so we have valid data to work with. This is needed since LateUpdate() will be called before the text object has a chance to generated when entering play mode.
            m_TextMeshPro.ForceMeshUpdate();
        }


        void LateUpdate()
        {
            m_isHoveringObject = false;

            if (TMP_TextUtilities.IsIntersectingRectTransform(m_TextMeshPro.rectTransform, Input.mousePosition, Camera.main))
            {
                m_isHoveringObject = true;
            }

            if (m_isHoveringObject)
            {
                #region Example of Character Selection
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(m_TextMeshPro, Input.mousePosition, Camera.main, true);
                if (charIndex != -1 && charIndex != m_lastCharIndex && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    //Debug.Log("[" + m_TextMeshPro.textInfo.characterInfo[charIndex].character + "] has been selected.");

                    m_lastCharIndex = charIndex;

                    int meshIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].materialReferenceIndex;

                    int vertexIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].vertexIndex;

                    Color32 c = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);

                    Color32[] vertexColors = m_TextMeshPro.textInfo.meshInfo[meshIndex].colors32;

                    vertexColors[vertexIndex + 0] = c;
                    vertexColors[vertexIndex + 1] = c;
                    vertexColors[vertexIndex + 2] = c;
                    vertexColors[vertexIndex + 3] = c;

                    //m_TextMeshPro.mesh.colors32 = vertexColors;
                    m_TextMeshPro.textInfo.meshInfo[meshIndex].mesh.colors32 = vertexColors;
                }
                #endregion

                #region Example of Link Handling
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, m_Camera);

                // Clear previous link selection if one existed.
                if ((linkIndex == -1 && m_selectedLink != -1) || linkIndex != m_selectedLink)
                {
                    //m_TextPopup_RectTransform.gameObject.SetActive(false);
                    m_selectedLink = -1;
                }

                // Handle new Link selection.
                if (linkIndex != -1 && linkIndex != m_selectedLink)
                {
                    m_selectedLink = linkIndex;

                    TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];

                    // The following provides an example of how to access the link properties.
                    //Debug.Log("Link ID: \"" + linkInfo.GetLinkID() + "\"   Link Text: \"" + linkInfo.GetLinkText() + "\""); // Example of how to retrieve the Link ID and Link Text.

                    Vector3 worldPointInRectangle;

                    RectTransformUtility.ScreenPointToWorldPointInRectangle(m_TextMeshPro.rectTransform, Input.mousePosition, m_Camera, out worldPointInRectangle);

                    switch (linkInfo.GetLinkID())
                    {
                        case "id_01": // 100041637: // id_01
                                      //m_TextPopup_RectTransform.position = worldPointInRectangle;
                                      //m_TextPopup_RectTransform.gameObject.SetActive(true);
                                      //m_TextPopup_TMPComponent.text = k_LinkText + " ID 01";
                            break;
                        case "id_02": // 100041638: // id_02
                                      //m_TextPopup_RectTransform.position = worldPointInRectangle;
                                      //m_TextPopup_RectTransform.gameObject.SetActive(true);
                                      //m_TextPopup_TMPComponent.text = k_LinkText + " ID 02";
                            break;
                    }
                }
                #endregion


                #region Example of Word Selection
                // Check if Mouse intersects any words and if so assign a random color to that word.
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextMeshPro, Input.mousePosition, Camera.main);
                if (wordIndex != -1 && wordIndex != m_lastWordIndex)
                {
                    m_lastWordIndex = wordIndex;

                    TMP_WordInfo wInfo = m_TextMeshPro.textInfo.wordInfo[wordIndex];

                    Vector3 wordPOS = m_TextMeshPro.transform.TransformPoint(m_TextMeshPro.textInfo.characterInfo[wInfo.firstCharacterIndex].bottomLeft);
                    wordPOS = Camera.main.WorldToScreenPoint(wordPOS);

                    //Debug.Log("Mouse Position: " + Input.mousePosition.ToString("f3") + "  Word Position: " + wordPOS.ToString("f3"));

                    Color32[] vertexColors = m_TextMeshPro.textInfo.meshInfo[0].colors32;

                    Color32 c = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                    for (int i = 0; i < wInfo.characterCount; i++)
                    {
                        int vertexIndex = m_TextMeshPro.textInfo.characterInfo[wInfo.firstCharacterIndex + i].vertexIndex;

                        vertexColors[vertexIndex + 0] = c;
                        vertexColors[vertexIndex + 1] = c;
                        vertexColors[vertexIndex + 2] = c;
                        vertexColors[vertexIndex + 3] = c;
                    }

                    m_TextMeshPro.mesh.colors32 = vertexColors;
                }
                #endregion
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter()");
            m_isHoveringObject = true;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("OnPointerExit()");
            m_isHoveringObject = false;
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_TextSelector_B.cs

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


#pragma warning disable 0618 // Disabled warning due to SetVertices being deprecated until new release with SetMesh() is available.

namespace TMPro.Examples
{

    public class TMP_TextSelector_B : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler
    {
        public RectTransform TextPopup_Prefab_01;

        private RectTransform m_TextPopup_RectTransform;
        private TextMeshProUGUI m_TextPopup_TMPComponent;
        private const string k_LinkText = "You have selected link <#ffff00>";
        private const string k_WordText = "Word Index: <#ffff00>";


        private TextMeshProUGUI m_TextMeshPro;
        private Canvas m_Canvas;
        private Camera m_Camera;

        // Flags
        private bool isHoveringObject;
        private int m_selectedWord = -1;
        private int m_selectedLink = -1;
        private int m_lastIndex = -1;

        private Matrix4x4 m_matrix;

        private TMP_MeshInfo[] m_cachedMeshInfoVertexData;

        void Awake()
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshProUGUI>();


            m_Canvas = gameObject.GetComponentInParent<Canvas>();

            // Get a reference to the camera if Canvas Render Mode is not ScreenSpace Overlay.
            if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                m_Camera = null;
            else
                m_Camera = m_Canvas.worldCamera;

            // Create pop-up text object which is used to show the link information.
            m_TextPopup_RectTransform = Instantiate(TextPopup_Prefab_01) as RectTransform;
            m_TextPopup_RectTransform.SetParent(m_Canvas.transform, false);
            m_TextPopup_TMPComponent = m_TextPopup_RectTransform.GetComponentInChildren<TextMeshProUGUI>();
            m_TextPopup_RectTransform.gameObject.SetActive(false);
        }


        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            // UnSubscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj == m_TextMeshPro)
            {
                // Update cached vertex data.
                m_cachedMeshInfoVertexData = m_TextMeshPro.textInfo.CopyMeshInfoVertexData();
            }
        }


        void LateUpdate()
        {
            if (isHoveringObject)
            {
                // Check if Mouse Intersects any of the characters. If so, assign a random color.
                #region Handle Character Selection
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(m_TextMeshPro, Input.mousePosition, m_Camera, true);

                // Undo Swap and Vertex Attribute changes.
                if (charIndex == -1 || charIndex != m_lastIndex)
                {
                    RestoreCachedVertexAttributes(m_lastIndex);
                    m_lastIndex = -1;
                }

                if (charIndex != -1 && charIndex != m_lastIndex && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    m_lastIndex = charIndex;

                    // Get the index of the material / sub text object used by this character.
                    int materialIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].materialReferenceIndex;

                    // Get the index of the first vertex of the selected character.
                    int vertexIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].vertexIndex;

                    // Get a reference to the vertices array.
                    Vector3[] vertices = m_TextMeshPro.textInfo.meshInfo[materialIndex].vertices;

                    // Determine the center point of the character.
                    Vector2 charMidBasline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

                    // Need to translate all 4 vertices of the character to aligned with middle of character / baseline.
                    // This is needed so the matrix TRS is applied at the origin for each character.
                    Vector3 offset = charMidBasline;

                    // Translate the character to the middle baseline.
                    vertices[vertexIndex + 0] = vertices[vertexIndex + 0] - offset;
                    vertices[vertexIndex + 1] = vertices[vertexIndex + 1] - offset;
                    vertices[vertexIndex + 2] = vertices[vertexIndex + 2] - offset;
                    vertices[vertexIndex + 3] = vertices[vertexIndex + 3] - offset;

                    float zoomFactor = 1.5f;

                    // Setup the Matrix for the scale change.
                    m_matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * zoomFactor);

                    // Apply Matrix operation on the given character.
                    vertices[vertexIndex + 0] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    // Translate the character back to its original position.
                    vertices[vertexIndex + 0] = vertices[vertexIndex + 0] + offset;
                    vertices[vertexIndex + 1] = vertices[vertexIndex + 1] + offset;
                    vertices[vertexIndex + 2] = vertices[vertexIndex + 2] + offset;
                    vertices[vertexIndex + 3] = vertices[vertexIndex + 3] + offset;

                    // Change Vertex Colors of the highlighted character
                    Color32 c = new Color32(255, 255, 192, 255);

                    // Get a reference to the vertex color
                    Color32[] vertexColors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;

                    vertexColors[vertexIndex + 0] = c;
                    vertexColors[vertexIndex + 1] = c;
                    vertexColors[vertexIndex + 2] = c;
                    vertexColors[vertexIndex + 3] = c;


                    // Get a reference to the meshInfo of the selected character.
                    TMP_MeshInfo meshInfo = m_TextMeshPro.textInfo.meshInfo[materialIndex];

                    // Get the index of the last character's vertex attributes.
                    int lastVertexIndex = vertices.Length - 4;

                    // Swap the current character's vertex attributes with those of the last element in the vertex attribute arrays.
                    // We do this to make sure this character is rendered last and over other characters.
                    meshInfo.SwapVertexData(vertexIndex, lastVertexIndex);

                    // Need to update the appropriate
                    m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                }
                #endregion


                #region Word Selection Handling
                //Check if Mouse intersects any words and if so assign a random color to that word.
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextMeshPro, Input.mousePosition, m_Camera);

                // Clear previous word selection.
                if (m_TextPopup_RectTransform != null && m_selectedWord != -1 && (wordIndex == -1 || wordIndex != m_selectedWord))
                {
                    TMP_WordInfo wInfo = m_TextMeshPro.textInfo.wordInfo[m_selectedWord];

                    // Iterate through each of the characters of the word.
                    for (int i = 0; i < wInfo.characterCount; i++)
                    {
                        int characterIndex = wInfo.firstCharacterIndex + i;

                        // Get the index of the material / sub text object used by this character.
                        int meshIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].materialReferenceIndex;

                        // Get the index of the first vertex of this character.
                        int vertexIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].vertexIndex;

                        // Get a reference to the vertex color
                        Color32[] vertexColors = m_TextMeshPro.textInfo.meshInfo[meshIndex].colors32;

                        Color32 c = vertexColors[vertexIndex + 0].Tint(1.33333f);

                        vertexColors[vertexIndex + 0] = c;
                        vertexColors[vertexIndex + 1] = c;
                        vertexColors[vertexIndex + 2] = c;
                        vertexColors[vertexIndex + 3] = c;
                    }

                    // Update Geometry
                    m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

                    m_selectedWord = -1;
                }


                // Word Selection Handling
                if (wordIndex != -1 && wordIndex != m_selectedWord && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    m_selectedWord = wordIndex;

                    TMP_WordInfo wInfo = m_TextMeshPro.textInfo.wordInfo[wordIndex];

                    // Iterate through each of the characters of the word.
                    for (int i = 0; i < wInfo.characterCount; i++)
                    {
                        int characterIndex = wInfo.firstCharacterIndex + i;

                        // Get the index of the material / sub text object used by this character.
                        int meshIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].materialReferenceIndex;

                        int vertexIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].vertexIndex;

                        // Get a reference to the vertex color
                        Color32[] vertexColors = m_TextMeshPro.textInfo.meshInfo[meshIndex].colors32;

                        Color32 c = vertexColors[vertexIndex + 0].Tint(0.75f);

                        vertexColors[vertexIndex + 0] = c;
                        vertexColors[vertexIndex + 1] = c;
                        vertexColors[vertexIndex + 2] = c;
                        vertexColors[vertexIndex + 3] = c;
                    }

                    // Update Geometry
                    m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

                }
                #endregion


                #region Example of Link Handling
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, m_Camera);

                // Clear previous link selection if one existed.
                if ((linkIndex == -1 && m_selectedLink != -1) || linkIndex != m_selectedLink)
                {
                    m_TextPopup_RectTransform.gameObject.SetActive(false);
                    m_selectedLink = -1;
                }

                // Handle new Link selection.
                if (linkIndex != -1 && linkIndex != m_selectedLink)
                {
                    m_selectedLink = linkIndex;

                    TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];

                    // Debug.Log("Link ID: \"" + linkInfo.GetLinkID() + "\"   Link Text: \"" + linkInfo.GetLinkText() + "\""); // Example of how to retrieve the Link ID and Link Text.

                    Vector3 worldPointInRectangle;
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(m_TextMeshPro.rectTransform, Input.mousePosition, m_Camera, out worldPointInRectangle);

                    switch (linkInfo.GetLinkID())
                    {
                        case "id_01": // 100041637: // id_01
                            m_TextPopup_RectTransform.position = worldPointInRectangle;
                            m_TextPopup_RectTransform.gameObject.SetActive(true);
                            m_TextPopup_TMPComponent.text = k_LinkText + " ID 01";
                            break;
                        case "id_02": // 100041638: // id_02
                            m_TextPopup_RectTransform.position = worldPointInRectangle;
                            m_TextPopup_RectTransform.gameObject.SetActive(true);
                            m_TextPopup_TMPComponent.text = k_LinkText + " ID 02";
                            break;
                    }
                }
                #endregion

            }
            else
            {
                // Restore any character that may have been modified
                if (m_lastIndex != -1)
                {
                    RestoreCachedVertexAttributes(m_lastIndex);
                    m_lastIndex = -1;
                }
            }

        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("OnPointerEnter()");
            isHoveringObject = true;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("OnPointerExit()");
            isHoveringObject = false;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Click at POS: " + eventData.position + "  World POS: " + eventData.worldPosition);

            // Check if Mouse Intersects any of the characters. If so, assign a random color.
            #region Character Selection Handling
            /*
            int charIndex = TMP_TextUtilities.FindIntersectingCharacter(m_TextMeshPro, Input.mousePosition, m_Camera, true);
            if (charIndex != -1 && charIndex != m_lastIndex)
            {
                //Debug.Log("Character [" + m_TextMeshPro.textInfo.characterInfo[index].character + "] was selected at POS: " + eventData.position);
                m_lastIndex = charIndex;

                Color32 c = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                int vertexIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].vertexIndex;

                UIVertex[] uiVertices = m_TextMeshPro.textInfo.meshInfo.uiVertices;

                uiVertices[vertexIndex + 0].color = c;
                uiVertices[vertexIndex + 1].color = c;
                uiVertices[vertexIndex + 2].color = c;
                uiVertices[vertexIndex + 3].color = c;

                m_TextMeshPro.canvasRenderer.SetVertices(uiVertices, uiVertices.Length);
            }
            */
            #endregion


            #region Word Selection Handling
            //Check if Mouse intersects any words and if so assign a random color to that word.
            /*
            int wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextMeshPro, Input.mousePosition, m_Camera);

            // Clear previous word selection.
            if (m_TextPopup_RectTransform != null && m_selectedWord != -1 && (wordIndex == -1 || wordIndex != m_selectedWord))
            {
                TMP_WordInfo wInfo = m_TextMeshPro.textInfo.wordInfo[m_selectedWord];

                // Get a reference to the uiVertices array.
                UIVertex[] uiVertices = m_TextMeshPro.textInfo.meshInfo.uiVertices;

                // Iterate through each of the characters of the word.
                for (int i = 0; i < wInfo.characterCount; i++)
                {
                    int vertexIndex = m_TextMeshPro.textInfo.characterInfo[wInfo.firstCharacterIndex + i].vertexIndex;

                    Color32 c = uiVertices[vertexIndex + 0].color.Tint(1.33333f);

                    uiVertices[vertexIndex + 0].color = c;
                    uiVertices[vertexIndex + 1].color = c;
                    uiVertices[vertexIndex + 2].color = c;
                    uiVertices[vertexIndex + 3].color = c;
                }

                m_TextMeshPro.canvasRenderer.SetVertices(uiVertices, uiVertices.Length);

                m_selectedWord = -1;
            }

            // Handle word selection
            if (wordIndex != -1 && wordIndex != m_selectedWord)
            {
                m_selectedWord = wordIndex;

                TMP_WordInfo wInfo = m_TextMeshPro.textInfo.wordInfo[wordIndex];

                // Get a reference to the uiVertices array.
                UIVertex[] uiVertices = m_TextMeshPro.textInfo.meshInfo.uiVertices;

                // Iterate through each of the characters of the word.
                for (int i = 0; i < wInfo.characterCount; i++)
                {
                    int vertexIndex = m_TextMeshPro.textInfo.characterInfo[wInfo.firstCharacterIndex + i].vertexIndex;

                    Color32 c = uiVertices[vertexIndex + 0].color.Tint(0.75f);

                    uiVertices[vertexIndex + 0].color = c;
                    uiVertices[vertexIndex + 1].color = c;
                    uiVertices[vertexIndex + 2].color = c;
                    uiVertices[vertexIndex + 3].color = c;
                }

                m_TextMeshPro.canvasRenderer.SetVertices(uiVertices, uiVertices.Length);
            }
            */
            #endregion


            #region Link Selection Handling
            /*
            // Check if Mouse intersects any words and if so assign a random color to that word.
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, m_Camera);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];
                int linkHashCode = linkInfo.hashCode;

                //Debug.Log(TMP_TextUtilities.GetSimpleHashCode("id_02"));

                switch (linkHashCode)
                {
                    case 291445: // id_01
                        if (m_LinkObject01 == null)
                            m_LinkObject01 = Instantiate(Link_01_Prefab);
                        else
                        {
                            m_LinkObject01.gameObject.SetActive(true);
                        }

                        break;
                    case 291446: // id_02
                        break;

                }

                // Example of how to modify vertex attributes like colors
                #region Vertex Attribute Modification Example
                UIVertex[] uiVertices = m_TextMeshPro.textInfo.meshInfo.uiVertices;

                Color32 c = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                for (int i = 0; i < linkInfo.characterCount; i++)
                {
                    TMP_CharacterInfo cInfo = m_TextMeshPro.textInfo.characterInfo[linkInfo.firstCharacterIndex + i];

                    if (!cInfo.isVisible) continue; // Skip invisible characters.

                    int vertexIndex = cInfo.vertexIndex;

                    uiVertices[vertexIndex + 0].color = c;
                    uiVertices[vertexIndex + 1].color = c;
                    uiVertices[vertexIndex + 2].color = c;
                    uiVertices[vertexIndex + 3].color = c;
                }

                m_TextMeshPro.canvasRenderer.SetVertices(uiVertices, uiVertices.Length);
                #endregion
            }
            */
            #endregion
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log("OnPointerUp()");
        }


        void RestoreCachedVertexAttributes(int index)
        {
            if (index == -1 || index > m_TextMeshPro.textInfo.characterCount - 1) return;

            // Get the index of the material / sub text object used by this character.
            int materialIndex = m_TextMeshPro.textInfo.characterInfo[index].materialReferenceIndex;

            // Get the index of the first vertex of the selected character.
            int vertexIndex = m_TextMeshPro.textInfo.characterInfo[index].vertexIndex;

            // Restore Vertices
            // Get a reference to the cached / original vertices.
            Vector3[] src_vertices = m_cachedMeshInfoVertexData[materialIndex].vertices;

            // Get a reference to the vertices that we need to replace.
            Vector3[] dst_vertices = m_TextMeshPro.textInfo.meshInfo[materialIndex].vertices;

            // Restore / Copy vertices from source to destination
            dst_vertices[vertexIndex + 0] = src_vertices[vertexIndex + 0];
            dst_vertices[vertexIndex + 1] = src_vertices[vertexIndex + 1];
            dst_vertices[vertexIndex + 2] = src_vertices[vertexIndex + 2];
            dst_vertices[vertexIndex + 3] = src_vertices[vertexIndex + 3];

            // Restore Vertex Colors
            // Get a reference to the vertex colors we need to replace.
            Color32[] dst_colors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;

            // Get a reference to the cached / original vertex colors.
            Color32[] src_colors = m_cachedMeshInfoVertexData[materialIndex].colors32;

            // Copy the vertex colors from source to destination.
            dst_colors[vertexIndex + 0] = src_colors[vertexIndex + 0];
            dst_colors[vertexIndex + 1] = src_colors[vertexIndex + 1];
            dst_colors[vertexIndex + 2] = src_colors[vertexIndex + 2];
            dst_colors[vertexIndex + 3] = src_colors[vertexIndex + 3];

            // Restore UV0S
            // UVS0
            Vector4[] src_uv0s = m_cachedMeshInfoVertexData[materialIndex].uvs0;
            Vector4[] dst_uv0s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs0;
            dst_uv0s[vertexIndex + 0] = src_uv0s[vertexIndex + 0];
            dst_uv0s[vertexIndex + 1] = src_uv0s[vertexIndex + 1];
            dst_uv0s[vertexIndex + 2] = src_uv0s[vertexIndex + 2];
            dst_uv0s[vertexIndex + 3] = src_uv0s[vertexIndex + 3];

            // UVS2
            Vector2[] src_uv2s = m_cachedMeshInfoVertexData[materialIndex].uvs2;
            Vector2[] dst_uv2s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs2;
            dst_uv2s[vertexIndex + 0] = src_uv2s[vertexIndex + 0];
            dst_uv2s[vertexIndex + 1] = src_uv2s[vertexIndex + 1];
            dst_uv2s[vertexIndex + 2] = src_uv2s[vertexIndex + 2];
            dst_uv2s[vertexIndex + 3] = src_uv2s[vertexIndex + 3];


            // Restore last vertex attribute as we swapped it as well
            int lastIndex = (src_vertices.Length / 4 - 1) * 4;

            // Vertices
            dst_vertices[lastIndex + 0] = src_vertices[lastIndex + 0];
            dst_vertices[lastIndex + 1] = src_vertices[lastIndex + 1];
            dst_vertices[lastIndex + 2] = src_vertices[lastIndex + 2];
            dst_vertices[lastIndex + 3] = src_vertices[lastIndex + 3];

            // Vertex Colors
            src_colors = m_cachedMeshInfoVertexData[materialIndex].colors32;
            dst_colors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;
            dst_colors[lastIndex + 0] = src_colors[lastIndex + 0];
            dst_colors[lastIndex + 1] = src_colors[lastIndex + 1];
            dst_colors[lastIndex + 2] = src_colors[lastIndex + 2];
            dst_colors[lastIndex + 3] = src_colors[lastIndex + 3];

            // UVS0
            src_uv0s = m_cachedMeshInfoVertexData[materialIndex].uvs0;
            dst_uv0s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs0;
            dst_uv0s[lastIndex + 0] = src_uv0s[lastIndex + 0];
            dst_uv0s[lastIndex + 1] = src_uv0s[lastIndex + 1];
            dst_uv0s[lastIndex + 2] = src_uv0s[lastIndex + 2];
            dst_uv0s[lastIndex + 3] = src_uv0s[lastIndex + 3];

            // UVS2
            src_uv2s = m_cachedMeshInfoVertexData[materialIndex].uvs2;
            dst_uv2s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs2;
            dst_uv2s[lastIndex + 0] = src_uv2s[lastIndex + 0];
            dst_uv2s[lastIndex + 1] = src_uv2s[lastIndex + 1];
            dst_uv2s[lastIndex + 2] = src_uv2s[lastIndex + 2];
            dst_uv2s[lastIndex + 3] = src_uv2s[lastIndex + 3];

            // Need to update the appropriate
            m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMP_UiFrameRateCounter.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class TMP_UiFrameRateCounter : MonoBehaviour
    {
        public float UpdateInterval = 5.0f;
        private float m_LastInterval = 0;
        private int m_Frames = 0;

        public enum FpsCounterAnchorPositions { TopLeft, BottomLeft, TopRight, BottomRight };

        public FpsCounterAnchorPositions AnchorPosition = FpsCounterAnchorPositions.TopRight;

        private string htmlColorTag;
        private const string fpsLabel = "{0:2}</color> <#8080ff>FPS \n<#FF8000>{1:2} <#8080ff>MS";

        private TextMeshProUGUI m_TextMeshPro;
        private RectTransform m_frameCounter_transform;

        private FpsCounterAnchorPositions last_AnchorPosition;

        void Awake()
        {
            if (!enabled)
                return;

            Application.targetFrameRate = 1000;

            GameObject frameCounter = new GameObject("Frame Counter");
            m_frameCounter_transform = frameCounter.AddComponent<RectTransform>();

            m_frameCounter_transform.SetParent(this.transform, false);

            m_TextMeshPro = frameCounter.AddComponent<TextMeshProUGUI>();
            m_TextMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            m_TextMeshPro.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Overlay");

            m_TextMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            m_TextMeshPro.fontSize = 36;

            m_TextMeshPro.isOverlay = true;

            Set_FrameCounter_Position(AnchorPosition);
            last_AnchorPosition = AnchorPosition;
        }


        void Start()
        {
            m_LastInterval = Time.realtimeSinceStartup;
            m_Frames = 0;
        }


        void Update()
        {
            if (AnchorPosition != last_AnchorPosition)
                Set_FrameCounter_Position(AnchorPosition);

            last_AnchorPosition = AnchorPosition;

            m_Frames += 1;
            float timeNow = Time.realtimeSinceStartup;

            if (timeNow > m_LastInterval + UpdateInterval)
            {
                // display two fractional digits (f2 format)
                float fps = m_Frames / (timeNow - m_LastInterval);
                float ms = 1000.0f / Mathf.Max(fps, 0.00001f);

                if (fps < 30)
                    htmlColorTag = "<color=yellow>";
                else if (fps < 10)
                    htmlColorTag = "<color=red>";
                else
                    htmlColorTag = "<color=green>";

                m_TextMeshPro.SetText(htmlColorTag + fpsLabel, fps, ms);

                m_Frames = 0;
                m_LastInterval = timeNow;
            }
        }


        void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)
        {
            switch (anchor_position)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopLeft;
                    m_frameCounter_transform.pivot = new Vector2(0, 1);
                    m_frameCounter_transform.anchorMin = new Vector2(0.01f, 0.99f);
                    m_frameCounter_transform.anchorMax = new Vector2(0.01f, 0.99f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(0, 1);
                    break;
                case FpsCounterAnchorPositions.BottomLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomLeft;
                    m_frameCounter_transform.pivot = new Vector2(0, 0);
                    m_frameCounter_transform.anchorMin = new Vector2(0.01f, 0.01f);
                    m_frameCounter_transform.anchorMax = new Vector2(0.01f, 0.01f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(0, 0);
                    break;
                case FpsCounterAnchorPositions.TopRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopRight;
                    m_frameCounter_transform.pivot = new Vector2(1, 1);
                    m_frameCounter_transform.anchorMin = new Vector2(0.99f, 0.99f);
                    m_frameCounter_transform.anchorMax = new Vector2(0.99f, 0.99f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(1, 1);
                    break;
                case FpsCounterAnchorPositions.BottomRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomRight;
                    m_frameCounter_transform.pivot = new Vector2(1, 0);
                    m_frameCounter_transform.anchorMin = new Vector2(0.99f, 0.01f);
                    m_frameCounter_transform.anchorMax = new Vector2(0.99f, 0.01f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(1, 0);
                    break;
            }
        }
    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\TMPro_InstructionOverlay.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    
    public class TMPro_InstructionOverlay : MonoBehaviour
    {

        public enum FpsCounterAnchorPositions { TopLeft, BottomLeft, TopRight, BottomRight };

        public FpsCounterAnchorPositions AnchorPosition = FpsCounterAnchorPositions.BottomLeft;

        private const string instructions = "Camera Control - <#ffff00>Shift + RMB\n</color>Zoom - <#ffff00>Mouse wheel.";

        private TextMeshPro m_TextMeshPro;
        private TextContainer m_textContainer;
        private Transform m_frameCounter_transform;
        private Camera m_camera;

        //private FpsCounterAnchorPositions last_AnchorPosition;

        void Awake()
        {
            if (!enabled)
                return;

            m_camera = Camera.main;

            GameObject frameCounter = new GameObject("Frame Counter");
            m_frameCounter_transform = frameCounter.transform;
            m_frameCounter_transform.parent = m_camera.transform;
            m_frameCounter_transform.localRotation = Quaternion.identity;


            m_TextMeshPro = frameCounter.AddComponent<TextMeshPro>();
            m_TextMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            m_TextMeshPro.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Overlay");

            m_TextMeshPro.fontSize = 30;

            m_TextMeshPro.isOverlay = true;
            m_textContainer = frameCounter.GetComponent<TextContainer>();

            Set_FrameCounter_Position(AnchorPosition);
            //last_AnchorPosition = AnchorPosition;

            m_TextMeshPro.text = instructions;

        }




        void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)
        {

            switch (anchor_position)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    //m_TextMeshPro.anchor = AnchorPositions.TopLeft;
                    m_textContainer.anchorPosition = TextContainerAnchors.TopLeft;
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(0, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomLeft:
                    //m_TextMeshPro.anchor = AnchorPositions.BottomLeft;
                    m_textContainer.anchorPosition = TextContainerAnchors.BottomLeft;
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(0, 0, 100.0f));
                    break;
                case FpsCounterAnchorPositions.TopRight:
                    //m_TextMeshPro.anchor = AnchorPositions.TopRight;
                    m_textContainer.anchorPosition = TextContainerAnchors.TopRight;
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(1, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomRight:
                    //m_TextMeshPro.anchor = AnchorPositions.BottomRight;
                    m_textContainer.anchorPosition = TextContainerAnchors.BottomRight;
                    m_frameCounter_transform.position = m_camera.ViewportToWorldPoint(new Vector3(1, 0, 100.0f));
                    break;
            }
        }
    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\VertexColorCycler.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class VertexColorCycler : MonoBehaviour
    {

        private TMP_Text m_TextComponent;

        void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
        }


        void Start()
        {
            StartCoroutine(AnimateVertexColors());
        }


        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimateVertexColors()
        {
            // Force the text object to update right away so we can have geometry to modify right from the start.
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            int currentCharacter = 0;

            Color32[] newVertexColors;
            Color32 c0 = m_TextComponent.color;

            while (true)
            {
                int characterCount = textInfo.characterCount;

                // If No Characters then just yield and wait for some text to be added
                if (characterCount == 0)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[currentCharacter].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[currentCharacter].vertexIndex;

                // Only change the vertex color if the text element is visible.
                if (textInfo.characterInfo[currentCharacter].isVisible)
                {
                    c0 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);

                    newVertexColors[vertexIndex + 0] = c0;
                    newVertexColors[vertexIndex + 1] = c0;
                    newVertexColors[vertexIndex + 2] = c0;
                    newVertexColors[vertexIndex + 3] = c0;

                    // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                    m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
                    // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
                }

                currentCharacter = (currentCharacter + 1) % characterCount;

                yield return new WaitForSeconds(0.05f);
            }
        }

    }
}

```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\VertexJitter.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class VertexJitter : MonoBehaviour
    {

        public float AngleMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float CurveScale = 1.0f;

        private TMP_Text m_TextComponent;
        private bool hasTextChanged;

        /// <summary>
        /// Structure to hold pre-computed animation data.
        /// </summary>
        private struct VertexAnim
        {
            public float angleRange;
            public float angle;
            public float speed;
        }

        void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        void Start()
        {
            StartCoroutine(AnimateVertexColors());
        }


        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj == m_TextComponent)
                hasTextChanged = true;
        }

        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimateVertexColors()
        {

            // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
            // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            Matrix4x4 matrix;

            int loopCount = 0;
            hasTextChanged = true;

            // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
            VertexAnim[] vertexAnim = new VertexAnim[1024];
            for (int i = 0; i < 1024; i++)
            {
                vertexAnim[i].angleRange = Random.Range(10f, 25f);
                vertexAnim[i].speed = Random.Range(1f, 3f);
            }

            // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            while (true)
            {
                // Get new copy of vertex data if the text has changed.
                if (hasTextChanged)
                {
                    // Update the copy of the vertex data for the text object.
                    cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

                    hasTextChanged = false;
                }

                int characterCount = textInfo.characterCount;

                // If No Characters then just yield and wait for some text to be added
                if (characterCount == 0)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }


                for (int i = 0; i < characterCount; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip characters that are not visible and thus have no geometry to manipulate.
                    if (!charInfo.isVisible)
                        continue;

                    // Retrieve the pre-computed animation data for the given character.
                    VertexAnim vertAnim = vertexAnim[i];

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Get the cached vertices of the mesh used by this text element (character or sprite).
                    Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                    // Determine the center point of each character at the baseline.
                    //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                    // Determine the center point of each character.
                    Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                    // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                    // This is needed so the matrix TRS is applied at the origin for each character.
                    Vector3 offset = charMidBasline;

                    Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                    destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                    destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                    destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                    destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                    vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                    Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                    matrix = Matrix4x4.TRS(jitterOffset * CurveScale, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

                    destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                    destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                    destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                    destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                    destinationVertices[vertexIndex + 0] += offset;
                    destinationVertices[vertexIndex + 1] += offset;
                    destinationVertices[vertexIndex + 2] += offset;
                    destinationVertices[vertexIndex + 3] += offset;

                    vertexAnim[i] = vertAnim;
                }

                // Push changes into meshes
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                loopCount += 1;

                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\VertexShakeA.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class VertexShakeA : MonoBehaviour
    {

        public float AngleMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float ScaleMultiplier = 1.0f;
        public float RotationMultiplier = 1.0f;

        private TMP_Text m_TextComponent;
        private bool hasTextChanged;


        void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        void Start()
        {
            StartCoroutine(AnimateVertexColors());
        }


        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj = m_TextComponent)
                hasTextChanged = true;
        }

        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimateVertexColors()
        {

            // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
            // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            Matrix4x4 matrix;
            Vector3[][] copyOfVertices = new Vector3[0][];

            hasTextChanged = true;

            while (true)
            {
                // Allocate new vertices 
                if (hasTextChanged)
                {
                    if (copyOfVertices.Length < textInfo.meshInfo.Length)
                        copyOfVertices = new Vector3[textInfo.meshInfo.Length][];

                    for (int i = 0; i < textInfo.meshInfo.Length; i++)
                    {
                        int length = textInfo.meshInfo[i].vertices.Length;
                        copyOfVertices[i] = new Vector3[length];
                    }

                    hasTextChanged = false;
                }

                int characterCount = textInfo.characterCount;

                // If No Characters then just yield and wait for some text to be added
                if (characterCount == 0)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                int lineCount = textInfo.lineCount;

                // Iterate through each line of the text.
                for (int i = 0; i < lineCount; i++)
                {

                    int first = textInfo.lineInfo[i].firstCharacterIndex;
                    int last = textInfo.lineInfo[i].lastCharacterIndex;

                    // Determine the center of each line
                    Vector3 centerOfLine = (textInfo.characterInfo[first].bottomLeft + textInfo.characterInfo[last].topRight) / 2;
                    Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(-0.25f, 0.25f) * RotationMultiplier);

                    // Iterate through each character of the line.
                    for (int j = first; j <= last; j++)
                    {
                        // Skip characters that are not visible and thus have no geometry to manipulate.
                        if (!textInfo.characterInfo[j].isVisible)
                            continue;

                        // Get the index of the material used by the current character.
                        int materialIndex = textInfo.characterInfo[j].materialReferenceIndex;

                        // Get the index of the first vertex used by this text element.
                        int vertexIndex = textInfo.characterInfo[j].vertexIndex;

                        // Get the vertices of the mesh used by this text element (character or sprite).
                        Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                        // Need to translate all 4 vertices of each quad to aligned with center of character.
                        // This is needed so the matrix TRS is applied at the origin for each character.
                        copyOfVertices[materialIndex][vertexIndex + 0] = sourceVertices[vertexIndex + 0] - centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 1] = sourceVertices[vertexIndex + 1] - centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 2] = sourceVertices[vertexIndex + 2] - centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 3] = sourceVertices[vertexIndex + 3] - centerOfLine;

                        // Determine the random scale change for each character.
                        float randomScale = Random.Range(0.995f - 0.001f * ScaleMultiplier, 1.005f + 0.001f * ScaleMultiplier);

                        // Setup the matrix rotation.
                        matrix = Matrix4x4.TRS(Vector3.one, rotation, Vector3.one * randomScale);

                        // Apply the matrix TRS to the individual characters relative to the center of the current line.
                        copyOfVertices[materialIndex][vertexIndex + 0] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 0]);
                        copyOfVertices[materialIndex][vertexIndex + 1] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 1]);
                        copyOfVertices[materialIndex][vertexIndex + 2] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 2]);
                        copyOfVertices[materialIndex][vertexIndex + 3] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 3]);

                        // Revert the translation change.
                        copyOfVertices[materialIndex][vertexIndex + 0] += centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 1] += centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 2] += centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 3] += centerOfLine;
                    }
                }

                // Push changes into meshes
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = copyOfVertices[i];
                    m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\VertexShakeB.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class VertexShakeB : MonoBehaviour
    {

        public float AngleMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float CurveScale = 1.0f;

        private TMP_Text m_TextComponent;
        private bool hasTextChanged;


        void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        void Start()
        {
            StartCoroutine(AnimateVertexColors());
        }


        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj = m_TextComponent)
                hasTextChanged = true;
        }

        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimateVertexColors()
        {

            // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
            // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            Matrix4x4 matrix;
            Vector3[][] copyOfVertices = new Vector3[0][];

            hasTextChanged = true;

            while (true)
            {
                // Allocate new vertices 
                if (hasTextChanged)
                {
                    if (copyOfVertices.Length < textInfo.meshInfo.Length)
                        copyOfVertices = new Vector3[textInfo.meshInfo.Length][];

                    for (int i = 0; i < textInfo.meshInfo.Length; i++)
                    {
                        int length = textInfo.meshInfo[i].vertices.Length;
                        copyOfVertices[i] = new Vector3[length];
                    }

                    hasTextChanged = false;
                }

                int characterCount = textInfo.characterCount;

                // If No Characters then just yield and wait for some text to be added
                if (characterCount == 0)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                int lineCount = textInfo.lineCount;

                // Iterate through each line of the text.
                for (int i = 0; i < lineCount; i++)
                {

                    int first = textInfo.lineInfo[i].firstCharacterIndex;
                    int last = textInfo.lineInfo[i].lastCharacterIndex;

                    // Determine the center of each line
                    Vector3 centerOfLine = (textInfo.characterInfo[first].bottomLeft + textInfo.characterInfo[last].topRight) / 2;
                    Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(-0.25f, 0.25f));

                    // Iterate through each character of the line.
                    for (int j = first; j <= last; j++)
                    {
                        // Skip characters that are not visible and thus have no geometry to manipulate.
                        if (!textInfo.characterInfo[j].isVisible)
                            continue;

                        // Get the index of the material used by the current character.
                        int materialIndex = textInfo.characterInfo[j].materialReferenceIndex;

                        // Get the index of the first vertex used by this text element.
                        int vertexIndex = textInfo.characterInfo[j].vertexIndex;

                        // Get the vertices of the mesh used by this text element (character or sprite).
                        Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                        // Determine the center point of each character at the baseline.
                        Vector3 charCenter = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                        // Need to translate all 4 vertices of each quad to aligned with center of character.
                        // This is needed so the matrix TRS is applied at the origin for each character.
                        copyOfVertices[materialIndex][vertexIndex + 0] = sourceVertices[vertexIndex + 0] - charCenter;
                        copyOfVertices[materialIndex][vertexIndex + 1] = sourceVertices[vertexIndex + 1] - charCenter;
                        copyOfVertices[materialIndex][vertexIndex + 2] = sourceVertices[vertexIndex + 2] - charCenter;
                        copyOfVertices[materialIndex][vertexIndex + 3] = sourceVertices[vertexIndex + 3] - charCenter;

                        // Determine the random scale change for each character.
                        float randomScale = Random.Range(0.95f, 1.05f);

                        // Setup the matrix for the scale change.
                        matrix = Matrix4x4.TRS(Vector3.one, Quaternion.identity, Vector3.one * randomScale);

                        // Apply the scale change relative to the center of each character.
                        copyOfVertices[materialIndex][vertexIndex + 0] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 0]);
                        copyOfVertices[materialIndex][vertexIndex + 1] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 1]);
                        copyOfVertices[materialIndex][vertexIndex + 2] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 2]);
                        copyOfVertices[materialIndex][vertexIndex + 3] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 3]);

                        // Revert the translation change.
                        copyOfVertices[materialIndex][vertexIndex + 0] += charCenter;
                        copyOfVertices[materialIndex][vertexIndex + 1] += charCenter;
                        copyOfVertices[materialIndex][vertexIndex + 2] += charCenter;
                        copyOfVertices[materialIndex][vertexIndex + 3] += charCenter;

                        // Need to translate all 4 vertices of each quad to aligned with the center of the line.
                        // This is needed so the matrix TRS is applied from the center of the line.
                        copyOfVertices[materialIndex][vertexIndex + 0] -= centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 1] -= centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 2] -= centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 3] -= centerOfLine;

                        // Setup the matrix rotation.
                        matrix = Matrix4x4.TRS(Vector3.one, rotation, Vector3.one);

                        // Apply the matrix TRS to the individual characters relative to the center of the current line.
                        copyOfVertices[materialIndex][vertexIndex + 0] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 0]);
                        copyOfVertices[materialIndex][vertexIndex + 1] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 1]);
                        copyOfVertices[materialIndex][vertexIndex + 2] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 2]);
                        copyOfVertices[materialIndex][vertexIndex + 3] = matrix.MultiplyPoint3x4(copyOfVertices[materialIndex][vertexIndex + 3]);

                        // Revert the translation change.
                        copyOfVertices[materialIndex][vertexIndex + 0] += centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 1] += centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 2] += centerOfLine;
                        copyOfVertices[materialIndex][vertexIndex + 3] += centerOfLine;
                    }
                }

                // Push changes into meshes
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = copyOfVertices[i];
                    m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\VertexZoom.cs

```csharp
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace TMPro.Examples
{

    public class VertexZoom : MonoBehaviour
    {
        public float AngleMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float CurveScale = 1.0f;

        private TMP_Text m_TextComponent;
        private bool hasTextChanged;


        void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            // UnSubscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        void Start()
        {
            StartCoroutine(AnimateVertexColors());
        }


        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj == m_TextComponent)
                hasTextChanged = true;
        }

        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimateVertexColors()
        {

            // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
            // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            Matrix4x4 matrix;
            TMP_MeshInfo[] cachedMeshInfoVertexData = textInfo.CopyMeshInfoVertexData();

            // Allocations for sorting of the modified scales
            List<float> modifiedCharScale = new List<float>();
            List<int> scaleSortingOrder = new List<int>();

            hasTextChanged = true;

            while (true)
            {
                // Allocate new vertices
                if (hasTextChanged)
                {
                    // Get updated vertex data
                    cachedMeshInfoVertexData = textInfo.CopyMeshInfoVertexData();

                    hasTextChanged = false;
                }

                int characterCount = textInfo.characterCount;

                // If No Characters then just yield and wait for some text to be added
                if (characterCount == 0)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                // Clear list of character scales
                modifiedCharScale.Clear();
                scaleSortingOrder.Clear();

                for (int i = 0; i < characterCount; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip characters that are not visible and thus have no geometry to manipulate.
                    if (!charInfo.isVisible)
                        continue;

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Get the cached vertices of the mesh used by this text element (character or sprite).
                    Vector3[] sourceVertices = cachedMeshInfoVertexData[materialIndex].vertices;

                    // Determine the center point of each character at the baseline.
                    //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                    // Determine the center point of each character.
                    Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                    // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                    // This is needed so the matrix TRS is applied at the origin for each character.
                    Vector3 offset = charMidBasline;

                    Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                    destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                    destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                    destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                    destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                    //Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                    // Determine the random scale change for each character.
                    float randomScale = Random.Range(1f, 1.5f);

                    // Add modified scale and index
                    modifiedCharScale.Add(randomScale);
                    scaleSortingOrder.Add(modifiedCharScale.Count - 1);

                    // Setup the matrix for the scale change.
                    //matrix = Matrix4x4.TRS(jitterOffset, Quaternion.Euler(0, 0, Random.Range(-5f, 5f)), Vector3.one * randomScale);
                    matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one * randomScale);

                    destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                    destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                    destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                    destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                    destinationVertices[vertexIndex + 0] += offset;
                    destinationVertices[vertexIndex + 1] += offset;
                    destinationVertices[vertexIndex + 2] += offset;
                    destinationVertices[vertexIndex + 3] += offset;

                    // Restore Source UVS which have been modified by the sorting
                    Vector4[] sourceUVs0 = cachedMeshInfoVertexData[materialIndex].uvs0;
                    Vector4[] destinationUVs0 = textInfo.meshInfo[materialIndex].uvs0;

                    destinationUVs0[vertexIndex + 0] = sourceUVs0[vertexIndex + 0];
                    destinationUVs0[vertexIndex + 1] = sourceUVs0[vertexIndex + 1];
                    destinationUVs0[vertexIndex + 2] = sourceUVs0[vertexIndex + 2];
                    destinationUVs0[vertexIndex + 3] = sourceUVs0[vertexIndex + 3];

                    // Restore Source Vertex Colors
                    Color32[] sourceColors32 = cachedMeshInfoVertexData[materialIndex].colors32;
                    Color32[] destinationColors32 = textInfo.meshInfo[materialIndex].colors32;

                    destinationColors32[vertexIndex + 0] = sourceColors32[vertexIndex + 0];
                    destinationColors32[vertexIndex + 1] = sourceColors32[vertexIndex + 1];
                    destinationColors32[vertexIndex + 2] = sourceColors32[vertexIndex + 2];
                    destinationColors32[vertexIndex + 3] = sourceColors32[vertexIndex + 3];
                }

                // Push changes into meshes
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    //// Sort Quads based modified scale
                    scaleSortingOrder.Sort((a, b) => modifiedCharScale[a].CompareTo(modifiedCharScale[b]));

                    textInfo.meshInfo[i].SortGeometry(scaleSortingOrder);

                    // Updated modified vertex attributes
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textInfo.meshInfo[i].mesh.SetUVs(0, textInfo.meshInfo[i].uvs0);
                    textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;

                    m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
```

---

## C:\project\surakshaAR\assets\TextMesh Pro\Examples & Extras\Scripts\WarpTextExample.cs

```csharp
using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    public class WarpTextExample : MonoBehaviour
    {

        private TMP_Text m_TextComponent;

        public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
        public float AngleMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float CurveScale = 1.0f;

        void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
        }


        void Start()
        {
            StartCoroutine(WarpText());
        }


        private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
        {
            AnimationCurve newCurve = new AnimationCurve();

            newCurve.keys = curve.keys;

            return newCurve;
        }


        /// <summary>
        ///  Method to curve text along a Unity animation curve.
        /// </summary>
        /// <param name="textComponent"></param>
        /// <returns></returns>
        IEnumerator WarpText()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            //Mesh mesh = m_TextComponent.textInfo.meshInfo[0].mesh;

            Vector3[] vertices;
            Matrix4x4 matrix;

            m_TextComponent.havePropertiesChanged = true; // Need to force the TextMeshPro Object to be updated.
            CurveScale *= 10;
            float old_CurveScale = CurveScale;
            AnimationCurve old_curve = CopyAnimationCurve(VertexCurve);

            while (true)
            {
                if (!m_TextComponent.havePropertiesChanged && old_CurveScale == CurveScale && old_curve.keys[1].value == VertexCurve.keys[1].value)
                {
                    yield return null;
                    continue;
                }

                old_CurveScale = CurveScale;
                old_curve = CopyAnimationCurve(VertexCurve);

                m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

                TMP_TextInfo textInfo = m_TextComponent.textInfo;
                int characterCount = textInfo.characterCount;


                if (characterCount == 0) continue;

                //vertices = textInfo.meshInfo[0].vertices;
                //int lastVertexIndex = textInfo.characterInfo[characterCount - 1].vertexIndex;

                float boundsMinX = m_TextComponent.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
                float boundsMaxX = m_TextComponent.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;



                for (int i = 0; i < characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Get the index of the mesh used by this character.
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    vertices = textInfo.meshInfo[materialIndex].vertices;

                    // Compute the baseline mid point for each character
                    Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                    //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                    // Apply offset to adjust our pivot point.
                    vertices[vertexIndex + 0] += -offsetToMidBaseline;
                    vertices[vertexIndex + 1] += -offsetToMidBaseline;
                    vertices[vertexIndex + 2] += -offsetToMidBaseline;
                    vertices[vertexIndex + 3] += -offsetToMidBaseline;

                    // Compute the angle of rotation for each character based on the animation curve
                    float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
                    float x1 = x0 + 0.0001f;
                    float y0 = VertexCurve.Evaluate(x0) * CurveScale;
                    float y1 = VertexCurve.Evaluate(x1) * CurveScale;

                    Vector3 horizontal = new Vector3(1, 0, 0);
                    //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                    Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

                    float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                    Vector3 cross = Vector3.Cross(horizontal, tangent);
                    float angle = cross.z > 0 ? dot : 360 - dot;

                    matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                    vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    vertices[vertexIndex + 0] += offsetToMidBaseline;
                    vertices[vertexIndex + 1] += offsetToMidBaseline;
                    vertices[vertexIndex + 2] += offsetToMidBaseline;
                    vertices[vertexIndex + 3] += offsetToMidBaseline;
                }


                // Upload the mesh with the revised information
                m_TextComponent.UpdateVertexData();

                yield return new WaitForSeconds(0.025f);
            }
        }
    }
}

```

---
