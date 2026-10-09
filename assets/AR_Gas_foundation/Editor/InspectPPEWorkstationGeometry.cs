using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class InspectPPEWorkstationGeometry
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string ReportPath = @"C:\SurakhshaAR\ppe_inspection_report.txt";

    public static void Run()
    {
        using (var sw = new StreamWriter(ReportPath, false))
        {
            sw.WriteLine("=== PPE WORKSTATION GEOMETRY INSPECTION ===");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sw.WriteLine("Failed to open scene: " + ScenePath);
                return;
            }

            GameObject station = GameObject.Find("PPE_Preparation_Station");
            if (station == null)
            {
                sw.WriteLine("PPE_Preparation_Station NOT FOUND");
                return;
            }

            sw.WriteLine($"PPE_Preparation_Station: worldPos={station.transform.position.ToString("F4")}, localPos={station.transform.localPosition.ToString("F4")}");

            // List all children
            for (int i = 0; i < station.transform.childCount; i++)
            {
                Transform child = station.transform.GetChild(i);
                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
                sw.WriteLine($"\nChild[{i}]: {child.name} (active={child.gameObject.activeSelf})");
                sw.WriteLine($"  LocalPos: {child.localPosition.ToString("F4")}");
                sw.WriteLine($"  LocalRot: {child.localRotation.eulerAngles.ToString("F4")}");
                sw.WriteLine($"  LocalScale: {child.localScale.ToString("F4")}");
                sw.WriteLine($"  WorldPos: {child.position.ToString("F4")}");

                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int r = 1; r < renderers.Length; r++) b.Encapsulate(renderers[r].bounds);
                    sw.WriteLine($"  World Bounds: Center={b.center.ToString("F4")}, Min={b.min.ToString("F4")}, Max={b.max.ToString("F4")}, Size={b.size.ToString("F4")}");
                    
                    // Local Y bounds relative to station
                    float localMinY = b.min.y - station.transform.position.y;
                    float localMaxY = b.max.y - station.transform.position.y;
                    sw.WriteLine($"  Station-Relative Y: Min={localMinY:F4}, Max={localMaxY:F4}");

                    foreach (var rend in renderers)
                    {
                        sw.WriteLine($"    Renderer on '{rend.name}':");
                        foreach (var m in rend.sharedMaterials)
                        {
                            sw.WriteLine($"      Material: {(m != null ? m.name : "null")}");
                        }
                    }
                }
            }

            // Also inspect ppe_vest asset
            GameObject vestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/ppe_vest.prefab");
            sw.WriteLine($"\nppe_vest.prefab found: {vestPrefab != null}");
            if (vestPrefab != null)
            {
                Renderer[] vestRends = vestPrefab.GetComponentsInChildren<Renderer>(true);
                if (vestRends.Length > 0)
                {
                    Bounds vb = vestRends[0].bounds;
                    for (int r = 1; r < vestRends.Length; r++) vb.Encapsulate(vestRends[r].bounds);
                    sw.WriteLine($"  Vest prefab bounds: size={vb.size.ToString("F4")}");
                }
            }
        }
    }
}
