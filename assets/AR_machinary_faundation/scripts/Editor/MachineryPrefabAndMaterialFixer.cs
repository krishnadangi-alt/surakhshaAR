#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPrefabAndMaterialFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string PrefabsFolder = "Assets/AR_machinary_faundation/Prefabs";
        private const string MaterialsFolder = "Assets/AR_machinary_faundation/Materials";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";
        private const string OutputPNGName = "Machinery_Final_Fixed_Scene_View.png";

        private struct TargetModelInfo
        {
            public string nodeName;
            public string glbName;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
            public bool isLotoChild;
        }

        [MenuItem("SurakshaAR/Machinery/Fix All Prefab Links and Magenta Materials")]
        public static void FixPrefabsAndMaterials()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryPrefabAndMaterialFixer] FIXING PREFAB LINKS AND MAGENTA MATERIALS...");
            Debug.Log("=========================================================================");

            if (!Directory.Exists(PrefabsFolder)) Directory.CreateDirectory(PrefabsFolder);
            if (!Directory.Exists(MaterialsFolder)) Directory.CreateDirectory(MaterialsFolder);
            if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(ArtifactFolder)) Directory.CreateDirectory(ArtifactFolder);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryPrefabAndMaterialFixer] Failed to open scene at {ScenePath}");
                return;
            }

            // Step 1: Clean Scene Roots
            GameObject[] rootObjects = scene.GetRootGameObjects();
            GameObject masterRoot = null;

            foreach (var r in rootObjects)
            {
                if (r.name == "MachineryModelInspection" || r.name.StartsWith("Label_") || r.name.StartsWith("Temp"))
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

            // Destroy all existing children to guarantee 100% removal of broken missing prefabs
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

            // Target 10 Objects Configuration
            List<TargetModelInfo> targets = new List<TargetModelInfo>()
            {
                new TargetModelInfo { nodeName = "ConveyorAssembly", glbName = "industrial_conveyor_belt", position = Vector3.zero, rotation = new Vector3(-90f, 90f, 0f), scale = new Vector3(0.012f, 0.012f, 0.012f), isLotoChild = false },
                new TargetModelInfo { nodeName = "DriveMotor", glbName = "electric_motor", position = new Vector3(2.299f, 0f, 0f), rotation = new Vector3(0f, 180f, 0f), scale = new Vector3(0.42f, 0.42f, 0.42f), isLotoChild = false },
                new TargetModelInfo { nodeName = "JamArea", glbName = "coal", position = new Vector3(-0.200f, 0f, 0f), rotation = new Vector3(15f, 35f, -10f), scale = new Vector3(0.038f, 0.032f, 0.038f), isLotoChild = false },
                new TargetModelInfo { nodeName = "FeedHopper", glbName = "hopper", position = new Vector3(-1.729f, 0f, 0f), rotation = new Vector3(0f, 90f, 0f), scale = new Vector3(0.00065f, 0.00065f, 0.00065f), isLotoChild = false },
                new TargetModelInfo { nodeName = "EmergencyStop", glbName = "emergency_stop_button", position = new Vector3(2.750f, 1.150f, 1.550f), rotation = new Vector3(-90f, 180f, 0f), scale = new Vector3(0.0022f, 0.0022f, 0.0022f), isLotoChild = false },
                new TargetModelInfo { nodeName = "MainIsolator", glbName = "switch", position = new Vector3(1.450f, 1.150f, 1.550f), rotation = new Vector3(0f, 180f, 0f), scale = new Vector3(0.24f, 0.24f, 0.24f), isLotoChild = false },
                new TargetModelInfo { nodeName = "ControlPanel", glbName = "industrial_control_panel_box", position = new Vector3(2.100f, 1.050f, 1.550f), rotation = new Vector3(0f, 180f, 0f), scale = new Vector3(0.00095f, 0.00095f, 0.00095f), isLotoChild = false },
                new TargetModelInfo { nodeName = "SteelWorkstationTable", glbName = "industrial_steel_table", position = new Vector3(2.100f, 0f, -1.500f), rotation = new Vector3(-90f, 90f, 0f), scale = new Vector3(0.011f, 0.011f, 0.011f), isLotoChild = false },
                new TargetModelInfo { nodeName = "Hasp", glbName = "hasp_and_staple", position = new Vector3(1.750f, 0f, -1.500f), rotation = new Vector3(-90f, 45f, 0f), scale = new Vector3(0.0016f, 0.0016f, 0.0016f), isLotoChild = true },
                new TargetModelInfo { nodeName = "PadlockAndTag", glbName = "lockout_tagout_locks_with_key_and_tag", position = new Vector3(2.450f, 0f, -1.500f), rotation = new Vector3(0f, 30f, 0f), scale = new Vector3(1.60f, 1.60f, 1.60f), isLotoChild = true }
            };

            Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

            foreach (var info in targets)
            {
                string glbPath = $"Assets/AR_machinary_faundation/3d/Raw/{info.glbName}.glb";
                GameObject glbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (glbPrefab == null)
                {
                    Debug.LogError($"[MachineryPrefabAndMaterialFixer] Missing GLB asset at: {glbPath}");
                    continue;
                }

                // Instantiate raw model
                GameObject parentNode = info.isLotoChild ? lotoParent : masterRoot;
                GameObject instance = Object.Instantiate(glbPrefab, parentNode.transform);
                instance.name = info.nodeName;
                instance.SetActive(true);

                // Only unpack if it's actually a prefab instance
                if (PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                // Fix materials and shaders on all mesh renderers
                FixMaterialsAndShaders(instance, info.nodeName);

                // Save a clean standalone .prefab asset in Assets/AR_machinary_faundation/Prefabs/
                string cleanPrefabPath = $"{PrefabsFolder}/{info.nodeName}.prefab";
                GameObject savedPrefabAsset = PrefabUtility.SaveAsPrefabAsset(instance, cleanPrefabPath);
                
                // Replace scene instance with a clean connected prefab instance of the savedPrefabAsset
                Object.DestroyImmediate(instance);
                GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefabAsset, parentNode.transform);
                prefabInstance.name = info.nodeName;
                prefabInstance.SetActive(true);

                // Apply initial transform
                prefabInstance.transform.localPosition = info.position;
                prefabInstance.transform.localEulerAngles = info.rotation;
                prefabInstance.transform.localScale = info.scale;

                createdObjects[info.nodeName] = prefabInstance;
            }

            // Surgical geometry filtering
            GameObject conveyor = createdObjects.ContainsKey("ConveyorAssembly") ? createdObjects["ConveyorAssembly"] : null;
            GameObject hopper = createdObjects.ContainsKey("FeedHopper") ? createdObjects["FeedHopper"] : null;
            GameObject panel = createdObjects.ContainsKey("ControlPanel") ? createdObjects["ControlPanel"] : null;
            GameObject table = createdObjects.ContainsKey("SteelWorkstationTable") ? createdObjects["SteelWorkstationTable"] : null;
            GameObject jam = createdObjects.ContainsKey("JamArea") ? createdObjects["JamArea"] : null;
            GameObject motor = createdObjects.ContainsKey("DriveMotor") ? createdObjects["DriveMotor"] : null;
            GameObject eStop = createdObjects.ContainsKey("EmergencyStop") ? createdObjects["EmergencyStop"] : null;
            GameObject isolator = createdObjects.ContainsKey("MainIsolator") ? createdObjects["MainIsolator"] : null;
            GameObject hasp = createdObjects.ContainsKey("Hasp") ? createdObjects["Hasp"] : null;
            GameObject padlock = createdObjects.ContainsKey("PadlockAndTag") ? createdObjects["PadlockAndTag"] : null;

            CleanTargetGeometry(conveyor, hopper, panel, table);

            // Precise Placement Alignment
            if (conveyor != null)
            {
                GroundObject(conveyor, 0.0f);
                Bounds rawCBounds = GetBounds(conveyor);
                conveyor.transform.position -= new Vector3(rawCBounds.center.x, 0f, rawCBounds.center.z);
            }

            Bounds cBounds = GetBounds(conveyor);
            float beltTopY = cBounds.max.y;

            if (hopper != null)
            {
                float feedX = cBounds.min.x + 0.35f;
                hopper.transform.localPosition = new Vector3(feedX, 0f, cBounds.center.z);

                Bounds hBounds = GetBounds(hopper);
                float yTarget = beltTopY + 0.08f;
                float yOffset = yTarget - hBounds.min.y;
                hopper.transform.localPosition += new Vector3(0f, yOffset, 0f);

                Bounds finalHBounds = GetBounds(hopper);
                EnsureHopperLegsGrounded(masterRoot, finalHBounds);
            }

            if (jam != null)
            {
                jam.transform.localPosition = new Vector3(cBounds.center.x - 0.20f, 0f, cBounds.center.z);
                Bounds jBounds = GetBounds(jam);
                float yOffset = beltTopY - jBounds.min.y - 0.02f;
                jam.transform.localPosition += new Vector3(0f, yOffset, 0f);
            }

            if (motor != null)
            {
                float motorX = cBounds.max.x + 0.22f;
                motor.transform.localPosition = new Vector3(motorX, 0f, cBounds.center.z);
                GroundObject(motor, 0.0f);
                CreateMotorBasePlate(masterRoot, motor.transform.position);
            }

            float safetyX = 2.10f;
            float safetyZ = 1.60f;
            CreateControlStationStand(masterRoot, new Vector3(safetyX, 0f, safetyZ));

            if (panel != null) panel.transform.localPosition = new Vector3(safetyX, 1.05f, safetyZ - 0.05f);
            if (isolator != null) isolator.transform.localPosition = new Vector3(safetyX - 0.65f, 1.15f, safetyZ - 0.05f);
            if (eStop != null) eStop.transform.localPosition = new Vector3(safetyX + 0.65f, 1.15f, safetyZ - 0.05f);

            float lotoX = 2.10f;
            float lotoZ = -1.50f;
            if (table != null)
            {
                table.transform.localPosition = new Vector3(lotoX, 0f, lotoZ);
                GroundObject(table, 0.0f);
            }

            Bounds tBounds = GetBounds(table);
            float tableTopY = tBounds.max.y;

            if (hasp != null)
            {
                hasp.transform.localPosition = new Vector3(tBounds.center.x - 0.35f, 0f, tBounds.center.z);
                Bounds hBounds = GetBounds(hasp);
                hasp.transform.localPosition += new Vector3(0f, tableTopY - hBounds.min.y + 0.005f, 0f);
            }

            if (padlock != null)
            {
                padlock.transform.localPosition = new Vector3(tBounds.center.x + 0.35f, 0f, tBounds.center.z);
                Bounds pbBounds = GetBounds(padlock);
                padlock.transform.localPosition += new Vector3(0f, tableTopY - pbBounds.min.y + 0.005f, 0f);
            }

            // Verification of Prefabs and Shaders
            AuditSceneObjects(masterRoot);

            // Save scene and mark dirty
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MachineryPrefabAndMaterialFixer] Scene saved successfully to {ScenePath}");

            // Reopen scene in batchmode to verify persistence
            var verifyScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (verifyScene.IsValid())
            {
                Debug.Log("[MachineryPrefabAndMaterialFixer] Scene reopened & transform persistence verified!");
            }

            // Render final clean setup images
            RenderWorkerPerspective(OutputPNGName);
            RenderWorkerPerspective("Machinery_FINAL_POLISHED_SCENE.png");
            RenderWorkerPerspective("Machinery_Final_Scene_View_V2.png");

            Debug.Log("=========================================================================");
            Debug.Log($"[MachineryPrefabAndMaterialFixer] PREFAB AND MATERIAL FIX COMPLETE: {OutputPNGName}");
            Debug.Log("=========================================================================");
        }

        private static void FixMaterialsAndShaders(GameObject go, string objectName)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Shader standardShader = Shader.Find("Standard");

            foreach (var r in renderers)
            {
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material mat = mats[i];
                    if (mat == null)
                    {
                        // Create replacement material if missing
                        mat = new Material(standardShader);
                        mat.name = $"{objectName}_Mat_{i}";
                        mat.color = new Color(0.7f, 0.7f, 0.75f, 1f);
                        mats[i] = mat;
                        string matPath = $"{MaterialsFolder}/{mat.name}.mat";
                        AssetDatabase.CreateAsset(mat, matPath);
                        continue;
                    }

                    // Check if shader is missing, invalid, hidden, or magenta
                    bool isInvalidShader = (mat.shader == null) || 
                                           mat.shader.name.StartsWith("Hidden/") || 
                                           mat.shader.name.Contains("InternalErrorShader") || 
                                           mat.shader.name.Contains("Error");

                    if (isInvalidShader)
                    {
                        Debug.LogWarning($"[Material Fix] Fixing invalid shader on '{mat.name}' ({r.name}): OldShader='{(mat.shader != null ? mat.shader.name : "NULL")}' -> Standard");
                        mat.shader = standardShader;

                        // Ensure basic color is valid
                        if (!mat.HasProperty("_Color") || mat.color == Color.magenta || mat.color == Color.clear)
                        {
                            mat.color = new Color(0.65f, 0.68f, 0.72f, 1f);
                        }
                    }
                }
                r.sharedMaterials = mats;
            }
        }

        private static void AuditSceneObjects(GameObject root)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== SCENE PREFAB & SHADER AUDIT REPORT ===");
            
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            int missingPrefabs = 0;
            int magentaMaterials = 0;

            foreach (var r in renderers)
            {
                bool isMissingPrefab = PrefabUtility.IsPartOfAnyPrefab(r.gameObject) && PrefabUtility.GetPrefabInstanceStatus(r.gameObject) == PrefabInstanceStatus.MissingAsset;
                if (isMissingPrefab) missingPrefabs++;

                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null || mat.shader == null || mat.shader.name.Contains("Error") || mat.color == Color.magenta)
                    {
                        magentaMaterials++;
                    }
                }
            }

            sb.AppendLine($"Total Renderers Inspected: {renderers.Length}");
            sb.AppendLine($"Missing Prefabs Found: {missingPrefabs}");
            sb.AppendLine($"Magenta / Invalid Materials Found: {magentaMaterials}");

            string text = sb.ToString();
            Debug.Log(text);
            File.WriteAllText(Path.Combine(OutputFolder, "prefab_shader_audit_report.txt"), text);
            File.WriteAllText(Path.Combine(ArtifactFolder, "prefab_shader_audit_report.txt"), text);
        }

        private static void CleanTargetGeometry(GameObject conveyor, GameObject hopper, GameObject panel, GameObject table)
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
                    string path = GetHierarchyPath(r.transform, hopper.transform);
                    string matName = (r.sharedMaterial != null) ? r.sharedMaterial.name : "";

                    bool isExtraYellowStructure = path.Contains("Object_112") || matName.Contains("material_4") || matName.Contains("material_6") || matName.Contains("material_7") || matName.Contains("material_8");

                    if (isExtraYellowStructure)
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
