using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
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
        private SurakshaAR.Localization.LocalizationManager _loc;  // stored for BuildModuleCard access

        public void OnShow(GameObject root, object param)
        {
            _loc        = AppManager.Instance?.Localization;
            var loc     = _loc;  // local alias for readability in OnShow
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

            // ── Localize nav labels ─────────────────────────────────────────
            // Nav labels route through LocalizationManager which has verified Ol Chiki values
            SetLabel(root, "label-nav-home",         loc?.Get("home.navHome")         ?? "Home");
            SetLabel(root, "label-nav-learn",        loc?.Get("home.navLearn")        ?? "Learn");
            SetLabel(root, "label-nav-progress",     loc?.Get("home.navProgress")     ?? "My Progress");
            SetLabel(root, "label-nav-certificates", loc?.Get("home.navCertificates") ?? "Certificates");

            // ── Localize header ───────────────────────────────────────────
            SetLabel(root, "label-title", loc?.Get("moduleSelection.title") ?? "Safety Training Modules");
            SetLabel(root, "label-sub",   loc?.Get("moduleSelection.subtitle") ?? "Build Skills for a Safer Tomorrow");

            // ── Populate Module Cards ─────────────────────────────────────
            var listContainer = UIHelper.FindRect(root, "module-list");
            if (listContainer == null) return;

            // Clear existing dynamic cards
            for (int i = listContainer.childCount - 1; i >= 0; i--)
                UIHelper.SafeDestroy(listContainer.GetChild(i).gameObject);

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

            // Ensure all dynamic module cards receive correct font for active language
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
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
                    statusLabel = _loc?.Get("module.statusAvailable") ?? "Available";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = _loc?.Get("module.fire.title") ?? "Fire & Explosion Response";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = _loc?.Get("module.fire.description") ?? "Learn to identify, respond and control fire hazards in mining environments.";
                    break;

                case ModuleId.GasLeakConfinedSpace:
                    iconSprite  = UIHelper.LoadProjectSprite("icon_gas_ref1.png") ?? UIHelper.GetGasEmojiSprite();
                    iconBg      = UIColors.Hex("#E0F2FE");
                    statusColor = UIColors.Hex("#0284C7");
                    statusLabel = _loc?.Get("module.statusTheoryOnly") ?? "THEORY / ASSESSMENT";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = _loc?.Get("module.gas.title") ?? "Gas Leak & Confined Space";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = _loc?.Get("module.gas.description") ?? "Stay safe in hazardous gas environments and confined spaces.";
                    break;

                case ModuleId.MachinerySafety:
                    iconSprite  = UIHelper.LoadProjectSprite("icon_gear_ref1.png") ?? UIHelper.GetGearEmojiSprite();
                    iconBg      = UIColors.Hex("#DCFCE7");
                    statusColor = UIColors.Hex("#0D9488");
                    statusLabel = _loc?.Get("module.statusTheoryOnly") ?? "THEORY / ASSESSMENT";
                    if (string.IsNullOrEmpty(title) || title == module.titleKey)
                        title = _loc?.Get("module.machinery.title") ?? "Machinery Safety";
                    if (string.IsNullOrEmpty(desc) || desc == module.descriptionKey)
                        desc = _loc?.Get("module.machinery.description") ?? "Identify machinery, understand risks and follow safe procedures.";
                    break;

                case ModuleId.ElectricalSafety:
                    iconSprite  = UIHelper.GetBoltSprite();
                    iconBg      = UIColors.Hex("#FEF3C7");
                    iconColor   = UIColors.Hex("#D97706");
                    statusColor = UIColors.Hex("#64748B");
                    statusLabel = _loc?.Get("module.statusComingSoon") ?? "COMING SOON";
                    title = _loc?.Get("module.electrical.title") ?? "Electrical Safety (Coming Soon)";
                    desc = _loc?.Get("module.electrical.description") ?? "Learn electrical safety practices for mining environments.";
                    break;

                case ModuleId.MineHazardEnvironment:
                    iconSprite  = UIHelper.GetHardHatSprite();
                    iconBg      = UIColors.Hex("#FFEDD5");
                    iconColor   = UIColors.Hex("#EA580C");
                    statusColor = UIColors.Hex("#64748B");
                    statusLabel = _loc?.Get("module.statusComingSoon") ?? "COMING SOON";
                    title = _loc?.Get("module.minehazard.title") ?? "Mine Hazard & Environment";
                    desc = _loc?.Get("module.minehazard.description") ?? "Understand mine hazards and environmental risks for a safer workplace.";
                    break;

                default:
                    iconSprite  = UIHelper.GetFireEmojiSprite();
                    iconBg      = UIColors.Hex("#F8FAFC");
                    statusColor = UIColors.Hex("#059669");
                    statusLabel = _loc?.Get("module.statusAvailable") ?? "Available";
                    break;
            }

            string typeChipText = _loc?.Get("module.trainingModule") ?? "Training Module";

            var cardGO = ModuleSelectionBuilder.CreateModuleCard(
                parent, $"card-{module.id}", iconSprite, iconBg,
                title, desc,
                module.isLocked, statusColor, statusLabel, iconColor, typeChipText);

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
            if (tmp != null && !string.IsNullOrEmpty(text))
            {
                if (DevanagariShaper.HasDevanagari(text))
                {
                    tmp.text = DevanagariShaper.Shape(text);
                    try { tmp.font = UIHelper.GetDevanagariFont(); } catch {}
                }
                else
                {
                    tmp.text = text;
                }
            }
        }

        private static void SetLabel(GameObject root, string name, string text)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp != null && !string.IsNullOrEmpty(text))
            {
                if (DevanagariShaper.HasDevanagari(text))
                {
                    tmp.text = DevanagariShaper.Shape(text);
                    try { tmp.font = UIHelper.GetDevanagariFont(); } catch {}
                }
                else
                {
                    tmp.text = text;
                }
            }
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
