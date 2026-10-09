using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TestGLBLoading
{
    [MenuItem("Tools/Test GLB Loading")]
    public static void TestLoad()
    {
        string glbPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb";
        string fbxPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";

        GameObject glbObj = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
        GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

        Object rawGlb = AssetDatabase.LoadMainAssetAtPath(glbPath);
        Object rawFbx = AssetDatabase.LoadMainAssetAtPath(fbxPath);

        string report = $"GLB GameObject: {(glbObj != null ? glbObj.name : "NULL")}\n" +
                       $"GLB Raw Asset Type: {(rawGlb != null ? rawGlb.GetType().FullName : "NULL")}\n" +
                       $"FBX GameObject: {(fbxObj != null ? fbxObj.name : "NULL")}\n" +
                       $"FBX Raw Asset Type: {(rawFbx != null ? rawFbx.GetType().FullName : "NULL")}\n";

        File.WriteAllText("C:/SurakhshaAR/glb_test_result.txt", report);
        Debug.Log(report);
    }
}
