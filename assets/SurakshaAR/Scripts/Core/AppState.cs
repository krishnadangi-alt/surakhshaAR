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
        public static AppState Instance { get; set; }

        public AppLanguage CurrentLanguage { get; set; } = AppLanguage.English;
        public string EmployeeId { get; set; } = "EMP-PROD-CERT";
        public string WorkerName { get; set; } = "Krishna";
        public string WorkerRole { get; set; } = "Safety Officer / Mine Worker";
        public string MineSite { get; set; } = "Dhanbad Colliery";
        public bool IsGuestMode { get; set; } = false;

        public ModuleData SelectedModule { get; set; }
        public int SelectedScenarioIndex { get; set; } = 1;
        public string SelectedScenarioTitle { get; set; } = "Electrical Panel Fire";

        public int AssessmentScore { get; set; } = 90;
        public int AssessmentTotalQuestions { get; set; } = 6;
        public int CompletedModulesCount { get; set; } = 1;
        public int TotalModulesCount { get; set; } = 3;

        // Rich Competency & Attempt Tracking
        public float LastARTimerSeconds { get; set; } = 0f;
        public int CorrectActionsCount { get; set; } = 6;
        public int WrongActionsCount { get; set; } = 0;
        public int UnsafeActionsCount { get; set; } = 0;
        public int CriticalErrorsCount { get; set; } = 0;
        public bool LastAttemptTimedOut { get; set; } = false;
        public bool FireExtinguishedSuccess { get; set; } = true;
        public bool IsPassed => AssessmentScore >= 80 && CriticalErrorsCount == 0 && !LastAttemptTimedOut;
        public bool IsRetrainingMode { get; set; } = false;
        public int PreviousAttemptScore { get; set; } = 0;
        public string CertificateId { get; set; } = "SUR-2026-0002";
        public string CertificationDate { get; set; } = "2026-09-21";

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

        public void SetUser(string employeeId, string workerName, bool isGuest = false, string role = "", string site = "")
        {
            EmployeeId = employeeId ?? "";
            WorkerName = !string.IsNullOrWhiteSpace(workerName) ? workerName : "";
            IsGuestMode = isGuest;
            WorkerRole = role ?? "";
            MineSite = site ?? "";

            if (!string.IsNullOrEmpty(EmployeeId) && OfflineSyncManager.Instance != null)
            {
                OfflineSyncManager.Instance.RegisterOrSyncWorkerId(EmployeeId, WorkerName, IsGuestMode, WorkerRole);
            }

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
            if (IsPassed && string.IsNullOrEmpty(CertificateId))
            {
                CertificateId = $"IND-SAR-{DateTime.UtcNow:yyyyMMdd}-{Mathf.Abs(EmployeeId.GetHashCode()) % 10000:D4}";
                CertificationDate = DateTime.UtcNow.ToString("dd MMM yyyy");
            }
            NotifyChange();
        }

        public void NotifyChange()
        {
            OnStateChanged?.Invoke();
        }
    }
}
