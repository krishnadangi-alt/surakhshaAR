using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Login screen builder matching Reference 3 (Right image).
    /// Full-bleed daytime sky + washery header banner, white rounded card with zero dead space,
    /// pill tab switcher, enlarged localized input fields, navy CTA button with arrow,
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

            // Sky blue root backdrop matching the header clouds seamlessly
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#CAE4F5");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // 1. Daytime Mining Sky Banner Header (Full bleed to top edges)
            var headerGO = UIHelper.MakeRect("LoginHeader", root.transform);
            headerGO.anchorMin = new Vector2(0f, 1f);
            headerGO.anchorMax = new Vector2(1f, 1f);
            headerGO.pivot = new Vector2(0.5f, 1f);
            headerGO.sizeDelta = new Vector2(0f, 740f);
            headerGO.anchoredPosition = Vector2.zero;

            var headerImg = headerGO.gameObject.AddComponent<Image>();
            headerImg.color = Color.white;
            var hdrSpr = UIHelper.LoadProjectSprite("home_hero_banner.jpg");
            if (hdrSpr != null)
            {
                headerImg.sprite = hdrSpr;
                headerImg.preserveAspect = false;
            }

            // 2. Footer Backdrop Image (Mountain silhouette and mining plant at bottom)
            var footerBgGO = UIHelper.MakeRect("FooterBg", root.transform);
            footerBgGO.anchorMin = Vector2.zero;
            footerBgGO.anchorMax = new Vector2(1f, 0f);
            footerBgGO.pivot = new Vector2(0.5f, 0f);
            footerBgGO.sizeDelta = new Vector2(0f, 320f);
            footerBgGO.anchoredPosition = Vector2.zero;

            var ftrImg = footerBgGO.gameObject.AddComponent<Image>();
            ftrImg.color = Color.white;
            var ftrSpr = UIHelper.LoadProjectSprite("login_footer_clean.png") 
                      ?? UIHelper.LoadProjectSprite("login_footer_perfect.png");
            if (ftrSpr != null)
            {
                ftrImg.sprite = ftrSpr;
                ftrImg.preserveAspect = false;
            }

            // 3. White Form Card (Rounded corners, perfectly fitted to content with zero empty void)
            var card = UIHelper.MakeRect("FormCard", root.transform);
            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = new Vector2(0f, -540f);
            card.sizeDelta = new Vector2(-48f, 0f); // 24px margin from side edges

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 36);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.08f);
            shadow.effectDistance = new Vector2(0, -6f);

            var outline = card.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#E2E8F0");
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Form Layout with generous spacing and automatic height fitting
            var vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(44, 44, 44, 44);
            vlg.spacing = 28f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = card.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Tab Switcher [ Worker Login | Guest Mode ]
            BuildTabRow(card.transform);

            // Field: Employee ID
            BuildInputField(card.transform, "field-employee-id", "Employee ID", "Enter Employee ID", UIHelper.GetProfileSprite(), false);

            // Field: Password / PIN
            BuildInputField(card.transform, "field-password", "Password / PIN", "Enter Password / PIN", UIHelper.GetLockSprite(), true);

            // Options Row: [✓] Remember me ... Forgot Password?
            BuildOptionsRow(card.transform);

            // Validation / Auth Error Message (Hidden by default)
            var errLbl = UIHelper.MakeLabel("label-login-error", card.transform, "", 30f, Hex("#DC2626"), TextAlignmentOptions.Center, bold: true, wrap: true);
            var errLe = UIHelper.SetLayout(errLbl.gameObject, minHeight: 0, flexibleWidth: true, flexWidth: 1);
            errLe.flexibleHeight = 0f;
            errLbl.gameObject.SetActive(false);

            // Primary Button: Login (Dark Navy with Arrow)
            BuildPrimaryLoginButton(card.transform);

            // Divider: OR
            BuildDivider(card.transform);

            // Secondary Button: QR Code Login
            BuildQrButton(card.transform);

            // 4. Centered phone home indicator bar at very bottom
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
            var le = UIHelper.SetLayout(tabWrap.gameObject, preferredHeight: 108, minHeight: 108);
            le.flexibleHeight = 0f;

            var bgImg = tabWrap.gameObject.AddComponent<Image>();
            bgImg.color = Hex("#F1F5F9");
            bgImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(bgImg, 26);

            var hlg = tabWrap.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 8, 8);
            hlg.spacing = 8;
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight = true;

            // Worker Login (Active)
            var tabWorker = UIHelper.MakeButton("tab-worker", tabWrap, "Worker Login", 40f, Hex("#0A192F"), Color.white, 22f);
            var workerLe = UIHelper.SetLayout(tabWorker.gameObject, flexibleWidth: true, flexWidth: 1);
            workerLe.flexibleHeight = 0f;

            // Guest Mode (Inactive)
            var tabGuest = UIHelper.MakeButton("tab-guest", tabWrap, "Guest Mode", 40f, UIColors.Transparent, Hex("#475569"), 22f);
            var guestLe = UIHelper.SetLayout(tabGuest.gameObject, flexibleWidth: true, flexWidth: 1);
            guestLe.flexibleHeight = 0f;
        }

        // =================================================================
        // INPUT FIELD
        // =================================================================
        private static void BuildInputField(Transform parent, string name, string label, string placeholder, Sprite icon, bool isPassword)
        {
            var fieldCol = UIHelper.MakeVertical(name, parent, 10, childForceWidth: true, childForceHeight: false);
            var le = UIHelper.SetLayout(fieldCol.gameObject, preferredHeight: 190, minHeight: 190);
            le.flexibleHeight = 0f;

            var lbl = UIHelper.MakeLabel("Label", fieldCol, label, 40f, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 52, minHeight: 50);

            var box = UIHelper.MakeRect("InputBox", fieldCol);
            UIHelper.SetLayout(box.gameObject, preferredHeight: 116, minHeight: 116);

            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.color = Hex("#FFFFFF");
            boxImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(boxImg, 22);

            var boxOutline = box.gameObject.AddComponent<Outline>();
            boxOutline.effectColor = Hex("#CBD5E1");
            boxOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var boxHLG = box.gameObject.AddComponent<HorizontalLayoutGroup>();
            boxHLG.padding = new RectOffset(22, 22, 0, 0);
            boxHLG.spacing = 18;
            boxHLG.childAlignment = TextAnchor.MiddleLeft;
            boxHLG.childForceExpandWidth = false;
            boxHLG.childControlWidth = true;
            boxHLG.childForceExpandHeight = false;
            boxHLG.childControlHeight = true;

            // Left Icon (42x42)
            var icGO = UIHelper.MakeRect("LeftIcon", box);
            UIHelper.SetLayout(icGO.gameObject, preferredWidth: 42, minWidth: 42, preferredHeight: 42, minHeight: 42);
            var icImg = icGO.gameObject.AddComponent<Image>();
            icImg.sprite = icon;
            icImg.color = Hex("#64748B");
            icImg.preserveAspect = true;

            // Input Field container
            var inputGO = UIHelper.MakeRect("TMPInput", box);
            UIHelper.SetLayout(inputGO.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 92, minHeight: 90);

            var inputField = inputGO.gameObject.AddComponent<TMP_InputField>();

            // Text Component
            var textGO = UIHelper.MakeRect("Text", inputGO);
            UIHelper.Stretch(textGO, 0, 0, 0, 0);
            var textTMP = UIHelper.AddTMP(textGO.gameObject);
            textTMP.fontSize = 38f;
            textTMP.color = Hex("#0F172A");
            textTMP.alignment = TextAlignmentOptions.Left;
            inputField.textComponent = textTMP;

            // Placeholder Component
            var phGO = UIHelper.MakeRect("Placeholder", inputGO);
            UIHelper.Stretch(phGO, 0, 0, 0, 0);
            var phTMP = UIHelper.AddTMP(phGO.gameObject);
            phTMP.fontSize = 38f;
            phTMP.color = Hex("#94A3B8");
            phTMP.alignment = TextAlignmentOptions.Left;
            phTMP.text = placeholder;
            inputField.placeholder = phTMP;

            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;

                // Eye Icon on Right (48x48 touch target)
                var eyeGO = UIHelper.MakeRect("EyeIcon", box);
                var eyeLe = UIHelper.SetLayout(eyeGO.gameObject, preferredWidth: 48, minWidth: 48, preferredHeight: 48, minHeight: 48);
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
            var row = UIHelper.MakeHorizontal("OptionsRow", parent, 14, childForceWidth: false, childForceHeight: false);
            var rowLe = UIHelper.SetLayout(row.gameObject, preferredHeight: 60, minHeight: 60);
            rowLe.flexibleHeight = 0f;
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Checkbox (56x56 touch target for mobile)
            var chkBtn = UIHelper.MakeButton("remember-checkbox", row, "", 14, Hex("#0A192F"), Color.white, 14);
            var chkLe = UIHelper.SetLayout(chkBtn.gameObject, preferredWidth: 56, minWidth: 56, preferredHeight: 56, minHeight: 56);
            chkLe.flexibleWidth = 0f;
            chkLe.flexibleHeight = 0f;

            var chkIcon = UIHelper.MakeRect("CheckMark", chkBtn.transform);
            chkIcon.anchorMin = new Vector2(0.5f, 0.5f);
            chkIcon.anchorMax = new Vector2(0.5f, 0.5f);
            chkIcon.pivot = new Vector2(0.5f, 0.5f);
            chkIcon.sizeDelta = new Vector2(34, 34);
            var chkImg = chkIcon.gameObject.AddComponent<Image>();
            chkImg.sprite = UIHelper.GetCheckmarkSprite();
            chkImg.color = Color.white;
            chkImg.preserveAspect = true;

            var remLbl = UIHelper.MakeLabel("RememberMeLabel", row, "Remember me", 36f, Hex("#0F172A"), TextAlignmentOptions.Left, bold: false);
            var remLe = UIHelper.SetLayout(remLbl.gameObject, flexibleWidth: true, flexWidth: 1);
            remLe.flexibleHeight = 0f;

            var forgotBtn = UIHelper.MakeButton("btn-forgot-password", row, "Forgot Password?", 36f, UIColors.Transparent, Hex("#2563EB"), 0);
            var forgotLe = UIHelper.SetLayout(forgotBtn.gameObject, preferredWidth: 320f, minWidth: 280f, preferredHeight: 56f, minHeight: 56f);
            forgotLe.flexibleWidth = 0f;
            forgotLe.flexibleHeight = 0f;
        }

        // =================================================================
        // PRIMARY LOGIN BUTTON
        // =================================================================
        private static void BuildPrimaryLoginButton(Transform parent)
        {
            var btn = UIHelper.MakeButton("btn-login", parent, "", 14, Hex("#0A192F"), Color.white, 26);
            var btnLe = UIHelper.SetLayout(btn.gameObject, preferredHeight: 120, minHeight: 120);
            btnLe.flexibleHeight = 0f;

            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.18f);
            shadow.effectDistance = new Vector2(0, -5f);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 18;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            var lbl = UIHelper.MakeLabel("LoginText", btn.transform, "Login", 42f, Color.white, TextAlignmentOptions.Center, bold: true);

            var arrowGO = UIHelper.MakeRect("Arrow", btn.transform);
            var arrowLe = UIHelper.SetLayout(arrowGO.gameObject, preferredWidth: 38, minWidth: 38, preferredHeight: 38, minHeight: 38);
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
            var rowLe = UIHelper.SetLayout(row.gameObject, preferredHeight: 44, minHeight: 44);
            rowLe.flexibleHeight = 0f;
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var l1 = UIHelper.MakeRect("Line1", row);
            var l1Le = UIHelper.SetLayout(l1.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 2f, minHeight: 2f);
            l1Le.flexibleHeight = 0f;
            var i1 = l1.gameObject.AddComponent<Image>();
            i1.color = Hex("#CBD5E1");

            var orLbl = UIHelper.MakeLabel("OrText", row, "OR", 32f, Hex("#64748B"), TextAlignmentOptions.Center, bold: true);
            var orLe = UIHelper.SetLayout(orLbl.gameObject, preferredWidth: 64);
            orLe.flexibleWidth = 0f;
            orLe.flexibleHeight = 0f;

            var l2 = UIHelper.MakeRect("Line2", row);
            var l2Le = UIHelper.SetLayout(l2.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 2f, minHeight: 2f);
            l2Le.flexibleHeight = 0f;
            var i2 = l2.gameObject.AddComponent<Image>();
            i2.color = Hex("#CBD5E1");
        }

        // =================================================================
        // QR CODE BUTTON
        // =================================================================
        private static void BuildQrButton(Transform parent)
        {
            var btn = UIHelper.MakeButton("btn-login-qr", parent, "", 14, Color.white, Hex("#0F172A"), 26);
            var btnLe = UIHelper.SetLayout(btn.gameObject, preferredHeight: 120, minHeight: 120);
            btnLe.flexibleHeight = 0f;

            var border = btn.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#CBD5E1");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -3f);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 18;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            var qrGO = UIHelper.MakeRect("QRIcon", btn.transform);
            var qrLe = UIHelper.SetLayout(qrGO.gameObject, preferredWidth: 46, minWidth: 46, preferredHeight: 46, minHeight: 46);
            qrLe.flexibleWidth = 0f;
            qrLe.flexibleHeight = 0f;
            
            var qrImg = qrGO.gameObject.AddComponent<Image>();
            qrImg.sprite = UIHelper.GetQRSprite();
            qrImg.color = Hex("#0F172A");
            qrImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel("QRText", btn.transform, "Login with QR Code", 40f, Hex("#0F172A"), TextAlignmentOptions.Center, bold: true);
        }
    }
}
