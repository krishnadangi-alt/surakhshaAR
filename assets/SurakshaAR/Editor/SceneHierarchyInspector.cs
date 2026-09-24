using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class SceneHierarchyInspector
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\inspect_scene_trigger.txt";
        private const string DumpFile = @"C:\project\surakshaAR\Temp\scene_dump.txt";
        private const string DiagFile = @"C:\project\surakshaAR\Temp\ar_diagnostics.txt";

        static SceneHierarchyInspector()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                SanitizeScene();
                DumpScene();
                DiagnoseARObjects();
            }
        }

        public static void SanitizeScene()
        {
            try
            {
                const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid()) return;

                bool keptAdapter = false;
                bool keptCollector = false;
                bool keptAppState = false;

                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go == null) continue;
                    string n = go.name;

                    if (n.Contains("FireAssessmentAdapter"))
                    {
                        if (!keptAdapter) keptAdapter = true;
                        else UnityEngine.Object.DestroyImmediate(go);
                    }
                    else if (n.Contains("MovementTelemetryCollector"))
                    {
                        if (!keptCollector) keptCollector = true;
                        else UnityEngine.Object.DestroyImmediate(go);
                    }
                    else if (n.Contains("AppState"))
                    {
                        if (!keptAppState) keptAppState = true;
                        else UnityEngine.Object.DestroyImmediate(go);
                    }
                    else if (n.Contains("TempCaptureCamera") || n.Contains("FireScenarioUGUI") || n.Contains("TargetBox") || n.Contains("MainCanvas"))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                }

                GameObject fireScenario = GameObject.Find("FireScenario");
                if (fireScenario != null)
                {
                    for (int i = fireScenario.transform.childCount - 1; i >= 0; i--)
                    {
                        var child = fireScenario.transform.GetChild(i);
                        if (child.name.Contains("FireScenarioUGUI") || child.name.Contains("TargetBox") || child.name.Contains("Canvas"))
                        {
                            UnityEngine.Object.DestroyImmediate(child.gameObject);
                        }
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            catch (Exception ex)
            {
                Debug.LogError("[SanitizeScene Exception] " + ex);
            }
        }

        [MenuItem("SurakshaAR/Dump FireTraining Scene Hierarchy")]
        public static void DumpScene()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== FireTraining.unity HIERARCHY DUMP ===");
            try
            {
                const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    sb.AppendLine("Failed to open scene: " + scenePath);
                    File.WriteAllText(DumpFile, sb.ToString());
                    return;
                }

                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    DumpTransform(root.transform, 0, sb);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Exception: " + ex);
            }

            File.WriteAllText(DumpFile, sb.ToString());
            Debug.Log("[SceneHierarchyInspector] Scene dump written to " + DumpFile);
        }

        public static void DiagnoseARObjects()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== AR TRAINING FUNCTIONAL DIAGNOSTICS ===");

            try
            {
                const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // 1. Alarm Diagnostics
                sb.AppendLine("\n--- 1. FIRE ALARM ---");
                var alarms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
                foreach (var t in alarms)
                {
                    if (t.name.IndexOf("alarm", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sb.AppendLine($"Name: {t.name}, Parent: {(t.parent != null ? t.parent.name : "null")}, Active: {t.gameObject.activeSelf}, ActiveInHierarchy: {t.gameObject.activeInHierarchy}");
                        sb.AppendLine($"  LocalPos: {t.localPosition}, LocalRot: {t.localEulerAngles}, LocalScale: {t.localScale}");
                        sb.AppendLine($"  WorldPos: {t.position}, WorldRot: {t.eulerAngles}, LossyScale: {t.lossyScale}");
                        foreach (var c in t.GetComponents<Component>())
                        {
                            sb.AppendLine($"  Component: {c.GetType().FullName}");
                        }
                        var r = t.GetComponent<Renderer>();
                        if (r != null)
                        {
                            sb.AppendLine($"  Renderer: enabled={r.enabled}, bounds={r.bounds.size}, center={r.bounds.center}");
                            if (r.sharedMaterials != null)
                            {
                                foreach (var mat in r.sharedMaterials)
                                    sb.AppendLine($"    Material: {(mat != null ? mat.name : "null")} (shader={(mat != null ? mat.shader.name : "none")})");
                            }
                        }
                        var col = t.GetComponent<Collider>();
                        if (col != null) sb.AppendLine($"  Collider: {col.GetType().Name}, enabled={col.enabled}, bounds={col.bounds.size}");
                    }
                }

                // 2. Extinguisher & Pin Diagnostics
                sb.AppendLine("\n--- 2. EXTINGUISHERS & PINS ---");
                foreach (var t in alarms)
                {
                    if (t.name.IndexOf("FireExt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        t.name.IndexOf("Pin", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sb.AppendLine($"Name: {t.name}, Parent: {(t.parent != null ? t.parent.name : "null")}, Active: {t.gameObject.activeSelf}, ActiveInHierarchy: {t.gameObject.activeInHierarchy}");
                        sb.AppendLine($"  LocalPos: {t.localPosition}, LocalRot: {t.localEulerAngles}, LocalScale: {t.localScale}");
                        sb.AppendLine($"  WorldPos: {t.position}, LossyScale: {t.lossyScale}");
                        foreach (var c in t.GetComponents<Component>())
                        {
                            sb.AppendLine($"  Component: {c.GetType().FullName}");
                        }
                        var r = t.GetComponent<Renderer>();
                        if (r != null)
                        {
                            sb.AppendLine($"  Renderer: enabled={r.enabled}, bounds={r.bounds.size}, center={r.bounds.center}");
                            if (r.sharedMaterials != null)
                            {
                                foreach (var mat in r.sharedMaterials)
                                    sb.AppendLine($"    Material: {(mat != null ? mat.name : "null")} (shader={(mat != null ? mat.shader.name : "none")})");
                            }
                        }
                        var col = t.GetComponent<Collider>();
                        if (col != null) sb.AppendLine($"  Collider: {col.GetType().Name}, enabled={col.enabled}, bounds={col.bounds.size}, center={(col is BoxCollider bc ? bc.center.ToString() : "")}");
                    }
                }

                // 3. Spray Diagnostics
                sb.AppendLine("\n--- 3. SPRAY & PARTICLES ---");
                var psList = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include);
                foreach (var ps in psList)
                {
                    sb.AppendLine($"ParticleSystem: {ps.name}, Parent: {(ps.transform.parent != null ? ps.transform.parent.name : "null")}, Active: {ps.gameObject.activeInHierarchy}");
                    sb.AppendLine($"  LocalPos: {ps.transform.localPosition}, LocalRot: {ps.transform.localEulerAngles}, Forward: {ps.transform.forward}");
                    sb.AppendLine($"  WorldPos: {ps.transform.position}, LossyScale: {ps.transform.lossyScale}");
                    var main = ps.main;
                    sb.AppendLine($"  Main: duration={main.duration}, loop={main.loop}, startSpeed={main.startSpeed.constant}, startSize={main.startSize.constant}, simSpace={main.simulationSpace}, playOnAwake={main.playOnAwake}");
                    var emission = ps.emission;
                    sb.AppendLine($"  Emission: enabled={emission.enabled}, rateOverTime={emission.rateOverTime.constant}");
                    var shape = ps.shape;
                    sb.AppendLine($"  Shape: enabled={shape.enabled}, shapeType={shape.shapeType}, angle={shape.angle}, radius={shape.radius}");
                    var col = ps.collision;
                    sb.AppendLine($"  Collision: enabled={col.enabled}, type={col.type}, mode={col.mode}, sendCollisionMessages={col.sendCollisionMessages}, quality={col.quality}");
                    var psRend = ps.GetComponent<ParticleSystemRenderer>();
                    if (psRend != null)
                    {
                        sb.AppendLine($"  Renderer: enabled={psRend.enabled}, mode={psRend.renderMode}, material={(psRend.sharedMaterial != null ? psRend.sharedMaterial.name : "null")}");
                    }
                }

                // 4. Fire Target Diagnostics
                sb.AppendLine("\n--- 4. FIRE TARGET (FLAMES / HAZARD) ---");
                var fireComp = UnityEngine.Object.FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
                if (fireComp != null)
                {
                    sb.AppendLine($"FireExtinguishable: {fireComp.name}, Pos: {fireComp.transform.position}, FireWorldPos: {fireComp.FireWorldPosition}");
                }
                var boxGO = GameObject.Find("Electric Box");
                if (boxGO != null)
                {
                    sb.AppendLine($"Electric Box: Pos={boxGO.transform.position}, Bounds of colliders:");
                    foreach (var c in boxGO.GetComponentsInChildren<Collider>(true))
                    {
                        sb.AppendLine($"  Collider on {c.name}: {c.GetType().Name}, bounds={c.bounds.size}, center={c.bounds.center}");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Exception: " + ex);
            }

            File.WriteAllText(DiagFile, sb.ToString());
            Debug.Log("[SceneHierarchyInspector] AR Diagnostics written to " + DiagFile);
        }

        private static void DumpTransform(Transform t, int indent, StringBuilder sb)
        {
            string pad = new string(' ', indent * 2);
            string activeStr = t.gameObject.activeSelf ? "[ACTIVE]" : "[INACTIVE]";
            string compList = "";
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c is Transform) continue;
                compList += c.GetType().Name + ", ";
            }
            if (compList.Length > 2) compList = " (" + compList.Substring(0, compList.Length - 2) + ")";

            var rend = t.GetComponent<Renderer>();
            string rendInfo = "";
            if (rend != null)
            {
                rendInfo = $" [Renderer: enabled={rend.enabled}, bounds={rend.bounds.size}]";
            }

            var col = t.GetComponent<Collider>();
            string colInfo = "";
            if (col != null)
            {
                colInfo = $" [Collider: {col.GetType().Name}, enabled={col.enabled}, isTrigger={col.isTrigger}]";
            }

            sb.AppendLine($"{pad}- {t.name} {activeStr} pos={t.localPosition} rot={t.localEulerAngles} scale={t.localScale}{compList}{rendInfo}{colInfo}");

            for (int i = 0; i < t.childCount; i++)
            {
                DumpTransform(t.GetChild(i), indent + 1, sb);
            }
        }
    }
}
