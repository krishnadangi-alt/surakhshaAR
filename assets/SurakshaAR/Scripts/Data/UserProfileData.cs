using System;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Plain data model for the worker profile collected on the
    /// "Worker Profile Setup" screen. Persisted to disk as JSON so the
    /// app remembers the worker between sessions (offline-friendly).
    /// </summary>
    [Serializable]
    public class UserProfileData
    {
        public string fullName = "";
        public string workerId = "";
        public string sector = "Mining";
        public string experienceYears = "Less than 1 year";
        public AppLanguage language = AppLanguage.English;

        private const string SaveKey = "SurakshaAR_UserProfile";

        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(fullName) &&
                   !string.IsNullOrWhiteSpace(workerId);
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static UserProfileData Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                try
                {
                    return JsonUtility.FromJson<UserProfileData>(json);
                }
                catch
                {
                    return new UserProfileData();
                }
            }
            return new UserProfileData();
        }

        public static void ClearSaved()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }
}