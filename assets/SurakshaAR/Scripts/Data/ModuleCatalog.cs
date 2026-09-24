using System.Collections.Generic;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Simple in-code catalog of the 5 modules shown in the mockups.
    /// This avoids having to hand-create 5 ScriptableObject assets in
    /// the Editor; AppManager builds ModuleData instances from this
    /// table at runtime.
    /// </summary>
    public static class ModuleCatalog
    {
        public static List<ModuleData> BuildDefaultCatalog()
        {
            var list = new List<ModuleData>();

            // ---- Fire & Explosion Response (fully implemented, has AR scene) ----
            var fire = ScriptableObjectUtil.CreateModule();
            fire.id = ModuleId.FireAndExplosion;
            fire.titleKey ="module.fire.title";
            fire.descriptionKey ="module.fire.description";
            fire.bannerSpriteName = "banner_fire";
            fire.iconSpriteName = "icon_fire";
            fire.scenarioCount = 3;
            fire.durationLabel = "15-20 min";
            fire.difficultyKey = "difficulty.intermediate";
            fire.learningPointKeys = new List<string>
            {
                "module.fire.learn.1", "module.fire.learn.2", "module.fire.learn.3",
                "module.fire.learn.4", "module.fire.learn.5"
            };
            fire.isLocked = false;
            fire.isImplemented = true;
            // The production AR scene is loaded additively by ARModuleLauncher.
            fire.arSceneName = "FireTraining";
            list.Add(fire);

            // ---- Gas Leak & Confined Space (UI only, AR not built yet) ----
            var gas = ScriptableObjectUtil.CreateModule();
            gas.id = ModuleId.GasLeakConfinedSpace;
            gas.titleKey = "module.gas.title";
            gas.descriptionKey = "module.gas.description";
            gas.bannerSpriteName = "banner_gas";
            gas.iconSpriteName = "icon_gas";
            gas.scenarioCount = 3;
            gas.durationLabel = "15-20 min";
            gas.difficultyKey = "difficulty.intermediate";
            gas.learningPointKeys = new List<string>
            {
                "module.gas.learn.1", "module.gas.learn.2", "module.gas.learn.3", "module.gas.learn.4"
            };
            gas.isLocked = false;
            gas.isImplemented = true;
            gas.arSceneName = "AR_gas_Foundation";
            list.Add(gas);

            // ---- Machinery Safety (UI only, AR not built yet) ----
            var machinery = ScriptableObjectUtil.CreateModule();
            machinery.id = ModuleId.MachinerySafety;
            machinery.titleKey = "module.machinery.title";
            machinery.descriptionKey = "module.machinery.description";
            machinery.bannerSpriteName = "banner_machinery";
            machinery.iconSpriteName = "icon_machinery";
            machinery.scenarioCount = 3;
            machinery.durationLabel = "15-20 min";
            machinery.difficultyKey = "difficulty.intermediate";
            machinery.learningPointKeys = new List<string>
            {
                "module.machinery.learn.1", "module.machinery.learn.2", "module.machinery.learn.3",
                "module.machinery.learn.4", "module.machinery.learn.5"
            };
            machinery.isLocked = false;
            machinery.isImplemented = false;
            machinery.arSceneName = "";
            list.Add(machinery);

            // ---- Electrical Safety (locked / coming soon per mockup) ----
            var electrical = ScriptableObjectUtil.CreateModule();
            electrical.id = ModuleId.ElectricalSafety;
            electrical.titleKey = "module.electrical.title";
            electrical.descriptionKey = "module.electrical.description";
            electrical.bannerSpriteName = "banner_electrical";
            electrical.iconSpriteName = "icon_electrical";
            electrical.isLocked = true;
            electrical.isImplemented = false;
            list.Add(electrical);

            // ---- Mine Hazard & Environment (locked / coming soon per mockup) ----
            var mineHazard = ScriptableObjectUtil.CreateModule();
            mineHazard.id = ModuleId.MineHazardEnvironment;

            mineHazard.titleKey = "module.minehazard.title";
            mineHazard.descriptionKey = "module.minehazard.description";
            mineHazard.bannerSpriteName = "banner_minehazard";
            mineHazard.iconSpriteName = "icon_minehazard";
            mineHazard.isLocked = true;
            mineHazard.isImplemented = false;
            list.Add(mineHazard);

            return list;
        }
    }

    /// <summary>
    /// Runtime ScriptableObjects must be created with
    /// ScriptableObject.CreateInstance, not "new". This tiny helper
    /// keeps ModuleCatalog readable.
    /// </summary>
    internal static class ScriptableObjectUtil
    {
        public static ModuleData CreateModule()
        {
            return UnityEngine.ScriptableObject.CreateInstance<ModuleData>();
        }
    }
}
