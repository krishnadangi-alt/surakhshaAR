#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class InspectExactModels
    {
        static InspectExactModels()
        {
            EditorApplication.delayCall += Run;
        }

        [MenuItem("SurakshaAR/Inspect Exact Models")]
        public static void Run()
        {
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null) return;

            using (StreamWriter sw = new StreamWriter("C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda/scratch/exact_models_report.txt"))
            {
                sw.WriteLine("=== EXACT SCENARIO OBJECTS AND ASSETS ===");
                for (int i = 0; i < scenario.transform.childCount; i++)
                {
                    Transform t = scenario.transform.GetChild(i);
                    sw.WriteLine($"\n--- [{i}] {t.name} (Active: {t.gameObject.activeSelf}, Pos: {t.localPosition}, Scale: {t.localScale}) ---");
                    MeshFilter[] mfs = t.GetComponentsInChildren<MeshFilter>(true);
                    sw.WriteLine($"  Total MeshFilters: {mfs.Length}");
                    for (int m = 0; m < Mathf.Min(mfs.Length, 15); m++)
                    {
                        MeshFilter mf = mfs[m];
                        string assetPath = "none";
                        if (mf.sharedMesh != null)
                        {
                            assetPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
                        }
                        sw.WriteLine($"    [{m}] {mf.gameObject.name} (active: {mf.gameObject.activeInHierarchy}) -> mesh: {mf.sharedMesh?.name}, asset: {assetPath}");
                    }
                }
            }
            Debug.Log("[InspectExactModels] Saved exact_models_report.txt");
        }
    }
}
#endif
