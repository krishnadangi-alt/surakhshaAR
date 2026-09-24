using UnityEngine;

namespace SurakshaAR.UI
{
    /// <summary>
    /// SurakshaAR brand color palette — navy blue professional theme.
    /// Matches the Figma design tokens exactly.
    /// </summary>
    public static class UIColors
    {
        // ── Primary Navy ──────────────────────────────────────────────
        public static readonly Color Primary      = Hex("#123F66"); // Deep Navy
        public static readonly Color PrimaryDark  = Hex("#0D2F4A"); // Dark Navy
        public static readonly Color PrimaryMid   = Hex("#164D78"); // Mid Navy
        public static readonly Color PrimaryLight = Hex("#EAF3F9"); // Light Navy Tint

        // ── Surfaces ──────────────────────────────────────────────────
        public static readonly Color Background   = Hex("#F6F8FA"); // Page BG
        public static readonly Color Card         = Color.white;
        public static readonly Color CardMuted    = Hex("#F0F3F6");

        // ── Text ──────────────────────────────────────────────────────
        public static readonly Color TextPrimary   = Hex("#17212B");
        public static readonly Color TextSecondary = Hex("#52606D");
        public static readonly Color TextMuted     = Hex("#7B8794");
        public static readonly Color TextInverse   = Color.white;
        public static readonly Color TextOnNavy    = Color.white;
        public static readonly Color TextOnNavyDim = new Color(1f, 1f, 1f, 0.70f);

        // ── Border ────────────────────────────────────────────────────
        public static readonly Color Border       = Hex("#D9E1E7");
        public static readonly Color BorderMedium = Hex("#C4CFD8");

        // ── Status ────────────────────────────────────────────────────
        public static readonly Color Success      = Hex("#238636");
        public static readonly Color SuccessBg    = Hex("#E6F4EC");
        public static readonly Color Warning      = Hex("#D97706");
        public static readonly Color WarningBg    = Hex("#FEF3C7");
        public static readonly Color Danger       = Hex("#C62828");
        public static readonly Color DangerBg     = Hex("#FDECEC");
        public static readonly Color Info         = Hex("#2563A6");
        public static readonly Color InfoBg       = Hex("#EAF2FB");

        // ── Module Accents ────────────────────────────────────────────
        public static readonly Color FireAccent   = Hex("#C62828");
        public static readonly Color FireAccentBg = Hex("#FDECEC");
        public static readonly Color GasAccent    = Hex("#2563A6");
        public static readonly Color GasAccentBg  = Hex("#EAF2FB");
        public static readonly Color MachineAccent   = Hex("#238636");
        public static readonly Color MachineAccentBg = Hex("#E6F4EC");
        public static readonly Color ElecAccent   = Hex("#D97706");
        public static readonly Color ElecAccentBg = Hex("#FEF3C7");

        // ── Semi-transparent overlays ─────────────────────────────────
        public static readonly Color NavyOverlay12   = new Color(1f, 1f, 1f, 0.12f);
        public static readonly Color NavyOverlay15   = new Color(1f, 1f, 1f, 0.15f);
        public static readonly Color NavyOverlay20   = new Color(1f, 1f, 1f, 0.20f);
        public static readonly Color NavyBorder20    = new Color(1f, 1f, 1f, 0.20f);
        // ── Primary Theme & CTA (Deep Blue per user mandate: "menu blue only not green") ────
        public static readonly Color SafetyGreen      = Hex("#123F66"); // Primary Blue CTA
        public static readonly Color SafetyGreenDark  = Hex("#0D2F4A"); // Pressed Deep Navy
        public static readonly Color SafetyGreenLight = Hex("#EAF3F9"); // Tinted Blue
        public static readonly Color SafetyOrange     = Hex("#EA580C"); // Safety Warning Orange
        public static readonly Color SafetyOrangeBg   = Hex("#FFF7ED");

        // ── AR Overlay Glassmorphism ──────────────────────────────────
        public static readonly Color AROverlayDark      = new Color(0.06f, 0.09f, 0.08f, 0.92f);
        public static readonly Color AROverlayBorder    = new Color(1f, 1f, 1f, 0.16f);
        public static readonly Color ARBarBg            = new Color(0f, 0f, 0f, 0.50f);
        public static readonly Color ARProgressBarGreen = Hex("#22C55E");
        public static readonly Color ScoreHudBg         = new Color(0.06f, 0.10f, 0.14f, 0.88f);
        public static readonly Color AmberBorder        = Hex("#FDE68A");
        public static readonly Color AmberBg            = Hex("#FEF3C7");
        public static readonly Color AmberTextDark      = Hex("#92400E");
        public static readonly Color Transparent        = new Color(0f, 0f, 0f, 0f);

        // ── Helper ────────────────────────────────────────────────────
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c)) return c;
            return Color.magenta; // fallback — should never happen
        }
    }
}
