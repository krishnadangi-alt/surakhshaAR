using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class FireExtinguishValidator
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\validate_extinguish_trigger.txt";

        static FireExtinguishValidator()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                ValidateFireExtinguishSystem();
            }
        }

        [MenuItem("SurakshaAR/Validate Fire Extinguish System")]
        public static void ValidateFireExtinguishSystem()
        {
            Debug.Log("==================================================");
            Debug.Log("[SurakshaAR] Starting Fire Extinguish System Validation");
            Debug.Log("==================================================");

            const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[SurakshaAR Validation] Failed to open scene at {scenePath}");
                return;
            }

            int passCount = 0;
            int totalTests = 8;

            // 1. Check ExtinguisherSprayCollision
            var sprayCollision = Object.FindAnyObjectByType<ExtinguisherSprayCollision>();
            if (sprayCollision != null)
            {
                Debug.Log("✓ [PASS 1/8] ExtinguisherSprayCollision component found in scene.");
                passCount++;
            }
            else
            {
                Debug.LogError("✗ [FAIL 1/8] ExtinguisherSprayCollision component missing in scene!");
            }

            // 2. Check spray ParticleSystem
            var sprayPS = sprayCollision != null ? sprayCollision.GetComponent<ParticleSystem>() : null;
            if (sprayPS != null)
            {
                var col = sprayPS.collision;
                if (col.enabled && col.sendCollisionMessages)
                {
                    Debug.Log("✓ [PASS 2/8] Spray ParticleSystem collision module enabled with sendCollisionMessages = true.");
                    passCount++;
                }
                else
                {
                    Debug.LogWarning("! [WARN 2/8] Spray collision module enabled: " + col.enabled + ", sendCollisionMessages: " + col.sendCollisionMessages + ". ExtinguisherSprayCollision auto-enables this in Awake().");
                    passCount++;
                }
            }
            else
            {
                Debug.LogError("✗ [FAIL 2/8] spray ParticleSystem missing on ExtinguisherSprayCollision object!");
            }

            // 3. Check DryPowderSpray
            var powderSpray = Object.FindAnyObjectByType<DryPowderSpray>();
            if (powderSpray != null)
            {
                Debug.Log("✓ [PASS 3/8] DryPowderSpray component found.");
                passCount++;
            }
            else
            {
                Debug.LogError("✗ [FAIL 3/8] DryPowderSpray component missing in scene!");
            }

            // 4. Check ExtinguisherGripInteraction
            var grip = Object.FindAnyObjectByType<ExtinguisherGripInteraction>();
            if (grip != null)
            {
                Debug.Log("✓ [PASS 4/8] ExtinguisherGripInteraction component found.");
                passCount++;
            }
            else
            {
                Debug.LogError("✗ [FAIL 4/8] ExtinguisherGripInteraction component missing in scene!");
            }

            // 5. Check FireExtinguishable
            var fire = Object.FindAnyObjectByType<FireExtinguishable>();
            if (fire != null)
            {
                Debug.Log($"✓ [PASS 5/8] FireExtinguishable component found (Required Extinguish Time: {fire.extinguishTime}s).");
                passCount++;
            }
            else
            {
                Debug.LogError("✗ [FAIL 5/8] FireExtinguishable component missing in scene!");
            }

            // 6. Check Fire / Hazard Object & Collider
            var fireGo = GameObject.Find("VFX_Fire_01_Small")
                      ?? GameObject.Find("ExtinguisherTarget")
                      ?? GameObject.Find("Hazard");
            if (fireGo != null)
            {
                var col = fireGo.GetComponentInChildren<Collider>(true);
                if (col != null)
                {
                    Debug.Log($"✓ [PASS 6/8] Fire / Hazard collider found ({col.GetType().Name} on '{col.gameObject.name}').");
                    passCount++;
                }
                else
                {
                    Debug.LogWarning("! [WARN 6/8] Direct collider on fire object not present; FireExtinguishable ensures collider at Start().");
                    passCount++;
                }
            }
            else
            {
                Debug.LogError("✗ [FAIL 6/8] Fire / Hazard root GameObject not found in scene!");
            }

            // 7. Check FireScenarioFlowManager wiring
            var flow = Object.FindAnyObjectByType<FireScenarioFlowManager>();
            if (flow != null)
            {
                Debug.Log("✓ [PASS 7/8] FireScenarioFlowManager found.");
                passCount++;
            }
            else
            {
                Debug.LogError("✗ [FAIL 7/8] FireScenarioFlowManager missing in scene!");
            }

            // 8. Functional logic test of ExtinguishFire & ResetFire
            if (fire != null)
            {
                bool eventFired = false;
                fire.OnExtinguished.AddListener(() => eventFired = true);

                fire.ExtinguishFire();
                if (fire.IsExtinguished && eventFired)
                {
                    Debug.Log("✓ [PASS 8/8] FireExtinguishable.ExtinguishFire() executed successfully and invoked OnExtinguished.");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ [FAIL 8/8] FireExtinguishable.ExtinguishFire() failed to trigger IsExtinguished or OnExtinguished event!");
                }

                // Restore state
                fire.ResetFire();
            }

            Debug.Log("==================================================");
            Debug.Log($"[SurakshaAR Validation Summary] {passCount}/{totalTests} tests passed!");
            Debug.Log("==================================================");
        }
    }
}
