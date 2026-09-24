#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GasRuntimeDiagnostic
{
    [MenuItem("SurakshaAR/Run Gas Runtime Diagnostic")]
    public static void RunDiagnostic()
    {
        Debug.Log("==================================================");
        Debug.Log("[GAS RUNTIME DIAGNOSTIC START]");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            Debug.LogError("[DIAGNOSTIC] GasScenarioRoot NOT found in scene!");
            return;
        }

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder != null)
        {
            builder.BuildEnvironment();
        }

        // 1. INSPECT WORKBENCH PREFAB & RENDERERS
        GameObject wbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/workbench.prefab");
        if (wbPrefab != null)
        {
            Renderer[] wbRends = wbPrefab.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[DIAGNOSTIC] Workbench Prefab found with {wbRends.Length} renderers.");
            foreach (var r in wbRends)
            {
                Debug.Log($"  Renderer: {r.name}, Local Bounds: {r.bounds.size}, SharedMaterials: {r.sharedMaterials.Length}");
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null)
                    {
                        Color c = mat.HasProperty("_Color") ? mat.color : (mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.black);
                        Debug.Log($"    Mat: {mat.name}, Shader: {mat.shader.name}, Color: {c}");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[DIAGNOSTIC] Workbench Prefab NOT found at Assets/AR_Gas_foundation/3d/PPE/workbench.prefab");
        }

        // 2. INSPECT SCENE WORKSPACE OBJECTS
        GameObject floorObj = GameObject.Find("Industrial_Concrete_Floor");
        if (floorObj != null)
        {
            Renderer r = floorObj.GetComponent<Renderer>();
            Debug.Log($"[DIAGNOSTIC] Floor Obj: {floorObj.name}, LocalScale: {floorObj.transform.localScale}, World Bounds: {r.bounds.size}");
            if (r.sharedMaterial != null)
            {
                Color c = r.sharedMaterial.HasProperty("_Color") ? r.sharedMaterial.color : Color.black;
                Debug.Log($"  Floor Material: {r.sharedMaterial.name}, Color: {c}");
            }
        }

        GameObject ppeWb = GameObject.Find("PPE_Workbench");
        if (ppeWb != null)
        {
            Renderer[] rends = ppeWb.GetComponentsInChildren<Renderer>(true);
            Bounds b = rends.Length > 0 ? rends[0].bounds : new Bounds();
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            Debug.Log($"[DIAGNOSTIC] PPE_Workbench in scene: LocalScale: {ppeWb.transform.localScale}, World Bounds: {b.size}, Center: {b.center}");
            foreach (var rend in rends)
            {
                foreach (var mat in rend.sharedMaterials)
                {
                    if (mat != null)
                    {
                        Color c = mat.HasProperty("_Color") ? mat.color : (mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.black);
                        Debug.Log($"  Workbench Sub-Mesh Material: {mat.name}, Color: {c}");
                    }
                }
            }
        }

        // 3. INSPECT UI CANVAS AND TEXT ELEMENTS
        GasTrainingStepController stepController = root.GetComponent<GasTrainingStepController>();
        if (stepController != null)
        {
            stepController.SetStep(GasTrainingStepController.TrainingStep.Intro);
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[DIAGNOSTIC] Canvas found: {canvas.name}, RenderMode: {canvas.renderMode}");
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"  CanvasScaler: Mode={scaler.uiScaleMode}, RefRes={scaler.referenceResolution}");
            }

            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                RectTransform rt = t.rectTransform;
                Debug.Log($"  TMP Text [{t.gameObject.name}]: text='{t.text}', fontSize={t.fontSize}, rect={rt.rect}, sizeDelta={rt.sizeDelta}, anchors=({rt.anchorMin.x},{rt.anchorMin.y})-({rt.anchorMax.x},{rt.anchorMax.y}), wrapping={t.textWrappingMode}");
            }
        }
        else
        {
            Debug.LogWarning("[DIAGNOSTIC] Canvas NOT found in scene!");
        }

        Debug.Log("==================================================");
        Debug.Log("[GAS RUNTIME DIAGNOSTIC END]");
        Debug.Log("==================================================");
    }
}
#endif
