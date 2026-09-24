using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Screens;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class LiveEditorSyncRunner
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\run_editor_live_sync_trigger.txt";
        private const string ResultFile = @"C:\project\surakshaAR\Temp\editor_live_sync_result.json";

        static LiveEditorSyncRunner()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                RunSyncTest();
            }
        }

        [MenuItem("SurakshaAR/Test Live Sync In Editor")]
        public static void RunSyncTest()
        {
            Debug.Log("[EDITOR SYNC TEST] Starting live sync test inside Unity Editor...");

            var report = new SyncTestReport();
            report.timestamp = DateTime.UtcNow.ToString("o");

            try
            {
                // 1. Setup singletons
                var syncGO = GameObject.Find("EditorSyncTestHost") ?? new GameObject("EditorSyncTestHost");
                
                var auth = syncGO.GetComponent<AuthSession>() ?? syncGO.AddComponent<AuthSession>();
                var appState = syncGO.GetComponent<AppState>() ?? syncGO.AddComponent<AppState>();
                var store = syncGO.GetComponent<OfflineDataStore>() ?? syncGO.AddComponent<OfflineDataStore>();
                var syncMgr = syncGO.GetComponent<OfflineSyncManager>() ?? syncGO.AddComponent<OfflineSyncManager>();

                // Configure worker and URLs
                appState.SetUser("JH-MN-004821", "Ramesh Kumar", false, "Equipment Operator", "Industrial Mine Site, Jharkhand");
                auth.SetSession("test-token", "worker", "JH-MN-004821", 2);
                syncMgr.BackendBaseUrl = "http://127.0.0.1:8000";

                report.initial_backend_url = syncMgr.BackendBaseUrl;

                // 2. Queue a new unique assessment session
                string testSessionId = $"sess_editor_{Guid.NewGuid():N}".Substring(0, 16);
                string testAttemptId = $"att_editor_{Guid.NewGuid():N}".Substring(0, 16);

                var newSession = new SyncSessionData
                {
                    type = "assessment",
                    module_id = 1,
                    score = 88.5f,
                    passed = true,
                    occurred_at = DateTime.UtcNow.ToString("o"),
                    scenario_type = "fire",
                    attempt_number = 99,
                    client_session_id = testSessionId,
                    attempt_id = testAttemptId,
                    events = new List<AssessmentEvent>
                    {
                        new AssessmentEvent
                        {
                            event_type = "assessment_started",
                            action = "start_assessment",
                            result = "correct",
                            timestamp = DateTime.UtcNow.ToString("o"),
                            hazard_type = "fire",
                            correct = true
                        },
                        new AssessmentEvent
                        {
                            event_type = "CORRECT_ACTION",
                            action = "aim_at_base_of_fire",
                            result = "correct",
                            timestamp = DateTime.UtcNow.ToString("o"),
                            correct = true,
                            response_time_seconds = 1.8f
                        },
                        new AssessmentEvent
                        {
                            event_type = "CORRECT_ACTION",
                            action = "fire_extinguished",
                            result = "correct",
                            timestamp = DateTime.UtcNow.ToString("o"),
                            correct = true,
                            response_time_seconds = 12.5f
                        },
                        new AssessmentEvent
                        {
                            event_type = "SCENARIO_COMPLETED",
                            action = "complete_scenario",
                            result = "correct",
                            timestamp = DateTime.UtcNow.ToString("o"),
                            correct = true,
                            response_time_seconds = 15.0f
                        }
                    }
                };

                store.Data.pending_sync_queue.Add(newSession);
                report.pending_before_sync = store.PendingSyncCount;
                report.test_session_id = testSessionId;

                // 3. Construct and send payload via UnityWebRequest directly
                // (mirroring what OfflineSyncManager does)
                var payload = new SyncPayloadDto
                {
                    device_id = "editor-laptop-test",
                    worker_id = 2,
                    employee_id = "JH-MN-004821",
                    employee_name = "Ramesh Kumar",
                    batch_id = $"batch_ed_{Guid.NewGuid():N}".Substring(0, 12),
                    pending_sessions = store.PendingSyncCount,
                    sessions = new List<SyncSessionData>(store.Data.pending_sync_queue)
                };

                string json = JsonUtility.ToJson(payload);
                string syncUrl = "http://127.0.0.1:8000/api/v1/sync";

                var req = new UnityWebRequest(syncUrl, "POST");
                byte[] raw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(raw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 10;

                var asyncOp = req.SendWebRequest();

                // Poll async operation in editor update loop
                EditorApplication.CallbackFunction poller = null;
                poller = () =>
                {
                    if (asyncOp.isDone)
                    {
                        EditorApplication.update -= poller;

                        report.http_status = req.responseCode;
                        report.http_error = req.error;
                        report.raw_response = req.downloadHandler != null ? req.downloadHandler.text : "";

                        if (req.result == UnityWebRequest.Result.Success)
                        {
                            report.success = true;
                            store.MarkSessionsSynced(payload.sessions.Count);
                            report.pending_after_sync = store.PendingSyncCount;
                            Debug.Log($"[EDITOR SYNC TEST] SUCCESS! Response: {report.raw_response}");

                            // 4. Verify UI transitions on HomeDashboard
                            try
                            {
                                var testCanvasGO = new GameObject("TestSyncCanvas", typeof(Canvas));
                                var uiRoot = HomeDashboardBuilder.Build();
                                uiRoot.transform.SetParent(testCanvasGO.transform, false);
                                var homeCtrl = new HomeDashboardController();
                                homeCtrl.OnShow(uiRoot, null);

                                var statusLbl = UIHelper.FindTMP(uiRoot, "label-sync-status-text");
                                var pillLbl = UIHelper.FindTMP(uiRoot, "label-sync-pill-text");

                                report.ui_status_text = statusLbl != null ? statusLbl.text : "NOT_FOUND";
                                report.ui_pill_text = pillLbl != null ? pillLbl.text : "NOT_FOUND";
                                report.ui_verified = (report.ui_pill_text == "SYNCED");

                                GameObject.DestroyImmediate(uiRoot);
                                GameObject.DestroyImmediate(testCanvasGO);
                            }
                            catch (Exception uiEx)
                            {
                                report.ui_error = uiEx.Message;
                            }
                        }
                        else
                        {
                            report.success = false;
                            Debug.LogError($"[EDITOR SYNC TEST] FAILED ({req.responseCode}): {req.error} | {report.raw_response}");
                        }

                        req.Dispose();
                        File.WriteAllText(ResultFile, JsonUtility.ToJson(report, true));
                    }
                };

                EditorApplication.update += poller;
            }
            catch (Exception ex)
            {
                report.success = false;
                report.error = ex.ToString();
                Debug.LogError($"[EDITOR SYNC TEST] Exception: {ex}");
                File.WriteAllText(ResultFile, JsonUtility.ToJson(report, true));
            }
        }

        [Serializable]
        private class SyncPayloadDto
        {
            public int worker_id = 0;
            public string guest_id;
            public string employee_id;
            public string employee_name;
            public string device_id;
            public string batch_id;
            public int pending_sessions;
            public List<SyncSessionData> sessions = new List<SyncSessionData>();
        }

        [Serializable]
        public class SyncTestReport
        {
            public bool success;
            public string timestamp;
            public string initial_backend_url;
            public string test_session_id;
            public int pending_before_sync;
            public int pending_after_sync;
            public long http_status;
            public string http_error;
            public string raw_response;
            public string error = "";
            public string ui_status_text = "";
            public string ui_pill_text = "";
            public bool ui_verified = false;
            public string ui_error = "";
        }
    }
}
