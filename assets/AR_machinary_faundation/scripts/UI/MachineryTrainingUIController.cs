using UnityEngine;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryTrainingUIController
    /// =============================
    /// Coordinates guided 7-step machinery training drill UI state transitions.
    /// Works with MachineryScenarioUIController to present step-by-step guidance.
    /// </summary>
    public class MachineryTrainingUIController : MonoBehaviour
    {
        public static MachineryTrainingUIController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void AdvanceToStep(int stepIndex)
        {
            if (MachineryScenarioUIController.Instance != null)
            {
                MachineryScenarioUIController.Instance.SetUIState(MachineryScenarioUIController.MachineryUIState.TrainingStep);
            }
        }
    }
}
