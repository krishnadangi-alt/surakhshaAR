#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class TestUprightOrientation
    {
        static TestUprightOrientation()
        {
            EditorApplication.delayCall += Run;
        }

        [MenuItem("SurakshaAR/Test Upright Orientation")]
        public static void Run()
        {
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null) return;

            Transform conv = scenario.transform.Find("ConveyorAssembly");
            Transform table = scenario.transform.Find("SteelWorkstationTable");

            using (StreamWriter sw = new StreamWriter("C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda/scratch/upright_test.txt"))
            {
                if (conv != null)
                {
                    conv.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                    conv.localPosition = Vector3.zero;

                    foreach (var r in new Vector3[] { new Vector3(90, -90, 0), new Vector3(-90, -90, 0), new Vector3(90, 90, 0), new Vector3(-90, 90, 0) })
                    {
                        conv.localEulerAngles = r;
                        Transform belt = conv.Find("Collada visual scene group/Machine_Conveyor_Belt/Conveyor_belt/Belt");
                        Transform leg = conv.Find("Collada visual scene group/Machine_Conveyor_Belt/Conveyor_belt/Leg_Double_01");
                        float beltY = belt != null ? belt.GetComponentInChildren<Renderer>().bounds.center.y : 0;
                        float legY = leg != null ? leg.GetComponentInChildren<Renderer>().bounds.center.y : 0;
                        sw.WriteLine($"Conveyor Rot {r}: Belt Y={beltY:F2}, Leg Y={legY:F2} -> (Belt higher than leg: {beltY > legY})");
                    }
                }

                if (table != null)
                {
                    table.localScale = new Vector3(0.010f, 0.010f, 0.010f);
                    table.localPosition = Vector3.zero;

                    foreach (var r in new Vector3[] { new Vector3(0, 0, 90), new Vector3(0, 0, -90), new Vector3(90, -90, 0), new Vector3(-90, -90, 0), new Vector3(0, 90, 90), new Vector3(0, -90, -90) })
                    {
                        table.localEulerAngles = r;
                        Bounds b = table.GetComponentInChildren<Renderer>().bounds;
                        sw.WriteLine($"Table Rot {r}: bounds center Y={b.center.y:F2}, min Y={b.min.y:F2}, max Y={b.max.y:F2}, size=({b.size.x:F2}, {b.size.y:F2}, {b.size.z:F2})");
                    }
                }
            }
            Debug.Log("[TestUprightOrientation] Done upright test!");
        }
    }
}
#endif
