using System;

/// <summary>
/// LocalAuthenticator
/// ==================
/// LOCAL / MOCK login for development - NO backend, NO network, NO fake
/// API calls. The Login button calls Authenticate() and this class
/// performs a real local validation action.
///
/// Rules (configurable):
///   - Employee ID must be at least 3 characters.
///   - Password must be at least 4 characters.
///
/// FUTURE BACKEND:
/// Replace the IAuthProvider implementation with a real one
/// (e.g. backed by SurakshaApiClient). The UI only depends on the
/// interface below, so nothing else changes.
/// </summary>
public interface IAuthProvider
{
    /// <summary>
    /// Validates credentials locally. onResult(success, message) is
    /// always invoked exactly once.
    /// </summary>
    void Authenticate(string employeeId, string password,
        Action<bool, string> onResult);
}

[System.Serializable]
public class LocalAuthenticator : IAuthProvider
{
    public int minIdLength = 3;
    public int minPasswordLength = 4;

    /// <summary>Small artificial delay so the button feels responsive.</summary>
    public float simulatedDelaySeconds = 0.25f;

    public void Authenticate(string employeeId, string password,
        Action<bool, string> onResult)
    {
        // Local, synchronous validation (no network).
        if (string.IsNullOrWhiteSpace(employeeId) ||
            employeeId.Trim().Length < minIdLength)
        {
            onResult?.Invoke(false,
                "Employee ID must be at least " + minIdLength + " characters.");
            return;
        }

        if (string.IsNullOrEmpty(password) ||
            password.Length < minPasswordLength)
        {
            onResult?.Invoke(false,
                "Password must be at least " + minPasswordLength + " characters.");
            return;
        }

        onResult?.Invoke(true, "OK");
    }
}
