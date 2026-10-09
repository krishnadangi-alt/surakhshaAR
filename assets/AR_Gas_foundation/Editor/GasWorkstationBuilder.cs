#if UNITY_EDITOR
using UnityEditor;

public class GasWorkstationBuilder
{
    [MenuItem("SurakshaAR/Build Complete 3D Industrial Gas Workstation")]
    public static void BuildCompleteWorkstation()
    {
        GasWorkstationVisualFixer.FixWorkstationVisuals();
    }

    [MenuItem("SurakshaAR/Build Workstation + Capture Screenshots")]
    public static void BuildAndCaptureScreenshots()
    {
        GasWorkstationVisualFixer.FixWorkstationVisuals();
    }

    [MenuItem("SurakshaAR/Build Workstation + Capture Screenshots + Build APK")]
    public static void BuildAllCombined()
    {
        GasWorkstationVisualFixer.FixWorkstationVisuals();
    }
}
#endif
