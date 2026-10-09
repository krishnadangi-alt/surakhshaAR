using UnityEngine;
using UnityEngine.EventSystems;
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
            // Do not raycast into 3D world if user tapped on UI canvas element (e.g. Continue button)
            if (IsPointerOverUI(touchPos))
            {
                return;
            }

            CheckObjectHit(touchPos);
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Touch.activeTouches.Count > 0)
        {
            int touchId = Touch.activeTouches[0].touchId;
            if (EventSystem.current.IsPointerOverGameObject(touchId)) return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId)) return true;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
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

            // Specific handling for Hazard Zone Identification step
            if (stepController.CurrentStep == GasTrainingStepController.TrainingStep.DetectorChecked ||
                stepController.CurrentStep == GasTrainingStepController.TrainingStep.HazardRecognition)
            {
                if (objCategory == "HazardZone" || objCategory == "Cylinder" || objCategory == "LeakPoint" || objCategory == "Regulator")
                {
                    stepController.OnHazardZoneTapped();
                }
                else
                {
                    HandleWrongObjectTap();
                }
                return;
            }

            // Specific handling for Step 4: PPE Selection
            if (stepController.CurrentStep == GasTrainingStepController.TrainingStep.PPECheck)
            {
                if (objCategory.StartsWith("PPE_"))
                {
                    stepController.OnPpeObjectTapped(objCategory, hit.collider.gameObject);
                }
                else
                {
                    HandleWrongObjectTap();
                }
                return;
            }

            if (string.IsNullOrEmpty(objCategory))
            {
                HandleWrongObjectTap();
                return;
            }

            // Action triggers for specific steps
            if (objCategory == "Cylinder" || objCategory == "LeakPoint" || (objCategory == "Regulator" && stepController.CurrentStep == GasTrainingStepController.TrainingStep.CylinderInteraction))
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
        else
        {
            // Empty air / floor tap during Hazard Zone or PPE steps
            if (stepController.CurrentStep == GasTrainingStepController.TrainingStep.DetectorChecked ||
                stepController.CurrentStep == GasTrainingStepController.TrainingStep.HazardRecognition)
            {
                HandleWrongObjectTap();
            }
            else if (stepController.CurrentStep == GasTrainingStepController.TrainingStep.PPECheck)
            {
                HandleWrongObjectTap();
            }
        }
    }

    private void HandleWrongObjectTap()
    {
        if (stepController == null) return;

        var step = stepController.CurrentStep;
        if (step == GasTrainingStepController.TrainingStep.WorkstationReady || step == GasTrainingStepController.TrainingStep.Intro || step == GasTrainingStepController.TrainingStep.DetectorInteraction)
        {
            stepController.OnWrongObjectTapped("Detector");
        }
        else if (step == GasTrainingStepController.TrainingStep.CylinderInteraction)
        {
            stepController.OnWrongObjectTapped("Cylinder");
        }
        else if (step == GasTrainingStepController.TrainingStep.DetectorChecked || step == GasTrainingStepController.TrainingStep.HazardRecognition)
        {
            stepController.OnWrongObjectTapped("HazardZone");
        }
        else if (step == GasTrainingStepController.TrainingStep.PPECheck)
        {
            stepController.OnWrongObjectTapped("PPE");
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
            if (n.Contains("HazardZone") || n.Contains("HazardArea") || n.Contains("HazardDisk") || n.Contains("DangerZone")) return "HazardZone";
            if (n.Contains("Helmet") || n.Contains("Hardhat") || n.Contains("hardhat") || n.Contains("helmet")) return "PPE_Hardhat";
            if (n.Contains("Goggles") || n.Contains("goggles")) return "PPE_Goggles";
            if (n.Contains("Gloves") || n.Contains("gloves") || n.Contains("Glove") || n.Contains("glove")) return "PPE_Gloves";
            if (n.Contains("Boot") || n.Contains("boot")) return "PPE_Boots";
            if (n.Contains("Cylinder") || n.Contains("gas_cylinder") || n.Contains("LeakPoint") || n.Contains("GasHazard") || n.Contains("PreLeak") || n.Contains("MarkerSphere") || n.Contains("LeakLabel")) return "Cylinder";
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
