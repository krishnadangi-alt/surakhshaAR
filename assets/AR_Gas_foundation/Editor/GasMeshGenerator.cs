#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GasMeshGenerator
{
    [MenuItem("SurakshaAR/Generate 3D Industrial Equipment Meshes")]
    public static void GenerateAllMeshes()
    {
        Debug.Log("[GasMeshGenerator] Generating 3D OBJ Assets...");

        string regDir = "Assets/AR_Gas_foundation/3d/GasRegulator";
        string gaugeDir = "Assets/AR_Gas_foundation/3d/PressureGauge";
        string pipeDir = "Assets/AR_Gas_foundation/3d/GasPipeline";

        if (!Directory.Exists(regDir)) Directory.CreateDirectory(regDir);
        if (!Directory.Exists(gaugeDir)) Directory.CreateDirectory(gaugeDir);
        if (!Directory.Exists(pipeDir)) Directory.CreateDirectory(pipeDir);

        GenerateRegulatorOBJ(Path.Combine(regDir, "gas_regulator.obj"));
        GeneratePressureGaugeOBJ(Path.Combine(gaugeDir, "pressure_gauge.obj"));
        GenerateIndustrialPipeOBJ(Path.Combine(pipeDir, "industrial_pipe.obj"));
        GeneratePipeElbowOBJ(Path.Combine(pipeDir, "pipe_elbow.obj"));

        AssetDatabase.Refresh();
        Debug.Log("[GasMeshGenerator] All 3D OBJ Assets Generated & Refreshed Successfully!");
    }

    // =========================================================================
    // 1. GAS REGULATOR OBJ GENERATOR
    // =========================================================================
    private static void GenerateRegulatorOBJ(string path)
    {
        OBJBuilder builder = new OBJBuilder("gas_regulator");

        // Main Diaphragm Chamber (Cylinder along X axis, diameter 0.07m, thickness 0.04m, center X=0.08)
        builder.AddCylinder(new Vector3(0.08f, 0f, 0f), 0.035f, 0.04f, 24, Vector3.right, "RegulatorChamber");

        // Inlet Stem & Hex Coupling Nut (X: 0.00 to 0.06, diameter 0.022m)
        builder.AddCylinder(new Vector3(0.03f, 0f, 0f), 0.012f, 0.06f, 16, Vector3.right, "InletStem");
        builder.AddCylinder(new Vector3(0.015f, 0f, 0f), 0.018f, 0.018f, 6, Vector3.right, "InletHexNut");

        // Outlet Stem (X: 0.10 to 0.15, diameter 0.02f)
        builder.AddCylinder(new Vector3(0.125f, 0f, 0f), 0.011f, 0.05f, 16, Vector3.right, "OutletStem");
        builder.AddCylinder(new Vector3(0.145f, 0f, 0f), 0.016f, 0.012f, 6, Vector3.right, "OutletCoupling");

        // Pressure Adjustment Knob (Top Y-axis extension at X=0.08, Y: 0.02 to 0.05)
        builder.AddCylinder(new Vector3(0.08f, 0.035f, 0f), 0.014f, 0.03f, 16, Vector3.up, "KnobStem");
        builder.AddCylinder(new Vector3(0.08f, 0.055f, 0f), 0.022f, 0.012f, 12, Vector3.up, "AdjustmentKnob");

        // Gauge Port Mount (Top Y-axis stem at X=0.05, Y: 0.02 to 0.04)
        builder.AddCylinder(new Vector3(0.05f, 0.03f, 0f), 0.009f, 0.025f, 12, Vector3.up, "GaugePort");

        File.WriteAllText(path, builder.Build());
        Debug.Log($"[GasMeshGenerator] Saved: {path}");
    }

    // =========================================================================
    // 2. PRESSURE GAUGE OBJ GENERATOR
    // =========================================================================
    private static void GeneratePressureGaugeOBJ(string path)
    {
        OBJBuilder builder = new OBJBuilder("pressure_gauge");

        // Bottom Mounting Stem (Nipple extending down along Y axis: Y=-0.03 to 0.00, diameter 0.012m)
        builder.AddCylinder(new Vector3(0f, -0.018f, 0f), 0.006f, 0.035f, 12, Vector3.up, "MountingStem");

        // Circular Dial Housing / Back Casing (Z-facing dial, diameter 0.08m, depth 0.025m)
        builder.AddCylinder(new Vector3(0f, 0.04f, 0f), 0.040f, 0.022f, 32, Vector3.forward, "GaugeHousing");

        // Front Bezel Ring (slight flare at front Z=-0.012)
        builder.AddCylinder(new Vector3(0f, 0.04f, -0.012f), 0.042f, 0.005f, 32, Vector3.forward, "BezelRing");

        // Dial Face Plate (recessed inside bezel at Z=-0.010)
        builder.AddCylinder(new Vector3(0f, 0.04f, -0.010f), 0.038f, 0.002f, 32, Vector3.forward, "DialFace");

        // Pointer / Needle (3D angled wedge at center facing Z=-0.013)
        builder.AddBox(new Vector3(0.008f, 0.048f, -0.013f), new Vector3(0.003f, 0.028f, 0.002f), "GaugePointer");

        // Transparent Front Glass Cover (Z=-0.014)
        builder.AddCylinder(new Vector3(0f, 0.04f, -0.014f), 0.040f, 0.001f, 32, Vector3.forward, "GlassCover");

        File.WriteAllText(path, builder.Build());
        Debug.Log($"[GasMeshGenerator] Saved: {path}");
    }

    // =========================================================================
    // 3. INDUSTRIAL PIPE OBJ GENERATOR
    // =========================================================================
    private static void GenerateIndustrialPipeOBJ(string path)
    {
        OBJBuilder builder = new OBJBuilder("industrial_pipe");

        // Straight Industrial Pipe Section along Z axis (Length 0.20m, Outer Diameter 0.038m, Inner Diameter 0.032m)
        float length = 0.20f;
        float radius = 0.019f;
        builder.AddCylinder(new Vector3(0f, 0f, length * 0.5f), radius, length, 24, Vector3.forward, "PipeTube");

        // Flange Rings at both ends
        builder.AddCylinder(new Vector3(0f, 0f, 0.006f), 0.028f, 0.012f, 24, Vector3.forward, "FlangeStart");
        builder.AddCylinder(new Vector3(0f, 0f, length - 0.006f), 0.028f, 0.012f, 24, Vector3.forward, "FlangeEnd");

        File.WriteAllText(path, builder.Build());
        Debug.Log($"[GasMeshGenerator] Saved: {path}");
    }

    // =========================================================================
    // 4. PIPE ELBOW OBJ GENERATOR
    // =========================================================================
    private static void GeneratePipeElbowOBJ(string path)
    {
        OBJBuilder builder = new OBJBuilder("pipe_elbow");

        // 90-degree Curved Torus Pipe Bend from horizontal +X axis to vertical -Y axis
        float pipeRadius = 0.019f;
        float bendRadius = 0.05f;
        int bendSegments = 16;
        int radialSegments = 16;

        builder.AddTorusArc(Vector3.zero, bendRadius, pipeRadius, 0f, 90f, bendSegments, radialSegments, "ElbowBend");

        // Connection Flanges at inlet (horizontal X) and outlet (vertical Y)
        builder.AddCylinder(new Vector3(bendRadius, 0f, 0f), 0.028f, 0.012f, 24, Vector3.right, "FlangeInlet");
        builder.AddCylinder(new Vector3(0f, -bendRadius, 0f), 0.028f, 0.012f, 24, Vector3.down, "FlangeOutlet");

        File.WriteAllText(path, builder.Build());
        Debug.Log($"[GasMeshGenerator] Saved: {path}");
    }

    // =========================================================================
    // HELPER: OBJ WAVEFRONT FORMAT BUILDER
    // =========================================================================
    private class OBJBuilder
    {
        private readonly string name;
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<int[]> faces = new List<int[]>(); // Each face is array of vertex indices (1-indexed)

        public OBJBuilder(string name)
        {
            this.name = name;
        }

        public void AddCylinder(Vector3 center, float radius, float height, int segments, Vector3 axis, string groupName)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, axis.normalized);
            int baseVertIndex = vertices.Count + 1;

            float halfH = height * 0.5f;

            // Top and Bottom center points
            Vector3 topCenterLocal = new Vector3(0f, halfH, 0f);
            Vector3 bottomCenterLocal = new Vector3(0f, -halfH, 0f);

            // Ring vertices
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                Vector3 posBottom = center + rot * new Vector3(x, -halfH, z);
                Vector3 posTop = center + rot * new Vector3(x, halfH, z);
                Vector3 normalSide = rot * new Vector3(x, 0f, z).normalized;

                vertices.Add(posBottom);
                normals.Add(normalSide);
                uvs.Add(new Vector2((float)i / segments, 0f));

                vertices.Add(posTop);
                normals.Add(normalSide);
                uvs.Add(new Vector2((float)i / segments, 1f));
            }

            // Side Quad Faces
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int b1 = baseVertIndex + i * 2;
                int t1 = b1 + 1;
                int b2 = baseVertIndex + next * 2;
                int t2 = b2 + 1;

                faces.Add(new int[] { b1, b2, t2, t1 });
            }

            // Top Cap
            int topCenterIdx = vertices.Count + 1;
            vertices.Add(center + rot * topCenterLocal);
            normals.Add(rot * Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int topRingStart = vertices.Count + 1;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                vertices.Add(center + rot * new Vector3(x, halfH, z));
                normals.Add(rot * Vector3.up);
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                faces.Add(new int[] { topCenterIdx, topRingStart + i, topRingStart + next });
            }

            // Bottom Cap
            int bottomCenterIdx = vertices.Count + 1;
            vertices.Add(center + rot * bottomCenterLocal);
            normals.Add(rot * Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int bottomRingStart = vertices.Count + 1;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                vertices.Add(center + rot * new Vector3(x, -halfH, z));
                normals.Add(rot * Vector3.down);
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                faces.Add(new int[] { bottomCenterIdx, bottomRingStart + next, bottomRingStart + i });
            }
        }

        public void AddBox(Vector3 center, Vector3 size, string groupName)
        {
            int baseVertIdx = vertices.Count + 1;
            Vector3 h = size * 0.5f;

            Vector3[] c = new Vector3[]
            {
                center + new Vector3(-h.x, -h.y, -h.z),
                center + new Vector3( h.x, -h.y, -h.z),
                center + new Vector3( h.x,  h.y, -h.z),
                center + new Vector3(-h.x,  h.y, -h.z),
                center + new Vector3(-h.x, -h.y,  h.z),
                center + new Vector3( h.x, -h.y,  h.z),
                center + new Vector3( h.x,  h.y,  h.z),
                center + new Vector3(-h.x,  h.y,  h.z),
            };

            foreach (var v in c)
            {
                vertices.Add(v);
                normals.Add((v - center).normalized);
                uvs.Add(new Vector2(0.5f, 0.5f));
            }

            // 6 Quad faces
            faces.Add(new int[] { baseVertIdx + 0, baseVertIdx + 3, baseVertIdx + 2, baseVertIdx + 1 }); // Front
            faces.Add(new int[] { baseVertIdx + 5, baseVertIdx + 6, baseVertIdx + 7, baseVertIdx + 4 }); // Back
            faces.Add(new int[] { baseVertIdx + 4, baseVertIdx + 7, baseVertIdx + 3, baseVertIdx + 0 }); // Left
            faces.Add(new int[] { baseVertIdx + 1, baseVertIdx + 2, baseVertIdx + 6, baseVertIdx + 5 }); // Right
            faces.Add(new int[] { baseVertIdx + 3, baseVertIdx + 7, baseVertIdx + 6, baseVertIdx + 2 }); // Top
            faces.Add(new int[] { baseVertIdx + 4, baseVertIdx + 0, baseVertIdx + 1, baseVertIdx + 5 }); // Bottom
        }

        public void AddTorusArc(Vector3 center, float bendRadius, float pipeRadius, float startAngleDeg, float endAngleDeg, int bendSegments, int radialSegments, string groupName)
        {
            int baseVertIdx = vertices.Count + 1;

            for (int b = 0; b <= bendSegments; b++)
            {
                float t = (float)b / bendSegments;
                float bendAngle = Mathf.Deg2Rad * Mathf.Lerp(startAngleDeg, endAngleDeg, t);

                Vector3 ringCenter = center + new Vector3(Mathf.Cos(bendAngle) * bendRadius, -Mathf.Sin(bendAngle) * bendRadius, 0f);
                Vector3 tangent = new Vector3(-Mathf.Sin(bendAngle), -Mathf.Cos(bendAngle), 0f);
                Vector3 normalRad = new Vector3(Mathf.Cos(bendAngle), -Mathf.Sin(bendAngle), 0f);
                Vector3 binormal = Vector3.forward;

                for (int r = 0; r < radialSegments; r++)
                {
                    float radAngle = (float)r / radialSegments * Mathf.PI * 2f;
                    Vector3 offset = normalRad * Mathf.Cos(radAngle) * pipeRadius + binormal * Mathf.Sin(radAngle) * pipeRadius;

                    vertices.Add(ringCenter + offset);
                    normals.Add(offset.normalized);
                    uvs.Add(new Vector2(t, (float)r / radialSegments));
                }
            }

            for (int b = 0; b < bendSegments; b++)
            {
                for (int r = 0; r < radialSegments; r++)
                {
                    int rNext = (r + 1) % radialSegments;
                    int currRing = baseVertIdx + b * radialSegments;
                    int nextRing = baseVertIdx + (b + 1) * radialSegments;

                    int v1 = currRing + r;
                    int v2 = currRing + rNext;
                    int v3 = nextRing + rNext;
                    int v4 = nextRing + r;

                    faces.Add(new int[] { v1, v2, v3, v4 });
                }
            }
        }

        public string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"# OBJ Generated by SurakshaAR GasMeshGenerator");
            sb.AppendLine($"o {name}");

            foreach (var v in vertices)
                sb.AppendLine($"v {v.x:F6} {v.y:F6} {v.z:F6}");

            foreach (var uv in uvs)
                sb.AppendLine($"vt {uv.x:F6} {uv.y:F6}");

            foreach (var n in normals)
                sb.AppendLine($"vn {n.x:F6} {n.y:F6} {n.z:F6}");

            foreach (var f in faces)
            {
                if (f.Length == 3)
                    sb.AppendLine($"f {f[0]}/{f[0]}/{f[0]} {f[1]}/{f[1]}/{f[1]} {f[2]}/{f[2]}/{f[2]}");
                else if (f.Length == 4)
                    sb.AppendLine($"f {f[0]}/{f[0]}/{f[0]} {f[1]}/{f[1]}/{f[1]} {f[2]}/{f[2]}/{f[2]} {f[3]}/{f[3]}/{f[3]}");
            }

            return sb.ToString();
        }
    }
}
#endif
