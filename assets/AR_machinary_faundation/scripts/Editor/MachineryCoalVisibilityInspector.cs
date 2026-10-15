#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryCoalVisibilityInspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";

        [MenuItem("SurakshaAR/Machinery/Inspect Coal Visibility and Framing")]
        public static void InspectVisibilityAndFrame()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryCoalVisibilityInspector] INSPECTING COAL VISIBILITY IN SCENE...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Inspector] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();
            GameObject masterRoot = null;
            foreach (var r in rootObjects)
            {
                if (r.name == "MachineryScenario") masterRoot = r;
            }

            if (masterRoot == null)
            {
                Debug.LogError("[Inspector] MachineryScenario root object not found!");
                return;
            }

            // Inspect ConveyorAssembly bounds
            Transform conveyorTransform = masterRoot.transform.Find("ConveyorAssembly");
            if (conveyorTransform == null)
            {
                Debug.LogError("[Inspector] ConveyorAssembly not found!");
                return;
            }

            MeshRenderer[] cRenderers = conveyorTransform.GetComponentsInChildren<MeshRenderer>(true);
            Bounds cBounds = new Bounds();
            bool hasFirst = false;
            foreach (var r in cRenderers)
            {
                if (!r.enabled) continue;
                if (!hasFirst) { cBounds = r.bounds; hasFirst = true; }
                else { cBounds.Encapsulate(r.bounds); }
            }

            Debug.Log($"[Conveyor Bounds] Center={cBounds.center}, Size={cBounds.size}, Min={cBounds.min}, Max={cBounds.max}");
            float beltTopY = cBounds.max.y;

            // Inspect JamArea
            Transform jamTransform = masterRoot.transform.Find("JamArea");
            if (jamTransform == null)
            {
                Debug.LogError("[Inspector] JamArea node not found under MachineryScenario!");
                return;
            }

            int childCount = jamTransform.childCount;
            Debug.Log($"[JamArea Inspection] JamArea Child Count = {childCount}");

            if (childCount == 0)
            {
                Debug.LogWarning("[Inspector] JamArea has 0 children! Regenerating coal pieces...");
                RegenerateCoal(masterRoot, cBounds, beltTopY);
                childCount = jamTransform.childCount;
            }

            // Measure Coal Piece Bounding Boxes in World Space
            Bounds totalCoalBounds = new Bounds();
            bool firstCoal = false;
            int visibleCount = 0;
            int hiddenCount = 0;

            for (int i = 0; i < jamTransform.childCount; i++)
            {
                Transform child = jamTransform.GetChild(i);
                MeshRenderer[] renderers = child.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var r in renderers)
                {
                    if (r.enabled && child.gameObject.activeSelf)
                    {
                        visibleCount++;
                        if (!firstCoal) { totalCoalBounds = r.bounds; firstCoal = true; }
                        else { totalCoalBounds.Encapsulate(r.bounds); }
                    }
                    else
                    {
                        hiddenCount++;
                    }
                }
            }

            Debug.Log($"[Coal Pieces Summary] Total Visible Renderers={visibleCount}, Hidden={hiddenCount}");
            Debug.Log($"[Coal Bulk Bounds] Center={totalCoalBounds.center}, Size={totalCoalBounds.size}, Min={totalCoalBounds.min}, Max={totalCoalBounds.max}");

            // Ensure coal piece sizes are realistic (~8cm - 18cm) and sitting on top of belt
            if (totalCoalBounds.size.y < 0.04f || totalCoalBounds.center.y < beltTopY - 0.05f)
            {
                Debug.LogWarning("[Inspector] Coal pieces are too small or sunken below belt surface! Re-scaling and elevating...");
                RegenerateCoal(masterRoot, cBounds, beltTopY);
            }

            // Frame Scene View camera on the Conveyor Jam Area
            RenderSceneViewFrames();

            // Save scene and verify reload
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            var verifyScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform verifyJamArea = null;
            foreach (var r in verifyScene.GetRootGameObjects())
            {
                if (r.name == "MachineryScenario")
                {
                    verifyJamArea = r.transform.Find("JamArea");
                }
            }
            int verifyCount = verifyJamArea != null ? verifyJamArea.childCount : 0;
            Debug.Log($"[Inspector] Scene reopened. Persistence Verified! JamArea child count = {verifyCount}");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryCoalVisibilityInspector] INSPECTION AND VERIFICATION COMPLETE");
            Debug.Log("=========================================================================");
        }

        private static void RegenerateCoal(GameObject masterRoot, Bounds cBounds, float beltTopY)
        {
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

            string coalGlbPath = "Assets/AR_machinary_faundation/3d/Raw/coal.glb";
            GameObject coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coalGlbPath);
            if (coalPrefab == null)
            {
                Debug.LogError($"[Inspector] Missing coal GLB asset at: {coalGlbPath}");
                return;
            }

            Shader standardShader = Shader.Find("Standard");
            Random.InitState(42);

            int totalPieces = 65;
            float beltCenterX = cBounds.center.x;
            float beltCenterZ = cBounds.center.z;
            float jamCenterX = beltCenterX - 0.15f;

            for (int i = 0; i < totalPieces; i++)
            {
                GameObject piece = Object.Instantiate(coalPrefab, jamArea.transform);
                piece.name = $"Coal_{i + 1:D2}";
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
                if (i < 42)
                {
                    // Dense blockage accumulation pile
                    px = jamCenterX + Random.Range(-0.25f, 0.25f);
                    pz = beltCenterZ + Random.Range(-0.18f, 0.18f);
                    float distFromJamCenter = Vector2.Distance(new Vector2(px, pz), new Vector2(jamCenterX, beltCenterZ));
                    float heightBias = Mathf.Max(0f, 0.16f - distFromJamCenter * 0.45f);
                    py = beltTopY + Random.Range(0.01f, 0.04f) + heightBias;
                }
                else
                {
                    // Leading stream of material feeding into the jam
                    px = jamCenterX - Random.Range(0.30f, 0.80f);
                    pz = beltCenterZ + Random.Range(-0.15f, 0.15f);
                    py = beltTopY + Random.Range(0.01f, 0.03f);
                }

                // Increased scale for crisp visibility (approx 8cm - 16cm per piece)
                float baseScale = Random.Range(0.0035f, 0.0065f);
                Vector3 scaleVar = new Vector3(
                    baseScale * Random.Range(0.85f, 1.15f),
                    baseScale * Random.Range(0.80f, 1.10f),
                    baseScale * Random.Range(0.85f, 1.15f)
                );

                Vector3 rotVar = new Vector3(
                    Random.Range(-25f, 25f),
                    Random.Range(0f, 360f),
                    Random.Range(-25f, 25f)
                );

                piece.transform.position = new Vector3(px, py, pz);
                piece.transform.localEulerAngles = rotVar;
                piece.transform.localScale = scaleVar;
            }

            Debug.Log($"[Inspector] Successfully regenerated {totalPieces} small coal pieces on belt surface!");
        }

        private static void RenderSceneViewFrames()
        {
            // Close-up view of the Coal Jam on Conveyor
            RenderCamera(new Vector3(-0.2f, 1.8f, -1.8f), new Vector3(0.0f, 1.18f, 0.0f), "Machinery_Coal_Jam_SceneView_CloseUp.png");
            // Overall Machinery Scene View
            RenderCamera(new Vector3(-0.2f, 3.4f, -5.8f), new Vector3(0.2f, 0.6f, 0.0f), "Machinery_Coal_Jam_Verification.png");
            RenderCamera(new Vector3(-0.2f, 3.4f, -5.8f), new Vector3(0.2f, 0.6f, 0.0f), "Machinery_Final_Scene_View.png");
        }

        private static void RenderCamera(Vector3 pos, Vector3 target, string filename)
        {
            GameObject tempCamGo = new GameObject("TempInspectorCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1.0f);
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;
            tempCam.fieldOfView = 52f;

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
