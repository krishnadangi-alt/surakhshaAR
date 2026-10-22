#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Machinery;

namespace SurakshaAR.Editor
{
    public static class MachineryIntegrationValidator
    {
        [MenuItem("SurakshaAR/Validate Complete 3-Module Integration")]
        public static void RunValidation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("SurakshaAR INTEGRATION VALIDATION REPORT");
            sb.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("================================================================================");

            bool allPassed = true;

            void AssertTest(string testName, bool condition, string details)
            {
                if (condition)
                {
                    sb.AppendLine($"[PASS] {testName} - {details}");
                }
                else
                {
                    sb.AppendLine($"[FAIL] {testName} - {details}");
                    allPassed = false;
                }
            }

            // ── TEST 1: Build Settings Registration ───────────────────────
            var buildScenes = EditorBuildSettings.scenes;
            bool hasMain = buildScenes.Any(s => s.enabled && s.path.Contains("Main.unity"));
            bool hasFire = buildScenes.Any(s => s.enabled && s.path.Contains("FireTraining.unity"));
            bool hasGas = buildScenes.Any(s => s.enabled && s.path.Contains("AR_gas_Foundation.unity"));
            bool hasMachinery = buildScenes.Any(s => s.enabled && s.path.Contains("MachineryConveyorJam.unity"));

            AssertTest("BUILD-01: Main Scene in Build Settings", hasMain, "Assets/SurakshaAR/Scenes/Main.unity registered");
            AssertTest("BUILD-02: Fire AR Scene in Build Settings", hasFire, "FireTraining.unity registered");
            AssertTest("BUILD-03: Gas AR Scene in Build Settings", hasGas, "AR_gas_Foundation.unity registered");
            AssertTest("BUILD-04: Machinery AR Scene in Build Settings", hasMachinery, "MachineryConveyorJam.unity registered");

            // ── TEST 2: Module Catalog Configuration ──────────────────────
            var catalog = ModuleCatalog.BuildDefaultCatalog();
            var fireMod = catalog.FirstOrDefault(m => m.id == ModuleId.FireAndExplosion);
            var gasMod = catalog.FirstOrDefault(m => m.id == ModuleId.GasLeakConfinedSpace);
            var machMod = catalog.FirstOrDefault(m => m.id == ModuleId.MachinerySafety);

            AssertTest("CATALOG-01: Fire Module Catalog", fireMod != null && fireMod.isImplemented && fireMod.arSceneName == "FireTraining", $"Implemented: {fireMod?.isImplemented}, Scene: {fireMod?.arSceneName}");
            AssertTest("CATALOG-02: Gas Module Catalog", gasMod != null && gasMod.isImplemented && gasMod.arSceneName == "AR_gas_Foundation", $"Implemented: {gasMod?.isImplemented}, Scene: {gasMod?.arSceneName}");
            AssertTest("CATALOG-03: Machinery Module Catalog", machMod != null && machMod.isImplemented && machMod.arSceneName == "MachineryConveyorJam", $"Implemented: {machMod?.isImplemented}, Scene: {machMod?.arSceneName}");

            // ── TEST 3: MachineryConveyorJam Scene Integrity ─────────────
            string machineryScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            if (File.Exists(machineryScenePath))
            {
                var scene = EditorSceneManager.OpenScene(machineryScenePath, OpenSceneMode.Single);
                int missingScripts = 0;
                var missingList = new System.Collections.Generic.List<string>();
                void CheckGO(GameObject g)
                {
                    int c = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(g);
                    if (c > 0)
                    {
                        missingScripts += c;
                        missingList.Add($"{g.name} ({c})");
                    }
                }
                foreach (var go in scene.GetRootGameObjects())
                {
                    CheckGO(go);
                    foreach (var child in go.GetComponentsInChildren<Transform>(true))
                    {
                        CheckGO(child.gameObject);
                    }
                }
                AssertTest("MACHINERY-01: Zero Missing Scripts", missingScripts == 0, $"Missing script count: {missingScripts}, Objects: {string.Join(", ", missingList)}");

                var uiCtrl = UnityEngine.Object.FindFirstObjectByType<MachineryScenarioUIController>();
                AssertTest("MACHINERY-02: MachineryScenarioUIController Present", uiCtrl != null, uiCtrl != null ? "Controller verified" : "Controller missing");

                var placeCtrl = UnityEngine.Object.FindFirstObjectByType<MachineryARPlacementController>();
                AssertTest("MACHINERY-03: MachineryARPlacementController Present", placeCtrl != null, placeCtrl != null ? "Placement controller verified" : "Placement controller missing");

                var scenarioRoot = GameObject.Find("MachineryScenario");
                AssertTest("MACHINERY-04: Machinery Physical Scenario Root", scenarioRoot != null, scenarioRoot != null ? $"Root found with {scenarioRoot.transform.childCount} children" : "MachineryScenario root missing");
            }
            else
            {
                AssertTest("MACHINERY: Scene File Exists", false, $"File not found: {machineryScenePath}");
            }

            // ── TEST 4: Fire Scene Preservation ───────────────────────────
            string fireScenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
            if (File.Exists(fireScenePath))
            {
                var scene = EditorSceneManager.OpenScene(fireScenePath, OpenSceneMode.Single);
                int missingScripts = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    foreach (var child in go.GetComponentsInChildren<Transform>(true))
                    {
                        missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                    }
                }
                AssertTest("FIRE-01: Fire Scene Zero Missing Scripts", missingScripts == 0, $"Missing script count: {missingScripts}");
            }
            else
            {
                AssertTest("FIRE: Scene File Exists", false, $"File not found: {fireScenePath}");
            }

            // ── TEST 5: Gas Scene Preservation ────────────────────────────
            string gasScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
            if (File.Exists(gasScenePath))
            {
                var scene = EditorSceneManager.OpenScene(gasScenePath, OpenSceneMode.Single);
                int missingScripts = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    foreach (var child in go.GetComponentsInChildren<Transform>(true))
                    {
                        missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                    }
                }
                AssertTest("GAS-01: Gas Scene Zero Missing Scripts", missingScripts == 0, $"Missing script count: {missingScripts}");
            }
            else
            {
                AssertTest("GAS: Scene File Exists", false, $"File not found: {gasScenePath}");
            }

            // ── TEST 6: Main Entry Scene Preservation ─────────────────────
            string mainScenePath = "Assets/SurakshaAR/Scenes/Main.unity";
            if (File.Exists(mainScenePath))
            {
                var scene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
                int missingScripts = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    foreach (var child in go.GetComponentsInChildren<Transform>(true))
                    {
                        missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                    }
                }
                AssertTest("MAIN-01: Main Scene Zero Missing Scripts", missingScripts == 0, $"Missing script count: {missingScripts}");
            }
            else
            {
                AssertTest("MAIN: Scene File Exists", false, $"File not found: {mainScenePath}");
            }

            sb.AppendLine("================================================================================");
            sb.AppendLine($"INTEGRATION VALIDATION RESULT: {(allPassed ? "ALL TESTS PASSED" : "TEST FAILURES DETECTED")}");
            sb.AppendLine("================================================================================");

            File.WriteAllText("integration_validation_report.txt", sb.ToString());
            Debug.Log(sb.ToString());

            if (!allPassed)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }
    }
}
#endif
