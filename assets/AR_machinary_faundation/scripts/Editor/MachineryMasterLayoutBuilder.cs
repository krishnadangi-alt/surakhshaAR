using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryMasterLayoutBuilder
    {
        [MenuItem("SurakshaAR/Build Master Machinery Layout")]
        public static void BuildMasterLayout()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null)
            {
                Debug.LogError("MachineryScenario GameObject not found!");
                return;
            }

            Debug.Log("=== BUILDING MASTER MACHINERY LAYOUT ===");

            // 1. CONVEYOR ASSEMBLY
            Transform conveyorT = scenario.transform.Find("ConveyorAssembly");
            if (conveyorT != null)
            {
                conveyorT.gameObject.SetActive(true);
                conveyorT.localPosition = new Vector3(-1.50f, 0.0f, 0.0f);
                conveyorT.localEulerAngles = new Vector3(-90.0f, -90.0f, 0.0f);
                conveyorT.localScale = new Vector3(0.012f, 0.012f, 0.012f);
            }

            // 2. FEED HOPPER
            Transform hopperT = scenario.transform.Find("FeedHopper");
            if (hopperT != null)
            {
                hopperT.gameObject.SetActive(true);
                hopperT.localPosition = new Vector3(-1.18f, 2.15f, 1.53f);
                hopperT.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                hopperT.localScale = new Vector3(0.00065f, 0.00065f, 0.00065f);
            }

            // 3. DRIVE MOTOR
            Transform motorT = scenario.transform.Find("DriveMotor");
            if (motorT != null)
            {
                motorT.gameObject.SetActive(true);
                motorT.localPosition = new Vector3(2.35f, 0.45f, 0.0f);
                motorT.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                motorT.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            }

            Transform basePlateT = scenario.transform.Find("MotorBasePlate");
            if (basePlateT != null)
            {
                basePlateT.gameObject.SetActive(true);
                basePlateT.localPosition = new Vector3(2.35f, 0.02f, 0.0f);
                basePlateT.localEulerAngles = Vector3.zero;
                basePlateT.localScale = new Vector3(0.85f, 0.04f, 0.75f);
            }

            // 4. CONTROL PANEL (Small, human-sized compared to conveyor setup)
            Transform ctrlT = scenario.transform.Find("ControlPanel");
            if (ctrlT != null)
            {
                ctrlT.gameObject.SetActive(true);
                ctrlT.localPosition = new Vector3(0.65f, 0.95f, -1.45f);
                ctrlT.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                ctrlT.localScale = new Vector3(0.00060f, 0.00060f, 0.00060f);
            }

            // 5. EMERGENCY STOP
            Transform estopT = scenario.transform.Find("EmergencyStop");
            if (estopT != null)
            {
                estopT.gameObject.SetActive(true);
                estopT.localPosition = new Vector3(1.15f, 0.90f, -1.45f);
                estopT.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                estopT.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
            }

            // 6. MAIN ISOLATOR
            Transform isolatorT = scenario.transform.Find("MainIsolator");
            if (isolatorT != null)
            {
                isolatorT.gameObject.SetActive(true);
                isolatorT.localPosition = new Vector3(1.55f, 0.95f, -1.45f);
                isolatorT.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
                isolatorT.localScale = new Vector3(0.10f, 0.10f, 0.10f);
            }

            // 7. STEEL WORKSTATION TABLE
            Transform tableT = scenario.transform.Find("SteelWorkstationTable");
            if (tableT != null)
            {
                tableT.gameObject.SetActive(true);
                tableT.localPosition = new Vector3(2.45f, 0.0f, -1.45f);
                tableT.localEulerAngles = new Vector3(-90.0f, -90.0f, 0.0f);
                tableT.localScale = new Vector3(0.0075f, 0.0075f, 0.0075f);
            }

            // 8. LOTO EQUIPMENT (HASP & PADLOCK+TAG) ON TABLETOP
            float tableTopY = 0.80f;
            Transform haspT = scenario.transform.Find("Hasp");
            if (haspT != null)
            {
                haspT.gameObject.SetActive(true);
                haspT.localPosition = new Vector3(2.20f, tableTopY + 0.02f, -1.45f);
                haspT.localEulerAngles = new Vector3(90.0f, 0.0f, 45.0f);
                haspT.localScale = new Vector3(0.0030f, 0.0030f, 0.0030f);
            }

            Transform padlockT = scenario.transform.Find("PadlockAndTag");
            if (padlockT != null)
            {
                padlockT.gameObject.SetActive(true);
                padlockT.localPosition = new Vector3(2.55f, tableTopY + 0.02f, -1.45f);
                padlockT.localEulerAngles = new Vector3(90.0f, 0.0f, 25.0f);
                padlockT.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            }

            // 9. COAL / JAM MATERIAL ON BELT SURFACE
            Transform jamAreaT = scenario.transform.Find("JamArea");
            if (jamAreaT != null)
            {
                ArrangeCoalOnBelt(jamAreaT);
            }

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Master Machinery Layout Successfully Built and Saved!");
        }

        private static void ArrangeCoalOnBelt(Transform jamArea)
        {
            jamArea.localPosition = Vector3.zero;
            jamArea.localEulerAngles = Vector3.zero;
            jamArea.localScale = Vector3.one;

            int count = jamArea.childCount;
            if (count == 0) return;

            // Belt surface top Y is ~1.04m to 1.08m
            float beltY = 1.06f;
            float beltZMin = -0.35f;
            float beltZMax = 0.35f;

            System.Random rand = new System.Random(42);

            for (int i = 0; i < count; i++)
            {
                Transform coal = jamArea.GetChild(i);
                float progress = (float)i / (float)count;

                // Zone 1: Feed stream (X: -1.6m to -0.5m) ~ 30% of coal
                // Zone 2: Flow stream (X: -0.5m to 0.3m) ~ 30% of coal
                // Zone 3: Dense jam pile (X: 0.3m to 1.2m) ~ 40% of coal

                float x, y, z;
                float rotX = (float)(rand.NextDouble() * 360.0);
                float rotY = (float)(rand.NextDouble() * 360.0);
                float rotZ = (float)(rand.NextDouble() * 360.0);
                float s = 0.0035f * (float)(0.8 + rand.NextDouble() * 0.5);

                if (progress < 0.3f)
                {
                    // Feed stream
                    float t = progress / 0.3f;
                    x = Mathf.Lerp(-1.6f, -0.5f, t);
                    z = Mathf.Lerp(beltZMin * 0.5f, beltZMax * 0.5f, (float)rand.NextDouble());
                    y = beltY + (float)(rand.NextDouble() * 0.04);
                }
                else if (progress < 0.6f)
                {
                    // Flow stream
                    float t = (progress - 0.3f) / 0.3f;
                    x = Mathf.Lerp(-0.5f, 0.3f, t);
                    z = Mathf.Lerp(beltZMin * 0.7f, beltZMax * 0.7f, (float)rand.NextDouble());
                    y = beltY + (float)(rand.NextDouble() * 0.05);
                }
                else
                {
                    // Dense jam accumulation pile
                    float t = (progress - 0.6f) / 0.4f;
                    x = Mathf.Lerp(0.3f, 1.2f, t);
                    z = Mathf.Lerp(beltZMin * 0.9f, beltZMax * 0.9f, (float)rand.NextDouble());
                    // Pile height builds up in the middle of jam
                    float pileHeight = Mathf.Sin(t * Mathf.PI) * 0.18f;
                    y = beltY + pileHeight + (float)(rand.NextDouble() * 0.06);
                }

                coal.localPosition = new Vector3(x, y, z);
                coal.localEulerAngles = new Vector3(rotX, rotY, rotZ);
                coal.localScale = new Vector3(s, s, s);
            }

            Debug.Log($"Arranged {count} coal pieces along belt surface from X=-1.6 to X=1.2");
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return child;
                Transform found = FindDeepChild(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void DisableHopperWallPanels(GameObject hopper)
        {
            Renderer[] renderers = hopper.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r.bounds.size.magnitude > 5.0f)
                {
                    r.gameObject.SetActive(false);
                    Debug.Log($"Disabled large wall panel renderer '{r.gameObject.name}' in Hopper");
                }
            }
        }
    }
}
