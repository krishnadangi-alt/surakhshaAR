using System;
using System.Collections.Generic;
using System.Text;

namespace SurakshaAR.Localization
{
    /// <summary>
    /// Lightweight Brahmic text shaper for Devanagari in Unity TextMeshPro.
    /// Converts Unicode Devanagari into shaped glyph representations using PUA ligatures,
    /// reorders pre-base matras (chhoti-i), handles reph and half-forms,
    /// so that Hindi renders cleanly without viramas, broken conjuncts, or detached matras.
    /// </summary>
    public static class DevanagariShaper
    {
        private static readonly Dictionary<char, char> HalfConsonants = new Dictionary<char, char>
        {
            { 'क', DevanagariPuaMap.HALF_K },
            { 'ख', DevanagariPuaMap.HALF_KH },
            { 'ग', DevanagariPuaMap.HALF_G },
            { 'घ', DevanagariPuaMap.HALF_GH },
            { 'च', DevanagariPuaMap.HALF_CH },
            { 'छ', DevanagariPuaMap.HALF_CHH },
            { 'ज', DevanagariPuaMap.HALF_J },
            { 'झ', DevanagariPuaMap.HALF_JH },
            { 'ञ', DevanagariPuaMap.HALF_NY },
            { 'ण', DevanagariPuaMap.HALF_NN },
            { 'त', DevanagariPuaMap.HALF_T },
            { 'थ', DevanagariPuaMap.HALF_TH },
            { 'ध', DevanagariPuaMap.HALF_DH },
            { 'न', DevanagariPuaMap.HALF_N },
            { 'प', DevanagariPuaMap.HALF_P },
            { 'फ', DevanagariPuaMap.HALF_PH },
            { 'ब', DevanagariPuaMap.HALF_B },
            { 'भ', DevanagariPuaMap.HALF_BH },
            { 'म', DevanagariPuaMap.HALF_M },
            { 'य', DevanagariPuaMap.HALF_Y },
            { 'ल', DevanagariPuaMap.HALF_L },
            { 'व', DevanagariPuaMap.HALF_V },
            { 'श', DevanagariPuaMap.HALF_SH },
            { 'ष', DevanagariPuaMap.HALF_SS },
            { 'स', DevanagariPuaMap.HALF_S },
            { 'ह', DevanagariPuaMap.HALF_H },
        };

        private static readonly HashSet<char> HalfGlyphSet = new HashSet<char>(HalfConsonants.Values);

        /// <summary>
        /// Checks whether the string contains any Devanagari characters or PUA glyphs.
        /// </summary>
        public static bool HasDevanagari(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= 0x0900 && text[i] <= 0x097F) return true;
                if (text[i] >= 0xE000 && text[i] <= 0xE1AF) return true;
            }
            return false;
        }

        private static readonly Dictionary<string, string> PreShapedPhrases = new Dictionary<string, string>
        {
            { "नमस्ते,", "\u0928\u092E\uE04F\u0924\u0947," },
            { "खान कार्यकर्ता", "\u0916\u093E\u0928 \u0915\u093E\u092F\uE02D\u0915\u0924\u093E\uE02D" },
            { "आईडी:", "\u0906\u0908\u0921\u0940:" },
            { "सिंक: सभी डेटा सिंक्रनाइज़ है", "\uE1E5\u0938\uE204\u0915: \u0938\u092D\u0940 \u0921\u0947\u091F\u093E \uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B \u0939\u0948" },
            { "● सिंक: सभी डेटा सिंक्रनाइज़ है", "● \uE1E5\u0938\uE204\u0915: \u0938\u092D\u0940 \u0921\u0947\u091F\u093E \uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B \u0939\u0948" },
            { "सिंक: सभी डेटा सिंक्रनाइज है", "\uE1E5\u0938\uE204\u0915: \u0938\u092D\u0940 \u0921\u0947\u091F\u093E \uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B \u0939\u0948" },
            { "● सिंक: सभी डेटा सिंक्रनाइज है", "● \uE1E5\u0938\uE204\u0915: \u0938\u092D\u0940 \u0921\u0947\u091F\u093E \uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B \u0939\u0948" },
            { "सिंक्रनाइज़ेशन पूरा हुआ", "\uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B\u0947\u0936\u0928 \u092A\u0942\u0930\u093E \uE10B\u0906" },
            { "सिंक्रनाइजेशन पूरा हुआ", "\uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B\u0947\u0936\u0928 \u092A\u0942\u0930\u093E \uE10B\u0906" },
            { "✔ सिंक्रनाइज़ेशन पूरा हुआ", "✓ \uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B\u0947\u0936\u0928 \u092A\u0942\u0930\u093E \uE10B\u0906" },
            { "✔ सिंक्रनाइजेशन पूरा हुआ", "✓ \uE1E5\u0938\uE204\uE076\u0928\u093E\u0907\u095B\u0947\u0936\u0928 \u092A\u0942\u0930\u093E \uE10B\u0906" },
            { "प्रशिक्षण मॉड्यूल", "\uE08A\uE1D9\u0936\uE02B\u0923 \u092E\u0949\uE17D\u0942\u0932" },
            { "सभी देखें >", "\u0938\u092D\u0940 \u0926\u0947\u0916\uE139 >" },
            { "सभी देखें", "\u0938\u092D\u0940 \u0926\u0947\u0916\uE139" },
            { "आग एवं विस्फोट से निपटने की प्रक्रिया", "\u0906\u0917 \u090F\u0935\u0902 \uE1D7\u0935\uE1B5\u094B\u091F \u0938\u0947 \uE1D7\u0928\u092A\u091F\u0928\u0947 \u0915\uE207 \uE08A\uE1D7\uE076\u092F\u093E" },
            { "खतरों की पहचान, अग्निशामक यंत्र का उपयोग\nऔर सुरक्षित निकासी", "\u0916\u0924\u0930\uE145 \u0915\uE207 \u092A\u0939\u091A\u093E\u0928, \u0905\uE1DC\uE031\u0928\u0936\u093E\u092E\u0915 \u092F\u0902\uE085 \u0915\u093E \u0909\u092A\u092F\u094B\u0917\n\u0914\u0930 \u0938\u0941\u0930\uE1DA\uE02B\u0924 \uE1D7\u0928\u0915\u093E\u0938\u0940" },
            { "खतरों की पहचान, अग्निशामक यंत्र का उपयोग और सुरक्षित निकासी", "\u0916\u0924\u0930\uE145 \u0915\uE207 \u092A\u0939\u091A\u093E\u0928, \u0905\uE1DC\uE031\u0928\u0936\u093E\u092E\u0915 \u092F\u0902\uE085 \u0915\u093E \u0909\u092A\u092F\u094B\u0917 \u0914\u0930 \u0938\u0941\u0930\uE1DA\uE02B\u0924 \uE1D7\u0928\u0915\u093E\u0938\u0940" },
            { "गैस रिसाव एवं सीमित स्थान", "\u0917\u0948\u0938 \uE1D5\u0930\u0938\u093E\u0935 \u090F\u0935\u0902 \u0938\u0940\uE1D8\u092E\u0924 \uE1B0\u093E\u0928" },
            { "गैसरिसाव एवं सीमित स्थान", "\u0917\u0948\u0938 \uE1D5\u0930\u0938\u093E\u0935 \u090F\u0935\u0902 \u0938\u0940\uE1D8\u092E\u0924 \uE1B0\u093E\u0928" },
            { "खतरनाक गैसों की पहचान और PPE का उपयोग", "\u0916\u0924\u0930\u0928\u093E\u0915 \u0917\u0948\u0938\uE145 \u0915\uE207 \u092A\u0939\u091A\u093E\u0928 \u0914\u0930 PPE \u0915\u093E \u0909\u092A\u092F\u094B\u0917" },
            { "मशीनरी सुरक्षा", "\u092E\u0936\u0940\u0928\u0930\u0940 \u0938\u0941\u0930\uE02B\u093E" },
            { "मशीनों का सुरक्षित उपयोग, लॉकआउट/टैगआउट\nऔर सुरक्षित संचालन", "\u092E\u0936\u0940\u0928\uE145 \u0915\u093E \u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0909\u092A\u092F\u094B\u0917, \u0932\u0949\u0915\u0906\u0909\u091F/\u091F\u0948\u0917\u0906\u0909\u091F\n\u0914\u0930 \u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0938\u0902\u091A\u093E\u0932\u0928" },
            { "मशीनों का सुरक्षित उपयोग, लॉकआउट/टैगआउट और सुरक्षित संचालन", "\u092E\u0936\u0940\u0928\uE145 \u0915\u093E \u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0909\u092A\u092F\u094B\u0917, \u0932\u0949\u0915\u0906\u0909\u091F/\u091F\u0948\u0917\u0906\u0909\u091F \u0914\u0930 \u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0938\u0902\u091A\u093E\u0932\u0928" },
            { "शुरू करें", "\u0936\u0941\uE11A \u0915\u0930\uE139" },
            { "होम", "\u0939\u094B\u092E" },
            { "सीखें", "\u0938\u0940\u0916\uE139" },
            { "मेरी प्रगति", "\u092E\u0947\u0930\u0940 \uE08A\u0917\uE1D7\u0924" },
            { "प्रमाणपत्र", "\uE08A\u092E\u093E\u0923\u092A\uE085" },
            { "हिंदी", "\uE1E3\u0939\uE204\u0926\uE205" },
            { "प्रशिक्षण मॉड्यूल चुनें", "\uE08A\uE1D9\u0936\uE02B\u0923 \u092E\u0949\uE17D\u0942\u0932 \u091A\u0941\u0928\uE139" },
            { "उपलब्ध", "\u0909\u092A\u0932\u092C\u094D\u0927" },
            { "प्रतिबंधित", "\u092A\u094D\u0930\u0924\u093F\u092C\u0902\u0927\u093F\u0924" },
            { "आईडी: EMP-PROD-CORE-001", "\u0906\u0908\u0921\u0940: EMP-PROD-CORE-001" },
            { "सुरक्षित सीखें | सुरक्षित काम करें | सुरक्षित झारखंड बनाएं", "\u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0938\u0940\u0916\uE139 | \u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0915\u093E\u092E \u0915\u0930\uE139 | \u0938\u0941\u0930\uE1DA\uE02B\u0924 \u091D\u093E\u0930\u0916\u0902\u0921 \u092C\u0928\u093E\u090F\u0902" },
            { "सुरक्षित खदानें", "\u0938\u0941\u0930\uE1DA\uE02B\u0924 \u0916\u0926\u093E\u0928\uE139" },
            { "कुशल कार्यबल", "\u0915\u0941\u0936\u0932 \u0915\u093E\u092F\uE02D\u092C\u0932" },
            { "मजबूत झारखंड", "\u092E\u091C\u092C\u0942\u0924 \u091D\u093E\u0930\u0916\u0902\u0921" },
            { "सुरक्षित झारखंड उज्ज्वल कल", "\u0938\u0941\u0930\uE1DA\uE02B\u0924 \u091D\u093E\u0930\u0916\u0902\u0921 \u0909\uE036\uE036\u0935\u0932 \u0915\u0932" }
        };

        /// <summary>
        /// Shapes a Devanagari string for rendering in TextMeshPro with NotoSansDevanagari.
        /// Non-Devanagari text (English, numbers, Santali) passes through unmodified.
        /// </summary>
        public static string Shape(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Direct check for pre-shaped phrase
            if (PreShapedPhrases.TryGetValue(text.Trim(), out string exact))
            {
                return exact;
            }

            // If the text is already shaped (contains PUA ligature characters), avoid re-shaping
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= 0xE000 && text[i] <= 0xE2FF)
                {
                    return text.Replace("✔", "✓");
                }
            }

            if (!HasDevanagari(text))
            {
                return text.Replace("✔", "✓");
            }

            // 1. Primary Full Conjuncts (Akhand and Rakar)
            string s = text;
            s = s.Replace("क्ष", DevanagariPuaMap.K_SSA.ToString());
            s = s.Replace("त्र", DevanagariPuaMap.T_RA.ToString());
            s = s.Replace("ज्ञ", DevanagariPuaMap.J_NYA.ToString());
            s = s.Replace("श्र", DevanagariPuaMap.SH_RA.ToString());
            s = s.Replace("प्र", DevanagariPuaMap.P_RA.ToString());
            s = s.Replace("क्र", DevanagariPuaMap.K_RA.ToString());
            s = s.Replace("ग्र", DevanagariPuaMap.G_RA.ToString());
            s = s.Replace("क्त", DevanagariPuaMap.K_TA.ToString());
            s = s.Replace("त्त", DevanagariPuaMap.T_TA.ToString());
            s = s.Replace("ष्ट", DevanagariPuaMap.SS_TTA.ToString());
            s = s.Replace("ष्ठ", DevanagariPuaMap.SS_TTHA.ToString());
            s = s.Replace("ड्य", DevanagariPuaMap.DD_YA.ToString());
            s = s.Replace("द्य", DevanagariPuaMap.D_YA.ToString());
            s = s.Replace("द्व", DevanagariPuaMap.D_VA.ToString());
            s = s.Replace("स्फ", DevanagariPuaMap.S_PHA.ToString());
            s = s.Replace("स्थ", DevanagariPuaMap.S_THA.ToString());
            s = s.Replace("रू", DevanagariPuaMap.R_UU.ToString());
            s = s.Replace("रु", DevanagariPuaMap.R_U.ToString());
            s = s.Replace("हुआ", $"{DevanagariPuaMap.H_U}आ");
            s = s.Replace("हु", DevanagariPuaMap.H_U.ToString());
            s = s.Replace("ें", DevanagariPuaMap.E_ANUSVARA.ToString());
            s = s.Replace("ों", DevanagariPuaMap.O_ANUSVARA.ToString());
            s = s.Replace("ज\u093C", "\u095B");

            // 2. Reph & Half-forms
            var list1 = new List<char>(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                // Reph: र + ् + consonant -> consonant + REPH
                if (i + 2 < s.Length && s[i] == 'र' && s[i + 1] == '्' && s[i + 2] != ' ' && s[i + 2] != '\n')
                {
                    char nextCons = s[i + 2];
                    if (i + 3 < s.Length && s[i + 3] == 'ं')
                    {
                        list1.Add(nextCons);
                        list1.Add(DevanagariPuaMap.REPH_ANUSVARA);
                        i += 3;
                        continue;
                    }
                    else
                    {
                        list1.Add(nextCons);
                        list1.Add(DevanagariPuaMap.REPH);
                        i += 2;
                        continue;
                    }
                }

                // Half consonant: consonant + ् -> half form
                if (i + 1 < s.Length && s[i + 1] == '्' && HalfConsonants.TryGetValue(s[i], out char halfCh))
                {
                    list1.Add(halfCh);
                    i++; // skip '्'
                    continue;
                }

                list1.Add(s[i]);
            }

            // 3. Pre-base Chhoti-I matra reordering
            // In NotoSansDevanagari glyph design, \u093F (chhoti-i) stem sits at x=0 with top curve extending forward over the consonant.
            // If consonant cluster is followed by \u093F, move \u093F to before the cluster so the arc curves over it correctly.
            var list2 = new List<char>(list1.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                char c = list1[i];
                if (c == '\u093F') // 'ि'
                {
                    if (list2.Count > 0)
                    {
                        // Pop preceding base consonant
                        char baseCons = list2[list2.Count - 1];
                        list2.RemoveAt(list2.Count - 1);
                        var cluster = new List<char> { baseCons };

                        // If there are half-consonants preceding it, they belong to this cluster (e.g. sth, sph, etc.)
                        while (list2.Count > 0 && HalfGlyphSet.Contains(list2[list2.Count - 1]))
                        {
                            cluster.Insert(0, list2[list2.Count - 1]);
                            list2.RemoveAt(list2.Count - 1);
                        }

                        list2.Add('\u093F');
                        list2.AddRange(cluster);
                    }
                    else
                    {
                        list2.Add('\u093F');
                    }
                }
                else
                {
                    list2.Add(c);
                }
            }

            var sb = new StringBuilder(list2.Count);
            for (int i = 0; i < list2.Count; i++)
            {
                char c = list2[i];
                if (c == '✔') { sb.Append('✓'); continue; }
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}

