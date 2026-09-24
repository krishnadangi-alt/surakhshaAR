using UnityEngine;

public class GasEnvironmentBuilder : MonoBehaviour
{
    [Header("Equipment Assets (Original Imported GLB/FBX/OBJ Models)")]
    [SerializeField] private GameObject gasCylinderPrefab;
    [SerializeField] private GameObject gasDetectorPrefab;
    [SerializeField] private GameObject h2sDetectorPrefab;
    [SerializeField] private GameObject pipelineValvePrefab;
    [SerializeField] private GameObject ballValvePrefab;
    [SerializeField] private GameObject industrialValvePrefab;
    [SerializeField] private GameObject gasRegulatorPrefab;
    [SerializeField] private GameObject pressureGaugePrefab;
    [SerializeField] private GameObject industrialPipePrefab;
    [SerializeField] private GameObject pipeElbowPrefab;
    [SerializeField] private GameObject metalPipesPrefab;
    [SerializeField] private GameObject confinedVesselPrefab;
    [SerializeField] private GameObject industrialTanksPrefab;
    [SerializeField] private GameObject ventilationFanPrefab;
    [SerializeField] private GameObject lifelinePrefab;
    [SerializeField] private GameObject workbenchPrefab;
    [SerializeField] private GameObject hardhatPrefab;
    [SerializeField] private GameObject safetyVestPrefab;
    [SerializeField] private GameObject gogglesPrefab;
    [SerializeField] private GameObject glovesPrefab;
    [SerializeField] private GameObject bootsPrefab;

    private GameObject currentGasCylinder;
    private GameObject currentGasDetector;
    private GameObject currentH2SDetector;
    private GameObject currentPipelineAssembly;
    private GameObject currentIndustrialEnvironment;

    public GameObject CurrentGasCylinder => currentGasCylinder;
    public GameObject CurrentGasDetector => currentGasDetector;
    public GameObject CurrentPipelineAssembly => currentPipelineAssembly;
    public GameObject CurrentIndustrialEnvironment => currentIndustrialEnvironment;

    public void BuildEnvironment()
    {
        ClearEnvironment();

        // Load PBR Materials for untextured OBJ/FBX models
#if UNITY_EDITOR
        Material vesselMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Vessel_Tank.mat");
        Material signMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Safety_Sign.mat");
        Material workbenchMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PPE_Leather.mat");
        Material pipeMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Modular_Pipes_Steel.mat");
        Material valveMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Valve_Red.mat");
#else
        Material vesselMat = null, signMat = null, workbenchMat = null, pipeMat = null, valveMat = null;
#endif

        if (pipeMat == null) pipeMat = Resources.Load<Material>("MAT_Modular_Pipes_Steel");
        if (pipeMat == null)
        {
            Shader pipeShader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                         ?? Shader.Find("Mobile/Bumped Specular")
                         ?? Shader.Find("Standard");
            if (pipeShader != null)
            {
                pipeMat = new Material(pipeShader);
                pipeMat.color = new Color(0.68f, 0.70f, 0.74f, 1.0f);
                if (pipeMat.HasProperty("_Smoothness")) pipeMat.SetFloat("_Smoothness", 0.65f);
                if (pipeMat.HasProperty("_Metallic")) pipeMat.SetFloat("_Metallic", 0.75f);
            }
        }

        if (valveMat == null) valveMat = Resources.Load<Material>("MAT_Valve_Red");
        if (valveMat == null)
        {
            Shader valveShader = Shader.Find("Universal Render Pipeline/Lit")
                          ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                          ?? Shader.Find("Mobile/Bumped Specular")
                          ?? Shader.Find("Standard");
            if (valveShader != null)
            {
                valveMat = new Material(valveShader);
                valveMat.color = new Color(0.85f, 0.12f, 0.10f, 1.0f);
                if (valveMat.HasProperty("_Smoothness")) valveMat.SetFloat("_Smoothness", 0.50f);
            }
        }

        // 1. SURROUNDING INDUSTRIAL SITE & PPE / CONFINED SPACE STATIONS
        BuildIndustrialEnvironment(vesselMat, signMat, valveMat, workbenchMat);

        // 2. CENTER ZONE - ORIGINAL RED GAS CYLINDER (Grounded at X = 0.00m, Z = 0.00m)
        if (gasCylinderPrefab != null)
        {
            currentGasCylinder = Instantiate(gasCylinderPrefab, transform, false);
            currentGasCylinder.name = "Realistic_Gas_Cylinder";
            currentGasCylinder.transform.localPosition = new Vector3(0.00f, 0.00f, 0.00f);
            currentGasCylinder.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            SetTargetPhysicalSize(currentGasCylinder, 1.20f);

            SnapMeshLowestLocalYToSurface(currentGasCylinder, transform, 0.00f, 0.00f);

            Transform cylOutletSocket = currentGasCylinder.transform.Find("OutletSocket");
            if (cylOutletSocket == null)
            {
                GameObject socketObj = new GameObject("OutletSocket");
                socketObj.transform.SetParent(currentGasCylinder.transform, false);
                socketObj.transform.localPosition = new Vector3(0.0f, 1.150f, 0.0f);
                socketObj.transform.localRotation = Quaternion.identity;
                cylOutletSocket = socketObj.transform;
            }

            Transform leakPoint = currentGasCylinder.transform.Find("LeakPoint");
            if (leakPoint == null)
            {
                GameObject lpObj = new GameObject("LeakPoint");
                lpObj.transform.SetParent(currentGasCylinder.transform, false);
                lpObj.transform.localPosition = new Vector3(0.0f, 1.150f, 0.0f);
                lpObj.transform.localRotation = Quaternion.identity;
                leakPoint = lpObj.transform;
            }

            float neckHeightY = 1.150f;
            Renderer[] cylinderRenderers = currentGasCylinder.GetComponentsInChildren<Renderer>();
            if (cylinderRenderers.Length > 0)
            {
                Bounds bounds = GetLocalCombinedBounds(cylinderRenderers, transform);
                neckHeightY = bounds.max.y;
                CreateHazardCollider(currentGasCylinder, bounds);
            }

            // 3. PHYSICAL PIPELINE CHAIN (Connected directly from regulator to valve to elbow)
            BuildPhysicalPipelineTree(cylOutletSocket, neckHeightY, pipeMat, valveMat);
        }

        // 4. GAS DETECTORS (Exactly ONE intended multi-gas detector grounded upright on floor beside cylinder base)
        if (gasDetectorPrefab != null)
        {
            currentGasDetector = Instantiate(gasDetectorPrefab, transform, false);
            currentGasDetector.name = "Multi_Gas_Detector";
            currentGasDetector.transform.localPosition = new Vector3(-0.18f, 0.00f, -0.15f);
            currentGasDetector.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            SetTargetPhysicalSize(currentGasDetector, 0.18f);

            SnapMeshLowestLocalYToSurface(currentGasDetector, transform, 0.00f, 0.00f);

            if (currentGasDetector.GetComponent<Collider>() == null && currentGasDetector.GetComponentInChildren<Collider>() == null)
            {
                BoxCollider detCol = currentGasDetector.AddComponent<BoxCollider>();
                detCol.center = new Vector3(0f, 0.09f, 0f);
                detCol.size = new Vector3(0.13f, 0.19f, 0.08f);
            }
        }

        Debug.Log("[GasAR] COHERENT THREE-ZONE INDUSTRIAL WORKSTATION INSTANTIATED SUCCESSFULLY!");
    }

    private void BuildIndustrialEnvironment(Material vesselMat, Material signMat, Material valveMat, Material workbenchMat)
    {
        currentIndustrialEnvironment = new GameObject("IndustrialEnvironment");
        currentIndustrialEnvironment.transform.SetParent(transform, false);
        currentIndustrialEnvironment.transform.localPosition = Vector3.zero;

        // RIGHT ZONE - LARGE CONFINED SPACE TRAINING STATION (Centered at X = +2.60m, Z = 0.00m)
        GameObject confinedRoot = new GameObject("Confined_Space_Training_Area");
        confinedRoot.transform.SetParent(currentIndustrialEnvironment.transform, false);
        confinedRoot.transform.localPosition = new Vector3(2.85f, 0.00f, 0.00f);

        // PRIMARY CONFINED SPACE MODEL: Real 53.7 MB industrial GLB asset (industrial_tanks_and_equipment.glb)
        GameObject vesselSource = industrialTanksPrefab != null ? industrialTanksPrefab : confinedVesselPrefab;
        if (vesselSource != null)
        {
            GameObject indEquipment = Instantiate(vesselSource, confinedRoot.transform, false);
            indEquipment.name = "Industrial_Tanks_And_Equipment";
            // Remove clutter & physically assemble essential components:
            // Object_6: Main Vessel & Entry Manhole
            // Object_7: Supporting Frame & Legs
            // Object_10: Ladder, Platform, & Railings
            Transform node0 = indEquipment.transform.Find("root/GLTF_SceneRootNode/_0");
            if (node0 != null)
            {
                for (int i = 0; i < node0.childCount; i++)
                {
                    Transform child = node0.GetChild(i);
                    bool isEssential = (child.name == "Object_6" || child.name == "Object_7" || child.name == "Object_10");
                    child.gameObject.SetActive(isEssential);
                }

                Transform obj6 = node0.Find("Object_6");
                Transform obj7 = node0.Find("Object_7");
                Transform obj10 = node0.Find("Object_10");

                // Local physical assembly: Reset obj6, obj7, obj10 to Vector3.zero to restore native GLB model authoring alignment
                if (obj7 != null) { obj7.localPosition = Vector3.zero; obj7.localRotation = Quaternion.identity; }
                if (obj6 != null) { obj6.localPosition = Vector3.zero; obj6.localRotation = Quaternion.identity; }
                if (obj10 != null) { obj10.localPosition = Vector3.zero; obj10.localRotation = Quaternion.identity; }
            }

            SetTargetPhysicalSize(indEquipment, 5.025f); // 50% increase (1.5x scale, target height 1.866m)
            SnapMeshLowestLocalYToSurface(indEquipment, confinedRoot.transform, 0.00f, 0.00f);

            Renderer[] rList = indEquipment.GetComponentsInChildren<Renderer>(false);
            if (rList.Length > 0)
            {
                Bounds b = GetLocalCombinedBounds(rList, confinedRoot.transform);
                Debug.Log($"[PROPER_ARRANGEMENT_BOUNDS] Industrial_Tanks_And_Equipment local bounds.min.y = {b.min.y:F4}m, bounds.max.y = {b.max.y:F4}m, height = {b.size.y:F4}m");
            }
        }

        if (ventilationFanPrefab != null)
        {
            GameObject fan = Instantiate(ventilationFanPrefab, confinedRoot.transform, false);
            fan.name = "Axial_Ventilation_Fan";
            fan.transform.localPosition = new Vector3(0.30f, 0.00f, 1.05f);
            fan.transform.localRotation = Quaternion.Euler(0f, -140f, 0f); // Duct facing vessel manhole entry
            SetTargetPhysicalSize(fan, 0.87f); // 1.5x scale
            SnapMeshLowestLocalYToSurface(fan, confinedRoot.transform, 0.00f, 0.00f);

            Renderer[] rList = fan.GetComponentsInChildren<Renderer>(false);
            if (rList.Length > 0)
            {
                Bounds b = GetLocalCombinedBounds(rList, confinedRoot.transform);
                Debug.Log($"[BOUNDS_LOG] Axial_Ventilation_Fan local bounds.min.y = {b.min.y:F4}m, bounds.max.y = {b.max.y:F4}m");
            }
        }

        if (lifelinePrefab != null)
        {
            GameObject lifeline = Instantiate(lifelinePrefab, confinedRoot.transform, false);
            lifeline.name = "Self_Retracting_Lifeline";
            lifeline.transform.localPosition = new Vector3(1.25f, 2.22f, 0.00f);
            lifeline.transform.localRotation = Quaternion.identity;
            SetTargetPhysicalSize(lifeline, 0.78f); // 1.5x scale

            Renderer[] rList = lifeline.GetComponentsInChildren<Renderer>(false);
            if (rList.Length > 0)
            {
                Bounds b = GetLocalCombinedBounds(rList, confinedRoot.transform);
                Debug.Log($"[BOUNDS_LOG] Self_Retracting_Lifeline local bounds.min.y = {b.min.y:F4}m, bounds.max.y = {b.max.y:F4}m");
            }
        }

        // LEFT ZONE - PPE PREPARATION STATION (Centered at X = -1.65m, Z = 0.00m)
        GameObject ppeRoot = new GameObject("PPE_Preparation_Station");
        ppeRoot.transform.SetParent(currentIndustrialEnvironment.transform, false);
        ppeRoot.transform.localPosition = new Vector3(-1.65f, 0.00f, 0.00f);

        float topShelfY = 0.76f;

        if (workbenchPrefab != null)
        {
            GameObject table = Instantiate(workbenchPrefab, ppeRoot.transform, false);
            table.name = "PPE_Workbench";
            table.transform.localPosition = Vector3.zero;
            table.transform.localRotation = Quaternion.identity;
            SetTargetPhysicalSize(table, 1.20f);
            SnapMeshLowestLocalYToSurface(table, ppeRoot.transform, 0.00f, 0.00f);
            ForceApplyMaterial(table, workbenchMat);

            Renderer[] rends = table.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds lb = GetLocalCombinedBounds(rends, ppeRoot.transform);
                topShelfY = lb.max.y;
            }
        }

        if (hardhatPrefab != null)
        {
            GameObject helmet = Instantiate(hardhatPrefab, ppeRoot.transform, false);
            helmet.name = "Safety_Helmet";
            helmet.transform.localPosition = new Vector3(-0.35f, 0.0f, 0.00f);
            helmet.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            SetTargetPhysicalSize(helmet, 0.28f);
            SnapMeshLowestLocalYToSurface(helmet, ppeRoot.transform, topShelfY, 0.003f);
        }

        if (safetyVestPrefab != null)
        {
            GameObject vest = Instantiate(safetyVestPrefab, ppeRoot.transform, false);
            vest.name = "Safety_Vest";
            vest.transform.localPosition = new Vector3(-0.15f, 0.0f, 0.00f);
            vest.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            SetTargetPhysicalSize(vest, 0.42f);
            SnapMeshLowestLocalYToSurface(vest, ppeRoot.transform, topShelfY, 0.003f);
#if UNITY_EDITOR
            Material vestMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Detector_Yellow.mat");
#else
            Material vestMat = null;
#endif
            if (vestMat == null) vestMat = Resources.Load<Material>("MAT_Detector_Yellow");
            if (vestMat == null)
            {
                Shader vestShader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                             ?? Shader.Find("Mobile/Bumped Specular")
                             ?? Shader.Find("Standard");
                if (vestShader != null)
                {
                    vestMat = new Material(vestShader);
                    vestMat.color = new Color(0.95f, 0.85f, 0.10f, 1.0f);
                }
            }
            if (vestMat != null) ForceApplyMaterial(vest, vestMat);
        }

        if (gogglesPrefab != null)
        {
            GameObject goggles = Instantiate(gogglesPrefab, ppeRoot.transform, false);
            goggles.name = "Safety_Goggles";
            goggles.transform.localPosition = new Vector3(0.08f, 0.0f, 0.00f);
            goggles.transform.localRotation = Quaternion.identity;
            SetTargetPhysicalSize(goggles, 0.18f);
            SnapMeshLowestLocalYToSurface(goggles, ppeRoot.transform, topShelfY, 0.003f);
        }

        if (glovesPrefab != null)
        {
            GameObject leftGlove = Instantiate(glovesPrefab, ppeRoot.transform, false);
            leftGlove.name = "Work_Gloves_Left";
            leftGlove.transform.localPosition = new Vector3(0.24f, 0.0f, 0.05f);
            leftGlove.transform.localRotation = Quaternion.identity;
            SetTargetPhysicalSize(leftGlove, 0.22f);
            SnapMeshLowestLocalYToSurface(leftGlove, ppeRoot.transform, topShelfY, 0.003f);

            GameObject rightGlove = Instantiate(glovesPrefab, ppeRoot.transform, false);
            rightGlove.name = "Work_Gloves_Right";
            rightGlove.transform.localPosition = new Vector3(0.38f, 0.0f, 0.05f);
            rightGlove.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            SetTargetPhysicalSize(rightGlove, 0.22f);
            SnapMeshLowestLocalYToSurface(rightGlove, ppeRoot.transform, topShelfY, 0.003f);
        }

        if (bootsPrefab != null)
        {
            GameObject leftBoot = Instantiate(bootsPrefab, ppeRoot.transform, false);
            leftBoot.name = "Safety_Boots_Left";
            leftBoot.transform.localPosition = new Vector3(-0.25f, 0.0f, 0.00f);
            leftBoot.transform.localRotation = Quaternion.identity;
            SetTargetPhysicalSize(leftBoot, 0.28f);
            SnapMeshLowestLocalYToSurface(leftBoot, ppeRoot.transform, 0.00f, 0.001f);

            GameObject rightBoot = Instantiate(bootsPrefab, ppeRoot.transform, false);
            rightBoot.name = "Safety_Boots_Right";
            rightBoot.transform.localPosition = new Vector3(0.05f, 0.0f, 0.00f);
            rightBoot.transform.localRotation = Quaternion.identity;
            SetTargetPhysicalSize(rightBoot, 0.28f);
            SnapMeshLowestLocalYToSurface(rightBoot, ppeRoot.transform, 0.00f, 0.001f);
        }
    }

    private void BuildPhysicalPipelineTree(Transform cylinderOutletSocket, float neckHeightY, Material pipeMat, Material valveMat)
    {
        GameObject pipeAssemblyObj = new GameObject("GasPipelineAssembly");
        pipeAssemblyObj.transform.SetParent(transform, false);
        pipeAssemblyObj.transform.localPosition = Vector3.zero;
        pipeAssemblyObj.transform.localRotation = Quaternion.identity;
        pipeAssemblyObj.transform.localScale = Vector3.one;
        currentPipelineAssembly = pipeAssemblyObj;

        Vector3 cylNeckPos = new Vector3(0.00f, 1.150f, 0.00f);
        float pipeRadius = 0.018f;
        float flangeRadius = 0.024f;
        float flangeThickness = 0.005f;

        // 1. REAL REGULATOR: Physically mounted to cylinder neck/outlet facing right (+X)
        if (gasRegulatorPrefab != null)
        {
            GameObject reg = Instantiate(gasRegulatorPrefab, pipeAssemblyObj.transform, false);
            reg.name = "Gas_Regulator_3D";
            reg.transform.localPosition = cylNeckPos;
            reg.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            SetTargetPhysicalSize(reg, 0.14f);
        }

        // 2. REAL PRESSURE GAUGE: Threaded into top port of regulator
        if (pressureGaugePrefab != null)
        {
            GameObject gauge = Instantiate(pressureGaugePrefab, pipeAssemblyObj.transform, false);
            gauge.name = "Pressure_Gauge_3D";
            gauge.transform.localPosition = cylNeckPos + new Vector3(0.012f, 0.090f, 0.00f);
            gauge.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            SetTargetPhysicalSize(gauge, 0.10f);
        }

        // 3. REAL RED ISOLATION VALVE (VALVE.fbx): Mounted directly inline on pipeline with realistic proportions
        Vector3 targetValveCenter = cylNeckPos + new Vector3(0.165f, 0.000f, 0.00f);
        float valveInletX = targetValveCenter.x - 0.022f;
        float valveOutletX = targetValveCenter.x + 0.022f;

        if (pipelineValvePrefab != null)
        {
            GameObject valve = Instantiate(pipelineValvePrefab, pipeAssemblyObj.transform, false);
            valve.name = "Isolation_Valve_3D";
            valve.transform.localPosition = targetValveCenter;
            valve.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
            SetTargetPhysicalSize(valve, 0.090f);
            ForceApplyMaterial(valve, valveMat);

            BoxCollider vCol = valve.GetComponent<BoxCollider>();
            if (vCol == null) vCol = valve.AddComponent<BoxCollider>();
            vCol.center = new Vector3(0.00f, 0.035f, 0.00f);
            vCol.size = new Vector3(0.12f, 0.15f, 0.12f);

            Renderer[] valveRends = valve.GetComponentsInChildren<Renderer>(true);
            if (valveRends.Length > 0)
            {
                Bounds vb = GetLocalCombinedBounds(valveRends, pipeAssemblyObj.transform);
                // Center X alignment so valve center is placed at targetValveCenter.x
                float offsetX = targetValveCenter.x - vb.center.x;
                // Align bore Y to cylNeckPos.y (bore center is 0.012m above bottom body for 0.090m scaled model)
                float boreCenterY = vb.min.y + 0.012f;
                float offsetY = cylNeckPos.y - boreCenterY;
                valve.transform.localPosition += new Vector3(offsetX, offsetY, 0f);

                // Measure exact valve connection bore vertices (Y near cylNeckPos.y, Z near 0)
                float minBoreX = float.MaxValue;
                float maxBoreX = float.MinValue;
                MeshFilter[] valveMfs = valve.GetComponentsInChildren<MeshFilter>(true);
                foreach (var mf in valveMfs)
                {
                    if (mf.sharedMesh == null) continue;
                    Vector3[] verts = mf.sharedMesh.vertices;
                    foreach (var v in verts)
                    {
                        Vector3 worldV = mf.transform.TransformPoint(v);
                        Vector3 localV = pipeAssemblyObj.transform.InverseTransformPoint(worldV);
                        // Restrict Y strictly to the bottom connection body cylinder (ignoring upper stem and handwheel disk)
                        if (localV.y >= (cylNeckPos.y - 0.015f) && localV.y <= (cylNeckPos.y + 0.008f) && Mathf.Abs(localV.z - cylNeckPos.z) < 0.025f)
                        {
                            if (localV.x < minBoreX) minBoreX = localV.x;
                            if (localV.x > maxBoreX) maxBoreX = localV.x;
                        }
                    }
                }

                if (minBoreX != float.MaxValue && maxBoreX != float.MinValue)
                {
                    valveInletX = minBoreX;
                    valveOutletX = maxBoreX;
                    Debug.Log($"[VALVE_REALISTIC_FIT] Measured exact valve ports: LeftInletX={valveInletX:F4}m, RightOutletX={valveOutletX:F4}m");
                }
                else
                {
                    Bounds vbAligned = GetCombinedBounds(valveRends);
                    valveInletX = vbAligned.center.x - 0.018f;
                    valveOutletX = vbAligned.center.x + 0.018f;
                }
            }
        }

        // 4. REGULATOR OUTLET FLANGE & SHORT PIPE 1 (Extending from Regulator Outlet X=0.038m to Valve Inlet)
        Vector3 p1Start = cylNeckPos + new Vector3(0.038f, 0.000f, 0.00f);
        CreateFlangeCollar(pipeAssemblyObj.transform, p1Start, Vector3.right, flangeRadius, flangeThickness, pipeMat, "Regulator_Outlet_Flange");

        // Pipe 1 ends slightly past valveInletX to seat fully into the valve body
        Vector3 p1End = new Vector3(valveInletX + 0.004f, cylNeckPos.y, 0.00f);
        CreateProceduralStraightPipe(pipeAssemblyObj.transform, p1Start + new Vector3(flangeThickness, 0f, 0f), p1End, pipeRadius, flangeRadius, pipeMat, "Horizontal_Pipe_1");

        // ANSI PIPE FLANGE AT VALVE INLET (Placed at p1End facing -X, wrapping valve inlet face)
        CreateFlangeCollar(pipeAssemblyObj.transform, p1End, Vector3.left, flangeRadius, 0.008f, pipeMat, "Pipe_1_Valve_Inlet_Flange");

        // 5. ANSI PIPE FLANGE AT VALVE OUTLET & HORIZONTAL PIPE 2 (Starting at p2Start facing +X, wrapping valve outlet face)
        Vector3 p2Start = new Vector3(valveOutletX - 0.004f, cylNeckPos.y, 0.00f);
        CreateFlangeCollar(pipeAssemblyObj.transform, p2Start, Vector3.right, flangeRadius, 0.008f, pipeMat, "Pipe_2_Valve_Outlet_Flange");

        Vector3 p2End = new Vector3(valveOutletX + 0.055f, cylNeckPos.y, 0.00f);
        CreateProceduralStraightPipe(pipeAssemblyObj.transform, p2Start, p2End, pipeRadius, flangeRadius, pipeMat, "Horizontal_Pipe_2");

        // 6. PROCEDURAL CURVED 90-DEGREE PIPE ELBOW: Smooth torus arc bend
        Vector3 elbowInlet = p2End;
        Vector3 elbowOutlet = new Vector3(elbowInlet.x + 0.035f, cylNeckPos.y - 0.035f, 0.00f);
        CreateProceduralCurvedElbow(pipeAssemblyObj.transform, elbowInlet, elbowOutlet, pipeRadius, flangeRadius, pipeMat, "Pipe_Elbow_3D");

        // 7. PROCEDURAL DOWNSTREAM PIPE: Vertical pipe extending down to floor level (Y=0.000m)
        Vector3 dpStart = elbowOutlet;
        Vector3 dpEnd = new Vector3(elbowOutlet.x, 0.000f, elbowOutlet.z);
        CreateProceduralStraightPipe(pipeAssemblyObj.transform, dpStart, dpEnd, pipeRadius, flangeRadius, pipeMat, "Downstream_Pipe_3D");

        // Ground Flange Coupling resting flat on floor
        CreateFlangeCollar(pipeAssemblyObj.transform, dpEnd, Vector3.up, flangeRadius * 1.25f, 0.012f, pipeMat, "Ground_Flange_Coupling");
    }

    private GameObject CreateProceduralStraightPipe(Transform parent, Vector3 startPos, Vector3 endPos, float pipeRadius, float flangeRadius, Material pipeMat, string name)
    {
        GameObject pipeObj = new GameObject(name);
        pipeObj.transform.SetParent(parent, false);
        pipeObj.transform.localPosition = Vector3.zero;
        pipeObj.transform.localRotation = Quaternion.identity;

        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        if (length < 0.0001f) return pipeObj;

        GameObject bodyObj = new GameObject(name + "_Body");
        bodyObj.transform.SetParent(pipeObj.transform, false);
        bodyObj.transform.localPosition = startPos;
        bodyObj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);

        MeshFilter mf = bodyObj.AddComponent<MeshFilter>();
        MeshRenderer mr = bodyObj.AddComponent<MeshRenderer>();
        if (pipeMat != null) mr.sharedMaterial = pipeMat;

        mf.sharedMesh = GenerateTubeMesh(pipeRadius, length, 24, true, true);

        // Flange Collar at Start
        CreateFlangeCollar(pipeObj.transform, startPos, delta.normalized, flangeRadius, 0.008f, pipeMat, name + "_StartFlange");

        // Flange Collar at End
        CreateFlangeCollar(pipeObj.transform, endPos, delta.normalized, flangeRadius, 0.008f, pipeMat, name + "_EndFlange");

        return pipeObj;
    }

    private GameObject CreateProceduralCurvedElbow(Transform parent, Vector3 inletPos, Vector3 outletPos, float pipeRadius, float flangeRadius, Material pipeMat, string name)
    {
        GameObject elbowObj = new GameObject(name);
        elbowObj.transform.SetParent(parent, false);
        elbowObj.transform.localPosition = Vector3.zero;
        elbowObj.transform.localRotation = Quaternion.identity;

        Vector3 arcCenter = new Vector3(inletPos.x, outletPos.y, inletPos.z);
        float bendRadius = Vector3.Distance(inletPos, arcCenter);

        GameObject bendObj = new GameObject(name + "_CurvedBend");
        bendObj.transform.SetParent(elbowObj.transform, false);
        bendObj.transform.localPosition = arcCenter;
        bendObj.transform.localRotation = Quaternion.identity;

        MeshFilter mf = bendObj.AddComponent<MeshFilter>();
        MeshRenderer mr = bendObj.AddComponent<MeshRenderer>();
        if (pipeMat != null) mr.sharedMaterial = pipeMat;

        mf.sharedMesh = GenerateTorusArcMesh(bendRadius, pipeRadius, 16, 24);

        // Inlet Flange Collar (Facing +X)
        CreateFlangeCollar(elbowObj.transform, inletPos, Vector3.right, flangeRadius, 0.008f, pipeMat, name + "_InletFlange");

        // Outlet Flange Collar (Facing -Y)
        CreateFlangeCollar(elbowObj.transform, outletPos, Vector3.down, flangeRadius, 0.008f, pipeMat, name + "_OutletFlange");

        return elbowObj;
    }

    private GameObject CreateFlangeCollar(Transform parent, Vector3 pos, Vector3 direction, float flangeRadius, float thickness, Material pipeMat, string name)
    {
        GameObject flangeObj = new GameObject(name);
        flangeObj.transform.SetParent(parent, false);
        flangeObj.transform.localPosition = pos;
        flangeObj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

        MeshFilter mf = flangeObj.AddComponent<MeshFilter>();
        MeshRenderer mr = flangeObj.AddComponent<MeshRenderer>();
        if (pipeMat != null) mr.sharedMaterial = pipeMat;

        mf.sharedMesh = GenerateTubeMesh(flangeRadius, thickness, 24, true, true);
        return flangeObj;
    }

    private Mesh GenerateTubeMesh(float radius, float height, int radialSegments, bool topCap, bool bottomCap)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralPipeTube";

        System.Collections.Generic.List<Vector3> verts = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<Vector3> normals = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<Vector2> uvs = new System.Collections.Generic.List<Vector2>();
        System.Collections.Generic.List<int> tris = new System.Collections.Generic.List<int>();

        for (int i = 0; i <= radialSegments; i++)
        {
            float angle = (float)i / radialSegments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 normal = new Vector3(cos, 0f, sin);
            Vector3 posBottom = new Vector3(cos * radius, 0f, sin * radius);
            Vector3 posTop = new Vector3(cos * radius, height, sin * radius);

            verts.Add(posBottom);
            normals.Add(normal);
            uvs.Add(new Vector2((float)i / radialSegments, 0f));

            verts.Add(posTop);
            normals.Add(normal);
            uvs.Add(new Vector2((float)i / radialSegments, 1f));
        }

        for (int i = 0; i < radialSegments; i++)
        {
            int b1 = i * 2;
            int t1 = b1 + 1;
            int b2 = (i + 1) * 2;
            int t2 = b2 + 1;

            tris.Add(b1);
            tris.Add(t1);
            tris.Add(t2);

            tris.Add(b1);
            tris.Add(t2);
            tris.Add(b2);
        }

        if (bottomCap)
        {
            int centerIdx = verts.Count;
            verts.Add(Vector3.zero);
            normals.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int ringStart = verts.Count;
            for (int i = 0; i < radialSegments; i++)
            {
                float angle = (float)i / radialSegments * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                verts.Add(pos);
                normals.Add(Vector3.down);
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
            }

            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                tris.Add(centerIdx);
                tris.Add(ringStart + next);
                tris.Add(ringStart + i);
            }
        }

        if (topCap)
        {
            int centerIdx = verts.Count;
            verts.Add(new Vector3(0f, height, 0f));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int ringStart = verts.Count;
            for (int i = 0; i < radialSegments; i++)
            {
                float angle = (float)i / radialSegments * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
                verts.Add(pos);
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
            }

            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                tris.Add(centerIdx);
                tris.Add(ringStart + i);
                tris.Add(ringStart + next);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    private Mesh GenerateTorusArcMesh(float bendRadius, float pipeRadius, int bendSegments, int radialSegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralTorusArcElbow";

        System.Collections.Generic.List<Vector3> verts = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<Vector3> normals = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<Vector2> uvs = new System.Collections.Generic.List<Vector2>();
        System.Collections.Generic.List<int> tris = new System.Collections.Generic.List<int>();

        for (int b = 0; b <= bendSegments; b++)
        {
            float theta = (float)b / bendSegments * (Mathf.PI * 0.5f);
            float cosTheta = Mathf.Cos(theta);
            float sinTheta = Mathf.Sin(theta);

            Vector3 centerPos = new Vector3(bendRadius * sinTheta, bendRadius * cosTheta, 0f);
            Vector3 radNormal = new Vector3(sinTheta, cosTheta, 0f);
            Vector3 binormal = new Vector3(0f, 0f, 1f);

            for (int r = 0; r <= radialSegments; r++)
            {
                float phi = (float)r / radialSegments * (Mathf.PI * 2f);
                float cosPhi = Mathf.Cos(phi);
                float sinPhi = Mathf.Sin(phi);

                Vector3 dir = radNormal * cosPhi + binormal * sinPhi;
                Vector3 pos = centerPos + dir * pipeRadius;

                verts.Add(pos);
                normals.Add(dir.normalized);
                uvs.Add(new Vector2((float)b / bendSegments, (float)r / radialSegments));
            }
        }

        int ringStride = radialSegments + 1;
        for (int b = 0; b < bendSegments; b++)
        {
            for (int r = 0; r < radialSegments; r++)
            {
                int current = b * ringStride + r;
                int nextRing = (b + 1) * ringStride + r;

                tris.Add(current);
                tris.Add(nextRing);
                tris.Add(nextRing + 1);

                tris.Add(current);
                tris.Add(nextRing + 1);
                tris.Add(current + 1);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    private void ApplyVesselMaterials(GameObject vesselObj, Material mainMat, Material signMat, Material valveMat)
    {
        if (vesselObj == null) return;
        Renderer[] renderers = vesselObj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            int count = r.sharedMaterials.Length;
            if (count == 0) continue;
            Material[] mats = new Material[count];
            for (int i = 0; i < count; i++)
            {
                if (i == 0) mats[i] = mainMat != null ? mainMat : r.sharedMaterials[i];
                else if (i == 1) mats[i] = signMat != null ? signMat : (mainMat != null ? mainMat : r.sharedMaterials[i]);
                else if (i == 2) mats[i] = valveMat != null ? valveMat : (mainMat != null ? mainMat : r.sharedMaterials[i]);
                else mats[i] = mainMat != null ? mainMat : r.sharedMaterials[i];
            }
            r.sharedMaterials = mats;
        }
    }

    private void ForceApplyMaterial(GameObject targetObj, Material overrideMat)
    {
        if (targetObj == null || overrideMat == null) return;
        Renderer[] renderers = targetObj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = overrideMat;
            }
            r.sharedMaterials = mats;
        }
    }

    private void SetTargetPhysicalSize(GameObject targetObj, float targetMaxDimension)
    {
        if (targetObj == null) return;
        targetObj.transform.localScale = Vector3.one;
        Renderer[] rends = targetObj.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;

        Bounds bounds = GetCombinedBounds(rends);
        float currentMax = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (currentMax > 0.0001f)
        {
            float scaleFactor = targetMaxDimension / currentMax;
            targetObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }
    }

    private void SnapMeshLowestLocalYToSurface(GameObject targetObj, Transform relativeToParent, float targetSurfaceLocalY, float paddingOffset)
    {
        if (targetObj == null || relativeToParent == null) return;
        Renderer[] renderers = targetObj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds localBounds = GetLocalCombinedBounds(renderers, relativeToParent);
        float currentLowestLocalY = localBounds.min.y;
        float deltaY = (targetSurfaceLocalY + paddingOffset) - currentLowestLocalY;

        targetObj.transform.localPosition += new Vector3(0f, deltaY, 0f);
    }

    private Bounds GetLocalCombinedBounds(Renderer[] renderers, Transform relativeToParent)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var r in renderers)
        {
            Bounds b = r.bounds;
            Vector3[] corners = new Vector3[8];
            corners[0] = relativeToParent.InverseTransformPoint(new Vector3(b.min.x, b.min.y, b.min.z));
            corners[1] = relativeToParent.InverseTransformPoint(new Vector3(b.max.x, b.min.y, b.min.z));
            corners[2] = relativeToParent.InverseTransformPoint(new Vector3(b.min.x, b.max.y, b.min.z));
            corners[3] = relativeToParent.InverseTransformPoint(new Vector3(b.max.x, b.max.y, b.min.z));
            corners[4] = relativeToParent.InverseTransformPoint(new Vector3(b.min.x, b.min.y, b.max.z));
            corners[5] = relativeToParent.InverseTransformPoint(new Vector3(b.max.x, b.min.y, b.max.z));
            corners[6] = relativeToParent.InverseTransformPoint(new Vector3(b.min.x, b.max.y, b.max.z));
            corners[7] = relativeToParent.InverseTransformPoint(new Vector3(b.max.x, b.max.y, b.max.z));

            for (int i = 0; i < 8; i++)
            {
                min = Vector3.Min(min, corners[i]);
                max = Vector3.Max(max, corners[i]);
            }
        }

        Bounds localBounds = new Bounds();
        localBounds.SetMinMax(min, max);
        return localBounds;
    }

    private void SnapMeshLowestYToSurface(GameObject targetObj, float targetSurfaceWorldY, float paddingOffset)
    {
        if (targetObj == null) return;
        Renderer[] renderers = targetObj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = GetCombinedBounds(renderers);
        float currentLowestY = bounds.min.y;
        float deltaY = (targetSurfaceWorldY + paddingOffset) - currentLowestY;

        targetObj.transform.position += new Vector3(0f, deltaY, 0f);
    }

    private void CreateHazardCollider(GameObject cylinder, Bounds worldBounds)
    {
        if (cylinder.GetComponentInChildren<BoxCollider>() != null) return;

        GameObject colliderObject = new GameObject("GasHazardCollider");
        colliderObject.transform.SetParent(cylinder.transform, false);

        BoxCollider boxCollider = colliderObject.AddComponent<BoxCollider>();
        Vector3 localCenter = cylinder.transform.InverseTransformPoint(worldBounds.center);
        boxCollider.center = localCenter;
        boxCollider.size = new Vector3(0.30f, 1.22f, 0.30f);
        boxCollider.isTrigger = false;
    }

    private Bounds GetCombinedBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    public void ClearEnvironment()
    {
        GasLeakVisualController gasLeak = GetComponent<GasLeakVisualController>();
        if (gasLeak != null)
        {
            gasLeak.StopGasLeak();
        }

        if (currentGasCylinder != null) { SafeDestroy(currentGasCylinder); currentGasCylinder = null; }
        if (currentGasDetector != null) { SafeDestroy(currentGasDetector); currentGasDetector = null; }
        if (currentH2SDetector != null) { SafeDestroy(currentH2SDetector); currentH2SDetector = null; }
        if (currentPipelineAssembly != null) { SafeDestroy(currentPipelineAssembly); currentPipelineAssembly = null; }
        if (currentIndustrialEnvironment != null) { SafeDestroy(currentIndustrialEnvironment); currentIndustrialEnvironment = null; }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            SafeDestroy(child.gameObject);
        }
    }

    private void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Object.Destroy(obj);
        else Object.DestroyImmediate(obj);
    }
}