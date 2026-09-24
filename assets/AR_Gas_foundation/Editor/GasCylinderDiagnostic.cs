#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;

public class GasCylinderDiagnostic
{
    public static void RunDiagnostic()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine("GAS MODULE ASSETS & SCENE DEEP DIAGNOSTIC");
        sb.AppendLine("==================================================");

        // 1. Gas Cylinder GLB
        InspectAsset("Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb", sb);
        InspectAsset("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab", sb);

        // 2. Gas Detector GLB & Prefab
        InspectAsset("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb", sb);
        InspectAsset("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab", sb);

        // 3. Pipeline Valve FBX
        InspectAsset("Assets/industrial-valve/source/VALVE.fbx", sb);

        // 4. Scene Inspection
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        sb.AppendLine($"\nOpened Scene: {scenePath}");
        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root != null)
        {
            sb.AppendLine($"Found GasScenarioRoot at World Pos: {root.transform.position.ToString("F4")}, Rot: {root.transform.eulerAngles.ToString("F4")}");
            sb.AppendLine($"Child count: {root.transform.childCount}");
            for (int i = 0; i < root.transform.childCount; i++)
            {
                Transform child = root.transform.GetChild(i);
                sb.AppendLine($"  - Child [{i}]: '{child.name}', LocalPos={child.localPosition.ToString("F4")}, LocalRot={child.localEulerAngles.ToString("F4")}, LocalScale={child.localScale.ToString("F4")}");
            }
        }
        else
        {
            sb.AppendLine("GasScenarioRoot NOT found in scene!");
        }

        sb.AppendLine("==================================================");
        File.WriteAllText("C:/SurakhshaAR/glb_info.txt", sb.ToString());
        Debug.Log("[GasCylinderDiagnostic] Diagnostic output written to C:/SurakhshaAR/glb_info.txt");
    }

    private static void InspectAsset(string assetPath, StringBuilder sb)
    {
        sb.AppendLine($"\n--- INSPECTING: {assetPath} ---");
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            sb.AppendLine($"ERROR: Asset NOT found at {assetPath}");
            return;
        }

        sb.AppendLine($"Asset Loaded: {asset.name}");
        GameObject instance = Object.Instantiate(asset);
        instance.name = "Diagnostic_Instance";

        MeshFilter[] mfs = instance.GetComponentsInChildren<MeshFilter>(true);
        sb.AppendLine($"Found {mfs.Length} MeshFilter(s):");
        foreach (var mf in mfs)
        {
            sb.AppendLine($"  - MeshFilter GO: '{mf.gameObject.name}', Active={mf.gameObject.activeInHierarchy}, LocalPos={mf.transform.localPosition.ToString("F4")}, LocalRot={mf.transform.localEulerAngles.ToString("F4")}, LocalScale={mf.transform.localScale.ToString("F4")}");
            if (mf.sharedMesh != null)
            {
                Mesh m = mf.sharedMesh;
                sb.AppendLine($"    Mesh Name: '{m.name}'");
                sb.AppendLine($"    Vertex Count: {m.vertexCount}");
                sb.AppendLine($"    Mesh Bounds Center: {m.bounds.center.ToString("F4")}, Size: {m.bounds.size.ToString("F4")}");
            }
        }

        Renderer[] rnds = instance.GetComponentsInChildren<Renderer>(true);
        sb.AppendLine($"Found {rnds.Length} Renderer(s):");
        Bounds totalBounds = new Bounds();
        bool first = true;
        foreach (var r in rnds)
        {
            sb.AppendLine($"  - Renderer GO: '{r.gameObject.name}', Active={r.gameObject.activeInHierarchy}, Enabled={r.enabled}");
            sb.AppendLine($"    Local Bounds Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}");
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null)
                {
                    sb.AppendLine($"    Material Name: '{mat.name}', Shader: '{mat.shader?.name}'");
                    if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null)
                        sb.AppendLine($"      _BaseMap Texture: '{mat.GetTexture("_BaseMap").name}'");
                    if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null)
                        sb.AppendLine($"      _MainTex Texture: '{mat.GetTexture("_MainTex").name}'");
                    if (mat.HasProperty("_BaseColor"))
                        sb.AppendLine($"      _BaseColor: {mat.GetColor("_BaseColor")}");
                    if (mat.HasProperty("_Color"))
                        sb.AppendLine($"      _Color: {mat.GetColor("_Color")}");
                }
                else
                {
                    sb.AppendLine("    Material: NULL (MISSING MATERIAL!)");
                }
            }
            if (first) { totalBounds = r.bounds; first = false; }
            else { totalBounds.Encapsulate(r.bounds); }
        }
        sb.AppendLine($"Total Encapsulated Bounds Size: {totalBounds.size.ToString("F4")}, Center: {totalBounds.center.ToString("F4")}");

        Object.DestroyImmediate(instance);
    }
}
#endif

