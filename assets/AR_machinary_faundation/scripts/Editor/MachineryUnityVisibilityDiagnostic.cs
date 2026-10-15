#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryUnityVisibilityDiagnostic
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/visibility_diagnostic_log.txt";

        [MenuItem("SurakshaAR/Machinery/Diagnose and Fix Unity Scene Visibility Only")]
        public static void DiagnoseAndFixVisibilityOnly()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — UNITY EDITOR SCENE VISIBILITY DIAGNOSTIC & LIGHTING FIX");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("CRITICAL ERROR: Could not open scene at " + ScenePath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            // 1. Inspect RenderSettings & Environment Lighting
            sb.AppendLine("--- 1. RENDER SETTINGS & ENVIRONMENT LIGHTING ---");
            sb.AppendLine($"  Ambient Mode: {RenderSettings.ambientMode}");
            sb.AppendLine($"  Ambient Light Color: {RenderSettings.ambientSkyColor}");
            sb.AppendLine($"  Ambient Intensity: {RenderSettings.ambientIntensity}");
            sb.AppendLine($"  Skybox Material: {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "NULL")}");

            // Ensure clear, crisp environment lighting in Scene View
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = new Color(0.60f, 0.65f, 0.70f, 1.0f);
            RenderSettings.ambientEquatorColor = new Color(0.40f, 0.45f, 0.50f, 1.0f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.22f, 0.25f, 1.0f);
            RenderSettings.ambientIntensity = 1.0f;
            sb.AppendLine("  [FIXED] Configured bright flat ambient lighting (0.60, 0.65, 0.70).\n");

            // 2. Inspect Directional Lights
            sb.AppendLine("--- 2. DIRECTIONAL LIGHTS ---");
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            sb.AppendLine($"  Total Lights in Scene: {lights.Length}");
            Light dirLight = null;
            foreach (var l in lights)
            {
                sb.AppendLine($"  Light '{l.name}': Type={l.type}, Enabled={l.enabled}, Intensity={l.intensity}, ActiveInHierarchy={l.gameObject.activeInHierarchy}, Rot={l.transform.eulerAngles}");
                if (l.type == LightType.Directional && l.enabled && l.gameObject.activeInHierarchy)
                {
                    dirLight = l;
                }
            }

            if (dirLight == null)
            {
                GameObject lightGo = new GameObject("DirectionalLight");
                dirLight = lightGo.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.intensity = 1.25f;
                dirLight.color = new Color(1.0f, 0.98f, 0.95f);
                dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                sb.AppendLine("  [FIXED] Created crisp DirectionalLight at rot (50, -30, 0), intensity 1.25.\n");
            }
            else
            {
                dirLight.intensity = Mathf.Max(dirLight.intensity, 1.25f);
                dirLight.color = Color.white;
                sb.AppendLine("  [VERIFIED] Directional light intensity ensured at >= 1.25.\n");
            }

            // 3. Inspect MachineryScenario Root & Children
            sb.AppendLine("--- 3. MACHINERY SCENARIO HIERARCHY & MODEL VISIBILITY ---");
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                sb.AppendLine("CRITICAL ERROR: MachineryScenario root not found in scene!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            masterRoot.SetActive(true);
            sb.AppendLine($"  MachineryScenario: ActiveSelf={masterRoot.activeSelf}, ActiveInHierarchy={masterRoot.activeInHierarchy}, Layer={LayerMask.LayerToName(masterRoot.layer)}");

            int totalNodesChecked = 0;
            int totalRenderersEnabled = 0;
            int materialsFixed = 0;

            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null) standardShader = Shader.Find("Mobile/Diffuse");

            foreach (Transform child in masterRoot.transform)
            {
                totalNodesChecked++;
                child.gameObject.SetActive(true);
                child.gameObject.layer = 0; // Default layer

                MeshRenderer[] renderers = child.GetComponentsInChildren<MeshRenderer>(true);
                int activeRenderersInChild = 0;

                foreach (var r in renderers)
                {
                    // Ensure GameObject and Renderer are active and on Default layer
                    r.gameObject.SetActive(true);
                    r.gameObject.layer = 0;

                    // Skip disabled room walls in FeedHopper/Conveyor
                    if (!r.enabled)
                    {
                        string rPath = r.name.ToLower();
                        if (rPath.Contains("floor") || rPath.Contains("vinyl") || rPath.Contains("object_781") || rPath.Contains("object_112"))
                        {
                            continue;
                        }
                    }

                    r.enabled = true;
                    activeRenderersInChild++;
                    totalRenderersEnabled++;

                    // Verify materials & shaders
                    Material[] mats = r.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error") || mats[m].shader.name.Contains("InternalError"))
                        {
                            Material fixedMat = new Material(standardShader);
                            fixedMat.name = child.name + "_Mat_" + m;
                            fixedMat.color = new Color(0.35f, 0.45f, 0.65f, 1.0f);
                            mats[m] = fixedMat;
                            materialsFixed++;
                        }
                    }
                    r.sharedMaterials = mats;
                }

                Bounds b = CalculateBounds(child.gameObject);
                sb.AppendLine($"  Node '{child.name}': Active={child.gameObject.activeSelf}, ActiveRenderers={activeRenderersInChild}/{renderers.Length}, BoundsCenter={b.center}, Size={b.size}");
            }

            sb.AppendLine($"\nTotal Nodes Checked: {totalNodesChecked}");
            sb.AppendLine($"Total Renderers Active: {totalRenderersEnabled}");
            sb.AppendLine($"Materials Fixed: {materialsFixed}\n");

            // 4. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[MachineryUnityVisibilityDiagnostic] Diagnostic & Visibility Fix Complete! Log written to " + LogPath);

            // 5. Render updated camera views
            MachinerySceneCameraRenderer.RenderAllViews();
        }

        private static Bounds CalculateBounds(GameObject go)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!hasFirst) { b = r.bounds; hasFirst = true; }
                else { b.Encapsulate(r.bounds); }
            }
            if (!hasFirst) b = new Bounds(go.transform.position, Vector3.one);
            return b;
        }
    }
}
#endif
