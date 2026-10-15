using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public static class MachineryLotoTableFixer
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
    private const string OutputFolder = "Builds/Verification/Machinery/LOTO_FIX";

    [MenuItem("Machinery/Execute LOTO Table Fix Only")]
    public static void RunLotoFix()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene: {ScenePath}");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("STRICT LOTO TABLE-ONLY FIX EXECUTION");
        sb.AppendLine("==========================================================");

        // 1. RECORD ALL OBJECT TRANSFORMS IN SCENE FOR STRICT AUDIT
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        Dictionary<string, TransformState> initialStates = new Dictionary<string, TransformState>();

        foreach (var t in allTransforms)
        {
            string path = GetFullPath(t.gameObject);
            initialStates[path] = new TransformState(t);
        }

        sb.AppendLine($"Recorded {initialStates.Count} object states before modification.");

        // 2. FIND TARGET OBJECTS
        GameObject table = GameObject.Find("SteelWorkstationTable");
        GameObject hasp = GameObject.Find("Hasp");
        GameObject padlock = GameObject.Find("PadlockAndTag");

        if (table == null || hasp == null || padlock == null)
        {
            Debug.LogError($"Target object missing! Table: {table != null}, Hasp: {hasp != null}, Padlock: {padlock != null}");
            return;
        }

        // Calculate Tabletop Surface Height
        MeshRenderer[] tableRenderers = table.GetComponentsInChildren<MeshRenderer>(true);
        Bounds tableBounds = new Bounds();
        bool first = true;
        foreach (var mr in tableRenderers)
        {
            if (mr.enabled && mr.gameObject.activeInHierarchy)
            {
                // Exclude Hasp and PadlockAndTag renderers from table bounds calculation
                if (mr.transform.IsChildOf(hasp.transform) || mr.transform.IsChildOf(padlock.transform))
                    continue;

                if (first) { tableBounds = mr.bounds; first = false; }
                else { tableBounds.Encapsulate(mr.bounds); }
            }
        }

        float tabletopSurfaceY = tableBounds.max.y; // 0.5974m
        sb.AppendLine($"Calculated Tabletop Surface Top Y = {tabletopSurfaceY:F4}m");

        // 3. ADJUST HASP TO SIT ON TABLETOP
        // Hasp target world position: X = 3.8500m, Z = -1.3500m
        hasp.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        hasp.transform.position = new Vector3(3.8500f, tabletopSurfaceY + 0.0500f, -1.3500f);
        Bounds haspBounds = GetCombinedBounds(hasp);
        float haspMinYDelta = tabletopSurfaceY - haspBounds.min.y;
        hasp.transform.position += new Vector3(0f, haspMinYDelta + 0.002f, 0f);

        // 4. ADJUST PADLOCK AND TAG TO SIT ON TABLETOP
        // Padlock target world position: X = 4.1500m, Z = -1.3500m
        padlock.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        padlock.transform.position = new Vector3(4.1500f, tabletopSurfaceY + 0.0500f, -1.3500f);
        Bounds padlockBounds = GetCombinedBounds(padlock);
        float padlockMinYDelta = tabletopSurfaceY - padlockBounds.min.y;
        padlock.transform.position += new Vector3(0f, padlockMinYDelta + 0.002f, 0f);

        EditorUtility.SetDirty(hasp);
        EditorUtility.SetDirty(padlock);

        // Final Bounds Verification
        haspBounds = GetCombinedBounds(hasp);
        padlockBounds = GetCombinedBounds(padlock);

        sb.AppendLine($"\n--- FINAL HASP BOUNDS ---");
        sb.AppendLine($"  World Pos: {hasp.transform.position.ToString("F4")}");
        sb.AppendLine($"  Bounds Center: {haspBounds.center.ToString("F4")}, Size: {haspBounds.size.ToString("F4")}");
        sb.AppendLine($"  MinY: {haspBounds.min.y:F4}m, MaxY: {haspBounds.max.y:F4}m (Delta from Tabletop: {haspBounds.min.y - tabletopSurfaceY:F4}m)");

        sb.AppendLine($"\n--- FINAL PADLOCK AND TAG BOUNDS ---");
        sb.AppendLine($"  World Pos: {padlock.transform.position.ToString("F4")}");
        sb.AppendLine($"  Bounds Center: {padlockBounds.center.ToString("F4")}, Size: {padlockBounds.size.ToString("F4")}");
        sb.AppendLine($"  MinY: {padlockBounds.min.y:F4}m, MaxY: {padlockBounds.max.y:F4}m (Delta from Tabletop: {padlockBounds.min.y - tabletopSurfaceY:F4}m)");

        // 5. AUDIT ALL OTHER TRANSFORMS (EXCLUDING HASP & PADLOCK BRANCHES)
        sb.AppendLine("\n--- STRICT AUDIT OF ALL OTHER SCENE TRANSFORMS ---");
        bool extraChanged = false;
        foreach (var kvp in initialStates)
        {
            if (kvp.Key.Contains("/Hasp") || kvp.Key.Contains("/PadlockAndTag") || kvp.Key.EndsWith("/Hasp") || kvp.Key.EndsWith("/PadlockAndTag"))
            {
                continue; // Allowed LOTO targets
            }

            GameObject go = GameObject.Find(kvp.Key);
            if (go != null)
            {
                TransformState current = new TransformState(go.transform);
                if (!kvp.Value.Matches(current))
                {
                    extraChanged = true;
                    sb.AppendLine($"[CRITICAL ERROR] Object '{kvp.Key}' transform UNINTENTIONALLY changed!");
                }
            }
        }

        sb.AppendLine($"Strict Audit Result: {(extraChanged ? "FAILED - Extra objects changed!" : "PASSED - ONLY Hasp and PadlockAndTag changed!")}");

        // 6. SAVE SCENE
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("Scene saved successfully.");

        // 7. RENDER VERIFICATION SCREENSHOTS
        RenderLotoScreenshots();

        File.WriteAllText("loto_table_fix_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static Bounds GetCombinedBounds(GameObject go)
    {
        MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>(true);
        Bounds b = new Bounds();
        bool first = true;
        foreach (var mr in mrs)
        {
            if (mr.enabled && mr.gameObject.activeInHierarchy)
            {
                if (first) { b = mr.bounds; first = false; }
                else { b.Encapsulate(mr.bounds); }
            }
        }
        return b;
    }

    private static void RenderLotoScreenshots()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo == null) camGo = GameObject.Find("Main Camera");
        Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;

        GameObject tempCamGo = new GameObject("TempLotoCam");
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
            Debug.Log($"Rendered LOTO screenshot: {savePath}");
        }

        // LOTO Table Overview
        Capture(new Vector3(4.0f, 1.4f, -2.5f), new Vector3(4.0f, 0.6f, -1.35f), "LOTO_Table_Overview.png");

        // LOTO Table Close-up
        Capture(new Vector3(4.0f, 1.0f, -1.9f), new Vector3(4.0f, 0.61f, -1.35f), "LOTO_Table_CloseUp.png");

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(tempCamGo);
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    private struct TransformState
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public TransformState(Transform t)
        {
            position = t.position;
            rotation = t.eulerAngles;
            scale = t.lossyScale;
        }

        public bool Matches(TransformState other)
        {
            return Vector3.Distance(position, other.position) < 0.001f &&
                   Vector3.Distance(rotation, other.rotation) < 0.01f &&
                   Vector3.Distance(scale, other.scale) < 0.001f;
        }
    }
}
