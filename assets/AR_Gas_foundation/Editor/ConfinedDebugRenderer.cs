#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class ConfinedDebugRenderer
{
    [MenuItem("SurakshaAR/Render Confined Space Debug Objects")]
    public static void RenderDebugObjects()
    {
        GameObject glbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb");
        if (glbPrefab == null)
        {
            Debug.LogError("Could not load industrial_tanks_and_equipment.glb");
            return;
        }

        string outputDir = Path.Combine(Application.dataPath, "AR_Gas_foundation/Validation");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        // Create diagnostic stage
        GameObject stage = new GameObject("DebugStage");
        stage.transform.position = Vector3.zero;
        stage.transform.rotation = Quaternion.identity;

        GameObject camObj = new GameObject("DebugCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 50f;
        cam.fieldOfView = 40f;

        GameObject lightObj = new GameObject("DebugLight");
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.35f;
        lightObj.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        GameObject inst = Object.Instantiate(glbPrefab, stage.transform, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        Transform node0 = inst.transform.Find("root/GLTF_SceneRootNode/_0");
        if (node0 != null)
        {
            for (int i = 0; i < node0.childCount; i++)
            {
                Transform child = node0.GetChild(i);
                string objName = child.name;

                // Disable all children first
                for (int j = 0; j < node0.childCount; j++)
                {
                    node0.GetChild(j).gameObject.SetActive(false);
                }

                // Enable only current child
                child.gameObject.SetActive(true);

                Renderer r = child.GetComponent<Renderer>();
                if (r == null) continue;

                Bounds b = r.bounds;
                Vector3 center = b.center;
                float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                if (maxDim < 0.001f) maxDim = 1.0f;

                // Frame camera on single object
                camObj.transform.position = center + new Vector3(0f, maxDim * 0.3f, -maxDim * 2.2f);
                camObj.transform.LookAt(center);

                string outPath = Path.Combine(outputDir, $"CONFINE_DEBUG_{objName.ToUpper()}.png");
                RenderCameraToPNG(cam, outPath, 800, 600);
                Debug.Log($"Rendered debug screenshot for {objName}: {outPath} (Bounds: Size={b.size}, Center={b.center})");
            }
        }

        Object.DestroyImmediate(stage);
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(lightObj);
    }

    private static void RenderCameraToPNG(Camera cam, string filePath, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Object.DestroyImmediate(screenShot);
    }
}
#endif
