using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class FireExtHandoffDiagnostic
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\run_handoff_diagnostic_trigger.txt";
        private const string OutputFile = @"C:\project\surakshaAR\Temp\fire_ext_handoff_diagnostic.txt";

        static FireExtHandoffDiagnostic()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                RunDiagnostic();
            }
        }

        [MenuItem("SurakshaAR/Run FireExt Handoff Diagnostic")]
        public static void RunDiagnostic()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== FIREEXT HANDOFF DIAGNOSTIC ===");
            sb.AppendLine($"Timestamp: {DateTime.UtcNow:o}");

            const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine($"ERROR: Failed to open scene {scenePath}");
                File.WriteAllText(OutputFile, sb.ToString());
                return;
            }

            var flow = UnityEngine.Object.FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
            var display = UnityEngine.Object.FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);
            var pickup = UnityEngine.Object.FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);

            sb.AppendLine("\n--- INITIAL RESOLUTION ---");
            sb.AppendLine($"FlowManager found: {flow != null}");
            sb.AppendLine($"ExtinguisherDisplayPickup found: {display != null}");
            sb.AppendLine($"ExtinguisherPickup found: {pickup != null}");

            if (display != null)
            {
                sb.AppendLine($"display.originalExtinguisher: {display.originalExtinguisher?.name}");
                sb.AppendLine($"display.arCamera: {display.arCamera?.name}");
                sb.AppendLine($"display.flowManager: {display.flowManager?.name}");
            }

            if (flow != null)
            {
                sb.AppendLine($"flow.displayPickup: {flow.displayPickup?.name}");
                sb.AppendLine($"flow.originalPickup: {flow.originalPickup?.name}");
            }

            if (display != null && flow != null && flow.originalPickup != null)
            {
                bool sameObject = ReferenceEquals(display.originalExtinguisher, flow.originalPickup.gameObject);
                sb.AppendLine($"display.originalExtinguisher == flow.originalPickup.gameObject: {sameObject}");
            }

            if (pickup != null)
            {
                sb.AppendLine("\n--- ORIGINAL FIREEXT STATE BEFORE INITIALIZE ---");
                sb.AppendLine($"pickup.gameObject: {pickup.gameObject.name} (activeSelf={pickup.gameObject.activeSelf}, activeInHierarchy={pickup.gameObject.activeInHierarchy})");
                sb.AppendLine($"pickup.IsHeld(): {pickup.IsHeld()}");
                sb.AppendLine($"pickup.IsRuntimeVisible(): {pickup.IsRuntimeVisible()}");
                sb.AppendLine($"pickup.transform.parent: {pickup.transform.parent?.name}");
                sb.AppendLine($"pickup.transform.position: {pickup.transform.position}");
                sb.AppendLine($"pickup.transform.lossyScale: {pickup.transform.lossyScale}");

                foreach (var r in pickup.GetComponentsInChildren<Renderer>(true))
                {
                    sb.AppendLine($"  Renderer: {r.name} ({r.GetType().Name}) enabled={r.enabled}");
                }
            }

            // Test what InitializeForTraining does
            sb.AppendLine("\n--- CALLING flow.InitializeForTraining() ---");
            flow?.InitializeForTraining();

            if (pickup != null)
            {
                sb.AppendLine($"pickup.gameObject activeSelf={pickup.gameObject.activeSelf}, activeInHierarchy={pickup.gameObject.activeInHierarchy}");
                sb.AppendLine($"pickup.IsHeld(): {pickup.IsHeld()}");
                sb.AppendLine($"pickup.IsRuntimeVisible(): {pickup.IsRuntimeVisible()}");
                foreach (var r in pickup.GetComponentsInChildren<Renderer>(true))
                {
                    sb.AppendLine($"  Renderer: {r.name} enabled={r.enabled}");
                }
            }
            if (display != null)
            {
                sb.AppendLine($"display.gameObject activeSelf={display.gameObject.activeSelf}");
            }

            // Test transition to Step 3
            sb.AppendLine("\n--- ADVANCING TO STEP 3 ---");
            flow?.TransitionToStep1();
            flow?.TransitionToStep2_ActivateAlarm();
            flow?.TransitionToStep3_SelectExtinguisher();

            sb.AppendLine($"FlowManager CurrentStage: {flow?.CurrentStage}");
            if (pickup != null)
            {
                sb.AppendLine($"pickup.gameObject activeSelf={pickup.gameObject.activeSelf}, activeInHierarchy={pickup.gameObject.activeInHierarchy}");
                sb.AppendLine($"pickup.IsHeld(): {pickup.IsHeld()}");
                sb.AppendLine($"pickup.IsRuntimeVisible(): {pickup.IsRuntimeVisible()}");
            }
            if (display != null)
            {
                sb.AppendLine($"display.gameObject activeSelf={display.gameObject.activeSelf}");
            }

            // Test display.Pickup()
            sb.AppendLine("\n--- EXECUTING display.Pickup() ---");
            display?.Pickup();

            sb.AppendLine($"FlowManager CurrentStage after Pickup(): {flow?.CurrentStage}");
            if (display != null)
            {
                sb.AppendLine($"display.gameObject activeSelf={display.gameObject.activeSelf}, isPickedUp={display.IsPickedUp}");
            }
            if (pickup != null)
            {
                sb.AppendLine($"pickup.gameObject activeSelf={pickup.gameObject.activeSelf}, activeInHierarchy={pickup.gameObject.activeInHierarchy}");
                sb.AppendLine($"pickup.IsHeld(): {pickup.IsHeld()}");
                sb.AppendLine($"pickup.IsRuntimeVisible(): {pickup.IsRuntimeVisible()}");
                sb.AppendLine($"pickup.transform.parent: {pickup.transform.parent?.name}");
                sb.AppendLine($"pickup.transform.position: {pickup.transform.position}");
                sb.AppendLine($"pickup.arCamera: {pickup.arCamera?.name}");
                if (pickup.arCamera != null)
                {
                    Vector3 expected = pickup.arCamera.TransformPoint(pickup.holdPosition);
                    sb.AppendLine($"expected hold pos: {expected}, dist: {Vector3.Distance(pickup.transform.position, expected)}");
                }
                foreach (var r in pickup.GetComponentsInChildren<Renderer>(true))
                {
                    sb.AppendLine($"  Renderer: {r.name} enabled={r.enabled}");
                }
            }

            File.WriteAllText(OutputFile, sb.ToString());
            Debug.Log($"[FireExtHandoffDiagnostic] Results written to {OutputFile}");
        }
    }
}
