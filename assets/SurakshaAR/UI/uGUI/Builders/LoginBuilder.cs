using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Login screen builder matching Reference 3 (Right image).
    /// Daytime sky + washery header banner, white rounded card,
    /// pill tab switcher, localized input fields, navy CTA button with arrow,
    /// QR code login, and industrial mine footer.
    /// </summary>
    public static class LoginBuilder
    {
        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("LoginScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // 1. Daytime Mining Sky Banner Header (Matching Reference 3 Right)
            var headerGO = UIHelper.MakeRect("LoginHeader", root.transform);
            headerGO.anchorMin = new Vector2(0f, 1f);
            headerGO.anchorMax = new Vector2(1f, 1f);
            headerGO.pivot = new Vector2(0.5f, 1f);
            headerGO.sizeDelta = new Vector2(0f, 600f);
            headerGO.anchoredPosition = Vector2.zero;

            var headerImg = headerGO.gameObject.AddComponent<Image>();
            headerImg.color = Color.white;
            var hdrSpr = UIHelper.LoadProjectSprite("login_header_perfect.png")
                      ?? UIHelper.LoadProjectSprite("header_ref1_clean_bg.png");
            if (hdrSpr != null)
            {
                headerImg.sprite = hdrSpr;
                headerImg.preserveAspect = false;
            }

            // 2. Footer Backdrop Image (Mountain silhouette at bottom)
            var footerBgGO = UIHelper.MakeRect("FooterBg", root.transform);
            footerBgGO.anchorMin = Vector2.zero;
            footerBgGO.anchorMax = new Vector2(1f, 0f);
            footerBgGO.pivot = new Vector2(0.5f, 0f);
            footerBgGO.sizeDelta = new Vector2(0f, 280f);
            footerBgGO.anchoredPosition = Vector2.zero;

            var ftrImg = footerBgGO.gameObject.AddComponent<Image>();
            ftrImg.color = Color.white;
            var ftrSpr = UIHelper.LoadProjectSprite("login_footer_clean.png") ?? UIHelper.LoadProjectSprite("login_footer_perfect.png");
            if (ftrSpr != null)
            {
                ftrImg.sprite = ftrSpr;
                ftrImg.preserveAspect = false;
            }

            // 3. White Form Card (Rounded top corners, sitting gracefully below header)
            var card = UIHelper.MakeRect("FormCard", root.transform);
            card.anchorMin = new Vector2(0f, 0f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.offsetMin = new Vector2(0f, 280f); // Space for footer
            card.offsetMax = new Vector2(0f, -540f); // Sits under header

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 36);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -6f);

            var outline = card.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#F1F5F9");
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Form Layout
            var vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(48, 48, 40, 40);
            vlg.spacing = 20f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Tab Switcher [ Worker Login | Guest Mode ]
            BuildTabRow(card.transform);

            // Field: Employee ID
            BuildInputField(card.transform, "field-employee-id", "Employee ID", "अपनी कर्मचारी आईडी दर्ज करें", UIHelper.GetProfileSprite(), false);

            // Field: Password / PIN
            BuildInputField(card.transform, "field-password", "Password / PIN", "अपना पासवर्ड दर्ज करें", UIHelper.GetLockSprite(), true);

            // Options Row: [✓] Remember me ... Forgot Password?
            BuildOptionsRow(card.transform);

            // Primary Button: Login (Dark Navy with Arrow)
            BuildPrimaryLoginButton(card.transform);

            // Divider: OR
            BuildDivider(card.transform);

            // Secondary Button: QR Code Login
            BuildQrButton(card.transform);

            // 4. Centered Footer Text & Home Indicator
            // (Helmet icon and "Safe Mines | Stronger India" are already in login_footer_perfect.png)

            // Centered phone home indicator bar at very bottom
            var barGO = UIHelper.MakeRect("PhoneHomeIndicator", root.transform);
            barGO.anchorMin = new Vector2(0.5f, 0f);
            barGO.anchorMax = new Vector2(0.5f, 0f);
            barGO.pivot = new Vector2(0.5f, 0f);
            barGO.sizeDelta = new Vector2(280f, 8f);
            barGO.anchoredPosition = new Vector2(0f, 16f);

            var barImg = barGO.gameObject.AddComponent<Image>();
            barImg.color = Hex("#94A3B8");
            barImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barImg, 4);

            return root;
        }

        // =================================================================
        // TABS: Worker Login / Guest Mode
        // =================================================================
        private static void BuildTabRow(Transform parent)
        {
            var tabWrap = UIHelper.MakeRect("TabWrap", parent);
            var le = UIHelper.SetLayout(tabWrap.gameObject, preferredHeight: 76, minHeight: 76);
            le.flexibleHeight = 0f; // Fix: prevent tabWrap from taking all remaining vertical space

            var bgImg = tabWrap.gameObject.AddComponent<Image>();
            bgImg.color = Hex("#F1F5F9");
            bgImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(bgImg, 22);

            var hlg = tabWrap.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(6, 6, 6, 6);
            hlg.spacing = 6;
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight = true;

            // Worker Login (Active)
            var tabWorker = UIHelper.MakeButton("tab-worker", tabWrap, "Worker Login", 26f, Hex("#0A192F"), Color.white, 18f);
            var workerLe = UIHelper.SetLayout(tabWorker.gameObject, flexibleWidth: true, flexWidth: 1);
            workerLe.flexibleHeight = 0f;

            // Guest Mode (Inactive)
            var tabGuest = UIHelper.MakeButton("tab-guest", tabWrap, "Guest Mode", 26f, UIColors.Transparent, Hex("#475569"), 18f);
            var guestLe = UIHelper.SetLayout(tabGuest.gameObject, flexibleWidth: true, flexWidth: 1);
            guestLe.flexibleHeight = 0f;
        }

        // =================================================================
        // INPUT FIELD
        // =================================================================
        private static void BuildInputField(Transform parent, string name, string label, string placeholder, Sprite icon, bool isPassword)
        {
            var fieldCol = UIHelper.MakeVertical(name, parent, 8, childForceWidth: true, childForceHeight: false);
            var le = UIHelper.SetLayout(fieldCol.gameObject, preferredHeight: 120, minHeight: 120);
            le.flexibleHeight = 0f;

            var lbl = UIHelper.MakeLabel("Label", fieldCol, label, 24f, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 30);

            var box = UIHelper.MakeRect("InputBox", fieldCol);
            UIHelper.SetLayout(box.gameObject, preferredHeight: 78, minHeight: 78);

            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.color = Hex("#FFFFFF");
            boxImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(boxImg, 18);

            var boxOutline = box.gameObject.AddComponent<Outline>();
            boxOutline.effectColor = Hex("#E2E8F0");
            boxOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var boxHLG = box.gameObject.AddComponent<HorizontalLayoutGroup>();
            boxHLG.padding = new RectOffset(20, 20, 0, 0);
            boxHLG.spacing = 16;
            boxHLG.childAlignment = TextAnchor.MiddleLeft;
            boxHLG.childForceExpandWidth = false;
            boxHLG.childControlWidth = true;
            boxHLG.childForceExpandHeight = false;
            boxHLG.childControlHeight = true;

            // Left Icon
            var icGO = UIHelper.MakeRect("LeftIcon", box);
            UIHelper.SetLayout(icGO.gameObject, preferredWidth: 28, minWidth: 28, preferredHeight: 28, minHeight: 28);
            var icImg = icGO.gameObject.AddComponent<Image>();
            icImg.sprite = icon;
            icImg.color = Hex("#64748B");
            icImg.preserveAspect = true;

            // Input Field container
            var inputGO = UIHelper.MakeRect("TMPInput", box);
            UIHelper.SetLayout(inputGO.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 60, minHeight: 60);

            var inputField = inputGO.gameObject.AddComponent<TMP_InputField>();

            // Text Component
            var textGO = UIHelper.MakeRect("Text", inputGO);
            UIHelper.Stretch(textGO, 0, 0, 0, 0);
            var textTMP = UIHelper.AddTMP(textGO.gameObject);
            textTMP.fontSize = 24f;
            textTMP.color = Hex("#0F172A");
            textTMP.alignment = TextAlignmentOptions.Left;
            inputField.textComponent = textTMP;

            // Placeholder Component
            var phGO = UIHelper.MakeRect("Placeholder", inputGO);
            UIHelper.Stretch(phGO, 0, 0, 0, 0);
            var phTMP = UIHelper.AddTMP(phGO.gameObject);
            phTMP.fontSize = 24f;
            phTMP.color = Hex("#94A3B8");
            phTMP.alignment = TextAlignmentOptions.Left;
            phTMP.text = placeholder;
            inputField.placeholder = phTMP;

            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;

                // Eye Icon on Right
                var eyeGO = UIHelper.MakeRect("EyeIcon", box);
                var eyeLe = UIHelper.SetLayout(eyeGO.gameObject, preferredWidth: 28, minWidth: 28, preferredHeight: 28, minHeight: 28);
                eyeLe.flexibleWidth = 0f;
                var eyeImg = eyeGO.gameObject.AddComponent<Image>();
                eyeImg.sprite = UIHelper.GetCircleOutlineSprite();
                eyeImg.color = Hex("#64748B");
                eyeImg.preserveAspect = true;
            }
        }

        // =================================================================
        // OPTIONS ROW
        // =================================================================
        private static void BuildOptionsRow(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("OptionsRow", parent, 12, childForceWidth: false, childForceHeight: false);
            var rowLe = UIHelper.SetLayout(row.gameObject, preferredHeight: 44, minHeight: 44);
            rowLe.flexibleHeight = 0f;
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Checkbox
            var chkBtn = UIHelper.MakeButton("remember-checkbox", row, "", 14, Hex("#0A192F"), Color.white, 8);
            var chkLe = UIHelper.SetLayout(chkBtn.gameObject, preferredWidth: 32, minWidth: 32, preferredHeight: 32, minHeight: 32);
            chkLe.flexibleWidth = 0f; // Fix horizontal stretching
            chkLe.flexibleHeight = 0f;

            var chkIcon = UIHelper.MakeRect("CheckIcon", chkBtn.transform);
            chkIcon.anchorMin = new Vector2(0.5f, 0.5f);
            chkIcon.anchorMax = new Vector2(0.5f, 0.5f);
            chkIcon.pivot = new Vector2(0.5f, 0.5f);
            chkIcon.sizeDelta = new Vector2(20, 20);
            var chkImg = chkIcon.gameObject.AddComponent<Image>();
            chkImg.sprite = UIHelper.GetCheckmarkSprite();
            chkImg.color = Color.white;
            chkImg.preserveAspect = true;

            var remLbl = UIHelper.MakeLabel("RememberMeLabel", row, "Remember me", 23f, Hex("#0F172A"), TextAlignmentOptions.Left, bold: false);
            var remLe = UIHelper.SetLayout(remLbl.gameObject, flexibleWidth: true, flexWidth: 1);
            remLe.flexibleHeight = 0f;

            var forgotBtn = UIHelper.MakeButton("btn-forgot-password", row, "Forgot Password?", 23f, UIColors.Transparent, Hex("#2563EB"), 0);
            var forgotLe = UIHelper.SetLayout(forgotBtn.gameObject, preferredWidth: 220f, minWidth: 220f, preferredHeight: 40f, minHeight: 40f);
            forgotLe.flexibleWidth = 0f;
            forgotLe.flexibleHeight = 0f;
        }

        // =================================================================
        // PRIMARY LOGIN BUTTON
        // =================================================================
        private static void BuildPrimaryLoginButton(Transform parent)
        {
            var btn = UIHelper.MakeButton("btn-login", parent, "", 14, Hex("#0A192F"), Color.white, 22);
            var btnLe = UIHelper.SetLayout(btn.gameObject, preferredHeight: 90, minHeight: 90);
            btnLe.flexibleHeight = 0f;

            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.15f);
            shadow.effectDistance = new Vector2(0, -4f);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 16;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            var lbl = UIHelper.MakeLabel("LoginText", btn.transform, "लॉगिन करें", 30f, Color.white, TextAlignmentOptions.Center, bold: true);

            var arrowGO = UIHelper.MakeRect("Arrow", btn.transform);
            var arrowLe = UIHelper.SetLayout(arrowGO.gameObject, preferredWidth: 26, minWidth: 26, preferredHeight: 26, minHeight: 26);
            arrowLe.flexibleWidth = 0f;
            arrowLe.flexibleHeight = 0f;
            
            var arrowImg = arrowGO.gameObject.AddComponent<Image>();
            arrowImg.sprite = UIHelper.LoadProjectSprite("icon_arrow_right.png") ?? UIHelper.GetRightChevronSprite();
            arrowImg.color = Color.white;
            arrowImg.preserveAspect = true;
        }

        // =================================================================
        // DIVIDER
        // =================================================================
        private static void BuildDivider(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("OrDivider", parent, 16, childForceWidth: false, childForceHeight: false);
            var rowLe = UIHelper.SetLayout(row.gameObject, preferredHeight: 32, minHeight: 32);
            rowLe.flexibleHeight = 0f;
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var l1 = UIHelper.MakeRect("Line1", row);
            var l1Le = UIHelper.SetLayout(l1.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 1.5f, minHeight: 1.5f);
            l1Le.flexibleHeight = 0f;
            var i1 = l1.gameObject.AddComponent<Image>();
            i1.color = Hex("#E2E8F0");

            var orLbl = UIHelper.MakeLabel("OrText", row, "OR", 20f, Hex("#94A3B8"), TextAlignmentOptions.Center, bold: true);
            var orLe = UIHelper.SetLayout(orLbl.gameObject, preferredWidth: 40);
            orLe.flexibleWidth = 0f;
            orLe.flexibleHeight = 0f;

            var l2 = UIHelper.MakeRect("Line2", row);
            var l2Le = UIHelper.SetLayout(l2.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 1.5f, minHeight: 1.5f);
            l2Le.flexibleHeight = 0f;
            var i2 = l2.gameObject.AddComponent<Image>();
            i2.color = Hex("#E2E8F0");
        }

        // =================================================================
        // QR CODE BUTTON
        // =================================================================
        private static void BuildQrButton(Transform parent)
        {
            var btn = UIHelper.MakeButton("btn-login-qr", parent, "", 14, Color.white, Hex("#0F172A"), 22);
            var btnLe = UIHelper.SetLayout(btn.gameObject, preferredHeight: 88, minHeight: 88);
            btnLe.flexibleHeight = 0f;

            var border = btn.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.02f);
            shadow.effectDistance = new Vector2(0, -3f);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 16;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            var qrGO = UIHelper.MakeRect("QRIcon", btn.transform);
            var qrLe = UIHelper.SetLayout(qrGO.gameObject, preferredWidth: 32, minWidth: 32, preferredHeight: 32, minHeight: 32);
            qrLe.flexibleWidth = 0f;
            qrLe.flexibleHeight = 0f;
            
            var qrImg = qrGO.gameObject.AddComponent<Image>();
            qrImg.sprite = UIHelper.GetQRSprite();
            qrImg.color = Hex("#0F172A");
            qrImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel("QRText", btn.transform, "क्यूआर कोड से लॉगिन करें", 26f, Hex("#0F172A"), TextAlignmentOptions.Center, bold: true);
        }
    }
}
