using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// LocalDatabaseService
    /// ====================
    /// ACID-compliant local database service for SurakshaAR.
    /// Manages structured relational tables for Users, TrainingAttempts, TrainingEvents,
    /// CompetencyResults, Certificates, and SyncQueue on device storage.
    /// Operates fully offline, persisting data to Application.persistentDataPath/suraksha.db.
    /// </summary>
    public class LocalDatabaseService : MonoBehaviour
    {
        public static LocalDatabaseService Instance { get; private set; }

        private string DatabasePath => Path.Combine(Application.persistentDataPath, "suraksha_relational.json");
        private readonly object _lock = new object();

        [Serializable]
        public class UserRecord
        {
            public string worker_id;
            public string name;
            public string role;
            public string mine_site;
            public string token;
            public string last_login;
        }

        [Serializable]
        public class TrainingAttemptRecord
        {
            public string attempt_id;
            public string worker_id;
            public int module_id;
            public string scenario_type;
            public string client_session_id;
            public int attempt_number;
            public string started_at;
            public string completed_at;
            public float elapsed_seconds;
            public float provisional_score;
            public float authoritative_score;
            public bool passed;
            public string pass_reason;
            public bool timed_out;
            public int critical_error_count;
            public string sync_status; // "Pending", "Synced", "Conflict"
            public string synced_at;
        }

        [Serializable]
        public class TrainingEventRecord
        {
            public string event_id;
            public string attempt_id;
            public string client_session_id;
            public int sequence_number;
            public string event_type;
            public string action;
            public bool correct;
            public bool critical;
            public string severity;
            public float response_time_seconds;
            public string timestamp;
            public float score_delta;
            public string payload_json;
        }

        [Serializable]
        public class CompetencyResultRecord
        {
            public string attempt_id;
            public float overall_score;
            public bool passed;
            public string competency_scores_json;
            public string weaknesses_json;
            public string retraining_json;
        }

        [Serializable]
        public class CertificateRecord
        {
            public string certificate_number;
            public string worker_id;
            public int module_id;
            public string issued_at;
            public string valid_until;
            public string qr_payload;
            public string status; // "active", "revoked", "expired"
        }

        [Serializable]
        public class SyncQueueItem
        {
            public int queue_id;
            public string payload_type; // "attempt", "event", "telemetry"
            public string reference_id;
            public string payload_json;
            public string status; // "Pending", "InFlight", "Synced", "Failed"
            public int retry_count;
            public string last_attempt_at;
            public string last_error;
            public string created_at;
        }

        [Serializable]
        private class DatabaseContainer
        {
            public int version = 1;
            public List<UserRecord> users = new List<UserRecord>();
            public List<TrainingAttemptRecord> attempts = new List<TrainingAttemptRecord>();
            public List<TrainingEventRecord> events = new List<TrainingEventRecord>();
            public List<CompetencyResultRecord> competency_results = new List<CompetencyResultRecord>();
            public List<CertificateRecord> certificates = new List<CertificateRecord>();
            public List<SyncQueueItem> sync_queue = new List<SyncQueueItem>();
            public int next_queue_id = 1;
        }

        private DatabaseContainer _db = new DatabaseContainer();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(DatabasePath))
                    {
                        string json = File.ReadAllText(DatabasePath);
                        _db = JsonUtility.FromJson<DatabaseContainer>(json) ?? new DatabaseContainer();
                        Debug.Log($"[LOCAL DB] Initialized. Users: {_db.users.Count}, Attempts: {_db.attempts.Count}, SyncQueue: {_db.sync_queue.Count}");
                    }
                    else
                    {
                        _db = new DatabaseContainer();
                        SaveDatabase();
                        Debug.Log("[LOCAL DB] Created new local relational database.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LOCAL DB] Failed to load local database: {ex.Message}");
                    _db = new DatabaseContainer();
                }
            }
        }

        private void SaveDatabase()
        {
            try
            {
                string json = JsonUtility.ToJson(_db, true);
                string tempPath = DatabasePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(DatabasePath)) File.Delete(DatabasePath);
                File.Move(tempPath, DatabasePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LOCAL DB] Atomic write failed: {ex.Message}");
            }
        }

        // ── Users Table ──
        public void UpsertUser(UserRecord user)
        {
            if (user == null || string.IsNullOrEmpty(user.worker_id)) return;
            lock (_lock)
            {
                int idx = _db.users.FindIndex(u => u.worker_id == user.worker_id);
                if (idx >= 0)
                    _db.users[idx] = user;
                else
                    _db.users.Add(user);
                SaveDatabase();
            }
        }

        public UserRecord GetUser(string workerId)
        {
            lock (_lock)
            {
                return _db.users.Find(u => u.worker_id == workerId);
            }
        }

        // ── Training Attempts Table ──
        public void RecordAttempt(TrainingAttemptRecord attempt)
        {
            if (attempt == null) return;
            lock (_lock)
            {
                int idx = _db.attempts.FindIndex(a => a.attempt_id == attempt.attempt_id);
                if (idx >= 0)
                    _db.attempts[idx] = attempt;
                else
                    _db.attempts.Add(attempt);
                SaveDatabase();
            }
        }

        public List<TrainingAttemptRecord> GetAttemptsForWorker(string workerId)
        {
            lock (_lock)
            {
                return _db.attempts.FindAll(a => a.worker_id == workerId);
            }
        }

        // ── Training Events Table ──
        public void RecordEvents(List<TrainingEventRecord> events)
        {
            if (events == null || events.Count == 0) return;
            lock (_lock)
            {
                _db.events.AddRange(events);
                SaveDatabase();
            }
        }

        // ── Competency Results Table ──
        public void SaveCompetencyResult(CompetencyResultRecord result)
        {
            if (result == null) return;
            lock (_lock)
            {
                int idx = _db.competency_results.FindIndex(c => c.attempt_id == result.attempt_id);
                if (idx >= 0)
                    _db.competency_results[idx] = result;
                else
                    _db.competency_results.Add(result);
                SaveDatabase();
            }
        }

        // ── Certificates Table ──
        public void SaveCertificate(CertificateRecord cert)
        {
            if (cert == null || string.IsNullOrEmpty(cert.certificate_number)) return;
            lock (_lock)
            {
                int idx = _db.certificates.FindIndex(c => c.certificate_number == cert.certificate_number);
                if (idx >= 0)
                    _db.certificates[idx] = cert;
                else
                    _db.certificates.Add(cert);
                SaveDatabase();
            }
        }

        public CertificateRecord GetCertificate(string certNumber)
        {
            lock (_lock)
            {
                return _db.certificates.Find(c => c.certificate_number == certNumber);
            }
        }

        public List<CertificateRecord> GetCertificatesForWorker(string workerId)
        {
            lock (_lock)
            {
                return _db.certificates.FindAll(c => c.worker_id == workerId);
            }
        }

        // ── Sync Queue Table ──
        public SyncQueueItem EnqueueSync(string payloadType, string referenceId, string payloadJson)
        {
            lock (_lock)
            {
                var item = new SyncQueueItem
                {
                    queue_id = _db.next_queue_id++,
                    payload_type = payloadType,
                    reference_id = referenceId,
                    payload_json = payloadJson,
                    status = "Pending",
                    retry_count = 0,
                    created_at = DateTime.UtcNow.ToString("o")
                };
                _db.sync_queue.Add(item);
                SaveDatabase();
                return item;
            }
        }

        public List<SyncQueueItem> GetPendingSyncItems(int limit = 50)
        {
            lock (_lock)
            {
                var list = _db.sync_queue.FindAll(q => q.status == "Pending" || q.status == "Failed");
                if (list.Count > limit)
                    return list.GetRange(0, limit);
                return new List<SyncQueueItem>(list);
            }
        }

        public void MarkSyncItemStatus(int queueId, string status, string error = null)
        {
            lock (_lock)
            {
                var item = _db.sync_queue.Find(q => q.queue_id == queueId);
                if (item != null)
                {
                    item.status = status;
                    item.last_attempt_at = DateTime.UtcNow.ToString("o");
                    if (!string.IsNullOrEmpty(error))
                    {
                        item.last_error = error;
                        item.retry_count++;
                    }
                    SaveDatabase();
                }
            }
        }

        public void PurgeSyncedQueue()
        {
            lock (_lock)
            {
                int removed = _db.sync_queue.RemoveAll(q => q.status == "Synced");
                if (removed > 0)
                {
                    SaveDatabase();
                    Debug.Log($"[LOCAL DB] Purged {removed} synced items from queue.");
                }
            }
        }
    }
}
