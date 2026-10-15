using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// MachineryFinalMasterFixer - Repositions all machinery to match the reference image.
/// Menu: SurakshaAR > Machinery > Final Master Fix
/// </summary>
public class MachineryFinalMasterFixer : EditorWindow
{
    [MenuItem("SurakshaAR/Machinery/Final Master Fix - Run Now")]
    public static void RunMasterFix()
    {
        Debug.Log("=== MACHINERY FINAL MASTER FIXER STARTED ===");
        var fixer = new MachineryFinalMasterFixer();
        fixer.Execute();
    }

    [MenuItem("SurakshaAR/Machinery/Inspect Scene Objects")]
    public static void InspectScene()
    {
        Debug.Log("=== SCENE INSPECTION START ===");
        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        var rootObjects = new List<GameObject>();
        foreach (var go in allGOs)
        {
            if (go.scene.IsValid() && go.scene.name != "" && go.transform.parent == null)
                rootObjects.Add(go);
        }

        foreach (var root in rootObjects)
        {
            Debug.Log("[ROOT] " + root.name + " | Active=" + root.activeSelf + " | pos=" + root.transform.position);
            InspectChildren(root.transform, 1);
        }
        Debug.Log("=== SCENE INSPECTION END ===");
    }

    private static void InspectChildren(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        foreach (Transform child in t)
        {
            var rb = GetRendererBounds(child.gameObject);
            string boundsStr = rb.HasValue
                ? string.Format("bounds_center={0:F2} size={1:F2}", rb.Value.center, rb.Value.size)
                : "no-renderer";
            Debug.Log(indent + "[" + child.name + "] lpos=" + child.localPosition + " wpos=" + child.position + " | " + boundsStr);
            if (depth < 3)
                InspectChildren(child, depth + 1);
        }
    }

    private static Bounds? GetRendererBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return null;
        var b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b;
    }

    public void Execute()
    {
        // ---- 1. Find MachineryScenario root ----
        GameObject scenarioRoot = FindGameObject("MachineryScenario");
        if (scenarioRoot == null)
        {
            Debug.LogError("MachineryScenario root not found! Listing all roots:");
            var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGOs)
            {
                if (go.scene.IsValid() && go.transform.parent == null)
                    Debug.Log("Root object found: " + go.name);
            }
            return;
        }

        Debug.Log("Found MachineryScenario at: " + scenarioRoot.transform.position);

        // ---- 2. Find all key objects ----
        var conveyor = FindInHierarchy(scenarioRoot, "ConveyorAssembly");
        var hopper = FindInHierarchy(scenarioRoot, "FeedHopper");
        var motor = FindInHierarchy(scenarioRoot, "DriveMotor");
        var jamArea = FindInHierarchy(scenarioRoot, "JamArea");
        var controlPanel = FindInHierarchy(scenarioRoot, "ControlPanel");
        var emergencyStop = FindInHierarchy(scenarioRoot, "EmergencyStop");
        var mainIsolator = FindInHierarchy(scenarioRoot, "MainIsolator");
        var steelTable = FindInHierarchy(scenarioRoot, "SteelWorkstation");
        var hasp = FindInHierarchy(scenarioRoot, "Hasp");
        var padlock = FindInHierarchy(scenarioRoot, "PadlockAnd");

        LogFound("ConveyorAssembly", conveyor);
        LogFound("FeedHopper", hopper);
        LogFound("DriveMotor", motor);
        LogFound("JamArea", jamArea);
        LogFound("ControlPanel", controlPanel);
        LogFound("EmergencyStop", emergencyStop);
        LogFound("MainIsolator", mainIsolator);
        LogFound("SteelWorkstation", steelTable);
        LogFound("Hasp", hasp);
        LogFound("PadlockAndTag", padlock);

        if (conveyor == null)
        {
            Debug.LogError("ConveyorAssembly not found - cannot proceed without the master reference!");
            return;
        }

        // ---- 3. Ensure conveyor is active and get its bounds ----
        EnsureActiveInHierarchy(conveyor);
        Bounds conveyorBounds = GetFullBounds(conveyor);
        Debug.Log(string.Format("Conveyor bounds: center={0:F3} size={1:F3} min={2:F3} max={3:F3}",
            conveyorBounds.center, conveyorBounds.size, conveyorBounds.min, conveyorBounds.max));

        float convCenterX = conveyorBounds.center.x;
        float convCenterZ = conveyorBounds.center.z;
        float convMinX = conveyorBounds.min.x;
        float convMaxX = conveyorBounds.max.x;
        float convBeltY = conveyorBounds.max.y;
        float convBottomY = conveyorBounds.min.y;
        float convWidth = conveyorBounds.size.z;
        float convLength = conveyorBounds.size.x;
        float convMidY = conveyorBounds.center.y;

        Debug.Log(string.Format("Conveyor: Length={0:F2}m Width={1:F2}m BeltY={2:F2} FeedX={3:F2} DriveX={4:F2}",
            convLength, convWidth, convBeltY, convMinX, convMaxX));

        // ---- 4. Fix FeedHopper ----
        if (hopper != null)
        {
            EnsureActiveInHierarchy(hopper);
            Bounds hopperBounds = GetFullBounds(hopper);

            float hopperTargetX = convMinX + (hopperBounds.size.x * 0.3f);
            float hopperTargetZ = convCenterZ;
            float hopperTargetY = convBeltY;

            SetWorldPosition(hopper, new Vector3(hopperTargetX, hopperTargetY, hopperTargetZ));
            Debug.Log(string.Format("FeedHopper repositioned to world pos: {0:F3}", hopper.transform.position));
        }

        // ---- 5. Fix DriveMotor ----
        if (motor != null)
        {
            EnsureActiveInHierarchy(motor);
            Bounds motorBounds = GetFullBounds(motor);

            float motorTargetX = convMaxX + (motorBounds.size.x * 0.5f) + 0.3f;
            float motorTargetZ = convCenterZ - (convWidth * 0.5f) - (motorBounds.size.z * 0.5f) - 0.1f;
            float motorTargetY = convMidY;

            SetWorldPosition(motor, new Vector3(motorTargetX, motorTargetY, motorTargetZ));
            motor.transform.rotation = Quaternion.Euler(0, 90, 0);

            Debug.Log(string.Format("DriveMotor repositioned to world pos: {0:F3}", motor.transform.position));
        }

        // ---- 6. Deactivate extra MotorBasePlate objects ----
        FixMotorBasePlates(motor);

        // ---- 7. JamArea coal ----
        if (jamArea != null)
        {
            EnsureActiveInHierarchy(jamArea);
            float jamX = convMinX + convLength * 0.55f;
            float jamZ = convCenterZ;
            float jamY = convBeltY + 0.05f;
            SetWorldPosition(jamArea, new Vector3(jamX, jamY, jamZ));
            Debug.Log(string.Format("JamArea repositioned to world pos: {0:F3}", jamArea.transform.position));
        }

        // ---- 8. Safety station ----
        float safetyZOffset = convCenterZ + convWidth * 0.5f + 1.5f;
        float safetyX = convCenterX;

        if (controlPanel != null)
        {
            EnsureActiveInHierarchy(controlPanel);
            Bounds cpBounds = GetFullBounds(controlPanel);
            float cpHeight = cpBounds.size.y > 0 ? cpBounds.size.y : 1.8f;

            SetWorldPosition(controlPanel, new Vector3(safetyX + 1.0f, cpHeight * 0.5f, safetyZOffset));
            controlPanel.transform.rotation = Quaternion.Euler(0, 180, 0);
            Debug.Log(string.Format("ControlPanel repositioned to world pos: {0:F3}", controlPanel.transform.position));
        }

        if (emergencyStop != null)
        {
            EnsureActiveInHierarchy(emergencyStop);
            SetWorldPosition(emergencyStop, new Vector3(safetyX - 1.5f, 1.1f, safetyZOffset + 0.2f));
            emergencyStop.transform.rotation = Quaternion.Euler(0, 180, 0);
            Debug.Log(string.Format("EmergencyStop repositioned to world pos: {0:F3}", emergencyStop.transform.position));
        }

        if (mainIsolator != null)
        {
            EnsureActiveInHierarchy(mainIsolator);
            SetWorldPosition(mainIsolator, new Vector3(safetyX + 2.5f, 1.1f, safetyZOffset + 0.2f));
            mainIsolator.transform.rotation = Quaternion.Euler(0, 180, 0);
            Debug.Log(string.Format("MainIsolator repositioned to world pos: {0:F3}", mainIsolator.transform.position));
        }

        // ---- 9. Steel Table ----
        if (steelTable != null)
        {
            EnsureActiveInHierarchy(steelTable);
            Bounds tableBounds = GetFullBounds(steelTable);
            float tableHeight = tableBounds.size.y > 0 ? tableBounds.size.y : 0.9f;

            SetWorldPosition(steelTable, new Vector3(convMaxX + 1.0f, tableHeight * 0.5f, safetyZOffset + 0.5f));
            steelTable.transform.rotation = Quaternion.Euler(0, 0, 0);
            Debug.Log(string.Format("SteelWorkstationTable repositioned to world pos: {0:F3}", steelTable.transform.position));

            Bounds tableBoundsUpdated = GetFullBounds(steelTable);
            float tabletopY = tableBoundsUpdated.max.y + 0.02f;
            float tableTopX = tableBoundsUpdated.center.x;
            float tableTopZ = tableBoundsUpdated.center.z;

            if (hasp != null)
            {
                EnsureActiveInHierarchy(hasp);
                SetWorldPosition(hasp, new Vector3(tableTopX - 0.15f, tabletopY, tableTopZ - 0.1f));
                hasp.transform.rotation = Quaternion.Euler(0, 45, 0);
                Debug.Log(string.Format("Hasp repositioned to tabletop: {0:F3}", hasp.transform.position));
            }

            if (padlock != null)
            {
                EnsureActiveInHierarchy(padlock);
                SetWorldPosition(padlock, new Vector3(tableTopX + 0.15f, tabletopY, tableTopZ + 0.1f));
                padlock.transform.rotation = Quaternion.Euler(0, -30, 0);
                Debug.Log(string.Format("PadlockAndTag repositioned to tabletop: {0:F3}", padlock.transform.position));
            }
        }

        // ---- 10. Fill hopper with coal ----
        if (hopper != null)
            FillHopperWithCoal(hopper, scenarioRoot);

        // ---- 11. Distribute coal on belt ----
        PopulateConveyorWithCoal(scenarioRoot, conveyorBounds);

        // ---- 12. Save ----
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("=== MACHINERY FINAL MASTER FIXER COMPLETE ===");
        Debug.Log("Check Scene View for visual verification.");
    }

    private void FixMotorBasePlates(GameObject motor)
    {
        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        var basePlates = new List<GameObject>();
        foreach (var go in allGOs)
        {
            if (go.scene.IsValid() && go.name.Contains("MotorBase"))
                basePlates.Add(go);
        }

        Debug.Log("Found " + basePlates.Count + " MotorBasePlate objects");
        foreach (var bp in basePlates)
        {
            bool isUnderMotor = motor != null && IsChildOf(bp.transform, motor.transform);
            if (!isUnderMotor)
            {
                bp.SetActive(false);
                Debug.Log("Deactivated MotorBasePlate: " + bp.name);
            }
        }
    }

    private bool IsChildOf(Transform child, Transform potentialParent)
    {
        Transform t = child.parent;
        while (t != null)
        {
            if (t == potentialParent) return true;
            t = t.parent;
        }
        return false;
    }

    private void FillHopperWithCoal(GameObject hopper, GameObject scenarioRoot)
    {
        string[] coalAssets = AssetDatabase.FindAssets("coal", new[] { "Assets/AR_machinary_faundation/3d" });
        if (coalAssets.Length == 0)
        {
            Debug.LogWarning("No coal asset found!");
            return;
        }

        string coalPath = AssetDatabase.GUIDToAssetPath(coalAssets[0]);
        Debug.Log("Found coal asset: " + coalPath);
        var coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coalPath);
        if (coalPrefab == null)
        {
            Debug.LogError("Could not load coal prefab from: " + coalPath);
            return;
        }

        Bounds hopperBounds = GetFullBounds(hopper);

        GameObject hopperCoalContainer = FindInHierarchy(scenarioRoot, "HopperCoal");
        if (hopperCoalContainer == null)
        {
            hopperCoalContainer = new GameObject("HopperCoal");
            hopperCoalContainer.transform.SetParent(hopper.transform, false);
        }
        else
        {
            for (int i = hopperCoalContainer.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(hopperCoalContainer.transform.GetChild(i).gameObject);
        }

        System.Random rng = new System.Random(42);
        int coalCount = 18;
        float innerW = hopperBounds.size.x * 0.35f;
        float innerD = hopperBounds.size.z * 0.35f;
        float baseY = hopperBounds.min.y + hopperBounds.size.y * 0.15f;
        float topY = hopperBounds.min.y + hopperBounds.size.y * 0.55f;

        for (int i = 0; i < coalCount; i++)
        {
            float rx = (float)(rng.NextDouble() * 2 - 1) * innerW;
            float rz = (float)(rng.NextDouble() * 2 - 1) * innerD;
            float ry = Mathf.Lerp(baseY, topY, (float)i / coalCount + (float)(rng.NextDouble() * 0.2f));
            float scale = (float)(rng.NextDouble() * 0.15f + 0.08f);

            GameObject coal = (GameObject)PrefabUtility.InstantiatePrefab(coalPrefab);
            if (coal == null) coal = Object.Instantiate(coalPrefab);
            coal.transform.SetParent(hopperCoalContainer.transform, false);
            coal.transform.localPosition = new Vector3(rx, ry - hopperBounds.center.y, rz);
            coal.transform.localScale = Vector3.one * scale;
            coal.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360));
            coal.name = "HopperCoal_" + i;
        }

        Debug.Log("Placed " + coalCount + " coal pieces inside FeedHopper");
    }

    private void PopulateConveyorWithCoal(GameObject scenarioRoot, Bounds conveyorBounds)
    {
        string[] coalAssets = AssetDatabase.FindAssets("coal", new[] { "Assets/AR_machinary_faundation/3d" });
        if (coalAssets.Length == 0) return;

        string coalPath = AssetDatabase.GUIDToAssetPath(coalAssets[0]);
        var coalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coalPath);
        if (coalPrefab == null) return;

        GameObject beltCoalContainer = FindInHierarchy(scenarioRoot, "BeltCoal");
        if (beltCoalContainer == null)
        {
            beltCoalContainer = new GameObject("BeltCoal");
            beltCoalContainer.transform.SetParent(scenarioRoot.transform, false);
        }
        else
        {
            for (int i = beltCoalContainer.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(beltCoalContainer.transform.GetChild(i).gameObject);
        }

        System.Random rng = new System.Random(123);
        float beltY = conveyorBounds.max.y + 0.04f;
        float beltLeft = conveyorBounds.min.x + conveyorBounds.size.x * 0.1f;
        float beltRight = conveyorBounds.max.x - conveyorBounds.size.x * 0.05f;
        float beltLen = beltRight - beltLeft;
        float beltCenterZ = conveyorBounds.center.z;
        float beltWidth = conveyorBounds.size.z * 0.55f;

        int totalCoal = 45;
        for (int i = 0; i < totalCoal; i++)
        {
            float jamBias = (float)rng.NextDouble();
            float posT;
            if (jamBias < 0.55f)
                posT = 0.5f + (float)(rng.NextDouble() * 0.35f);
            else
                posT = (float)(rng.NextDouble() * 0.5f);

            float worldX = beltLeft + posT * beltLen + (float)(rng.NextDouble() - 0.5) * 0.15f;
            float worldZ = beltCenterZ + (float)(rng.NextDouble() * 2 - 1) * (beltWidth * 0.5f);

            float baseScale = posT > 0.5f ? 0.12f : 0.09f;
            float scale = baseScale + (float)(rng.NextDouble() * 0.06f);

            GameObject coal = (GameObject)PrefabUtility.InstantiatePrefab(coalPrefab);
            if (coal == null) coal = Object.Instantiate(coalPrefab);
            coal.transform.SetParent(beltCoalContainer.transform, false);
            coal.transform.position = new Vector3(worldX, beltY, worldZ);
            coal.transform.localScale = Vector3.one * scale;
            coal.transform.rotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 30 - 15),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 30 - 15));
            coal.name = "BeltCoal_" + i;
        }

        Debug.Log("Placed " + totalCoal + " coal pieces on conveyor belt");
    }

    private static GameObject FindGameObject(string name)
    {
        var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allGOs)
        {
            if (go.scene.IsValid() && go.name == name)
                return go;
        }
        return null;
    }

    private static GameObject FindInHierarchy(GameObject root, string nameContains)
    {
        if (root.name.Contains(nameContains)) return root;
        foreach (Transform child in root.transform)
        {
            var found = FindInHierarchy(child.gameObject, nameContains);
            if (found != null) return found;
        }
        return null;
    }

    private static void EnsureActiveInHierarchy(GameObject go)
    {
        if (!go.activeSelf) go.SetActive(true);
        Transform t = go.transform.parent;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            t = t.parent;
        }
    }

    private static Bounds GetFullBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(false);
        if (renderers.Length == 0)
            renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.one);

        var b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b;
    }

    private static void SetWorldPosition(GameObject go, Vector3 worldPos)
    {
        Undo.RecordObject(go.transform, "Machinery Final Fix - " + go.name);
        go.transform.position = worldPos;
        EditorUtility.SetDirty(go);
    }

    private static void LogFound(string label, GameObject go)
    {
        if (go != null)
            Debug.Log("[FOUND] " + label + " = " + go.name + " | active=" + go.activeInHierarchy + " | wpos=" + go.transform.position);
        else
            Debug.LogWarning("[NOT FOUND] " + label);
    }
}
