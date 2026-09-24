using UnityEngine;
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
            stepController = FindFirstObjectByType<GasTrainingStepController>();
        }
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

        // 2. Legacy Touch Input Fallback
        if (!hasTouch && Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchPos = touch.position;
                hasTouch = true;
            }
        }

        // 3. Mouse Click Fallback
        if (!hasTouch && Input.GetMouseButtonDown(0))
        {
            touchPos = Input.mousePosition;
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
            if (arCamera == null) arCamera = FindFirstObjectByType<Camera>();
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
        else if (step == GasTrainingStepController.TrainingStep.SafetyResponse)
        {
            stepController.OnWrongObjectTapped("Valve");
        }
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
            if (n.Contains("Vessel") || n.Contains("Tank") || n.Contains("Confined")) return "Vessel";
            t = t.parent;
        }
        return "";
    }
}
