using UnityEngine;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryStepController
    /// =======================
    /// Controls the 7-step conveyor jam procedure sequence data and step state.
    /// Exposes step count, titles, safety notes, and verification statuses.
    /// </summary>
    public class MachineryStepController : MonoBehaviour
    {
        public static MachineryScenarioUIController.GuidedStepDef[] AllSteps => MachineryScenarioUIController.GuidedSteps;

        public static int TotalSteps => AllSteps.Length;

        public static MachineryScenarioUIController.GuidedStepDef GetStep(int stepIndex)
        {
            if (stepIndex >= 0 && stepIndex < AllSteps.Length)
            {
                return AllSteps[stepIndex];
            }
            return default;
        }
    }
}
