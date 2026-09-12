namespace SurakshaAR.Core
{
    /// <summary>
    /// Semantic UI states the app must be able to represent.
    /// Used conceptually by controllers and screen builders to adjust
    /// visual state. No longer depends on UnityEngine.UIElements.
    /// </summary>
    public enum UIStateKind
    {
        Normal,
        Loading,
        Empty,
        Error,
        Success,
        Locked,
        Completed,
        InProgress,
        Failed,
        Offline,
        CriticalError,
        AssessmentPassed,
        AssessmentFailed,
        RetrainingRequired
    }

    /// <summary>
    /// Provides string representations of UIStateKind values.
    /// All UIToolkit/VisualElement code has been removed.
    /// </summary>
    public static class UIState
    {
        public const string ClassPrefix = "ui-state--";

        public static string ToClassName(UIStateKind state)
        {
            switch (state)
            {
                case UIStateKind.Loading:             return ClassPrefix + "loading";
                case UIStateKind.Empty:               return ClassPrefix + "empty";
                case UIStateKind.Error:               return ClassPrefix + "error";
                case UIStateKind.Success:             return ClassPrefix + "success";
                case UIStateKind.Locked:              return ClassPrefix + "locked";
                case UIStateKind.Completed:           return ClassPrefix + "completed";
                case UIStateKind.InProgress:          return ClassPrefix + "in-progress";
                case UIStateKind.Failed:              return ClassPrefix + "failed";
                case UIStateKind.Offline:             return ClassPrefix + "offline";
                case UIStateKind.CriticalError:       return ClassPrefix + "critical-error";
                case UIStateKind.AssessmentPassed:    return ClassPrefix + "assessment-passed";
                case UIStateKind.AssessmentFailed:    return ClassPrefix + "assessment-failed";
                case UIStateKind.RetrainingRequired:  return ClassPrefix + "retraining-required";
                default:                              return ClassPrefix + "normal";
            }
        }
    }
}