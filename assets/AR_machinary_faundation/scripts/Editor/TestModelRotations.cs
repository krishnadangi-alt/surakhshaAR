#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class TestModelRotations
    {
        static TestModelRotations()
        {
            EditorApplication.delayCall += Run;
        }

        [MenuItem("SurakshaAR/Test Model Rotations")]
        public static void Run()
        {
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null) return;

            Transform conv = scenario.transform.Find("ConveyorAssembly");
            Transform table = scenario.transform.Find("SteelWorkstationTable");

            using (StreamWriter sw = new StreamWriter("C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda/scratch/rotations_test.txt"))
            {
                sw.WriteLine("=== TESTING CONVEYOR ROTATIONS ===");
                Vector3[] testRots = new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 90),
                    new Vector3(0, 0, -90),
                    new Vector3(90, 0, 0),
                    new Vector3(-90, 0, 0),
                    new Vector3(0, 90, 90),
                    new Vector3(0, -90, -90),
                    new Vector3(90, -90, 0),
                    new Vector3(-90, -90, 0),
                    new Vector3(0, 90, 0),
                    new Vector3(0, -90, 0)
                };

                if (conv != null)
                {
                    Vector3 origRot = conv.localEulerAngles;
                    Vector3 origScale = conv.localScale;
                    conv.localScale = new Vector3(0.012f, 0.012f, 0.012f);

                    foreach (var r in testRots)
                    {
                        conv.localEulerAngles = r;
                        Bounds b = GetBounds(conv.gameObject);
                        sw.WriteLine($"Conveyor Rot {r} -> size: ({b.size.x:F2}, {b.size.y:F2}, {b.size.z:F2})");
                    }
                    conv.localEulerAngles = origRot;
                    conv.localScale = origScale;
                }

                sw.WriteLine("\n=== TESTING TABLE ROTATIONS ===");
                if (table != null)
                {
                    Vector3 origRot = table.localEulerAngles;
                    Vector3 origScale = table.localScale;
                    table.localScale = new Vector3(0.007f, 0.007f, 0.007f);

                    foreach (var r in testRots)
                    {
                        table.localEulerAngles = r;
                        Bounds b = GetBounds(table.gameObject);
                        sw.WriteLine($"Table Rot {r} -> size: ({b.size.x:F2}, {b.size.y:F2}, {b.size.z:F2})");
                    }
                    table.localEulerAngles = origRot;
                    table.localScale = origScale;
                }
            }
            Debug.Log("[TestModelRotations] Done test, saved to rotations_test.txt");
        }

        private static Bounds GetBounds(GameObject go)
        {
            Renderer[] rends = go.GetComponentsInChildren<Renderer>(false);
            if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }
    }
}
#endif
