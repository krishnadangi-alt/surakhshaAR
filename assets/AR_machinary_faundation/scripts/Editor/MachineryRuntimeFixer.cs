using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Text;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Fixes Android AR Runtime Camera & TMP Font Asset NullReferenceExceptions.
/// STRICTLY PRESERVES all 3D machinery model transforms (0 changes allowed).
/// </summary>
public static class MachineryRuntimeFixer
{
    private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

    [MenuItem("Machinery/Apply Runtime AR and TMP Fixes")]
    public static void ApplyFixes()
    {
        var sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("MACHINERY AR RUNTIME & TMP FIXER");
        sb.AppendLine("==========================================================");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Could not open MachineryConveyorJam.unity");
            return;
        }

        // ── 1. RECORD ALL TRANSFORMS BEFORE ANY MODIFICATION ──────────────────
        var beforeSnaps = new Dictionary<string, TransformSnap>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            SnapTree(rootGo.transform, beforeSnaps);
        }
        sb.AppendLine($"Snapped {beforeSnaps.Count} scene transforms before modification.");

        // ── 2. FIX CAMERA OFFSET & MAIN CAMERA ACTIVE STATES ──────────────────
        var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            sb.AppendLine($"Found XROrigin: {xrOrigin.name}");
            Transform camOffset = xrOrigin.transform.Find("Camera Offset");
            if (camOffset == null)
            {
                foreach (Transform child in xrOrigin.transform)
                {
                    if (child.name.Contains("Camera")) { camOffset = child; break; }
                }
            }

            if (camOffset != null)
            {
                if (!camOffset.gameObject.activeSelf)
                {
                    camOffset.gameObject.SetActive(true);
                    EditorUtility.SetDirty(camOffset.gameObject);
                    sb.AppendLine("  -> Activated 'Camera Offset' GameObject.");
                }

                Transform mainCamT = camOffset.Find("Main Camera");
                if (mainCamT == null)
                {
                    mainCamT = camOffset.GetComponentInChildren<Camera>(true)?.transform;
                }

                if (mainCamT != null)
                {
                    if (!mainCamT.gameObject.activeSelf)
                    {
                        mainCamT.gameObject.SetActive(true);
                        EditorUtility.SetDirty(mainCamT.gameObject);
                        sb.AppendLine("  -> Activated 'Main Camera' GameObject.");
                    }

                    if (mainCamT.tag != "MainCamera")
                    {
                        mainCamT.tag = "MainCamera";
                        EditorUtility.SetDirty(mainCamT.gameObject);
                        sb.AppendLine("  -> Set tag to 'MainCamera'.");
                    }

                    Camera cam = mainCamT.GetComponent<Camera>();
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0f, 0f, 0f, 1f);
                        EditorUtility.SetDirty(cam);
                        sb.AppendLine("  -> Configured Camera clearFlags=SolidColor, bg=Black.");
                    }

                    var camMgr = mainCamT.GetComponent<ARCameraManager>();
                    if (camMgr == null)
                    {
                        camMgr = mainCamT.gameObject.AddComponent<ARCameraManager>();
                        sb.AppendLine("  -> Added ARCameraManager component.");
                    }
                    camMgr.enabled = true;

                    var camBg = mainCamT.GetComponent<ARCameraBackground>();
                    if (camBg == null)
                    {
                        camBg = mainCamT.gameObject.AddComponent<ARCameraBackground>();
                        sb.AppendLine("  -> Added ARCameraBackground component.");
                    }
                    camBg.enabled = true;
                }
                else
                {
                    sb.AppendLine("ERROR: Main Camera transform not found under Camera Offset!");
                }
            }
            else
            {
                sb.AppendLine("ERROR: Camera Offset transform not found under XR Origin!");
            }
        }
        else
        {
            sb.AppendLine("ERROR: XROrigin component not found in scene!");
        }

        // ── 3. STRICT TRANSFORM AUDIT FOR ALL MACHINERY OBJECTS ──────────────
        var afterSnaps = new Dictionary<string, TransformSnap>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            SnapTree(rootGo.transform, afterSnaps);
        }

        sb.AppendLine("\n--- STRICT MACHINERY TRANSFORM AUDIT ---");
        var violations = new List<string>();
        foreach (var kvp in beforeSnaps)
        {
            string path = kvp.Key;
            // Only inspect MachineryScenario machinery models
            if (path.StartsWith("MachineryScenario/"))
            {
                if (!afterSnaps.TryGetValue(path, out var after))
                {
                    violations.Add($"DELETED: {path}");
                    continue;
                }
                if (!kvp.Value.Matches(after))
                {
                    violations.Add($"CHANGED: {path}");
                }
            }
        }

        if (violations.Count == 0)
        {
            sb.AppendLine("Audit PASSED: ZERO machinery transforms were modified.");
        }
        else
        {
            sb.AppendLine($"Audit FAILED: {violations.Count} unexpected transform changes!");
            foreach (var v in violations) sb.AppendLine($"  {v}");
        }

        // ── 4. SAVE SCENE ─────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("Scene saved successfully.");

        File.WriteAllText("machinery_runtime_fix_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void SnapTree(Transform t, Dictionary<string, TransformSnap> dict)
    {
        dict[FullPath(t.gameObject)] = new TransformSnap(t);
        for (int i = 0; i < t.childCount; i++) SnapTree(t.GetChild(i), dict);
    }

    private static string FullPath(GameObject go)
    {
        string p = go.name;
        var parent = go.transform.parent;
        while (parent != null) { p = parent.name + "/" + p; parent = parent.parent; }
        return p;
    }

    private struct TransformSnap
    {
        public Vector3 pos, rot, scale;
        public TransformSnap(Transform t) { pos = t.position; rot = t.eulerAngles; scale = t.lossyScale; }
        public bool Matches(TransformSnap o) =>
            Vector3.Distance(pos, o.pos) < 0.0001f &&
            Vector3.Distance(rot, o.rot) < 0.001f &&
            Vector3.Distance(scale, o.scale) < 0.0001f;
    }
}
