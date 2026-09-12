using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class ModuleSelectionController : IScreenController
    {
        private Button _btnBack;

        public void OnShow(GameObject root, object param)
        {
            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null) _btnBack.onClick.AddListener(GoBack);

            var listContainer = UIHelper.FindRect(root, "module-list");
            if (listContainer != null && AppManager.Instance?.Modules != null)
            {
                // Clear existing
                for (int i = listContainer.childCount - 1; i >= 0; i--)
                {
                    Object.Destroy(listContainer.GetChild(i).gameObject);
                }

                foreach (var module in AppManager.Instance.Modules)
                {
                    CreateModuleCard(listContainer, module);
                }
            }
        }

        private void CreateModuleCard(Transform parent, ModuleData module)
        {
            var loc = AppManager.Instance?.Localization;
            string title = loc != null ? loc.Get(module.titleKey) : module.titleKey;
            string desc = loc != null ? loc.Get(module.descriptionKey) : module.descriptionKey;

            var cardGO = new GameObject($"Card_{module.id}");
            cardGO.transform.SetParent(parent, false);
            UIHelper.SetLayout(cardGO, preferredHeight: 120);

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = UIColors.Card;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 18);

            var btn = cardGO.AddComponent<Button>();

            var inner = UIHelper.MakeHorizontal("Inner", cardGO.transform, 16, new RectOffset(16, 16, 14, 14));
            UIHelper.Stretch(inner, 0, 0, 0, 0);

            // Icon
            var iconBox = UIHelper.MakeRect("IconBox", inner);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 64, preferredHeight: 64);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.color = module.id == ModuleId.FireAndExplosion ? UIColors.FireAccentBg : UIColors.GasAccentBg;
            UIHelper.SetImageRoundedSprite(iconImg, 16);

            string badge = module.id == ModuleId.FireAndExplosion ? "FIRE" :
                           (module.id == ModuleId.GasLeakConfinedSpace ? "GAS" : "MECH");
            Color badgeColor = module.id == ModuleId.FireAndExplosion ? UIColors.FireAccent :
                               (module.id == ModuleId.GasLeakConfinedSpace ? UIColors.Primary : UIColors.Success);
            var iconLbl = UIHelper.MakeLabel("Icon", iconBox, badge, 16, badgeColor, TextAlignmentOptions.Center, bold: true, wrap: false);
            UIHelper.Stretch(iconLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Content
            var infoCol = UIHelper.MakeVertical("InfoCol", inner, 4);
            UIHelper.SetLayout(infoCol.gameObject, flexibleWidth: true, flexWidth: 1);

            var titleLbl = UIHelper.MakeLabel("Title", infoCol, title, 20, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 26);

            var descLbl = UIHelper.MakeLabel("Desc", infoCol, desc, 16, UIColors.TextSecondary);
            UIHelper.SetLayout(descLbl.gameObject, preferredHeight: 22);

            if (module.isLocked)
            {
                var lockBadge = UIHelper.MakeLabel("Badge", infoCol, "Coming Soon", 14, UIColors.TextMuted, bold: true);
                UIHelper.SetLayout(lockBadge.gameObject, preferredHeight: 20);
                btn.interactable = false;
            }
            else
            {
                btn.onClick.AddListener(() =>
                {
                    if (AppState.Instance != null) AppState.Instance.SelectedModule = module;
                    UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail, module);
                });
            }
        }

        private void GoBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveListener(GoBack);
        }
    }
}
