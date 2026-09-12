using System.Collections.Generic;
using System.Text;

namespace SurakshaAR.Localization
{
    /// <summary>
    /// Converts Hindi / Devanagari text to Romanized Hindi (Latin script)
    /// when the active Unity TextMeshPro font does not have Devanagari glyphs loaded.
    /// This prevents missing character square boxes ('□') and ensures all workers can
    /// read Hindi clearly across all mobile devices.
    /// </summary>
    public static class DevanagariTransliteration
    {
        private static readonly Dictionary<string, string> CuratedPhrases = new Dictionary<string, string>
        {
            { "भाषा चुनें", "Bhasha Chunein" },
            { "जारी रखने के लिए अपनी पसंदीदा भाषा चुनें", "Jaari rakhne ke liye apni pasandida bhasha chunein" },
            { "छोड़ें", "Chhodein" },
            { "जारी रखें", "Jaari Rakhein" },
            { "एक सुरक्षित कल के लिए साथ सीखें", "Ek surakshit kal ke liye saath seekhein" },
            { "सुरक्षाएआर", "SurakshaAR" },
            { "औद्योगिक श्रमिकों के लिए एआर-आधारित सुरक्षा प्रशिक्षण", "Audyogik shramikon ke liye AR-aadhaarit suraksha prashikshan" },
            { "सुरक्षित श्रमिक, समृद्ध भारत", "Surakshit Shramik, Samriddh Bharat" },
            { "श्रमिक प्रवेश", "Shramik Pravesh" },
            { "श्रमिक लॉगिन", "Shramik Login" },
            { "अतिथि मोड", "Guest Mode" },
            { "कर्मचारी आईडी", "Employee ID" },
            { "अपनी कर्मचारी आईडी दर्ज करें", "Apni Employee ID darj karein" },
            { "पासवर्ड", "Password" },
            { "अपना पासवर्ड दर्ज करें", "Apna password darj karein" },
            { "मुझे याद रखें", "Remember me" },
            { "पासवर्ड भूल गए?", "Forgot Password?" },
            { "लॉगिन करें", "Login" },
            { "या", "OR" },
            { "क्यूआर कोड से लॉगिन करें", "QR Code se login karein" },
            { "सुरक्षित खदानें, सशक्त समुदाय", "Surakshit khadanein, sashakt samuday" },
            { "नमस्ते,", "Namaste," },
            { "खान श्रमिक", "Mine Worker" },
            { "हर श्रमिक सुरक्षित, हर परिवार मजबूत", "Har shramik surakshit, har parivaar majboot" },
            { "प्रशिक्षण मॉड्यूल", "Prashikshan Module" },
            { "सभी देखें", "Sabhi Dekhein" },
            { "समग्र प्रगति", "Samagra Pragati" },
            { "3 / 5 मॉड्यूल पूर्ण", "3 / 5 Module Poorn" },
            { "होम", "Home" },
            { "सीखें", "Seekhein" },
            { "मेरी प्रगति", "Meri Pragati" },
            { "प्रमाणपत्र", "Certificates" },
            { "प्रोफ़ाइल", "Profile" },
            { "आग सुरक्षा और रोकथाम", "Fire Safety & Prevention" },
            { "आपातकालीन निकास", "Emergency Exit" },
            { "आग बुझाएं", "Aag Bujhaein" },
            { "लीवर दबाएं और आग पूरी तरह बुझने तक आधार पर दाएं-बाएं छिड़काव करें।", "Lever dabaein aur aag poori tarah bujhne tak aadhaar par daayein-baayein sweep karein." },
            { "हैंडल दबाएं और स्प्रे करें", "Handle dabaein aur spray karein" },
            { "सुरक्षित निकासी", "Surakshit Nikaasi" },
            { "आग बुझ गई है। शांत रहकर पीछे हटें और आपातकालीन निकास संकेतों का पालन करें।", "Aag bujh gayi hai. Shaant rahkar peechhe hatein aur emergency exit sanketon ka paalan karein." },
            { "आपातकालीन निकास की ओर बढ़ें", "Emergency exit ki or badhein" },
            { "खतरा पहचान", "Khatra Pehchaan" },
            { "अग्निशामक यंत्र खोजें", "Fire Extinguisher Khojein" },
            { "सही यंत्र चुनें", "Sahi Yantra Chunein" },
            { "सेफ्टी पिन निकालें", "Safety Pin Nikalein" },
            { "आग के आधार पर निशाना साधें", "Aag ke aadhaar par nishaana saadhein" }
        };

        private static readonly Dictionary<char, string> GlyphMap = new Dictionary<char, string>
        {
            {'अ', "a"}, {'आ', "aa"}, {'इ', "i"}, {'ई', "ee"}, {'उ', "u"}, {'ऊ', "oo"},
            {'ऋ', "ri"}, {'ए', "e"}, {'ऐ', "ai"}, {'ओ', "o"}, {'औ', "au"},
            {'क', "k"}, {'ख', "kh"}, {'ग', "g"}, {'घ', "gh"}, {'ङ', "ng"},
            {'च', "ch"}, {'छ', "chh"}, {'ज', "j"}, {'झ', "jh"}, {'ञ', "ny"},
            {'ट', "t"}, {'ठ', "th"}, {'ड', "d"}, {'ढ', "dh"}, {'ण', "n"},
            {'त', "t"}, {'थ', "th"}, {'द', "d"}, {'ध', "dh"}, {'न', "n"},
            {'प', "p"}, {'फ', "ph"}, {'ब', "b"}, {'भ', "bh"}, {'म', "m"},
            {'य', "y"}, {'र', "r"}, {'ल', "l"}, {'व', "v"}, {'श', "sh"},
            {'ष', "sh"}, {'स', "s"}, {'ह', "h"},
            {'ा', "a"}, {'ि', "i"}, {'ी', "ee"}, {'ु', "u"}, {'ू', "oo"}, {'ृ', "ri"},
            {'े', "e"}, {'ै', "ai"}, {'ो', "o"}, {'ौ', "au"}, {'्', ""},
            {'ं', "n"}, {'ँ', "n"}, {'ः', "h"}, {'़', ""},
            {'।', "."}, {'॥', "."},
            {'०', "0"}, {'१', "1"}, {'२', "2"}, {'३', "3"}, {'४', "4"},
            {'५', "5"}, {'६', "6"}, {'७', "7"}, {'८', "8"}, {'९', "9"}
        };

        private static readonly HashSet<char> Consonants = new HashSet<char>
        {
            'क', 'ख', 'ग', 'घ', 'ङ', 'च', 'छ', 'ज', 'झ', 'ञ',
            'ट', 'ठ', 'ड', 'ढ', 'ण', 'त', 'थ', 'द', 'ध', 'न',
            'प', 'फ', 'ब', 'भ', 'म', 'य', 'र', 'ल', 'व', 'श',
            'ष', 'स', 'ह'
        };

        private static readonly HashSet<char> MatrasAndModifiers = new HashSet<char>
        {
            'ा', 'ि', 'ी', 'ु', 'ू', 'ृ', 'े', 'ै', 'ो', 'ौ', '्', 'ं', 'ँ', 'ः', '़'
        };

        /// <summary>
        /// Converts Hindi text to natural Romanized script if input contains Devanagari glyphs.
        /// </summary>
        public static string Transliterate(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // 1. Check curated high-polish phrases
            if (CuratedPhrases.TryGetValue(text.Trim(), out var curated))
                return curated;

            // 2. Quick check: Does string contain any Devanagari character?
            bool hasDevanagari = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= 0x0900 && text[i] <= 0x097F)
                {
                    hasDevanagari = true;
                    break;
                }
            }
            if (!hasDevanagari) return text;

            // 3. Algorithmic phonetic transliteration
            var sb = new StringBuilder(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= 0x0900 && c <= 0x097F)
                {
                    if (GlyphMap.TryGetValue(c, out var roman))
                    {
                        sb.Append(roman);
                        // In Hindi, an inherent 'a' follows a consonant unless followed by a matra, virama, space, or punctuation
                        if (Consonants.Contains(c))
                        {
                            bool hasFollowingModifier = (i + 1 < text.Length) && MatrasAndModifiers.Contains(text[i + 1]);
                            bool isEndOfWord = (i + 1 >= text.Length) || char.IsWhiteSpace(text[i + 1]) || char.IsPunctuation(text[i + 1]);
                            if (!hasFollowingModifier && !isEndOfWord)
                            {
                                sb.Append('a');
                            }
                        }
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
