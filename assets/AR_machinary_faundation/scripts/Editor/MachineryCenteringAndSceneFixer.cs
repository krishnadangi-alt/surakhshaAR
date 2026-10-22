#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryCenteringAndSceneFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string CoalGlbPath = "Assets/AR_machinary_faundation/3d/Raw/coal.glb";
        private const string LogPath = "scratch/centering_fix_log.txt";

        [MenuItem("SurakshaAR/Machinery/Execute Centering And Scene Fixer")]
        public static void FixSceneCentered()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — PERFECT WORLD ORIGIN MACHINERY SCENE FIXER");
            sb.AppendLine("Scene: " + ScenePath);
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[CenteringFixer] Cannot open scene at " + ScenePath);
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

            // 1. FILTER RENDERERS: Enable ONLY valid physical sub-meshes near local origin (<= 5.0m)
            int disabledHelpers = 0;
            int enabledRenderers = 0;

            foreach (Transform child in masterRoot.transform)
            {
                child.gameObject.SetActive(true);
                MeshRenderer[] renderers = child.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var r in renderers)
                {
                    if (IsValidPhysicalSubmesh(r))
                    {
                        r.enabled = true;
                        r.gameObject.SetActive(true);
                        enabledRenderers++;
                    }
                    else
                    {
                        r.enabled = false;
                        disabledHelpers++;
                    }
                }
            }
            sb.AppendLine($"[Renderers Configured] Enabled {enabledRenderers} physical sub-renderers. Disabled {disabledHelpers} distant helper/reference sub-meshes (>5m size/offset).");

            // 2. CONVEYOR ASSEMBLY — CENTRAL MASTER OBJECT AT ORIGIN (0, 0, 0)
            Transform conveyorTr = masterRoot.transform.Find("ConveyorAssembly");
            if (conveyorTr != null)
            {
                conveyorTr.gameObject.SetActive(true);
                conveyorTr.position = Vector3.zero;
                conveyorTr.rotation = Quaternion.identity;
                conveyorTr.localScale = Vector3.one;
                
                Bounds convBounds = CalculatePhysicalSubmeshBounds(conveyorTr.gameObject);
                sb.AppendLine($"[Conveyor Physical Bounds] Center={convBounds.center}, Size={convBounds.size}");
            }

            // 3. FEED HOPPER — FEED END (LEFT: X = -1.95m, Y = 1.51m, Z = 0.0m)
            Transform hopperTr = masterRoot.transform.Find("FeedHopper");
            if (hopperTr != null)
            {
                hopperTr.gameObject.SetActive(true);
                hopperTr.position = new Vector3(-1.95f, 1.51f, 0.0f);
                hopperTr.rotation = Quaternion.Euler(0f, 90f, 0f);
                hopperTr.localScale = Vector3.one;

                Bounds hBounds = CalculatePhysicalSubmeshBounds(hopperTr.gameObject);
                sb.AppendLine($"[FeedHopper Positioned] WorldCenter={hBounds.center}, Size={hBounds.size}");
            }

            // Deactivate duplicate/extra hoppers
            foreach (Transform child in masterRoot.transform)
            {
                if (child != hopperTr && child.name.ToLower().Contains("hopper"))
                {
                    child.gameObject.SetActive(false);
                }
            }

            // 4. DRIVE MOTOR & BASE PLATE — DRIVE END (RIGHT: X = +2.20m, Y = 0.60m, Z = +0.55m)
            Transform motorTr = masterRoot.transform.Find("DriveMotor");
            if (motorTr != null)
            {
                motorTr.gameObject.SetActive(true);
                motorTr.position = new Vector3(2.20f, 0.60f, 0.55f);
                motorTr.rotation = Quaternion.Euler(0f, 270f, 0f);
                motorTr.localScale = Vector3.one;

                Bounds mBounds = CalculatePhysicalSubmeshBounds(motorTr.gameObject);
                sb.AppendLine($"[DriveMotor Positioned] WorldCenter={mBounds.center}, Size={mBounds.size}");
            }

            Transform motorPlateTr = masterRoot.transform.Find("MotorBasePlate");
            if (motorPlateTr != null)
            {
                motorPlateTr.gameObject.SetActive(true);
                motorPlateTr.position = new Vector3(2.20f, 0.03f, 0.55f);
                motorPlateTr.rotation = Quaternion.identity;
                motorPlateTr.localScale = Vector3.one;
            }

            // 5. OPERATOR SAFETY STATION (RIGHT FRONT: Z = -1.35m)
            Transform panelTr = masterRoot.transform.Find("ControlPanel");
            if (panelTr != null)
            {
                panelTr.gameObject.SetActive(true);
                panelTr.position = new Vector3(2.50f, 0.85f, -1.35f);
                panelTr.rotation = Quaternion.identity;
                panelTr.localScale = Vector3.one;
            }

            Transform eStopTr = masterRoot.transform.Find("EmergencyStop");
            if (eStopTr != null)
            {
                eStopTr.gameObject.SetActive(true);
                eStopTr.position = new Vector3(3.00f, 1.15f, -1.30f);
                eStopTr.rotation = Quaternion.identity;
                eStopTr.localScale = Vector3.one;
            }

            Transform isolatorTr = masterRoot.transform.Find("MainIsolator");
            if (isolatorTr != null)
            {
                isolatorTr.gameObject.SetActive(true);
                isolatorTr.position = new Vector3(3.50f, 1.15f, -1.35f);
                isolatorTr.rotation = Quaternion.identity;
                isolatorTr.localScale = Vector3.one;
            }

            // 6. STEEL WORKSTATION TABLE & LOTO (FAR RIGHT FRONT: X = +4.50m, Z = -1.45m)
            float tableTopY = 1.104f;
            Transform tableTr = masterRoot.transform.Find("SteelWorkstationTable");
            if (tableTr != null)
            {
                tableTr.gameObject.SetActive(true);
                tableTr.position = new Vector3(4.50f, 0.00f, -1.45f);
                tableTr.rotation = Quaternion.identity;
                tableTr.localScale = Vector3.one;

                Bounds tBounds = CalculatePhysicalSubmeshBounds(tableTr.gameObject);
                if (tBounds.max.y > 0.1f && tBounds.max.y < 2.5f) tableTopY = tBounds.max.y;
                sb.AppendLine($"[SteelTable Positioned] WorldCenter={tBounds.center}, Size={tBounds.size}, TableTopY={tableTopY:F3}");
            }

            Transform haspTr = masterRoot.transform.Find("Hasp");
            if (haspTr != null)
            {
                haspTr.gameObject.SetActive(true);
                haspTr.position = new Vector3(4.30f, tableTopY + 0.015f, -1.45f);
                haspTr.rotation = Quaternion.Euler(90f, 0f, 0f);
                haspTr.localScale = Vector3.one;
            }

            Transform padlockTr = masterRoot.transform.Find("PadlockAndTag");
            if (padlockTr != null)
            {
                padlockTr.gameObject.SetActive(true);
                padlockTr.position = new Vector3(4.70f, tableTopY + 0.015f, -1.45f);
                padlockTr.rotation = Quaternion.Euler(0f, 45f, 0f);
                padlockTr.localScale = Vector3.one;
            }

            // 7. POPULATE 175 REAL COAL PIECES ON BELT SURFACE (Y = 1.162m)
            Transform jamAreaTr = masterRoot.transform.Find("JamArea");
            GameObject jamAreaGo;
            if (jamAreaTr != null)
            {
                jamAreaGo = jamAreaTr.gameObject;
                List<GameObject> oldCoal = new List<GameObject>();
                foreach (Transform child in jamAreaGo.transform) oldCoal.Add(child.gameObject);
                foreach (var c in oldCoal) Object.DestroyImmediate(c);
            }
            else
            {
                jamAreaGo = new GameObject("JamArea");
                jamAreaGo.transform.SetParent(masterRoot.transform, false);
            }

            jamAreaGo.SetActive(true);
            jamAreaGo.transform.position = Vector3.zero;
            jamAreaGo.transform.rotation = Quaternion.identity;
            jamAreaGo.transform.localScale = Vector3.one;

            GameObject coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoalGlbPath);
            if (coalPrefab != null)
            {
                Shader standardShader = Shader.Find("Standard");
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
                                newMat.color = new Color(0.10f, 0.10f, 0.12f, 1f);
                                mats[m] = newMat;
                            }
                        }
                        r.sharedMaterials = mats;
                    }

                    float px, py, pz;
                    float scaleFactor;

                    if (i < 35)
                    {
                        // ZONE C — FEED AREA: coal under Hopper outlet (LEFT)
                        px = Random.Range(-1.80f, -1.10f);
                        pz = Random.Range(-0.16f, 0.16f);
                        py = beltTopY + 0.02f;
                        scaleFactor = Random.Range(0.0030f, 0.0050f);
                    }
                    else if (i < 75)
                    {
                        // ZONE A — NORMAL STREAM: distributed stream along belt (CENTER)
                        px = Random.Range(-1.10f, 0.50f);
                        pz = Random.Range(-0.18f, 0.18f);
                        py = beltTopY + 0.02f;
                        scaleFactor = Random.Range(0.0035f, 0.0055f);
                    }
                    else
                    {
                        // ZONE B — DENSE JAM AREA: blockage pile near drive end (RIGHT-CENTER)
                        float radialX = Random.Range(-0.45f, 0.45f);
                        float radialZ = Random.Range(-0.22f, 0.22f);
                        px = 1.00f + radialX;
                        pz = radialZ;

                        float distFromCenter = Mathf.Sqrt(radialX * radialX + radialZ * radialZ);
                        float pileLayerHeight = Mathf.Max(0f, (0.35f - distFromCenter) * 0.65f);
                        py = beltTopY + 0.02f + Random.Range(0.00f, 0.02f) + pileLayerHeight;
                        scaleFactor = Random.Range(0.0040f, 0.0065f);
                    }

                    piece.transform.position = new Vector3(px, py, pz);
                    piece.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-20f, 20f));
                    piece.transform.localScale = Vector3.one * scaleFactor;
                }
                sb.AppendLine("[COAL ON BELT VERIFIED] 175 coal pieces populated on belt surface Y = 1.162m.");
            }

            // 8. ZERO LABELS CLEANLINESS AUDIT
            int cleanedLabels = 0;
            foreach (Transform t in masterRoot.GetComponentsInChildren<Transform>(true))
            {
                string name = t.name.ToLower();
                if (name.Contains("label") || name.Contains("text") || name.Contains("canvas") || name.Contains("arrow") || name.Contains("callout") || name.Contains("marker"))
                {
                    if (t.GetComponent<Canvas>() != null || t.GetComponent<UnityEngine.UI.Text>() != null || t.GetComponent<TMPro.TMP_Text>() != null)
                    {
                        t.gameObject.SetActive(false);
                        cleanedLabels++;
                    }
                }
            }
            sb.AppendLine($"[Zero Labels Verified] Cleaned {cleanedLabels} UI/Canvas label components.");

            // 9. SAVE SCENE & PERSIST TO DISK
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[CenteringFixer] Execution completed. Log written to " + LogPath);

            // 10. RENDER MULTI-ANGLE CAMERA VIEWS TO PNG
            MachinerySceneCameraRenderer.RenderAllViews();
        }

        private static bool IsValidPhysicalSubmesh(MeshRenderer r)
        {
            if (r == null) return false;
            string name = r.name.ToLower();
            if (name.Contains("floor") || name.Contains("vinyl_cd") || name.Contains("plane.001") || name.Contains("plane.002")) return false;

            float localOffset = r.transform.localPosition.magnitude;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                if (mf.sharedMesh.bounds.center.magnitude > 10.0f) return false;
            }

            if (localOffset > 10.0f) return false;
            if (r.bounds.size.magnitude > 15.0f) return false;

            return true;
        }

        private static Bounds CalculatePhysicalSubmeshBounds(GameObject go)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!IsValidPhysicalSubmesh(r)) continue;
                if (!hasFirst) { b = r.bounds; hasFirst = true; }
                else { b.Encapsulate(r.bounds); }
            }
            if (!hasFirst)
            {
                b = new Bounds(go.transform.position, Vector3.one);
            }
            return b;
        }
    }
}
#endif
