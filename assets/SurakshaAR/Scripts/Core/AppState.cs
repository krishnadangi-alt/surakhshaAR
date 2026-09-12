using System;
using System.Collections.Generic;
using SurakshaAR.Data;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Centralized application state tracking active user, selected module/scenario,
    /// language, and assessment scores. Persists session data and notifies listeners.
    /// </summary>
    public class AppState : MonoBehaviour
    {
        public static AppState Instance { get; private set; }

        public AppLanguage CurrentLanguage { get; set; } = AppLanguage.English;
        public string EmployeeId { get; set; } = "M10234";
        public string WorkerName { get; set; } = "Ramesh Kumar";
        public string WorkerRole { get; set; } = "Mine Worker";
        public bool IsGuestMode { get; set; } = false;

        public ModuleData SelectedModule { get; set; }
        public int SelectedScenarioIndex { get; set; } = 1;
        public string SelectedScenarioTitle { get; set; } = "Electrical Panel Fire";

        public int AssessmentScore { get; set; } = 95;
        public int AssessmentTotalQuestions { get; set; } = 7;
        public int CompletedModulesCount { get; set; } = 3;
        public int TotalModulesCount { get; set; } = 5;

        // Rich Competency & Attempt Tracking
        public float LastARTimerSeconds { get; set; } = 48f;
        public int CorrectActionsCount { get; set; } = 7;
        public int WrongActionsCount { get; set; } = 0;
        public int UnsafeActionsCount { get; set; } = 0;
        public int CriticalErrorsCount { get; set; } = 0;
        public bool IsPassed => AssessmentScore >= 75 && CriticalErrorsCount == 0;
        public bool IsRetrainingMode { get; set; } = false;
        public int PreviousAttemptScore { get; set; } = 65;
        public string CertificateId { get; set; } = "IND-SAR-2026-0941";
        public string CertificationDate { get; set; } = "12 Sept 2026";

        public event Action OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void SetLanguage(AppLanguage language)
        {
            CurrentLanguage = language;
            AppManager.Instance?.Localization?.SetLanguage(language);
            NotifyChange();
        }

        public void SetUser(string employeeId, string workerName, bool isGuest = false)
        {
            EmployeeId = string.IsNullOrWhiteSpace(employeeId) ? "M10234" : employeeId;
            WorkerName = string.IsNullOrWhiteSpace(workerName) ? "Ramesh Kumar" : workerName;
            IsGuestMode = isGuest;
            NotifyChange();
        }

        public void SelectScenario(int index, string title)
        {
            SelectedScenarioIndex = index;
            SelectedScenarioTitle = title;
            NotifyChange();
        }

        public void RecordAssessmentResult(int score, int total)
        {
            AssessmentScore = score;
            AssessmentTotalQuestions = total;
            if (CompletedModulesCount < TotalModulesCount)
            {
                CompletedModulesCount = Mathf.Min(CompletedModulesCount + 1, TotalModulesCount);
            }
            NotifyChange();
        }

        public void NotifyChange()
        {
            OnStateChanged?.Invoke();
        }
    }
}
