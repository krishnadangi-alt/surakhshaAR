using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class FireTrainingWorkflowFixer
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\fix_ar_trigger.txt";
        private const string ReportFile = @"C:\project\surakshaAR\Temp\ar_fix_report.txt";

        static FireTrainingWorkflowFixer()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                FixAndAlignARWorkflow();
            }
        }

        [MenuItem("SurakshaAR/Fix AR Fire Training Scene & Objects")]
        public static void FixAndAlignARWorkflow()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SURAKSHAAR AR WORKFLOW & SCENE FIXER ===");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    sb.AppendLine($"[ERROR] Failed to open scene at {scenePath}");
                    File.WriteAllText(ReportFile, sb.ToString());
                    return;
                }

                // -------------------------------------------------------------
                // 1. CREATE COMMON MATERIALS (Chrome Pin Ring & Bright Red/Yellow Tamper Seal)
                // -------------------------------------------------------------
                string matFolder = "Assets/AR_Fire_foundation/Materials";
                if (!AssetDatabase.IsValidFolder(matFolder))
                {
                    AssetDatabase.CreateFolder("Assets/AR_Fire_foundation", "Materials");
                }

                string chromeMatPath = $"{matFolder}/M_SafetyPin_Chrome.mat";
                Material chromeMat = AssetDatabase.LoadAssetAtPath<Material>(chromeMatPath);
                if (chromeMat == null)
                {
                    chromeMat = new Material(Shader.Find("Standard"));
                    chromeMat.color = new Color(0.92f, 0.93f, 0.95f, 1f);
                    chromeMat.SetFloat("_Metallic", 0.95f);
                    chromeMat.SetFloat("_Glossiness", 0.88f);
                    AssetDatabase.CreateAsset(chromeMat, chromeMatPath);
                }

                string sealMatPath = $"{matFolder}/M_SafetyPin_Seal.mat";
                Material sealMat = AssetDatabase.LoadAssetAtPath<Material>(sealMatPath);
                if (sealMat == null)
                {
                    sealMat = new Material(Shader.Find("Standard"));
                    sealMat.color = new Color(1.0f, 0.22f, 0.05f, 1f); // Bright safety red/orange
                    sealMat.SetFloat("_Metallic", 0.1f);
                    sealMat.SetFloat("_Glossiness", 0.6f);
                    AssetDatabase.CreateAsset(sealMat, sealMatPath);
                }

                // Load Audio Clips
                AudioClip sirenClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AR_Fire_foundation/audio/WhatsApp Audio 2026-09-07 at 04.40.03.wav");
                AudioClip sprayClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AR_Fire_foundation/audio/fire Extigusher/extinguisher_spray_10s_seamless_loop.mp3");
                AudioClip fireClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Vefects/Free Fire VFX/Audio/SFX_FireMedium_L.wav");

                sb.AppendLine($"[AUDIO] Siren Clip: {(sirenClip != null ? sirenClip.name : "NULL")}");
                sb.AppendLine($"[AUDIO] Spray Clip: {(sprayClip != null ? sprayClip.name : "NULL")}");
                sb.AppendLine($"[AUDIO] Fire Clip: {(fireClip != null ? fireClip.name : "NULL")}");

                // -------------------------------------------------------------
                // 2. LOCATE ROOTS & SAFETY EQUIPMENT
                // -------------------------------------------------------------
                GameObject scenarioRoot = GameObject.Find("FireScenario") ?? GameObject.Find("Scenario");
                GameObject safetyEquipment = GameObject.Find("SafetyEquipment");
                GameObject fireExtDisplay = GameObject.Find("FireExt_display");
                GameObject fireExt = GameObject.Find("FireExt");

                if (safetyEquipment == null && fireExt != null)
                    safetyEquipment = fireExt.transform.parent?.gameObject;

                sb.AppendLine($"[ROOTS] Scenario: {scenarioRoot?.name}, SafetyEquip: {safetyEquipment?.name}, FireExt: {fireExt?.name}, Display: {fireExtDisplay?.name}");

                // -------------------------------------------------------------
                // 3. FIX FIRE ALARM PROP & ALIGNMENT
                // -------------------------------------------------------------
                GameObject fireAlarm = GameObject.Find("FireAlarm");
                if (fireAlarm != null)
                {
                    // Move out of FireExt_display if parented there
                    if (safetyEquipment != null && fireAlarm.transform.parent != safetyEquipment.transform)
                    {
                        fireAlarm.transform.SetParent(safetyEquipment.transform, false);
                        sb.AppendLine("[ALARM] Unparented FireAlarm from display extinguisher -> parented to SafetyEquipment");
                    }

                    // Set standard visible wall position
                    fireAlarm.transform.localPosition = new Vector3(1.2f, 1.25f, 0.4f);
                    fireAlarm.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    fireAlarm.transform.localScale = Vector3.one;

                    Transform euf = fireAlarm.transform.Find("EUfFireAlarm");
                    if (euf != null)
                    {
                        euf.localPosition = Vector3.zero;
                        euf.localRotation = Quaternion.Euler(270f, 0f, 0f);
                        euf.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                    }

                    // Add/Configure BoxCollider
                    var col = fireAlarm.GetComponent<BoxCollider>();
                    if (col == null) col = fireAlarm.AddComponent<BoxCollider>();
                    col.center = new Vector3(0f, 0f, 0f);
                    col.size = new Vector3(0.45f, 0.55f, 0.35f);

                    // Add/Configure AudioSource
                    var audio = fireAlarm.GetComponent<AudioSource>();
                    if (audio == null) audio = fireAlarm.AddComponent<AudioSource>();
                    if (sirenClip != null) audio.clip = sirenClip;
                    audio.playOnAwake = false;
                    audio.loop = false;
                    audio.spatialBlend = 0.6f;
                    audio.volume = 1.0f;

                    // Add/Configure AlarmInteraction
                    var alarmComp = fireAlarm.GetComponent<AlarmInteraction>();
                    if (alarmComp == null) alarmComp = fireAlarm.AddComponent<AlarmInteraction>();

                    sb.AppendLine($"[ALARM] Configured FireAlarm at localPos {fireAlarm.transform.localPosition}, BoxCollider: {col.size}, Audio: {audio.clip?.name}");
                }
                else
                {
                    sb.AppendLine("[ALARM] WARNING: FireAlarm GameObject not found in scene!");
                }

                // -------------------------------------------------------------
                // 4. FIX SAFETY PIN & PULL-RING ON EXTINGUISHERS
                // -------------------------------------------------------------
                Mesh ringMesh = CreateTorusMesh(0.024f, 0.0035f, 18, 8);

                void SetupPinWithRing(GameObject extGO, string label)
                {
                    if (extGO == null) return;
                    Transform pinT = extGO.transform.Find("Pin");
                    if (pinT == null) return;

                    // Ensure FirePinInteraction
                    var pinComp = pinT.GetComponent<FirePinInteraction>();
                    if (pinComp == null) pinComp = pinT.gameObject.AddComponent<FirePinInteraction>();

                    // Ensure comfortable raycast box collider on Pin
                    var pCol = pinT.GetComponent<BoxCollider>();
                    if (pCol == null) pCol = pinT.gameObject.AddComponent<BoxCollider>();
                    pCol.center = new Vector3(0.0f, 0.01f, 0.45f);
                    pCol.size = new Vector3(0.14f, 0.14f, 0.14f);

                    // Attach prominent PullRing
                    Transform existingRing = pinT.Find("SafetyPullRing");
                    if (existingRing != null)
                    {
                        UnityEngine.Object.DestroyImmediate(existingRing.gameObject);
                    }

                    GameObject ringGO = new GameObject("SafetyPullRing");
                    ringGO.transform.SetParent(pinT, false);
                    ringGO.transform.localPosition = new Vector3(0.02f, 0.015f, 0.46f);
                    ringGO.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    ringGO.transform.localScale = Vector3.one;

                    var mf = ringGO.AddComponent<MeshFilter>();
                    mf.sharedMesh = ringMesh;

                    var mr = ringGO.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = chromeMat;

                    // Attach bright safety tag (Tamper Seal)
                    GameObject sealGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sealGO.name = "TamperSeal";
                    sealGO.transform.SetParent(ringGO.transform, false);
                    var sCol = sealGO.GetComponent<Collider>();
                    if (sCol != null) UnityEngine.Object.DestroyImmediate(sCol);

                    sealGO.transform.localPosition = new Vector3(0f, -0.025f, 0.01f);
                    sealGO.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                    sealGO.transform.localScale = new Vector3(0.003f, 0.035f, 0.018f);

                    var sealMR = sealGO.GetComponent<MeshRenderer>();
                    if (sealMR != null) sealMR.sharedMaterial = sealMat;

                    sb.AppendLine($"[PIN] {label}: Attached high-visibility PullRing (chrome) and TamperSeal (safety red). BoxCollider: {pCol.size}");
                }

                SetupPinWithRing(fireExt, "Equipped FireExt");
                SetupPinWithRing(fireExtDisplay, "Display FireExt_display");

                // -------------------------------------------------------------
                // 5. CONFIGURE EXTINGUISHER HOLD POSITION & NOZZLE FORWARD
                // -------------------------------------------------------------
                if (fireExt != null)
                {
                    // Let ExtinguisherPickup script and its Inspector fields (holdPosition & holdRotation) control the extinguisher positioning on pickup exclusively
                    /*
                    if (pickup != null)
                    {
                        pickup.holdPosition = new Vector3(0f, -0.70f, 0.80f);
                        pickup.holdRotation = new Vector3(0f, 20f, 0f);
                        sb.AppendLine($"[PICKUP] Reverted holdPosition to {pickup.holdPosition}, holdRotation to {pickup.holdRotation}");
                    }
                    */

                    // ---------------------------------------------------------
                    // 6. SPRAY POINT & SPRAY PARTICLE SYSTEM
                    // ---------------------------------------------------------
                    Transform spraypoint = fireExt.transform.Find("spraypoint");
                    if (spraypoint != null)
                    {
                        // Align spraypoint forward with nozzle
                        spraypoint.localPosition = new Vector3(-0.02f, 0.61f, 0.35f);
                        spraypoint.localRotation = Quaternion.Euler(0f, 0f, 0f);

                        Transform sprayT = spraypoint.Find("spray");
                        if (sprayT != null)
                        {
                            sprayT.localPosition = Vector3.zero;
                            sprayT.localRotation = Quaternion.identity; // Forward aligns with spraypoint & nozzle!
                            sprayT.localScale = Vector3.one;

                            var ps = sprayT.GetComponent<ParticleSystem>();
                            if (ps != null)
                            {
                                var main = ps.main;
                                main.simulationSpace = ParticleSystemSimulationSpace.World;
                                main.startSpeed = 8.0f;
                                main.startSize = 0.45f;
                                main.startLifetime = 1.1f;
                                main.playOnAwake = false;
                                main.loop = true;
                                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

                                var emission = ps.emission;
                                emission.rateOverTime = 380f;

                                var shape = ps.shape;
                                shape.enabled = true;
                                shape.shapeType = ParticleSystemShapeType.Cone;
                                shape.angle = 6.0f;
                                shape.radius = 0.02f;
                                shape.rotation = Vector3.zero; // Straight along nozzle forward!

                                var collision = ps.collision;
                                collision.enabled = false; // VISUAL SPRAY: COLLISION OFF!

                                var renderer = sprayT.GetComponent<ParticleSystemRenderer>();
                                if (renderer != null) renderer.enabled = true;

                                sb.AppendLine($"[SPRAY] Configured Visual Spray: simSpace=World, collision=OFF, speed=8.0, rate=380, rot={sprayT.localEulerAngles}");
                            }

                            // Configure Invisible Collision Detector probe on SprayCollision child
                            Transform colT = spraypoint.Find("SprayCollision");
                            if (colT == null)
                            {
                                var colGo = new GameObject("SprayCollision");
                                colT = colGo.transform;
                                colT.SetParent(spraypoint, false);
                            }
                            colT.localPosition = new Vector3(0f, 0f, 0.20f); // Offset 20cm ahead of nozzle tip
                            colT.localRotation = Quaternion.identity;
                            colT.localScale = Vector3.one;

                            var colPS = colT.GetComponent<ParticleSystem>();
                            if (colPS == null) colPS = colT.gameObject.AddComponent<ParticleSystem>();

                            var colMain = colPS.main;
                            colMain.simulationSpace = ParticleSystemSimulationSpace.World;
                            colMain.startSpeed = 8.0f;
                            colMain.startSize = 0.1f;
                            colMain.startLifetime = 1.1f;
                            colMain.playOnAwake = false;
                            colMain.loop = true;

                            var colEmission = colPS.emission;
                            colEmission.rateOverTime = 25f; // Lightweight probe emission

                            var colShape = colPS.shape;
                            colShape.enabled = true;
                            colShape.shapeType = ParticleSystemShapeType.Cone;
                            colShape.angle = 6.0f;
                            colShape.radius = 0.02f;
                            colShape.rotation = Vector3.zero;

                            var colModule = colPS.collision;
                            colModule.enabled = true;
                            colModule.type = ParticleSystemCollisionType.World;
                            colModule.mode = ParticleSystemCollisionMode.Collision3D;
                            colModule.sendCollisionMessages = true;
                            colModule.quality = ParticleSystemCollisionQuality.High;
                            colModule.bounce = 0f;
                            colModule.dampen = 0.5f;
                            colModule.radiusScale = 0.25f;

                            var colRend = colT.GetComponent<ParticleSystemRenderer>();
                            if (colRend != null) colRend.enabled = false; // Completely invisible!

                            // AudioSource on spray
                            var sprayAudio = sprayT.GetComponent<AudioSource>();
                            if (sprayAudio == null) sprayAudio = sprayT.gameObject.AddComponent<AudioSource>();
                            if (sprayClip != null) sprayAudio.clip = sprayClip;
                            sprayAudio.playOnAwake = false;
                            sprayAudio.loop = true;
                            sprayAudio.volume = 0.85f;

                            // Components
                            var dryPowder = sprayT.GetComponent<DryPowderSpray>();
                            if (dryPowder == null) dryPowder = sprayT.gameObject.AddComponent<DryPowderSpray>();
                            dryPowder.sprayParticles = ps;
                            dryPowder.collisionParticles = colPS;
                            dryPowder.sprayPoint = spraypoint;

                            var sprayCol = colT.GetComponent<ExtinguisherSprayCollision>();
                            if (sprayCol == null) sprayCol = colT.gameObject.AddComponent<ExtinguisherSprayCollision>();

                            // Remove old ExtinguisherSprayCollision from sprayT if present
                            var oldCol = sprayT.GetComponent<ExtinguisherSprayCollision>();
                            if (oldCol != null) UnityEngine.Object.DestroyImmediate(oldCol);

                            sb.AppendLine($"[SPRAY] AudioSource assigned {sprayAudio.clip?.name}, Visual Spray & Invisible SprayCollision probe confirmed.");
                        }
                    }

                    // Extinguisher Grip
                    Transform gripT = fireExt.transform.Find("Grip");
                    if (gripT != null)
                    {
                        var gripCol = gripT.GetComponent<BoxCollider>();
                        if (gripCol == null) gripCol = gripT.gameObject.AddComponent<BoxCollider>();
                        gripCol.center = new Vector3(0f, 0.01f, 0.46f);
                        gripCol.size = new Vector3(0.15f, 0.12f, 0.15f);

                        var gripComp = gripT.GetComponent<ExtinguisherGripInteraction>();
                        if (gripComp == null) gripComp = gripT.gameObject.AddComponent<ExtinguisherGripInteraction>();
                        sb.AppendLine($"[GRIP] Grip BoxCollider {gripCol.size}, ExtinguisherGripInteraction confirmed.");
                    }
                }

                // -------------------------------------------------------------
                // 7. FIX FIRE COLLIDER & FIRE AUDIO ON VFX_Fire_01_Small
                // -------------------------------------------------------------
                GameObject fireGO = GameObject.Find("VFX_Fire_01_Small");
                if (fireGO != null)
                {
                    var fCol = fireGO.GetComponent<BoxCollider>();
                    if (fCol == null) fCol = fireGO.AddComponent<BoxCollider>();
                    fCol.center = new Vector3(0f, 0.30f, 0f); // Centered directly on the flame base!
                    fCol.size = new Vector3(1.1f, 0.85f, 1.1f);
                    fCol.isTrigger = false;

                    var fAudio = fireGO.GetComponent<AudioSource>();
                    if (fAudio == null) fAudio = fireGO.AddComponent<AudioSource>();
                    if (fireClip != null) fAudio.clip = fireClip;
                    fAudio.playOnAwake = true;
                    fAudio.loop = true;
                    fAudio.volume = 0.8f;
                    fAudio.spatialBlend = 0.8f;

                    // Ensure FireExtinguishable exists and references this fire particle
                    var fireExtComp = UnityEngine.Object.FindAnyObjectByType<FireExtinguishable>();
                    if (fireExtComp != null)
                    {
                        var fireField = typeof(FireExtinguishable).GetField("fireParticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (fireField != null)
                        {
                            var ps = fireGO.GetComponent<ParticleSystem>();
                            fireField.SetValue(fireExtComp, ps);
                        }
                    }

                    sb.AppendLine($"[FIRE] VFX_Fire_01_Small: Centered BoxCollider to {fCol.center}, size {fCol.size}, AudioSource assigned {fAudio.clip?.name}");
                }
                else
                {
                    sb.AppendLine("[FIRE] WARNING: VFX_Fire_01_Small GameObject not found!");
                }

                // -------------------------------------------------------------
                // 8. UPDATE SCENARIO ALIGNMENT SCRIPT & FLOW MANAGER
                // -------------------------------------------------------------
                var alignment = UnityEngine.Object.FindAnyObjectByType<FireScenarioAlignment>();
                if (alignment != null)
                {
                    if (fireAlarm != null)
                    {
                        alignment.fireAlarm = fireAlarm.transform;
                        alignment.fireAlarmPosition = new Vector3(1.2f, 1.25f, 0.4f);
                        alignment.fireAlarmRotation = new Vector3(0f, 180f, 0f);
                    }
                    alignment.ApplyAlignment();
                    sb.AppendLine("[ALIGNMENT] Updated FireScenarioAlignment references & applied alignment.");
                }

                var flow = UnityEngine.Object.FindAnyObjectByType<FireScenarioFlowManager>();
                if (flow != null)
                {
                    flow.SendMessage("ResolveReferences", SendMessageOptions.DontRequireReceiver);
                    sb.AppendLine("[FLOW] Invoked ResolveReferences on FireScenarioFlowManager.");
                }

                // Save scene
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                sb.AppendLine("[SCENE] Successfully saved FireTraining.unity!");

                File.WriteAllText(ReportFile, sb.ToString());
                Debug.Log(sb.ToString());
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[EXCEPTION] {ex}");
                File.WriteAllText(ReportFile, sb.ToString());
                Debug.LogError($"[FireTrainingWorkflowFixer EXCEPTION] {ex}");
            }
        }

        private static Mesh CreateTorusMesh(float majorRadius, float minorRadius, int majorSegments, int minorSegments)
        {
            var mesh = new Mesh();
            mesh.name = "Procedural_Torus_PullRing";

            int numVertices = (majorSegments + 1) * (minorSegments + 1);
            Vector3[] vertices = new Vector3[numVertices];
            Vector3[] normals = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];

            int vi = 0;
            for (int i = 0; i <= majorSegments; i++)
            {
                float u = (float)i / majorSegments;
                float theta = u * Mathf.PI * 2f;
                float cosTheta = Mathf.Cos(theta);
                float sinTheta = Mathf.Sin(theta);

                Vector3 center = new Vector3(cosTheta * majorRadius, 0f, sinTheta * majorRadius);

                for (int j = 0; j <= minorSegments; j++)
                {
                    float v = (float)j / minorSegments;
                    float phi = v * Mathf.PI * 2f;
                    float cosPhi = Mathf.Cos(phi);
                    float sinPhi = Mathf.Sin(phi);

                    Vector3 normal = new Vector3(cosTheta * cosPhi, sinPhi, sinTheta * cosPhi);
                    vertices[vi] = center + normal * minorRadius;
                    normals[vi] = normal;
                    uvs[vi] = new Vector2(u, v);
                    vi++;
                }
            }

            int numTriangles = majorSegments * minorSegments * 6;
            int[] triangles = new int[numTriangles];
            int ti = 0;

            for (int i = 0; i < majorSegments; i++)
            {
                for (int j = 0; j < minorSegments; j++)
                {
                    int current = i * (minorSegments + 1) + j;
                    int next = current + minorSegments + 1;

                    triangles[ti++] = current;
                    triangles[ti++] = current + 1;
                    triangles[ti++] = next;

                    triangles[ti++] = next;
                    triangles[ti++] = current + 1;
                    triangles[ti++] = next + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
