using System.Collections.Generic;
using SurakshaAR.Data;

namespace SurakshaAR.Localization
{
    /// <summary>
    /// Central string table for SurakshaAR in English, Hindi (Devanagari), and Santali (Ol Chiki).
    /// Keys use dot-notation: "screen.element".
    ///
    /// Hindi quality policy:
    ///   - Official Government / PSU / Industrial Safety standard.
    ///   - Native Indian Android public-service style (natural, concise, familiar).
    ///   - Terminology aligns with Ministry of Mines & DGMS standards.
    ///   - Natural worker-friendly instructions and clear action cues.
    ///
    /// Santali integrity policy:
    ///   - Only verified genuine Santali strings in Ol Chiki script (U+1C50 - U+1C7F).
    ///   - Never Romanized, never fake Ol Chiki, never silent Hindi substitution.
    /// </summary>
    public static class LocalizedStrings
    {
        public static readonly Dictionary<string, Dictionary<AppLanguage, string>> Table =
            new Dictionary<string, Dictionary<AppLanguage, string>>
        {
            { "assessment.confirmSubmit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Are you sure you want to submit your assessment?" },
                { AppLanguage.Hindi,   "क्या आप निश्चित रूप से अपना मूल्यांकन जमा करना चाहते हैं?" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱡᱚᱢᱟ ᱞᱟᱹᱜᱤᱫ ᱥᱟᱹᱨᱤ ᱜᱮ ᱨᱮᱵᱮᱱ ᱢᱮᱱᱟᱢᱟ?" } } },
            { "assessment.finishAndCertify", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Submit Assessment & View Certificate ✓" },
                { AppLanguage.Hindi,   "मूल्यांकन जमा करें और प्रमाणपत्र देखें ✓" },
                { AppLanguage.Santali, "ᱛᱮᱞᱟ ᱡᱚᱢᱟ ᱟᱨ ᱥᱟᱨᱴᱤᱯᱷᱤᱠᱮᱴ ᱧᱮᱞ ᱢᱮ ✓" } } },
            { "assessment.nextQuestion", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Next Question ➔" },
                { AppLanguage.Hindi,   "अगला प्रश्न ➔" },
                { AppLanguage.Santali, "ᱫᱚᱥᱟᱨ ᱠᱩᱠᱞᱤ ➔" } } },
            { "assessment.question1.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "A 415V live electrical panel fire is a CLASS C hazard. Select the ONLY agent that is electrically non-conductive and safe. Water and foam conduct electricity — DO NOT use them on live equipment." },
                { AppLanguage.Hindi,   "415V चालू विद्युत पैनल में आग वर्ग C का खतरा है। केवल वही माध्यम चुनें जो विद्युत कुचालक और सुरक्षित हो। पानी और झाग बिजली का संचालन करते हैं — चालू उपकरणों पर उनका उपयोग न करें।" },
                { AppLanguage.Santali, "415V ᱵᱤᱡᱽᱞᱤ ᱯᱮᱱᱟᱞ ᱥᱮᱸᱜᱮᱞ ᱫᱚ ᱠᱞᱟᱥ C ᱵᱚᱛᱚᱨ ᱠᱟᱱᱟ ᱾ ᱠᱷᱟᱹᱞᱤ ᱵᱤᱡᱽᱞᱤ ᱵᱟᱝ ᱯᱟᱨᱚᱢᱚᱜ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱤᱬᱤᱡᱤᱡ ᱵᱟᱪᱷᱟᱣ ᱢᱮ ᱾ ᱫᱟᱜ ᱟᱨ ᱯᱷᱮᱱ ᱫᱚ ᱵᱤᱡᱽᱞᱤ ᱯᱟᱨᱚᱢᱟ — ᱵᱤᱡᱽᱞᱤ ᱥᱟᱢᱟᱱ ᱨᱮ ᱟᱞᱚᱢ ᱵᱮᱵᱷᱟᱨᱟ ᱾" } } },
            { "assessment.question1.hintBtn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hint" },
                { AppLanguage.Hindi,   "संकेत" },
                { AppLanguage.Santali, "ᱫᱤᱥᱟᱹ" } } },
            { "assessment.question1.opt1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "CO₂ Extinguisher (Class C/Electrical)" },
                { AppLanguage.Hindi,   "CO₂ अग्निशामक (वर्ग C/विद्युत)" },
                { AppLanguage.Santali, "CO₂ ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡᱤᱡ (ᱠᱞᱟᱥ C/ᱵᱤᱡᱽᱞᱤ)" } } },
            { "assessment.question1.opt2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Water Hose Reel (Class A only)" },
                { AppLanguage.Hindi,   "वाटर होज़ रील (केवल वर्ग A)" },
                { AppLanguage.Santali, "ᱫᱟᱜ ᱦᱳᱡᱽ (ᱠᱷᱟᱹᱞᱤ ᱠᱞᱟᱥ A)" } } },
            { "assessment.question1.opt3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Dry Powder Extinguisher (Multipurpose)" },
                { AppLanguage.Hindi,   "ड्राई पाउडर अग्निशामक (बहुउद्देश्यीय)" },
                { AppLanguage.Santali, "ᱨᱚᱦᱚᱲ ᱜᱩᱸᱰᱟᱹ ᱤᱬᱤᱡᱤᱡ (ᱡᱚᱛᱚ ᱞᱮᱠᱟᱱ)" } } },
            { "assessment.question1.opt4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Foam Extinguisher (Flammable Liquids)" },
                { AppLanguage.Hindi,   "फोम अग्निशामक (ज्वलनशील तरल)" },
                { AppLanguage.Santali, "ᱯᱷᱮᱱ ᱤᱬᱤᱡᱤᱡ (ᱡᱩᱞᱩᱜ ᱫᱟᱜ ᱡᱤᱱᱤᱥ)" } } },
            { "assessment.question1.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Which extinguisher is correct for an electrical panel fire?" },
                { AppLanguage.Hindi,   "विद्युत पैनल की आग बुझाने के लिए कौन सा अग्निशामक सही है?" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱯᱮᱱᱟᱞ ᱥᱮᱸᱜᱮᱞ ᱞᱟᱹᱜᱤᱫ ᱚᱠᱟ ᱤᱬᱤᱡᱤᱡ ᱴᱷᱤᱠ ᱜᱮᱭᱟ?" } } },
            { "assessment.question2.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "PASS is the standard 4-step sequence: Pull safety pin, Aim nozzle at base of fire, Squeeze operating lever, and Sweep side-to-side across the hazard." },
                { AppLanguage.Hindi,   "PASS 4 चरणों का मानक क्रम है: सुरक्षा पिन निकालें (Pull), आग के आधार पर निशाना लगाएँ (Aim), लीवर दबाएँ (Squeeze), और दोनों तरफ घुमाएँ (Sweep)।" },
                { AppLanguage.Santali, "PASS ᱫᱚ ᱔ ᱛᱷᱚᱠ ᱨᱮᱱᱟᱜ ᱱᱤᱭᱚᱢ ᱠᱟᱱᱟ: ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ (Pull), ᱥᱮᱸᱜᱮᱞ ᱵᱩᱴᱟᱹ ᱨᱮ ᱴᱟᱨᱜᱮᱴ (Aim), ᱞᱤᱵᱷᱟᱨ ᱞᱤᱱ (Squeeze), ᱟᱨ ᱦᱟᱱᱛᱮ ᱱᱷᱟᱛᱮ ᱦᱤᱞᱟᱹᱣ (Sweep) ᱾" } } },
            { "assessment.question2.hintBtn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hint" },
                { AppLanguage.Hindi,   "संकेत" },
                { AppLanguage.Santali, "ᱫᱤᱥᱟᱹ" } } },
            { "assessment.question2.opt1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Pull pin, Aim at base, Squeeze lever, Sweep side-to-side" },
                { AppLanguage.Hindi,   "पिन निकालें, आधार पर निशाना लगाएँ, लीवर दबाएँ, दोनों तरफ घुमाएँ" },
                { AppLanguage.Santali, "ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ, ᱵᱩᱴᱟᱹ ᱨᱮ ᱴᱟᱨᱜᱮᱴ, ᱞᱤᱵᱷᱟᱨ ᱞᱤᱱ, ᱦᱟᱱᱛᱮ ᱱᱷᱟᱛᱮ ᱦᱤᱞᱟᱹᱣ" } } },
            { "assessment.question2.opt2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Push handle, Aim at flames, Spray direct, Step back" },
                { AppLanguage.Hindi,   "हैंडल दबाएं, लपटों पर निशाना लगाएं, सीधे स्प्रे करें, पीछे हटें" },
                { AppLanguage.Santali, "ᱦᱮᱱᱰᱮᱞ ᱴᱷᱮᱞᱟᱣ, ᱥᱮᱸᱜᱮᱞ ᱞᱟᱯᱟᱴ ᱨᱮ ᱥᱯᱨᱮ, ᱛᱟᱭᱚᱢ ᱥᱮᱱ ᱚᱰᱚᱠ" } } },
            { "assessment.question2.opt3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Press valve, Activate horn, Squeeze trigger, Stop" },
                { AppLanguage.Hindi,   "वाल्व दबाएं, हॉर्न चालू करें, ट्रिगर दबाएं, रुकें" },
                { AppLanguage.Santali, "ᱵᱷᱟᱞᱵᱽ ᱞᱤᱱ, ᱦᱚᱨᱱ ᱪᱟᱹᱞᱩ, ᱴᱨᱤᱜᱟᱨ ᱞᱤᱱ, ᱛᱷᱟᱢᱵᱷᱟᱣ" } } },
            { "assessment.question2.opt4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Pull pin, Aim at top smoke, Squeeze valve, Shake cylinder" },
                { AppLanguage.Hindi,   "पिन खींचें, ऊपर धुएं पर निशाना लगाएं, वाल्व दबाएं, सिलेंडर हिलाएं" },
                { AppLanguage.Santali, "ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ, ᱪᱮᱛᱟᱱ ᱫᱷᱩᱶᱟᱹ ᱨᱮ ᱥᱯᱨᱮ, ᱵᱷᱟᱞᱵᱽ ᱞᱤᱱ, ᱥᱤᱞᱤᱱᱰᱟᱨ ᱦᱤᱞᱟᱹᱣ" } } },
            { "assessment.question2.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "What does the official PASS fire extinguisher protocol stand for?" },
                { AppLanguage.Hindi,   "आधिकारिक PASS अग्निशामक संचालन प्रोटोकॉल का क्या अर्थ है?" },
                { AppLanguage.Santali, "ᱥᱚᱨᱠᱟᱨᱤ PASS ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡ ᱱᱤᱭᱚᱢ ᱨᱮᱱᱟᱜ ᱢᱮᱱᱮᱛ ᱫᱚ ᱪᱮᱫ?" } } },
            { "assessment.question3.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "DGMS safety rules require sounding the mine emergency alarm to alert crew and switching off / isolating the main power supply before approaching." },
                { AppLanguage.Hindi,   "DGMS सुरक्षा नियमों के अनुसार अग्निशामक चलाने से पहले आपातकालीन अलार्म बजाना और मुख्य बिजली आपूर्ति बंद करना अनिवार्य है।" },
                { AppLanguage.Santali, "DGMS ᱨᱩᱠᱷᱤᱭᱟᱹ ᱱᱤᱭᱚᱢ ᱞᱮᱠᱟᱛᱮ ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡ ᱢᱟᱬᱟᱝ ᱨᱮ ᱮᱞᱟᱨᱢ ᱵᱟᱡᱟᱣ ᱟᱨ ᱢᱩᱬᱩᱛ ᱵᱤᱡᱽᱞᱤ ᱠᱟᱴᱟᱣ ᱞᱟᱹᱠᱛᱤ ᱜᱮᱭᱟ ᱾" } } },
            { "assessment.question3.hintBtn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hint" },
                { AppLanguage.Hindi,   "संकेत" },
                { AppLanguage.Santali, "ᱫᱤᱥᱟᱹ" } } },
            { "assessment.question3.opt1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Sound emergency alarm and isolate electrical power supply" },
                { AppLanguage.Hindi,   "आपातकालीन अलार्म बजाएं और विद्युत मुख्य बिजली आपूर्ति बंद करें" },
                { AppLanguage.Santali, "ᱮᱢᱟᱨᱡᱮᱱᱥᱤ ᱮᱞᱟᱨᱢ ᱵᱟᱡᱟᱣ ᱟᱨ ᱵᱤᱡᱽᱞᱤ ᱨᱮᱱᱟᱜ ᱢᱩᱬᱩᱛ ᱥᱟᱯᱞᱟᱭ ᱵᱚᱸᱫᱽ ᱢᱮ" } } },
            { "assessment.question3.opt2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Immediately throw dry sand into panel vents without alerting others" },
                { AppLanguage.Hindi,   "दूसरों को सूचित किए बिना तुरंत पैनल में सूखी रेत फेंकें" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱞᱟᱹᱭ ᱠᱟᱛᱮ ᱞᱚᱜᱚᱱ ᱯᱮᱱᱟᱞ ᱨᱮ ᱨᱚᱦᱚᱲ ᱵᱟᱹᱞᱤ ᱪᱷᱟᱴᱠᱟᱣ ᱢᱮ" } } },
            { "assessment.question3.opt3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Open cabinet doors to inspect internal wiring connections" },
                { AppLanguage.Hindi,   "आंतरिक तारों की जांच करने के लिए कैबिनेट के दरवाजे खोलें" },
                { AppLanguage.Santali, "ᱵᱷᱤᱛᱨᱤ ᱛᱟᱨ ᱧᱮᱞ ᱞᱟᱹᱜᱤᱫ ᱠᱮᱵᱤᱱᱮᱴ ᱫᱩᱣᱟᱹᱨ ᱡᱷᱤᱡᱽ ᱢᱮ" } } },
            { "assessment.question3.opt4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Wait 10 minutes to see if the fire extinguishes on its own" },
                { AppLanguage.Hindi,   "10 मिनट प्रतीक्षा करें कि क्या आग अपने आप बुझ जाएगी" },
                { AppLanguage.Santali, "᱑᱐ ᱢᱤᱱᱤᱴ ᱛᱟᱺᱜᱤ ᱢᱮ ᱥᱮᱸᱜᱮᱞ ᱟᱡ ᱛᱮ ᱤᱬᱤᱡᱚᱜ ᱠᱟᱱᱟ ᱥᱮ ᱵᱟᱝ" } } },
            { "assessment.question3.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "What is the critical first action upon discovering an electrical fire before suppression?" },
                { AppLanguage.Hindi,   "बिजली की आग लगने पर बुझाने से पहले सबसे पहला महत्वपूर्ण कदम क्या है?" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱥᱮᱸᱜᱮᱞ ᱧᱮᱞ ᱠᱟᱛᱮ ᱤᱬᱤᱡ ᱢᱟᱬᱟᱝ ᱨᱮ ᱡᱚᱛᱚ ᱠᱷᱚᱱ ᱯᱩᱭᱞᱩ ᱠᱟᱹᱢᱤ ᱫᱚ ᱪᱮᱫ?" } } },
            { "assessment.questionProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "{0} of {1}" },
                { AppLanguage.Hindi,   "{0} का {1}" },
                { AppLanguage.Santali, "{1} ᱢᱩᱫᱽ ᱨᱮ {0}" } } },
            { "assessment.selectOne", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select one option to continue" },
                { AppLanguage.Hindi,   "आगे बढ़ने के लिए एक विकल्प का चयन करें" },
                { AppLanguage.Santali, "ᱢᱤᱫᱴᱟᱹᱝ ᱴᱷᱤᱠ ᱛᱮᱞᱟ ᱵᱟᱪᱷᱟᱣ ᱢᱮ:" } } },
            { "assessment.submit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Submit Assessment Response" },
                { AppLanguage.Hindi,   "मूल्यांकन उत्तर जमा करें" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱨᱮᱱᱟᱜ ᱛᱮᱞᱟ ᱡᱚᱢᱟ ᱢᱮ" } } },
            { "assessment.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire Safety Knowledge & SOP Test" },
                { AppLanguage.Hindi,   "अग्नि सुरक्षा ज्ञान एवं SOP मूल्यांकन" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱥᱮᱪᱮᱫ ᱯᱚᱨᱚᱠ ᱢᱮ" } } },
            { "assessment.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire Safety Assessment" },
                { AppLanguage.Hindi,   "अग्नि सुरक्षा मूल्यांकन" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱵᱤᱰᱟᱹᱣ" } } },
            { "cert.assessmentStatus", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Assessment Status:" },
                { AppLanguage.Hindi,   "मूल्यांकन स्थिति:" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱦᱟᱞᱚᱛ:" } } },
            { "cert.awaitingIssuance", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Awaiting Issuance" },
                { AppLanguage.Hindi,   "प्रमाणपत्र जारी होना प्रतीक्षित" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱮᱢ ᱛᱟᱺᱜᱤ ᱨᱮ" } } },
            { "cert.certId", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificate Number:" },
                { AppLanguage.Hindi,   "प्रमाणपत्र संख्या:" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱮᱞ:" } } },
            { "cert.certIdPending", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificate Number: Pending Official Decision" },
                { AppLanguage.Hindi,   "प्रमाणपत्र संख्या: आधिकारिक अनुमोदन लंबित" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱮᱞ: ᱥᱚᱨᱠᱟᱨᱤ ᱯᱷᱟᱹᱭᱥᱟᱞᱟ ᱛᱟᱺᱜᱤ ᱨᱮ" } } },
            { "cert.competencyGrade", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Competency Grade:" },
                { AppLanguage.Hindi,   "दक्षता श्रेणी:" },
                { AppLanguage.Santali, "ᱫᱟᱲᱮ ᱜᱨᱮᱰ: {0}" } } },
            { "cert.criticalErrors", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Critical Errors:" },
                { AppLanguage.Hindi,   "गंभीर त्रुटियाँ:" },
                { AppLanguage.Santali, "ᱵᱟᱹᱲᱤᱡ ᱵᱷᱩᱞ ᱠᱚ:" } } },
            { "cert.dateIssued", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Date Issued:" },
                { AppLanguage.Hindi,   "जारी करने की तिथि:" },
                { AppLanguage.Santali, "ᱮᱢ ᱟᱠᱟᱱ ᱢᱟᱹᱦᱤᱛ:" } } },
            { "cert.govTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "GOVERNMENT OF INDIA" },
                { AppLanguage.Hindi,   "भारत सरकार" },
                { AppLanguage.Santali, "ᱥᱤᱧᱚᱛ ᱥᱚᱨᱠᱟᱨ — ᱠᱷᱟᱫᱟᱱ ᱢᱚᱱᱛᱨᱟᱲᱚᱭ" } } },
            { "cert.guestWorker", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Guest Trainee" },
                { AppLanguage.Hindi,   "अतिथि शिक्षार्थी" },
                { AppLanguage.Santali, "ᱯᱮᱲᱟ ᱠᱟᱹᱢᱤᱭᱟᱹ (ᱥᱮᱪᱮᱫᱤᱭᱟᱹ)" } } },
            { "cert.idLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "ID:" },
                { AppLanguage.Hindi,   "आईडी:" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ: {0}" } } },
            { "cert.invalid", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Invalid" },
                { AppLanguage.Hindi,   "अमान्य" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱴᱷᱤᱠ" } } },
            { "cert.mineWorker", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mine Worker" },
                { AppLanguage.Hindi,   "खान कार्यकर्ता" },
                { AppLanguage.Santali, "ᱥᱟᱹᱵᱤᱛ ᱠᱷᱟᱫᱟᱱ ᱠᱟᱹᱢᱤᱭᱟᱹ" } } },
            { "cert.notAttempted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Not Attempted" },
                { AppLanguage.Hindi,   "प्रयास नहीं किया गया" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱠᱩᱨᱩᱢᱩᱴᱩ ᱟᱠᱟᱱᱟ" } } },
            { "cert.qrVerify", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scan QR Code for Official Verification" },
                { AppLanguage.Hindi,   "आधिकारिक सत्यापन के लिए क्यूआर कोड स्कैन करें" },
                { AppLanguage.Santali, "ᱯᱚᱨᱚᱠ ᱞᱟᱹᱜᱤᱫ ᱠᱤᱣ.ᱟᱨ. ᱠᱳᱰ ᱥᱠᱮᱱ ᱢᱮ" } } },
            { "cert.qualified", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Qualified (Certified)" },
                { AppLanguage.Hindi,   "योग्य (प्रमाणित)" },
                { AppLanguage.Santali, "ᱥᱟᱹᱵᱤᱛ ᱟᱠᱟᱱᱟ (QUALIFIED)" } } },
            { "cert.retrainingRequired", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Retraining Required" },
                { AppLanguage.Hindi,   "पुनः प्रशिक्षण आवश्यक" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱥᱮᱪᱮᱫ ᱞᱟᱹᱠᱛᱤᱭᱟ" } } },
            { "cert.score", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Score:" },
                { AppLanguage.Hindi,   "स्कोर:" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱮᱞ: {0}%" } } },
            { "cert.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Official Safety Certificate" },
                { AppLanguage.Hindi,   "आधिकारिक सुरक्षा प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱥᱚᱨᱠᱟᱨᱤ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ" } } },
            { "cert.valid", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Valid" },
                { AppLanguage.Hindi,   "मान्य" },
                { AppLanguage.Santali, "ᱴᱷᱤᱠ ᱜᱮᱭᱟ" } } },
            { "cert.verificationInProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Verification: In Progress" },
                { AppLanguage.Hindi,   "सत्यापन: जारी है" },
                { AppLanguage.Santali, "ᱯᱚᱨᱚᱠ: ᱪᱟᱞᱟᱜ ᱠᱟᱱᱟ" } } },
            { "cert.verifiedOfficial", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Verified per DGMS Industrial Safety Standards" },
                { AppLanguage.Hindi,   "खान सुरक्षा महानिदेशालय (DGMS) मानकों के अनुरूप सत्यापित" },
                { AppLanguage.Santali, "DGMS ᱠᱷᱟᱫᱟᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱱᱤᱭᱟᱹᱢ ᱞᱮᱠᱟᱛᱮ ᱥᱟᱹᱵᱤᱛ ᱟᱠᱟᱱᱟ" } } },
            { "certificate.allCredentials", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "All Safety Credentials" },
                { AppLanguage.Hindi,   "सभी सुरक्षा प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱡᱚᱛᱚ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱠᱚ" } } },
            { "certificate.assessmentCompletedDesc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Your fire response assessment has been submitted. Official certificate issuance is pending review by the safety administrator." },
                { AppLanguage.Hindi,   "आपका अग्नि सुरक्षा मूल्यांकन जमा कर दिया गया है। आधिकारिक प्रमाणपत्र जारी करने के लिए सुरक्षा प्रशासक की समीक्षा लंबित है।" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱥᱮᱸᱜᱮᱞ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱵᱤᱰᱟᱹᱣ ᱡᱚᱢᱟ ᱮᱱᱟ ᱾ ᱟᱹᱢᱟᱹᱞᱤᱭᱟᱹ ᱧᱮᱞ ᱯᱩᱨᱟᱹᱣ ᱛᱟᱭᱚᱢ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱮᱢᱚᱜ ᱦᱩᱭᱩᱜᱼᱟ ᱾" } } },
            { "certificate.assessmentCompletedTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Assessment Completed" },
                { AppLanguage.Hindi,   "मूल्यांकन संपन्न" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱯᱩᱨᱟᱹᱣ ᱮᱱᱟ" } } },
            { "certificate.awardedTo", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "This certificate is proudly awarded to" },
                { AppLanguage.Hindi,   "यह प्रमाणपत्र निष्ठापूर्वक प्रदान किया जाता है" },
                { AppLanguage.Santali, "ᱱᱚᱣᱟ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱫᱚ ᱢᱟᱹᱱ ᱥᱟᱞᱟᱜ ᱮᱢ ᱦᱩᱭᱩᱜ ᱠᱟᱱᱟ" } } },
            { "certificate.downloadPdf", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Download Official PDF" },
                { AppLanguage.Hindi,   "आधिकारिक PDF डाउनलोड करें" },
                { AppLanguage.Santali, "ᱥᱚᱨᱠᱟᱨᱤ PDF ᱰᱟᱣᱩᱱᱞᱳᱰ ᱢᱮ" } } },
            { "certificate.emptyDesc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Complete the practical AR training and assessment. Once reviewed and approved by a safety officer, your official certificate will appear here." },
                { AppLanguage.Hindi,   "प्रायोगिक AR प्रशिक्षण और मूल्यांकन पूरा करें। सुरक्षा अधिकारी द्वारा समीक्षा और अनुमोदन के बाद, आपका आधिकारिक प्रमाणपत्र यहाँ दिखाई देगा।" },
                { AppLanguage.Santali, "ᱮ.ᱟᱨ. ᱴᱨᱮᱱᱤᱝ ᱟᱨ ᱵᱤᱰᱟᱹᱣ ᱯᱩᱨᱟᱹᱣ ᱢᱮ ᱾ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱟᱹᱢᱟᱹᱞᱤᱭᱟᱹ ᱧᱮᱞ ᱠᱟᱛᱮ ᱮᱢ ᱞᱮᱠᱷᱟᱱ, ᱱᱚᱰᱮ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱧᱮᱞᱚᱜᱼᱟ ᱾" } } },
            { "certificate.emptyTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "No Issued Certificates Yet" },
                { AppLanguage.Hindi,   "अभी तक कोई प्रमाणपत्र जारी नहीं हुआ" },
                { AppLanguage.Santali, "ᱱᱤᱛ ᱫᱷᱟᱹᱵᱤᱡ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱵᱟᱝ ᱮᱢ ᱟᱠᱟᱱᱟ" } } },
            { "certificate.latestBadge", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "LATEST ISSUED CREDENTIAL" },
                { AppLanguage.Hindi,   "नवीनतम जारी प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱱᱟᱣᱟ ᱮᱢ ᱟᱠᱟᱱ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ" } } },
            { "certificate.qrSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scan this QR code using Google Lens or any smartphone camera to view official compliance certificate directly." },
                { AppLanguage.Hindi,   "आधिकारिक प्रमाणपत्र देखने के लिए Google Lens या किसी भी स्मार्टफ़ोन कैमरे से यह क्यूआर कोड स्कैन करें।" },
                { AppLanguage.Santali, "ᱥᱚᱨᱠᱟᱨᱤ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱧᱮᱞ ᱞᱟᱹᱜᱤᱫ ᱱᱚᱣᱟ QR ᱠᱳᱰ ᱥᱠᱮᱱ ᱢᱮ ᱾" } } },
            { "certificate.qrTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scan to Verify Credential" },
                { AppLanguage.Hindi,   "प्रमाणपत्र सत्यापन के लिए स्कैन करें" },
                { AppLanguage.Santali, "ᱯᱚᱨᱚᱠ ᱞᱟᱹᱜᱤᱫ ᱥᱠᱮᱱ ᱢᱮ" } } },
            { "certificate.refreshCerts", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Sync & Refresh Certificates" },
                { AppLanguage.Hindi,   "प्रमाणपत्र सिंक एवं रीफ़्रेश करें" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱥᱤᱝᱠ ᱟᱨ ᱨᱤᱯᱷᱨᱮᱥ" } } },
            { "certificate.returnHome", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Return to Dashboard" },
                { AppLanguage.Hindi,   "डैशबोर्ड पर लौटें" },
                { AppLanguage.Santali, "ᱰᱮᱥᱵᱚᱨᱰ ᱛᱮ ᱨᱩᱣᱟᱹᱲ ᱢᱮ" } } },
            { "certificate.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "My Safety Certificates" },
                { AppLanguage.Hindi,   "मेरे सुरक्षा प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱤᱧᱟᱜ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱠᱚ" } } },
            { "certificate.verifyOnline", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Verify Online" },
                { AppLanguage.Hindi,   "ऑनलाइन सत्यापित करें" },
                { AppLanguage.Santali, "ᱚᱱᱞᱟᱭᱤᱱ ᱨᱮ ᱯᱚᱨᱚᱠ ᱢᱮ" } } },
            { "certificate.viewImage", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Open Certificate Image >" },
                { AppLanguage.Hindi,   "प्रमाणपत्र चित्र देखें >" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱪᱤᱛᱟᱹᱨ ᱧᱮᱞ ᱢᱮ >" } } },
            { "common.back", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Back" },
                { AppLanguage.Hindi,   "वापस" },
                { AppLanguage.Santali, "ᱛᱟᱭᱚᱢ" } } },
            { "common.cancel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Cancel" },
                { AppLanguage.Hindi,   "रद्द करें" },
                { AppLanguage.Santali, "ᱵᱟᱹᱛᱤᱞ" } } },
            { "common.certificates", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificates" },
                { AppLanguage.Hindi,   "प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱠᱚ" } } },
            { "common.close", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Close" },
                { AppLanguage.Hindi,   "बंद करें" },
                { AppLanguage.Santali, "ᱵᱚᱱᱫᱚ" } } },
            { "common.connectionRestored", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Network connection restored." },
                { AppLanguage.Hindi,   "नेटवर्क कनेक्शन वापस आ गया है।" },
                { AppLanguage.Santali, "ᱱᱮᱴᱣᱚᱨᱠ ᱡᱚᱲᱟᱣ ᱫᱚᱦᱲᱟ ᱮᱦᱚᱵ ᱮᱱᱟ ᱾" } } },
            { "common.continue", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Continue" },
                { AppLanguage.Hindi,   "जारी रखें" },
                { AppLanguage.Santali, "ᱞᱟᱦᱟᱜ ᱢᱮ" } } },
            { "common.done", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Done" },
                { AppLanguage.Hindi,   "पूरा हुआ" },
                { AppLanguage.Santali, "ᱦᱩᱭᱮᱱᱟ" } } },
            { "common.error", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Error" },
                { AppLanguage.Hindi,   "त्रुटि" },
                { AppLanguage.Santali, "ᱵᱷᱩᱞ" } } },
            { "common.exit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Exit" },
                { AppLanguage.Hindi,   "बाहर निकलें" },
                { AppLanguage.Santali, "ᱚᱰᱚᱠ" } } },
            { "common.home", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Home" },
                { AppLanguage.Hindi,   "होम" },
                { AppLanguage.Santali, "ᱢᱩᱞ ᱥᱟᱦᱴᱟ" } } },
            { "common.learn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn" },
                { AppLanguage.Hindi,   "सीखें" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ" } } },
            { "common.loading", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Loading..." },
                { AppLanguage.Hindi,   "लोड हो रहा है..." },
                { AppLanguage.Santali, "ᱞᱟᱫᱮᱜ ᱠᱟᱱᱟ..." } } },
            { "common.myProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "My Progress" },
                { AppLanguage.Hindi,   "मेरी प्रगति" },
                { AppLanguage.Santali, "ᱤᱧᱟᱜ ᱞᱟᱦᱟᱱᱛᱤ" } } },
            { "common.networkUnavailable", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Network connection unavailable." },
                { AppLanguage.Hindi,   "नेटवर्क कनेक्शन उपलब्ध नहीं है।" },
                { AppLanguage.Santali, "ᱱᱮᱴᱣᱚᱨᱠ ᱡᱚᱲᱟᱣ ᱵᱟᱹᱱᱩᱜᱼᱟ ᱾" } } },
            { "common.next", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Next" },
                { AppLanguage.Hindi,   "आगे" },
                { AppLanguage.Santali, "ᱞᱟᱦᱟ" } } },
            { "common.noData", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "No data available." },
                { AppLanguage.Hindi,   "कोई डेटा उपलब्ध नहीं है।" },
                { AppLanguage.Santali, "ᱪᱮᱫ ᱰᱟᱴᱟ ᱦᱚᱸ ᱵᱟᱹᱱᱩᱜᱼᱟ ᱾" } } },
            { "common.offlineMode", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Offline Mode" },
                { AppLanguage.Hindi,   "ऑफ़लाइन मोड" },
                { AppLanguage.Santali, "ᱚᱯᱷᱞᱟᱭᱤᱱ ᱢᱚᱰ" } } },
            { "common.ok", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "OK" },
                { AppLanguage.Hindi,   "ठीक है" },
                { AppLanguage.Santali, "ᱴᱷᱤᱠ ᱜᱮᱭᱟ" } } },
            { "common.retry", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Retry" },
                { AppLanguage.Hindi,   "फिर कोशिश करें" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱪᱮᱥᱴᱟ" } } },
            { "common.save", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Save" },
                { AppLanguage.Hindi,   "सहेजें" },
                { AppLanguage.Santali, "ᱥᱟᱧᱪᱟᱣ" } } },
            { "common.somethingWentWrong", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Something went wrong. Please try again." },
                { AppLanguage.Hindi,   "कुछ गड़बड़ हो गई। कृपया फिर कोशिश करें।" },
                { AppLanguage.Santali, "ᱡᱟᱦᱟᱸᱱᱟᱜ ᱵᱷᱩᱞ ᱦᱩᱭᱮᱱᱟ ᱾ ᱫᱚᱦᱲᱟ ᱪᱮᱥᱴᱟᱭ ᱢᱮ ᱾" } } },
            { "common.start", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Start" },
                { AppLanguage.Hindi,   "शुरू करें" },
                { AppLanguage.Santali, "ᱮᱦᱚᱵ" } } },
            { "common.submit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Submit" },
                { AppLanguage.Hindi,   "जमा करें" },
                { AppLanguage.Santali, "ᱡᱚᱢᱟ ᱢᱮ" } } },
            { "common.success", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Success" },
                { AppLanguage.Hindi,   "सफलता" },
                { AppLanguage.Santali, "ᱫᱟᱲᱮ" } } },
            { "common.syncFailed", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Data synchronization failed." },
                { AppLanguage.Hindi,   "डेटा सिंक्रनाइज़ेशन विफल रहा।" },
                { AppLanguage.Santali, "ᱰᱟᱴᱟ ᱥᱤᱝᱠ ᱵᱟᱝ ᱜᱟᱱ ᱞᱮᱱᱟ ᱾" } } },
            { "common.syncSuccess", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Data synchronized successfully." },
                { AppLanguage.Hindi,   "डेटा सफलतापूर्वक सिंक्रनाइज़ हो गया।" },
                { AppLanguage.Santali, "ᱰᱟᱴᱟ ᱥᱤᱝᱠ ᱥᱟᱹᱛ ᱮᱱᱟ ᱾" } } },
            { "common.viewAll", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View All" },
                { AppLanguage.Hindi,   "सभी देखें" },
                { AppLanguage.Santali, "ᱡᱚᱛᱚ ᱧᱮᱞ ᱢᱮ" } } },
            { "common.warning", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Warning" },
                { AppLanguage.Hindi,   "चेतावनी" },
                { AppLanguage.Santali, "ᱦᱩᱥᱤᱭᱟᱹᱨ" } } },
            { "difficulty.advanced", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Advanced Level" },
                { AppLanguage.Hindi,   "उन्नत स्तर" },
                { AppLanguage.Santali, "ᱞᱟᱦᱟᱱᱛᱤ" } } },
            { "difficulty.beginner", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Beginner Level" },
                { AppLanguage.Hindi,   "शुरुआती स्तर" },
                { AppLanguage.Santali, "ᱮᱛᱚᱦᱚᱵᱤᱡ" } } },
            { "difficulty.intermediate", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Intermediate Level" },
                { AppLanguage.Hindi,   "मध्यम स्तर" },
                { AppLanguage.Santali, "ᱛᱟᱞᱟᱢᱟᱞᱟ" } } },
            { "fire.actionHint.step1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tap on Fire" },
                { AppLanguage.Hindi,   "आग पर टैप करें" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱨᱮ ᱚᱛᱟᱭ ᱢᱮ" } } },
            { "fire.actionHint.step2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tap Alarm" },
                { AppLanguage.Hindi,   "अलार्म पर टैप करें" },
                { AppLanguage.Santali, "ᱮᱞᱟᱨᱢ ᱨᱮ ᱚᱛᱟᱭ ᱢᱮ" } } },
            { "fire.actionHint.step3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select CO₂ Extinguisher" },
                { AppLanguage.Hindi,   "CO₂ अग्निशामक चुनें" },
                { AppLanguage.Santali, "CO2 ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "fire.actionHint.step4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Pull Safety Pin" },
                { AppLanguage.Hindi,   "सुरक्षा पिन निकालें" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ ᱢᱮ" } } },
            { "fire.actionHint.step5", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim Horn at Base" },
                { AppLanguage.Hindi,   "आग के आधार पर निशाना लगाएँ" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱵᱩᱴᱟᱹ ᱥᱮᱫ ᱫᱷᱮᱭᱟᱱ ᱢᱮ" } } },
            { "fire.actionHint.step6", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Press & Hold to Spray" },
                { AppLanguage.Hindi,   "स्प्रे करने के लिए दबाकर रखें" },
                { AppLanguage.Santali, "ᱞᱤᱢᱵᱩᱫ ᱠᱟᱛᱮ ᱪᱟᱞᱟᱣ ᱢᱮ" } } },
            { "fire.ar.completionSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Excellent work! Proceed to assessment." },
                { AppLanguage.Hindi,   "शाबाश! मूल्यांकन के लिए आगे बढ़ें।" },
                { AppLanguage.Santali, "ᱟᱹᱰᱤ ᱱᱟᱯᱟᱭ ᱠᱟᱹᱢᱤ ! ᱵᱤᱰᱟᱹᱣ ᱞᱟᱹᱜᱤᱫ ᱞᱟᱦᱟᱜ ᱢᱮ ᱾" } } },
            { "fire.ar.completionTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire Extinguished!" },
                { AppLanguage.Hindi,   "आग बुझ गई!" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱤᱨᱤᱡ ᱮᱱᱟ!" } } },
            { "fire.ar.placementPrompt", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scanning for surface..." },
                { AppLanguage.Hindi,   "सतह खोज रहे हैं..." },
                { AppLanguage.Santali, "ᱚᱛ ᱥᱮᱸᱫᱽᱨᱟᱜ ᱠᱟᱱᱟ..." } } },
            { "fire.ar.readyToSpray", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Ready to Spray" },
                { AppLanguage.Hindi,   "स्प्रे के लिए तैयार" },
                { AppLanguage.Santali, "ᱪᱷᱤᱴᱠᱟᱹᱣ ᱞᱟᱹᱜᱤᱫ ᱥᱟᱯᱲᱟᱣ" } } },
            { "fire.ar.score", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Score:" },
                { AppLanguage.Hindi,   "स्कोर:" },
                { AppLanguage.Santali, "ᱢᱟᱨᱥᱟᱞ:" } } },
            { "fire.ar.sprayPaused", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Off Target (Paused)" },
                { AppLanguage.Hindi,   "लक्ष्य से बाहर (रुका)" },
                { AppLanguage.Santali, "ᱡᱚᱥ ᱠᱷᱚᱱ ᱵᱟᱦᱨᱮ (ᱛᱷᱟᱯᱚᱱ ᱟᱠᱟᱱᱟ)" } } },
            { "fire.ar.spraying", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Spraying..." },
                { AppLanguage.Hindi,   "स्प्रे हो रहा है..." },
                { AppLanguage.Santali, "ᱤᱨᱤᱡ ᱪᱟᱞᱟᱜ ᱠᱟᱱᱟ..." } } },
            { "fire.ar.stepOf", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Step {0} of {1}" },
                { AppLanguage.Hindi,   "चरण {0} / {1}" },
                { AppLanguage.Santali, "ᱪᱟᱸᱜ {0} / {1}" } } },
            { "fire.evac.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Exit Reached — Complete Evacuation" },
                { AppLanguage.Hindi,   "निकास बिंदु पर पहुँचे — निकासी पूर्ण करें" },
                { AppLanguage.Santali, "ᱚᱰᱚᱠ ᱡᱟᱭᱜᱟ ᱨᱮ ᱥᱮᱴᱮᱨ ᱮᱱᱟ — ᱚᱰᱚᱠ ᱯᱩᱨᱟᱹᱣ ᱢᱮ" } } },
            { "fire.evac.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Follow the green EXIT signs to the emergency assembly point. Do not attempt to re-enter." },
                { AppLanguage.Hindi,   "आपातकालीन असेंबली बिंदु तक पहुँचने के लिए हरे निकास संकेतों का पालन करें। दोबारा प्रवेश करने का प्रयास न करें।" },
                { AppLanguage.Santali, "ᱦᱟᱹᱨᱭᱟᱹᱲ EXIT ᱪᱤᱱᱦᱟᱹ ᱯᱟᱸᱡᱟ ᱠᱟᱛᱮ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱡᱟᱭᱜᱟ ᱛᱮ ᱪᱟᱞᱟᱜ ᱢᱮ ᱾ ᱫᱚᱦᱲᱟ ᱵᱚᱞᱚᱱ ᱟᱞᱚᱢ ᱪᱮᱥᱴᱟᱭᱟ ᱾" } } },
            { "fire.evac.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Inform emergency services of fire location, fuel type, and any personnel still inside." },
                { AppLanguage.Hindi,   "आपातकालीन सेवाओं को आग के स्थान, ईंधन के प्रकार और अंदर मौजूद कर्मियों की सूचना दें।" },
                { AppLanguage.Santali, "ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱠᱟᱹᱢᱤᱭᱟᱹ ᱠᱚ ᱥᱮᱸᱜᱮᱞ ᱡᱟᱭᱜᱟ ᱟᱨ ᱵᱷᱤᱛᱨᱤ ᱨᱮ ᱦᱚᱲ ᱢᱮᱱᱟᱜ ᱠᱚ ᱠᱷᱟᱱ ᱠᱷᱚᱵᱚᱨ ᱮᱢᱟᱠᱚ ᱢᱮ ᱾" } } },
            { "fire.evac.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Evacuate the Hazard Zone" },
                { AppLanguage.Hindi,   "खतरे के क्षेत्र से सुरक्षित बाहर निकलें" },
                { AppLanguage.Santali, "ᱵᱚᱛᱚᱨ ᱡᱟᱭᱜᱟ ᱠᱷᱚᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱚᱰᱚᱠ" } } },
            { "fire.feedback.activateAlarmFirst", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Activate the fire alarm before selecting the extinguisher." },
                { AppLanguage.Hindi,   "अग्निशामक चुनने से पहले फायर अलार्म बजाएँ।" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡᱤᱡ ᱵᱟᱪᱷᱟᱣ ᱢᱟᱲᱟᱝ ᱥᱮᱸᱜᱮᱞ ᱟᱞᱟᱨᱢ ᱪᱟᱹᱞᱩᱭ ᱢᱮ ᱾" } } },
            { "fire.feedback.aimAtBase", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim the nozzle at the base of the fire." },
                { AppLanguage.Hindi,   "नोज़ल को आग के आधार की ओर रखें।" },
                { AppLanguage.Santali, "ᱱᱳᱡᱚᱞ ᱫᱚ ᱥᱮᱸᱜᱮᱞ ᱨᱮᱱᱟᱜ ᱵᱩᱰᱟᱹ ᱨᱮ ᱡᱚᱥ ᱢᱮ ᱾" } } },
            { "fire.feedback.contactReset", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Contact lost. Start spraying again." },
                { AppLanguage.Hindi,   "संपर्क टूट गया। स्प्रे फिर से शुरू करें।" },
                { AppLanguage.Santali, "ᱡᱚᱲᱟᱣ ᱛᱚᱯᱟᱜ ᱮᱱᱟ ᱾ ᱫᱚᱦᱲᱟ ᱪᱷᱤᱴᱠᱟᱹᱣ ᱮᱦᱚᱵ ᱢᱮ ᱾" } } },
            { "fire.feedback.contactValid", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Good contact! Keep spraying." },
                { AppLanguage.Hindi,   "सही निशाना! स्प्रे जारी रखें।" },
                { AppLanguage.Santali, "ᱴᱷᱤᱠ ᱡᱚᱲᱟᱣ ! ᱪᱷᱤᱴᱠᱟᱹᱣ ᱪᱟᱞᱩ ᱫᱚᱦᱚᱭ ᱢᱮ ᱾" } } },
            { "fire.feedback.gripActivated", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Grip activated. Keep spraying." },
                { AppLanguage.Hindi,   "हैंडल दब गया। स्प्रे जारी रखें।" },
                { AppLanguage.Santali, "ᱞᱤᱵᱷᱟᱨ ᱪᱟᱹᱞᱩ ᱮᱱᱟ ᱾ ᱪᱷᱤᱴᱠᱟᱹᱣ ᱪᱟᱞᱩ ᱫᱚᱦᱚᱭ ᱢᱮ ᱾" } } },
            { "fire.feedback.identifyHazardFirst", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify the fire hazard first." },
                { AppLanguage.Hindi,   "पहले आग के खतरे की पहचान करें।" },
                { AppLanguage.Santali, "ᱢᱟᱲᱟᱝ ᱥᱮᱸᱜᱮᱞ ᱵᱚᱛᱚᱨ ᱪᱤᱱᱦᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "fire.feedback.prematureSpray", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remove the safety pin before spraying." },
                { AppLanguage.Hindi,   "स्प्रे करने से पहले सुरक्षा पिन निकालें।" },
                { AppLanguage.Santali, "ᱪᱷᱤᱴᱠᱟᱹᱣ ᱢᱟᱲᱟᱝ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱰᱚᱠ ᱢᱮ ᱾" } } },
            { "fire.feedback.removePinBeforeHandle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remove the safety pin before using the handle." },
                { AppLanguage.Hindi,   "लीवर दबाने से पहले सुरक्षा पिन निकालें।" },
                { AppLanguage.Santali, "ᱦᱮᱱᱰᱮᱞ ᱵᱮᱵᱷᱟᱨ ᱢᱟᱲᱟᱝ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱰᱚᱠ ᱢᱮ ᱾" } } },
            { "fire.feedback.removePinFirst", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remove the safety pin first." },
                { AppLanguage.Hindi,   "पहले सुरक्षा पिन निकालें।" },
                { AppLanguage.Santali, "ᱢᱟᱲᱟᱝ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱰᱚᱠ ᱢᱮ ᱾" } } },
            { "fire.feedback.selectExtinguisherFirst", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select the CO₂ extinguisher first." },
                { AppLanguage.Hindi,   "पहले CO₂ अग्निशामक का चयन करें।" },
                { AppLanguage.Santali, "ᱢᱟᱲᱟᱝ CO₂ ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡᱤᱡ ᱵᱟᱪᱷᱟᱣ ᱢᱮ ᱾" } } },
            { "fire.feedback.sprayContactInterrupted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Continuous spray interrupted! Keep aiming at the base." },
                { AppLanguage.Hindi,   "स्प्रे संपर्क टूट गया! आग के आधार पर निशाना बनाए रखें।" },
                { AppLanguage.Santali, "ᱞᱮᱛᱟᱲ ᱪᱷᱤᱴᱠᱟᱹᱣ ᱛᱚᱯᱟᱜ ᱮᱱᱟ ! ᱥᱮᱸᱜᱮᱞ ᱵᱩᱰᱟᱹ ᱨᱮ ᱡᱚᱥ ᱫᱚᱦᱚᱭ ᱢᱮ ᱾" } } },
            { "fire.feedback.sprayInterrupted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Spray interrupted. Re-aim and spray." },
                { AppLanguage.Hindi,   "स्प्रे रुक गया। फिर से निशाना लगाएँ और स्प्रे करें।" },
                { AppLanguage.Santali, "ᱪᱷᱤᱴᱠᱟᱹᱣ ᱛᱚᱯᱟᱜ ᱮᱱᱟ ᱾ ᱫᱚᱦᱲᱟ ᱡᱚᱥ ᱠᱟᱛᱮ ᱪᱷᱤᱴᱠᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "fire.guidance.completion.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Well done. The fire has been safely extinguished." },
                { AppLanguage.Hindi,   "शाबाश! आग सुरक्षित रूप से बुझा दी गई है।" },
                { AppLanguage.Santali, "ᱟᱹᱰᱤ ᱵᱷᱟᱹᱜᱤ ᱾ ᱥᱮᱸᱜᱮᱞ ᱫᱚ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱛᱮ ᱤᱬᱤᱡ ᱮᱱᱟ ᱾" } } },
            { "fire.guidance.spray.progress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "{0:F1} / {1:F1} s" },
                { AppLanguage.Hindi,   "{0:F1} / {1:F1} से." },
                { AppLanguage.Santali, "{0:F1} / {1:F1} ᱴᱤᱯᱤᱡ" } } },
            { "fire.guidance.step1.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Find the highlighted electrical fire and tap it." },
                { AppLanguage.Hindi,   "अपने आसपास देखें और विद्युत आग को पहचानें। आग दिखाई देने पर उस पर टैप करें।" },
                { AppLanguage.Santali, "ᱪᱤᱱᱦᱟᱹᱣ ᱟᱠᱟᱱ ᱵᱤᱡᱽᱞᱤ ᱥᱮᱸᱜᱮᱞ ᱯᱟᱱᱛᱮ ᱠᱟᱛᱮ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.guidance.step2.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Find the fire alarm and tap it to alert everyone." },
                { AppLanguage.Hindi,   "दीवार पर लगे लाल फायर अलार्म स्टेशन का पता लगाएँ और सभी को सूचित करने के लिए इसे टैप करें।" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱟᱞᱟᱨᱢ ᱯᱟᱱᱛᱮ ᱠᱟᱛᱮ ᱡᱚᱛᱚ ᱦᱚᱲ ᱵᱟᱰᱟᱭ ᱦᱚᱪᱚ ᱞᱟᱹᱜᱤᱫ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.guidance.step3.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tap the highlighted CO\u2082 extinguisher to pick it up." },
                { AppLanguage.Hindi,   "जलते हुए उपकरण की जाँच करें और विद्युत आग के लिए सही CO₂ अग्निशामक चुनें।" },
                { AppLanguage.Santali, "ᱪᱤᱱᱦᱟᱹᱣ ᱟᱠᱟᱱ CO₂ ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡᱤᱡ ᱛᱩᱞ ᱞᱟᱹᱜᱤᱫ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.guidance.step4.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tap and pull the safety pin out before using the handle." },
                { AppLanguage.Hindi,   "हैंडल अनलॉक करने के लिए सुरक्षा पिन निकालें।" },
                { AppLanguage.Santali, "ᱦᱮᱱᱰᱮᱞ ᱵᱮᱵᱷᱟᱨ ᱢᱟᱲᱟᱝ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ ᱢᱮ ᱾" } } },
            { "fire.guidance.step5.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim the nozzle at the base of the fire, not the flames." },
                { AppLanguage.Hindi,   "इंसुलेटेड हॉर्न को पकड़ें। लपटों पर नहीं, सीधे आग के आधार पर निशाना लगाएँ।" },
                { AppLanguage.Santali, "ᱱᱳᱡᱚᱞ ᱫᱚ ᱥᱮᱸᱜᱮᱞ ᱞᱟᱴᱷᱟ ᱨᱮ ᱵᱟᱝ, ᱵᱩᱰᱟᱹ ᱨᱮ ᱡᱚᱥ ᱢᱮ ᱾" } } },
            { "fire.guidance.step6.body", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Keep the nozzle at the base and spray continuously." },
                { AppLanguage.Hindi,   "हैंडल दबाकर रखें और आग बुझने तक उसके आधार पर दायें-बायें स्प्रे करें।" },
                { AppLanguage.Santali, "ᱱᱳᱡᱚᱞ ᱫᱚ ᱵᱩᱰᱟᱹ ᱨᱮ ᱫᱚᱦᱚ ᱠᱟᱛᱮ ᱞᱮᱛᱟᱲ ᱪᱷᱤᱴᱠᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "fire.intro.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Start AR Training" },
                { AppLanguage.Hindi,   "AR प्रशिक्षण शुरू करें" },
                { AppLanguage.Santali, "ᱮ.ᱟᱨ. ᱥᱮᱪᱮᱫ ᱮᱦᱚᱵ ᱢᱮ" } } },
            { "fire.intro.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "In this scenario, an electrical equipment fire breaks out in a mining facility.\nFollow standard operating procedures (SOP) to safely respond and evacuate." },
                { AppLanguage.Hindi,   "इस परिदृश्य में, खनन परिसर के विद्युत उपकरण में आग लग जाती है।\nसुरक्षित बचाव और निकासी के लिए मानक संचालन प्रक्रिया (SOP) का पालन करें।" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱤᱨᱤᱡ ᱨᱮᱱᱟᱜ PASS ᱱᱤᱭᱟᱹᱢ ᱥᱮᱪᱮᱫ ᱢᱮ: ᱚᱨ, ᱫᱷᱮᱭᱟᱱ, ᱞᱤᱢᱵᱩᱫ, ᱟᱨ ᱟᱹᱪᱩᱨ ᱾" } } },
            { "fire.intro.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scan the floor and tap the reticle to anchor the 3D training scenario." },
                { AppLanguage.Hindi,   "फर्श को स्कैन करें और 3D प्रशिक्षण परिदृश्य स्थापित करने के लिए रेटिकल पर टैप करें।" },
                { AppLanguage.Santali, "᱓ᱰᱤ ᱥᱮᱪᱮᱫ ᱯᱚᱨᱚᱠ ᱛᱷᱟᱯᱚᱱ ᱞᱟᱹᱜᱤᱫ ᱚᱛ ᱥᱠᱮᱱ ᱢᱮ ᱟᱨ ᱨᱮᱴᱤᱠᱟᱞ ᱨᱮ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.intro.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Industrial Fire Response Training" },
                { AppLanguage.Hindi,   "औद्योगिक आग से निपटने का प्रशिक्षण" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱮᱪᱮᱫ" } } },
            { "fire.scan.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Simulate Placement" },
                { AppLanguage.Hindi,   "यहाँ रखें" },
                { AppLanguage.Santali, "ᱛᱷᱟᱯᱚᱱ ᱯᱚᱨᱚᱠ ᱢᱮ" } } },
            { "fire.scan.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Move your phone slowly to scan the ground.\nWhen the placement reticle appears, tap anywhere to place the industrial scenario." },
                { AppLanguage.Hindi,   "सतह स्कैन करने के लिए अपने फ़ोन को धीरे-धीरे घुमाएँ।\nनिशान दिखाई देने पर प्रशिक्षण शुरू करने के लिए टैप करें।" },
                { AppLanguage.Santali, "ᱚᱛ ᱥᱠᱮᱱ ᱞᱟᱹᱜᱤᱫ ᱟᱢᱟᱜ ᱯᱷᱳᱱ ᱵᱟᱹᱭ-ᱵᱟᱹᱭ ᱛᱮ ᱟᱹᱪᱩᱨ ᱢᱮ ᱾\nᱡᱚᱠᱷᱚᱱ ᱨᱮᱴᱤᱠᱟᱞ ᱧᱮᱞᱚᱜᱼᱟ, ᱯᱚᱨᱚᱠ ᱛᱷᱟᱯᱚᱱ ᱞᱟᱹᱜᱤᱫ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.scan.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Ensure adequate ambient lighting for optical feature tracking." },
                { AppLanguage.Hindi,   "ऑप्टिकल फीचर ट्रैकिंग के लिए पर्याप्त प्रकाश सुनिश्चित करें।" },
                { AppLanguage.Santali, "ᱢᱮᱫ ᱪᱤᱱᱦᱟᱹᱣ ᱞᱟᱹᱜᱤᱫ ᱴᱷᱤᱠ ᱢᱟᱨᱥᱟᱞ ᱢᱮᱱᱟᱜᱼᱟ ᱥᱮ ᱵᱟᱝ ᱧᱮᱞ ᱢᱮ ᱾" } } },
            { "fire.scan.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Flat Surface" },
                { AppLanguage.Hindi,   "समतल सतह चुनें" },
                { AppLanguage.Santali, "ᱥᱚᱢᱟᱱ ᱚᱛ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "fire.sop.step1.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "I Have Identified Fire Source" },
                { AppLanguage.Hindi,   "मैंने आग पहचान ली है" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱟᱨ ᱚᱰᱚᱠ ᱰᱟᱦᱟᱨ ᱧᱮᱞ ᱢᱮ" } } },
            { "fire.sop.step1.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Look around your surroundings to locate the electrical fire. Aim your camera at the flames or tap directly on the fire in AR." },
                { AppLanguage.Hindi,   "अपने आसपास देखें और विद्युत आग को पहचानें। आग दिखाई देने पर उस पर टैप करें।" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱥᱮᱸᱜᱮᱞ ᱯᱟᱱᱛᱮ ᱧᱟᱢ ᱞᱟᱹᱜᱤᱫ ᱟᱢ ᱟᱰᱮᱯᱟᱥᱮ ᱧᱮᱞ ᱢᱮ ᱾ ᱠᱮᱢᱮᱨᱟ ᱥᱮᱸᱜᱮᱞ ᱥᱮᱫ ᱟᱹᱪᱩᱨ ᱢᱮ ᱟᱨ ᱮ.ᱟᱨ. ᱨᱮ ᱥᱮᱸᱜᱮᱞ ᱨᱮ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.sop.step1.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Look for sparks, dark smoke, and electrical panel indicators." },
                { AppLanguage.Hindi,   "चिंगारी, काले धुएँ और विद्युत पैनल के संकेतों की जाँच करें।" },
                { AppLanguage.Santali, "ᱪᱤᱱᱜᱟᱹᱨᱤ, ᱦᱮᱸᱫᱮ ᱫᱷᱩᱶᱟᱹ ᱟᱨ ᱵᱤᱡᱽᱞᱤ ᱯᱮᱱᱮᱞ ᱪᱤᱱᱦᱟᱹ ᱠᱚ ᱧᱮᱞ ᱢᱮ ᱾" } } },
            { "fire.sop.step1.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify Hazard" },
                { AppLanguage.Hindi,   "आग पहचानें" },
                { AppLanguage.Santali, "ᱦᱟᱞᱚᱛ ᱧᱮᱞ ᱢᱮ" } } },
            { "fire.sop.step2.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Activate Fire Alarm" },
                { AppLanguage.Hindi,   "अलार्म सक्रिय करें" },
                { AppLanguage.Santali, "ᱥᱩᱨ ᱨᱮᱱᱟᱜ ᱮᱞᱟᱨᱢ ᱚᱛᱟᱭ ᱢᱮ" } } },
            { "fire.sop.step2.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Locate the red fire alarm pull station on the wall and tap it to alert all personnel in the facility." },
                { AppLanguage.Hindi,   "दीवार पर लगे लाल फायर अलार्म स्टेशन का पता लगाएँ और सभी को सूचित करने के लिए इसे टैप करें।" },
                { AppLanguage.Santali, "ᱵᱷᱤᱛ ᱨᱮ ᱟᱨᱟᱜ ᱥᱮᱸᱜᱮᱞ ᱮᱞᱟᱨᱢ ᱥᱴᱮᱥᱚᱱ ᱯᱟᱱᱛᱮ ᱧᱟᱢ ᱢᱮ ᱟᱨ ᱠᱟᱹᱢᱤᱭᱟᱹ ᱠᱚ ᱦᱩᱥᱤᱭᱟᱹᱨ ᱞᱟᱹᱜᱤᱫ ᱚᱛᱟᱭ ᱢᱮ ᱾" } } },
            { "fire.sop.step2.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Always alert others before attempting to fight a fire alone. Never skip the alarm." },
                { AppLanguage.Hindi,   "अकेले आग बुझाने का प्रयास करने से पहले हमेशा दूसरों को सतर्क करें। अलार्म को कभी न छोड़ें।" },
                { AppLanguage.Santali, "ᱮᱠᱞᱟ ᱥᱮᱸᱜᱮᱞ ᱤᱨᱤᱡ ᱞᱟᱦᱟᱨᱮ ᱮᱴᱟᱜ ᱦᱚᱲ ᱦᱩᱥᱤᱭᱟᱹᱨ ᱠᱚ ᱢᱮ ᱾ ᱮᱞᱟᱨᱢ ᱚᱛᱟ ᱟᱞᱚᱢ ᱵᱟᱹᱜᱤᱭᱟ ᱾" } } },
            { "fire.sop.step2.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Activate Fire Alarm" },
                { AppLanguage.Hindi,   "फायर अलार्म सक्रिय करें" },
                { AppLanguage.Santali, "ᱮᱞᱟᱨᱢ ᱚᱛᱟᱭ ᱢᱮ" } } },
            { "fire.sop.step3.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Equip CO₂ Extinguisher" },
                { AppLanguage.Hindi,   "CO₂ अग्निशामक चुनें" },
                { AppLanguage.Santali, "ᱴᱷᱤᱠ ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "fire.sop.step3.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Examine the burning equipment. Tap on the CO2 Extinguisher (Black band) in your surroundings to equip it." },
                { AppLanguage.Hindi,   "जलते हुए उपकरण की जाँच करें और विद्युत आग के लिए सही CO₂ अग्निशामक चुनें।" },
                { AppLanguage.Santali, "ᱡᱩᱞᱩᱜ ᱠᱟᱱ ᱥᱟᱢᱟᱱ ᱧᱮᱞ ᱢᱮ ᱾ ᱵᱤᱡᱽᱞᱤ ᱥᱮᱸᱜᱮᱞ ᱞᱟᱹᱜᱤᱫ CO2 ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ (ᱦᱮᱸᱫᱮ ᱯᱟᱹᱴᱤ) ᱵᱟᱪᱷᱟᱣ ᱢᱮ ᱾" } } },
            { "fire.sop.step3.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "DANGER: Never use Water or Foam on live electrical panels! Electrocution hazard." },
                { AppLanguage.Hindi,   "खतरा: चालू विद्युत पैनलों पर कभी भी पानी या फोम का उपयोग न करें! करंट लगने का गंभीर जोखिम है।" },
                { AppLanguage.Santali, "ᱵᱚᱛᱚᱨ: ᱵᱤᱡᱽᱞᱤ ᱥᱟᱢᱟᱱ ᱨᱮ ᱫᱟᱜ ᱟᱨ ᱯᱷᱳᱢ ᱟᱞᱚᱢ ᱫᱩᱞᱟ! ᱠᱟᱨᱮᱱᱴ ᱞᱟᱜᱟᱣ ᱨᱮᱱᱟᱜ ᱵᱚᱛᱚᱨ ᱢᱮᱱᱟᱜᱼᱟ ᱾" } } },
            { "fire.sop.step3.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select CO₂ Extinguisher" },
                { AppLanguage.Hindi,   "सही अग्निशामक यंत्र चुनें" },
                { AppLanguage.Santali, "ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "fire.sop.step4.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Pull Safety Pin" },
                { AppLanguage.Hindi,   "सुरक्षा पिन निकालें" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ ᱢᱮ" } } },
            { "fire.sop.step4.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tap the safety pin on the extinguisher handle to break the tamper seal and unlock the lever." },
                { AppLanguage.Hindi,   "हैंडल अनलॉक करने के लिए सुरक्षा पिन निकालें।" },
                { AppLanguage.Santali, "ᱥᱤᱞ ᱨᱟᱹᱯᱩᱫ ᱞᱟᱹᱜᱤᱫ ᱟᱨ ᱞᱤᱵᱷᱟᱨ ᱠᱷᱩᱞᱟᱹ ᱞᱟᱹᱜᱤᱫ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱨ ᱚᱰᱚᱠ ᱢᱮ ᱾" } } },
            { "fire.sop.step4.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Twist slightly and pull firmly. Do not squeeze the lever while pulling." },
                { AppLanguage.Hindi,   "पिन को थोड़ा घुमाएँ और मजबूती से बाहर खींचें। खींचते समय लीवर को न दबाएँ।" },
                { AppLanguage.Santali, "ᱯᱤᱱ ᱱᱟᱥᱮ ᱟᱹᱪᱩᱨ ᱠᱟᱛᱮ ᱠᱮᱴᱮᱡ ᱛᱮ ᱚᱨ ᱢᱮ ᱾ ᱚᱨ ᱚᱠᱛᱚ ᱞᱤᱵᱷᱟᱨ ᱟᱞᱚᱢ ᱞᱤᱢᱵᱩᱫᱼᱟ ᱾" } } },
            { "fire.sop.step4.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remove Safety Pin" },
                { AppLanguage.Hindi,   "सुरक्षा पिन निकालें" },
                { AppLanguage.Santali, "ᱯᱤᱱ ᱚᱨ ᱢᱮ (P)" } } },
            { "fire.sop.step5.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim Horn at Base" },
                { AppLanguage.Hindi,   "आग के आधार पर निशाना लगाएँ" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱵᱩᱴᱟᱹ ᱥᱮᱫ ᱫᱷᱮᱭᱟᱱ ᱢᱮ" } } },
            { "fire.sop.step5.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hold the insulated discharge horn. Aim directly at the fuel base of the fire, not at the high flames." },
                { AppLanguage.Hindi,   "इंसुलेटेड हॉर्न को पकड़ें। लपटों पर नहीं, सीधे आग के आधार पर निशाना लगाएँ।" },
                { AppLanguage.Santali, "ᱦᱚᱨᱱ ᱥᱟᱵ ᱢᱮ ᱾ ᱪᱮᱛᱟᱱ ᱞᱟᱯᱟᱴ ᱵᱟᱝ ᱠᱟᱛᱮ ᱥᱮᱸᱜᱮᱞ ᱨᱮᱱᱟᱜ ᱵᱩᱴᱟᱹ (ᱤᱸᱫᱷᱚᱱ) ᱥᱮᱫ ᱫᱷᱮᱭᱟᱱ ᱢᱮ ᱾" } } },
            { "fire.sop.step5.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aiming at the flames allows the fire to continue feeding from the combustible base." },
                { AppLanguage.Hindi,   "लपटों पर निशाना साधने से आग आधार से सुलगती रहती है।" },
                { AppLanguage.Santali, "ᱞᱟᱯᱟᱴ ᱥᱮᱫ ᱫᱷᱮᱭᱟᱱ ᱞᱮᱠᱷᱟᱱ ᱵᱩᱴᱟᱹ ᱠᱷᱚᱱ ᱥᱮᱸᱜᱮᱞ ᱡᱩᱞ ᱛᱮᱜᱮ ᱛᱟᱦᱮᱸᱱᱟ ᱾" } } },
            { "fire.sop.step5.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim at Base of Fire" },
                { AppLanguage.Hindi,   "आग के आधार पर निशाना लगाएँ" },
                { AppLanguage.Santali, "ᱫᱷᱮᱭᱟᱱ ᱢᱮ (A)" } } },
            { "fire.sop.step6.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Press Handle & Spray" },
                { AppLanguage.Hindi,   "हैंडल दबाकर स्प्रे करें" },
                { AppLanguage.Santali, "ᱞᱤᱢᱵᱩᱫ ᱠᱟᱛᱮ ᱪᱟᱞᱟᱣ ᱢᱮ" } } },
            { "fire.sop.step6.actionStop", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Release Handle (Stop Spray)" },
                { AppLanguage.Hindi,   "हैंडल छोड़ें (स्प्रे रोकें)" },
                { AppLanguage.Santali, "ᱤᱨᱤᱡ ᱦᱩᱭᱮᱱᱟ" } } },
            { "fire.sop.step6.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Squeeze the operating lever or tap the button below to discharge spray. Sweep side-to-side across the fuel base until the fire is completely out." },
                { AppLanguage.Hindi,   "हैंडल दबाकर रखें और आग बुझने तक उसके आधार पर दायें-बायें स्प्रे करें।" },
                { AppLanguage.Santali, "ᱞᱤᱵᱷᱟᱨ ᱞᱤᱢᱵᱩᱫ ᱢᱮ ᱟᱨ ᱞᱟᱛᱟᱨ ᱵᱟᱴᱚᱱ ᱚᱛᱟᱭ ᱢᱮ ᱾ ᱥᱮᱸᱜᱮᱞ ᱵᱟᱝ ᱤᱨᱤᱡᱚᱜ ᱫᱷᱟᱹᱵᱤᱡ ᱵᱩᱴᱟᱹ ᱨᱮ ᱱᱚᱣᱟ ᱠᱷᱚᱱ ᱦᱟᱱᱛᱮ ᱟᱹᱪᱩᱨ ᱠᱟᱛᱮ ᱤᱨᱤᱡ ᱢᱮ ᱾" } } },
            { "fire.sop.step6.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Maintain continuous discharge for 10 seconds until all flames and smoke cease." },
                { AppLanguage.Hindi,   "आग और धुआँ पूरी तरह समाप्त होने तक लगातार 10 सेकंड स्प्रे करते रहें।" },
                { AppLanguage.Santali, "ᱞᱟᱯᱟᱴ ᱟᱨ ᱫᱷᱩᱶᱟᱹ ᱵᱟᱝ ᱪᱟᱵᱟᱜ ᱫᱷᱟᱹᱵᱤᱡ ᱑᱐ ᱥᱮᱠᱮᱱᱰ ᱞᱮᱛᱟᱲ ᱤᱨᱤᱡ ᱪᱟᱞᱟᱣ ᱢᱮ ᱾" } } },
            { "fire.sop.step6.sprayingDesc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Spraying active! Sweep side-to-side across the fuel base. Keep particles directly on the fire." },
                { AppLanguage.Hindi,   "स्प्रे जारी है! आग के आधार पर दायें-बायें स्वीप करें। स्प्रे को सीधे आग पर रखें।" },
                { AppLanguage.Santali, "ᱤᱨᱤᱡ ᱪᱟᱞᱟᱜ ᱠᱟᱱᱟ! ᱥᱮᱸᱜᱮᱞ ᱵᱩᱴᱟᱹ ᱨᱮ ᱱᱚᱣᱟ ᱠᱷᱚᱱ ᱦᱟᱱᱛᱮ ᱟᱹᱪᱩᱨ ᱢᱮ ᱾ ᱤᱨᱤᱡ ᱫᱟᱜ ᱥᱮᱸᱜᱮᱞ ᱨᱮ ᱫᱚᱦᱚᱭ ᱢᱮ ᱾" } } },
            { "fire.sop.step6.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Extinguish the Fire (PASS)" },
                { AppLanguage.Hindi,   "हैंडल दबाकर स्प्रे करें" },
                { AppLanguage.Santali, "ᱞᱤᱢᱵᱩᱫ ᱟᱨ ᱟᱹᱪᱩᱨ (S-S)" } } },
            { "fire.sop.step7.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Proceed to Emergency Exit" },
                { AppLanguage.Hindi,   "आपातकालीन निकास की ओर बढ़ें" },
                { AppLanguage.Santali, "ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱚᱰᱚᱠ ᱰᱟᱦᱟᱨ ᱛᱮ ᱞᱟᱦᱟᱜ ᱢᱮ" } } },
            { "fire.sop.step7.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "The fire is suppressed. Back away slowly while keeping visual contact. Follow the emergency EXIT signs to the assembly point." },
                { AppLanguage.Hindi,   "आग बुझा दी गई है। निरंतर नजर रखते हुए धीरे-धीरे पीछे हटें और आपातकालीन निकास संकेतों का पालन करते हुए सुरक्षित स्थान पर जाएँ।" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱤᱨᱤᱡ ᱮᱱᱟ ᱾ ᱥᱮᱸᱜᱮᱞ ᱧᱮᱞ ᱛᱩᱞᱩᱡ ᱵᱟᱹᱭ-ᱵᱟᱹᱭ ᱛᱮ ᱛᱟᱭᱚᱢᱚᱜ ᱢᱮ ᱟᱨ ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱚᱰᱚᱠ ᱰᱟᱦᱟᱨ ᱛᱮ ᱥᱟᱺᱜᱤᱧ ᱡᱟᱭᱜᱟ ᱛᱮ ᱪᱟᱞᱟᱜ ᱢᱮ ᱾" } } },
            { "fire.sop.step7.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Never turn your back on a suppressed fire due to re-ignition risk." },
                { AppLanguage.Hindi,   "पुनः आग भड़कने के जोखिम के कारण कभी भी बुझी हुई आग की ओर पीठ न करें।" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱡᱩᱞᱩᱜ ᱵᱚᱛᱚᱨ ᱠᱷᱟᱹᱛᱤᱨ ᱤᱨᱤᱡ ᱟᱠᱟᱱ ᱥᱮᱸᱜᱮᱞ ᱥᱮᱫ ᱛᱟᱭᱚᱢ ᱟᱞᱚᱢ ᱠᱚᱭᱚᱜᱼᱟ ᱾" } } },
            { "fire.sop.step7.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safe Evacuation" },
                { AppLanguage.Hindi,   "सुरक्षित निकासी" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱚᱰᱚᱠ" } } },
            { "fire.timeout.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Proceed to Emergency Exit" },
                { AppLanguage.Hindi,   "आपातकालीन निकास की ओर बढ़ें" },
                { AppLanguage.Santali, "ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱚᱰᱚᱠ ᱰᱟᱦᱟᱨ ᱛᱮ ᱞᱟᱦᱟᱜ ᱢᱮ" } } },
            { "fire.timeout.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "The 7-minute time limit has been reached. The fire was not suppressed in time. You must now evacuate via the emergency exit." },
                { AppLanguage.Hindi,   "निर्धारित समय सीमा समाप्त हो गई है। आग समय पर नहीं बुझाई जा सकी। अब आपको आपातकालीन निकास से सुरक्षित बाहर निकलना होगा।" },
                { AppLanguage.Santali, "᱗ ᱢᱤᱱᱤᱴ ᱚᱠᱛᱚ ᱪᱟᱵᱟ ᱮᱱᱟ ᱾ ᱥᱮᱸᱜᱮᱞ ᱚᱠᱛᱚ ᱨᱮ ᱵᱟᱝ ᱤᱨᱤᱡ ᱞᱮᱱᱟ ᱾ ᱱᱤᱛ ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱰᱟᱦᱟᱨ ᱛᱮ ᱚᱰᱚᱠᱚᱜ ᱢᱮ ᱾" } } },
            { "fire.timeout.hint", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safety first: Never remain in a hazard zone once the emergency timeout is reached." },
                { AppLanguage.Hindi,   "सुरक्षा सर्वोपरि: आपातकालीन समय समाप्त होने पर खतरे के क्षेत्र में कभी न रहें।" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱩᱭᱞᱩ: ᱚᱠᱛᱚ ᱪᱟᱵᱟ ᱞᱮᱱᱠᱷᱟᱱ ᱵᱚᱛᱚᱨ ᱡᱟᱭᱜᱟ ᱨᱮ ᱟᱞᱚᱢ ᱛᱟᱦᱮᱸᱱᱟ ᱾" } } },
            { "fire.timeout.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Time Expired" },
                { AppLanguage.Hindi,   "प्रशिक्षण समय सीमा समाप्त" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱚᱠᱛᱚ ᱪᱟᱵᱟ ᱮᱱᱟ" } } },
            { "fire.toast.aim.sub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Ready for sweep discharge." },
                { AppLanguage.Hindi,   "स्प्रे और स्वीप के लिए तैयार।" },
                { AppLanguage.Santali, "ᱤᱨᱤᱡ ᱞᱟᱹᱜᱤᱫ ᱥᱟᱯᱲᱟᱣ ᱮᱱᱟ ᱾" } } },
            { "fire.toast.aim.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Nozzle Aimed at Base!" },
                { AppLanguage.Hindi,   "आधार पर निशाना लगाया गया!" },
                { AppLanguage.Santali, "ᱵᱩᱴᱟᱹ ᱥᱮᱫ ᱫᱷᱮᱭᱟᱱ ᱮᱱᱟ!" } } },
            { "fire.toast.alarm.sub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Facility evacuation warning issued." },
                { AppLanguage.Hindi,   "परिसर खाली करने की चेतावनी जारी की गई।" },
                { AppLanguage.Santali, "ᱡᱟᱭᱜᱟ ᱠᱷᱟᱹᱞᱤ ᱞᱟᱹᱜᱤᱫ ᱦᱩᱥᱤᱭᱟᱹᱨ ᱠᱷᱚᱵᱚᱨ ᱮᱢ ᱮᱱᱟ ᱾" } } },
            { "fire.toast.alarm.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire Alarm Activated!" },
                { AppLanguage.Hindi,   "अलार्म सक्रिय हुआ!" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱮᱞᱟᱨᱢ ᱪᱟᱞᱟᱣ ᱮᱱᱟ!" } } },
            { "fire.toast.ext.sub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Prepare extinguisher for operation." },
                { AppLanguage.Hindi,   "अग्निशामक को संचालन के लिए तैयार करें।" },
                { AppLanguage.Santali, "ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱪᱟᱞᱟᱣ ᱞᱟᱹᱜᱤᱫ ᱥᱟᱯᱲᱟᱣ ᱢᱮ ᱾" } } },
            { "fire.toast.ext.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "CO₂ Extinguisher Equipped!" },
                { AppLanguage.Hindi,   "CO₂ अग्निशामक चयनित!" },
                { AppLanguage.Santali, "CO2 ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱵᱟᱪᱷᱟᱣ ᱮᱱᱟ!" } } },
            { "fire.toast.extinguished.sub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Proceed to emergency exit safely." },
                { AppLanguage.Hindi,   "अब आपातकालीन निकास की ओर सुरक्षित बढ़ें।" },
                { AppLanguage.Santali, "ᱱᱤᱛ ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱚᱰᱚᱠ ᱰᱟᱦᱟᱨ ᱛᱮ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱛᱮ ᱞᱟᱦᱟᱜ ᱢᱮ ᱾" } } },
            { "fire.toast.extinguished.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire Extinguished!" },
                { AppLanguage.Hindi,   "आग पूरी तरह बुझ गई!" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱤᱨᱤᱡ ᱮᱱᱟ!" } } },
            { "fire.toast.hazard.sub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Activate the fire alarm immediately!" },
                { AppLanguage.Hindi,   "तुरंत फायर अलार्म सक्रिय करें!" },
                { AppLanguage.Santali, "ᱞᱚᱜᱚᱱ ᱥᱮᱸᱜᱮᱞ ᱮᱞᱟᱨᱢ ᱚᱛᱟᱭ ᱢᱮ!" } } },
            { "fire.toast.hazard.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hazard Identified!" },
                { AppLanguage.Hindi,   "खतरे की पहचान की गई!" },
                { AppLanguage.Santali, "ᱵᱚᱛᱚᱨ ᱪᱤᱱᱦᱟᱹᱣ ᱮᱱᱟ!" } } },
            { "fire.toast.pin.sub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Handle unlocked. Extinguisher is armed and ready." },
                { AppLanguage.Hindi,   "लीवर अनलॉक हुआ। अग्निशामक उपयोग के लिए तैयार है।" },
                { AppLanguage.Santali, "ᱞᱤᱵᱷᱟᱨ ᱠᱷᱩᱞᱟᱹ ᱮᱱᱟ ᱾ ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱥᱟᱯᱲᱟᱣ ᱢᱮᱱᱟᱜᱼᱟ ᱾" } } },
            { "fire.toast.pin.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safety Pin Removed!" },
                { AppLanguage.Hindi,   "सुरक्षा पिन निकाली गई!" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱰᱚᱠ ᱮᱱᱟ!" } } },
            { "fire.voice.comingSoon", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Voice guidance coming soon" },
                { AppLanguage.Hindi,   "वॉयस गाइडेंस जल्द आएगी" },
                { AppLanguage.Santali, "ᱟᱲᱟᱝ ᱫᱤᱥᱟᱹ-ᱩᱫᱩᱜ ᱞᱚᱜᱚᱱ ᱜᱮ ᱦᱤᱡᱩᱜ ᱠᱟᱱᱟ" } } },
            { "fire.voice.listenBtn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Listen" },
                { AppLanguage.Hindi,   "सुनें" },
                { AppLanguage.Santali, "ᱟᱧᱡᱚᱢ ᱢᱮ" } } },
            { "fire.voice.santaliPending", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Santali voice guidance pending recording" },
                { AppLanguage.Hindi,   "संथाली वॉयस गाइडेंस रिकॉर्डिंग लंबित है" },
                { AppLanguage.Santali, "ᱥᱟᱱᱛᱟᱲᱤ ᱟᱲᱟᱝ ᱫᱤᱥᱟᱹ-ᱩᱫᱩᱜ ᱨᱮᱠᱳᱨᱰᱤᱝ ᱵᱟᱹᱠᱤ ᱢᱮᱱᱟᱜᱼᱟ" } } },
            { "fire.voice.speaking", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Speaking..." },
                { AppLanguage.Hindi,   "बोल रहे हैं..." },
                { AppLanguage.Santali, "ᱨᱚᱲ ᱠᱟᱱᱟᱭ..." } } },
            { "fire.voice.stopBtn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Stop" },
                { AppLanguage.Hindi,   "रोकें" },
                { AppLanguage.Santali, "ᱛᱷᱟᱢᱟᱠ" } } },
            { "home.comingSoon", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Coming Soon" },
                { AppLanguage.Hindi,   "शीघ्र उपलब्ध होगा" },
                { AppLanguage.Santali, "ᱞᱚᱜᱚᱱ ᱜᱮ ᱦᱤᱡᱩᱜ ᱠᱟᱱᱟ" } } },
            { "home.defaultRole", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mine Safety Trainee" },
                { AppLanguage.Hindi,   "खान कार्यकर्ता" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱮᱪᱮᱫᱤᱭᱟᱹ" } } },
            { "home.fire.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Electrical Fire, Extinguisher & Evacuation SOP" },
                { AppLanguage.Hindi,   "खतरों की पहचान, अग्निशामक यंत्र का उपयोग और सुरक्षित निकासी" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱥᱮᱸᱜᱮᱞ, ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱟᱨ ᱚᱰᱚᱠ SOP" } } },
            { "home.fire.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire & Explosion Response" },
                { AppLanguage.Hindi,   "आग एवं विस्फोट से निपटने की प्रक्रिया" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱟᱨ ᱵᱷᱚᱢ ᱯᱚᱨᱚᱠ" } } },
            { "home.gas.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hazardous Gas Detection & PPE Protocol" },
                { AppLanguage.Hindi,   "खतरनाक गैसों की पहचान और PPE का उपयोग" },
                { AppLanguage.Santali, "ᱵᱤᱥ ᱜᱮᱥ ᱯᱟᱱᱛᱮ ᱟᱨ PPE ᱱᱤᱭᱟᱹᱢ" } } },
            { "home.gas.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Gas Leak & Confined Space" },
                { AppLanguage.Hindi,   "गैस रिसाव एवं सीमित स्थान" },
                { AppLanguage.Santali, "ᱜᱮᱥ ᱞᱤᱠ ᱟᱨ ᱥᱟᱸᱜᱷᱟᱨ ᱡᱟᱭᱜᱟ" } } },
            { "home.greeting", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Welcome," },
                { AppLanguage.Hindi,   "नमस्ते," },
                { AppLanguage.Santali, "ᱡᱚᱦᱟᱨ, {0}" } } },
            { "home.heroTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker Safety, National Prosperity" },
                { AppLanguage.Hindi,   "श्रमिक सुरक्षा, राष्ट्र समृद्धि" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱨᱩᱠᱷᱤᱭᱟᱹ, ᱫᱤᱥᱚᱢ ᱞᱟᱦᱟᱱᱛᱤ" } } },
            { "home.idLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "ID:" },
                { AppLanguage.Hindi,   "आईडी:" },
                { AppLanguage.Santali, "ᱩᱯᱨᱩᱢ:" } } },
            { "home.machinery.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Moving Parts, Lockout/Tagout & Safe Ops" },
                { AppLanguage.Hindi,   "मशीनों का सुरक्षित उपयोग, लॉकआउट/टैगआउट और सुरक्षित संचालन" },
                { AppLanguage.Santali, "ᱞᱟᱲᱟᱣ ᱦᱟᱹᱴᱤᱧ, ᱞᱚᱠ-ᱟᱣᱩᱴ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱠᱟᱹᱢᱤ" } } },
            { "home.machinery.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Machinery Safety" },
                { AppLanguage.Hindi,   "मशीनरी सुरक्षा" },
                { AppLanguage.Santali, "ᱢᱟᱹᱥᱤᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ" } } },
            { "home.mineFacility", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mining Facility" },
                { AppLanguage.Hindi,   "खनन परिसर" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱡᱟᱭᱜᱟ" } } },
            { "home.modulesCompleted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "{0} of {1} Modules Completed" },
                { AppLanguage.Hindi,   "{1} में से {0} मॉड्यूल पूरा" },
                { AppLanguage.Santali, "{1} ᱨᱮ {0} ᱢᱚᱰᱩᱞ ᱯᱩᱨᱟᱹᱣ ᱮᱱᱟ" } } },
            { "home.modulesSection", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Modules" },
                { AppLanguage.Hindi,   "प्रशिक्षण मॉड्यूल" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱢᱚᱰᱩᱞ ᱠᱚ" } } },
            { "home.navCertificates", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificates" },
                { AppLanguage.Hindi,   "प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱠᱚ" } } },
            { "home.navHome", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Home" },
                { AppLanguage.Hindi,   "होम" },
                { AppLanguage.Santali, "ᱢᱩᱞ ᱥᱟᱦᱴᱟ" } } },
            { "home.navLearn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn" },
                { AppLanguage.Hindi,   "सीखें" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ" } } },
            { "home.navProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "My Progress" },
                { AppLanguage.Hindi,   "मेरी प्रगति" },
                { AppLanguage.Santali, "ᱤᱧᱟᱜ ᱞᱟᱦᱟᱱᱛᱤ" } } },
            { "home.overallProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Overall Progress" },
                { AppLanguage.Hindi,   "कुल प्रगति" },
                { AppLanguage.Santali, "ᱢᱩᱴᱷᱟᱹᱱ ᱞᱟᱦᱟᱱᱛᱤ" } } },
            { "home.roleLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mine Worker" },
                { AppLanguage.Hindi,   "खान कार्यकर्ता" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱠᱟᱹᱢᱤᱭᱟᱹ" } } },
            { "home.startTraining", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Start Training" },
                { AppLanguage.Hindi,   "शुरू करें >" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱮᱦᱚᱵ ᱢᱮ" } } },
            { "home.sync.allDataSynced", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Sync: All Data Synced (Live)" },
                { AppLanguage.Hindi,   "सिंक: सभी डेटा सिंक्रनाइज़ है" },
                { AppLanguage.Santali, "ᱥᱤᱝᱠ: ᱡᱚᱛᱚ ᱰᱟᱴᱟ ᱥᱤᱝᱠ ᱟᱠᱟᱱᱟ" } } },
            { "home.sync.pendingTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Offline Assessment Ready" },
                { AppLanguage.Hindi,   "ऑफ़लाइन मूल्यांकन तैयार" },
                { AppLanguage.Santali, "ᱚᱯᱷᱞᱟᱭᱤᱱ ᱵᱤᱰᱟᱹᱣ ᱥᱟᱯᱲᱟᱣ ᱢᱮᱱᱟᱜᱼᱟ" } } },
            { "home.sync.retry", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "RETRY SYNC >" },
                { AppLanguage.Hindi,   "फिर कोशिश करें >" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱪᱮᱥᱴᱟᱭ ᱢᱮ >" } } },
            { "home.sync.syncNow", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SYNC NOW >" },
                { AppLanguage.Hindi,   "अभी सिंक करें >" },
                { AppLanguage.Santali, "ᱱᱤᱛ ᱥᱤᱝᱠ ᱢᱮ >" } } },
            { "home.sync.synced", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SYNCED" },
                { AppLanguage.Hindi,   "सिंक्रनाइज़ेशन पूरा हुआ" },
                { AppLanguage.Santali, "ᱥᱤᱝᱠ ᱮᱱᱟ" } } },
            { "home.sync.syncing", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SYNCING" },
                { AppLanguage.Hindi,   "डेटा सिंक्रनाइज़ हो रहा है" },
                { AppLanguage.Santali, "ᱥᱤᱝᱠᱚᱜ ᱠᱟᱱᱟ" } } },
            { "home.viewAll", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View All" },
                { AppLanguage.Hindi,   "सभी देखें >" },
                { AppLanguage.Santali, "ᱡᱚᱛᱚ ᱧᱮᱞ ᱢᱮ" } } },
            { "lang.santali", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Santali" },
                { AppLanguage.Hindi,   "संथाली" },
                { AppLanguage.Santali, "ᱥᱟᱱᱛᱟᱲᱤ" } } },
            { "lang.santali_proceed", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Proceed in Santali" },
                { AppLanguage.Hindi,   "संथाली में आगे बढ़ें" },
                { AppLanguage.Santali, "ᱥᱟᱱᱛᱟᱲᱤ ᱛᱮ ᱞᱟᱦᱟᱜ ᱢᱮ" } } },
            { "lang.santali_script", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Ol Chiki" },
                { AppLanguage.Hindi,   "ओल चिकी" },
                { AppLanguage.Santali, "ᱚᱞ ᱪᱤᱠᱤ" } } },
            { "language.bannerQuote", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn together for a safer tomorrow" },
                { AppLanguage.Hindi,   "सुरक्षित भविष्य के लिए साथ मिलकर सीखें" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱜᱟᱯᱟ ᱞᱟᱹᱜᱤᱫ ᱢᱤᱫ ᱛᱮ ᱥᱮᱪᱮᱫᱚᱜ ᱢᱮ" } } },
            { "language.continue", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Continue" },
                { AppLanguage.Hindi,   "जारी रखें" },
                { AppLanguage.Santali, "ᱞᱟᱦᱟᱜ ᱢᱮ" } } },
            { "language.skip", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Skip" },
                { AppLanguage.Hindi,   "छोड़ें" },
                { AppLanguage.Santali, "ᱵᱟᱹᱜᱤ ᱢᱮ" } } },
            { "language.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Choose your preferred language to continue" },
                { AppLanguage.Hindi,   "आगे बढ़ने के लिए अपनी पसंदीदा भाषा चुनें" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱠᱩᱥᱤ ᱯᱟᱹᱨᱥᱤ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "language.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Language" },
                { AppLanguage.Hindi,   "भाषा चुनें" },
                { AppLanguage.Santali, "ᱯᱟᱹᱨᱥᱤ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "login.continueAsGuest", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Continue as Guest" },
                { AppLanguage.Hindi,   "अतिथि के रूप में जारी रखें" },
                { AppLanguage.Santali, "ᱯᱮᱲᱟ ᱞᱮᱠᱟᱛᱮ ᱞᱟᱦᱟᱜ ᱢᱮ" } } },
            { "login.employeeId", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Employee ID" },
                { AppLanguage.Hindi,   "कर्मचारी आईडी" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ ᱮᱞ" } } },
            { "login.employeeIdPlaceholder", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Enter your Employee ID" },
                { AppLanguage.Hindi,   "अपनी कर्मचारी आईडी दर्ज करें" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ ᱮᱞ ᱚᱞ ᱢᱮ" } } },
            { "login.emptyEmployeeError", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Please enter an Employee ID." },
                { AppLanguage.Hindi,   "कृपया कर्मचारी आईडी दर्ज करें।" },
                { AppLanguage.Santali, "ᱫᱟᱭᱟ ᱠᱟᱛᱮ ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ ᱮᱞ ᱚᱞ ᱢᱮ ᱾" } } },
            { "login.emptyGuestError", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Please enter a Guest ID." },
                { AppLanguage.Hindi,   "कृपया अतिथि आईडी दर्ज करें।" },
                { AppLanguage.Santali, "ᱫᱟᱭᱟ ᱠᱟᱛᱮ ᱯᱮᱲᱟ ᱩᱯᱨᱩᱢ ᱮᱞ ᱚᱞ ᱢᱮ ᱾" } } },
            { "login.forgotPassword", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Forgot Password?" },
                { AppLanguage.Hindi,   "पासवर्ड भूल गए?" },
                { AppLanguage.Santali, "ᱯᱟᱥᱣᱚᱨᱰ ᱦᱤᱲᱤᱧ ᱮᱱᱟ?" } } },
            { "login.forgotPasswordMessage", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Please contact your Mine Safety Administrator or Site Supervisor to reset your PIN." },
                { AppLanguage.Hindi,   "कृपया अपना पिन रीसेट करने के लिए अपने खान सुरक्षा प्रशासक या साइट पर्यवेक्षक से संपर्क करें।" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱯᱤᱱ ᱨᱤᱥᱮᱴ ᱞᱟᱹᱜᱤᱫ ᱟᱢᱤᱡ ᱠᱷᱟᱫᱟᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱟᱹᱢᱟᱹᱞᱤᱭᱟᱹ ᱥᱟᱶ ᱡᱚᱯᱲᱟᱣ ᱢᱮ ᱾" } } },
            { "login.guestIdLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Guest ID" },
                { AppLanguage.Hindi,   "अतिथि आईडी" },
                { AppLanguage.Santali, "ᱯᱮᱲᱟ ᱩᱯᱨᱩᱢ ᱮᱞ" } } },
            { "login.guestIdPlaceholder", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Enter your Guest ID (e.g. GUEST-12345)" },
                { AppLanguage.Hindi,   "अपनी अतिथि आईडी दर्ज करें (उदा. GUEST-12345)" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱯᱮᱲᱟ ᱩᱯᱨᱩᱢ ᱮᱞ ᱚᱞ ᱢᱮ (ᱞᱮᱠᱟ: GUEST-12345)" } } },
            { "login.guestMode", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Guest Mode" },
                { AppLanguage.Hindi,   "अतिथि मोड" },
                { AppLanguage.Santali, "ᱯᱮᱲᱟ ᱢᱚᱰ" } } },
            { "login.invalidCredentials", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Invalid credentials. Please check your Employee ID and password." },
                { AppLanguage.Hindi,   "अमान्य क्रेडेंशियल। कृपया अपनी कर्मचारी आईडी और पासवर्ड जांचें।" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱴᱷᱤᱠ ᱩᱯᱨᱩᱢ ᱾ ᱫᱟᱭᱟ ᱠᱟᱛᱮ ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ ᱮᱞ ᱟᱨ ᱯᱟᱥᱣᱚᱨᱰ ᱧᱮᱞ ᱢᱮ ᱾" } } },
            { "login.or", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "OR" },
                { AppLanguage.Hindi,   "अथवा" },
                { AppLanguage.Santali, "ᱵᱟᱝᱠᱷᱟᱱ" } } },
            { "login.password", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Password / PIN" },
                { AppLanguage.Hindi,   "पासवर्ड / पिन" },
                { AppLanguage.Santali, "ᱯᱟᱥᱣᱚᱨᱰ / ᱯᱤᱱ" } } },
            { "login.passwordPlaceholder", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Enter your password" },
                { AppLanguage.Hindi,   "अपना पासवर्ड दर्ज करें" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱯᱟᱥᱣᱚᱨᱰ ᱚᱞ ᱢᱮ" } } },
            { "login.qrCode", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Login with QR Code" },
                { AppLanguage.Hindi,   "क्यूआर कोड द्वारा लॉगिन करें" },
                { AppLanguage.Santali, "ᱠᱤᱣ.ᱟᱨ. ᱠᱳᱰ ᱛᱮ ᱵᱚᱞᱚᱱ ᱢᱮ" } } },
            { "login.rememberMe", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remember me" },
                { AppLanguage.Hindi,   "मुझे याद रखें" },
                { AppLanguage.Santali, "ᱤᱧ ᱩᱭᱦᱟᱹᱨ ᱤᱧ ᱢᱮ" } } },
            { "login.submit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Login" },
                { AppLanguage.Hindi,   "लॉगिन करें" },
                { AppLanguage.Santali, "ᱵᱚᱞᱚᱱ ᱢᱮ" } } },
            { "login.tagline", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safe Mines, Empowered Nation" },
                { AppLanguage.Hindi,   "सुरक्षित खनन, समृद्ध राष्ट्र" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱠᱷᱟᱫᱟᱱ, ᱫᱟᱲᱮᱭᱟᱱ ᱫᱤᱥᱚᱢ" } } },
            { "login.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker Access" },
                { AppLanguage.Hindi,   "श्रमिक प्रवेश" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱵᱚᱞᱚᱱ" } } },
            { "login.workerLogin", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker Login" },
                { AppLanguage.Hindi,   "श्रमिक लॉगिन" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱞᱚᱜᱤᱱ" } } },
            { "module.electrical.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify electrical hazards, high-voltage risks, insulation integrity, and grounding safety procedures." },
                { AppLanguage.Hindi,   "विद्युत संबंधी खतरों की पहचान करना, लॉकआउट/टैगआउट प्रक्रियाओं का पालन करना और कार्यस्थल पर सुरक्षित व्यवहार अपनाना।" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱵᱚᱛᱚᱨ ᱪᱤᱱᱦᱟᱹᱣ ᱢᱮ, ᱞᱚᱠ-ᱟᱣᱩᱴ ᱱᱤᱭᱟᱹᱢ ᱯᱟᱸᱡᱟᱭ ᱢᱮ ᱟᱨ ᱠᱟᱹᱢᱤ ᱡᱟᱭᱜᱟ ᱨᱮ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱫᱚᱦᱚᱭ ᱢᱮ ᱾" } } },
            { "module.electrical.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Electrical Safety" },
                { AppLanguage.Hindi,   "विद्युत सुरक्षा" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱨᱩᱠᱷᱤᱭᱟᱹ" } } },
            { "module.fire.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn to identify fire hazards, use extinguishers, activate alarms and evacuate safely in industrial environments." },
                { AppLanguage.Hindi,   "आग के खतरों की पहचान करना, अग्निशामक यंत्र का उपयोग करना, फायर अलार्म सक्रिय करना और आपात स्थिति में सुरक्षित तरीके से निकासी करना।" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱵᱚᱛᱚᱨ ᱪᱤᱱᱦᱟᱹᱣ, ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱵᱮᱵᱷᱟᱨ, ᱮᱞᱟᱨᱢ ᱪᱟᱞᱟᱣ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱛᱮ ᱚᱰᱚᱠᱚᱜ ᱥᱮᱪᱮᱫ ᱢᱮ ᱾" } } },
            { "module.fire.learn.1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify various classes of fire hazards" },
                { AppLanguage.Hindi,   "विभिन्न श्रेणियों के अग्नि खतरों की पहचान करें" },
                { AppLanguage.Santali, "ᱟᱭᱢᱟ ᱞᱮᱠᱟᱱ ᱥᱮᱸᱜᱮᱞ ᱵᱚᱛᱚᱨ ᱪᱤᱱᱦᱟᱹᱣ ᱢᱮ" } } },
            { "module.fire.learn.2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Locate and operate appropriate safety equipment" },
                { AppLanguage.Hindi,   "उपयुक्त अग्निशामक का चयन एवं संचालन करें" },
                { AppLanguage.Santali, "ᱴᱷᱤᱠ ᱥᱮᱸᱜᱮᱞ ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱵᱟᱪᱷᱟᱣ ᱟᱨ ᱪᱟᱞᱟᱣ ᱢᱮ" } } },
            { "module.fire.learn.3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Follow the PASS technique and safety procedures" },
                { AppLanguage.Hindi,   "PASS तकनीक और सुरक्षा प्रक्रियाओं का पालन करें" },
                { AppLanguage.Santali, "PASS ᱱᱤᱭᱟᱹᱢ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱰᱟᱦᱟᱨ ᱯᱟᱸᱡᱟᱭ ᱢᱮ" } } },
            { "module.fire.learn.4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Navigate emergency evacuation routes safely" },
                { AppLanguage.Hindi,   "आपातकालीन निकास मार्गों का सुरक्षित अनुसरण करें" },
                { AppLanguage.Santali, "ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱚᱰᱚᱠ ᱰᱟᱦᱟᱨ ᱛᱮ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱛᱮ ᱚᱰᱚᱠᱚᱜ ᱢᱮ" } } },
            { "module.fire.learn.5", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Report hazards and near-misses to supervisors" },
                { AppLanguage.Hindi,   "खतरों तथा दुर्घटना की आशंका की सूचना पर्यवेक्षक को दें" },
                { AppLanguage.Santali, "ᱵᱚᱛᱚᱨ ᱟᱨ ᱜᱷᱚᱴᱚᱱ ᱨᱮᱱᱟᱜ ᱠᱷᱚᱵᱚᱨ ᱥᱩᱯᱚᱨᱵᱷᱟᱭᱤᱡᱚᱨ ᱮᱢᱟᱭ ᱢᱮ" } } },
            { "module.fire.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Electrical Fire, Extinguisher & Evacuation SOP" },
                { AppLanguage.Hindi,   "खतरों की पहचान, अग्निशामक यंत्र का उपयोग और सुरक्षित निकासी" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱥᱮᱸᱜᱮᱞ, ᱤᱨᱤᱡ ᱥᱟᱢᱟᱱ ᱟᱨ ᱚᱰᱚᱠ SOP" } } },
            { "module.fire.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire & Explosion Response" },
                { AppLanguage.Hindi,   "आग एवं विस्फोट से निपटने की प्रक्रिया" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱟᱨ ᱵᱷᱚᱢ ᱯᱚᱨᱚᱠ" } } },
            { "module.gas.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify hazardous gases, follow confined-space procedures, and select proper Personal Protective Equipment (PPE)." },
                { AppLanguage.Hindi,   "खतरनाक गैसों की पहचान करना, सीमित स्थानों में सुरक्षा प्रक्रियाओं का पालन करना और व्यक्तिगत सुरक्षा उपकरण (PPE) का सही उपयोग करना।" },
                { AppLanguage.Santali, "ᱵᱤᱥ ᱜᱮᱥ ᱪᱤᱱᱦᱟᱹᱣ, ᱥᱟᱸᱜᱷᱟᱨ ᱡᱟᱭᱜᱟ ᱨᱮ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱱᱤᱭᱟᱹᱢ ᱢᱟᱱᱟᱣ ᱟᱨ PPE ᱵᱮᱵᱷᱟᱨ ᱢᱮ ᱾" } } },
            { "module.gas.learn.1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Recognize gas leak indicators and use detectors" },
                { AppLanguage.Hindi,   "गैस रिसाव के संकेतों तथा गैस डिटेक्टर का उपयोग समझें" },
                { AppLanguage.Santali, "ᱜᱮᱥ ᱞᱤᱠ ᱪᱤᱱᱦᱟᱹ ᱟᱨ ᱜᱮᱥ ᱰᱤᱴᱮᱠᱴᱚᱨ ᱵᱮᱵᱷᱟᱨ ᱵᱩᱡᱷᱟᱹᱣ ᱢᱮ" } } },
            { "module.gas.learn.2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Inspect and wear respiratory protection properly" },
                { AppLanguage.Hindi,   "श्वसन सुरक्षा उपकरण (SCBA/मास्क) की जाँच एवं उपयोग करें" },
                { AppLanguage.Santali, "ᱥᱟᱦᱮᱫ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱢᱟᱱ (SCBA/ᱢᱟᱥᱠ) ᱧᱮᱞ ᱟᱨ ᱦᱚᱨᱚᱜ ᱢᱮ" } } },
            { "module.gas.learn.3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Follow confined space entry permit protocols" },
                { AppLanguage.Hindi,   "सीमित स्थान प्रवेश परमिट और वेंटिलेशन प्रक्रियाओं का पालन करें" },
                { AppLanguage.Santali, "ᱥᱟᱸᱜᱷᱟᱨ ᱡᱟᱭᱜᱟ ᱵᱚᱞᱚᱱ ᱪᱷᱟᱹᱲ ᱟᱨ ᱦᱚᱭ ᱦᱤᱥᱤᱫ ᱱᱤᱭᱟᱹᱢ ᱢᱟᱱᱟᱣ ᱢᱮ" } } },
            { "module.gas.learn.4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Evacuate safely and sound emergency alarms" },
                { AppLanguage.Hindi,   "सुरक्षित रूप से बाहर निकलें तथा आपातकालीन अलार्म सक्रिय करें" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱛᱮ ᱚᱰᱚᱠᱚᱜ ᱢᱮ ᱟᱨ ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱮᱞᱟᱨᱢ ᱚᱛᱟᱭ ᱢᱮ" } } },
            { "module.gas.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Gas Leak & Confined Space" },
                { AppLanguage.Hindi,   "गैस रिसाव एवं सीमित स्थान" },
                { AppLanguage.Santali, "ᱜᱮᱥ ᱞᱤᱠ ᱟᱨ ᱥᱟᱸᱜᱷᱟᱨ ᱡᱟᱭᱜᱟ" } } },
            { "module.machinery.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Understand machine guarding, lockout/tagout procedures, and safe operational distances around moving parts." },
                { AppLanguage.Hindi,   "औद्योगिक मशीनों से संबंधित खतरों की पहचान करना और सुरक्षित संचालन प्रक्रियाओं का पालन करना।" },
                { AppLanguage.Santali, "ᱢᱟᱹᱥᱤᱱ ᱨᱮᱱᱟᱜ ᱞᱟᱲᱟᱣ ᱦᱟᱹᱴᱤᱧ ᱵᱚᱛᱚᱨ ᱵᱩᱡᱷᱟᱹᱣ ᱢᱮ ᱟᱨ ᱞᱚᱠ-ᱟᱣᱩᱴ ᱱᱤᱭᱟᱹᱢ ᱢᱟᱱᱟᱣ ᱢᱮ ᱾" } } },
            { "module.machinery.learn.1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify machine guarding and emergency stop controls" },
                { AppLanguage.Hindi,   "मशीनरी गार्डिंग तथा आपातकालीन स्टॉप नियंत्रण की पहचान करें" },
                { AppLanguage.Santali, "ᱢᱟᱹᱥᱤᱱ ᱜᱟᱨᱰ ᱟᱨ ᱟᱯᱚᱛᱠᱟᱲᱤᱱ ᱛᱷᱟᱢᱵᱷᱟᱣ ᱪᱤᱱᱦᱟᱹᱣ ᱢᱮ" } } },
            { "module.machinery.learn.2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Apply lockout/tagout (LOTO) before maintenance" },
                { AppLanguage.Hindi,   "रखरखाव से पूर्व लॉकआउट/टैगआउट (LOTO) प्रक्रिया लागू करें" },
                { AppLanguage.Santali, "ᱥᱟᱡᱟᱣ ᱞᱟᱦᱟᱨᱮ ᱞᱚᱠ-ᱟᱣᱩᱴ/ᱴᱮᱜ-ᱟᱣᱩᱴ (LOTO) ᱱᱤᱭᱟᱹᱢ ᱞᱟᱜᱟᱣ ᱢᱮ" } } },
            { "module.machinery.learn.3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Maintain safe clearance from moving parts" },
                { AppLanguage.Hindi,   "गतिशील एवं घूर्णनशील पुर्जों से सुरक्षित दूरी बनाए रखें" },
                { AppLanguage.Santali, "ᱞᱟᱲᱟᱣ ᱦᱟᱹᱴᱤᱧ ᱠᱷᱚᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱺᱜᱤᱧ ᱨᱮ ᱛᱟᱦᱮᱸᱱ ᱢᱮ" } } },
            { "module.machinery.learn.4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Wear mandatory Personal Protective Equipment (PPE)" },
                { AppLanguage.Hindi,   "उचित व्यक्तिगत सुरक्षा उपकरण (PPE) का अनिवार्य उपयोग करें" },
                { AppLanguage.Santali, "ᱞᱟᱹᱠᱛᱤᱭᱟᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱢᱟᱱ (PPE) ᱞᱟᱜᱟᱣ ᱢᱮ" } } },
            { "module.machinery.learn.5", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Report damaged or malfunctioning equipment immediately" },
                { AppLanguage.Hindi,   "क्षतिग्रस्त मशीनरी एवं उपकरणों की तत्काल रिपोर्ट करें" },
                { AppLanguage.Santali, "ᱵᱟᱹᱲᱤᱡ ᱢᱟᱹᱥᱤᱱ ᱟᱨ ᱥᱟᱢᱟᱱ ᱠᱚ ᱞᱚᱜᱚᱱ ᱜᱮ ᱠᱷᱚᱵᱚᱨ ᱮᱢ ᱢᱮ" } } },
            { "module.machinery.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Machinery Safety" },
                { AppLanguage.Hindi,   "मशीनरी सुरक्षा" },
                { AppLanguage.Santali, "ᱢᱟᱹᱥᱤᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ" } } },
            { "module.minehazard.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify mining-specific hazards, roof fall risks, ventilation checks, and industrial environmental protection." },
                { AppLanguage.Hindi,   "खनन-विशिष्ट खतरों की पहचान करना तथा औद्योगिक खनन में पर्यावरण संरक्षण प्रक्रियाओं का पालन करना।" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱨᱮᱱᱟᱜ ᱵᱚᱛᱚᱨ ᱠᱚ ᱪᱤᱱᱦᱟᱹᱣ ᱢᱮ ᱟᱨ ᱠᱟᱹᱨᱜᱟᱲ ᱠᱷᱟᱫᱟᱱ ᱨᱮ ᱯᱚᱨᱤᱵᱮᱥ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱱᱤᱭᱟᱹᱢ ᱢᱟᱱᱟᱣ ᱢᱮ ᱾" } } },
            { "module.minehazard.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mining Hazards & Environmental Safety" },
                { AppLanguage.Hindi,   "खनन संबंधी खतरे एवं पर्यावरण सुरक्षा" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱵᱚᱛᱚᱨ ᱟᱨ ᱯᱚᱨᱤᱵᱮᱥ ᱨᱩᱠᱷᱤᱭᱟᱹ" } } },
            { "module.statusAvailable", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "AVAILABLE" },
                { AppLanguage.Hindi,   "उपलब्ध" },
                { AppLanguage.Santali, "ᱧᱟᱢᱚᱜ ᱠᱟᱱᱟ" } } },
            { "module.statusComingSoon", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "COMING SOON" },
                { AppLanguage.Hindi,   "जल्द आ रहा है" },
                { AppLanguage.Santali, "ᱞᱚᱜᱚᱱ ᱦᱤᱡᱩᱜ ᱠᱟᱱᱟ" } } },
            { "module.statusLocked", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "LOCKED" },
                { AppLanguage.Hindi,   "प्रतिबंधित" },
                { AppLanguage.Santali, "ᱵᱚᱱᱫᱚ ᱢᱮᱱᱟᱜᱼᱟ" } } },
            { "module.statusTheoryOnly", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "THEORY / ASSESSMENT" },
                { AppLanguage.Hindi,   "सावधानी / मूल प्रशिक्षण" },
                { AppLanguage.Santali, "ᱛᱷᱤᱭᱚᱨᱤ / ᱵᱤᱰᱟᱹᱣ" } } },
            { "module.trainingModule", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "TRAINING MODULE" },
                { AppLanguage.Hindi,   "प्रशिक्षण मॉड्यूल" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱢᱚᱰᱩᱞ" } } },
            { "moduleDetail.duration", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "15–20 min" },
                { AppLanguage.Hindi,   "15–20 मिनट" },
                { AppLanguage.Santali, "᱑᱕–᱒᱐ ᱢᱤᱱᱤᱴ" } } },
            { "moduleDetail.learnPoint1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Step-by-step SOP for live electrical panel fires" },
                { AppLanguage.Hindi,   "चालू विद्युत पैनल की आग के लिए चरणबद्ध एसओपी" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱯᱮᱱᱟᱞ ᱥᱮᱸᱜᱮᱞ ᱞᱟᱹᱜᱤᱫ ᱫᱷᱟᱯ-ᱫᱷᱟᱯ ᱮᱥ.ᱳ.ᱯᱤ." } } },
            { "moduleDetail.learnPoint2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Class C extinguisher selection and safety pin operation" },
                { AppLanguage.Hindi,   "वर्ग C अग्निशामक का चयन और सुरक्षा पिन संचालन" },
                { AppLanguage.Santali, "ᱠᱞᱟᱥ C ᱤᱬᱤᱡᱤᱡ ᱵᱟᱪᱷᱟᱣ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱯᱤᱱ ᱚᱰᱚᱠ" } } },
            { "moduleDetail.learnPoint3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "PASS method: Pull, Aim, Squeeze, Sweep technique" },
                { AppLanguage.Hindi,   "PASS विधि: पिन खींचना, निशाना लगाना, दबाना और झाड़ना" },
                { AppLanguage.Santali, "PASS ᱛᱚᱦᱚᱨ: ᱚᱨ, ᱡᱚᱥ, ᱚᱛᱟ, ᱯᱷᱟᱭᱞᱟᱣ" } } },
            { "moduleDetail.learnPoint4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Post-extinguishment thermal re-ignition prevention" },
                { AppLanguage.Hindi,   "आग बुझाने के बाद पुनः आग भड़कने से रोकथाम" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡ ᱛᱟᱭᱚᱢ ᱫᱚᱦᱲᱟ ᱡᱩᱞᱩᱜ ᱵᱟᱧᱪᱟᱣ" } } },
            { "moduleDetail.learnTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "What you'll learn" },
                { AppLanguage.Hindi,   "आप क्या सीखेंगे" },
                { AppLanguage.Santali, "ᱟᱢ ᱪᱮᱫ ᱮᱢ ᱥᱮᱪᱮᱫᱚᱜᱼᱟ" } } },
            { "moduleDetail.safetyNoteDesc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Never attempt to fight a fire if evacuation routes are compromised. Always alert colleagues first before responding." },
                { AppLanguage.Hindi,   "यदि निकास मार्ग अवरुद्ध हो तो कभी भी आग बुझाने का प्रयास न करें। कार्रवाई करने से पहले हमेशा साथियों को सतर्क करें।" },
                { AppLanguage.Santali, "ᱡᱩᱫᱤ ᱚᱰᱚᱠᱚᱜ ᱦᱚᱨ ᱵᱚᱱᱫᱚ ᱛᱟᱦᱮᱸᱱᱟ ᱛᱚᱵᱮ ᱥᱮᱸᱜᱮᱞ ᱤᱬᱤᱡ ᱟᱞᱚᱢ ᱪᱮᱥᱴᱟᱭᱟ ᱾ ᱢᱟᱲᱟᱝ ᱥᱟᱶᱛᱮᱱ ᱠᱚ ᱦᱩᱥᱤᱭᱟᱹᱨ ᱠᱚᱣᱢᱮ ᱾" } } },
            { "moduleDetail.safetyNoteTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SAFETY NOTE" },
                { AppLanguage.Hindi,   "सुरक्षा निर्देश" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱫᱤᱥᱟᱹ" } } },
            { "moduleDetail.scenarios", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scenarios" },
                { AppLanguage.Hindi,   "परिदृश्य" },
                { AppLanguage.Santali, "ᱯᱚᱨᱚᱠ ᱠᱚ" } } },
            { "moduleDetail.startModule", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Start Module" },
                { AppLanguage.Hindi,   "प्रशिक्षण शुरू करें" },
                { AppLanguage.Santali, "ᱢᱚᱰᱩᱞ ᱮᱦᱚᱵ ᱢᱮ" } } },
            { "moduleDetail.subtitleLearn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn • Practice • Assess" },
                { AppLanguage.Hindi,   "सीखें • अभ्यास करें • मूल्यांकन करें" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ • ᱪᱮᱥᱴᱟ • ᱵᱤᱰᱟᱹᱣ" } } },
            { "moduleDetail.whatYouLearn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "What You Will Learn:" },
                { AppLanguage.Hindi,   "प्रशिक्षण के मुख्य बिंदु:" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱨᱮᱱᱟᱜ ᱢᱩᱞ ᱠᱟᱛᱷᱟ:" } } },
            { "moduleSelection.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Choose an industrial safety module to begin practical AR training" },
                { AppLanguage.Hindi,   "व्यावहारिक AR प्रशिक्षण शुरू करने के लिए कोई मॉड्यूल चुनें" },
                { AppLanguage.Santali, "ᱮ.ᱟᱨ. ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱮᱪᱮᱫ ᱮᱦᱚᱵ ᱞᱟᱹᱜᱤᱫ ᱢᱚᱰᱩᱞ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "moduleSelection.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Training Module" },
                { AppLanguage.Hindi,   "प्रशिक्षण मॉड्यूल चुनें" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱢᱚᱰᱩᱞ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "notif.caughtUp", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "You're all caught up!" },
                { AppLanguage.Hindi,   "आप पूरी तरह अपडेट हैं!" },
                { AppLanguage.Santali, "ᱟᱢ ᱡᱚᱛᱚᱣᱟᱜ ᱧᱮᱞ ᱯᱩᱨᱟᱹᱣ ᱠᱮᱫᱟᱢ !" } } },
            { "notif.emptySub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "You currently have no pending alerts or safety notices." },
                { AppLanguage.Hindi,   "आपके पास वर्तमान में कोई लंबित अलर्ट या सुरक्षा सूचना नहीं है।" },
                { AppLanguage.Santali, "ᱟᱢ ᱴᱷᱮᱱ ᱱᱤᱛ ᱫᱷᱟᱹᱵᱤᱡ ᱪᱮᱫ ᱦᱚᱸ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱦᱩᱥᱤᱭᱟᱹᱨ ᱵᱟᱹᱱᱩᱜᱼᱟ ᱾" } } },
            { "notif.markAllRead", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mark All as Read" },
                { AppLanguage.Hindi,   "सभी को पढ़ा हुआ चिह्नित करें" },
                { AppLanguage.Santali, "ᱡᱚᱛᱚ ᱯᱟᱲᱦᱟᱣ ᱟᱠᱟᱱ ᱪᱤᱱᱦᱟᱹᱣ ᱢᱮ" } } },
            { "notif.noNew", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "No new notifications" },
                { AppLanguage.Hindi,   "कोई नई सूचना नहीं है" },
                { AppLanguage.Santali, "ᱱᱟᱣᱟ ᱠᱷᱚᱵᱚᱨ ᱵᱟᱹᱱᱩᱜᱼᱟ" } } },
            { "notif.read", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Read" },
                { AppLanguage.Hindi,   "पढ़ी गई" },
                { AppLanguage.Santali, "ᱯᱟᱲᱦᱟᱣ ᱮᱱᱟ" } } },
            { "notif.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Notifications" },
                { AppLanguage.Hindi,   "सूचनाएँ" },
                { AppLanguage.Santali, "ᱵᱟᱰᱟᱭ ᱡᱚᱝ ᱠᱷᱚᱵᱚᱨ" } } },
            { "notif.unread", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Unread" },
                { AppLanguage.Hindi,   "अपठित" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱯᱟᱲᱦᱟᱣ ᱟᱠᱟᱱ" } } },
            { "profile.appSettings", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "App Settings" },
                { AppLanguage.Hindi,   "ऐप सेटिंग्स" },
                { AppLanguage.Santali, "ᱮᱯ ᱥᱟᱡᱟᱣ" } } },
            { "profile.appSettingsSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Sound, Privacy, Help" },
                { AppLanguage.Hindi,   "ध्वनि, गोपनीयता, सहायता" },
                { AppLanguage.Santali, "ᱥᱟᱰᱮ, ᱩᱠᱩ, ᱜᱚᱲᱚ" } } },
            { "profile.close", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Close" },
                { AppLanguage.Hindi,   "बंद करें" },
                { AppLanguage.Santali, "ᱵᱚᱱᱫᱚ" } } },
            { "profile.completedLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Modules Completed" },
                { AppLanguage.Hindi,   "पूर्ण किए गए मॉड्यूल" },
                { AppLanguage.Santali, "ᱯᱩᱨᱟᱹᱣ ᱟᱠᱟᱱ ᱢᱚᱰᱩᱞ" } } },
            { "profile.contact", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Contact" },
                { AppLanguage.Hindi,   "संपर्क" },
                { AppLanguage.Santali, "ᱥᱟᱹᱜᱟᱹᱭ" } } },
            { "profile.defaultRole", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mining Safety Personnel" },
                { AppLanguage.Hindi,   "खनन सुरक्षा कर्मचारी" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱠᱟᱹᱢᱤᱭᱟᱹ" } } },
            { "profile.defaultSite", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Industrial Mining Facility" },
                { AppLanguage.Hindi,   "औद्योगिक खनन परिसर" },
                { AppLanguage.Santali, "ᱠᱟᱹᱨᱜᱟᱲ ᱠᱷᱟᱫᱟᱱ ᱡᱟᱭᱜᱟ" } } },
            { "profile.department", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Department" },
                { AppLanguage.Hindi,   "विभाग" },
                { AppLanguage.Santali, "ᱦᱟᱹᱴᱤᱧ" } } },
            { "profile.earnedLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificates Earned" },
                { AppLanguage.Hindi,   "अर्जित प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱦᱟᱢᱮᱴ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ" } } },
            { "profile.edit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Edit" },
                { AppLanguage.Hindi,   "संपादित करें" },
                { AppLanguage.Santali, "ᱥᱟᱯᱲᱟᱣ" } } },
            { "profile.employeeIdLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Employee ID" },
                { AppLanguage.Hindi,   "कर्मचारी आईडी" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ" } } },
            { "profile.experience", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Experience (Years)" },
                { AppLanguage.Hindi,   "कार्य अनुभव (वर्ष)" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤ ᱦᱩᱱᱟᱹᱨ (ᱥᱮᱨᱢᱟ)" } } },
            { "profile.fullName", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Full Name" },
                { AppLanguage.Hindi,   "पूरा नाम" },
                { AppLanguage.Santali, "ᱯᱩᱨᱟᱹ ᱧᱩᱛᱩᱢ" } } },
            { "profile.guestIdLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Guest ID" },
                { AppLanguage.Hindi,   "अतिथि आईडी" },
                { AppLanguage.Santali, "ᱯᱮᱲᱟ ᱩᱯᱨᱩᱢ" } } },
            { "profile.hoursLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Hours" },
                { AppLanguage.Hindi,   "प्रशिक्षण के कुल घंटे" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱜᱷᱟᱱᱴᱟ" } } },
            { "profile.logout", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Logout" },
                { AppLanguage.Hindi,   "लॉगआउट" },
                { AppLanguage.Santali, "ᱞᱚᱜᱽ ᱟᱣᱩᱴ" } } },
            { "profile.logoutSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Sign out from this device" },
                { AppLanguage.Hindi,   "इस उपकरण से साइन आउट करें" },
                { AppLanguage.Santali, "ᱱᱚᱣᱟ ᱥᱟᱫᱷᱚᱱ ᱠᱷᱚᱱ ᱚᱰᱚᱠᱚᱜ ᱢᱮ" } } },
            { "profile.myCertificates", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "My Certificates" },
                { AppLanguage.Hindi,   "मेरे प्रमाणपत्र" },
                { AppLanguage.Santali, "ᱤᱧᱟᱜ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ" } } },
            { "profile.myCertificatesSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View and download your certificates" },
                { AppLanguage.Hindi,   "प्रमाणपत्र देखें एवं डाउनलोड करें" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱧᱮᱞ ᱟᱨ ᱰᱟᱣᱩᱱᱞᱳᱰ" } } },
            { "profile.next", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Next" },
                { AppLanguage.Hindi,   "आगे" },
                { AppLanguage.Santali, "ᱞᱟᱦᱟ" } } },
            { "profile.notAvailable", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Not Available" },
                { AppLanguage.Hindi,   "उपलब्ध नहीं है" },
                { AppLanguage.Santali, "ᱵᱟᱹᱱᱩᱜᱼᱟ" } } },
            { "profile.notProvided", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Not Provided" },
                { AppLanguage.Hindi,   "प्रदान नहीं किया गया है" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱮᱢ ᱟᱠᱟᱱᱟ" } } },
            { "profile.personalInfo", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Personal Information" },
                { AppLanguage.Hindi,   "व्यक्तिगत विवरण" },
                { AppLanguage.Santali, "ᱟᱯᱱᱟᱨ ᱵᱤᱵᱚᱨᱚᱱ" } } },
            { "profile.personalInfoSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Name, Contact, Department" },
                { AppLanguage.Hindi,   "नाम, संपर्क, विभाग" },
                { AppLanguage.Santali, "ᱧᱩᱛᱩᱢ, ᱥᱟᱹᱜᱟᱹᱭ, ᱦᱟᱹᱴᱤᱧ" } } },
            { "profile.safetyPreferences", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safety Preferences" },
                { AppLanguage.Hindi,   "सुरक्षा प्राथमिकताएँ" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱠᱩᱥᱤ" } } },
            { "profile.safetyPreferencesSub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Language, Notifications" },
                { AppLanguage.Hindi,   "भाषा, सूचनाएँ" },
                { AppLanguage.Santali, "ᱯᱟᱹᱨᱥᱤ, ᱠᱷᱚᱵᱚᱨ" } } },
            { "profile.save", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Save Changes" },
                { AppLanguage.Hindi,   "परिवर्तन सहेजें" },
                { AppLanguage.Santali, "ᱵᱚᱫᱚᱞ ᱥᱟᱧᱪᱟᱣ" } } },
            { "profile.saveSuccess", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Profile updated successfully." },
                { AppLanguage.Hindi,   "प्रोफ़ाइल सफलतापूर्वक अपडेट की गई।" },
                { AppLanguage.Santali, "ᱯᱷᱨᱳᱯᱷᱟᱭᱤᱞ ᱵᱮᱥᱛᱮ ᱵᱚᱫᱚᱞ ᱮᱱᱟ ᱾" } } },
            { "profile.sector", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Industrial Sector" },
                { AppLanguage.Hindi,   "औद्योगिक क्षेत्र" },
                { AppLanguage.Santali, "ᱠᱟᱹᱨᱜᱟᱲ ᱦᱟᱹᱴᱤᱧ" } } },
            { "profile.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Personnel information and safety training records" },
                { AppLanguage.Hindi,   "व्यक्तिगत विवरण एवं सुरक्षा प्रशिक्षण अभिलेख" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱵᱤᱵᱚᱨᱚᱱ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱮᱪᱮᱫ ᱨᱮᱠᱚᱨᱰ" } } },
            { "profile.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker Profile" },
                { AppLanguage.Hindi,   "श्रमिक प्रोफ़ाइल" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱯᱷᱨᱳᱯᱷᱟᱭᱤᱞ" } } },
            { "profile.trainingHistory", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training History" },
                { AppLanguage.Hindi,   "प्रशिक्षण इतिहास" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱱᱟᱜᱟᱢ" } } },
            { "profile.trainingHistorySub", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Completed modules and scores" },
                { AppLanguage.Hindi,   "पूर्ण किए गए मॉड्यूल एवं अंक" },
                { AppLanguage.Santali, "ᱯᱩᱨᱟᱹᱣ ᱟᱠᱟᱱ ᱢᱚᱰᱩᱞ ᱟᱨ ᱱᱚᱢᱵᱚᱨ" } } },
            { "profile.workerId", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker ID" },
                { AppLanguage.Hindi,   "श्रमिक आईडी" },
                { AppLanguage.Santali, "ᱠᱟᱹᱢᱤᱭᱟᱹ ᱩᱯᱨᱩᱢ" } } },
            { "progress.drillsCompleted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Completed Drills" },
                { AppLanguage.Hindi,   "पूर्ण की गई अभ्यास ड्रिल" },
                { AppLanguage.Santali, "ᱯᱩᱨᱟᱹᱣ ᱟᱠᱟᱱ ᱰᱨᱤᱞ" } } },
            { "progress.electrical.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Complete training on electrical hazard identification and safe operational procedures." },
                { AppLanguage.Hindi,   "विद्युत खतरों की पहचान और सुरक्षित कार्य प्रक्रियाओं से संबंधित प्रशिक्षण पूरा करें।" },
                { AppLanguage.Santali, "ᱵᱤᱡᱽᱞᱤ ᱵᱚᱛᱚᱨ ᱩᱨᱩᱢ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱠᱟᱹᱢᱤ ᱦᱚᱨᱟ ᱥᱮᱪᱮᱫ ᱯᱩᱨᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "progress.fire.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Complete practical training on hazard identification, extinguisher usage and safe evacuation." },
                { AppLanguage.Hindi,   "खतरों की पहचान, अग्निशामक यंत्र का उपयोग और सुरक्षित निकासी से संबंधित प्रशिक्षण पूरा करें।" },
                { AppLanguage.Santali, "ᱥᱮᱸᱜᱮᱞ ᱵᱚᱛᱚᱨ ᱩᱨᱩᱢ, ᱤᱬᱤᱡᱤᱡ ᱵᱮᱵᱷᱟᱨ ᱟᱨ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱚᱰᱚᱠ ᱥᱮᱪᱮᱫ ᱯᱩᱨᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "progress.gas.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Complete training on toxic gas detection, confined space procedures and PPE usage." },
                { AppLanguage.Hindi,   "खतरनाक गैसों की पहचान, सीमित स्थानों की सुरक्षा प्रक्रियाएं और PPE के उपयोग से संबंधित प्रशिक्षण पूरा करें।" },
                { AppLanguage.Santali, "ᱵᱤᱥ ᱦᱚᱭ ᱩᱨᱩᱢ, ᱥᱤᱢᱤᱛ ᱡᱟᱭᱜᱟ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱱᱤᱭᱚᱢ ᱟᱨ PPE ᱵᱮᱵᱷᱟᱨ ᱥᱮᱪᱮᱫ ᱯᱩᱨᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "progress.headerLastAssessment", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Last Assessment" },
                { AppLanguage.Hindi,   "अंतिम मूल्यांकन" },
                { AppLanguage.Santali, "ᱢᱩᱪᱟᱹᱫ ᱵᱤᱰᱟᱹᱣ" } } },
            { "progress.headerModule", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Module" },
                { AppLanguage.Hindi,   "मॉड्यूल" },
                { AppLanguage.Santali, "ᱢᱚᱰᱩᱞ" } } },
            { "progress.headerRetentionScore", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Retention Score" },
                { AppLanguage.Hindi,   "प्रतिधारण स्कोर" },
                { AppLanguage.Santali, "ᱫᱚᱦᱚ ᱮᱞ" } } },
            { "progress.hoursSpent", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Hours" },
                { AppLanguage.Hindi,   "प्रशिक्षण के कुल घंटे" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱜᱷᱟᱱᱴᱟ" } } },
            { "progress.inProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "In Progress" },
                { AppLanguage.Hindi,   "प्रगति पर" },
                { AppLanguage.Santali, "ᱪᱟᱞᱟᱜ ᱠᱟᱱᱟ" } } },
            { "progress.machinery.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Complete training on safe machine operation, guarding and maintenance protocols." },
                { AppLanguage.Hindi,   "मशीनों के सुरक्षित उपयोग और रखरखाव से संबंधित प्रशिक्षण पूरा करें।" },
                { AppLanguage.Santali, "ᱢᱤᱥᱤᱱ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱵᱮᱵᱷᱟᱨ ᱟᱨ ᱡᱚᱛᱚᱱ ᱥᱮᱪᱮᱫ ᱯᱩᱨᱟᱹᱣ ᱢᱮ ᱾" } } },
            { "progress.moduleStatusBreakdown", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Module Status Breakdown" },
                { AppLanguage.Hindi,   "मॉड्यूल प्रगति विवरण" },
                { AppLanguage.Santali, "ᱢᱚᱰᱩᱞ ᱦᱟᱞᱚᱛ ᱵᱤᱵᱚᱨᱚᱱ" } } },
            { "progress.modulesSummary", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "{0} of {1} Modules Completed" },
                { AppLanguage.Hindi,   "{1} में से {0} मॉड्यूल पूरा" },
                { AppLanguage.Santali, "{1} ᱨᱮ {0} ᱢᱚᱰᱩᱞ ᱯᱩᱨᱟᱹᱣ ᱮᱱᱟ" } } },
            { "progress.notAvailable", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Not Available" },
                { AppLanguage.Hindi,   "उपलब्ध नहीं है" },
                { AppLanguage.Santali, "ᱵᱟᱹᱱᱩᱜᱼᱟ" } } },
            { "progress.notStarted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Not Started" },
                { AppLanguage.Hindi,   "अभी शुरू नहीं किया गया" },
                { AppLanguage.Santali, "ᱵᱟᱝ ᱮᱦᱚᱵ ᱟᱠᱟᱱᱟ" } } },
            { "progress.overallScore", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Average Safety Score" },
                { AppLanguage.Hindi,   "औसत सुरक्षा स्कोर" },
                { AppLanguage.Santali, "ᱜᱩᱸᱴ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱮᱞ" } } },
            { "progress.passedStatus", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Passed (Qualified)" },
                { AppLanguage.Hindi,   "सफल (उत्तीर्ण)" },
                { AppLanguage.Santali, "ᱯᱟᱥ ᱮᱱᱟ (ᱥᱟᱹᱵᱤᱛ)" } } },
            { "progress.retentionTableTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Knowledge Retention & Performance" },
                { AppLanguage.Hindi,   "सुझाव और अगला कदम" },
                { AppLanguage.Santali, "ᱜᱮᱭᱟᱱ ᱫᱚᱦᱚ ᱟᱨ ᱠᱟᱹᱢᱤ ᱦᱚᱨᱟ" } } },
            { "progress.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Review safety competencies, module completions, and assessment scores" },
                { AppLanguage.Hindi,   "अपने प्रशिक्षण की प्रगति देखें और अगले चरण की ओर बढ़ते रहें" },
                { AppLanguage.Santali, "ᱟᱢᱟᱜ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱫᱟᱲᱮ, ᱢᱚᱰᱩᱞ ᱯᱩᱨᱟᱹᱣ ᱟᱨ ᱵᱤᱰᱟᱹᱣ ᱮᱞ ᱠᱚ ᱧᱮᱞ ᱢᱮ" } } },
            { "progress.summaryTip", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Recommendation: Re-practice modules within 30 days for annual safety recertification." },
                { AppLanguage.Hindi,   "सभी मॉड्यूल पूरा करके अपनी सुरक्षा जागरूकता और कौशल को और मजबूत बनाएं।" },
                { AppLanguage.Santali, "ᱫᱤᱥᱟᱹ: ᱥᱮᱨᱢᱟᱠᱤᱭᱟᱹ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱞᱟᱹᱜᱤᱫ ᱓᱐ ᱢᱟᱦᱟᱸ ᱵᱷᱤᱛᱨᱤ ᱨᱮ ᱫᱚᱦᱲᱟ ᱨᱤᱦᱟᱨᱥᱟᱞ ᱢᱮ ᱾" } } },
            { "progress.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Progress" },
                { AppLanguage.Hindi,   "मेरी प्रगति" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱞᱟᱦᱟᱱᱛᱤ" } } },
            { "result.accuracyLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Accuracy" },
                { AppLanguage.Hindi,   "सटीकता" },
                { AppLanguage.Santali, "ᱴᱷᱤᱠ ᱦᱟᱹᱴᱤᱧ" } } },
            { "result.competencyBreakdown", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Competency Breakdown" },
                { AppLanguage.Hindi,   "दक्षता विवरण" },
                { AppLanguage.Santali, "ᱫᱟᱲᱮ ᱵᱤᱵᱚᱨᱚᱬ" } } },
            { "result.competent", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Competent & Qualified" },
                { AppLanguage.Hindi,   "योग्य एवं सक्षम" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱹᱵᱤᱛ ᱠᱟᱹᱢᱤᱭᱟᱹ" } } },
            { "result.continuousImprovement", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Continuous Safety Improvement" },
                { AppLanguage.Hindi,   "निरंतर सुरक्षा सुधार" },
                { AppLanguage.Santali, "ᱞᱮᱛᱟᱲ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱩᱫᱷᱟᱹᱨ" } } },
            { "result.criticalErrorsLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Critical Errors" },
                { AppLanguage.Hindi,   "गंभीर त्रुटियाँ" },
                { AppLanguage.Santali, "ᱵᱟᱹᱲᱤᱡ ᱵᱷᱩᱞ ᱠᱚ" } } },
            { "result.criticalViolation", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Critical safety violation recorded ({0}). Mandatory re-practice required." },
                { AppLanguage.Hindi,   "गंभीर सुरक्षा उल्लंघन दर्ज ({0})। मानक संचालन के अनुसार अनिवार्य पुनः अभ्यास आवश्यक है।" },
                { AppLanguage.Santali, "ᱵᱟᱹᱲᱤᱡ ᱵᱷᱩᱞ: {0}" } } },
            { "result.passed", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Assessment Passed!" },
                { AppLanguage.Hindi,   "मूल्यांकन में सफल!" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱨᱮ ᱯᱟᱥ ᱮᱱᱟ!" } } },
            { "result.recommendation", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Recommendation" },
                { AppLanguage.Hindi,   "अनुशंसा" },
                { AppLanguage.Santali, "ᱫᱤᱥᱟᱹ ᱩᱫᱩᱜ" } } },
            { "result.repeatedMistakes", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Repeated Mistakes" },
                { AppLanguage.Hindi,   "बार-बार की गई गलतियाँ" },
                { AppLanguage.Santali, "ᱵᱟᱨ ᱵᱟᱨ ᱦᱩᱭ ᱟᱠᱟᱱ ᱵᱷᱩᱞ" } } },
            { "result.retrain", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Retraining Required" },
                { AppLanguage.Hindi,   "पुनः प्रशिक्षण आवश्यक" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱥᱮᱪᱮᱫ ᱞᱟᱹᱠᱛᱤᱭᱟ" } } },
            { "result.retrainingRequired", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Retraining Required" },
                { AppLanguage.Hindi,   "पुनः प्रशिक्षण आवश्यक" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱥᱮᱪᱮᱫ ᱞᱟᱹᱠᱛᱤᱭᱟ" } } },
            { "result.returnHome", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Return to Dashboard" },
                { AppLanguage.Hindi,   "डैशबोर्ड पर लौटें" },
                { AppLanguage.Santali, "ᱰᱮᱥᱵᱚᱨᱰ ᱛᱮ ᱨᱩᱣᱟᱹᱲ ᱢᱮ" } } },
            { "result.scoreBelowThreshold", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Score below 80% threshold. Targeted drill practice recommended." },
                { AppLanguage.Hindi,   "प्राप्तांक 80% की न्यूनतम योग्यता सीमा से कम है। लक्षित ड्रिल अभ्यास की अनुशंसा की जाती है।" },
                { AppLanguage.Santali, "ᱮᱞ ᱫᱚ ᱯᱟᱥ ᱮᱞ (75%) ᱠᱷᱚᱱ ᱠᱚᱢ ᱜᱮᱭᱟ ᱾" } } },
            { "result.scoreLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Score" },
                { AppLanguage.Hindi,   "स्कोर" },
                { AppLanguage.Santali, "ᱯᱨᱟᱯᱛᱟᱝᱠ" } } },
            { "result.sopCompliance", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Demonstrated compliance with Ministry of Mines Industrial Safety SOP." },
                { AppLanguage.Hindi,   "खान मंत्रालय एवं औद्योगिक सुरक्षा मानक संचालन प्रक्रिया (SOP) का पूर्ण अनुपालन प्रदर्शित किया गया।" },
                { AppLanguage.Santali, "SOP ᱢᱟᱱᱟᱣ: {0}%" } } },
            { "result.startRetraining", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Start Retraining Drill" },
                { AppLanguage.Hindi,   "पुनः प्रशिक्षण शुरू करें" },
                { AppLanguage.Santali, "ᱫᱚᱦᱲᱟ ᱥᱮᱪᱮᱫ ᱮᱦᱚᱵ ᱢᱮ" } } },
            { "result.strongAreas", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Strong Areas" },
                { AppLanguage.Hindi,   "सुदृढ़ क्षेत्र" },
                { AppLanguage.Santali, "ᱠᱮᱴᱮᱡ ᱦᱟᱹᱴᱤᱧ" } } },
            { "result.timeSpentLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Time Elapsed" },
                { AppLanguage.Hindi,   "कुल समय" },
                { AppLanguage.Santali, "ᱯᱟᱨᱚᱢ ᱮᱱ ᱚᱠᱛᱚ" } } },
            { "result.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Assessment Result" },
                { AppLanguage.Hindi,   "मूल्यांकन परिणाम" },
                { AppLanguage.Santali, "ᱵᱤᱰᱟᱹᱣ ᱚᱨᱡᱚ" } } },
            { "result.viewCertificate", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View Certificate" },
                { AppLanguage.Hindi,   "प्रमाणपत्र देखें" },
                { AppLanguage.Santali, "ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱧᱮᱞ ᱢᱮ" } } },
            { "result.viewOfficialCertificate", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View Official Certificate" },
                { AppLanguage.Hindi,   "आधिकारिक प्रमाणपत्र देखें" },
                { AppLanguage.Santali, "ᱥᱚᱨᱠᱟᱨᱤ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱧᱮᱞ ᱢᱮ" } } },
            { "result.weakAreas", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Areas for Improvement" },
                { AppLanguage.Hindi,   "सुधार की आवश्यकता वाले क्षेत्र" },
                { AppLanguage.Santali, "ᱥᱩᱫᱷᱟᱹᱨ ᱞᱟᱹᱠᱛᱤ ᱦᱟᱹᱴᱤᱧ" } } },
            { "result.zeroCriticalErrors", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Zero critical errors maintained across all industrial procedures." },
                { AppLanguage.Hindi,   "सभी औद्योगिक प्रक्रियाओं में शून्य गंभीर त्रुटियां रखी गईं।" },
                { AppLanguage.Santali, "ᱡᱚᱛᱚ ᱠᱟᱹᱨᱜᱟᱲ ᱠᱟᱹᱢᱤ ᱨᱮ ᱢᱤᱫᱴᱟᱝ ᱦᱚᱸ ᱵᱷᱩᱞ ᱵᱟᱝ ᱦᱩᱭ ᱟᱠᱟᱱᱟ ᱾" } } },
            { "scenario.s1.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "1. Electrical Panel Fire" },
                { AppLanguage.Hindi,   "1. विद्युत पैनल में आग" },
                { AppLanguage.Santali, "᱑. ᱵᱤᱡᱽᱞᱤ ᱯᱮᱱᱮᱞ ᱨᱮ ᱥᱮᱸᱜᱮᱞ" } } },
            { "scenario.s2.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "2. Chemical Storage Fire" },
                { AppLanguage.Hindi,   "2. रासायनिक भंडारण में आग" },
                { AppLanguage.Santali, "2. ᱨᱟᱥᱟᱭᱚᱱᱤᱠ ᱫᱚᱦᱚ ᱴᱷᱟᱶ ᱨᱮ ᱥᱮᱸᱜᱮᱞ" } } },
            { "scenario.s3.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "3. Conveyor Belt Fire" },
                { AppLanguage.Hindi,   "3. कन्वेयर बेल्ट में आग" },
                { AppLanguage.Santali, "3. ᱠᱚᱱᱵᱷᱮᱭᱟᱨ ᱵᱮᱞᱴ ᱨᱮ ᱥᱮᱸᱜᱮᱞ" } } },
            { "scenario.tip", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tip: For best results, start with Scenario 1." },
                { AppLanguage.Hindi,   "सुझाव: सर्वोत्तम अभ्यास के लिए परिदृश्य 1 से शुरुआत करें।" },
                { AppLanguage.Santali, "ᱫᱤᱥᱟᱹ: ᱵᱷᱟᱹᱜᱤ ᱥᱮᱪᱮᱫ ᱞᱟᱹᱜᱤᱫ, ᱯᱚᱨᱚᱠ ᱑ ᱠᱷᱚᱱ ᱮᱦᱚᱵ ᱢᱮ ᱾" } } },
            { "scenario.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Practice Scenario" },
                { AppLanguage.Hindi,   "अभ्यास परिदृश्य चुनें" },
                { AppLanguage.Santali, "ᱥᱮᱪᱮᱫ ᱯᱚᱨᱚᱠ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "scenarioSelection.bannerDesc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select an emergency situation below to begin mobile AR assessment. Zero critical errors are required for compliance certification." },
                { AppLanguage.Hindi,   "मोबाइल एआर मूल्यांकन शुरू करने के लिए नीचे दी गई आपातकालीन स्थिति का चयन करें। अनुपालन प्रमाणन के लिए शून्य गंभीर त्रुटियां अनिवार्य हैं।" },
                { AppLanguage.Santali, "ᱢᱚᱵᱟᱭᱤᱞ ᱮ.ᱟᱨ. ᱵᱤᱰᱟᱹᱣ ᱮᱦᱚᱵ ᱞᱟᱹᱜᱤᱫ ᱞᱟᱛᱟᱨ ᱨᱮᱱᱟᱜ ᱵᱚᱛᱚᱨ ᱯᱚᱨᱤᱥᱛᱷᱤᱛᱤ ᱵᱟᱪᱷᱟᱣ ᱢᱮ ᱾ ᱥᱟᱠᱷᱤ ᱥᱟᱠᱟᱢ ᱧᱟᱢ ᱞᱟᱹᱜᱤᱫ ᱢᱤᱫᱴᱟᱝ ᱦᱚᱸ ᱵᱷᱩᱞ ᱵᱟᱝ ᱦᱩᱭᱩᱜ ᱞᱟᱹᱠᱛᱤ ᱾" } } },
            { "scenarioSelection.bannerTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Standard Industrial Scenarios" },
                { AppLanguage.Hindi,   "मानक औद्योगिक परिदृश्य" },
                { AppLanguage.Santali, "ᱢᱟᱱᱚᱠ ᱠᱟᱹᱨᱜᱟᱲ ᱯᱚᱨᱤᱥᱛᱷᱤᱛᱤ" } } },
            { "scenarioSelection.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Scenario" },
                { AppLanguage.Hindi,   "परिदृश्य चुनें" },
                { AppLanguage.Santali, "ᱯᱚᱨᱤᱥᱛᱷᱤᱛᱤ ᱵᱟᱪᱷᱟᱣ ᱢᱮ" } } },
            { "settings.audioAlerts", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safety Audio Alerts" },
                { AppLanguage.Hindi,   "सुरक्षा ध्वनि अलर्ट" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱟᱰᱮ ᱦᱩᱥᱤᱭᱟᱹᱨ" } } },
            { "settings.autoSync", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Auto-Sync when Online" },
                { AppLanguage.Hindi,   "ऑनलाइन होने पर स्वतः सिंक करें" },
                { AppLanguage.Santali, "ᱚᱱᱞᱟᱭᱤᱱ ᱨᱮ ᱟᱯᱱᱟᱨᱛᱮ ᱥᱤᱝᱠ" } } },
            { "settings.haptics", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Haptic Feedback" },
                { AppLanguage.Hindi,   "हैप्टिक कंपन" },
                { AppLanguage.Santali, "ᱦᱟᱯᱴᱤᱠ ᱞᱟᱲᱟᱣ" } } },
            { "settings.highContrast", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "High Contrast Mode" },
                { AppLanguage.Hindi,   "उच्च कंट्रास्ट मोड" },
                { AppLanguage.Santali, "ᱩᱥᱩᱞ ᱠᱚᱱᱴᱨᱟᱥᱴ ᱢᱳᱰ" } } },
            { "settings.privacyNote", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "All training records are stored securely on-device with offline-first encryption." },
                { AppLanguage.Hindi,   "सभी प्रशिक्षण रिकॉर्ड ऑफलाइन-प्रथम एन्क्रिप्शन के साथ डिवाइस पर सुरक्षित संग्रहीत हैं।" },
                { AppLanguage.Santali, "ᱡᱚᱛᱚ ᱥᱮᱪᱮᱫ ᱨᱮᱠᱚᱨᱰ ᱱᱚᱣᱟ ᱥᱟᱫᱷᱚᱱ ᱨᱮ ᱨᱩᱠᱷᱤᱭᱟᱹ ᱢᱮᱱᱟᱜᱼᱟ ᱾" } } },
            { "settings.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "App Settings" },
                { AppLanguage.Hindi,   "ऐप सेटिंग्स" },
                { AppLanguage.Santali, "ᱮᱯ ᱥᱟᱡᱟᱣ" } } },
            { "settings.version", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SurakshaAR v1.0.0 (Build 6000.6)" },
                { AppLanguage.Hindi,   "SurakshaAR v1.0.0 (बिल्ड 6000.6)" },
                { AppLanguage.Santali, "ᱥᱩᱨᱚᱠᱷᱟ ᱮ.ᱟᱨ. v1.0.0 (ᱵᱤᱞᱰ 6000.6)" } } },
            { "splash.appName", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SurakshaAR" },
                { AppLanguage.Hindi,   "सुरक्षाAR" },
                { AppLanguage.Santali, "ᱥᱩᱨᱚᱠᱷᱟ ᱮ.ᱟᱨ." } } },
            { "splash.footer", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SAFE WORKERS, STRONGER INDIA" },
                { AppLanguage.Hindi,   "सुरक्षित श्रमिक, सशक्त भारत" },
                { AppLanguage.Santali, "ᱨᱩᱠᱷᱤᱭᱟᱹ ᱠᱟᱹᱢᱤᱭᱟᱹ, ᱠᱮᱴᱮᱡ ᱥᱤᱧᱚᱛ" } } },
            { "splash.tagline", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "AR-Based Safety Training for Industrial Workers" },
                { AppLanguage.Hindi,   "औद्योगिक श्रमिकों के लिए एआर-आधारित सुरक्षा प्रशिक्षण" },
                { AppLanguage.Santali, "ᱠᱷᱟᱫᱟᱱ ᱟᱨ ᱠᱟᱹᱨᱜᱟᱲ ᱠᱟᱹᱢᱤᱭᱟᱹ ᱠᱚ ᱞᱟᱹᱜᱤᱫ ᱮ.ᱟᱨ. ᱨᱩᱠᱷᱤᱭᱟᱹ ᱥᱮᱪᱮᱫ" } } }
        };
    }
}
