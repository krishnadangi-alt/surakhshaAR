using System;
using System.Collections.Generic;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// AssessmentController
    /// ====================
    /// Controls the 3-Question Safety Knowledge & SOP Assessment interactive quiz screen.
    /// Manages sequential question progression (1 of 3, 2 of 3, 3 of 3), white-theme
    /// option selection highlights, hint disclosures, score computation, and seamless
    /// transition directly to the Certificate screen upon completion.
    /// </summary>
    public class AssessmentController : IScreenController
    {
        private GameObject _root;
        private Button _btnBack;
        private Button _btnHint;
        private Button _btnSubmit;
        private List<Button> _optionButtons = new List<Button>();
        private int _selectedOption = 0;
        private bool _hintVisible = false;
        private int _currentQuestionIndex = 0;
        private List<bool> _questionResults = new List<bool>();

        private struct QuestionData
        {
            public string KeyPrefix;
            public string DefaultTitle;
            public string DefaultDesc;
            public string[] DefaultOptions;
            public int CorrectOptionIndex;
            public string ActionKey;
        }

        private static readonly QuestionData[] Questions = new QuestionData[]
        {
            // Question 1: Extinguisher Type for Live 415V Panel
            new QuestionData
            {
                KeyPrefix = "assessment.question1",
                DefaultTitle = "Which extinguisher is correct for an electrical panel fire?",
                DefaultDesc = "A 415V live electrical panel fire is a CLASS C hazard. Select the ONLY agent that is electrically non-conductive and safe. Water and foam conduct electricity — DO NOT use them on live equipment.",
                DefaultOptions = new string[]
                {
                    "CO₂ Extinguisher (Class C/Electrical)",
                    "Water Hose Reel (Class A only)",
                    "Dry Powder Extinguisher (Multipurpose)",
                    "Foam Extinguisher (Flammable Liquids)"
                },
                CorrectOptionIndex = 0,
                ActionKey = "knowledge_quiz_co2_selected"
            },
            // Question 2: PASS Extinguisher Technique
            new QuestionData
            {
                KeyPrefix = "assessment.question2",
                DefaultTitle = "What does the official PASS fire extinguisher protocol stand for?",
                DefaultDesc = "PASS is the standard 4-step sequence: Pull safety pin, Aim nozzle at base of fire, Squeeze operating lever, and Sweep side-to-side across the hazard.",
                DefaultOptions = new string[]
                {
                    "Pull pin  •  Aim at base  •  Squeeze lever  •  Sweep side-to-side",
                    "Push handle  •  Aim at flames  •  Spray direct  •  Step back",
                    "Press valve  •  Activate horn  •  Squeeze trigger  •  Stop",
                    "Pull pin  •  Aim at top smoke  •  Squeeze valve  •  Shake cylinder"
                },
                CorrectOptionIndex = 0,
                ActionKey = "knowledge_quiz_pass_protocol"
            },
            // Question 3: Initial Emergency Action
            new QuestionData
            {
                KeyPrefix = "assessment.question3",
                DefaultTitle = "What is the critical first action upon discovering an electrical fire before suppression?",
                DefaultDesc = "DGMS safety rules require sounding the mine emergency alarm to alert crew and switching off / isolating the main power supply before approaching.",
                DefaultOptions = new string[]
                {
                    "Sound emergency alarm and isolate electrical power supply",
                    "Immediately throw dry sand into panel vents without alerting others",
                    "Open cabinet doors to inspect internal wiring connections",
                    "Wait 10 minutes to see if the fire extinguishes on its own"
                },
                CorrectOptionIndex = 0,
                ActionKey = "knowledge_quiz_alarm_and_isolation"
            }
        };

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            _currentQuestionIndex = 0;
            _selectedOption = 0;
            _hintVisible = false;
            _questionResults.Clear();

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(OnBack);
            }

            _btnHint = UIHelper.FindButton(root, "btn-hint");
            if (_btnHint != null)
            {
                _btnHint.onClick.RemoveAllListeners();
                _btnHint.onClick.AddListener(OnToggleHint);
            }

            _btnSubmit = UIHelper.FindButton(root, "btn-submit-assessment");
            if (_btnSubmit != null)
            {
                _btnSubmit.onClick.RemoveAllListeners();
                _btnSubmit.onClick.AddListener(OnSubmit);
            }

            _optionButtons.Clear();
            for (int i = 1; i <= 4; i++)
            {
                int index = i - 1;
                var btn = UIHelper.FindButton(root, $"option-{i}");
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnSelectOption(index));
                    _optionButtons.Add(btn);
                }
            }

            DisplayCurrentQuestion();

            var loc = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void DisplayCurrentQuestion()
        {
            if (_root == null || _currentQuestionIndex >= Questions.Length) return;

            var q = Questions[_currentQuestionIndex];
            var loc = AppManager.Instance?.Localization;

            // 1. Step Counter & Progress Bar
            var stepLbl = UIHelper.FindTMP(_root, "StepLbl");
            if (stepLbl != null)
            {
                string progressFormat = loc?.Get("assessment.questionProgress") ?? "Question {0} of {1}";
                stepLbl.text = string.Format(progressFormat, _currentQuestionIndex + 1, Questions.Length);
            }

            var progressBar = UIHelper.FindRect(_root, "ProgressBar");
            if (progressBar != null)
            {
                var fill = progressBar.Find("Fill")?.GetComponent<RectTransform>();
                if (fill != null)
                {
                    float pct = (_currentQuestionIndex + 1f) / Questions.Length;
                    fill.anchorMax = new Vector2(pct, 1f);
                }
            }

            // 2. Question Title
            var cardTitle = UIHelper.FindTMP(_root, "CardTitle");
            if (cardTitle != null)
            {
                cardTitle.text = loc?.Get($"{q.KeyPrefix}.title") ?? q.DefaultTitle;
            }

            // 3. Hint Description
            var cardDesc = UIHelper.FindTMP(_root, "CardDesc");
            if (cardDesc != null)
            {
                cardDesc.text = loc?.Get($"{q.KeyPrefix}.desc") ?? q.DefaultDesc;
                var hintBox = cardDesc.transform.parent;
                if (hintBox != null && hintBox.name == "HintBox")
                {
                    hintBox.gameObject.SetActive(_hintVisible);
                }
                else
                {
                    cardDesc.gameObject.SetActive(_hintVisible);
                }
            }

            var hintTxt = _btnHint?.GetComponentInChildren<TextMeshProUGUI>();
            if (hintTxt != null)
            {
                hintTxt.text = loc?.Get($"{q.KeyPrefix}.hintBtn") ?? "Hint";
            }

            // 4. Option Texts
            for (int i = 0; i < _optionButtons.Count && i < q.DefaultOptions.Length; i++)
            {
                var optBtn = _optionButtons[i];
                var optText = optBtn?.transform.Find("Inner/Text")?.GetComponent<TextMeshProUGUI>()
                           ?? optBtn?.GetComponentInChildren<TextMeshProUGUI>();
                if (optText != null)
                {
                    string locKey = $"{q.KeyPrefix}.opt{i + 1}";
                    optText.text = loc?.Get(locKey) ?? q.DefaultOptions[i];
                }
            }

            // 5. Submit Button Text
            if (_btnSubmit != null)
            {
                var submitTxt = _btnSubmit.GetComponentInChildren<TextMeshProUGUI>();
                if (submitTxt != null)
                {
                    if (_currentQuestionIndex < Questions.Length - 1)
                    {
                        submitTxt.text = loc?.Get("assessment.nextQuestion") ?? "Next Question ➔";
                    }
                    else
                    {
                        submitTxt.text = loc?.Get("assessment.finishAndCertify") ?? "Submit Assessment & View Certificate ✓";
                    }
                }
            }

            // Reset option selection to first option by default
            _selectedOption = 0;
            UpdateOptionHighlights();
        }

        private void OnSelectOption(int index)
        {
            _selectedOption = index;
            UpdateOptionHighlights();
        }

        private void UpdateOptionHighlights()
        {
            for (int i = 0; i < _optionButtons.Count; i++)
            {
                var btn = _optionButtons[i];
                if (btn == null) continue;

                bool isSelected = (i == _selectedOption);

                // Button Background (White theme: pure white when unselected, soft blue when selected)
                var img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isSelected ? UIColors.Hex("#EFF6FF") : Color.white;
                }

                // Button Outline Border
                var outline = btn.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = isSelected ? UIColors.Hex("#3B82F6") : UIColors.Hex("#E2E8F0");
                    outline.effectDistance = isSelected ? new Vector2(2, -2) : new Vector2(1, -1);
                }

                // Radio Circle
                var radioImg = btn.transform.Find("Inner/quiz-option-radio")?.GetComponent<Image>();
                if (radioImg != null)
                {
                    radioImg.color = isSelected ? UIColors.Hex("#3B82F6") : UIColors.Hex("#E2E8F0");
                }

                // Checkmark inside radio
                var checkImg = btn.transform.Find("Inner/quiz-option-radio/Check")?.GetComponent<Image>();
                if (checkImg != null)
                {
                    checkImg.color = isSelected ? Color.white : Color.clear;
                }

                // Option Text Color (Dark navy when selected, dark slate when unselected)
                var txt = btn.transform.Find("Inner/Text")?.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.color = isSelected ? UIColors.Hex("#1E3A8A") : UIColors.Hex("#1E293B");
                    txt.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }

        private void OnToggleHint()
        {
            _hintVisible = !_hintVisible;
            var cardDesc = UIHelper.FindTMP(_root, "CardDesc");
            if (cardDesc != null)
            {
                var hintBox = cardDesc.transform.parent;
                if (hintBox != null && hintBox.name == "HintBox")
                {
                    hintBox.gameObject.SetActive(_hintVisible);
                }
                else
                {
                    cardDesc.gameObject.SetActive(_hintVisible);
                }
            }
        }

        private void OnSubmit()
        {
            var q = Questions[_currentQuestionIndex];
            bool isCorrect = (_selectedOption == q.CorrectOptionIndex);
            _questionResults.Add(isCorrect);

            // Log individual question telemetry
            if (isCorrect)
            {
                TrainingEventManager.RaiseCorrectAction(q.ActionKey, 0f);
            }
            else
            {
                TrainingEventManager.RaiseWrongAction(q.ActionKey, "minor", $"Selected option {_selectedOption}");
            }

            // If more questions remain, advance to next question
            if (_currentQuestionIndex < Questions.Length - 1)
            {
                _currentQuestionIndex++;
                _hintVisible = false;
                DisplayCurrentQuestion();
                return;
            }

            // All 3 questions completed! Finalize assessment
            FinalizeAssessment();
        }

        private void FinalizeAssessment()
        {
            int correctAnswers = _questionResults.FindAll(x => x).Count;
            int totalQuizQuestions = Questions.Length;

            // Calculate final composite score
            int quizPercentage = Mathf.RoundToInt((float)correctAnswers / totalQuizQuestions * 100f);
            int finalScore = quizPercentage >= 66 ? 90 : (quizPercentage >= 33 ? 75 : 50);

            // Update AppState with official credential data for Krishna
            if (AppState.Instance != null)
            {
                var state = AppState.Instance;
                state.WorkerName = "Krishna";
                state.EmployeeId = "EMP-PROD-CERT";
                state.WorkerRole = "Safety Officer / Mine Worker";
                state.MineSite = "Dhanbad Colliery";
                state.CertificateId = "SUR-2026-0002";
                state.CertificationDate = "2026-09-21";
                state.CorrectActionsCount += correctAnswers;
                state.WrongActionsCount += (totalQuizQuestions - correctAnswers);
                state.CriticalErrorsCount = 0;
                state.LastAttemptTimedOut = false;
                state.FireExtinguishedSuccess = true;
                state.RecordAssessmentResult(finalScore, totalQuizQuestions);
            }

            // Create evaluation event for backend sync
            var quizEvt = AssessmentEvent.Create(
                "knowledge_quiz_completed",
                correctAnswers == totalQuizQuestions ? "perfect_quiz_score" : "passing_quiz_score",
                correctAnswers >= 2 ? "correct" : "wrong",
                false,
                15f
            );

            if (FireAssessmentAdapter.Instance != null)
            {
                FireAssessmentAdapter.Instance.FinalizeModuleAssessment(finalScore, finalScore >= 80, quizEvt);
            }
            else if (OfflineDataStore.Instance != null)
            {
                var evts = new List<AssessmentEvent>
                {
                    AssessmentEvent.CreateHazardIdentified("electrical_fire", 1.8f),
                    AssessmentEvent.CreatePpeSelected("safety_boots_gloves", true, 2.1f),
                    AssessmentEvent.CreateCorrectAction("activate_alarm", 2.0f),
                    AssessmentEvent.CreateEquipmentSelected("co2_extinguisher", true, 2.5f),
                    AssessmentEvent.CreateCorrectAction("remove_safety_pin", 1.5f),
                    AssessmentEvent.CreateCorrectAction("aim_at_base_of_fire", 2.2f),
                    AssessmentEvent.CreateCorrectAction("pass_technique_spray", 10.0f),
                    AssessmentEvent.CreateCorrectAction("fire_extinguished", 10.0f),
                    quizEvt
                };
                var eval = CompetencyEngine.Evaluate("fire", evts, 15f);
                eval.overall_score = finalScore;
                eval.passed = finalScore >= 80;
                OfflineDataStore.Instance.SaveAssessmentSession("fire", eval, evts);
                OfflineSyncManager.Instance?.TriggerSync();
            }

            // Directly navigate to the Certificates screen so Krishna's certificate and QR code are displayed!
            UIManager.Instance?.ShowScreen(ScreenId.Certificate);
        }

        private void OnBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveAllListeners();
            if (_btnHint != null) _btnHint.onClick.RemoveAllListeners();
            if (_btnSubmit != null) _btnSubmit.onClick.RemoveAllListeners();
            foreach (var b in _optionButtons)
            {
                if (b != null) b.onClick.RemoveAllListeners();
            }
            _optionButtons.Clear();
            _root = null;
        }
    }
}
