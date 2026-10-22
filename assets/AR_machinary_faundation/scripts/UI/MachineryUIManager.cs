using UnityEngine;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryUIManager
    /// ==================
    /// Central manager and API access point for Machinery AR Training UI.
    /// Exposes high-level state control and coordinates with MachineryScenarioUIController.
    /// </summary>
    public class MachineryUIManager : MonoBehaviour
    {
        public static MachineryUIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public MachineryScenarioUIController Controller => MachineryScenarioUIController.Instance;

        public void ShowMainMenu() => Controller?.SetUIState(MachineryScenarioUIController.MachineryUIState.MainMenu);
        public void StartARScanning() => Controller?.SetUIState(MachineryScenarioUIController.MachineryUIState.ARScanning);
        public void ShowComponentOverview() => Controller?.SetUIState(MachineryScenarioUIController.MachineryUIState.ComponentOverview);
        public void StartTrainingDrill()
        {
            if (Controller != null)
            {
                Controller.SetUIState(MachineryScenarioUIController.MachineryUIState.TrainingStep);
            }
        }
        public void ShowTrainingComplete() => Controller?.SetUIState(MachineryScenarioUIController.MachineryUIState.TrainingComplete);
    }
}
