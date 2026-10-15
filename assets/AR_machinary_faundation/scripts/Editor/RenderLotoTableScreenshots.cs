using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class RenderLotoTableScreenshots
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
    private const string OutputFolder = "Builds/Verification/Machinery";

    [MenuItem("Machinery/Render LOTO Table Screenshots")]
    public static void RenderScreenshots()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) return;

        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo == null) camGo = GameObject.Find("Main Camera");
        Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;

        GameObject tempCamGo = new GameObject("TempLotoTableCam");
        Camera tempCam = tempCamGo.AddComponent<Camera>();
        if (mainCam != null) tempCam.CopyFrom(mainCam);

        tempCam.clearFlags = CameraClearFlags.SolidColor;
        tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        tempCam.nearClipPlane = 0.05f;
        tempCam.farClipPlane = 100f;

        int width = 1280;
        int height = 720;
        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        void Capture(Vector3 eyePos, Vector3 lookAt, string filename)
        {
            tempCam.transform.position = eyePos;
            tempCam.transform.LookAt(lookAt);
            tempCam.targetTexture = rt;
            tempCam.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            string savePath = Path.Combine(OutputFolder, filename);
            File.WriteAllBytes(savePath, bytes);
            Debug.Log($"Rendered screenshot: {savePath}");
        }

        // LOTO_Table_CloseUp.png
        Capture(new Vector3(3.95f, 1.25f, -2.45f), new Vector3(3.95f, 0.65f, -1.35f), "LOTO_Table_CloseUp.png");

        // LOTO_Table_Overview.png
        Capture(new Vector3(4.80f, 1.60f, -2.80f), new Vector3(3.50f, 0.60f, -0.80f), "LOTO_Table_Overview.png");

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(tempCamGo);
    }
}
