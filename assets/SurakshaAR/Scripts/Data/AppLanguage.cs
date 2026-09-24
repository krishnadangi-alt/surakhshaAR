namespace SurakshaAR.Data
{
    /// <summary>
    /// Writing direction for localized scripts.
    /// Santali (Ol Chiki), Hindi (Devanagari), and English (Latin) are strictly LeftToRight.
    /// </summary>
    public enum TextDirection
    {
        LeftToRight = 0,
        RightToLeft = 1
    }

    /// <summary>
    /// The three languages supported by the Language Selection screen.
    /// </summary>
    public enum AppLanguage
    {
        English = 0,
        Hindi = 1,
        Santali = 2
    }
}