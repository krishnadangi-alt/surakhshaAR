#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class TestVesselOrientations
{
    [MenuItem("SurakshaAR/Test Vessel Orientations")]
    public static void TestOrientations()
    {
        Debug.Log("========================================================");
        Debug.Log("TESTING CONFINED SPACE VESSEL UPRIGHT ROTATIONS");
        Debug.Log("========================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) return;

        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null) return;

        Transform confinedStation = gasRoot.transform.Find("Confined_Space_Training_Area");
        if (confinedStation == null) return;

        Transform vessel = confinedStation.Find("Real_Confined_Space_Model");
        if (vessel == null) return;

        string outDir = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb\OrientationTests";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        Vector3[] rotations = new Vector3[]
        {
            new Vector3(0f, 0f, 90f),
            new Vector3(0f, 0f, -90f),
            new Vector3(90f, 0f, 0f),
            new Vector3(-90f, 0f, 0f),
            new Vector3(0f, 90f, 90f),
            new Vector3(0f, -90f, 90f),
            new Vector3(90f, 90f, 0f),
            new Vector3(0f, 0f, 0f)
        };

        Vector3 origPos = vessel.localPosition;

        for (int i = 0; i < rotations.Length; i++)
        {
            vessel.localRotation = Quaternion.Euler(rotations[i]);
            vessel.transform.localScale = Vector3.one;

            Renderer[] rends = vessel.GetComponentsInChildren<Renderer>(true);
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int k = 1; k < rends.Length; k++) b.Encapsulate(rends[k].bounds);

                float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                float scale = 3.20f / maxDim;
                vessel.transform.localScale = new Vector3(scale, scale, scale);

                float lowestY = float.MaxValue;
                foreach (var r in rends)
                {
                    Vector3 minLocal = confinedStation.InverseTransformPoint(r.bounds.min);
                    if (minLocal.y < lowestY) lowestY = minLocal.y;
                }
                if (lowestY != float.MaxValue)
                {
                    vessel.localPosition = new Vector3(0f, -lowestY, 0f);
                }

                b = rends[0].bounds;
                for (int k = 1; k < rends.Length; k++) b.Encapsulate(rends[k].bounds);
                Debug.Log(string.Format("Test {0} [Rot {1}] -> Bounds Size: ({2:F2}, {3:F2}, {4:F2})", i, rotations[i], b.size.x, b.size.y, b.size.z));
            }

            string savePath = Path.Combine(outDir, string.Format("Vessel_Rot_{0}_({1}_{2}_{3}).png", i, (int)rotations[i].x, (int)rotations[i].y, (int)rotations[i].z));
            CaptureCameraFrame(confinedStation.position + new Vector3(0f, 1.6f, -3.6f), Quaternion.Euler(12f, 0f, 0f), 45f, 1280, 720, savePath);
        }

        vessel.localPosition = origPos;
        Debug.Log("========================================================");
    }

    private static void CaptureCameraFrame(Vector3 pos, Quaternion rot, float fov, int width, int height, string filePath)
    {
        GameObject camObj = new GameObject("RenderCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        camObj.transform.position = pos;
        camObj.transform.rotation = rot;

        GameObject keyLightObj = new GameObject("KeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 0.85f;
        keyLight.color = new Color(1.0f, 0.96f, 0.90f);
        keyLightObj.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

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

        Object.DestroyImmediate(keyLightObj);
        Object.DestroyImmediate(camObj);
    }
}
#endif
