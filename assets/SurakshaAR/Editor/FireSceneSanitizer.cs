using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using SurakshaAR.Core;
using SurakshaAR.Data;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class FireSceneSanitizer
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\sanitize_fire_scene_trigger.txt";
        private const string ReportFile = @"C:\project\surakshaAR\Temp\sanitize_fire_scene_report.txt";

        static FireSceneSanitizer()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                SanitizeAndAlignFireScene();
            }
        }

        private static GameObject FindInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindInChildren(root.transform, name);
                if (found != null) return found.gameObject;
            }
            return null;
        }

        private static Transform FindInChildren(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindInChildren(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        [MenuItem("SurakshaAR/Sanitize and Repair FireTraining Scene")]
        public static void SanitizeAndAlignFireScene()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SANITIZING FIRETRAINING.UNITY SCENE ===");

            try
            {
                const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    sb.AppendLine("ERROR: Scene could not be opened: " + scenePath);
                    File.WriteAllText(ReportFile, sb.ToString());
                    return;
                }

                // 1. Remove duplicate singletons leaked into scene root
                int removedAdapters = 0;
                int removedCollectors = 0;
                int removedAppStates = 0;
                int removedCanvases = 0;

                bool keptOneAdapter = false;
                bool keptOneCollector = false;
                bool keptOneAppState = false;

                var roots = scene.GetRootGameObjects();
                foreach (var go in roots)
                {
                    string n = go.name;

                    if (n.Contains("FireAssessmentAdapter"))
                    {
                        if (!keptOneAdapter)
                        {
                            keptOneAdapter = true;
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(go);
                            removedAdapters++;
                        }
                    }
                    else if (n.Contains("MovementTelemetryCollector"))
                    {
                        if (!keptOneCollector)
                        {
                            keptOneCollector = true;
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(go);
                            removedCollectors++;
                        }
                    }
                    else if (n.Contains("AppState"))
                    {
                        if (!keptOneAppState)
                        {
                            keptOneAppState = true;
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(go);
                            removedAppStates++;
                        }
                    }
                    else if (n.Contains("TempCaptureCamera"))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                    else if (n.Contains("FireScenarioUGUI") || n.Contains("MainCanvas") || n.Contains("TargetBox"))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                        removedCanvases++;
                    }
                }

                sb.AppendLine($"[CLEANUP] Removed {removedAdapters} duplicate FireAssessmentAdapters");
                sb.AppendLine($"[CLEANUP] Removed {removedCollectors} duplicate MovementTelemetryCollectors");
                sb.AppendLine($"[CLEANUP] Removed {removedAppStates} duplicate AppStates");
                sb.AppendLine($"[CLEANUP] Removed {removedCanvases} stray UI GameObjects from root");

                // 2. Search within FireScenario for stray baked UI
                GameObject fireScenario = FindInScene(scene, "FireScenario");
                if (fireScenario != null)
                {
                    for (int i = fireScenario.transform.childCount - 1; i >= 0; i--)
                    {
                        var child = fireScenario.transform.GetChild(i);
                        if (child.name.Contains("FireScenarioUGUI") || child.name.Contains("TargetBox") || child.name.Contains("Canvas"))
                        {
                            UnityEngine.Object.DestroyImmediate(child.gameObject);
                            sb.AppendLine($"[CLEANUP] Removed child UI from FireScenario: {child.name}");
                        }
                    }
                }

                // 3. Locate and wire essential scenario targets
                // A. Fire Hazard & Particle System
                var fireVFX = FindInScene(scene, "VFX_Fire_01_Small");
                if (fireVFX != null)
                {
                    fireVFX.SetActive(true);

                    // Configure all child particle systems to be active & emit
                    foreach (var ps in fireVFX.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        if (ps == null) continue;
                        ps.gameObject.SetActive(true);
                        var main = ps.main;
                        main.playOnAwake = true;
                        var em = ps.emission;
                        em.enabled = true;
                        var rend = ps.GetComponent<ParticleSystemRenderer>();
                        if (rend != null && ps.name != "VFX_Fire_01_Small") rend.enabled = true;
                    }

                    var fCol = fireVFX.GetComponent<BoxCollider>();
                    if (fCol == null) fCol = fireVFX.AddComponent<BoxCollider>();
                    fCol.center = new Vector3(0f, 0.30f, 0f);
                    fCol.size = new Vector3(1.2f, 1.0f, 1.2f);
                    fCol.isTrigger = false;
                    fCol.enabled = true;

                    var fExt = fireVFX.GetComponent<FireExtinguishable>();
                    if (fExt == null) fExt = fireVFX.AddComponent<FireExtinguishable>();
                    fExt.enabled = true;

                    sb.AppendLine($"[TARGET:FIRE] VFX_Fire_01_Small activated with BoxCollider center={fCol.center}, size={fCol.size}");
                }

                // B. Fire Alarm & Collider
                var fireAlarm = FindInScene(scene, "FireAlarm");
                if (fireAlarm != null)
                {
                    fireAlarm.SetActive(true);
                    var aCol = fireAlarm.GetComponent<BoxCollider>();
                    if (aCol == null) aCol = fireAlarm.AddComponent<BoxCollider>();
                    aCol.center = new Vector3(0f, 0f, 0f);
                    aCol.size = new Vector3(0.5f, 0.5f, 0.5f);
                    aCol.isTrigger = false;
                    aCol.enabled = true;

                    var aInter = fireAlarm.GetComponent<AlarmInteraction>();
                    if (aInter == null) aInter = fireAlarm.AddComponent<AlarmInteraction>();
                    aInter.enabled = true;

                    sb.AppendLine($"[TARGET:ALARM] FireAlarm configured at pos={fireAlarm.transform.position}");
                }

                // C. FireExt_display (Floor prop for step 3)
                var displayExt = FindInScene(scene, "FireExt_display");
                if (displayExt != null)
                {
                    displayExt.SetActive(true);
                    var dCol = displayExt.GetComponent<BoxCollider>();
                    if (dCol == null) dCol = displayExt.AddComponent<BoxCollider>();
                    dCol.center = new Vector3(0f, 0.25f, 0f);
                    dCol.size = new Vector3(0.35f, 0.65f, 0.35f);
                    dCol.isTrigger = false;
                    dCol.enabled = true;

                    var dPickup = displayExt.GetComponent<ExtinguisherDisplayPickup>();
                    if (dPickup == null) dPickup = displayExt.AddComponent<ExtinguisherDisplayPickup>();
                    dPickup.enabled = true;

                    // Display Pin
                    var dPin = displayExt.transform.Find("Pin");
                    if (dPin != null)
                    {
                        dPin.gameObject.SetActive(true);
                        var dpCol = dPin.GetComponent<BoxCollider>();
                        if (dpCol == null) dpCol = dPin.gameObject.AddComponent<BoxCollider>();
                        dpCol.center = new Vector3(0f, 0.01f, 0.45f);
                        dpCol.size = new Vector3(0.18f, 0.18f, 0.18f);
                        dpCol.enabled = true;
                    }

                    sb.AppendLine($"[TARGET:DISPLAY_EXT] FireExt_display active on floor, BoxCollider={dCol.size}");
                }

                // D. FireExt (Held operational extinguisher)
                var originalExt = FindInScene(scene, "FireExt");
                if (originalExt != null)
                {
                    originalExt.SetActive(true);

                    // Remove orphaned FireExtinguishable child under FireExt if present
                    var orphanExt = originalExt.transform.Find("FireExtinguishable");
                    if (orphanExt != null)
                    {
                        UnityEngine.Object.DestroyImmediate(orphanExt.gameObject);
                        sb.AppendLine("[CLEANUP] Removed orphaned FireExt/FireExtinguishable child.");
                    }

                    var oPickup = originalExt.GetComponent<ExtinguisherPickup>();
                    if (oPickup == null) oPickup = originalExt.AddComponent<ExtinguisherPickup>();
                    oPickup.enabled = true;

                    if (displayExt != null)
                    {
                        var dPickup = displayExt.GetComponent<ExtinguisherDisplayPickup>();
                        if (dPickup != null)
                        {
                            dPickup.originalExtinguisher = originalExt;
                            if (Camera.main != null) dPickup.arCamera = Camera.main.transform;
                        }
                    }

                    // Pin
                    var pin = originalExt.transform.Find("Pin");
                    if (pin != null)
                    {
                        pin.gameObject.SetActive(true);

                        var pinComp = pin.GetComponent<FirePinInteraction>();
                        if (pinComp == null) pinComp = pin.gameObject.AddComponent<FirePinInteraction>();
                        pinComp.enabled = true;

                        var pCol = pin.GetComponent<BoxCollider>();
                        if (pCol == null) pCol = pin.gameObject.AddComponent<BoxCollider>();
                        pCol.center = new Vector3(0f, 0.01f, 0.45f);
                        pCol.size = new Vector3(0.18f, 0.18f, 0.18f);
                        pCol.isTrigger = false;
                        pCol.enabled = true;

                        var pRend = pin.GetComponent<MeshRenderer>();
                        if (pRend != null) pRend.enabled = true;

                        sb.AppendLine($"[TARGET:PIN] FirePinInteraction configured on {pin.name}, BoxCollider center={pCol.center}, size={pCol.size}");
                    }

                    // Grip
                    var grip = originalExt.transform.Find("Grip");
                    if (grip != null)
                    {
                        grip.gameObject.SetActive(true);

                        var gripComp = grip.GetComponent<ExtinguisherGripInteraction>();
                        if (gripComp == null) gripComp = grip.gameObject.AddComponent<ExtinguisherGripInteraction>();
                        gripComp.enabled = true;

                        var gCol = grip.GetComponent<BoxCollider>();
                        if (gCol == null) gCol = grip.gameObject.AddComponent<BoxCollider>();
                        gCol.center = new Vector3(0f, 0.01f, 0.46f);
                        gCol.size = new Vector3(0.20f, 0.18f, 0.20f);
                        gCol.isTrigger = false;
                        gCol.enabled = true;

                        var gRend = grip.GetComponent<MeshRenderer>();
                        if (gRend != null) gRend.enabled = true;

                        sb.AppendLine($"[TARGET:GRIP] ExtinguisherGripInteraction configured on {grip.name}, BoxCollider center={gCol.center}, size={gCol.size}");
                    }

                    // Hose / Spray / Body children
                    for (int i = 0; i < originalExt.transform.childCount; i++)
                    {
                        var child = originalExt.transform.GetChild(i);
                        child.gameObject.SetActive(true);
                    }
                }

                // 4. Update FlowManager references
                var flow = UnityEngine.Object.FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
                if (flow != null)
                {
                    flow.fireScenario = fireScenario;
                    if (displayExt != null) flow.displayPickup = displayExt.GetComponent<ExtinguisherDisplayPickup>();
                    if (originalExt != null) flow.originalPickup = originalExt.GetComponent<ExtinguisherPickup>();
                    if (fireVFX != null) flow.fire = fireVFX.GetComponent<FireExtinguishable>();
                    if (fireAlarm != null) flow.alarmInteraction = fireAlarm.GetComponent<AlarmInteraction>();
                    if (originalExt != null)
                    {
                        flow.pinInteraction = originalExt.GetComponentInChildren<FirePinInteraction>(true);
                        flow.gripInteraction = originalExt.GetComponentInChildren<ExtinguisherGripInteraction>(true);
                    }
                    EditorUtility.SetDirty(flow);
                    sb.AppendLine("[FLOW] FlowManager references verified and saved.");
                }

                // Save scene
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                sb.AppendLine("[SCENE] FireTraining.unity successfully sanitized and saved!");

                File.WriteAllText(ReportFile, sb.ToString());
                Debug.Log(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[EXCEPTION] {ex}");
                File.WriteAllText(ReportFile, sb.ToString());
                Debug.LogError($"[FireSceneSanitizer EXCEPTION] {ex}");
            }
        }
    }
}
