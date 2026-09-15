using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Author-time data describing one training module card.
    /// Create one instance per module via
    /// Assets > Create > SurakshaAR > Module Data, or use the
    /// hard-coded ModuleCatalog if you prefer not to
    /// create ScriptableObject assets by hand.
    /// </summary>
    [CreateAssetMenu(fileName = "ModuleData", menuName = "SurakshaAR/Module Data")]
    public class ModuleData : ScriptableObject
    {
        [Header("Identity")]
        public ModuleId id;

        [Header("Display (English fallback; real text comes from LocalizationManager)")]
        public string titleKey;          // localization key, e.g. "module.fire.title"
        public string descriptionKey;    // localization key
        public string bannerSpriteName;  // sprite in Resources/Sprites, e.g. "banner_fire"
        public string iconSpriteName;    // small icon sprite, e.g. "icon_fire"

        [Header("Stat chips")]
        public int scenarioCount = 3;
        public string durationLabel = "15-20 min";
        public string difficultyKey = "difficulty.intermediate";

        [Header("What you will learn (localization keys, in order)")]
        public List<string> learningPointKeys = new List<string>();

        [Header("Availability")]
        public bool isLocked = false;       // true => "Coming Soon", card is dimmed and not tappable
        public bool isImplemented = false;  // true only for modules that actually have an AR scene built

        [Header("AR Handoff")]
        [Tooltip("Exact scene name as added in File > Build Settings. Must match a scene that already exists inside Assets/AR_Fire_Foundation (or a future module's AR folder). Leave empty if isImplemented is false.")]
        public string arSceneName;
    }
}