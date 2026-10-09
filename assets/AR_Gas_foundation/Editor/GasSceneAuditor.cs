#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasSceneAuditor
{
    [MenuItem("SurakshaAR/Audit Gas Scene Hierarchy")]
    public static void AuditScene()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Debug.Log("==================================================");
        Debug.Log("[SCENE AUDIT START] " + scenePath);
        Debug.Log("==================================================");

        GameObject[] rootObjs = scene.GetRootGameObjects();
        foreach (var root in rootObjs)
        {
            DumpHierarchy(root.transform, 0);
        }

        Debug.Log("==================================================");
        Debug.Log("[SCENE AUDIT END]");
        Debug.Log("==================================================");
    }

    private static void DumpHierarchy(Transform trans, int indent)
    {
        string prefix = new string(' ', indent * 2);
        MeshFilter mf = trans.GetComponent<MeshFilter>();
        MeshRenderer mr = trans.GetComponent<MeshRenderer>();

        string meshInfo = (mf != null && mf.sharedMesh != null) ? $" [Mesh: {mf.sharedMesh.name}]" : "";
        string matInfo = "";
        if (mr != null && mr.sharedMaterials != null)
        {
            matInfo = " [Mats: ";
            foreach (var m in mr.sharedMaterials)
            {
                matInfo += (m != null ? m.name : "null") + ", ";
            }
            matInfo += "]";
        }

        Object prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject);
        string prefabInfo = (prefabParent != null) ? $" [SourceAsset: {AssetDatabase.GetAssetPath(prefabParent)}]" : "";

        Debug.Log($"{prefix}- {trans.name} (Active: {trans.gameObject.activeSelf}){meshInfo}{matInfo}{prefabInfo}");

        for (int i = 0; i < trans.childCount; i++)
        {
            DumpHierarchy(trans.GetChild(i), indent + 1);
        }
    }
}
#endif
