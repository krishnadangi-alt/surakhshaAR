using System;

/// <summary>
/// TrainingEventManager
/// =====================
/// Central, local event bus for the whole SurakshaAR training application.
///
/// Every important worker action raises exactly one event here:
///
///   Login -> LanguageSelected -> TrainingSelected -> TrainingStarted ->
///   ScenarioPlaced -> AlarmActivated -> ExtinguisherPickedUp ->
///   PinRemoved -> ExtinguisherUsed -> FireExtinguished -> TrainingCompleted
///
/// Every raise is logged to the Unity Console as "[TRAINING EVENT] ...",
/// so you can verify that clicks really generate events even without
/// any backend.
///
/// FUTURE BACKEND:
/// This class is the single hook point. When a real backend exists,
/// subscribe here (e.g. from SurakshaApiClient) and forward the events.
/// No part of the app needs to change.
/// </summary>
public static class TrainingEventManager
{
    // =====================================================
    // EVENTS
    // =====================================================

    /// <summary>Raised after a (mock/local) login succeeded.</summary>
    public static event Action<string> OnLogin;

    /// <summary>Raised when the worker picks a language ("English"/"Hindi").</summary>
    public static event Action<string> OnLanguageSelected;

    /// <summary>Raised when the worker selects a training ("FireSafety").</summary>
    public static event Action<string> OnTrainingSelected;

    /// <summary>Raised when the worker presses START TRAINING.</summary>
    public static event Action OnTrainingStarted;

    /// <summary>Raised when the fire scenario is placed on the floor.</summary>
    public static event Action OnScenarioPlaced;

    /// <summary>Raised when the fire alarm is activated.</summary>
    public static event Action OnAlarmActivated;

    /// <summary>Raised when the extinguisher is picked up.</summary>
    public static event Action OnExtinguisherPickedUp;

    /// <summary>Raised when the safety pin is removed.</summary>
    public static event Action OnPinRemoved;

    /// <summary>Raised when the worker starts using (spraying) the extinguisher.</summary>
    public static event Action OnExtinguisherUsed;

    /// <summary>Raised only when the fire is actually extinguished.</summary>
    public static event Action OnFireExtinguished;

    /// <summary>Raised when the training flow is fully completed.</summary>
    public static event Action OnTrainingCompleted;

    // =====================================================
    // RAISE METHODS (single logging point)
    // =====================================================

    public static void RaiseLogin(string employeeId)
    {
        Log("Login: " + employeeId);
        OnLogin?.Invoke(employeeId);
    }

    public static void RaiseLanguageSelected(string language)
    {
        Log("LanguageSelected: " + language);
        OnLanguageSelected?.Invoke(language);
    }

    public static void RaiseTrainingSelected(string training)
    {
        Log("TrainingSelected: " + training);
        OnTrainingSelected?.Invoke(training);
    }

    public static void RaiseTrainingStarted()
    {
        Log("TrainingStarted");
        OnTrainingStarted?.Invoke();
    }

    public static void RaiseScenarioPlaced()
    {
        Log("ScenarioPlaced");
        OnScenarioPlaced?.Invoke();
    }

    public static void RaiseAlarmActivated()
    {
        Log("AlarmActivated");
        OnAlarmActivated?.Invoke();
    }

    public static void RaiseExtinguisherPickedUp()
    {
        Log("ExtinguisherPickedUp");
        OnExtinguisherPickedUp?.Invoke();
    }

    public static void RaisePinRemoved()
    {
        Log("PinRemoved");
        OnPinRemoved?.Invoke();
    }

    public static void RaiseExtinguisherUsed()
    {
        Log("ExtinguisherUsed");
        OnExtinguisherUsed?.Invoke();
    }

    public static void RaiseFireExtinguished()
    {
        Log("FireExtinguished");
        OnFireExtinguished?.Invoke();
    }

    public static void RaiseTrainingCompleted()
    {
        Log("TrainingCompleted");
        OnTrainingCompleted?.Invoke();
    }

    private static void Log(string message)
    {
        UnityEngine.Debug.Log("[TRAINING EVENT] " + message);
    }
}
