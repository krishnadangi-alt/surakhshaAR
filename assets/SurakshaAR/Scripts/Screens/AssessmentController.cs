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
    /// Controls the Safety Knowledge & SOP Assessment interactive quiz screen.
    /// Manages option selection, answer validation, hint disclosures,
    /// and transition to ResultScreen with competency score propagation.
    /// </summary>
    public class AssessmentController : IScreenController
    {
        private GameObject _root;
        private Button _btnBack;
        private Button _btnHint;
        private Button _btnSubmit;
        private List<Button> _optionButtons = new List<Button>();
        private int _selectedOption = 0; // 0 = CO2 (correct)
        private bool _hintVisible = false;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
                _btnBack.onClick.AddListener(OnBack);

            _btnHint = UIHelper.FindButton(root, "btn-hint");
            if (_btnHint != null)
                _btnHint.onClick.AddListener(OnToggleHint);

            _btnSubmit = UIHelper.FindButton(root, "btn-submit-assessment");
            if (_btnSubmit != null)
                _btnSubmit.onClick.AddListener(OnSubmit);

            _optionButtons.Clear();
            for (int i = 1; i <= 4; i++)
            {
                int index = i - 1;
                var btn = UIHelper.FindButton(root, $"option-{i}");
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnSelectOption(index));
                    _optionButtons.Add(btn);
                }
            }

            // Set initial selection
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
                var img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isSelected ? UIColors.Hex("#1B3855") : UIColors.Hex("#102233");
                }

                var radioImg = btn.transform.Find("Inner/quiz-option-radio")?.GetComponent<Image>();
                if (radioImg != null)
                {
                    radioImg.color = isSelected ? UIColors.Primary : new Color(1, 1, 1, 0.25f);
                }

                var checkImg = btn.transform.Find("Inner/quiz-option-radio/Check")?.GetComponent<Image>();
                if (checkImg != null)
                {
                    checkImg.color = isSelected ? Color.white : Color.clear;
                }
            }
        }

        private void OnToggleHint()
        {
            _hintVisible = !_hintVisible;
            var desc = UIHelper.FindTMP(_root, "CardDesc");
            if (desc != null)
            {
                desc.gameObject.SetActive(_hintVisible);
            }
        }

        private void OnSubmit()
        {
            bool isCorrect = (_selectedOption == 0); // CO2 is correct
            int score = isCorrect ? 90 : 50;

            if (AppState.Instance != null)
            {
                AppState.Instance.RecordAssessmentResult(score, 4);
                AppState.Instance.CorrectActionsCount = isCorrect ? 4 : 2;
                AppState.Instance.WrongActionsCount = isCorrect ? 0 : 2;
                AppState.Instance.CriticalErrorsCount = 0;
            }

            // Route to Result Screen
            UIManager.Instance?.ShowScreen(ScreenId.Result);
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
