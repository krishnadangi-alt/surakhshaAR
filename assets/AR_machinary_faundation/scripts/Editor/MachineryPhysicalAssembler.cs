#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPhysicalAssembler
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";
        private const string CoalGlbPath = "Assets/AR_machinary_faundation/3d/Raw/coal.glb";

        [MenuItem("SurakshaAR/Machinery/Assemble Coherent Physical Machinery Station")]
        public static void AssembleCoherentStation()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPhysicalAssembler] STARTING PHYSICAL MACHINERY ASSEMBLY PASS...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[PhysicalAssembler] Failed to open scene at {ScenePath}");
                return;
            }

            if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(ArtifactFolder)) Directory.CreateDirectory(ArtifactFolder);

            GameObject[] rootObjects = scene.GetRootGameObjects();

            // -------------------------------------------------------------------------
            // A. REMOVE ALL UNWANTED GEOMETRY ACROSS THE SCENE
            // -------------------------------------------------------------------------
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (var root in rootObjects)
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    string tname = t.name;

                    // Identify poles, legs, boards, signs, shutter doors, frames, background walls
                    if (tname.Contains("Pole") || tname.Contains("Board") || tname.Contains("Sign") ||
                        tname.Contains("Shutter") || tname.Contains("Post") || tname.Contains("Wall") ||
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
                if (obj != null)
                {
                    Debug.Log($"[Cleanup] Destroying unwanted geometry: {obj.name}");
                    Object.DestroyImmediate(obj);
                }
            }

            // Find or create MachineryScenario root
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
            }

            // Locate required primary nodes under MachineryScenario
            Transform conveyor = masterRoot.transform.Find("ConveyorAssembly");
            Transform hopper = masterRoot.transform.Find("FeedHopper");
            Transform motor = masterRoot.transform.Find("DriveMotor");
            Transform controlPanel = masterRoot.transform.Find("ControlPanel");
            Transform eStop = masterRoot.transform.Find("EmergencyStop");
            Transform isolator = masterRoot.transform.Find("MainIsolator");
            Transform table = masterRoot.transform.Find("SteelWorkstationTable");
            Transform hasp = masterRoot.transform.Find("Hasp");
            Transform padlock = masterRoot.transform.Find("PadlockAndTag");

            if (conveyor == null) { Debug.LogError("Missing ConveyorAssembly"); return; }

            // -------------------------------------------------------------------------
            // B. MASTER CONVEYOR BOUNDS & BASE POSITIONING
            // -------------------------------------------------------------------------
            conveyor.localPosition = Vector3.zero;
            conveyor.localEulerAngles = Vector3.zero;
            conveyor.localScale = Vector3.one;

            Bounds cBounds = GetWorldBounds(conveyor.gameObject);
            float beltTopY = cBounds.max.y;
            float beltCenterX = cBounds.center.x;
            float beltCenterZ = cBounds.center.z;
            float beltMinX = cBounds.min.x;
            float beltMaxX = cBounds.max.x;
            float beltWidthZ = cBounds.size.z;

            Debug.Log($"[1. Master Conveyor] Bounds Center={cBounds.center}, Size={cBounds.size}");
            Debug.Log($"   X Range=[{beltMinX:F3}, {beltMaxX:F3}], Belt Surface TopY={beltTopY:F3}");

            float feedEndX = beltMaxX - 0.20f;
            float jamAreaX = beltMinX + 0.60f;
            float driveEndX = beltMinX + 0.15f;

            // -------------------------------------------------------------------------
            // C. FEED HOPPER — PHYSICAL ALIGNMENT AT FEED END
            // -------------------------------------------------------------------------
            if (hopper != null)
            {
                hopper.gameObject.SetActive(true);

                // Position FeedHopper so its funnel spout sits directly over the Conveyor Feed End
                float targetHopperX = feedEndX + 0.10f;
                float targetHopperZ = beltCenterZ;

                hopper.position = new Vector3(targetHopperX, 0.45f, targetHopperZ);
                hopper.localEulerAngles = new Vector3(0, -90, 0);

                Debug.Log($"[2. Feed Hopper] Physically aligned at Feed End spout: Pos={hopper.position}");
            }

            // -------------------------------------------------------------------------
            // D. DRIVE MOTOR — MECHANICAL COUPLING AT DRIVE END
            // -------------------------------------------------------------------------
            if (motor != null)
            {
                motor.gameObject.SetActive(true);
                Bounds mBounds = GetWorldBounds(motor.gameObject);

                float motorOffsetY = -mBounds.min.y; // Ground motor base to Y=0
                float targetMotorX = driveEndX - 0.25f;
                float targetMotorZ = beltCenterZ + 0.55f; // Mounted on drive shaft side

                motor.position = new Vector3(targetMotorX, motorOffsetY, targetMotorZ);
                motor.localEulerAngles = new Vector3(0, 90, 0);

                Debug.Log($"[3. Drive Motor] Mechanically coupled at Drive End: Pos={motor.position}");
            }

            // -------------------------------------------------------------------------
            // E. SAFETY & CONTROL STATION (Control Panel, E-Stop, Main Isolator)
            // -------------------------------------------------------------------------
            float stationZ = beltCenterZ - 1.35f;
            float stationCenterX = beltCenterX;

            if (controlPanel != null)
            {
                controlPanel.gameObject.SetActive(true);
                Bounds cpBounds = GetWorldBounds(controlPanel.gameObject);
                float cpOffsetY = -cpBounds.min.y;

                controlPanel.position = new Vector3(stationCenterX - 0.35f, cpOffsetY + 0.85f, stationZ);
                controlPanel.localEulerAngles = new Vector3(0, 0, 0); // Facing worker
            }

            if (eStop != null)
            {
                eStop.gameObject.SetActive(true);
                eStop.position = new Vector3(stationCenterX, 1.15f, stationZ + 0.05f);
                eStop.localEulerAngles = new Vector3(0, 0, 0);
            }

            if (isolator != null)
            {
                isolator.gameObject.SetActive(true);
                isolator.position = new Vector3(stationCenterX + 0.35f, 1.15f, stationZ);
                isolator.localEulerAngles = new Vector3(0, 0, 0);
            }

            Debug.Log($"[4. Safety/Control Station] Assembled at Z={stationZ:F3}");

            // -------------------------------------------------------------------------
            // F. LOTO WORKSTATION & TABLE EQUIPMENT
            // -------------------------------------------------------------------------
            float tableX = stationCenterX + 1.35f;
            float tableZ = stationZ;

            if (table != null)
            {
                table.gameObject.SetActive(true);
                Bounds tBounds = GetWorldBounds(table.gameObject);
                float tableOffsetY = -tBounds.min.y; // Ground table legs at Y=0

                table.position = new Vector3(tableX, tableOffsetY, tableZ);
                table.localEulerAngles = new Vector3(0, 0, 0);

                Bounds newTBounds = GetWorldBounds(table.gameObject);
                float tableTopY = newTBounds.max.y > 0.1f ? newTBounds.max.y : 0.75f;

                // Place Hasp and PadlockAndTag DIRECTLY ON TOP of SteelWorkstationTable
                if (hasp != null)
                {
                    hasp.gameObject.SetActive(true);
                    hasp.position = new Vector3(tableX - 0.15f, tableTopY + 0.02f, tableZ);
                    hasp.localEulerAngles = new Vector3(90, 0, 0); // Resting flat on surface
                }

                if (padlock != null)
                {
                    padlock.gameObject.SetActive(true);
                    padlock.position = new Vector3(tableX + 0.15f, tableTopY + 0.02f, tableZ);
                    padlock.localEulerAngles = new Vector3(0, 45, 0); // Resting flat on surface
                }

                Debug.Log($"[5. LOTO Workstation] Table grounded at Y=0, TopY={tableTopY:F3}. Hasp & Padlock placed on table top surface!");
            }

            // -------------------------------------------------------------------------
            // G. CONTINUOUS COAL MATERIAL STREAM (HOPPER -> CONVEYOR -> JAM)
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
            if (coalPrefab == null)
            {
                Debug.LogError($"[PhysicalAssembler] Missing coal GLB asset at {CoalGlbPath}");
                return;
            }

            Shader standardShader = Shader.Find("Standard");
            Random.InitState(101);

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
                    // AREA C: JAM BLOCKAGE — Dense piling near jam location
                    px = jamAreaX + Random.Range(-0.35f, 0.35f);
                    pz = beltCenterZ + Random.Range(-channelWidthZ, channelWidthZ);
                    float dist = Vector2.Distance(new Vector2(px, pz), new Vector2(jamAreaX, beltCenterZ));
                    float pileHeight = Mathf.Max(0f, 0.18f - dist * 0.45f);
                    py = beltTopY + Random.Range(0.005f, 0.03f) + pileHeight;
                }
                else if (i < 145)
                {
                    // AREA B: NORMAL STREAM — Continuous flow between hopper and jam
                    px = Mathf.Lerp(jamAreaX + 0.35f, feedEndX - 0.25f, Random.Range(0f, 1f));
                    pz = beltCenterZ + Random.Range(-channelWidthZ, channelWidthZ);
                    py = beltTopY + Random.Range(0.005f, 0.025f);
                }
                else
                {
                    // AREA A: INCOMING COAL — Originating directly below Feed Hopper spout
                    px = feedEndX + Random.Range(-0.15f, 0.20f);
                    pz = beltCenterZ + Random.Range(-channelWidthZ * 0.8f, channelWidthZ * 0.8f);
                    py = beltTopY + Random.Range(0.005f, 0.04f);
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

            Debug.Log($"[6. Coal Material Stream] Created {totalCoalPieces} coal pieces connecting Hopper Spout -> Belt -> Jam Pile!");

            // -------------------------------------------------------------------------
            // H. SAVE SCENE & RE-OPEN VERIFICATION
            // -------------------------------------------------------------------------
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            var verifyScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform verifyJamArea = null;
            GameObject freshMasterRoot = null;
            foreach (var r in verifyScene.GetRootGameObjects())
            {
                if (r.name == "MachineryScenario")
                {
                    freshMasterRoot = r;
                    verifyJamArea = r.transform.Find("JamArea");
                }
            }

            int count = verifyJamArea != null ? verifyJamArea.childCount : 0;
            Debug.Log($"[7. Persistence Verification] Scene saved & reloaded. Total Coal Pieces = {count}");

            // Detailed inspector of all renderers under freshMasterRoot
            Debug.Log("--- LISTING ALL MESH RENDERERS UNDER MachineryScenario ---");
            MeshRenderer[] allRenderers = freshMasterRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in allRenderers)
            {
                Debug.Log($"Renderer: '{r.gameObject.name}' | Parent: '{r.transform.parent?.name}' | Bounds Center: {r.bounds.center} | Size: {r.bounds.size} | LocalScale: {r.transform.localScale}");
                
                // If a renderer is huge (e.g., size > 15m in any axis), it is background wall/shutter/room geometry attached inside a model
                if (r.bounds.size.x > 15f || r.bounds.size.y > 15f || r.bounds.size.z > 15f ||
                    r.gameObject.name.Contains("Shutter") || r.gameObject.name.Contains("Wall") || r.gameObject.name.Contains("Door") || r.gameObject.name.Contains("Room"))
                {
                    Debug.Log($"[Cleanup] Disabling huge/background mesh: '{r.gameObject.name}' (Size: {r.bounds.size})");
                    r.gameObject.SetActive(false);
                }
            }

            // Calculate overall machinery bounds for exact framing (only active renderers)
            Bounds overallBounds = GetWorldBounds(freshMasterRoot);
            Vector3 center = overallBounds.center;
            Debug.Log($"[Camera Framing] Clean Machinery Bounds Center={center}, Size={overallBounds.size}");

            // Set camera position to clearly view the setup centered around (0, 0.6, 0)
            Vector3 cameraTarget = new Vector3(0.0f, 0.6f, 0.0f);
            Vector3 cameraEye = new Vector3(4.5f, 3.5f, -5.0f);

            RenderCamera(cameraEye, cameraTarget, "Machinery_Final_Scene_View.png");
            RenderCamera(cameraEye, cameraTarget, "Overall_Machinery_Scene.png");
            RenderCamera(new Vector3(2.5f, 2.0f, -3.5f), new Vector3(1.35f, 0.7f, -1.35f), "LOTO_Area.png");
            RenderCamera(new Vector3(-0.5f, 2.0f, -3.5f), new Vector3(0.0f, 1.0f, -1.35f), "Control_Safety_Area.png");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPhysicalAssembler] TARGETED MACHINERY CORRECTION COMPLETE");
            Debug.Log("=========================================================================");
        }

        private static Bounds GetWorldBounds(GameObject go)
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

        private static void RenderCamera(Vector3 pos, Vector3 target, string filename)
        {
            GameObject tempCamGo = new GameObject("TempPhysicalAssemblerCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1.0f);
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;
            tempCam.fieldOfView = 50f;

            tempCam.transform.position = pos;
            tempCam.transform.LookAt(target);

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            rt.Create();
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            tempCam.targetTexture = rt;
            tempCam.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();

            if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(ArtifactFolder)) Directory.CreateDirectory(ArtifactFolder);

            File.WriteAllBytes(Path.Combine(OutputFolder, filename), bytes);
            File.WriteAllBytes(Path.Combine(ArtifactFolder, filename), bytes);

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log($"[RenderCamera] Saved: {filename} ({bytes.Length} bytes)");
        }
    }
}
#endif

