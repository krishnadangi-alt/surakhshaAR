using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class GasARInteraction : MonoBehaviour
{
    [SerializeField] private GasTrainingStepController stepController;

    private Camera arCamera;

    private void Awake()
    {
        if (stepController == null)
        {
            stepController = GetComponent<GasTrainingStepController>();
        }
        if (stepController == null)
        {
            stepController = FindAnyObjectByType<GasTrainingStepController>();
        }
    }

    private void OnEnable()
    {
        try { EnhancedTouchSupport.Enable(); } catch { }
    }

    private void OnDisable()
    {
        try { EnhancedTouchSupport.Disable(); } catch { }
    }

    private void Update()
    {
        if (stepController == null) return;

        Vector2 touchPos = Vector2.zero;
        bool hasTouch = false;

        // 1. Enhanced Touch Input System
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchPos = touch.screenPosition;
                hasTouch = true;
            }
        }

        // 2. Touchscreen Input Fallback (New Input System)
        if (!hasTouch && Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                touchPos = primary.position.ReadValue();
                hasTouch = true;
            }
        }

        // 3. Mouse Click Fallback (New Input System for Editor / PC)
        if (!hasTouch && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchPos = Mouse.current.position.ReadValue();
            hasTouch = true;
        }

        if (hasTouch)
        {
            CheckObjectHit(touchPos);
        }
    }

    private void CheckObjectHit(Vector2 screenPosition)
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
            if (arCamera == null) arCamera = FindAnyObjectByType<Camera>();
        }

        if (arCamera == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            string objCategory = GetHitObjectCategory(hit.transform);
            Debug.Log($"[GasAR Interaction] Hit object: {hit.transform.name} -> Category: {objCategory}");

            if (string.IsNullOrEmpty(objCategory))
            {
                HandleWrongObjectTap();
                return;
            }

            // Action triggers for specific steps
            if (objCategory == "Cylinder" || objCategory == "LeakPoint")
            {
                stepController.OnCylinderOrLeakPointTapped();
            }
            else if (objCategory == "Detector")
            {
                stepController.OnDetectorTapped();
            }
            else if (objCategory == "PPE")
            {
                stepController.SelectPpe("Approved_Gas_PPE", true);
            }
            else if (objCategory == "HazardZone")
            {
                stepController.OnHazardZoneAcknowledged();
            }
            else if (objCategory == "Buddy")
            {
                stepController.OnBuddyConfirmed();
            }
            else if (objCategory == "Valve")
            {
                stepController.OnPipelineValveTapped();
            }
            else if (objCategory == "Vessel")
            {
                stepController.OnConfinedSpaceTapped();
            }
            else
            {
                HandleWrongObjectTap();
            }
        }
    }

    private void HandleWrongObjectTap()
    {
        if (stepController == null) return;

        var step = stepController.CurrentStep;
        if (step == GasTrainingStepController.TrainingStep.WorkstationReady || step == GasTrainingStepController.TrainingStep.CylinderInteraction)
        {
            stepController.OnWrongObjectTapped("Cylinder");
        }
        else if (step == GasTrainingStepController.TrainingStep.DetectorInteraction)
        {
            stepController.OnWrongObjectTapped("Detector");
        }
        else if (step == GasTrainingStepController.TrainingStep.PPECheck)
        {
            stepController.OnWrongObjectTapped("PPE");
        }
        else if (step == GasTrainingStepController.TrainingStep.HazardRecognition)
        {
            stepController.OnWrongObjectTapped("Hazard");
        }
        else if (step == GasTrainingStepController.TrainingStep.BuddySystemCheck)
        {
            stepController.OnWrongObjectTapped("Buddy");
        }
        else if (step == GasTrainingStepController.TrainingStep.SafetyResponse)
        {
            stepController.OnWrongObjectTapped("Valve");
        }
    }

    public bool SimulateInteractionTest(string objCategory)
    {
        if (stepController == null)
        {
            stepController = GetComponent<GasTrainingStepController>() ?? FindAnyObjectByType<GasTrainingStepController>();
        }
        if (stepController == null) return false;

        var step = stepController.CurrentStep;

        if (objCategory == "Cylinder" || objCategory == "LeakPoint")
        {
            if (step == GasTrainingStepController.TrainingStep.WorkstationReady || step == GasTrainingStepController.TrainingStep.CylinderInteraction)
            {
                stepController.OnCylinderOrLeakPointTapped();
                return true;
            }
        }
        else if (objCategory == "Detector")
        {
            if (step == GasTrainingStepController.TrainingStep.DetectorInteraction)
            {
                stepController.OnDetectorTapped();
                return true;
            }
        }
        else if (objCategory == "PPE")
        {
            if (step == GasTrainingStepController.TrainingStep.PPECheck)
            {
                stepController.SelectPpe("Approved_Gas_PPE", true);
                return true;
            }
        }
        else if (objCategory == "HazardZone")
        {
            if (step == GasTrainingStepController.TrainingStep.HazardRecognition)
            {
                stepController.OnHazardZoneAcknowledged();
                return true;
            }
        }
        else if (objCategory == "Buddy")
        {
            if (step == GasTrainingStepController.TrainingStep.BuddySystemCheck)
            {
                stepController.OnBuddyConfirmed();
                return true;
            }
        }
        else if (objCategory == "Valve")
        {
            if (step == GasTrainingStepController.TrainingStep.SafetyResponse)
            {
                stepController.OnPipelineValveTapped();
                return true;
            }
        }
        else if (objCategory == "Vessel")
        {
            if (step == GasTrainingStepController.TrainingStep.ConfinedSpaceIntro || step == GasTrainingStepController.TrainingStep.ConfinedSpaceDrill)
            {
                stepController.OnConfinedSpaceTapped();
                return true;
            }
        }

        HandleWrongObjectTap();
        return false;
    }

    private string GetHitObjectCategory(Transform t)
    {
        while (t != null)
        {
            string n = t.name;
            if (n.Contains("Cylinder") || n.Contains("gas_cylinder") || n.Contains("LeakPoint")) return "Cylinder";
            if (n.Contains("Detector") || n.Contains("detector") || n.Contains("Multi_Gas")) return "Detector";
            if (n.Contains("Regulator") || n.Contains("regulator")) return "Regulator";
            if (n.Contains("Gauge") || n.Contains("gauge")) return "Gauge";
            if (n.Contains("Valve") || n.Contains("valve") || n.Contains("Pipeline")) return "Valve";
            if (n.Contains("Vessel") || n.Contains("Tank") || n.Contains("Confined") || n.Contains("Manhole") || n.Contains("Fan") || n.Contains("Lifeline")) return "Vessel";
            if (n.Contains("Hazard") || n.Contains("Disk") || n.Contains("Perimeter")) return "HazardZone";
            if (n.Contains("Workbench") || n.Contains("Hardhat") || n.Contains("Vest") || n.Contains("Goggles") || n.Contains("Gloves") || n.Contains("Boots") || n.Contains("PPE")) return "PPE";
            if (n.Contains("Buddy") || n.Contains("Standby") || n.Contains("Worker")) return "Buddy";
            t = t.parent;
        }
        return "";
    }
}
