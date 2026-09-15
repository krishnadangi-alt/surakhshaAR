using SurakshaAR.Core;
using UnityEngine;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Routes ScreenId to the correct builder.
    /// UIManager calls this to get a screen's root GameObject.
    /// </summary>
    public static class ScreenFactory
    {
        public static GameObject Build(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Splash:            return SplashScreenBuilder.Build();
                case ScreenId.LanguageSelection: return LanguageSelectionBuilder.Build();
                case ScreenId.Login:             return LoginBuilder.Build();
                case ScreenId.HomeDashboard:     return HomeDashboardBuilder.Build();
                case ScreenId.ModuleSelection:   return ModuleSelectionBuilder.Build();
                case ScreenId.ModuleDetail:      return ModuleDetailBuilder.Build();
                case ScreenId.ScenarioSelection: return ScenarioSelectionBuilder.Build();
                case ScreenId.Assessment:        return AssessmentBuilder.Build();
                case ScreenId.Result:            return ResultBuilder.Build();
                case ScreenId.Certificate:       return CertificateBuilder.Build();
                case ScreenId.Progress:          return ProgressBuilder.Build();
                case ScreenId.ProfileSetup:      return ProfileBuilder.Build();
                case ScreenId.Notifications:     return NotificationsBuilder.Build();
                default:
                    Debug.LogWarning($"[ScreenFactory] No builder for {id}, returning empty screen.");
                    return MakeFallback(id.ToString());
            }
        }

        private static GameObject MakeFallback(string name)
        {
            var go = new GameObject($"Screen_{name}");
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = UIColors.Background;
            return go;
        }
    }
}
