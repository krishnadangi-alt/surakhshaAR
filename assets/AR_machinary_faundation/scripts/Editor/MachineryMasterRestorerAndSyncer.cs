#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryMasterRestorerAndSyncer
    {
        private const string MainScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string Raw3DFolder = "Assets/AR_machinary_faundation/3d/Raw";
        private const string PrefabFolder = "Assets/AR_machinary_faundation/prefabs";
        private const string LogPath = "scratch/master_restoration_and_sync_log.txt";

        private static readonly string[] TargetScenes = new string[]
        {
            "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity",
            "Assets/AR_machinary_faundation/scenes/AR_machinery_Foundation.unity",
            "Assets/AR_machinary_faundation/scenes/MachineryARCameraDiagnostic.unity",
            "Assets/SurakshaAR/Scenes/Main.unity"
        };

        [MenuItem("SurakshaAR/Machinery/Execute Master Restoration and Scene Sync")]
        public static void ExecuteMasterRestorationAndSync()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — MASTER SCENE RESTORATION, PREFAB EXPORT & ALL SCENE SYNC");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            // 1. Open Primary Target Scene
            var primaryScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            if (!primaryScene.IsValid())
            {
                sb.AppendLine("CRITICAL ERROR: Failed to open primary scene " + MainScenePath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
            }
            masterRoot.SetActive(true);
            masterRoot.transform.position = Vector3.zero;
            masterRoot.transform.rotation = Quaternion.identity;
            masterRoot.transform.localScale = Vector3.one;

            // Clear old children under MachineryScenario
            List<GameObject> existingChildren = new List<GameObject>();
            foreach (Transform child in masterRoot.transform)
            {
                existingChildren.Add(child.gameObject);
            }
            foreach (var child in existingChildren)
            {
                Object.DestroyImmediate(child);
            }
            sb.AppendLine("[Cleanup] Cleared all old nodes under MachineryScenario.");

            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null) standardShader = Shader.Find("Mobile/Diffuse");

            // 2. Restore ConveyorAssembly (Master Central Model)
            string convGlbPath = Raw3DFolder + "/industrial_conveyor_belt.glb";
            GameObject convPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(convGlbPath);
            if (convPrefab == null)
            {
                sb.AppendLine("CRITICAL ERROR: Conveyor GLB missing at " + convGlbPath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject conveyorGo = (GameObject)PrefabUtility.InstantiatePrefab(convPrefab, masterRoot.transform);
            conveyorGo.name = "ConveyorAssembly";
            PrefabUtility.UnpackPrefabInstance(conveyorGo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            conveyorGo.SetActive(true);
            conveyorGo.transform.localPosition = Vector3.zero;
            conveyorGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 90f);
            conveyorGo.transform.localScale = new Vector3(0.0125f, 0.0125f, 0.0125f);

            MeshRenderer[] convRenderers = conveyorGo.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in convRenderers)
            {
                r.gameObject.SetActive(true);
                string path = r.transform.name.ToLower();
                Transform p = r.transform.parent;
                while (p != null && p != conveyorGo.transform)
                {
                    path = p.name.ToLower() + "/" + path;
                    p = p.parent;
                }

                if (path.Contains("floor") || path.Contains("vinyl"))
                {
                    r.enabled = false;
                    r.gameObject.SetActive(false);
                    continue;
                }
                r.enabled = true;

                Material[] mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error"))
                    {
                        Material newMat = new Material(standardShader);
                        newMat.name = "Conveyor_Mat_" + m;
                        newMat.color = new Color(0.25f, 0.40f, 0.65f, 1.0f);
                        mats[m] = newMat;
                    }
                }
                r.sharedMaterials = mats;
            }

            Bounds convB = CalculateBounds(conveyorGo);
            Vector3 convOffset = new Vector3(-convB.center.x, -convB.min.y, -convB.center.z);
            conveyorGo.transform.position += convOffset;
            Bounds finalConvB = CalculateBounds(conveyorGo);
            sb.AppendLine($"[ConveyorAssembly Restored] Bounds Center={finalConvB.center}, Size={finalConvB.size}");

            // 3. Restore Other Required Models
            var models = new (string nodeName, string glbPath, Vector3 targetPos, Vector3 targetRot, Vector3 targetScale)[]
            {
                ("FeedHopper", "hopper.glb", new Vector3(-1.95f, 1.25f, 0.0f), new Vector3(0f, 90f, 0f), new Vector3(0.00065f, 0.00065f, 0.00065f)),
                ("DriveMotor", "electric_motor.glb", new Vector3(2.20f, 0.45f, 0.55f), new Vector3(0f, 270f, 0f), new Vector3(0.40f, 0.40f, 0.40f)),
                ("MotorBasePlate", null, new Vector3(2.20f, 0.03f, 0.55f), new Vector3(0f, 0f, 0f), new Vector3(0.75f, 0.05f, 0.60f)),
                ("ControlPanel", "industrial_control_panel_box.glb", new Vector3(2.50f, 0.85f, -0.90f), new Vector3(0f, 0f, 0f), new Vector3(0.0015f, 0.0015f, 0.0015f)),
                ("EmergencyStop", "emergency_stop_button.glb", new Vector3(2.95f, 0.85f, -0.90f), new Vector3(0f, 0f, 0f), new Vector3(0.0015f, 0.0015f, 0.0015f)),
                ("MainIsolator", "switch.glb", new Vector3(3.35f, 0.85f, -0.90f), new Vector3(0f, 0f, 0f), new Vector3(0.25f, 0.25f, 0.25f)),
                ("SteelWorkstationTable", "industrial_steel_table.glb", new Vector3(4.50f, 0.00f, -1.45f), new Vector3(-90f, 0f, 0f), new Vector3(0.010f, 0.010f, 0.010f)),
                ("Hasp", "hasp_and_staple.glb", new Vector3(4.30f, 0.81f, -1.45f), new Vector3(90f, 0f, 0f), new Vector3(0.0015f, 0.0015f, 0.0015f)),
                ("PadlockAndTag", "lockout_tagout_locks_with_key_and_tag.glb", new Vector3(4.70f, 0.81f, -1.45f), new Vector3(0f, 45f, 0f), new Vector3(1.50f, 1.50f, 1.50f)),
            };

            foreach (var item in models)
            {
                GameObject nodeGo = null;
                if (!string.IsNullOrEmpty(item.glbPath))
                {
                    string fullGlbPath = Raw3DFolder + "/" + item.glbPath;
                    GameObject rawPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullGlbPath);
                    if (rawPrefab != null)
                    {
                        nodeGo = (GameObject)PrefabUtility.InstantiatePrefab(rawPrefab, masterRoot.transform);
                        nodeGo.name = item.nodeName;
                        PrefabUtility.UnpackPrefabInstance(nodeGo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                    else
                    {
                        sb.AppendLine($"ERROR: Could not load GLB at {fullGlbPath}");
                        continue;
                    }
                }
                else
                {
                    nodeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    nodeGo.name = item.nodeName;
                    nodeGo.transform.SetParent(masterRoot.transform, false);
                }

                if (nodeGo == null) continue;

                nodeGo.SetActive(true);
                nodeGo.transform.localPosition = item.targetPos;
                nodeGo.transform.localRotation = Quaternion.Euler(item.targetRot);
                nodeGo.transform.localScale = item.targetScale;

                MeshRenderer[] renderers = nodeGo.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var r in renderers)
                {
                    r.gameObject.SetActive(true);
                    r.enabled = true;

                    if (item.nodeName == "FeedHopper")
                    {
                        string rName = r.gameObject.name.ToLower();
                        Transform pTr = r.transform.parent;
                        string path = rName;
                        while (pTr != null && pTr != nodeGo.transform)
                        {
                            path = pTr.name.ToLower() + "/" + path;
                            pTr = pTr.parent;
                        }

                        float mag = r.bounds.size.magnitude;
                        Vector3 c = r.bounds.center;

                        if (mag > 100f || Mathf.Abs(c.x) > 500f || Mathf.Abs(c.y) > 500f || Mathf.Abs(c.z) > 500f || path.Contains("object_781") || path.Contains("object_112") || path.Contains("building") || path.Contains("room") || path.Contains("wall") || path.Contains("floor"))
                        {
                            r.enabled = false;
                            r.gameObject.SetActive(false);
                            continue;
                        }
                    }

                    Material[] mats = r.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error"))
                        {
                            Material newMat = new Material(standardShader);
                            newMat.name = item.nodeName + "_Mat_" + m;
                            newMat.color = new Color(0.25f, 0.40f, 0.65f, 1.0f);
                            mats[m] = newMat;
                        }
                    }
                    r.sharedMaterials = mats;
                }

                Bounds rawB = CalculateBounds(nodeGo);
                if (rawB.size.magnitude > 0.001f)
                {
                    Vector3 physicalOffset = rawB.center - nodeGo.transform.position;
                    nodeGo.transform.position = item.targetPos - physicalOffset;
                }

                Bounds finalB = CalculateBounds(nodeGo);
                sb.AppendLine($"  Node '{item.nodeName}': Active={nodeGo.activeSelf}, Pos={nodeGo.transform.position}, BoundsCenter={finalB.center}, Size={finalB.size}");
            }

            // 4. Populate 175 Coal Pieces Flush on Conveyor Belt Surface
            GameObject jamAreaGo = new GameObject("JamArea");
            jamAreaGo.transform.SetParent(masterRoot.transform, false);
            jamAreaGo.SetActive(true);
            jamAreaGo.transform.position = Vector3.zero;
            jamAreaGo.transform.rotation = Quaternion.identity;
            jamAreaGo.transform.localScale = Vector3.one;

            GameObject coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Raw3DFolder + "/coal.glb");
            if (coalPrefab != null)
            {
                Random.InitState(12345);
                float beltTopY = 1.162f;

                for (int i = 0; i < 175; i++)
                {
                    GameObject piece = Object.Instantiate(coalPrefab, jamAreaGo.transform);
                    piece.name = $"Coal_{i + 1:D3}";
                    piece.SetActive(true);

                    foreach (var r in piece.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        r.enabled = true;
                        r.gameObject.SetActive(true);
                        Material[] mats = r.sharedMaterials;
                        for (int m = 0; m < mats.Length; m++)
                        {
                            if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error"))
                            {
                                Material newMat = new Material(standardShader);
                                newMat.name = "Coal_Mat";
                                newMat.color = new Color(0.12f, 0.12f, 0.14f, 1f);
                                mats[m] = newMat;
                            }
                        }
                        r.sharedMaterials = mats;
                    }

                    float px, py, pz;
                    float scaleFactor;

                    if (i < 35)
                    {
                        px = Random.Range(-1.80f, -1.10f);
                        pz = Random.Range(-0.16f, 0.16f);
                        py = beltTopY + 0.015f;
                        scaleFactor = Random.Range(0.0020f, 0.0028f);
                    }
                    else if (i < 75)
                    {
                        px = Random.Range(-1.10f, 0.50f);
                        pz = Random.Range(-0.18f, 0.18f);
                        py = beltTopY + 0.015f;
                        scaleFactor = Random.Range(0.0022f, 0.0030f);
                    }
                    else
                    {
                        float radialX = Random.Range(-0.40f, 0.40f);
                        float radialZ = Random.Range(-0.20f, 0.20f);
                        px = 1.00f + radialX;
                        pz = radialZ;

                        float distFromCenter = Mathf.Sqrt(radialX * radialX + radialZ * radialZ);
                        float pileLayerHeight = Mathf.Max(0f, (0.30f - distFromCenter) * 0.45f);
                        py = beltTopY + 0.015f + pileLayerHeight;
                        scaleFactor = Random.Range(0.0025f, 0.0035f);
                    }

                    piece.transform.position = new Vector3(px, py, pz);
                    piece.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-20f, 20f));
                    piece.transform.localScale = Vector3.one * scaleFactor;
                }
                sb.AppendLine("[Coal Populated] 175 coal pieces populated flush on conveyor belt surface.");
            }

            // Zero Labels: disable any floating canvas / text objects
            int cleanedLabels = 0;
            foreach (Transform t in masterRoot.GetComponentsInChildren<Transform>(true))
            {
                string name = t.name.ToLower();
                if (name.Contains("label") || name.Contains("text") || name.Contains("canvas") || name.Contains("arrow") || name.Contains("callout"))
                {
                    if (t.GetComponent<Canvas>() != null || t.GetComponent<UnityEngine.UI.Text>() != null || t.GetComponent<TMPro.TMP_Text>() != null)
                    {
                        t.gameObject.SetActive(false);
                        cleanedLabels++;
                    }
                }
            }
            sb.AppendLine($"[Zero Labels] Cleaned {cleanedLabels} label components.");

            // 5. Save Primary Scene
            EditorSceneManager.MarkSceneDirty(primaryScene);
            EditorSceneManager.SaveScene(primaryScene);
            sb.AppendLine($"[Primary Scene Saved] {MainScenePath}");

            // 6. Save Clean Prefabs to Assets/AR_machinary_faundation/prefabs/
            Directory.CreateDirectory(PrefabFolder);
            string masterPrefabPath = PrefabFolder + "/MachineryScenario.prefab";
            GameObject masterScenarioPrefab = PrefabUtility.SaveAsPrefabAsset(masterRoot, masterPrefabPath);
            sb.AppendLine($"[Master Prefab Created] {masterPrefabPath}");

            foreach (Transform child in masterRoot.transform)
            {
                string prefabPath = PrefabFolder + "/" + child.name + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(child.gameObject, prefabPath);
                sb.AppendLine($"[Child Prefab Created/Updated] {prefabPath}");
            }

            // 7. Synchronize All Other Machinery Scenes in Project
            foreach (var scenePath in TargetScenes)
            {
                if (scenePath == MainScenePath) continue;
                if (!File.Exists(scenePath)) continue;

                var targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!targetScene.IsValid()) continue;

                GameObject existingRoot = GameObject.Find("MachineryScenario");
                if (existingRoot != null)
                {
                    Object.DestroyImmediate(existingRoot);
                }

                // Instantiate master scenario prefab into target scene fresh
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(masterPrefabPath);
                if (prefabAsset != null)
                {
                    GameObject masterCopy = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                    masterCopy.name = "MachineryScenario";
                    masterCopy.SetActive(true);
                    PrefabUtility.UnpackPrefabInstance(masterCopy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    sb.AppendLine($"[Synced Scene Saved] {scenePath}");
                }

                EditorSceneManager.MarkSceneDirty(targetScene);
                EditorSceneManager.SaveScene(targetScene);
            }

            // Re-open primary scene at the end
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[MasterRestorerAndSyncer] Master Restoration & Sync COMPLETE! Written to " + LogPath);

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
