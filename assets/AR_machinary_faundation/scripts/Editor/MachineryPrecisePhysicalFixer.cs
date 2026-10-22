#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPrecisePhysicalFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string CoalGlbPath = "Assets/AR_machinary_faundation/3d/Raw/coal.glb";

        [MenuItem("SurakshaAR/Machinery/Execute Precise Physical Model Alignment & Bounds Separation")]
        public static void ExecutePrecisePhysicalAssembly()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPrecisePhysicalFixer] STARTING EXACT MESH-BOUNDS PHYSICAL ASSEMBLY...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Fixer] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();

            // 1. CLEANUP UNWANTED BACKGROUND / DECLARED CLUTTER
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (var root in rootObjects)
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    string tname = t.name;
                    if (tname.Contains("Shutter") || tname.Contains("Warehouse") || tname.Contains("Room") ||
                        tname.Contains("Board") || tname.Contains("Pole") || tname.Contains("Wall") ||
                        tname.Contains("Grid") || tname.Contains("Temp") || tname.Contains("Duplicate") ||
                        tname.Contains("Inspection") || tname.Contains("Helper") || tname.Contains("Frame_BG") ||
                        tname.Contains("Leg_Single") || tname.Contains("Leg_Double") || tname.Contains("Leg_Front") ||
                        tname.Contains("Leg_Back") || tname == "Floor_01" || tname == "Lever_Sign")
                    {
                        toDestroy.Add(t.gameObject);
                    }
                }
            }
            foreach (var obj in toDestroy)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }

            // Master scenario root
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null) masterRoot = new GameObject("MachineryScenario");

            // Locate all primary required transforms
            Transform conveyor = masterRoot.transform.Find("ConveyorAssembly");
            Transform hopper = masterRoot.transform.Find("FeedHopper");
            Transform motor = masterRoot.transform.Find("DriveMotor");
            Transform controlPanel = masterRoot.transform.Find("ControlPanel");
            Transform eStop = masterRoot.transform.Find("EmergencyStop");
            Transform isolator = masterRoot.transform.Find("MainIsolator");
            Transform table = masterRoot.transform.Find("SteelWorkstationTable");
            Transform hasp = masterRoot.transform.Find("Hasp");
            Transform padlock = masterRoot.transform.Find("PadlockAndTag");

            if (conveyor == null)
            {
                Debug.LogError("[Fixer] ConveyorAssembly node missing!");
                return;
            }

            // -------------------------------------------------------------------------
            // A. CONVEYOR = MASTER REFERENCE
            // -------------------------------------------------------------------------
            conveyor.localPosition = Vector3.zero;
            conveyor.localEulerAngles = Vector3.zero;
            conveyor.localScale = Vector3.one;

            Bounds cBounds = CalculateMeshBounds(conveyor.gameObject);
            float beltTopY = cBounds.max.y > 0.5f ? cBounds.max.y : 1.162f;
            float beltCenterX = cBounds.center.x;
            float beltCenterZ = cBounds.center.z;
            float beltMinX = cBounds.min.x;
            float beltMaxX = cBounds.max.x;
            float beltWidthZ = cBounds.size.z > 0.1f ? cBounds.size.z : 0.60f;

            float feedEndX = beltMaxX - 0.20f;
            float driveEndX = beltMinX + 0.15f;
            float jamAreaX = beltMinX + 0.60f;

            Debug.Log($"[1. Conveyor Master] Bounds Center={cBounds.center}, Size={cBounds.size}, BeltTopY={beltTopY:F3}");

            // -------------------------------------------------------------------------
            // B. FEED HOPPER — PHYSICAL SPOUT ALIGNMENT AT FEED END
            // -------------------------------------------------------------------------
            if (hopper != null)
            {
                hopper.gameObject.SetActive(true);
                hopper.localScale = Vector3.one;

                // Zero out any internal mesh offsets on child transforms
                foreach (Transform child in hopper.GetComponentsInChildren<Transform>(true))
                {
                    if (child != hopper) child.localPosition = Vector3.zero;
                }

                float hopperTargetX = feedEndX + 0.10f;
                float hopperTargetZ = beltCenterZ;
                float hopperTargetY = 0.35f;

                hopper.position = new Vector3(hopperTargetX, hopperTargetY, hopperTargetZ);
                hopper.localEulerAngles = new Vector3(0, -90, 0);

                Bounds hBounds = CalculateMeshBounds(hopper.gameObject);
                Debug.Log($"[2. Feed Hopper] Placed at Pos={hopper.position}, Mesh Bounds Center={hBounds.center}, Size={hBounds.size}");
            }

            // -------------------------------------------------------------------------
            // C. DRIVE MOTOR — MECHANICALLY COUPLED AT DRIVE END
            // -------------------------------------------------------------------------
            if (motor != null)
            {
                motor.gameObject.SetActive(true);
                motor.localScale = Vector3.one;
                motor.localEulerAngles = new Vector3(0, 90, 0);

                // Position Motor beside drive shaft outside conveyor body bounds
                float motorTargetX = driveEndX - 0.35f;
                float motorTargetZ = beltCenterZ + 0.95f; // Separated cleanly from conveyor frame
                float motorTargetY = 0.0f; // Grounded base

                motor.position = new Vector3(motorTargetX, motorTargetY, motorTargetZ);
                Bounds mBounds = CalculateMeshBounds(motor.gameObject);
                Debug.Log($"[3. Drive Motor] Placed at Pos={motor.position}, Mesh Bounds Center={mBounds.center}, Size={mBounds.size}");
            }

            // -------------------------------------------------------------------------
            // D. SAFETY CONTROL STATION (Control Panel, E-Stop, Main Isolator)
            // -------------------------------------------------------------------------
            // Positioned alongside conveyor facing worker pathway at Z = -1.45m with clear X spacing
            float stationZ = beltCenterZ - 1.45f;

            if (controlPanel != null)
            {
                controlPanel.gameObject.SetActive(true);
                controlPanel.localEulerAngles = Vector3.zero; // Facing +Z worker
                controlPanel.position = new Vector3(beltCenterX - 0.85f, 0.85f, stationZ);
            }

            if (eStop != null)
            {
                eStop.gameObject.SetActive(true);
                eStop.localEulerAngles = Vector3.zero;
                eStop.position = new Vector3(beltCenterX, 1.15f, stationZ + 0.05f);
            }

            if (isolator != null)
            {
                isolator.gameObject.SetActive(true);
                isolator.localEulerAngles = Vector3.zero;
                isolator.position = new Vector3(beltCenterX + 0.85f, 1.15f, stationZ);
            }

            Debug.Log($"[4. Safety Station] ControlPanel, E-Stop, Isolator aligned along operator path Z={stationZ:F3}");

            // -------------------------------------------------------------------------
            // E. LOTO WORKSTATION & EQUIPMENT ON TABLE SURFACE
            // -------------------------------------------------------------------------
            float tableX = beltCenterX + 1.85f; // Positioned cleanly past MainIsolator (+0.85m)
            float tableZ = stationZ;

            if (table != null)
            {
                table.gameObject.SetActive(true);
                table.localEulerAngles = Vector3.zero;
                table.position = new Vector3(tableX, 0.0f, tableZ); // Grounded legs

                Bounds tBounds = CalculateMeshBounds(table.gameObject);
                float tableTopY = tBounds.max.y > 0.1f ? tBounds.max.y : 0.75f;

                // Place Hasp and PadlockAndTag DIRECTLY ON TOP OF TABLE SURFACE
                if (hasp != null)
                {
                    hasp.gameObject.SetActive(true);
                    hasp.position = new Vector3(tableX - 0.18f, tableTopY + 0.015f, tableZ);
                    hasp.localEulerAngles = new Vector3(90, 0, 0); // Flat on table surface
                }

                if (padlock != null)
                {
                    padlock.gameObject.SetActive(true);
                    padlock.position = new Vector3(tableX + 0.18f, tableTopY + 0.015f, tableZ);
                    padlock.localEulerAngles = new Vector3(0, 45, 0); // Flat on table surface
                }

                Debug.Log($"[5. LOTO Table] Table grounded at Y=0, TopY={tableTopY:F3}. Hasp & Padlock placed flat on top surface!");
            }

            // -------------------------------------------------------------------------
            // F. CONTINUOUS COAL STREAM (HOPPER -> BELT -> JAM)
            // -------------------------------------------------------------------------
            Transform jamTransform = masterRoot.transform.Find("JamArea");
            GameObject jamArea;
            if (jamTransform != null)
            {
                jamArea = jamTransform.gameObject;
                List<GameObject> oldChildren = new List<GameObject>();
                foreach (Transform child in jamArea.transform) oldChildren.Add(child.gameObject);
                foreach (var child in oldChildren) Object.DestroyImmediate(child);
            }
            else
            {
                jamArea = new GameObject("JamArea");
                jamArea.transform.SetParent(masterRoot.transform, false);
            }

            jamArea.transform.localPosition = Vector3.zero;
            jamArea.transform.localEulerAngles = Vector3.zero;
            jamArea.transform.localScale = Vector3.one;

            GameObject coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoalGlbPath);
            if (coalPrefab != null)
            {
                Shader standardShader = Shader.Find("Standard");
                Random.InitState(2026);

                int totalCoalPieces = 175;
                float channelWidthZ = beltWidthZ * 0.45f;

                for (int i = 0; i < totalCoalPieces; i++)
                {
                    GameObject piece = Object.Instantiate(coalPrefab, jamArea.transform);
                    piece.name = $"Coal_{i + 1:D3}";
                    piece.SetActive(true);

                    MeshRenderer[] renderers = piece.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var r in renderers)
                    {
                        r.enabled = true;
                        Material[] mats = r.sharedMaterials;
                        for (int m = 0; m < mats.Length; m++)
                        {
                            if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error"))
                            {
                                Material newMat = new Material(standardShader);
                                newMat.name = "Coal_Material";
                                newMat.color = new Color(0.12f, 0.12f, 0.14f, 1f);
                                mats[m] = newMat;
                            }
                        }
                        r.sharedMaterials = mats;
                    }

                    float px, pz, py;
                    if (i < 80)
                    {
                        // JAM AREA (Dense accumulation)
                        px = jamAreaX + Random.Range(-0.35f, 0.35f);
                        pz = beltCenterZ + Random.Range(-channelWidthZ, channelWidthZ);
                        float dist = Vector2.Distance(new Vector2(px, pz), new Vector2(jamAreaX, beltCenterZ));
                        float pileHeight = Mathf.Max(0f, 0.18f - dist * 0.45f);
                        py = beltTopY + Random.Range(0.005f, 0.025f) + pileHeight;
                    }
                    else if (i < 145)
                    {
                        // BELT STREAM (Normal flow)
                        px = Mathf.Lerp(jamAreaX + 0.35f, feedEndX - 0.25f, Random.Range(0f, 1f));
                        pz = beltCenterZ + Random.Range(-channelWidthZ, channelWidthZ);
                        py = beltTopY + Random.Range(0.005f, 0.020f);
                    }
                    else
                    {
                        // INCOMING FROM HOPPER SPOUT
                        px = feedEndX + Random.Range(-0.15f, 0.20f);
                        pz = beltCenterZ + Random.Range(-channelWidthZ * 0.8f, channelWidthZ * 0.8f);
                        py = beltTopY + Random.Range(0.005f, 0.035f);
                    }

                    float baseScale = Random.Range(0.0035f, 0.0060f);
                    Vector3 scaleVar = new Vector3(
                        baseScale * Random.Range(0.85f, 1.15f),
                        baseScale * Random.Range(0.80f, 1.10f),
                        baseScale * Random.Range(0.85f, 1.15f)
                    );

                    Vector3 rotVar = new Vector3(
                        Random.Range(-30f, 30f),
                        Random.Range(0f, 360f),
                        Random.Range(-30f, 30f)
                    );

                    piece.transform.position = new Vector3(px, py, pz);
                    piece.transform.localEulerAngles = rotVar;
                    piece.transform.localScale = scaleVar;
                }

                Debug.Log($"[6. Coal Flow] Created {totalCoalPieces} continuous coal pieces on top of belt surface!");
            }

            // -------------------------------------------------------------------------
            // G. EXPLICIT AUTOMATED BOUNDS OVERLAP CHECK
            // -------------------------------------------------------------------------
            Debug.Log("--- PERFORMING AUTOMATED BOUNDS OVERLAP CHECK ---");
            Dictionary<string, Bounds> majorBounds = new Dictionary<string, Bounds>();
            if (conveyor != null) majorBounds["ConveyorAssembly"] = CalculateMeshBounds(conveyor.gameObject);
            if (hopper != null) majorBounds["FeedHopper"] = CalculateMeshBounds(hopper.gameObject);
            if (motor != null) majorBounds["DriveMotor"] = CalculateMeshBounds(motor.gameObject);
            if (controlPanel != null) majorBounds["ControlPanel"] = CalculateMeshBounds(controlPanel.gameObject);
            if (eStop != null) majorBounds["EmergencyStop"] = CalculateMeshBounds(eStop.gameObject);
            if (isolator != null) majorBounds["MainIsolator"] = CalculateMeshBounds(isolator.gameObject);
            if (table != null) majorBounds["SteelWorkstationTable"] = CalculateMeshBounds(table.gameObject);

            int overlapCount = 0;
            List<string> majorKeys = new List<string>(majorBounds.Keys);
            for (int i = 0; i < majorKeys.Count; i++)
            {
                for (int j = i + 1; j < majorKeys.Count; j++)
                {
                    string k1 = majorKeys[i];
                    string k2 = majorKeys[j];

                    // Allow intentional spout alignment between Hopper and Conveyor feed end
                    if ((k1 == "FeedHopper" && k2 == "ConveyorAssembly") || (k2 == "FeedHopper" && k1 == "ConveyorAssembly"))
                        continue;

                    if (majorBounds[k1].Intersects(majorBounds[k2]))
                    {
                        Debug.LogWarning($"[Overlap Warning] {k1} overlaps with {k2}!");
                        overlapCount++;
                    }
                    else
                    {
                        Debug.Log($"[Overlap Pass] {k1} vs {k2}: SEPARATED cleanly.");
                    }
                }
            }

            Debug.Log($"[7. Overlap Check Result] Major body overlaps found = {overlapCount}");

            // -------------------------------------------------------------------------
            // H. SAVE SCENE & PERSISTENCE RELOAD CHECK
            // -------------------------------------------------------------------------
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            var reloadScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            bool sceneValid = reloadScene.IsValid();
            Debug.Log($"[8. Persistence Check] Scene saved and reloaded successfully. Scene Valid = {sceneValid}");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPrecisePhysicalFixer] PHYSICAL MODEL ALIGNMENT & BOUNDS FIX COMPLETE");
            Debug.Log("=========================================================================");
        }

        private static Bounds CalculateMeshBounds(GameObject go)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                if (!hasFirst) { b = r.bounds; hasFirst = true; }
                else { b.Encapsulate(r.bounds); }
            }
            return b;
        }
    }
}
#endif
