using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class AssessmentController : IScreenController
    {
        private Button _btnBack, _btnSubmit;
        private Button[] _options;
        private int _selectedIndex = 0;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;

            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null && loc != null) labelTitle.text = loc.Get("assessment.title");

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null) _btnBack.onClick.AddListener(GoBack);

            _btnSubmit = UIHelper.FindButton(root, "btn-submit-assessment");
            if (_btnSubmit != null)
            {
                if (loc != null)
                {
                    var tmp = _btnSubmit.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = loc.Get("assessment.submit");
                }
                _btnSubmit.onClick.AddListener(SubmitAssessment);
            }

            _options = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                _options[i] = UIHelper.FindButton(root, $"option-{i + 1}");
                if (_options[i] != null)
                {
                    _options[i].onClick.AddListener(() => SelectOption(index));
                }
            }

            SelectOption(0);
        }

        private void SelectOption(int index)
        {
            _selectedIndex = index;
            for (int i = 0; i < 4; i++)
            {
                if (_options[i] == null) continue;
                var optImg = _options[i].GetComponent<Image>();
                var radio = _options[i].transform.Find("Inner/quiz-option-radio");
                var check = radio != null ? radio.GetComponentInChildren<TextMeshProUGUI>() : null;
                var radioImg = radio != null ? radio.GetComponent<Image>() : null;

                bool isSelected = (i == index);
                if (optImg != null) optImg.color = isSelected ? UIColors.Hex("#1B3855") : UIColors.Hex("#102233");
                if (check != null) check.text = isSelected ? "✓" : "";
                if (radioImg != null) radioImg.color = isSelected ? UIColors.Primary : new Color(1, 1, 1, 0.25f);
            }
        }

        private void SubmitAssessment()
        {
            var telemetry = AssessmentTelemetryManager.Instance;
            if (telemetry != null)
            {
                telemetry.StartSession("fire");

                // Hazard & PPE evaluation
                telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("hazard_identified")
                {
                    hazard_type = "electrical_panel",
                    correct = true,
                    action = "identify_electrical_hazard"
                });

                telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("ppe_selected")
                {
                    ppe_type = "dielectric_gloves_and_helmet",
                    correct = true,
                    action = "equip_safety_gear"
                });

                // Option evaluation
                if (_selectedIndex == 0) // CO2 Extinguisher (Class C/Electrical) - Correct!
                {
                    telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("equipment_selected")
                    {
                        equipment_type = "co2_extinguisher",
                        correct = true,
                        action = "select_co2_extinguisher"
                    });
                    telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("evacuation_started")
                    {
                        route = "primary_exit_north",
                        safe = true,
                        action = "safe_evacuation"
                    });
                }
                else if (_selectedIndex == 1) // Water Hose Reel - CRITICAL SAFETY VIOLATION!
                {
                    telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("equipment_selected")
                    {
                        equipment_type = "water_hose_reel",
                        correct = false,
                        action = "select_water_extinguisher"
                    });
                    telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("critical_action")
                    {
                        action = "water_on_electrical_panel",
                        reason = "Electrocution hazard: Conductive water stream on live 415V electrical panel.",
                        correct = false
                    });
                }
                else // Dry powder / Foam
                {
                    telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("equipment_selected")
                    {
                        equipment_type = "powder_foam",
                        correct = false,
                        action = "select_secondary_agent"
                    });
                    telemetry.LogEvent(new SurakshaAR.Data.AssessmentEvent("wrong_action")
                    {
                        action = "suboptimal_extinguisher_agent",
                        severity = "minor",
                        reason = "Agent creates corrosive residue on electrical contacts.",
                        correct = false
                    });
                }

                var result = telemetry.CompleteSession();
                if (AppState.Instance != null)
                {
                    AppState.Instance.AssessmentScore = Mathf.RoundToInt(result.overall_score);
                    AppState.Instance.CriticalErrorsCount = result.critical_errors.Count;
                    AppState.Instance.CorrectActionsCount = result.passed ? 4 : (_selectedIndex == 1 ? 1 : 2);
                    AppState.Instance.RecordAssessmentResult(Mathf.RoundToInt(result.overall_score), 4);
                }
            }
            else
            {
                int score = (_selectedIndex == 0) ? 95 : (_selectedIndex == 1 ? 40 : 70);
                if (AppState.Instance != null)
                {
                    AppState.Instance.AssessmentScore = score;
                    AppState.Instance.CriticalErrorsCount = (_selectedIndex == 1) ? 1 : 0;
                    AppState.Instance.RecordAssessmentResult(score, 4);
                }
            }

            UIManager.Instance?.ShowScreen(ScreenId.Result);
        }

        private void GoBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveListener(GoBack);
            if (_btnSubmit != null) _btnSubmit.onClick.RemoveListener(SubmitAssessment);
        }
    }
}
