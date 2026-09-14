using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Core
{
    /// <summary>
    /// CertificateGenerator
    /// ====================
    /// Governs verifiable training certificate issuance for SurakshaAR.
    /// Satisfies Rehan's Day 6 deliverable:
    ///   - Strict eligibility gate: Passing assessment + Competency requirements cleared + Zero critical errors.
    ///   - Standardized Certificate ID structure: SUR-YYYY-NNNN.
    ///   - Verifiable QR Code payload and cryptographic tamper-detection checksum.
    ///   - Backend sync with Omesh's /api/v1/certificates/verify/{certificate_number}.
    /// </summary>
    public class CertificateGenerator : MonoBehaviour
    {
        public static CertificateGenerator Instance { get; private set; }

        public const string VERIFICATION_BASE_URL = "https://surakshaar.gov.in/api/v1/certificates/verify/";

        [Serializable]
        public class IssuedCertificate
        {
            public string certificate_id;          // e.g. SUR-2026-0042
            public int worker_id;
            public string worker_name;
            public string module_id;
            public string module_name;
            public float final_score;
            public string issued_at;
            public string valid_until;
            public string verification_url;
            public string security_signature;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Strict eligibility check.
        /// A certificate is NEVER issued purely on a high score if a critical safety violation occurred.
        /// </summary>
        public bool IsEligibleForCertificate(AssessmentResultData result, out string eligibilityReason)
        {
            if (result == null)
            {
                eligibilityReason = "No assessment result available.";
                return false;
            }

            if (result.critical_errors != null && result.critical_errors.Count > 0)
            {
                eligibilityReason = "INELIGIBLE: Critical safety violations occurred during the evaluation.";
                return false;
            }

            if (!result.passed || result.overall_score < CompetencyEngine.OVERALL_PASS_THRESHOLD)
            {
                eligibilityReason = $"INELIGIBLE: Overall score ({result.overall_score:F1}%) does not meet minimum pass threshold ({CompetencyEngine.OVERALL_PASS_THRESHOLD:F0}%).";
                return false;
            }

            if (result.weaknesses != null && result.weaknesses.Count > 0)
            {
                eligibilityReason = "INELIGIBLE: Worker has unaddressed competency weaknesses. Remedial retraining required.";
                return false;
            }

            eligibilityReason = "ELIGIBLE: All competencies passed with zero critical errors.";
            return true;
        }

        /// <summary>
        /// Issues a certificate following official ID format SUR-YYYY-NNNN.
        /// </summary>
        public IssuedCertificate GenerateCertificate(int workerId, string workerName, string moduleId, string moduleName, AssessmentResultData result)
        {
            if (!IsEligibleForCertificate(result, out string reason))
            {
                Debug.LogError($"[CERTIFICATE] Issuance rejected: {reason}");
                return null;
            }

            int year = DateTime.UtcNow.Year;
            int counter = PlayerPrefs.GetInt("CERT_GLOBAL_COUNT", 100);
            counter++;
            PlayerPrefs.SetInt("CERT_GLOBAL_COUNT", counter);
            PlayerPrefs.Save();

            string certId = $"SUR-{year}-{counter:0000}";
            string issueDate = DateTime.UtcNow.ToString("o");
            string validUntil = DateTime.UtcNow.AddYears(1).ToString("o");
            string verifyUrl = $"{VERIFICATION_BASE_URL}{certId}";
            string signature = ComputeSignature(certId, workerId, moduleId, result.overall_score);

            var cert = new IssuedCertificate
            {
                certificate_id = certId,
                worker_id = workerId,
                worker_name = workerName,
                module_id = moduleId,
                module_name = moduleName,
                final_score = result.overall_score,
                issued_at = issueDate,
                valid_until = validUntil,
                verification_url = verifyUrl,
                security_signature = signature
            };

            // Store locally in offline registry
            string json = JsonUtility.ToJson(cert);
            PlayerPrefs.SetString($"CERT_{certId}", json);
            PlayerPrefs.SetString($"WORKER_LATEST_CERT_{workerId}_{moduleId}", certId);
            PlayerPrefs.Save();

            Debug.Log($"[CERTIFICATE] Certificate {certId} successfully issued to {workerName} for {moduleName}!");
            return cert;
        }

        /// <summary>
        /// Cryptographic signature for tamper-detection.
        /// </summary>
        public static string ComputeSignature(string certId, int workerId, string moduleId, float score)
        {
            string raw = $"{certId}|{workerId}|{moduleId}|{score:F1}|SURAKSHA_AR_SECRET";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16);
            }
        }

        public static bool VerifySignature(IssuedCertificate cert)
        {
            if (cert == null) return false;
            string expected = ComputeSignature(cert.certificate_id, cert.worker_id, cert.module_id, cert.final_score);
            return cert.security_signature == expected;
        }
    }
}
