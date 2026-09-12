using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Login screen — pixel-perfect recreation of Figma reference:
    /// 1080 × 2400 reference scaling:
    /// - Deep navy top section (460px) with clean emblem, 56px title, 26px subtitle.
    /// - White form card filling the lower screen with 36px rounded top corners.
    /// - 76px Tab bar (Worker Login / Guest Mode).
    /// - 130px Input fields with 88px input boxes and 56x56 ID/*** badges.
    /// - 48px Remember me / Forgot Password row.
    /// - 128px high rich safety green "Login" CTA button (#1B5E3C).
    /// - 110px high "Login with QR Code" outlined button.
    /// </summary>
    public static class LoginBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("LoginScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.PrimaryDark;
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Navy Top Section (460px high) ────────────────────────────
            var header = UIHelper.MakeRect("Header", root.transform);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1);
            header.sizeDelta = new Vector2(0, 460);
            header.anchoredPosition = Vector2.zero;

            var headerImg = header.gameObject.AddComponent<Image>();
            headerImg.color = UIColors.PrimaryDark;
            headerImg.sprite = UIHelper.GetWhiteSprite();

            // Emblem Circle / Box (110x110)
            var shieldCircle = UIHelper.MakeRect("ShieldCircle", header);
            shieldCircle.anchorMin = new Vector2(0.5f, 1f);
            shieldCircle.anchorMax = new Vector2(0.5f, 1f);
            shieldCircle.pivot = new Vector2(0.5f, 1f);
            shieldCircle.sizeDelta = new Vector2(110, 110);
            shieldCircle.anchoredPosition = new Vector2(0, -50);

            var shieldCircleImg = shieldCircle.gameObject.AddComponent<Image>();
            shieldCircleImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(shieldCircleImg, 30);
            shieldCircleImg.color = new Color(1f, 1f, 1f, 0.15f);

            var shieldIcon = UIHelper.MakeLabel("ShieldIcon", shieldCircle, "AR", 48,
                Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
            UIHelper.Stretch(shieldIcon.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // App Name label
            var appName = UIHelper.MakeLabel("AppName", header, "SurakshaAR", 56, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
            var appNameRT = appName.GetComponent<RectTransform>();
            appNameRT.anchorMin = new Vector2(0, 1);
            appNameRT.anchorMax = new Vector2(1, 1);
            appNameRT.pivot = new Vector2(0.5f, 1);
            appNameRT.sizeDelta = new Vector2(0, 64);
            appNameRT.anchoredPosition = new Vector2(0, -180);

            // Tagline label
            var appSub = UIHelper.MakeLabel("AppSubtitle", header, "AR-Based Safety Training for Industrial Workers", 26, Hex("#94A3B8"), TextAlignmentOptions.Center, bold: false, wrap: false);
            var appSubRT = appSub.GetComponent<RectTransform>();
            appSubRT.anchorMin = new Vector2(0, 1);
            appSubRT.anchorMax = new Vector2(1, 1);
            appSubRT.pivot = new Vector2(0.5f, 1);
            appSubRT.sizeDelta = new Vector2(0, 36);
            appSubRT.anchoredPosition = new Vector2(0, -250);

            // ── 2. White Form Card ──────────────────────────────────────────
            var card = UIHelper.MakeRect("FormCard", root.transform);
            card.anchorMin = new Vector2(0f, 0f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 0f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = new Vector2(0, -380); // sits under the header

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 40);

            // Scrollable Form Container
            var scrollRoot = UIHelper.MakeRect("FormScroll", card);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = Vector2.zero;

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;

            var viewport = UIHelper.MakeRect("Viewport", scrollRoot);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport;

            var form = UIHelper.MakeRect("FormContent", viewport);
            form.anchorMin = new Vector2(0, 1);
            form.anchorMax = new Vector2(1, 1);
            form.pivot = new Vector2(0.5f, 1);
            form.sizeDelta = new Vector2(0, 1600);
            scrollRect.content = form;

            var formVLG = form.gameObject.AddComponent<VerticalLayoutGroup>();
            formVLG.padding = new RectOffset(48, 48, 44, 60);
            formVLG.spacing = 26;
            formVLG.childForceExpandWidth = true;
            formVLG.childForceExpandHeight = false;
            formVLG.childControlHeight = true;
            formVLG.childControlWidth = true;

            var csf = form.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Section heading
            MakeFormHeader(form, "LoginTitle", "Worker Login", 46, UIColors.TextPrimary, bold: true, height: 56);
            MakeFormHeader(form, "LoginSubtitle", "Safe Mines Stronger Communities", 26, Hex("#64748B"), bold: false, height: 36);

            // Tab row: Worker Login | Guest Mode
            BuildTabRow(form);

            // Employee ID field
            BuildInputField(form, "field-employee-id", "Employee ID", "e.g. JH-MN-004821", "ID", false);

            // Password / PIN field
            BuildInputField(form, "field-password", "Password / PIN", "Enter PIN or Password", "***", true);

            // Remember + Forgot row
            BuildOptionsRow(form);

            // Login button (Rich Safety Green, 128px high)
            var btnContainer = UIHelper.MakeRect("LoginBtnWrap", form);
            UIHelper.SetLayout(btnContainer.gameObject, preferredHeight: 128);
            var btnContainerHLG = btnContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            btnContainerHLG.childForceExpandWidth = true;
            btnContainerHLG.childForceExpandHeight = true;
            btnContainerHLG.childControlWidth = true;
            btnContainerHLG.childControlHeight = true;

            var loginBtn = UIHelper.MakeButton("btn-login", btnContainer,
                "Login", 38, UIColors.SafetyGreen, Color.white, 26f);
            UIHelper.SetLayout(loginBtn.gameObject, preferredHeight: 128);

            // OR divider
            BuildDivider(form);

            // QR Login button (110px high)
            var qrContainer = UIHelper.MakeRect("QrBtnWrap", form);
            UIHelper.SetLayout(qrContainer.gameObject, preferredHeight: 110);
            var qrContainerHLG = qrContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            qrContainerHLG.childForceExpandWidth = true;
            qrContainerHLG.childForceExpandHeight = true;
            qrContainerHLG.childControlWidth = true;
            qrContainerHLG.childControlHeight = true;

            var qrBtn = UIHelper.MakeButton("btn-login-qr", qrContainer,
                "Login with QR Code", 30, Color.white, UIColors.TextPrimary, 24f);
            UIHelper.SetLayout(qrBtn.gameObject, preferredHeight: 110);

            var qrBtnImg = qrBtn.GetComponent<Image>();
            qrBtnImg.color = Color.white;
            var qrBtnOutline = qrBtn.gameObject.AddComponent<Outline>();
            qrBtnOutline.effectColor = UIColors.Border;
            qrBtnOutline.effectDistance = new Vector2(2, -2);

            return root;
        }

        private static void MakeFormHeader(Transform parent, string name,
            string text, float size, Color color, bool bold, float height)
        {
            var rt = UIHelper.MakeRect(name, parent);
            UIHelper.SetLayout(rt.gameObject, preferredHeight: height);

            var tmp = UIHelper.MakeLabel(name + "_txt", rt, text, size, color, TextAlignmentOptions.Left, bold: bold, wrap: false);
            UIHelper.Stretch(tmp.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        private static void BuildTabRow(Transform parent)
        {
            var tabRow = UIHelper.MakeRect("TabRow", parent);
            UIHelper.SetLayout(tabRow.gameObject, preferredHeight: 76);

            var tabImg = tabRow.gameObject.AddComponent<Image>();
            tabImg.color = Hex("#F1F5F9");
            tabImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(tabImg, 18);

            var hlg = tabRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(6, 6, 6, 6);
            hlg.spacing = 8;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var workerBtn = UIHelper.MakeButton("tab-worker", tabRow,
                "Worker Login", 28, UIColors.Primary, Color.white, 14f);
            UIHelper.SetLayout(workerBtn.gameObject, flexibleWidth: true, flexWidth: 1);

            var guestBtn = UIHelper.MakeButton("tab-guest", tabRow,
                "Guest Mode", 28, Color.clear, Hex("#64748B"), 14f);
            guestBtn.GetComponent<Image>().color = Color.clear;
            UIHelper.SetLayout(guestBtn.gameObject, flexibleWidth: true, flexWidth: 1);
        }

        private static void BuildInputField(Transform parent,
            string fieldName, string label, string placeholder, string badgeText, bool password)
        {
            var container = UIHelper.MakeRect($"FieldGroup_{fieldName}", parent);
            UIHelper.SetLayout(container.gameObject, preferredHeight: 130);

            var cvlg = container.gameObject.AddComponent<VerticalLayoutGroup>();
            cvlg.spacing = 10;
            cvlg.childForceExpandWidth = true;
            cvlg.childForceExpandHeight = false;
            cvlg.childControlHeight = true;
            cvlg.childControlWidth = true;

            // Label
            var labelRT = UIHelper.MakeRect("FieldLabel", container);
            UIHelper.SetLayout(labelRT.gameObject, preferredHeight: 32);
            var labelTMP = UIHelper.MakeLabel("FieldLabel_txt", labelRT, label, 26, Hex("#334155"), bold: true, wrap: false);
            UIHelper.Stretch(labelTMP.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Input box container (88px high)
            var inputBox = UIHelper.MakeRect(fieldName, container);
            UIHelper.SetLayout(inputBox.gameObject, preferredHeight: 88);

            var boxImg = inputBox.gameObject.AddComponent<Image>();
            boxImg.color = Hex("#F8FAFC");
            boxImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(boxImg, 18);

            var boxOutline = inputBox.gameObject.AddComponent<Outline>();
            boxOutline.effectColor = Hex("#E2E8F0");
            boxOutline.effectDistance = new Vector2(2, -2);

            var hlg = inputBox.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(18, 18, 8, 8);
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Left icon / badge (56x56)
            var iconRT = UIHelper.MakeRect("Badge", inputBox);
            UIHelper.SetLayout(iconRT.gameObject, preferredWidth: 56, preferredHeight: 56, minWidth: 56, minHeight: 56);

            var iconBg = iconRT.gameObject.AddComponent<Image>();
            iconBg.color = Hex("#EEF2F6");
            UIHelper.SetImageRoundedSprite(iconBg, 12);

            var iconTMP = UIHelper.MakeLabel("BadgeText", iconRT, badgeText, 22, UIColors.Primary, TextAlignmentOptions.Center, bold: true, wrap: false);
            UIHelper.Stretch(iconTMP.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // TMP InputField (flexible width)
            var inputFieldRT = UIHelper.MakeRect("InputField", inputBox);
            var ifLE = inputFieldRT.gameObject.AddComponent<LayoutElement>();
            ifLE.flexibleWidth = 1;

            var inputBgImg = inputFieldRT.gameObject.AddComponent<Image>();
            inputBgImg.color = Color.clear;

            var inputField = inputFieldRT.gameObject.AddComponent<TMP_InputField>();

            var textAreaRT = UIHelper.MakeStretchRect("Text Area", inputFieldRT);
            textAreaRT.gameObject.AddComponent<RectMask2D>();

            var textRT = UIHelper.MakeStretchRect("Text", textAreaRT);
            var textTMP = UIHelper.AddTMP(textRT.gameObject);
            textTMP.fontSize = 30;
            textTMP.color = UIColors.TextPrimary;
            textTMP.alignment = TextAlignmentOptions.MidlineLeft;
            textTMP.raycastTarget = false;

            var phRT = UIHelper.MakeStretchRect("Placeholder", textAreaRT);
            var phTMP = UIHelper.AddTMP(phRT.gameObject);
            phTMP.text = placeholder;
            phTMP.fontSize = 30;
            phTMP.color = Hex("#94A3B8");
            phTMP.fontStyle = FontStyles.Italic;
            phTMP.alignment = TextAlignmentOptions.MidlineLeft;
            phTMP.raycastTarget = false;

            inputField.textViewport = textAreaRT;
            inputField.textComponent = textTMP;
            inputField.placeholder = phTMP;
            inputField.contentType = password
                ? TMP_InputField.ContentType.Password
                : TMP_InputField.ContentType.Standard;
            inputField.image = inputBgImg;

            // Rename so controller finds it by field name
            inputFieldRT.gameObject.name = fieldName;
        }

        private static void BuildOptionsRow(Transform parent)
        {
            var row = UIHelper.MakeRect("OptionsRow", parent);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 48);

            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.spacing = 14;

            // Remember Group (Left Container with Checkbox + Label)
            var remGroup = UIHelper.MakeRect("RememberGroup", row);
            UIHelper.SetLayout(remGroup.gameObject, preferredWidth: 280, preferredHeight: 48);
            var remHLG = remGroup.gameObject.AddComponent<HorizontalLayoutGroup>();
            remHLG.spacing = 14;
            remHLG.childAlignment = TextAnchor.MiddleLeft;
            remHLG.childControlWidth = false;
            remHLG.childControlHeight = false;

            // Checkbox (36x36)
            var cbRT = UIHelper.MakeRect("remember-checkbox", remGroup);
            cbRT.sizeDelta = new Vector2(36, 36);
            var cbImg = cbRT.gameObject.AddComponent<Image>();
            cbImg.color = UIColors.Primary;
            cbImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cbImg, 8);
            cbRT.gameObject.AddComponent<Button>();

            // Inner checkmark image (active by default)
            var cmkRT = UIHelper.MakeRect("CheckMark", cbRT);
            cmkRT.sizeDelta = new Vector2(20, 20);
            var cmkImg = cmkRT.gameObject.AddComponent<Image>();
            cmkImg.color = Color.white;
            cmkImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cmkImg, 4);

            // Remember label
            var remRT = UIHelper.MakeRect("RememberLabel", remGroup);
            remRT.sizeDelta = new Vector2(220, 40);
            var remTMP = UIHelper.MakeLabel("Remember_txt", remRT, "Remember me", 26, Hex("#475569"), wrap: false);
            UIHelper.Stretch(remTMP.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Spacer pushes forgot password to right
            var spacer = UIHelper.MakeRect("Spacer", row);
            var spacerLE = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLE.flexibleWidth = 1;

            // Forgot Password (Right aligned)
            var forgotRT = UIHelper.MakeRect("ForgotPassword", row);
            UIHelper.SetLayout(forgotRT.gameObject, preferredWidth: 260, preferredHeight: 48);
            var forgotTMP = UIHelper.MakeLabel("Forgot_txt", forgotRT, "Forgot Password?", 26, Hex("#1D4ED8"), TextAlignmentOptions.Right, bold: true, wrap: false);
            UIHelper.Stretch(forgotTMP.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        private static void BuildDivider(Transform parent)
        {
            var divRow = UIHelper.MakeRect("DividerRow", parent);
            UIHelper.SetLayout(divRow.gameObject, preferredHeight: 44);

            var hlg = divRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.spacing = 20;

            var line1 = UIHelper.MakeRect("Line1", divRow);
            var l1LE = line1.gameObject.AddComponent<LayoutElement>();
            l1LE.flexibleWidth = 1;
            line1.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");

            var orRT = UIHelper.MakeRect("OR", divRow);
            UIHelper.SetLayout(orRT.gameObject, preferredWidth: 60);
            var orTMP = UIHelper.MakeLabel("OR_txt", orRT, "OR", 22, Hex("#94A3B8"), TextAlignmentOptions.Center, bold: false, wrap: false);
            UIHelper.Stretch(orTMP.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var line2 = UIHelper.MakeRect("Line2", divRow);
            var l2LE = line2.gameObject.AddComponent<LayoutElement>();
            l2LE.flexibleWidth = 1;
            line2.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");
        }

        private static Color Hex(string hex) => UIColors.Hex(hex);
    }
}
