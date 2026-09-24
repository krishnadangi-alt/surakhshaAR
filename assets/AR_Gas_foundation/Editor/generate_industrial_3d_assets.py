import os
import math

class MultiMatMeshBuilder:
    def __init__(self):
        self.verts = []
        self.uvs = []
        self.normals = []
        self.mat_faces = {} # mat_name -> list of faces
        self.current_mat = "Default"

    def set_material(self, mat_name):
        self.current_mat = mat_name
        if mat_name not in self.mat_faces:
            self.mat_faces[mat_name] = []

    def add_vertex(self, x, y, z, u=0.0, v=0.0, nx=0.0, ny=1.0, nz=0.0):
        self.verts.append((x, y, z))
        self.uvs.append((u, v))
        self.normals.append((nx, ny, nz))
        return len(self.verts) # 1-based index

    def add_box(self, cx, cy, cz, sx, sy, sz, rx=0, ry=0, rz=0):
        hx, hy, hz = sx * 0.5, sy * 0.5, sz * 0.5
        faces_data = [
            [(cx-hx, cy-hy, cz+hz), (cx+hx, cy-hy, cz+hz), (cx+hx, cy+hy, cz+hz), (cx-hx, cy+hy, cz+hz)], # +Z
            [(cx+hx, cy-hy, cz-hz), (cx-hx, cy-hy, cz-hz), (cx-hx, cy+hy, cz-hz), (cx+hx, cy+hy, cz-hz)], # -Z
            [(cx-hx, cy+hy, cz+hz), (cx+hx, cy+hy, cz+hz), (cx+hx, cy+hy, cz-hz), (cx-hx, cy+hy, cz-hz)], # +Y
            [(cx-hx, cy-hy, cz-hz), (cx+hx, cy-hy, cz-hz), (cx+hx, cy-hy, cz+hz), (cx-hx, cy-hy, cz+hz)], # -Y
            [(cx+hx, cy-hy, cz+hz), (cx+hx, cy-hy, cz-hz), (cx+hx, cy+hy, cz-hz), (cx+hx, cy+hy, cz+hz)], # +X
            [(cx-hx, cy-hy, cz-hz), (cx-hx, cy-hy, cz+hz), (cx-hx, cy+hy, cz+hz), (cx-hx, cy+hy, cz-hz)], # -X
        ]
        norm_data = [(0,0,1), (0,0,-1), (0,1,0), (0,-1,0), (1,0,0), (-1,0,0)]

        for i, f_verts in enumerate(faces_data):
            nx, ny, nz = norm_data[i]
            idxs = []
            for j, p in enumerate(f_verts):
                u = 0.0 if j in [0,3] else 1.0
                v = 0.0 if j in [0,1] else 1.0
                idx = self.add_vertex(p[0], p[1], p[2], u, v, nx, ny, nz)
                idxs.append(idx)
            f_list = self.mat_faces.setdefault(self.current_mat, [])
            f_list.append([(idxs[0], idxs[0], idxs[0]), (idxs[1], idxs[1], idxs[1]), (idxs[2], idxs[2], idxs[2])])
            f_list.append([(idxs[0], idxs[0], idxs[0]), (idxs[2], idxs[2], idxs[2]), (idxs[3], idxs[3], idxs[3])])

    def add_cylinder(self, cx, cy, cz, r, h, segments=24, axis='Y'):
        half_h = h * 0.5
        side_idxs = []

        for i in range(segments):
            angle = 2.0 * math.pi * i / segments
            c, s = math.cos(angle), math.sin(angle)
            u = i / segments

            if axis == 'Y':
                px, py, pz = cx + r * c, cy + half_h, cz + r * s
                bx, by, bz = cx + r * c, cy - half_h, cz + r * s
                nx, ny, nz = c, 0.0, s
            elif axis == 'Z':
                px, py, pz = cx + r * c, cy + r * s, cz + half_h
                bx, by, bz = cx + r * c, cy + r * s, cz - half_h
                nx, ny, nz = c, s, 0.0
            else: # X
                px, py, pz = cx + half_h, cy + r * c, cz + r * s
                bx, by, bz = cx - half_h, cy + r * c, cz + r * s
                nx, ny, nz = 0.0, c, s

            t_idx = self.add_vertex(px, py, pz, u, 1.0, nx, ny, nz)
            b_idx = self.add_vertex(bx, by, bz, u, 0.0, nx, ny, nz)
            side_idxs.append((t_idx, b_idx))

        f_list = self.mat_faces.setdefault(self.current_mat, [])
        for i in range(segments):
            next_i = (i + 1) % segments
            t1, b1 = side_idxs[i]
            t2, b2 = side_idxs[next_i]
            f_list.append([(t1, t1, t1), (b1, b1, b1), (b2, b2, b2)])
            f_list.append([(t1, t1, t1), (b2, b2, b2), (t2, t2, t2)])

        # Top and Bottom Caps
        tnx, tny, tnz = (0,1,0) if axis=='Y' else ((0,0,1) if axis=='Z' else (1,0,0))
        bnx, bny, bnz = (0,-1,0) if axis=='Y' else ((0,0,-1) if axis=='Z' else (-1,0,0))

        tc_idx = self.add_vertex(cx + (half_h if axis=='X' else 0), cy + (half_h if axis=='Y' else 0), cz + (half_h if axis=='Z' else 0), 0.5, 0.5, tnx, tny, tnz)
        bc_idx = self.add_vertex(cx - (half_h if axis=='X' else 0), cy - (half_h if axis=='Y' else 0), cz - (half_h if axis=='Z' else 0), 0.5, 0.5, bnx, bny, bnz)

        top_ring = []
        bot_ring = []
        for i in range(segments):
            angle = 2.0 * math.pi * i / segments
            c, s = math.cos(angle), math.sin(angle)
            if axis == 'Y':
                px, py, pz = cx + r * c, cy + half_h, cz + r * s
                bx, by, bz = cx + r * c, cy - half_h, cz + r * s
            elif axis == 'Z':
                px, py, pz = cx + r * c, cy + r * s, cz + half_h
                bx, by, bz = cx + r * c, cy + r * s, cz - half_h
            else:
                px, py, pz = cx + half_h, cy + r * c, cz + r * s
                bx, by, bz = cx - half_h, cy + r * c, cz + r * s

            top_ring.append(self.add_vertex(px, py, pz, 0.5+c*0.5, 0.5+s*0.5, tnx, tny, tnz))
            bot_ring.append(self.add_vertex(bx, by, bz, 0.5+c*0.5, 0.5+s*0.5, bnx, bny, bnz))

        for i in range(segments):
            next_i = (i + 1) % segments
            f_list.append([(tc_idx, tc_idx, tc_idx), (top_ring[i], top_ring[i], top_ring[i]), (top_ring[next_i], top_ring[next_i], top_ring[next_i])])
            f_list.append([(bc_idx, bc_idx, bc_idx), (bot_ring[next_i], bot_ring[next_i], bot_ring[next_i]), (bot_ring[i], bot_ring[i], bot_ring[i])])

    def add_torus(self, cx, cy, cz, R, r, seg_R=24, seg_r=12):
        grid = []
        for i in range(seg_R):
            angle_R = 2.0 * math.pi * i / seg_R
            cos_R, sin_R = math.cos(angle_R), math.sin(angle_R)
            row = []
            for j in range(seg_r):
                angle_r = 2.0 * math.pi * j / seg_r
                cos_r, sin_r = math.cos(angle_r), math.sin(angle_r)

                px = cx + (R + r * cos_r) * cos_R
                py = cy + r * sin_r
                pz = cz + (R + r * cos_r) * sin_R

                nx = cos_r * cos_R
                ny = sin_r
                nz = cos_r * sin_R

                idx = self.add_vertex(px, py, pz, i/seg_R, j/seg_r, nx, ny, nz)
                row.append(idx)
            grid.append(row)

        f_list = self.mat_faces.setdefault(self.current_mat, [])
        for i in range(seg_R):
            next_i = (i + 1) % seg_R
            for j in range(seg_r):
                next_j = (j + 1) % seg_r
                p1 = grid[i][j]
                p2 = grid[next_i][j]
                p3 = grid[next_i][next_j]
                p4 = grid[i][next_j]
                f_list.append([(p1,p1,p1), (p2,p2,p2), (p3,p3,p3)])
                f_list.append([(p1,p1,p1), (p3,p3,p3), (p4,p4,p4)])

    def save(self, filepath):
        with open(filepath, 'w') as f:
            f.write(f"# Multi-Material High-Detail Industrial Asset: {os.path.basename(filepath)}\n")
            for v in self.verts:
                f.write(f"v {v[0]:.6f} {v[1]:.6f} {v[2]:.6f}\n")
            for vt in self.uvs:
                f.write(f"vt {vt[0]:.6f} {vt[1]:.6f}\n")
            for vn in self.normals:
                f.write(f"vn {vn[0]:.6f} {vn[1]:.6f} {vn[2]:.6f}\n")

            total_faces = 0
            # Sort material names alphabetically so Unity material slot indices are 100% deterministic!
            sorted_mats = sorted(self.mat_faces.keys())
            for mat_name in sorted_mats:
                faces = self.mat_faces[mat_name]
                f.write(f"usemtl {mat_name}\n")
                for face in faces:
                    f_str = " ".join([f"{v}/{vt if vt else ''}/{vn if vn else ''}" for v, vt, vn in face])
                    f.write(f"f {f_str}\n")
                    total_faces += 1
        print(f"Generated Deterministic Multi-Material OBJ: {filepath} ({len(self.verts)} verts, {total_faces} faces across {len(self.mat_faces)} materials)")

# =========================================================================
# GENERATE DETERMINISTIC MULTI-MATERIAL HIGH-DETAIL INDUSTRIAL 3D ASSETS
# =========================================================================

out_dir = r"c:\SurakhshaAR\Assets\AR_Gas_foundation\3d"

# 1. MULTI-GAS DETECTOR HD
# Mat 0: 01_Yellow, Mat 1: 02_Black, Mat 2: 03_Screen
b = MultiMatMeshBuilder()
b.set_material("01_Yellow")
b.add_box(0, 0.09, 0, 0.116, 0.175, 0.055) # Yellow outer casing

b.set_material("02_Black")
b.add_box(-0.048, 0.178, 0, 0.024, 0.022, 0.060) # Top bumpers
b.add_box( 0.048, 0.178, 0, 0.024, 0.022, 0.060)
for y_offset in [0.05, 0.09, 0.13]:
    b.add_box(-0.059, y_offset, 0, 0.008, 0.018, 0.045) # Side grips
    b.add_box( 0.059, y_offset, 0, 0.008, 0.018, 0.045)
b.add_box(0, 0.125, -0.027, 0.084, 0.064, 0.008) # Display bezel
for sx, sy in [(-0.024, 0.058), (0.024, 0.058), (-0.024, 0.026), (0.024, 0.026)]:
    b.add_cylinder(sx, sy, -0.028, 0.014, 0.010, segments=16, axis='Z') # 4 sensor caps
    b.add_cylinder(sx, sy, -0.033, 0.010, 0.004, segments=16, axis='Z')
b.add_box(-0.026, 0.082, -0.028, 0.018, 0.010, 0.008) # 3 buttons
b.add_box( 0.000, 0.082, -0.028, 0.018, 0.010, 0.008)
b.add_box( 0.026, 0.082, -0.028, 0.018, 0.010, 0.008)
b.add_box(0, 0.10, 0.032, 0.035, 0.110, 0.012) # Belt clip
b.add_cylinder(0, 0.145, 0.032, 0.008, 0.042, segments=12, axis='X')

b.set_material("03_Screen")
b.add_box(0, 0.125, -0.032, 0.080, 0.060, 0.002) # Active screen quad
b.save(os.path.join(out_dir, "GasDetector", "multi_gas_detector_hd.obj"))

# 2. GAS CYLINDER HD
# Mat 0: 01_Red, Mat 1: 02_Steel, Mat 2: 03_Brass
b = MultiMatMeshBuilder()
b.set_material("01_Red")
b.add_cylinder(0, 0.50, 0, 0.114, 0.92, segments=32, axis='Y')
b.add_cylinder(0, 0.04, 0, 0.118, 0.08, segments=32, axis='Y')
b.add_torus(0, 0.96, 0, 0.08, 0.034, seg_R=32, seg_r=16)

b.set_material("02_Steel")
b.add_cylinder(0, 1.04, 0, 0.045, 0.06, segments=24, axis='Y')
b.add_torus(0, 1.18, 0, 0.075, 0.012, seg_R=24, seg_r=12)
for i in range(3):
    ang = i * 2.0 * math.pi / 3.0
    cx, cz = 0.070 * math.cos(ang), 0.070 * math.sin(ang)
    b.add_cylinder(cx, 1.11, cz, 0.010, 0.14, segments=12, axis='Y')

b.set_material("03_Brass")
b.add_box(0, 1.07, 0, 0.045, 0.065, 0.045)
b.add_cylinder(-0.035, 1.07, 0, 0.014, 0.040, segments=16, axis='X')
b.add_cylinder(0, 1.13, 0, 0.010, 0.040, segments=12, axis='Y')
b.add_torus(0, 1.15, 0, 0.032, 0.006, seg_R=16, seg_r=8)
b.save(os.path.join(out_dir, "GasCylinder", "gas_cylinder_hd.obj"))

# 3. ISOLATION VALVE HD
# Mat 0: 01_Steel, Mat 1: 02_Brass, Mat 2: 03_Red
b = MultiMatMeshBuilder()
b.set_material("01_Steel")
b.add_cylinder(0, 0, 0.08, 0.042, 0.160, segments=24, axis='Z')
b.add_cylinder(0, 0.04, 0.08, 0.045, 0.080, segments=24, axis='Y')
b.add_cylinder(0, 0, 0.008, 0.060, 0.016, segments=24, axis='Z')
b.add_cylinder(0, 0, 0.152, 0.060, 0.016, segments=24, axis='Z')
b.add_cylinder(0, 0.080, 0.08, 0.050, 0.014, segments=24, axis='Y')
for i in range(6):
    ang = i * 2.0 * math.pi / 6.0
    bx, bz = 0.038 * math.cos(ang), 0.08 + 0.038 * math.sin(ang)
    b.add_cylinder(bx, 0.090, bz, 0.006, 0.012, segments=6, axis='Y')

b.set_material("02_Brass")
b.add_cylinder(0, 0.120, 0.08, 0.010, 0.070, segments=16, axis='Y')
b.add_cylinder(0, 0.095, 0.08, 0.016, 0.018, segments=6, axis='Y')

b.set_material("03_Red")
b.add_torus(0, 0.155, 0.08, 0.068, 0.009, seg_R=24, seg_r=12)
b.add_cylinder(0, 0.155, 0.08, 0.016, 0.016, segments=12, axis='Y')
for i in range(5):
    ang = i * 2.0 * math.pi / 5.0
    sx, sz = 0.034 * math.cos(ang), 0.08 + 0.034 * math.sin(ang)
    b.add_cylinder(sx, 0.155, sz, 0.006, 0.068, segments=8, axis='Y')
b.save(os.path.join(out_dir, "GasPipeline", "isolation_valve_hd.obj"))

# 4. CONFINED SPACE STORAGE VESSEL TANK HD
# Mat 0: 01_Steel, Mat 1: 02_Yellow, Mat 2: 03_Red
b = MultiMatMeshBuilder()
b.set_material("01_Steel")
b.add_cylinder(0, 0.90, 0, 0.475, 1.40, segments=32, axis='Y')
b.add_torus(0, 1.60, 0, 0.35, 0.125, seg_R=32, seg_r=16)
b.add_torus(0, 0.20, 0, 0.35, 0.125, seg_R=32, seg_r=16)
for i in range(4):
    ang = i * 2.0 * math.pi / 4.0 + math.pi/4.0
    lx, lz = 0.45 * math.cos(ang), 0.45 * math.sin(ang)
    b.add_box(lx, 0.40, lz, 0.08, 0.80, 0.08)
    b.add_box(lx, 0.02, lz, 0.16, 0.04, 0.16)
b.add_cylinder(0, 0.75, -0.48, 0.275, 0.12, segments=32, axis='Z')
b.add_cylinder(0, 0.75, -0.54, 0.325, 0.025, segments=32, axis='Z')
for i in range(8):
    ang = i * 2.0 * math.pi / 8.0
    bx, by = 0.30 * math.cos(ang), 0.75 + 0.30 * math.sin(ang)
    b.add_cylinder(bx, by, -0.555, 0.012, 0.020, segments=6, axis='Z')
b.add_cylinder(0, 0.75, -0.56, 0.280, 0.020, segments=32, axis='Z')
b.add_box(0.24, 0.75, -0.52, 0.04, 0.12, 0.08)

b.set_material("02_Yellow")
b.add_box(0, 0.95, -0.478, 0.45, 0.22, 0.005)

b.set_material("03_Red")
b.add_torus(0, 0.75, -0.57, 0.080, 0.010, seg_R=16, seg_r=8)
b.save(os.path.join(out_dir, "ConfinedSpace", "confined_vessel_hd.obj"))

# 5. INDUSTRIAL WORKBENCH HD
# Mat 0: 01_Steel, Mat 1: 02_Wood
b = MultiMatMeshBuilder()
b.set_material("01_Steel")
for lx in [-0.54, 0.54]:
    for lz in [-0.26, 0.26]:
        b.add_box(lx, 0.35, lz, 0.07, 0.70, 0.07)
b.add_box(0, 0.20, -0.26, 1.05, 0.04, 0.04)
b.add_box(0, 0.20,  0.26, 1.05, 0.04, 0.04)
b.add_box(-0.54, 0.20, 0, 0.04, 0.04, 0.48)
b.add_box( 0.54, 0.20, 0, 0.04, 0.04, 0.48)

b.set_material("02_Wood")
b.add_box(0, 0.72, 0, 1.20, 0.06, 0.65)
b.save(os.path.join(out_dir, "PPE", "workbench_hd.obj"))

print("DETERMINISTIC 3D ASSETS REGENERATED SUCCESSFULLY!")
