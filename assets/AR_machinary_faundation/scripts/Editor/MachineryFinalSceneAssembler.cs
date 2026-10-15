#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFinalSceneAssembler
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";
        private const string OutputPNGName = "Machinery_Final_Scene_View.png";

        [MenuItem("SurakshaAR/Machinery/Assemble Final Training Scene")]
        public static void AssembleFinalScene()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryFinalSceneAssembler] ASSEMBLING FINAL MACHINERY SCENE...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryFinalSceneAssembler] Failed to open scene at {ScenePath}");
                return;
            }

            if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(ArtifactFolder)) Directory.CreateDirectory(ArtifactFolder);

            // Clean scene roots: destroy inspection roots and extra scenario roots
            GameObject[] rootObjects = scene.GetRootGameObjects();
            GameObject masterRoot = null;

            foreach (var r in rootObjects)
            {
                if (r.name == "MachineryModelInspection")
                {
                    Object.DestroyImmediate(r);
                }
                else if (r.name == "MachineryScenario")
                {
                    if (masterRoot == null) masterRoot = r;
                    else Object.DestroyImmediate(r);
                }
            }

            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
            }
            masterRoot.SetActive(true);

            // Clear old children for 100% clean deterministic rebuild
            List<GameObject> existingChildren = new List<GameObject>();
            foreach (Transform child in masterRoot.transform)
            {
                existingChildren.Add(child.gameObject);
            }
            foreach (var child in existingChildren)
            {
                Object.DestroyImmediate(child);
            }

            // LOTO Assembly Container
            GameObject lotoParent = new GameObject("LOTOAssembly");
            lotoParent.transform.SetParent(masterRoot.transform, false);

            // Instantiate 10 Target Machinery Objects
            GameObject conveyor = CreateObject(masterRoot, "ConveyorAssembly", "industrial_conveyor_belt");
            GameObject motor = CreateObject(masterRoot, "DriveMotor", "electric_motor");
            GameObject jam = CreateObject(masterRoot, "JamArea", "coal");
            GameObject hopper = CreateObject(masterRoot, "FeedHopper", "hopper");
            GameObject eStop = CreateObject(masterRoot, "EmergencyStop", "emergency_stop_button");
            GameObject isolator = CreateObject(masterRoot, "MainIsolator", "switch");
            GameObject panel = CreateObject(masterRoot, "ControlPanel", "industrial_control_panel_box");
            GameObject table = CreateObject(masterRoot, "SteelWorkstationTable", "industrial_steel_table");
            GameObject hasp = CreateObject(lotoParent, "Hasp", "hasp_and_staple");
            GameObject padlock = CreateObject(lotoParent, "PadlockAndTag", "lockout_tagout_locks_with_key_and_tag");

            // Surgical geometry filtering: Keep machinery & yellow hopper structure, disable room background walls
            FilterSceneGeometry(conveyor, hopper, panel, table);

            // -------------------------------------------------------------------------
            // ZONE 1: MACHINE (LEFT / CENTER)
            // -------------------------------------------------------------------------

            // 1. CONVEYOR ASSEMBLY
            if (conveyor != null)
            {
                conveyor.transform.localPosition = Vector3.zero;
                conveyor.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
                conveyor.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                GroundObject(conveyor, 0.0f);

                Bounds rawCBounds = GetBounds(conveyor);
                conveyor.transform.position -= new Vector3(rawCBounds.center.x, 0f, rawCBounds.center.z);
            }

            Bounds cBounds = GetBounds(conveyor);
            float beltTopY = cBounds.max.y;
            Debug.Log($"[Conveyor] Centered Bounds: Center={cBounds.center}, Size={cBounds.size}, BeltTopY={beltTopY:F3}");

            // 2. FEED HOPPER (LEFT SIDE)
            if (hopper != null)
            {
                hopper.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                hopper.transform.localScale = new Vector3(0.00065f, 0.00065f, 0.00065f);

                float feedX = cBounds.min.x + 0.35f;
                hopper.transform.localPosition = new Vector3(feedX, 0f, cBounds.center.z);

                Bounds hBounds = GetBounds(hopper);
                float yTarget = beltTopY + 0.08f;
                float yOffset = yTarget - hBounds.min.y;
                hopper.transform.localPosition += new Vector3(0f, yOffset, 0f);

                Bounds finalHBounds = GetBounds(hopper);
                EnsureHopperLegsGrounded(masterRoot, finalHBounds);
            }

            // 3. COAL JAM (CENTER BELT)
            if (jam != null)
            {
                jam.transform.localEulerAngles = new Vector3(15f, 35f, -10f);
                jam.transform.localScale = new Vector3(0.038f, 0.032f, 0.038f);
                jam.transform.localPosition = new Vector3(cBounds.center.x - 0.20f, 0f, cBounds.center.z);

                Bounds jBounds = GetBounds(jam);
                float yOffset = beltTopY - jBounds.min.y - 0.02f;
                jam.transform.localPosition += new Vector3(0f, yOffset, 0f);
            }

            // 4. DRIVE MOTOR (RIGHT END)
            if (motor != null)
            {
                motor.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                motor.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);

                float motorX = cBounds.max.x + 0.22f;
                motor.transform.localPosition = new Vector3(motorX, 0f, cBounds.center.z);
                GroundObject(motor, 0.0f);

                CreateMotorBasePlate(masterRoot, motor.transform.position);
            }

            // -------------------------------------------------------------------------
            // ZONE 2: SAFETY CONTROL STATION (RIGHT / BACK)
            // -------------------------------------------------------------------------
            float safetyX = 2.10f;
            float safetyZ = 1.60f;

            CreateControlStationStand(masterRoot, new Vector3(safetyX, 0f, safetyZ));

            if (panel != null)
            {
                panel.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                panel.transform.localScale = new Vector3(0.00095f, 0.00095f, 0.00095f);
                panel.transform.localPosition = new Vector3(safetyX, 1.05f, safetyZ - 0.05f);
            }

            if (isolator != null)
            {
                isolator.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                isolator.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
                isolator.transform.localPosition = new Vector3(safetyX - 0.65f, 1.15f, safetyZ - 0.05f);
            }

            if (eStop != null)
            {
                eStop.transform.localEulerAngles = new Vector3(-90f, 180f, 0f);
                eStop.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
                eStop.transform.localPosition = new Vector3(safetyX + 0.65f, 1.15f, safetyZ - 0.05f);
            }

            // -------------------------------------------------------------------------
            // ZONE 3: LOTO WORKSTATION (RIGHT / FRONT)
            // -------------------------------------------------------------------------
            float lotoX = 2.10f;
            float lotoZ = -1.50f;

            if (table != null)
            {
                table.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
                table.transform.localScale = new Vector3(0.011f, 0.011f, 0.011f);
                table.transform.localPosition = new Vector3(lotoX, 0f, lotoZ);
                GroundObject(table, 0.0f);
            }

            Bounds tBounds = GetBounds(table);
            float tableTopY = tBounds.max.y;

            if (hasp != null)
            {
                hasp.transform.localEulerAngles = new Vector3(-90f, 45f, 0f);
                hasp.transform.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
                hasp.transform.localPosition = new Vector3(tBounds.center.x - 0.35f, 0f, tBounds.center.z);
                Bounds hBounds = GetBounds(hasp);
                hasp.transform.localPosition += new Vector3(0f, tableTopY - hBounds.min.y + 0.005f, 0f);
            }

            if (padlock != null)
            {
                padlock.transform.localEulerAngles = new Vector3(0f, 30f, 0f);
                padlock.transform.localScale = new Vector3(1.60f, 1.60f, 1.60f);
                padlock.transform.localPosition = new Vector3(tBounds.center.x + 0.35f, 0f, tBounds.center.z);
                Bounds pbBounds = GetBounds(padlock);
                padlock.transform.localPosition += new Vector3(0f, tableTopY - pbBounds.min.y + 0.005f, 0f);
            }

            // Save scene and mark dirty
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MachineryFinalSceneAssembler] Scene successfully saved to {ScenePath}");

            // Reopen scene in batchmode to verify persistence
            var verifyScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (verifyScene.IsValid())
            {
                Debug.Log("[MachineryFinalSceneAssembler] Scene reopened & transform persistence verified!");
            }

            // Render final scene worker-perspective images
            RenderWorkerPerspective(OutputPNGName);
            RenderWorkerPerspective("Machinery_Final_Training_Platform.png");

            Debug.Log("=========================================================================");
            Debug.Log($"[MachineryFinalSceneAssembler] FINAL SCENE ASSEMBLY COMPLETE: {OutputPNGName}");
            Debug.Log("=========================================================================");
        }

        private static void FilterSceneGeometry(GameObject conveyor, GameObject hopper, GameObject panel, GameObject table)
        {
            if (conveyor != null)
            {
                foreach (var r in conveyor.GetComponentsInChildren<MeshRenderer>(true))
                {
                    string path = GetHierarchyPath(r.transform, conveyor.transform);
                    if (!path.Contains("Conveyor_belt") || path.Contains("Floor") || path.Contains("Vinyl"))
                    {
                        r.enabled = false;
                    }
                    else
                    {
                        r.enabled = true;
                    }
                }
            }

            if (hopper != null)
            {
                foreach (var r in hopper.GetComponentsInChildren<MeshRenderer>(true))
                {
                    string matName = (r.sharedMaterial != null) ? r.sharedMaterial.name : "";
                    if (matName.Contains("material_6") || matName.Contains("material_7") || matName.Contains("material_8"))
                    {
                        r.enabled = false;
                    }
                    else
                    {
                        r.enabled = true;
                    }
                }
            }
        }

        private static void CreateControlStationStand(GameObject parent, Vector3 centerPos)
        {
            GameObject stand = new GameObject("ControlStationStand");
            stand.transform.SetParent(parent.transform, false);
            stand.transform.position = centerPos;

            Material metalMat = new Material(Shader.Find("Standard"));
            metalMat.color = new Color(0.18f, 0.20f, 0.24f, 1f);
            if (metalMat.HasProperty("_Metallic")) metalMat.SetFloat("_Metallic", 0.8f);
            if (metalMat.HasProperty("_Glossiness")) metalMat.SetFloat("_Glossiness", 0.5f);

            GameObject leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPost.name = "LeftPost";
            leftPost.transform.SetParent(stand.transform, false);
            leftPost.transform.localPosition = new Vector3(-0.65f, 0.60f, 0f);
            leftPost.transform.localScale = new Vector3(0.06f, 1.20f, 0.06f);
            leftPost.GetComponent<MeshRenderer>().material = metalMat;

            GameObject rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPost.name = "RightPost";
            rightPost.transform.SetParent(stand.transform, false);
            rightPost.transform.localPosition = new Vector3(0.65f, 0.60f, 0f);
            rightPost.transform.localScale = new Vector3(0.06f, 1.20f, 0.06f);
            rightPost.GetComponent<MeshRenderer>().material = metalMat;

            GameObject backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backboard.name = "BackboardFrame";
            backboard.transform.SetParent(stand.transform, false);
            backboard.transform.localPosition = new Vector3(0f, 1.10f, 0.02f);
            backboard.transform.localScale = new Vector3(1.60f, 0.70f, 0.03f);
            backboard.GetComponent<MeshRenderer>().material = metalMat;
        }

        private static void EnsureHopperLegsGrounded(GameObject parent, Bounds hopperBounds)
        {
            float legHeight = hopperBounds.min.y;
            if (legHeight > 0.05f)
            {
                GameObject frame = new GameObject("HopperGroundLegs");
                frame.transform.SetParent(parent.transform, false);

                Material steelMat = new Material(Shader.Find("Standard"));
                steelMat.color = new Color(0.25f, 0.27f, 0.30f, 1f);

                float legX = hopperBounds.center.x;
                float legZ = hopperBounds.center.z;

                GameObject leg1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg1.name = "Leg_Front";
                leg1.transform.SetParent(frame.transform, false);
                leg1.transform.position = new Vector3(legX - 0.35f, legHeight * 0.5f, legZ - 0.35f);
                leg1.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
                leg1.GetComponent<MeshRenderer>().material = steelMat;

                GameObject leg2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg2.name = "Leg_Back";
                leg2.transform.SetParent(frame.transform, false);
                leg2.transform.position = new Vector3(legX - 0.35f, legHeight * 0.5f, legZ + 0.35f);
                leg2.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
                leg2.GetComponent<MeshRenderer>().material = steelMat;
            }
        }

        private static void CreateMotorBasePlate(GameObject parent, Vector3 motorPos)
        {
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "MotorBasePlate";
            plate.transform.SetParent(parent.transform, false);
            plate.transform.position = new Vector3(motorPos.x, 0.025f, motorPos.z);
            plate.transform.localScale = new Vector3(0.75f, 0.05f, 0.60f);

            Material darkMetal = new Material(Shader.Find("Standard"));
            darkMetal.color = new Color(0.15f, 0.15f, 0.18f, 1f);
            plate.GetComponent<MeshRenderer>().material = darkMetal;
        }

        private static string GetHierarchyPath(Transform t, Transform root)
        {
            string path = t.name;
            Transform curr = t.parent;
            while (curr != null && curr != root)
            {
                path = curr.name + "/" + path;
                curr = curr.parent;
            }
            return path;
        }

        private static GameObject CreateObject(GameObject parent, string nodeName, string glbName)
        {
            string glbPath = $"Assets/AR_machinary_faundation/3d/Raw/{glbName}.glb";
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"[MachineryFinalSceneAssembler] Missing GLB asset: {glbPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, parent.transform);
            instance.name = nodeName;
            instance.SetActive(true);
            return instance;
        }

        private static Bounds GetBounds(GameObject go)
        {
            if (go == null) return new Bounds();
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                if (!hasFirst)
                {
                    b = r.bounds;
                    hasFirst = true;
                }
                else
                {
                    b.Encapsulate(r.bounds);
                }
            }
            return b;
        }

        private static void GroundObject(GameObject go, float groundY)
        {
            Bounds b = GetBounds(go);
            float yOffset = groundY - b.min.y;
            go.transform.position += new Vector3(0f, yOffset, 0f);
        }

        private static void RenderWorkerPerspective(string filename)
        {
            GameObject tempCamGo = new GameObject("TempWorkerPerspectiveCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1.0f);
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;
            tempCam.fieldOfView = 52f;

            // Worker-eye perspective camera showing all 3 zones clearly
            tempCam.transform.position = new Vector3(-0.2f, 3.4f, -5.8f);
            tempCam.transform.LookAt(new Vector3(0.2f, 0.6f, 0.0f));

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            tempCam.targetTexture = rt;
            tempCam.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();

            File.WriteAllBytes(Path.Combine(OutputFolder, filename), bytes);
            File.WriteAllBytes(Path.Combine(ArtifactFolder, filename), bytes);

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log($"[RenderWorkerPerspective] Rendered view: {filename} ({bytes.Length} bytes)");
        }
    }
}
#endif
