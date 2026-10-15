#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class GasAutomatedVerification
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\run_gas_validation_trigger.txt";
        private const string ResultFile = @"C:\project\surakshaAR\Temp\gas_validation_results.json";
        private const string BuildTriggerFile = @"C:\project\surakshaAR\Temp\run_gas_build_trigger.txt";

        static GasAutomatedVerification()
        {
            EditorApplication.update += CheckTrigger;
        }

        public static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                ExecuteAllGasValidation();
            }

            if (File.Exists(BuildTriggerFile))
            {
                try { File.Delete(BuildTriggerFile); } catch {}
                GasAndroidBuildScript.BuildAndroidAPK();
            }
        }

        [MenuItem("SurakshaAR/Execute Authoritative Gas Validation")]
        public static void ExecuteAllGasValidation()
        {
            Debug.Log("==================================================");
            Debug.Log("[GAS VALIDATION TRIGGER] Starting complete 14-step verification and screenshot capture...");
            Debug.Log("==================================================");

            var result = new GasValidationReport();
            result.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                GasFunctionalSequenceTester.RunFunctionalTest();
                result.functional_tests_completed = true;
            }
            catch (Exception ex)
            {
                result.functional_tests_completed = false;
                result.functional_error = ex.Message;
                Debug.LogError($"[GAS VALIDATION] Functional test failed: {ex.Message}\n{ex.StackTrace}");
            }

            try
            {
                GasFlowProofCapturer.RenderProofFlow();
                result.screenshots_rendered = true;

                string screenshotDir = @"C:\SurakhshaAR\ReviewScreenshots\PROOF_FLOW";
                if (Directory.Exists(screenshotDir))
                {
                    result.captured_files = Directory.GetFiles(screenshotDir, "*.png");
                }
            }
            catch (Exception ex)
            {
                result.screenshots_rendered = false;
                result.screenshot_error = ex.Message;
                Debug.LogError($"[GAS VALIDATION] Screenshot capture failed: {ex.Message}\n{ex.StackTrace}");
            }

            result.overall_success = result.functional_tests_completed && result.screenshots_rendered;

            string json = JsonUtility.ToJson(result, true);
            try
            {
                File.WriteAllText(ResultFile, json);
                Debug.Log($"[GAS VALIDATION] Report written to: {ResultFile}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GAS VALIDATION] Could not write report file: {ex.Message}");
            }

            Debug.Log("==================================================");
            Debug.Log($"[GAS VALIDATION COMPLETE] Success: {result.overall_success}, Screenshots: {result.captured_files?.Length ?? 0}");
            Debug.Log("==================================================");
        }

        [Serializable]
        public class GasValidationReport
        {
            public string timestamp;
            public bool overall_success;
            public bool functional_tests_completed;
            public string functional_error;
            public bool screenshots_rendered;
            public string screenshot_error;
            public string[] captured_files;
        }
    }
}
#endif
