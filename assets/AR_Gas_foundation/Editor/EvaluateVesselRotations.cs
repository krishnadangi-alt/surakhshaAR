#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EvaluateVesselRotations
{
    [MenuItem("SurakshaAR/Evaluate Vessel Rotations")]
    public static void Evaluate()
    {
        Debug.Log("========================================================");
        Debug.Log("EVALUATING VESSEL ROTATIONS & GROUND PLACEMENT");
        Debug.Log("========================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) return;

        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null) return;

        // Ground reference Y from Realistic_Gas_Cylinder base
        float groundY = 0.00f;
        Transform cylinder = gasRoot.transform.Find("Gas_Leak_Station/Realistic_Gas_Cylinder");
        if (cylinder != null)
        {
            Renderer cr = cylinder.GetComponentInChildren<Renderer>();
            if (cr != null) groundY = cr.bounds.min.y;
        }
        Debug.Log($"SCENE GROUND LEVEL REFERENCE Y = {groundY:F4}");

        Transform confinedArea = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (confinedArea == null)
        {
            confinedArea = new GameObject("Confined_Space_Training_Area").transform;
            confinedArea.SetParent(gasRoot.transform, false);
        }
        confinedArea.localPosition = new Vector3(3.00f, 0.00f, 0.00f);

        // Remove any loose/floating extra objects so ONLY the 1 complete assembly exists
        for (int i = confinedArea.childCount - 1; i >= 0; i--)
        {
            Transform child = confinedArea.GetChild(i);
            if (child.name != "Real_Confined_Space_Model")
            {
                Debug.Log($"Removing loose/extra object: {child.name}");
                Object.DestroyImmediate(child.gameObject);
            }
        }

        Transform vessel = confinedArea.Find("Real_Confined_Space_Model");
        if (vessel == null)
        {
            string fbxPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, confinedArea);
            inst.name = "Real_Confined_Space_Model";
            vessel = inst.transform;
        }

        // Apply GLB PBR material to vessel
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard");
        Material tanksMat = new Material(urpLit);
        tanksMat.name = "MAT_IndustrialTanks_OriginalGLB";
        Texture2D baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_0.jpg");
        Texture2D metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_1.png");
        Texture2D normTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/ConfinedSpace/Textures_Tanks/texture_2.jpg");
        if (baseTex != null) tanksMat.SetTexture("_BaseMap", baseTex);
        if (metalTex != null) tanksMat.SetTexture("_MetallicGlossMap", metalTex);
        if (normTex != null)
        {
            tanksMat.SetTexture("_BumpMap", normTex);
            tanksMat.EnableKeyword("_NORMALMAP");
        }
        tanksMat.SetFloat("_Metallic", 0.75f);
        tanksMat.SetFloat("_Smoothness", 0.65f);

        Renderer[] rends = vessel.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends) r.sharedMaterial = tanksMat;

        string outDir = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb\VesselEval";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        Vector3[] rotCandidates = new Vector3[]
        {
            new Vector3(0f, 0f, 90f),
            new Vector3(0f, 90f, 90f),
            new Vector3(0f, 180f, 90f),
            new Vector3(0f, -90f, 90f),
            new Vector3(90f, 0f, 0f),
            new Vector3(-90f, 0f, 0f),
            new Vector3(0f, 0f, 0f)
        };

        for (int idx = 0; idx < rotCandidates.Length; idx++)
        {
            Vector3 rot = rotCandidates[idx];
            vessel.localRotation = Quaternion.Euler(rot);

            // Calculate combined bounds before scale
            Bounds b = rends[0].bounds;
            for (int k = 1; k < rends.Length; k++) b.Encapsulate(rends[k].bounds);

            // Set uniform scale so max dimension (height) is 2.80m (realistic industrial tank size, larger than gas cylinder 1.20m)
            float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            float targetHeight = 2.80f;
            float uniformScale = targetHeight / maxDim;
            vessel.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

            // Snap lowest renderer Y of COMPLETE assembly to groundY
            float lowestY = float.MaxValue;
            foreach (var r in rends)
            {
                if (r.bounds.min.y < lowestY) lowestY = r.bounds.min.y;
            }
            float deltaY = groundY - lowestY;
            vessel.position += new Vector3(0f, deltaY, 0f);

            // Re-calculate bounds after snapping
            b = rends[0].bounds;
            for (int k = 1; k < rends.Length; k++) b.Encapsulate(rends[k].bounds);

            Debug.Log($"Candidate {idx} [Rot {rot}] -> Final Bounds: Size ({b.size.x:F2}, {b.size.y:F2}, {b.size.z:F2}), Min Y: {b.min.y:F4}, Max Y: {b.max.y:F4}");

            // Render Front View
            string frontPath = Path.Combine(outDir, $"Vessel_Cand_{idx}_Rot_{rot.x}_{rot.y}_{rot.z}_FRONT.png");
            CaptureCamera(confinedArea.position + new Vector3(0f, 1.4f, -4.0f), Quaternion.Euler(10f, 0f, 0f), 45f, 1280, 720, frontPath);

            // Render Side View
            string sidePath = Path.Combine(outDir, $"Vessel_Cand_{idx}_Rot_{rot.x}_{rot.y}_{rot.z}_SIDE.png");
            CaptureCamera(confinedArea.position + new Vector3(4.0f, 1.4f, 0f), Quaternion.Euler(10f, -90f, 0f), 45f, 1280, 720, sidePath);
        }

        Debug.Log("========================================================");
    }

    private static void CaptureCamera(Vector3 pos, Quaternion rot, float fov, int width, int height, string filePath)
    {
        GameObject camObj = new GameObject("EvalCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        camObj.transform.position = pos;
        camObj.transform.rotation = rot;

        GameObject keyLight = new GameObject("KeyLight");
        Light kl = keyLight.AddComponent<Light>();
        kl.type = LightType.Directional;
        kl.intensity = 0.9f;
        keyLight.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.5f;
        fillLight.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);

        Object.DestroyImmediate(keyLight);
        Object.DestroyImmediate(fillLight);
        Object.DestroyImmediate(camObj);
    }
}
#endif
