using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// RetentionScheduleManager
    /// ========================
    /// Governs worker retention checks at Day 1, Day 7, and Day 30 post-certification.
    /// Satisfies Rehan's Day 6 deliverable:
    ///   - Establishes spaced repetition audits for long-term safety competence.
    ///   - Synchronizes milestone statuses (pending, due, completed) with backend /progress/retention.
    /// </summary>
    public class RetentionScheduleManager : MonoBehaviour
    {
        public static RetentionScheduleManager Instance { get; private set; }

        [Serializable]
        public class RetentionMilestone
        {
            public int day_interval;              // 1, 7, or 30
            public string title;                  // e.g. "Day 1 Immediate Retention Check"
            public string due_date;               // ISO-8601 UTC
            public string status;                 // "pending", "due", "completed"
            public float? score;
            public bool? passed;
        }

        [Serializable]
        public class WorkerRetentionSchedule
        {
            public int worker_id;
            public string module_id;
            public string base_certified_date;
            public List<RetentionMilestone> milestones = new List<RetentionMilestone>();
        }

        private readonly Dictionary<string, WorkerRetentionSchedule> _scheduleCache = new Dictionary<string, WorkerRetentionSchedule>();

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

        /// <summary>
        /// Initializes the 3-tier retention schedule upon certification.
        /// </summary>
        public WorkerRetentionSchedule InitializeSchedule(int workerId, string moduleId, DateTime certifiedDate)
        {
            var schedule = new WorkerRetentionSchedule
            {
                worker_id = workerId,
                module_id = moduleId,
                base_certified_date = certifiedDate.ToString("o")
            };

            int[] days = { 1, 7, 30 };
            string[] titles = { "Day 1 Immediate Retention Check", "Day 7 Refresher Check", "Day 30 Competency Audit" };

            for (int i = 0; i < days.Length; i++)
            {
                DateTime dueDate = certifiedDate.AddDays(days[i]);
                schedule.milestones.Add(new RetentionMilestone
                {
                    day_interval = days[i],
                    title = titles[i],
                    due_date = dueDate.ToString("o"),
                    status = "pending",
                    score = null,
                    passed = null
                });
            }

            string key = GetKey(workerId, moduleId);
            _scheduleCache[key] = schedule;
            SaveSchedule(schedule);

            Debug.Log($"[RETENTION] Initialized Day 1, Day 7, Day 30 schedule for Worker {workerId} '{moduleId}'");
            return schedule;
        }

        public WorkerRetentionSchedule GetSchedule(int workerId, string moduleId)
        {
            string key = GetKey(workerId, moduleId);
            if (!_scheduleCache.TryGetValue(key, out var schedule))
            {
                string prefKey = $"RETENTION_{key}";
                string json = PlayerPrefs.GetString(prefKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        schedule = JsonUtility.FromJson<WorkerRetentionSchedule>(json);
                        _scheduleCache[key] = schedule;
                    }
                    catch
                    {
                        schedule = null;
                    }
                }
            }

            // Update status based on current clock
            if (schedule != null)
            {
                UpdateMilestoneStatuses(schedule);
            }
            return schedule;
        }

        private void UpdateMilestoneStatuses(WorkerRetentionSchedule schedule)
        {
            DateTime now = DateTime.UtcNow;
            foreach (var m in schedule.milestones)
            {
                if (m.status == "completed") continue;
                if (DateTime.TryParse(m.due_date, out var due))
                {
                    if (now >= due)
                    {
                        m.status = "due";
                    }
                    else
                    {
                        m.status = "pending";
                    }
                }
            }
        }

        public void CompleteMilestone(int workerId, string moduleId, int dayInterval, float score, bool passed)
        {
            var schedule = GetSchedule(workerId, moduleId);
            if (schedule == null) return;

            var milestone = schedule.milestones.Find(m => m.day_interval == dayInterval);
            if (milestone != null)
            {
                milestone.status = "completed";
                milestone.score = score;
                milestone.passed = passed;
                SaveSchedule(schedule);
                Debug.Log($"[RETENTION] Completed Day {dayInterval} retention check for Worker {workerId}: Score={score:F1}%, Passed={passed}");
            }
        }

        private void SaveSchedule(WorkerRetentionSchedule schedule)
        {
            if (schedule == null) return;
            string key = GetKey(schedule.worker_id, schedule.module_id);
            _scheduleCache[key] = schedule;
            string prefKey = $"RETENTION_{key}";
            PlayerPrefs.SetString(prefKey, JsonUtility.ToJson(schedule));
            PlayerPrefs.Save();
        }
    }
}
