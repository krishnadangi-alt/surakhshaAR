#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class TestValveOrientation
{
    [MenuItem("SurakhshaAR/Test Valve Orientation")]
    public static void Test()
    {
        StringBuilder sb = new StringBuilder();
        string path = "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_valve_converted.fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        Vector3[] rots = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 270, 0),
            new Vector3(90, 0, 0),
            new Vector3(90, 90, 0),
            new Vector3(0, 0, 90),
            new Vector3(0, 90, 90)
        };

        float sValve = 0.00160f;

        foreach (var r in rots)
        {
            inst.transform.position = Vector3.zero;
            inst.transform.rotation = Quaternion.Euler(r);
            inst.transform.localScale = new Vector3(sValve, sValve, sValve);

            Renderer[] rends = inst.GetComponentsInChildren<Renderer>(true);
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

            sb.AppendLine($"Euler {r}: World Bounds Min={b.min.ToString("F3")}, Max={b.max.ToString("F3")}, Size={b.size.ToString("F3")}, Center={b.center.ToString("F3")}");
        }

        Object.DestroyImmediate(inst);
        File.WriteAllText(@"C:\SurakhshaAR\ValveOrientationTest.txt", sb.ToString());
    }
}
#endif
