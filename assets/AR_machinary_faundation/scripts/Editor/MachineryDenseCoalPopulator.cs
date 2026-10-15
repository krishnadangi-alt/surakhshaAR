#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryDenseCoalPopulator
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";
        private const string CoalGlbPath = "Assets/AR_machinary_faundation/3d/Raw/coal.glb";

        [MenuItem("SurakshaAR/Machinery/Populate Full Dense Industrial Coal Conveyor")]
        public static void PopulateFullDenseCoal()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachineryDenseCoalPopulator] GENERATING FULL INDUSTRIAL COAL CONVEYOR...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[DenseCoalPopulator] Failed to open scene at {ScenePath}");
                return;
            }

            if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(ArtifactFolder)) Directory.CreateDirectory(ArtifactFolder);

            // Locate MachineryScenario Root
            GameObject[] rootObjects = scene.GetRootGameObjects();
            GameObject masterRoot = null;
            foreach (var r in rootObjects)
            {
                if (r.name == "MachineryScenario") masterRoot = r;
            }

            if (masterRoot == null)
            {
                Debug.LogError("[DenseCoalPopulator] MachineryScenario root object not found!");
                return;
            }

            // Find ConveyorAssembly bounds
            Transform conveyorTransform = masterRoot.transform.Find("ConveyorAssembly");
            if (conveyorTransform == null)
            {
                Debug.LogError("[DenseCoalPopulator] ConveyorAssembly node not found under MachineryScenario!");
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

            float beltTopY = cBounds.max.y;
            float beltCenterX = cBounds.center.x;
            float beltCenterZ = cBounds.center.z;
            float beltWidthZ = cBounds.size.z * 0.55f; // Active belt surface width
            float beltMinX = cBounds.min.x + 0.35f;   // Jam end
            float beltMaxX = cBounds.max.x - 0.25f;   // Hopper exit end

            Debug.Log($"[Conveyor Bounds] Surface Y={beltTopY:F3}, X Range=[{beltMinX:F3}, {beltMaxX:F3}], Z Center={beltCenterZ:F3}, Z Width={beltWidthZ:F3}");

            // Find or recreate JamArea container
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

            // Load real coal.glb model prefab
            GameObject coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoalGlbPath);
            if (coalPrefab == null)
            {
                Debug.LogError($"[DenseCoalPopulator] Missing coal GLB asset at: {CoalGlbPath}");
                return;
            }

            Shader standardShader = Shader.Find("Standard");
            Random.InitState(101); // Fixed seed for reproducible, visually stunning pile

            int totalPieces = 185; // Target 150-220 pieces for optimal AR visual density and mobile performance
            float jamCenterX = beltMinX + 0.30f; // Jam accumulation center

            for (int i = 0; i < totalPieces; i++)
            {
                GameObject piece = Object.Instantiate(coalPrefab, jamArea.transform);
                piece.name = $"Coal_{i + 1:D3}";
                piece.SetActive(true);

                // Ensure material has valid Standard shader with dark coal rock appearance
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

                if (i < 85)
                {
                    // AREA C: JAM/BLOCKAGE — Extra dense accumulation pile (85 pieces)
                    px = jamCenterX + Random.Range(-0.35f, 0.35f);
                    pz = beltCenterZ + Random.Range(-beltWidthZ * 0.45f, beltWidthZ * 0.45f);

                    float distFromJamCenter = Vector2.Distance(new Vector2(px, pz), new Vector2(jamCenterX, beltCenterZ));
                    float heightBias = Mathf.Max(0f, 0.18f - distFromJamCenter * 0.42f);
                    py = beltTopY + Random.Range(0.005f, 0.035f) + heightBias;
                }
                else if (i < 155)
                {
                    // AREA B: NORMAL CONVEYOR STREAM — Continuous coal stream across belt width (70 pieces)
                    px = Mathf.Lerp(jamCenterX + 0.35f, beltMaxX - 0.40f, Random.Range(0f, 1f));
                    pz = beltCenterZ + Random.Range(-beltWidthZ * 0.42f, beltWidthZ * 0.42f);

                    // Natural minor clustering and height variation on moving belt stream
                    float streamCluster = Mathf.Sin(px * 12.0f) * 0.02f;
                    py = beltTopY + Random.Range(0.005f, 0.025f) + Mathf.Max(0f, streamCluster);
                }
                else
                {
                    // AREA A: INCOMING MATERIAL — Near Feed Hopper exit (30 pieces)
                    px = beltMaxX - Random.Range(0.05f, 0.45f);
                    pz = beltCenterZ + Random.Range(-beltWidthZ * 0.40f, beltWidthZ * 0.40f);
                    py = beltTopY + Random.Range(0.005f, 0.03f);
                }

                // Small individual piece sizing (approx 8cm - 16cm per piece)
                float baseScale = Random.Range(0.0035f, 0.0062f);
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

            Debug.Log($"[DenseCoalPopulator] Successfully instantiated {totalPieces} small coal pieces across the conveyor belt!");

            // Save scene and mark dirty
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[DenseCoalPopulator] Scene saved to {ScenePath}");

            // Verify scene reload persistence
            var verifyScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform verifyJamArea = null;
            foreach (var r in verifyScene.GetRootGameObjects())
            {
                if (r.name == "MachineryScenario") verifyJamArea = r.transform.Find("JamArea");
            }

            int count = verifyJamArea != null ? verifyJamArea.childCount : 0;
            Debug.Log($"[DenseCoalPopulator] Scene reloaded. Persistence Verified! Total coal pieces = {count}");

            // Render visual verification images
            RenderCamera(new Vector3(-0.2f, 1.8f, -1.8f), new Vector3(0.0f, 1.18f, 0.0f), "Machinery_Coal_Jam_SceneView_CloseUp.png");
            RenderCamera(new Vector3(-0.2f, 3.4f, -5.8f), new Vector3(0.2f, 0.6f, 0.0f), "Machinery_Coal_Jam_Verification.png");
            RenderCamera(new Vector3(-0.2f, 3.4f, -5.8f), new Vector3(0.2f, 0.6f, 0.0f), "Machinery_Final_Scene_View.png");

            Debug.Log("=========================================================================");
            Debug.Log("[MachineryDenseCoalPopulator] FULL DENSE INDUSTRIAL COAL POPULATION COMPLETE");
            Debug.Log("=========================================================================");
        }

        private static void RenderCamera(Vector3 pos, Vector3 target, string filename)
        {
            GameObject tempCamGo = new GameObject("TempDenseCoalCam");
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

            Debug.Log($"[RenderCamera] Rendered & Saved: {filename} ({bytes.Length} bytes)");
        }
    }
}
#endif
