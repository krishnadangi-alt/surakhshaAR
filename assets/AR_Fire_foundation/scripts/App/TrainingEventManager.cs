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

    /// <summary>Raised when the fire hazard is identified.</summary>
    public static event Action OnHazardIdentified;

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

    // ── Fine-Grained Fire Workflow Events (added per Master Implementation Rule) ──

    /// <summary>Raised once when the worker correctly activates the grip after pin removal.</summary>
    public static event Action OnGripActivated;

    /// <summary>Raised when the nozzle aim is confirmed valid (within cone/distance of fire base).</summary>
    public static event Action OnValidAim;

    /// <summary>Raised (rate-limited) when the nozzle aim is off-target or invalid.</summary>
    public static event Action OnInvalidAim;

    /// <summary>Raised when spray particles make valid contact with the fire (contact start transition).</summary>
    public static event Action OnSprayContactValid;

    /// <summary>Raised when spray contact is first lost and the grace period begins.</summary>
    public static event Action OnSprayInterrupted;

    /// <summary>Raised when the grace period expires and the contact timer resets to 0.</summary>
    public static event Action OnSprayContactReset;

    /// <summary>Raised when the worker attempts to spray before removing the safety pin.</summary>
    public static event Action OnPrematureSprayAttempt;

    /// <summary>Raised when the 420-second scenario timeout is triggered (fire not extinguished in time).</summary>
    public static event Action OnScenarioTimeout;

    // ── Backend Telemetry & 12 Common Assessment Events (Day 1 Specification) ──
    public static event Action<string> OnAssessmentStarted;
    public static event Action<string, string> OnScenarioStartedCommon; // module, scenario
    public static event Action<string, float> OnHazardIdentifiedCommon; // hazardType, responseTime
    public static event Action<string, bool, float> OnPpeSelectedCommon; // ppeType, correct, responseTime
    public static event Action<string, bool, float> OnEquipmentSelectedCommon; // equipmentType, correct, responseTime
    public static event Action<string, string, float> OnObjectInteractionCommon; // objectId, interactionType, responseTime
    public static event Action<string, float> OnCorrectActionCommon; // action, responseTime
    public static event Action<string, string, string> OnWrongAction; // action, severity, reason
    public static event Action<string, string> OnUnsafeActionCommon; // action, reason
    public static event Action<string, string> OnCriticalAction; // action, reason
    public static event Action<string, string> OnSequenceErrorCommon; // expectedAction, actualAction
    public static event Action<string, float> OnResponseTimeCommon; // action, seconds
    public static event Action<string, float, bool> OnScenarioCompletedCommon; // module, score, passed
    public static event Action<string, bool> OnEvacuationStarted; // route, safe
    public static event Action<string, bool> OnPpeSelected; // ppeType, correct
    public static event Action<string, bool> OnEquipmentSelected; // equipmentType, correct
    public static event Action<float> OnAssessmentCompleted; // durationSeconds

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

    public static void RaiseHazardIdentified()
    {
        Log("HazardIdentified");
        OnHazardIdentified?.Invoke();
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

    public static void RaiseAssessmentStarted(string scenarioType)
    {
        Log("AssessmentStarted: " + scenarioType);
        OnAssessmentStarted?.Invoke(scenarioType);
    }

    public static void RaiseWrongAction(string action, string severity, string reason)
    {
        Log($"WrongAction [{severity}]: {action} - {reason}");
        OnWrongAction?.Invoke(action, severity, reason);
    }

    public static void RaiseCriticalAction(string action, string reason)
    {
        Log($"CRITICAL ACTION: {action} - {reason}");
        OnCriticalAction?.Invoke(action, reason);
    }

    public static void RaiseEvacuationStarted(string route, bool safe)
    {
        Log($"EvacuationStarted: Route={route}, Safe={safe}");
        OnEvacuationStarted?.Invoke(route, safe);
    }

    public static void RaisePpeSelected(string ppeType, bool correct)
    {
        Log($"PpeSelected: {ppeType}, Correct={correct}");
        OnPpeSelected?.Invoke(ppeType, correct);
    }

    public static void RaiseEquipmentSelected(string equipmentType, bool correct)
    {
        Log($"EquipmentSelected: {equipmentType}, Correct={correct}");
        OnEquipmentSelected?.Invoke(equipmentType, correct);
    }

    public static void RaiseAssessmentCompleted(float durationSeconds)
    {
        Log($"AssessmentCompleted: duration={durationSeconds}s");
        OnAssessmentCompleted?.Invoke(durationSeconds);
    }

    // ── 12 Common Assessment Event Raise Methods (Day 1 Standard) ─────────

    public static void RaiseScenarioStarted(string module, string scenario)
    {
        Log($"[COMMON EVENT] SCENARIO_STARTED: module={module}, scenario={scenario}");
        OnScenarioStartedCommon?.Invoke(module, scenario);
    }

    public static void RaiseHazardIdentified(string hazardType, float responseTime = 0f)
    {
        Log($"[COMMON EVENT] HAZARD_IDENTIFIED: type={hazardType}, responseTime={responseTime:F2}s");
        OnHazardIdentifiedCommon?.Invoke(hazardType, responseTime);
        OnHazardIdentified?.Invoke();
    }

    public static void RaisePpeSelected(string ppeType, bool correct, float responseTime = 0f)
    {
        Log($"[COMMON EVENT] PPE_SELECTED: ppe={ppeType}, correct={correct}, responseTime={responseTime:F2}s");
        OnPpeSelectedCommon?.Invoke(ppeType, correct, responseTime);
        OnPpeSelected?.Invoke(ppeType, correct);
    }

    public static void RaiseEquipmentSelected(string equipmentType, bool correct, float responseTime = 0f)
    {
        Log($"[COMMON EVENT] EQUIPMENT_SELECTED: equip={equipmentType}, correct={correct}, responseTime={responseTime:F2}s");
        OnEquipmentSelectedCommon?.Invoke(equipmentType, correct, responseTime);
        OnEquipmentSelected?.Invoke(equipmentType, correct);
    }

    public static void RaiseObjectInteraction(string objectId, string interactionType, float responseTime = 0f)
    {
        Log($"[COMMON EVENT] OBJECT_INTERACTION: obj={objectId}, type={interactionType}, responseTime={responseTime:F2}s");
        OnObjectInteractionCommon?.Invoke(objectId, interactionType, responseTime);
    }

    public static void RaiseCorrectAction(string action, float responseTime = 0f)
    {
        Log($"[COMMON EVENT] CORRECT_ACTION: {action}, responseTime={responseTime:F2}s");
        OnCorrectActionCommon?.Invoke(action, responseTime);
    }

    public static void RaiseUnsafeAction(string action, string reason)
    {
        Log($"[COMMON EVENT] UNSAFE_ACTION: {action} - {reason}");
        OnUnsafeActionCommon?.Invoke(action, reason);
    }

    public static void RaiseSequenceError(string expectedAction, string actualAction)
    {
        Log($"[COMMON EVENT] SEQUENCE_ERROR: Expected '{expectedAction}', performed '{actualAction}'");
        OnSequenceErrorCommon?.Invoke(expectedAction, actualAction);
    }

    public static void RaiseResponseTime(string action, float seconds)
    {
        Log($"[COMMON EVENT] RESPONSE_TIME: {action} took {seconds:F2}s");
        OnResponseTimeCommon?.Invoke(action, seconds);
    }

    public static void RaiseScenarioCompleted(string module, float score, bool passed)
    {
        Log($"[COMMON EVENT] SCENARIO_COMPLETED: module={module}, score={score:F1}, passed={passed}");
        OnScenarioCompletedCommon?.Invoke(module, score, passed);
    }

    // ── Fine-Grained Fire Workflow Raise Methods ────────────────────────────

    public static void RaiseGripActivated()
    {
        Log("GripActivated");
        OnGripActivated?.Invoke();
    }

    public static void RaiseValidAim()
    {
        Log("ValidAim");
        OnValidAim?.Invoke();
    }

    public static void RaiseInvalidAim()
    {
        Log("InvalidAim");
        OnInvalidAim?.Invoke();
    }

    public static void RaiseSprayContactValid()
    {
        Log("SprayContactValid");
        OnSprayContactValid?.Invoke();
    }

    public static void RaiseSprayInterrupted()
    {
        Log("SprayInterrupted");
        OnSprayInterrupted?.Invoke();
    }

    public static void RaiseSprayContactReset()
    {
        Log("SprayContactReset");
        OnSprayContactReset?.Invoke();
    }

    public static void RaisePrematureSprayAttempt()
    {
        Log("PrematureSprayAttempt");
        OnPrematureSprayAttempt?.Invoke();
    }

    public static void RaiseScenarioTimeout()
    {
        Log("ScenarioTimeout");
        OnScenarioTimeout?.Invoke();
    }

    private static void Log(string message)
    {
        UnityEngine.Debug.Log("[TRAINING EVENT] " + message);
    }
}
