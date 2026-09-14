namespace SurakshaAR.Data
{
    /// <summary>
    /// Identifies each safety module card shown on Home Dashboard /
    /// Module Selection / Module Detail.
    /// </summary>
    public enum ModuleId
    {
        FireAndExplosion = 0,
        GasLeakConfinedSpace = 1,
        MachinerySafety = 2,
        ElectricalSafety = 3,   // locked / coming soon
        MineHazardEnvironment = 4  // locked / coming soon
    }
}