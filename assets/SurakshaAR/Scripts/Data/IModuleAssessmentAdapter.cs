using System;
using System.Collections.Generic;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Generic contract for vocational training module assessment adapters.
    /// Allows Fire, Gas, and Machinery scenarios to feed the unified CompetencyEngine,
    /// OfflineDataStore, and backend synchronization pipeline without scenario-specific coupling.
    /// </summary>
    public interface IModuleAssessmentAdapter
    {
        string ModuleId { get; }
        string ScenarioType { get; }
        string ActiveScenarioId { get; }

        Dictionary<string, CompetencyEngine.CompetencyDef> SkillTaxonomy { get; }

        void StartScenario(string scenarioId);
        void CompleteScenario(float finalScore, bool passed);
        void AbortScenario(string reason);

        void RecordHazardIdentified(string hazardType, bool correct, float responseTime = 0f);
        void RecordPpeSelected(string ppeType, bool correct, float responseTime = 0f);
        void RecordEquipmentSelected(string equipmentType, bool correct, float responseTime = 0f);
        void RecordCorrectAction(string action, float responseTime = 0f);
        void RecordWrongAction(string action, string reason, string severity = "minor");
        void RecordUnsafeAction(string action, string reason);
        void RecordCriticalAction(string action, string reason);
        void RecordSequenceError(string expectedAction, string actualAction);
        void RecordEvacuation(bool safe, string route = "");
    }
}
