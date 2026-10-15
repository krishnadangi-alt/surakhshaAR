using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Triggered on Unity startup to open the correct scene and run final verification.
/// Menu: SurakshaAR > Machinery > Open Machinery Scene + Verify
/// </summary>
[InitializeOnLoad]
public class MachinerySceneOpener
{
    static MachinerySceneOpener()
    {
        // Only run once after domain reload
        EditorApplication.delayCall += OnStartup;
    }

    static void OnStartup()
    {
        EditorApplication.delayCall -= OnStartup;

        // Check if MachineryConveyorJam is already open
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name.Contains("MachineryConveyorJam"))
        {
            Debug.Log("MachineryConveyorJam scene is already open: " + scene.path);
            VerifyAndLog();
        }
    }

    [MenuItem("SurakshaAR/Machinery/Open Machinery Scene + Verify")]
    public static void OpenAndVerify()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("Scene opened: " + scenePath);
        VerifyAndLog();
    }

    [MenuItem("SurakshaAR/Machinery/Verify Current Positions")]
    public static void VerifyAndLog()
    {
        Debug.Log("=== MACHINERY SCENE POSITION VERIFICATION ===");

        string[] keyNames = {
            "ConveyorAssembly", "FeedHopper", "DriveMotor", "JamArea",
            "ControlPanel", "EmergencyStop", "MainIsolator",
            "SteelWorkstationTable", "Hasp", "PadlockAndTag"
        };

        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var key in keyNames)
        {
            foreach (var go in allGOs)
            {
                if (!go.scene.IsValid()) continue;
                if (go.name.Contains(key))
                {
                    var renderers = go.GetComponentsInChildren<Renderer>(true);
                    string parentName = go.transform.parent != null ? go.transform.parent.name : "ROOT";

                    // Only log objects that are direct children of MachineryScenario or roots
                    if (parentName == "MachineryScenario" || parentName == "ROOT")
                    {
                        Bounds? bounds = null;
                        if (renderers.Length > 0)
                        {
                            var b = renderers[0].bounds;
                            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                            bounds = b;
                        }

                        string boundsStr = bounds.HasValue
                            ? string.Format("world_bounds: min={0:F2} max={1:F2}", bounds.Value.min, bounds.Value.max)
                            : "no renderers";

                        Debug.Log(string.Format(
                            "[{0}] parent={1} | active={2} | world_pos={3:F3} | local_pos={4:F3} | {5}",
                            go.name, parentName, go.activeInHierarchy,
                            go.transform.position, go.transform.localPosition, boundsStr));
                    }
                }
            }
        }

        // Frame scene view
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.pivot = new Vector3(0, 1.5f, 0);
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(30, -30, 0);
            SceneView.lastActiveSceneView.size = 8f;
            SceneView.RepaintAll();
        }

        Debug.Log("=== VERIFICATION COMPLETE ===");
    }

    [MenuItem("SurakshaAR/Machinery/Frame Scene View")]
    public static void FrameSceneView()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.pivot = new Vector3(0, 1.5f, 0);
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(30, -30, 0);
            SceneView.lastActiveSceneView.size = 8f;
            SceneView.RepaintAll();
            Debug.Log("Scene view framed from worker perspective");
        }
    }
}
