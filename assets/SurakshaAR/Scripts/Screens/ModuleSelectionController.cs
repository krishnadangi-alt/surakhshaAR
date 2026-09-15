using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class ModuleSelectionController : IScreenController
    {
        private Button _btnBack;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;

        public void OnShow(GameObject root, object param)
        {
            var loc         = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : AppLanguage.English;

            // ── Back button ───────────────────────────────────────────────
            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null) _btnBack.onClick.AddListener(GoBack);

            // ── Wire Bottom Navigation ────────────────────────────────────
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) _navHome.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard));
            if (_navLearn        != null) _navLearn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection));
            if (_navProgress     != null) _navProgress.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress));
            if (_navCertificates != null) _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate));

            // ── Localize nav labels ───────────────────────────────────────
            SetNavLabel(root, "label-nav-home",         currentLang, "Home",         "होम",        "ᱚᱲᱟᱜ");
            SetNavLabel(root, "label-nav-learn",        currentLang, "Learn",        "सीखें",      "ᱥᱮᱪᱮᱫ");
            SetNavLabel(root, "label-nav-progress",     currentLang, "Progress",     "प्रगति",     "ᱞᱟᱦᱟᱱᱛᱤ");
            SetNavLabel(root, "label-nav-certificates", currentLang, "Certificates", "प्रमाणपत्र", "ᱥᱟᱹᱠᱷᱤ ᱥᱟᱠᱟᱢ");

            // ── Localize header ───────────────────────────────────────────
            var titleLbl = UIHelper.FindTMP(root, "label-title");
            if (titleLbl != null)
                titleLbl.text = currentLang == AppLanguage.Hindi
                    ? "प्रशिक्षण मॉड्यूल"
                    : "Safety Training Modules";

            var subLbl = UIHelper.FindTMP(root, "label-sub");
            if (subLbl != null)
                subLbl.text = currentLang == AppLanguage.Hindi
                    ? "एक सुरक्षित भविष्य के लिए सीखें"
                    : "Build Skills for a Safer Tomorrow";

            // ── Populate Module Cards ─────────────────────────────────────
            var listContainer = UIHelper.FindRect(root, "module-list");
            if (listContainer == null) return;

            // Clear existing dynamic cards
            for (int i = listContainer.childCount - 1; i >= 0; i--)
                Object.Destroy(listContainer.GetChild(i).gameObject);

            // Get modules from AppManager if available, otherwise fall back to default catalog
            var modules = AppManager.Instance?.Modules
                ?? SurakshaAR.Data.ModuleCatalog.BuildDefaultCatalog();

            bool hasElectrical = false;
            bool hasMineHazard = false;

            if (modules != null)
            {
                foreach (var module in modules)
                {
                    if (module.id == ModuleId.ElectricalSafety) hasElectrical = true;
                    if (module.id == ModuleId.MineHazardEnvironment) hasMineHazard = true;

                    string title = (loc != null && !string.IsNullOrEmpty(loc.Get(module.titleKey)))
                        ? loc.Get(module.titleKey)
                        : null;
                    string desc = (loc != null && !string.IsNullOrEmpty(loc.Get(module.descriptionKey)))
                        ? loc.Get(module.descriptionKey)
                        : null;

                    BuildModuleCard(listContainer, module, title, desc);
                }
            }

            // Fallback: if catalog didn't include the 4th/5th modules, add them
            if (!hasElectrical)
            {
                var elMod = ScriptableObject.CreateInstance<ModuleData>();
                elMod.id = ModuleId.ElectricalSafety;
                elMod.isLocked = true;
                BuildModuleCard(listContainer, elMod, null, null);
            }

            if (!hasMineHazard)
            {
                var mhMod = ScriptableObject.CreateInstance<ModuleData>();
                mhMod.id = ModuleId.MineHazardEnvironment;
                mhMod.isLocked = true;
                BuildModuleCard(listContainer, mhMod, null, null);
            }
        }

        // ── Build a card for a real AppManager module ─────────────────────
        private void BuildModuleCard(Transform parent, ModuleData module, string title, string desc)
        {
            Sprite iconSprite;
            Color  iconBg;
            Color  statusColor;
            string statusLabel;
            Color? iconColor = null;

            switch (module.id)
            {
                case ModuleId.FireAndExplosion:
                    iconSprite  = UIHelper.LoadProjectSprite("icon_fire_ref1.png") ?? UIHelper.GetFireEmojiSprite();
                    iconBg      = UIColors.Hex("#FFEDD5");
                    statusColor = UIColors.Hex("#EA580C");
                    statusLabel = "Available";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = "Fire & Explosion Response";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = "Learn to identify, respond and control fire hazards in mining environments.";
                    break;

                case ModuleId.GasLeakConfinedSpace:
                    iconSprite  = UIHelper.LoadProjectSprite("icon_gas_ref1.png") ?? UIHelper.GetGasEmojiSprite();
                    iconBg      = UIColors.Hex("#E0F2FE");
                    statusColor = UIColors.Hex("#0284C7");
                    statusLabel = "Available";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = "Gas Leak & Confined Space";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = "Stay safe in hazardous gas environments and confined spaces.";
                    break;

                case ModuleId.MachinerySafety:
                    iconSprite  = UIHelper.LoadProjectSprite("icon_gear_ref1.png") ?? UIHelper.GetGearEmojiSprite();
                    iconBg      = UIColors.Hex("#DCFCE7");
                    statusColor = UIColors.Hex("#16A34A");
                    statusLabel = "Available";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = "Machinery Safety";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = "Identify machinery, understand risks and follow safe procedures.";
                    break;

                case ModuleId.ElectricalSafety:
                    iconSprite  = UIHelper.GetBoltSprite();
                    iconBg      = UIColors.Hex("#FEF3C7");
                    iconColor   = UIColors.Hex("#D97706");
                    statusColor = UIColors.Hex("#64748B");
                    statusLabel = "Locked";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = "Electrical Safety (Coming Soon)";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = "Learn electrical safety practices for mining environments.";
                    break;

                case ModuleId.MineHazardEnvironment:
                    iconSprite  = UIHelper.GetHardHatSprite();
                    iconBg      = UIColors.Hex("#FFEDD5");
                    iconColor   = UIColors.Hex("#EA580C");
                    statusColor = UIColors.Hex("#64748B");
                    statusLabel = "Locked";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = "Mine Hazard & Environment";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = "Understand mine hazards and environmental risks for a safer workplace.";
                    break;

                default:
                    iconSprite  = UIHelper.GetFireEmojiSprite();
                    iconBg      = UIColors.Hex("#F8FAFC");
                    statusColor = UIColors.Hex("#059669");
                    statusLabel = "Available";
                    break;
            }

            var cardGO = ModuleSelectionBuilder.CreateModuleCard(
                parent, $"card-{module.id}", iconSprite, iconBg,
                title, desc,
                module.isLocked, statusColor, statusLabel, iconColor);

            // Wire tap on unlocked cards
            if (!module.isLocked)
            {
                var btn = cardGO.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = module;
                    btn.onClick.AddListener(() =>
                    {
                        if (AppState.Instance != null)
                            AppState.Instance.SelectedModule = captured;
                        UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail, captured);
                    });
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private static void SetNavLabel(
            GameObject root, string name,
            AppLanguage lang, string en, string hi, string sat)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp == null) return;
            tmp.text = lang switch
            {
                AppLanguage.Hindi   => hi,
                AppLanguage.Santali => sat,
                _                   => en
            };
        }

        private static void SetText(TextMeshProUGUI tmp, string text)
        {
            if (tmp != null && !string.IsNullOrEmpty(text)) tmp.text = text;
        }

        private void GoBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack         != null) _btnBack.onClick.RemoveListener(GoBack);
            if (_navHome         != null) _navHome.onClick.RemoveAllListeners();
            if (_navLearn        != null) _navLearn.onClick.RemoveAllListeners();
            if (_navProgress     != null) _navProgress.onClick.RemoveAllListeners();
            if (_navCertificates != null) _navCertificates.onClick.RemoveAllListeners();
        }
    }
}
