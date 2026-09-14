using System.Collections.Generic;
using SurakshaAR.Data;

namespace SurakshaAR.Localization
{
    /// <summary>
    /// String table for all in-scope screens in English, Hindi, and Santali.
    /// Keys use dot-notation: "screen.element".
    /// </summary>
    public static class LocalizedStrings
    {
        public static readonly Dictionary<string, Dictionary<AppLanguage, string>> Table =
            new Dictionary<string, Dictionary<AppLanguage, string>>
        {
            // ---------------- Splash ----------------
            { "splash.appName", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SurakshaAR" },
                { AppLanguage.Hindi, "सुरक्षाएआर" },
                { AppLanguage.Santali, "SurakshaAR" } } },
            { "splash.tagline", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "AR-Based Safety Training for Industrial Workers" },
                { AppLanguage.Hindi, "औद्योगिक श्रमिकों के लिए एआर-आधारित सुरक्षा प्रशिक्षण" },
                { AppLanguage.Santali, "Industrial dokoin do AR-lekha safety sikhaoli" } } },
            { "splash.footer", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "SAFE WORKERS, STRONGER INDIA" },
                { AppLanguage.Hindi, "सुरक्षित श्रमिक, समृद्ध भारत" },
                { AppLanguage.Santali, "Suraksha dokoin, gadhan India" } } },

            // ---------------- Language Selection ----------------
            { "language.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Language" },
                { AppLanguage.Hindi, "भाषा चुनें" },
                { AppLanguage.Santali, "Rasika Ol Bachao" } } },
            { "language.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Choose your preferred language to continue" },
                { AppLanguage.Hindi, "जारी रखने के लिए अपनी पसंदीदा भाषा चुनें" },
                { AppLanguage.Santali, "Chitaka rasikate lagit apna manoen ol bachao" } } },
            { "language.skip", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Skip" },
                { AppLanguage.Hindi, "छोड़ें" },
                { AppLanguage.Santali, "Chhoro" } } },
            { "language.continue", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Continue" },
                { AppLanguage.Hindi, "जारी रखें" },
                { AppLanguage.Santali, "Lagao" } } },
            { "language.bannerQuote", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn together for a safer tomorrow" },
                { AppLanguage.Hindi, "एक सुरक्षित कल के लिए साथ सीखें" },
                { AppLanguage.Santali, "Mitarte sikhao bhali gapa lagit" } } },

            // ---------------- Worker Access / Login ----------------
            { "login.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker Access" },
                { AppLanguage.Hindi, "श्रमिक प्रवेश" },
                { AppLanguage.Santali, "Kamiya Bolo" } } },
            { "login.workerLogin", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker Login" },
                { AppLanguage.Hindi, "श्रमिक लॉगिन" },
                { AppLanguage.Santali, "Kamiya Login" } } },
            { "login.guestMode", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Guest Mode" },
                { AppLanguage.Hindi, "अतिथि मोड" },
                { AppLanguage.Santali, "Pera Mode" } } },
            { "login.employeeId", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Employee ID" },
                { AppLanguage.Hindi, "कर्मचारी आईडी" },
                { AppLanguage.Santali, "Employee ID" } } },
            { "login.employeeIdPlaceholder", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Enter your Employee ID" },
                { AppLanguage.Hindi, "अपनी कर्मचारी आईडी दर्ज करें" },
                { AppLanguage.Santali, "Apna Employee ID ol me" } } },
            { "login.password", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Password" },
                { AppLanguage.Hindi, "पासवर्ड" },
                { AppLanguage.Santali, "Password" } } },
            { "login.passwordPlaceholder", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Enter your password" },
                { AppLanguage.Hindi, "अपना पासवर्ड दर्ज करें" },
                { AppLanguage.Santali, "Apna password ol me" } } },
            { "login.rememberMe", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remember me" },
                { AppLanguage.Hindi, "मुझे याद रखें" },
                { AppLanguage.Santali, "Inye disha dohoen" } } },
            { "login.forgotPassword", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Forgot Password?" },
                { AppLanguage.Hindi, "पासवर्ड भूल गए?" },
                { AppLanguage.Santali, "Password Hirinj ena?" } } },
            { "login.submit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Login" },
                { AppLanguage.Hindi, "लॉगिन करें" },
                { AppLanguage.Santali, "Login Me" } } },
            { "login.or", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "OR" },
                { AppLanguage.Hindi, "या" },
                { AppLanguage.Santali, "SE" } } },
            { "login.qrCode", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Login with QR Code" },
                { AppLanguage.Hindi, "क्यूआर कोड से लॉगिन करें" },
                { AppLanguage.Santali, "QR Code te Login Me" } } },
            { "login.tagline", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safe Mines Stronger Communities" },
                { AppLanguage.Hindi, "सुरक्षित खदानें, सशक्त समुदाय" },
                { AppLanguage.Santali, "Suraksha Khan, Ketej Gaon" } } },

            // ---------------- Home Dashboard ----------------
            { "home.greeting", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Namaste," },
                { AppLanguage.Hindi, "नमस्ते," },
                { AppLanguage.Santali, "Johar," } } },
            { "home.roleLabel", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mine Worker" },
                { AppLanguage.Hindi, "खान श्रमिक" },
                { AppLanguage.Santali, "Khan Kami" } } },
            { "home.heroTitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Every Worker Safe, Every Family Strong" },
                { AppLanguage.Hindi, "हर श्रमिक सुरक्षित, हर परिवार मजबूत" },
                { AppLanguage.Santali, "Sabaren Kami Suraksha, Sabaren Kutum Gadhan" } } },
            { "home.modulesSection", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Modules" },
                { AppLanguage.Hindi, "प्रशिक्षण मॉड्यूल" },
                { AppLanguage.Santali, "Sikhao Module" } } },
            { "home.viewAll", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View All" },
                { AppLanguage.Hindi, "सभी देखें" },
                { AppLanguage.Santali, "Joto Nel Me" } } },
            { "home.overallProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Overall Progress" },
                { AppLanguage.Hindi, "समग्र प्रगति" },
                { AppLanguage.Santali, "Muth Progress" } } },
            { "home.modulesCompleted", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "3 / 5 Modules Completed" },
                { AppLanguage.Hindi, "3 / 5 मॉड्यूल पूर्ण" },
                { AppLanguage.Santali, "3 / 5 Module Pura Ena" } } },
            { "home.navHome", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Home" }, { AppLanguage.Hindi, "होम" }, { AppLanguage.Santali, "Oda" } } },
            { "home.navLearn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn" }, { AppLanguage.Hindi, "सीखें" }, { AppLanguage.Santali, "Sikhao" } } },
            { "home.navProgress", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "My Progress" }, { AppLanguage.Hindi, "मेरी प्रगति" }, { AppLanguage.Santali, "Ini Progress" } } },
            { "home.navCertificates", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificates" }, { AppLanguage.Hindi, "प्रमाणपत्र" }, { AppLanguage.Santali, "Certificate" } } },
            { "home.comingSoon", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Coming Soon" }, { AppLanguage.Hindi, "जल्द आ रहा है" }, { AppLanguage.Santali, "Ayo Tayar" } } },

            // ---------------- Module Selection ----------------
            { "moduleSelection.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Training Module" },
                { AppLanguage.Hindi, "प्रशिक्षण मॉड्यूल चुनें" },
                { AppLanguage.Santali, "Sikhaoli Module Bachao" } } },

            // ---------------- Module titles / descriptions ----------------
            { "module.fire.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Fire & Explosion Response" },
                { AppLanguage.Hindi, "आग एवं विस्फोट प्रतिक्रिया" },
                { AppLanguage.Santali, "Sengel ar Explosion Reaction" } } },
            { "module.fire.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn to identify fire hazards, use fire extinguishers, activate alarms and evacuate safely in real-world industrial environments." },
                { AppLanguage.Hindi, "आग के खतरों को पहचानें, अग्निशामक का उपयोग करें, अलार्म बजाएं और औद्योगिक वातावरण में सुरक्षित निकासी सीखें।" },
                { AppLanguage.Santali, "Sengel bipod cinha, tahen extinguisher bachao ar suraksha lekha ruar sikhao." } } },
            { "module.gas.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Gas Leak & Confined Space" },
                { AppLanguage.Hindi, "गैस रिसाव एवं सीमित स्थान" },
                { AppLanguage.Santali, "Gas Beter ar Bandh Jayga" } } },
            { "module.gas.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn to identify hazard zones, select PPE and follow confined space safety procedures." },
                { AppLanguage.Hindi, "खतरे वाले क्षेत्रों की पहचान, पीपीई चयन और सीमित स्थान सुरक्षा प्रक्रिया सीखें।" },
                { AppLanguage.Santali, "Bipod jayga cinha, PPE bachao ar bandh jayga suraksha niyom sikhao." } } },
            { "module.machinery.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Machinery Safety (Coming Soon)" },
                { AppLanguage.Hindi, "मशीनरी सुरक्षा (जल्द आ रहा है)" },
                { AppLanguage.Santali, "Machinery Suraksha (Ayo Tayar)" } } },
            { "module.machinery.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Learn safe operation and hazard awareness around industrial machinery and moving parts." },
                { AppLanguage.Hindi, "औद्योगिक मशीनरी और गतिशील पुर्जों के आसपास सुरक्षित संचालन एवं खतरा जागरूकता सीखें।" },
                { AppLanguage.Santali, "Machinery ar chalao bhag re suraksha kaj ar bipod cinha sikhao." } } },
            { "module.electrical.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Electrical Safety (Coming Soon)" },
                { AppLanguage.Hindi, "विद्युत सुरक्षा (जल्द आ रहा है)" },
                { AppLanguage.Santali, "Bijli Suraksha (Ayo Tayar)" } } },
            { "module.electrical.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Coming soon." }, { AppLanguage.Hindi, "जल्द आ रहा है।" }, { AppLanguage.Santali, "Ayo tayar." } } },

            // ---------------- Fire learning points ----------------
            { "module.fire.learn.1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify different types of fires" }, { AppLanguage.Hindi, "विभिन्न प्रकार की आग को पहचानें" }, { AppLanguage.Santali, "Judan sengel bipod cinha" } } },
            { "module.fire.learn.2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Locate and use safety equipment" }, { AppLanguage.Hindi, "सुरक्षा उपकरण खोजें और उपयोग करें" }, { AppLanguage.Santali, "Suraksha saman khonja ar chalao" } } },
            { "module.fire.learn.3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Follow correct response procedures" }, { AppLanguage.Hindi, "सही प्रतिक्रिया प्रक्रियाओं का पालन करें" }, { AppLanguage.Santali, "Thik reak niyom mana me" } } },
            { "module.fire.learn.4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Find safe exit routes" }, { AppLanguage.Hindi, "सुरक्षित निकास मार्ग खोजें" }, { AppLanguage.Santali, "Suraksha rasta panja" } } },

            // ---------------- Difficulty ----------------
            { "difficulty.beginner", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Beginner Level" }, { AppLanguage.Hindi, "प्रारंभिक स्तर" }, { AppLanguage.Santali, "Etkhan Level" } } },
            { "difficulty.intermediate", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Intermediate Level" }, { AppLanguage.Hindi, "मध्यवर्ती स्तर" }, { AppLanguage.Santali, "Majhim Level" } } },
            { "difficulty.advanced", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Advanced Level" }, { AppLanguage.Hindi, "उन्नत स्तर" }, { AppLanguage.Santali, "Bare Level" } } },

            // ---------------- Module Detail ----------------
            { "moduleDetail.whatYouLearn", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "What you will learn:" },
                { AppLanguage.Hindi, "आप क्या सीखेंगे:" },
                { AppLanguage.Santali, "Am chet sikhaben:" } } },
            { "moduleDetail.startModule", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Start Module" },
                { AppLanguage.Hindi, "मॉड्यूल शुरू करें" },
                { AppLanguage.Santali, "Module Etaoha" } } },
            { "moduleDetail.scenarios", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Scenarios" }, { AppLanguage.Hindi, "परिदृश्य" }, { AppLanguage.Santali, "Scenarios" } } },
            { "moduleDetail.duration", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "15–20 min" }, { AppLanguage.Hindi, "15–20 मिनट" }, { AppLanguage.Santali, "15–20 min" } } },

            // ---------------- Scenario Selection ----------------
            { "scenario.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Scenario" },
                { AppLanguage.Hindi, "परिदृश्य चुनें" },
                { AppLanguage.Santali, "Scenario Bachao" } } },
            { "scenario.tip", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Tip: Start with Scenario 1 if you are a first-time user." },
                { AppLanguage.Hindi, "सुझाव: यदि आप पहली बार उपयोग कर रहे हैं तो परिदृश्य 1 से शुरू करें।" },
                { AppLanguage.Santali, "Disa: Pahil dhao khanjom Scenario 1 etahob me." } } },
            { "scenario.s1.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "1. Electrical Panel Fire" },
                { AppLanguage.Hindi, "1. इलेक्ट्रिकल पैनल में आग" },
                { AppLanguage.Santali, "1. Bijli Panel Sengel" } } },
            { "scenario.s2.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "2. Chemical Storage Fire" },
                { AppLanguage.Hindi, "2. रासायनिक भंडारण में आग" },
                { AppLanguage.Santali, "2. Chemical Godown Sengel" } } },
            { "scenario.s3.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "3. Workshop Fire" },
                { AppLanguage.Hindi, "3. कार्यशाला में आग" },
                { AppLanguage.Santali, "3. Karkhana Sengel" } } },

            // ---------------- Assessment ----------------
            { "assessment.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safety Assessment" },
                { AppLanguage.Hindi, "सुरक्षा मूल्यांकन" },
                { AppLanguage.Santali, "Suraksha Bichar" } } },
            { "assessment.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Test your fire safety response knowledge" },
                { AppLanguage.Hindi, "अग्नि सुरक्षा प्रतिक्रिया ज्ञान का परीक्षण करें" },
                { AppLanguage.Santali, "Sengel suraksha badae porikha" } } },
            { "assessment.submit", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Submit Assessment" },
                { AppLanguage.Hindi, "मूल्यांकन जमा करें" },
                { AppLanguage.Santali, "Bichar Jama Me" } } },

            // ---------------- Result ----------------
            { "result.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Result" },
                { AppLanguage.Hindi, "प्रशिक्षण परिणाम" },
                { AppLanguage.Santali, "Sikhao Fal" } } },
            { "result.passed", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Assessment Passed!" },
                { AppLanguage.Hindi, "मूल्यांकन उत्तीर्ण!" },
                { AppLanguage.Santali, "Bichar Re Pass Ena!" } } },
            { "result.viewCertificate", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "View Certificate" },
                { AppLanguage.Hindi, "प्रमाणपत्र देखें" },
                { AppLanguage.Santali, "Certificate Nel Me" } } },
            { "result.returnHome", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Return to Dashboard" },
                { AppLanguage.Hindi, "डैशबोर्ड पर लौटें" },
                { AppLanguage.Santali, "Dashboard te Ruar Me" } } },

            // ---------------- Certificate ----------------
            { "certificate.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Certificate of Completion" },
                { AppLanguage.Hindi, "पूर्णता प्रमाण पत्र" },
                { AppLanguage.Santali, "Kamil Certificate" } } },
            { "certificate.awardedTo", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "This certificate is proudly awarded to" },
                { AppLanguage.Hindi, "यह प्रमाण पत्र गर्व से प्रदान किया जाता है" },
                { AppLanguage.Santali, "Neh certificate sarhao te chal ediya" } } },

            // ---------------- Progress ----------------
            { "progress.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Training Progress" },
                { AppLanguage.Hindi, "प्रशिक्षण प्रगति" },
                { AppLanguage.Santali, "Sikhao Paragati" } } },
            { "progress.modulesSummary", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "{0} of {1} Modules Completed" },
                { AppLanguage.Hindi, "{1} में से {0} मॉड्यूल पूर्ण" },
                { AppLanguage.Santali, "{1} rane {0} module kamil" } } },

            // ---------------- Profile Setup ----------------
            { "profile.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Create Your Profile" },
                { AppLanguage.Hindi, "अपनी प्रोफ़ाइल बनाएं" },
                { AppLanguage.Santali, "Apna Profile Thokon" } } },
            { "profile.subtitle", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Let's get to know you" },
                { AppLanguage.Hindi, "आइए आपको जानें" },
                { AppLanguage.Santali, "Amod okoy janon" } } },
            { "profile.fullName", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Full Name" },
                { AppLanguage.Hindi, "पूरा नाम" },
                { AppLanguage.Santali, "Purno Nam" } } },
            { "profile.workerId", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Worker ID" },
                { AppLanguage.Hindi, "श्रमिक आईडी" },
                { AppLanguage.Santali, "Kamiya ID" } } },
            { "profile.sector", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Sector" },
                { AppLanguage.Hindi, "क्षेत्र" },
                { AppLanguage.Santali, "Kheto" } } },
            { "profile.experience", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Experience (Years)" },
                { AppLanguage.Hindi, "अनुभव (वर्ष)" },
                { AppLanguage.Santali, "Anubho (Bosor)" } } },
            { "profile.next", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Next" },
                { AppLanguage.Hindi, "आगे" },
                { AppLanguage.Santali, "Age" } } },

            // ---------------- Mine Hazard module ----------------
            { "module.minehazard.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Mine Hazard & Environment" },
                { AppLanguage.Hindi, "माइन खतरा और पर्यावरण" },
                { AppLanguage.Santali, "Mine Khatra Ar Poribesh" } } },
            { "module.minehazard.description", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Recognize mine-specific hazards and protect the surrounding environment." },
                { AppLanguage.Hindi, "माइन-विशिष्ट खतरों को पहचानें और आसपास के पर्यावरण की सुरक्षा करें।" },
                { AppLanguage.Santali, "Mine khatra chinhe ar poribesh rakha he." } } },

            // ---------------- Fire learning point 5 ----------------
            { "module.fire.learn.5", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Report hazards and near-misses to your supervisor" },
                { AppLanguage.Hindi, "खतरों और निकट-दुर्घटनाओं की रिपोर्ट पर्यवेक्षक को करें" },
                { AppLanguage.Santali, "Khatra ar najdik durghotona supervisor ko report me" } } },

            // ---------------- Gas learning points ----------------
            { "module.gas.learn.1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Detect gas leaks using your senses and detectors" },
                { AppLanguage.Hindi, "अपनी इंद्रियों और डिटेक्टर से गैस रिसाव का पता लगाएं" },
                { AppLanguage.Santali, "Gas leak indriyo ar detector khanjom" } } },
            { "module.gas.learn.2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Wear and check respiratory protection" },
                { AppLanguage.Hindi, "श्वसन सुरक्षा पहनें और जांचें" },
                { AppLanguage.Santali, "Respiratory protection por ar check" } } },
            { "module.gas.learn.3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Follow confined space entry permits" },
                { AppLanguage.Hindi, "बंद स्थान प्रवेश परमिट का पालन करें" },
                { AppLanguage.Santali, "Confined space entry permit follow me" } } },
            { "module.gas.learn.4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Evacuate and raise the alarm safely" },
                { AppLanguage.Hindi, "सुरक्षित रूप से निकलें और अलार्म बजाएं" },
                { AppLanguage.Santali, "Suraksha te duar me ar alarm bajao" } } },

            // ---------------- Machinery learning points ----------------
            { "module.machinery.learn.1", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify machine guarding and stop controls" },
                { AppLanguage.Hindi, "मशीन गार्डिंग और स्टॉप नियंत्रण पहचानें" },
                { AppLanguage.Santali, "Machine guarding ar stop control chinhe" } } },
            { "module.machinery.learn.2", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Apply lock-out / tag-out before maintenance" },
                { AppLanguage.Hindi, "रखरखाव से पहले लॉक-आउट / टैग-आउट लगाएं" },
                { AppLanguage.Santali, "Maintenance age lock-out / tag-out lagao" } } },
            { "module.machinery.learn.3", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Keep clear of moving parts" },
                { AppLanguage.Hindi, "चलते हिस्सों से दूर रहें" },
                { AppLanguage.Santali, "Chalte hissa rane jao dur" } } },
            { "module.machinery.learn.4", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Use the correct personal protective equipment" },
                { AppLanguage.Hindi, "सही व्यक्तिगत सुरक्षा उपकरण का उपयोग करें" },
                { AppLanguage.Santali, "Thik personal protective equipment beohar" } } },
            { "module.machinery.learn.5", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Report damaged equipment immediately" },
                { AppLanguage.Hindi, "क्षतिग्रस्त उपकरण तुरंत रिपोर्ट करें" },
                { AppLanguage.Santali, "Khotigrast equipment turonto report me" } } },

            // ---------------- Fire SOP 7-Step Sequence ----------------
            { "fire.sop.step1.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Identify the Fire Hazard" },
                { AppLanguage.Hindi, "आग के खतरे की पहचान करें" },
                { AppLanguage.Santali, "Sengel khatra chinhe me" } } },
            { "fire.sop.step1.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Look around the AR scene. Locate the electrical fire source and assess surrounding dangers." },
                { AppLanguage.Hindi, "AR दृश्य में देखें। बिजली की आग के स्रोत का पता लगाएं और आसपास के खतरों का आकलन करें।" },
                { AppLanguage.Santali, "AR scene re nel me. Bijli sengel dhari pata lagaome ar khatra bujhao me." } } },
            { "fire.sop.step1.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "I Have Identified Fire Source" },
                { AppLanguage.Hindi, "मैंने आग के स्रोत की पहचान कर ली है" },
                { AppLanguage.Santali, "In sengel source chinhe keda" } } },

            { "fire.sop.step2.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Activate Fire Alarm" },
                { AppLanguage.Hindi, "फायर अलार्म सक्रिय करें" },
                { AppLanguage.Santali, "Fire alarm bajao me" } } },
            { "fire.sop.step2.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Locate the emergency manual call point and activate it immediately to warn coworkers." },
                { AppLanguage.Hindi, "आपातकालीन मैनुअल कॉल पॉइंट ढूंढें और सहकर्मियों को चेतावनी देने के लिए तुरंत सक्रिय करें।" },
                { AppLanguage.Santali, "Emergency alarm call point sendra me ar gatiko hoshiyar lagit bajao me." } } },
            { "fire.sop.step2.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Activate Emergency Alarm" },
                { AppLanguage.Hindi, "आपातकालीन अलार्म बजाएं" },
                { AppLanguage.Santali, "Emergency alarm bajao me" } } },

            { "fire.sop.step3.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select Correct Extinguisher" },
                { AppLanguage.Hindi, "सही अग्निशामक चुनें" },
                { AppLanguage.Santali, "Sahi extinguisher bachao me" } } },
            { "fire.sop.step3.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Examine the burning equipment. Select the CO2 (black band) extinguisher suited for electrical fires." },
                { AppLanguage.Hindi, "जलते हुए उपकरण की जांच करें। बिजली की आग के लिए उपयुक्त CO2 अग्निशामक चुनें।" },
                { AppLanguage.Santali, "Jolte jinish nel me. Bijli sengel lagit CO2 extinguisher bachao me." } } },
            { "fire.sop.step3.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Select CO2 Extinguisher" },
                { AppLanguage.Hindi, "CO2 अग्निशामक चुनें" },
                { AppLanguage.Santali, "CO2 extinguisher bachao me" } } },

            { "fire.sop.step4.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Remove Safety Pin" },
                { AppLanguage.Hindi, "सुरक्षा पिन निकालें" },
                { AppLanguage.Santali, "Safety pin chhadao me" } } },
            { "fire.sop.step4.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Pull the safety pin firmly to break the tamper seal and unlock the operating lever." },
                { AppLanguage.Hindi, "सील तोड़ने और हैंडल अनलॉक करने के लिए सुरक्षा पिन को मजबूती से खींचें।" },
                { AppLanguage.Santali, "Handle unlock lagit safety pin jototey te orr chhadao me." } } },
            { "fire.sop.step4.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Pull Safety Pin" },
                { AppLanguage.Hindi, "सुरक्षा पिन खींचें" },
                { AppLanguage.Santali, "Safety pin orr me" } } },

            { "fire.sop.step5.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim at Fire Base" },
                { AppLanguage.Hindi, "आग के आधार पर निशाना साधें" },
                { AppLanguage.Santali, "Sengel buta re nishana lagao me" } } },
            { "fire.sop.step5.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Hold the insulated horn and aim directly at the fuel base of the fire, not at the flames." },
                { AppLanguage.Hindi, "इंसुलेटेड हॉर्न पकड़ें और लपटों के बजाय सीधे आग की जड़ पर निशाना लगाएं।" },
                { AppLanguage.Santali, "Horn sab kate sengel chetan bapu sengel buta re nishana sadhao me." } } },
            { "fire.sop.step5.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Aim Horn at Base" },
                { AppLanguage.Hindi, "आधार पर निशाना लगाएं" },
                { AppLanguage.Santali, "Buta re nishana lagao me" } } },

            { "fire.sop.step6.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Extinguish the Fire" },
                { AppLanguage.Hindi, "आग बुझाएं" },
                { AppLanguage.Santali, "Sengel irij me" } } },
            { "fire.sop.step6.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Squeeze the lever and sweep side-to-side across the base until the fire is completely out." },
                { AppLanguage.Hindi, "लीवर दबाएं और आग पूरी तरह बुझने तक आधार पर दाएं-बाएं छिड़काव करें।" },
                { AppLanguage.Santali, "Lever chipi kate sengel motamoti irij dhari lenda-jojom chhirkao me." } } },
            { "fire.sop.step6.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Press Handle & Spray" },
                { AppLanguage.Hindi, "हैंडल दबाएं और स्प्रे करें" },
                { AppLanguage.Santali, "Handle chipi kate spray me" } } },

            { "fire.sop.step7.title", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Safe Evacuation" },
                { AppLanguage.Hindi, "सुरक्षित निकासी" },
                { AppLanguage.Santali, "Suraksha nikal / bahar" } } },
            { "fire.sop.step7.desc", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "The fire is out. Back away calmly and follow the emergency EXIT signs to safety." },
                { AppLanguage.Hindi, "आग बुझ गई है। शांत रहकर पीछे हटें और आपातकालीन निकास संकेतों का पालन करें।" },
                { AppLanguage.Santali, "Sengel irij ena. Dhire dhire pichhu hat kate emergency exit dohar te chalo me." } } },
            { "fire.sop.step7.action", new Dictionary<AppLanguage, string> {
                { AppLanguage.English, "Proceed to Emergency Exit" },
                { AppLanguage.Hindi, "आपातकालीन निकास की ओर बढ़ें" },
                { AppLanguage.Santali, "Emergency exit se chal me" } } }
        };
    }
}
