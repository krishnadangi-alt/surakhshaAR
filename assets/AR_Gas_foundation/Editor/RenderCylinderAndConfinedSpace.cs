#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RenderCylinderAndConfinedSpace
{
    [MenuItem("SurakshaAR/Render Focused Cylinder and Confined Space")]
    public static void RenderFocusedView()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for focused screenshot: " + scene.path);

        int w = 1920, h = 1080;
        GameObject camObj = new GameObject("FocusedCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = 52f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;

        // Position camera to frame both RED GAS CYLINDER (X=-0.15, Y=0, H=1.2m) and CONFINED SPACE MODEL (X=2.6, Y=0, H=3.2m)
        cam.transform.position = new Vector3(1.15f, 1.65f, -4.80f);
        cam.transform.rotation = Quaternion.Euler(11f, 0f, 0f);

        GameObject keyLight = new GameObject("KeyLight");
        Light kl = keyLight.AddComponent<Light>();
        kl.type = LightType.Directional;
        kl.intensity = 1.25f;
        kl.color = new Color(1.0f, 0.96f, 0.90f);
        keyLight.transform.rotation = Quaternion.Euler(38f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.65f;
        fl.color = new Color(0.85f, 0.90f, 1.0f);
        fillLight.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

        RenderTexture rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();

        string dir1 = @"C:\SurakshaAR\Screenshots";
        string dir2 = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb";
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

        File.WriteAllBytes(Path.Combine(dir1, "RED_CYLINDER_AND_CONFINED_SPACE_FOCUSED.png"), bytes);
        File.WriteAllBytes(Path.Combine(dir2, "RED_CYLINDER_AND_CONFINED_SPACE_FOCUSED.png"), bytes);
        File.WriteAllBytes(Path.Combine(dir1, "Gas_Workstation_FULL.png"), bytes);
        File.WriteAllBytes(Path.Combine(dir2, "Gas_Workstation_FULL.png"), bytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);

        Object.DestroyImmediate(keyLight);
        Object.DestroyImmediate(fillLight);
        Object.DestroyImmediate(camObj);

        Debug.Log("[FOCUSED RENDER DONE] Saved focused view screenshot to " + dir1 + " and " + dir2);
    }
}
#endif
