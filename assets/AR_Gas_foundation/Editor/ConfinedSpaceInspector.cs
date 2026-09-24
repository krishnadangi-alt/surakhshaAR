#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ConfinedSpaceInspector
{
    [MenuItem("SurakshaAR/Inspect Confined Space Meshes")]
    public static void InspectConfinedSpace()
    {
        Debug.Log("==================================================");
        Debug.Log("[CONFINED SPACE MESH INSPECTION]");
        Debug.Log("==================================================");

        GameObject vesselObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj");
        if (vesselObj != null)
        {
            Debug.Log($"[Vessel OBJ] Name: {vesselObj.name}");
            InspectChildren(vesselObj.transform, 0);
        }

        GameObject tanksObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb");
        if (tanksObj != null)
        {
            Debug.Log($"[Tanks GLB] Name: {tanksObj.name}");
            InspectChildren(tanksObj.transform, 0);
        }
    }

    private static void InspectChildren(Transform t, int indent)
    {
        string space = new string(' ', indent * 2);
        Renderer r = t.GetComponent<Renderer>();
        MeshFilter mf = t.GetComponent<MeshFilter>();
        string rInfo = r != null ? $"[Renderer: {r.GetType().Name}, Mats: {r.sharedMaterials.Length}]" : "";
        string mInfo = mf != null && mf.sharedMesh != null ? $"[Mesh: {mf.sharedMesh.name}, Verts: {mf.sharedMesh.vertexCount}, Bounds: {mf.sharedMesh.bounds}]" : "";
        Debug.Log($"{space}- {t.name} {rInfo} {mInfo}");

        if (r != null)
        {
            foreach (var m in r.sharedMaterials)
            {
                if (m != null) Debug.Log($"{space}  * Material: {m.name} (Shader: {m.shader.name})");
                else Debug.Log($"{space}  * Material: NULL");
            }
        }

        for (int i = 0; i < t.childCount; i++)
        {
            InspectChildren(t.GetChild(i), indent + 1);
        }
    }
}
#endif
