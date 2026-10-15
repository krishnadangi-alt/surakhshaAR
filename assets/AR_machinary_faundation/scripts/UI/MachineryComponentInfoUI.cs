using UnityEngine;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryComponentInfoUI
    /// =========================
    /// Manages the component information modal data model and presentation.
    /// Used during Machinery Overview mode to inspect component safety specifications.
    /// </summary>
    public class MachineryComponentInfoUI : MonoBehaviour
    {
        public static MachineryScenarioUIController.ComponentInfoDef[] AllComponents => MachineryScenarioUIController.ComponentDefs;

        public static MachineryScenarioUIController.ComponentInfoDef GetComponentById(string id)
        {
            foreach (var def in AllComponents)
            {
                if (def.id.Equals(id, System.StringComparison.OrdinalIgnoreCase)) return def;
            }
            return default;
        }
    }
}
