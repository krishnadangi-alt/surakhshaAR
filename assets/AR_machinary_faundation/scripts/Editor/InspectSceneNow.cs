#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class InspectSceneNow
    {
        static InspectSceneNow()
        {
            EditorApplication.delayCall += RunInspection;
        }

        [MenuItem("SurakshaAR/Inspect Scene Now")]
        public static void RunInspection()
        {
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null)
            {
                Debug.LogError("[InspectSceneNow] MachineryScenario NOT found!");
                return;
            }

            using (StreamWriter sw = new StreamWriter("C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda/scratch/scene_scenario_dump.txt"))
            {
                sw.WriteLine("=== MachineryScenario Children ===");
                for (int i = 0; i < scenario.transform.childCount; i++)
                {
                    Transform child = scenario.transform.GetChild(i);
                    Renderer r = child.GetComponentInChildren<Renderer>(true);
                    string boundsStr = r != null ? $"bounds center={r.bounds.center}, size={r.bounds.size}" : "no renderer";
                    sw.WriteLine($"[{i}] {child.name} | Active: {child.gameObject.activeSelf} | Pos: {child.localPosition} | Rot: {child.localEulerAngles} | Scale: {child.localScale} | {boundsStr}");
                    
                    // Also print immediate sub-children
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform sub = child.GetChild(j);
                        Renderer sr = sub.GetComponentInChildren<Renderer>(true);
                        string sbStr = sr != null ? $"bounds size={sr.bounds.size}" : "no renderer";
                        sw.WriteLine($"    -> [{j}] {sub.name} | Active: {sub.gameObject.activeSelf} | {sbStr}");
                    }
                }
            }
            Debug.Log("[InspectSceneNow] Dump saved to scene_scenario_dump.txt");
        }
    }
}
#endif
