using UnityEngine;

/// <summary>
/// AppSession
/// ==========
/// Persistent (DontDestroyOnLoad) local session + training state machine.
///
/// Holds (no backend - all local):
///   - EmployeeId       (from the mock login)
///   - Language         ("English" / "Hindi")
///   - CurrentTraining  ("FireSafety")
///   - TrainingState    (the enforced step-by-step state machine)
///   - Completion status (PlayerPrefs)
///
/// The state machine records the training progress. The AR flow
/// (FireScenarioFlowManager) drives it forward; AppSession only stores,
/// validates and exposes it.
///
/// Attach to the AppSession object in the MainMenu scene.
/// It survives scene loads and is also auto-created if missing,
/// so the AR scene keeps working when launched directly.
/// </summary>
public class AppSession : MonoBehaviour
{
    // =====================================================
    // TRAINING STATE MACHINE
    // =====================================================

    public enum TrainingState
    {
        NotStarted,
        ScenarioNotPlaced,
        AlarmRequired,
        AlarmActivated,
        ExtinguisherRequired,
        ExtinguisherPickedUp,
        PinRequired,
        PinRemoved,
        ExtinguisherInUse,
        FireExtinguished,
        Completed
    }

    public static AppSession Instance { get; private set; }

    [Header("Session (local only)")]
    public string employeeId = "";
    public string language = "English";
    public string currentTraining = "";

    /// <summary>Current enforced training state.</summary>
    public TrainingState State { get; private set; } = TrainingState.NotStarted;

    private const string PrefLanguage = "surakshaar.language";
    private const string PrefLastEmployee = "surakshaar.lastEmployeeId";
    private const string PrefFireCompleted = "surakshaar.fireSafety.completed";

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Restore non-sensitive local data.
        language = PlayerPrefs.GetString(PrefLanguage, language);
        employeeId = PlayerPrefs.GetString(PrefLastEmployee, employeeId);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Returns the session, creating a temporary one if the app was
    /// launched directly in the AR scene (keeps AR scene standalone-safe).
    /// </summary>
    public static AppSession EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject go = new GameObject("AppSession (auto)");
        AppSession session = go.AddComponent<AppSession>();
        return session;
    }

    // =====================================================
    // LOGIN (mock / local only)
    // =====================================================

    public void CompleteLogin(string id)
    {
        employeeId = id;
        PlayerPrefs.SetString(PrefLastEmployee, id);
        PlayerPrefs.Save();
    }

    // =====================================================
    // LANGUAGE
    // =====================================================

    public void SetLanguage(string newLanguage)
    {
        language = newLanguage;
        PlayerPrefs.SetString(PrefLanguage, newLanguage);
        PlayerPrefs.Save();
    }

    // =====================================================
    // TRAINING STATE
    // =====================================================

    /// <summary>Move the training state machine forward.</summary>
    public void SetState(TrainingState newState)
    {
        if (State == newState)
        {
            return;
        }

        TrainingState previous = State;
        State = newState;

        Debug.Log("[TRAINING STATE] " + previous + " -> " + newState);

        if (newState == TrainingState.Completed)
        {
            MarkTrainingCompleted();
        }
    }

    /// <summary>Called when the worker presses START TRAINING.</summary>
    public void BeginTraining(string trainingId)
    {
        currentTraining = trainingId;
        SetState(TrainingState.ScenarioNotPlaced);
    }

    /// <summary>Resets the AR flow states (used on RETRY).</summary>
    public void ResetTrainingProgress()
    {
        SetState(TrainingState.NotStarted);
    }

    public bool IsFireSafetyCompleted()
    {
        return PlayerPrefs.GetInt(PrefFireCompleted, 0) == 1;
    }

    private void MarkTrainingCompleted()
    {
        if (currentTraining == "FireSafety")
        {
            PlayerPrefs.SetInt(PrefFireCompleted, 1);
            PlayerPrefs.Save();
        }
    }
}
