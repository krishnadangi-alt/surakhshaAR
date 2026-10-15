#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class InspectLiveControlPanel
    {
        static InspectLiveControlPanel()
        {
            EditorApplication.delayCall += Run;
        }

        [MenuItem("SurakshaAR/Inspect Live Control Panel")]
        public static void Run()
        {
            using (StreamWriter sw = new StreamWriter("C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda/scratch/control_panel_deep_dump.txt"))
            {
                sw.WriteLine("=== ALL OBJECTS NAMED ControlPanel ===");
                GameObject[] allGos = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allGos)
                {
                    if (go.name.IndexOf("ControlPanel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sw.WriteLine($"\nFound ControlPanel: {go.name} (activeInHierarchy={go.activeInHierarchy}, scene={go.scene.name}, parent={(go.transform.parent != null ? go.transform.parent.name : "null")})");
                        sw.WriteLine($"  Pos: {go.transform.position}, LocalPos: {go.transform.localPosition}, Scale: {go.transform.localScale}");
                        sw.WriteLine($"  Direct Children Count: {go.transform.childCount}");
                        for (int i = 0; i < go.transform.childCount; i++)
                        {
                            Transform c = go.transform.GetChild(i);
                            sw.WriteLine($"    [{i}] {c.name} (active={c.gameObject.activeSelf}, childCount={c.childCount})");
                            for (int j = 0; j < Mathf.Min(c.childCount, 10); j++)
                            {
                                Transform gc = c.GetChild(j);
                                sw.WriteLine($"        -> [{j}] {gc.name} (active={gc.gameObject.activeSelf})");
                            }
                        }
                    }
                }
            }
            Debug.Log("[InspectLiveControlPanel] Saved control_panel_deep_dump.txt");
        }
    }
}
#endif
