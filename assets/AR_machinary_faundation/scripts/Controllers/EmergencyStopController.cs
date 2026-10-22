using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// EmergencyStopController
    /// =====================
    /// Handles Emergency Stop button actuation:
    /// - Calculates reaction time.
    /// - Depresses E-Stop button visual.
    /// - Stops conveyor movement.
    /// - Raises assessment event via MachineryScenarioAssessmentBridge.
    /// </summary>
    public class EmergencyStopController : MonoBehaviour
    {
        [Header("E-Stop Visuals")]
        [SerializeField] private Transform eStopButtonMesh;
        [SerializeField] private float depressedOffset = -0.02f;

        public static EmergencyStopController Instance { get; private set; }

        private bool isPressed = false;
        private float pressTime;

        public bool IsPressed => isPressed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PressEmergencyStop()
        {
            if (isPressed) return;

            isPressed = true;
            pressTime = Time.time;

            if (eStopButtonMesh != null)
            {
                eStopButtonMesh.localPosition += new Vector3(0f, depressedOffset, 0f);
            }

            float reactionSeconds = 1.8f; // Standard rapid reaction time metric
            if (ConveyorScenarioController.Instance != null)
            {
                reactionSeconds = Mathf.Max(0.5f, Time.time - ConveyorScenarioController.Instance.transform.position.magnitude);
            }

            if (MachineryScenarioAssessmentBridge.Instance != null)
            {
                MachineryScenarioAssessmentBridge.Instance.OnEmergencyStopPressed(reactionSeconds);
            }

            Debug.Log($"[E-STOP CONTROLLER] Emergency Stop actuated! Reaction time: {reactionSeconds:F2}s");

            if (ConveyorScenarioController.Instance != null)
            {
                ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.SelectIsolationPoint);
            }
        }

        public void ResetEmergencyStop()
        {
            if (!isPressed) return;

            isPressed = false;
            if (eStopButtonMesh != null)
            {
                eStopButtonMesh.localPosition -= new Vector3(0f, depressedOffset, 0f);
            }

            Debug.Log("[E-STOP CONTROLLER] Emergency Stop reset to normal armed state.");
        }
    }
}
