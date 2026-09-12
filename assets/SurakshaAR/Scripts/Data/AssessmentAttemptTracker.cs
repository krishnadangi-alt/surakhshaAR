using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// AssessmentAttemptTracker
    /// ========================
    /// Tracks complete attempt history per worker per module.
    /// Satisfies Rehan's Day 4 deliverable:
    ///   - Stores each attempt with date, score, critical error count, pass/fail result.
    ///   - Computes score improvement delta across attempts.
    ///   - Integrates with OfflineDataStore for persistent storage.
    /// </summary>
    public class AssessmentAttemptTracker : MonoBehaviour
    {
        public static AssessmentAttemptTracker Instance { get; private set; }

        [Serializable]
        public class AttemptRecord
        {
            public int worker_id;
            public string module_id;
            public int attempt_number;
            public float score;
            public bool passed;
            public int critical_errors_count;
            public string timestamp;
            public float delta_from_previous;
        }

        [Serializable]
        public class WorkerHistory
        {
            public int worker_id;
            public string module_id;
            public List<AttemptRecord> attempts = new List<AttemptRecord>();
        }

        private readonly Dictionary<string, WorkerHistory> _historyCache = new Dictionary<string, WorkerHistory>();

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

        private string GetKey(int workerId, string moduleId) => $"{workerId}_{moduleId}";

        public WorkerHistory GetHistory(int workerId, string moduleId)
        {
            string key = GetKey(workerId, moduleId);
            if (!_historyCache.TryGetValue(key, out var history))
            {
                string prefKey = $"ATTEMPT_HIST_{key}";
                string json = PlayerPrefs.GetString(prefKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        history = JsonUtility.FromJson<WorkerHistory>(json);
                    }
                    catch
                    {
                        history = null;
                    }
                }

                if (history == null)
                {
                    history = new WorkerHistory
                    {
                        worker_id = workerId,
                        module_id = moduleId
                    };
                }
                _historyCache[key] = history;
            }
            return history;
        }

        public AttemptRecord RecordAttempt(int workerId, string moduleId, float score, bool passed, int criticalErrors)
        {
            var history = GetHistory(workerId, moduleId);
            int nextAttemptNum = history.attempts.Count + 1;
            float prevScore = history.attempts.Count > 0 ? history.attempts[history.attempts.Count - 1].score : score;
            float delta = score - prevScore;

            var record = new AttemptRecord
            {
                worker_id = workerId,
                module_id = moduleId,
                attempt_number = nextAttemptNum,
                score = score,
                passed = passed,
                critical_errors_count = criticalErrors,
                timestamp = DateTime.UtcNow.ToString("o"),
                delta_from_previous = delta
            };

            history.attempts.Add(record);
            string prefKey = $"ATTEMPT_HIST_{GetKey(workerId, moduleId)}";
            PlayerPrefs.SetString(prefKey, JsonUtility.ToJson(history));
            PlayerPrefs.Save();

            Debug.Log($"[ATTEMPT TRACKER] Worker {workerId} Module '{moduleId}' Attempt #{nextAttemptNum}: Score={score:F1}%, Passed={passed}, Delta={delta:+0.0;-0.0}");
            return record;
        }

        public int GetAttemptCount(int workerId, string moduleId)
        {
            return GetHistory(workerId, moduleId).attempts.Count;
        }

        public AttemptRecord GetLatestAttempt(int workerId, string moduleId)
        {
            var hist = GetHistory(workerId, moduleId);
            return hist.attempts.Count > 0 ? hist.attempts[hist.attempts.Count - 1] : null;
        }
    }
}
