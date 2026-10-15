// =============================================================================
//  MachineryFinalSceneFixAndVerify.cs
//  SurakshaAR - MachineryConveyorJam Scene: FINAL Fix + Verify
//
//  Menu: SurakshaAR / Machinery / FINAL -- Fix Renderers + Transforms + Verify
//
//  Phases:
//    Phase 0 -- Backup scene
//    Phase 1 -- Pre-fix read-only audit
//    Phase 2 -- Enable ALL MeshRenderers (CRITICAL: 862 disabled renderers)
//    Phase 3 -- Apply correct transforms (all 10 required objects)
//    Phase 4 -- Hierarchy cleanup (remove dupe Hasp, create LOTOStation, Floor_01)
//    Phase 5 -- Material verification (null/error shaders only)
//    Phase 6 -- Coal / JamArea arrangement (175 existing coal pieces)
//    Phase 7 -- Save scene
//    Phase 8 -- Post-fix validation report
//
//  SAFETY RULES:
//    - NEVER destroys JamArea or its 175 coal children
//    - NEVER touches AR Session, XR Origin, MachinerySession, UI Canvas, EventSystem
//    - NEVER modifies scripts/prefabs/GLB files
//    - Idempotent: safe to run multiple times
// =============================================================================

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFinalSceneFixAndVerify
    {
        private const string MENU_FIX   = "SurakshaAR/Machinery/FINAL -- Fix Renderers + Transforms + Verify";
        private const string MENU_AUDIT = "SurakshaAR/Machinery/AUDIT ONLY -- Scene Pre-Fix Report";

        private const string SCENE_PATH  = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string BACKUP_PATH = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam_PRE_FIX_BACKUP.unity";
        private const string REPORT_DIR  = "Builds/Verification/UnityScene";

        private static readonly HashSet<string> PRESERVE_ROOTS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AR Session", "XR Origin", "MachinerySession",
            "UI Canvas", "EventSystem", "Directional Light"
        };

        // =================================================================
        [MenuItem(MENU_FIX, false, 100)]
        public static void RunFullFix()
        {
            try
            {
                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Opening scene...", 0.02f);
                Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    Debug.LogError("[Fix] Could not open scene: " + SCENE_PATH);
                    return;
                }

                GameObject masterRoot = GameObject.Find("MachineryScenario");
                if (masterRoot == null)
                {
                    Debug.LogError("[Fix] MachineryScenario not found in scene!");
                    return;
                }

                var log = new System.Text.StringBuilder();
                log.AppendLine("=== SURAKSHAAR MACHINERY SCENE FIX -- " + DateTime.Now + " ===");
                log.AppendLine();

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 0 -- Backup...", 0.05f);
                Phase0_Backup(log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 1 -- Pre-fix audit...", 0.10f);
                Phase1_Audit(masterRoot, log, "PRE-FIX");

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 2 -- Enabling renderers...", 0.20f);
                Phase2_EnableAllRenderers(masterRoot, log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 3 -- Applying transforms...", 0.35f);
                Phase3_ApplyTransforms(masterRoot, log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 4 -- Hierarchy cleanup...", 0.55f);
                Phase4_HierarchyCleanup(masterRoot, log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 5 -- Materials...", 0.65f);
                Phase5_FixMaterials(masterRoot, log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 6 -- Coal placement...", 0.75f);
                Phase6_ArrangeCoal(masterRoot, log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 7 -- Saving scene...", 0.88f);
                Phase7_SaveScene(scene, log);

                EditorUtility.DisplayProgressBar("SurakshaAR Fix", "Phase 8 -- Post-fix validation...", 0.95f);
                Phase1_Audit(masterRoot, log, "POST-FIX");
                Phase8_ValidationReport(masterRoot, log);

                WriteReport(log.ToString());
                Debug.Log("[SurakshaAR] SCENE FIX COMPLETE. Check console and report in " + REPORT_DIR);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem(MENU_AUDIT, false, 101)]
        public static void RunAuditOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid()) { Debug.LogError("[Audit] Could not open scene."); return; }
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null) { Debug.LogError("[Audit] MachineryScenario not found."); return; }
            var log = new System.Text.StringBuilder();
            log.AppendLine("=== SURAKSHAAR MACHINERY SCENE AUDIT -- " + DateTime.Now + " ===");
            log.AppendLine();
            Phase1_Audit(masterRoot, log, "AUDIT");
            WriteReport(log.ToString());
            Debug.Log("[Audit] Done. Report written to " + REPORT_DIR);
        }

        // =================================================================
        //  PHASE 0 -- BACKUP
        // =================================================================
        private static void Phase0_Backup(System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 0: BACKUP --");
            if (!AssetDatabase.CopyAsset(SCENE_PATH, BACKUP_PATH))
            {
                string tsPath = BACKUP_PATH.Replace(".unity",
                    "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".unity");
                if (AssetDatabase.CopyAsset(SCENE_PATH, tsPath))
                    log.AppendLine("  Backup created: " + tsPath);
                else
                    log.AppendLine("  WARNING: Could not create backup (file may already exist).");
            }
            else
            {
                log.AppendLine("  Backup created: " + BACKUP_PATH);
            }
            log.AppendLine();
        }

        // =================================================================
        //  PHASE 1 -- READ-ONLY AUDIT
        // =================================================================
        private static void Phase1_Audit(GameObject masterRoot, System.Text.StringBuilder log, string label)
        {
            log.AppendLine("-- PHASE 1: " + label + " AUDIT --");

            string[] requiredObjects = {
                "ConveyorAssembly", "FeedHopper", "DriveMotor", "JamArea",
                "ControlPanel", "EmergencyStop", "MainIsolator",
                "SteelWorkstationTable", "Hasp", "PadlockAndTag", "MotorBasePlate"
            };

            MeshRenderer[] allR = masterRoot.GetComponentsInChildren<MeshRenderer>(true);
            int totalR = allR.Length, enabledR = 0, disabledR = 0;
            foreach (var r in allR)
            {
                if (r.enabled && r.gameObject.activeInHierarchy) enabledR++;
                else disabledR++;
            }

            log.AppendLine("  MachineryScenario pos:" + masterRoot.transform.localPosition
                + " rot:" + masterRoot.transform.localEulerAngles
                + " scale:" + masterRoot.transform.localScale);
            log.AppendLine("  Total MeshRenderers: " + totalR
                + " | Enabled+Active: " + enabledR
                + " | Disabled/Inactive: " + disabledR);
            log.AppendLine();

            foreach (string objName in requiredObjects)
            {
                Transform t = FindDeepChild(masterRoot.transform, objName);
                if (t == null) { log.AppendLine("  [MISSING] " + objName); continue; }

                MeshRenderer[] mrs = t.GetComponentsInChildren<MeshRenderer>(true);
                int enR = 0;
                foreach (var r in mrs) if (r.enabled) enR++;

                Bounds b = GetBounds(t.gameObject);
                int dupCount = 0;
                foreach (Transform child in masterRoot.transform)
                    if (string.Equals(child.name, objName, StringComparison.OrdinalIgnoreCase))
                        dupCount++;

                string dupWarn = dupCount > 1 ? " [DUPLICATE x" + dupCount + "]" : "";
                log.AppendLine("  [" + (t.gameObject.activeInHierarchy ? "ACTIVE" : "INACTIVE") + "] "
                    + objName + dupWarn);
                log.AppendLine("     pos=" + t.localPosition.ToString("F3")
                    + " rot=" + t.localEulerAngles.ToString("F1")
                    + " scale=" + t.localScale.ToString("F4"));
                log.AppendLine("     MeshR: " + mrs.Length + " total | " + enR
                    + " enabled | bounds.size=" + b.size.ToString("F2"));
                if (objName == "JamArea")
                    log.AppendLine("     Coal children: " + t.childCount);
            }
            log.AppendLine();
        }

        // =================================================================
        //  PHASE 2 -- ENABLE ALL MESH RENDERERS
        // =================================================================
        private static void Phase2_EnableAllRenderers(GameObject masterRoot, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 2: ENABLE ALL MESH RENDERERS --");

            // Activate all intermediate GameObjects first
            Transform[] allTransforms = masterRoot.GetComponentsInChildren<Transform>(true);
            int activatedParents = 0;
            foreach (var t in allTransforms)
            {
                if (!t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(true);
                    activatedParents++;
                }
            }

            // Enable all MeshRenderers
            MeshRenderer[] allRenderers = masterRoot.GetComponentsInChildren<MeshRenderer>(true);
            int enabledCount = 0;
            foreach (var r in allRenderers)
            {
                if (!r.enabled)
                {
                    r.enabled = true;
                    enabledCount++;
                }
            }

            log.AppendLine("  Enabled " + enabledCount + " previously-disabled renderers.");
            log.AppendLine("  Activated " + activatedParents + " previously-inactive GameObjects.");
            log.AppendLine();
            Debug.Log("[Phase2] Enabled " + enabledCount + " renderers, activated "
                + activatedParents + " objects under MachineryScenario.");
        }

        // =================================================================
        //  PHASE 3 -- APPLY CORRECT TRANSFORMS
        // =================================================================
        private static void Phase3_ApplyTransforms(GameObject masterRoot, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 3: APPLY CORRECT TRANSFORMS --");

            // Reset MachineryScenario root
            masterRoot.transform.localPosition    = Vector3.zero;
            masterRoot.transform.localEulerAngles = Vector3.zero;
            masterRoot.transform.localScale       = Vector3.one;
            log.AppendLine("  MachineryScenario reset to (0,0,0).");

            // 1. CONVEYOR ASSEMBLY (Master Reference)
            Transform conveyorT = FindDirectChild(masterRoot.transform, "ConveyorAssembly");
            if (conveyorT != null)
            {
                conveyorT.localEulerAngles = new Vector3(-90f, 90f, 0f);
                conveyorT.localScale       = new Vector3(0.012f, 0.012f, 0.012f);
                conveyorT.localPosition    = Vector3.zero;
                GroundObject(conveyorT.gameObject);
                CenterObjectXZ(conveyorT.gameObject);
                log.AppendLine("  ConveyorAssembly -> pos=" + conveyorT.localPosition.ToString("F3")
                    + " rot=(-90,90,0) scale=0.012");
            }
            else log.AppendLine("  [MISSING] ConveyorAssembly");

            Bounds cBounds = GetBounds(conveyorT != null ? conveyorT.gameObject : masterRoot);
            float beltTopY = cBounds.max.y;
            log.AppendLine("  Conveyor bounds: " + cBounds + "  beltTopY=" + beltTopY.ToString("F3"));

            // 2. FEED HOPPER
            Transform hopperT = FindDirectChild(masterRoot.transform, "FeedHopper");
            if (hopperT != null)
            {
                hopperT.localEulerAngles = new Vector3(0f, 90f, 0f);
                hopperT.localScale       = new Vector3(0.00065f, 0.00065f, 0.00065f);
                float hX = cBounds.min.x + 0.35f;
                float hZ = cBounds.center.z;
                hopperT.localPosition = new Vector3(hX, 0f, hZ);
                Bounds hBounds = GetBounds(hopperT.gameObject);
                float hY = beltTopY + 0.08f - hBounds.min.y;
                hopperT.localPosition = new Vector3(hX, hY, hZ);
                log.AppendLine("  FeedHopper -> pos=" + hopperT.localPosition.ToString("F3")
                    + " rot=(0,90,0) scale=0.00065");
            }
            else log.AppendLine("  [MISSING] FeedHopper");

            // 3. DRIVE MOTOR
            Transform motorT = FindDirectChild(masterRoot.transform, "DriveMotor");
            if (motorT != null)
            {
                motorT.localEulerAngles = new Vector3(0f, 180f, 0f);
                motorT.localScale       = new Vector3(0.42f, 0.42f, 0.42f);
                float mX = cBounds.max.x + 0.22f;
                motorT.localPosition = new Vector3(mX, 0f, cBounds.center.z);
                GroundObject(motorT.gameObject);
                log.AppendLine("  DriveMotor -> pos=" + motorT.localPosition.ToString("F3")
                    + " rot=(0,180,0) scale=0.42");
            }
            else log.AppendLine("  [MISSING] DriveMotor");

            // 3b. MOTOR BASE PLATE
            Transform basePlateT = FindDirectChild(masterRoot.transform, "MotorBasePlate");
            if (basePlateT != null && motorT != null)
            {
                basePlateT.localPosition    = new Vector3(motorT.localPosition.x, 0.025f, motorT.localPosition.z);
                basePlateT.localEulerAngles = Vector3.zero;
                basePlateT.localScale       = new Vector3(0.75f, 0.05f, 0.60f);
                log.AppendLine("  MotorBasePlate -> pos=" + basePlateT.localPosition.ToString("F3"));
            }

            // 4. CONTROL PANEL
            Transform ctrlT = FindDirectChild(masterRoot.transform, "ControlPanel");
            if (ctrlT != null)
            {
                ctrlT.localEulerAngles = new Vector3(0f, 180f, 0f);
                ctrlT.localScale       = new Vector3(0.00095f, 0.00095f, 0.00095f);
                ctrlT.localPosition    = new Vector3(2.10f, 1.05f, 1.60f);
                log.AppendLine("  ControlPanel -> pos=" + ctrlT.localPosition.ToString("F3")
                    + " rot=(0,180,0) scale=0.00095");
            }
            else log.AppendLine("  [MISSING] ControlPanel");

            // 5. EMERGENCY STOP
            Transform estopT = FindDirectChild(masterRoot.transform, "EmergencyStop");
            if (estopT != null)
            {
                estopT.localEulerAngles = new Vector3(-90f, 180f, 0f);
                estopT.localScale       = new Vector3(0.0022f, 0.0022f, 0.0022f);
                estopT.localPosition    = new Vector3(2.75f, 1.15f, 1.60f);
                log.AppendLine("  EmergencyStop -> pos=" + estopT.localPosition.ToString("F3")
                    + " rot=(-90,180,0) scale=0.0022");
            }
            else log.AppendLine("  [MISSING] EmergencyStop");

            // 6. MAIN ISOLATOR
            Transform isolatorT = FindDirectChild(masterRoot.transform, "MainIsolator");
            if (isolatorT != null)
            {
                isolatorT.localEulerAngles = new Vector3(0f, 180f, 0f);
                isolatorT.localScale       = new Vector3(0.24f, 0.24f, 0.24f);
                isolatorT.localPosition    = new Vector3(1.45f, 1.15f, 1.60f);
                log.AppendLine("  MainIsolator -> pos=" + isolatorT.localPosition.ToString("F3")
                    + " rot=(0,180,0) scale=0.24");
            }
            else log.AppendLine("  [MISSING] MainIsolator");

            // 7. STEEL WORKSTATION TABLE
            Transform tableT = FindDirectChild(masterRoot.transform, "SteelWorkstationTable");
            if (tableT != null)
            {
                tableT.localEulerAngles = new Vector3(-90f, 90f, 0f);
                tableT.localScale       = new Vector3(0.011f, 0.011f, 0.011f);
                tableT.localPosition    = new Vector3(2.10f, 0f, -1.50f);
                GroundObject(tableT.gameObject);
                log.AppendLine("  SteelWorkstationTable -> pos=" + tableT.localPosition.ToString("F3")
                    + " rot=(-90,90,0) scale=0.011");
            }
            else log.AppendLine("  [MISSING] SteelWorkstationTable");

            Bounds tBounds = GetBounds(tableT != null ? tableT.gameObject : masterRoot);
            float tableTopY = tBounds.max.y;
            float tableCX   = tBounds.center.x;
            float tableCZ   = tBounds.center.z;
            log.AppendLine("  Table top Y=" + tableTopY.ToString("F3")
                + " center X=" + tableCX.ToString("F3") + " Z=" + tableCZ.ToString("F3"));

            // 8. HASP (first instance -- duplicate removed in Phase 4)
            Transform haspT = FindDirectChild(masterRoot.transform, "Hasp");
            if (haspT != null)
            {
                haspT.localEulerAngles = new Vector3(-90f, 45f, 0f);
                haspT.localScale       = new Vector3(0.0016f, 0.0016f, 0.0016f);
                Bounds haspB = GetBounds(haspT.gameObject);
                float haspY  = tableTopY - haspB.min.y + 0.005f;
                haspT.localPosition = new Vector3(tableCX - 0.20f, haspY, tableCZ);
                log.AppendLine("  Hasp -> pos=" + haspT.localPosition.ToString("F3")
                    + " rot=(-90,45,0) scale=0.0016");
            }
            else log.AppendLine("  [MISSING] Hasp");

            // 9. PADLOCK AND TAG
            Transform padlockT = FindDirectChild(masterRoot.transform, "PadlockAndTag");
            if (padlockT != null)
            {
                padlockT.localEulerAngles = new Vector3(0f, 30f, 0f);
                padlockT.localScale       = new Vector3(1.60f, 1.60f, 1.60f);
                Bounds pbBounds = GetBounds(padlockT.gameObject);
                float padY = tableTopY - pbBounds.min.y + 0.005f;
                padlockT.localPosition = new Vector3(tableCX + 0.20f, padY, tableCZ);
                log.AppendLine("  PadlockAndTag -> pos=" + padlockT.localPosition.ToString("F3")
                    + " rot=(0,30,0) scale=1.6");
            }
            else log.AppendLine("  [MISSING] PadlockAndTag");

            log.AppendLine();
        }

        // =================================================================
        //  PHASE 4 -- HIERARCHY CLEANUP
        // =================================================================
        private static void Phase4_HierarchyCleanup(GameObject masterRoot, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 4: HIERARCHY CLEANUP --");

            // Remove duplicate Hasp instances
            var hasps = new List<Transform>();
            foreach (Transform child in masterRoot.transform)
                if (string.Equals(child.name, "Hasp", StringComparison.OrdinalIgnoreCase))
                    hasps.Add(child);

            if (hasps.Count > 1)
            {
                for (int i = 1; i < hasps.Count; i++)
                {
                    log.AppendLine("  Removing duplicate Hasp: '" + hasps[i].name + "'");
                    GameObject.DestroyImmediate(hasps[i].gameObject);
                }
                log.AppendLine("  Kept 1 Hasp, removed " + (hasps.Count - 1) + " duplicate(s).");
            }
            else
            {
                log.AppendLine("  Hasp count: " + hasps.Count + " -- no duplicates.");
            }

            // Disable Floor_01 slab inside ConveyorAssembly
            Transform conveyorT = FindDirectChild(masterRoot.transform, "ConveyorAssembly");
            if (conveyorT != null)
            {
                Transform floorSlab = FindDeepChild(conveyorT, "Floor_01");
                if (floorSlab != null)
                {
                    floorSlab.gameObject.SetActive(false);
                    log.AppendLine("  Disabled Floor_01 slab inside ConveyorAssembly.");
                }
                else
                {
                    log.AppendLine("  Floor_01 not found inside ConveyorAssembly (may already be removed).");
                }
            }

            // Create LOTOStation parent node if not present
            Transform lotoStationT = FindDirectChild(masterRoot.transform, "LOTOStation");
            if (lotoStationT == null)
            {
                GameObject lotoGO = new GameObject("LOTOStation");
                lotoGO.transform.SetParent(masterRoot.transform, false);
                lotoGO.transform.localPosition    = Vector3.zero;
                lotoGO.transform.localEulerAngles = Vector3.zero;
                lotoGO.transform.localScale       = Vector3.one;
                lotoStationT = lotoGO.transform;
                log.AppendLine("  Created LOTOStation parent node.");
            }
            else
            {
                log.AppendLine("  LOTOStation already exists.");
            }

            // Reparent SteelWorkstationTable, Hasp, PadlockAndTag -> LOTOStation
            string[] lotoChildren = { "SteelWorkstationTable", "Hasp", "PadlockAndTag" };
            foreach (string childName in lotoChildren)
            {
                Transform childT = FindDirectChild(masterRoot.transform, childName);
                if (childT != null && childT.parent != lotoStationT)
                {
                    childT.SetParent(lotoStationT, true);
                    log.AppendLine("  Reparented " + childName + " -> LOTOStation.");
                }
            }

            // Remove temp/debug objects
            string[] tempPrefixes = { "Label_", "Temp", "DEBUG_", "MachineryModelInspection", "TestModel" };
            var toDestroy = new List<GameObject>();

            foreach (Transform child in masterRoot.transform)
                foreach (string prefix in tempPrefixes)
                    if (child.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    { toDestroy.Add(child.gameObject); break; }

            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (PRESERVE_ROOTS.Contains(go.name) || go.name == "MachineryScenario") continue;
                foreach (string prefix in tempPrefixes)
                    if (go.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    { toDestroy.Add(go); break; }
            }

            foreach (var go in toDestroy)
            {
                log.AppendLine("  Removed temp object: '" + go.name + "'");
                GameObject.DestroyImmediate(go);
            }
            if (toDestroy.Count == 0) log.AppendLine("  No temp/debug objects found.");
            log.AppendLine();
        }

        // =================================================================
        //  PHASE 5 -- MATERIAL VERIFICATION
        // =================================================================
        private static void Phase5_FixMaterials(GameObject masterRoot, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 5: MATERIAL VERIFICATION --");

            Shader stdShader = Shader.Find("Standard");
            if (stdShader == null) stdShader = Shader.Find("Universal Render Pipeline/Lit");
            if (stdShader == null)
            {
                log.AppendLine("  WARNING: Could not find Standard or URP shader -- skipping.");
                return;
            }

            MeshRenderer[] allRenderers = masterRoot.GetComponentsInChildren<MeshRenderer>(true);
            int fixedCount = 0;
            foreach (var r in allRenderers)
            {
                Material[] mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null
                        || mats[i].shader == null
                        || mats[i].shader.name.Contains("Error")
                        || mats[i].shader.name.StartsWith("Hidden/"))
                    {
                        mats[i] = new Material(stdShader);
                        mats[i].color = new Color(0.65f, 0.68f, 0.72f, 1f);
                        changed = true;
                        fixedCount++;
                    }
                }
                if (changed) r.sharedMaterials = mats;
            }

            log.AppendLine("  Fixed " + fixedCount + " null/error materials across "
                + allRenderers.Length + " renderers.");
            log.AppendLine();
        }

        // =================================================================
        //  PHASE 6 -- COAL ARRANGEMENT
        // =================================================================
        private static void Phase6_ArrangeCoal(GameObject masterRoot, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 6: COAL / JAMAREA ARRANGEMENT --");

            Transform jamAreaT = FindDirectChild(masterRoot.transform, "JamArea");
            if (jamAreaT == null)
            {
                log.AppendLine("  [MISSING] JamArea -- skipping coal arrangement.");
                return;
            }

            // Reset JamArea transform
            jamAreaT.localPosition    = Vector3.zero;
            jamAreaT.localEulerAngles = Vector3.zero;
            jamAreaT.localScale       = Vector3.one;

            int coalCount = jamAreaT.childCount;
            if (coalCount == 0) { log.AppendLine("  JamArea has no children."); return; }

            // Get belt top Y
            Transform conveyorT = FindDirectChild(masterRoot.transform, "ConveyorAssembly");
            float beltTopY = 1.06f; // safe default
            if (conveyorT != null)
            {
                Bounds cB = GetBounds(conveyorT.gameObject);
                if (cB.size.magnitude > 0.01f) beltTopY = cB.max.y;
            }

            float beltXMin  = -1.60f;
            float beltXMax  =  1.20f;
            float beltZHalf =  0.35f;

            System.Random rand = new System.Random(42); // deterministic seed

            for (int i = 0; i < coalCount; i++)
            {
                Transform coal = jamAreaT.GetChild(i);
                float progress = (float)i / (float)coalCount;

                float x, y, z;
                float rotX = (float)(rand.NextDouble() * 360.0);
                float rotY = (float)(rand.NextDouble() * 360.0);
                float rotZ = (float)(rand.NextDouble() * 360.0);
                float s    = 0.001f * (float)(0.8 + rand.NextDouble() * 0.5); // 5-13cm pieces

                if (progress < 0.30f)
                {
                    // Zone 1: Feed stream (sparse, near hopper)
                    float t = progress / 0.30f;
                    x = Mathf.Lerp(beltXMin, -0.50f, t);
                    z = Mathf.Lerp(-beltZHalf * 0.5f, beltZHalf * 0.5f, (float)rand.NextDouble());
                    y = beltTopY + (float)(rand.NextDouble() * 0.04);
                }
                else if (progress < 0.60f)
                {
                    // Zone 2: Flow stream (moderate, mid-belt)
                    float t = (progress - 0.30f) / 0.30f;
                    x = Mathf.Lerp(-0.50f, 0.30f, t);
                    z = Mathf.Lerp(-beltZHalf * 0.70f, beltZHalf * 0.70f, (float)rand.NextDouble());
                    y = beltTopY + (float)(rand.NextDouble() * 0.05);
                }
                else
                {
                    // Zone 3: Dense jam pile (accumulation at drive end)
                    float t = (progress - 0.60f) / 0.40f;
                    x = Mathf.Lerp(0.30f, beltXMax, t);
                    z = Mathf.Lerp(-beltZHalf * 0.90f, beltZHalf * 0.90f, (float)rand.NextDouble());
                    float pileHeight = Mathf.Sin(t * Mathf.PI) * 0.18f;
                    y = beltTopY + pileHeight + (float)(rand.NextDouble() * 0.06);
                }

                coal.localPosition    = new Vector3(x, y, z);
                coal.localEulerAngles = new Vector3(rotX, rotY, rotZ);
                coal.localScale       = new Vector3(s, s, s);
            }

            log.AppendLine("  Arranged " + coalCount + " coal pieces. Belt X: "
                + beltXMin + " to " + beltXMax + " | beltTopY=" + beltTopY.ToString("F3"));
            log.AppendLine();
        }

        // =================================================================
        //  PHASE 7 -- SAVE SCENE
        // =================================================================
        private static void Phase7_SaveScene(Scene scene, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 7: SAVE SCENE --");
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            log.AppendLine("  Scene saved: " + (saved ? "SUCCESS" : "FAILED") + " -> " + SCENE_PATH);
            log.AppendLine();
        }

        // =================================================================
        //  PHASE 8 -- FINAL VALIDATION REPORT
        // =================================================================
        private static void Phase8_ValidationReport(GameObject masterRoot, System.Text.StringBuilder log)
        {
            log.AppendLine("-- PHASE 8: FINAL VALIDATION REPORT --");

            bool allPass = true;

            string[] required = {
                "ConveyorAssembly", "FeedHopper", "DriveMotor", "JamArea",
                "ControlPanel", "EmergencyStop", "MainIsolator",
                "SteelWorkstationTable", "MotorBasePlate"
            };

            foreach (string name in required)
            {
                Transform t = FindDeepChild(masterRoot.transform, name);
                bool present = t != null;
                bool active  = present && t.gameObject.activeInHierarchy;
                bool hasR    = false;
                bool sized   = false;
                int  rCount  = 0;

                if (present)
                {
                    MeshRenderer[] mrs = t.GetComponentsInChildren<MeshRenderer>(true);
                    rCount = mrs.Length;
                    foreach (var r in mrs) if (r.enabled) { hasR = true; break; }
                    Bounds b = GetBounds(t.gameObject);
                    sized = b.size.magnitude > 0.01f;
                }

                bool pass = present && active && hasR && sized;
                allPass &= pass;
                log.AppendLine("  [" + (pass ? "PASS" : "FAIL") + "] " + name.PadRight(26)
                    + " Present:" + BoolStr(present)
                    + " Active:"  + BoolStr(active)
                    + " Renderer:" + BoolStr(hasR)
                    + " Size:"    + BoolStr(sized)
                    + " R:" + rCount);
            }

            // Coal count check
            Transform jamT = FindDeepChild(masterRoot.transform, "JamArea");
            int coalC = jamT != null ? jamT.childCount : 0;
            bool coalOk = coalC >= 100;
            allPass &= coalOk;
            log.AppendLine("  [" + (coalOk ? "PASS" : "FAIL") + "] JamArea coal count: "
                + coalC + " (need >= 100)");

            // LOTOStation check
            Transform lotoT = FindDeepChild(masterRoot.transform, "LOTOStation");
            bool lotoOk = lotoT != null;
            allPass &= lotoOk;
            log.AppendLine("  [" + (lotoOk ? "PASS" : "FAIL") + "] LOTOStation exists");

            if (lotoT != null)
            {
                bool haspOk = FindDeepChild(lotoT, "Hasp") != null;
                bool padOk  = FindDeepChild(lotoT, "PadlockAndTag") != null;
                log.AppendLine("  [" + (haspOk ? "PASS" : "FAIL") + "] Hasp inside LOTOStation");
                log.AppendLine("  [" + (padOk  ? "PASS" : "FAIL") + "] PadlockAndTag inside LOTOStation");
            }

            // Renderer total check
            MeshRenderer[] allR = masterRoot.GetComponentsInChildren<MeshRenderer>(true);
            int enAll = 0, disAll = 0;
            foreach (var r in allR)
            {
                if (r.enabled && r.gameObject.activeInHierarchy) enAll++;
                else disAll++;
            }
            bool rendersOk = disAll <= 5; // Only Floor_01 (and maybe tiny helpers) may remain disabled
            allPass &= rendersOk;
            log.AppendLine("  [" + (rendersOk ? "PASS" : "WARN") + "] MeshRenderers -- Enabled: "
                + enAll + " | Still-disabled: " + disAll);

            log.AppendLine();
            log.AppendLine("==================================================");
            log.AppendLine(allPass
                ? "  ALL CHECKS PASSED -- SCENE IS READY"
                : "  SOME CHECKS FAILED -- SEE DETAILS ABOVE");
            log.AppendLine("==================================================");
            log.AppendLine();
            log.AppendLine("IMPORTANT: Visual verification must be done in Unity Editor Scene View.");
            log.AppendLine("           Open MachineryConveyorJam.unity and compare layout");
            log.AppendLine("           against the reference Before/After industrial diagram.");

            Debug.Log(log.ToString());
        }

        // =================================================================
        //  HELPER UTILITIES
        // =================================================================

        /// <summary>Ground an object: shifts localPosition.Y so bounds.min.Y = 0.</summary>
        private static void GroundObject(GameObject go)
        {
            Bounds b = GetBounds(go);
            if (b.size.magnitude < 0.001f) return;
            Vector3 pos = go.transform.localPosition;
            pos.y -= b.min.y;
            go.transform.localPosition = pos;
        }

        /// <summary>Centers an object in X/Z: shifts localPosition so bounds.center.X/Z = 0.</summary>
        private static void CenterObjectXZ(GameObject go)
        {
            Bounds b = GetBounds(go);
            if (b.size.magnitude < 0.001f) return;
            Vector3 pos = go.transform.localPosition;
            pos.x -= b.center.x;
            pos.z -= b.center.z;
            go.transform.localPosition = pos;
        }

        /// <summary>Returns world-space combined bounds of all MeshRenderers in go and its children.</summary>
        private static Bounds GetBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0)
                return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        /// <summary>Finds an immediate child (depth=1) by name, case-insensitive.</summary>
        private static Transform FindDirectChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                    return child;
            return null;
        }

        /// <summary>Finds any descendant by name, depth-first, case-insensitive.</summary>
        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                    return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static string BoolStr(bool v) => v ? "YES" : "NO ";

        private static void WriteReport(string content)
        {
            string dir = Path.Combine(Application.dataPath, "..", REPORT_DIR);
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir,
                "fix_report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            File.WriteAllText(file, content);
            Debug.Log("[SurakshaAR] Report written to: " + file);
        }
    }
}
