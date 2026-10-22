using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Deep inspects all key machinery objects - world positions, bounds, active state.
/// Menu: SurakshaAR > Machinery > Deep Inspect Scene
/// </summary>
public class MachineryDeepInspector : EditorWindow
{
    [MenuItem("SurakshaAR/Machinery/Deep Inspect Scene")]
    public static void DeepInspect()
    {
        Debug.Log("=== MACHINERY DEEP INSPECTION ===");

        string[] keyNames = {
            "MachineryScenario", "ConveyorAssembly", "FeedHopper", "DriveMotor",
            "JamArea", "ControlPanel", "EmergencyStop", "MainIsolator",
            "SteelWorkstation", "Hasp", "PadlockAnd", "MotorBase", "BeltCoal", "HopperCoal"
        };

        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allGOs)
        {
            if (!go.scene.IsValid()) continue;
            string goName = go.name;
            foreach (var key in keyNames)
            {
                if (goName.Contains(key))
                {
                    var renderers = go.GetComponentsInChildren<Renderer>(true);
                    Bounds? bounds = null;
                    if (renderers.Length > 0)
                    {
                        var b = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                        bounds = b;
                    }

                    string parentName = go.transform.parent != null ? go.transform.parent.name : "NONE";
                    string boundsStr = bounds.HasValue
                        ? string.Format("bounds_min={0:F2} max={1:F2} size={2:F2}", bounds.Value.min, bounds.Value.max, bounds.Value.size)
                        : "NO RENDERERS";

                    Debug.Log(string.Format(
                        "[{0}] parent={1} | active={2} | wpos={3:F3} | lpos={4:F3} | {5} | renderers={6}",
                        goName, parentName, go.activeInHierarchy,
                        go.transform.position, go.transform.localPosition,
                        boundsStr, renderers.Length));
                    break;
                }
            }
        }

        Debug.Log("=== INSPECTION COMPLETE ===");
    }

    [MenuItem("SurakshaAR/Machinery/List All Scene Roots")]
    public static void ListRoots()
    {
        Debug.Log("=== SCENE ROOT OBJECTS ===");
        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allGOs)
        {
            if (go.scene.IsValid() && go.scene.name != "" && go.transform.parent == null)
            {
                int childCount = go.transform.childCount;
                Debug.Log("[ROOT] " + go.name + " | active=" + go.activeSelf + " | children=" + childCount + " | pos=" + go.transform.position);
                // List direct children
                foreach (Transform child in go.transform)
                {
                    Debug.Log("  [CHILD] " + child.name + " | active=" + child.gameObject.activeSelf + " | lpos=" + child.localPosition + " | wpos=" + child.position);
                }
            }
        }
        Debug.Log("=== END ROOTS ===");
    }
}
